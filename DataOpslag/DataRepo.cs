using System.Globalization;
using System.Net.Sockets;
using WinkelDomein;
using WinkelDomein.Enums;
using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace DataOpslag {
    public class DataRepo : IDataOpslag {
        private const string PRODUCTSFILEPATH = @"Producten.txt";
        private const string PARKEDTICKETSFILEPATH = @"ParkedTickets.txt";
        private const string REDUCTIONSFILEPATH = @"Kortingen.txt";

        public List<Korting> ParseReductions() {
            List<Korting> reductions = [];
            if (!File.Exists(REDUCTIONSFILEPATH)) File.Create(REDUCTIONSFILEPATH).Close();
            string[] rawReductions = File.ReadAllLines(REDUCTIONSFILEPATH);
            if (rawReductions.Length == 0) {
                Logger.GeneralLog("Geen kortingscodes beschikbaar");
                return reductions;
            }
            foreach (var rawReduction in rawReductions) {
                string[] rawReductionSplit = rawReduction.Split(';');
                if (!Enum.TryParse(rawReductionSplit[0], ignoreCase: true, out ProductCategorie category)) {
                    Logger.LogError("Foute productcategorie in kortingen");
                    continue;
                }
                if (!int.TryParse(rawReductionSplit[1], out int reductionPercentage)) {
                    Logger.LogError("Fout kortingspercentage in kortingen");
                    continue;
                }
                if (!DateOnly.TryParseExact(rawReductionSplit[2], "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly startDate)) {
                    Logger.LogError("Foute startdatum in kortingen");
                    continue;
                }
                if (!DateOnly.TryParseExact(rawReductionSplit[3], "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly endDate)) {
                    Logger.LogError("Foute einddatum in kortingen");
                    continue;
                }
                Korting newReduction;
                try { newReduction = new(category, reductionPercentage, startDate, endDate); }
                catch { Logger.LogError("Fout bij aanmaken korting"); continue; }
                reductions.Add(newReduction);
            }
            Logger.GeneralLog($"{reductions.Count} kortingscode(s) ingeladen");
            return reductions;
        }
        public List<Product> ParsePossibleProducts(List<Korting> reductions) {
            List<Product> possibleProducts = [];
            if (!File.Exists(PRODUCTSFILEPATH)) File.Create(PRODUCTSFILEPATH).Close();
            string[] rawProducts = File.ReadAllLines(PRODUCTSFILEPATH);
            if (rawProducts.Length == 0) {
                Logger.GeneralLog("Geen producten beschikbaar");
                return possibleProducts;
            }
            foreach (var line in rawProducts) {
                string[] productInfo = line.Split(';');
                string code = productInfo[0];
                string name = productInfo[1];
                if (!decimal.TryParse(productInfo[2], out decimal price)) {
                    Logger.LogError("Foute prijs in producten");
                    continue;
                }
                if (!int.TryParse(productInfo[3], out int btw)) {
                    Logger.LogError("Foute btw in producten");
                    continue;
                }
                if (!Enum.TryParse(productInfo[4], ignoreCase: true, out ProductCategorie category)) {
                    Logger.LogError("Foute productcategorie in producten");
                    continue;
                }
                Korting? possibleReduction = reductions.Find((x) => x.Category == category);

                Product newProduct;
                try { newProduct = new(code, name, price, btw, category, possibleReduction); }
                catch { Logger.LogError("Fout bij aanmaken product"); continue; }
                possibleProducts.Add(newProduct);
            }
            Logger.GeneralLog(possibleProducts.Count + " producten ingeladen");
            return possibleProducts;
        }
        public List<KassaTicket> ParseParkedTickets(List<Product> possibleProducts) {
            List<KassaTicket> tickets = [];
            if (!File.Exists(PARKEDTICKETSFILEPATH)) File.Create(PARKEDTICKETSFILEPATH).Close();
            string[] rawTickets = File.ReadAllLines(PARKEDTICKETSFILEPATH);

            for (int i = 0; i < rawTickets.Length; i++) {
                DateTime now = DateTime.Now;
                int ticketLifetimeInMinutes = 30;
                string[] rawTicket = rawTickets[i].Split(';');
                if (!long.TryParse(rawTicket[0], out long dateAsNumber)) {
                    Logger.LogError("Foute creatietijd in geparkeerde tickets");
                    continue;
                }
                DateTime creationTime;
                try { creationTime = new(dateAsNumber); }
                catch { Logger.LogError("Foute creatietijd in geparkeerde tickets"); continue; }
                // Aanpassing ----
                if (creationTime.AddMinutes(ticketLifetimeInMinutes) < now) {
                    RemoveStoredTicketByCeationTime(creationTime);
                    Logger.GeneralLog($"Ticket vervallen {dateAsNumber}");
                    continue;
                }
                List<GescandProduct> scannedProducts = [];
                for (int j = 1; j < rawTicket.Length; j += 2) {
                    string productCode = rawTicket[j];
                    if (!int.TryParse(rawTicket[j + 1], out int amount)) {
                        Logger.LogError("Fout aantal in gescande producten in geparkeerde tickets");
                        continue;
                    }
                    Product? product = possibleProducts.Find(x => (x.Code == productCode));
                    GescandProduct scannedProduct;
                    try { scannedProduct = new(product!, amount); }
                    catch { Logger.LogError("Foute gescande product in geparkeerde tickets"); continue; }
                    scannedProducts.Add(scannedProduct);
                }

                KassaTicket ticket;
                try { ticket = new(creationTime, scannedProducts); }
                catch { Logger.LogError("Fout bij aanmaken ticket"); continue; }
                tickets.Add(ticket);
            }
            Logger.GeneralLog(tickets.Count + " geparkeerde tickets ingeladen");
            return tickets;
        }

        public void StoreTicket(KassaTicket ticket) {
            string ticketToRecord = ticket.ToRecordString();
            File.AppendAllText(PARKEDTICKETSFILEPATH, $"{ticketToRecord}\n");
        }
        public void RemoveStoredTicket(KassaTicket ticket) {
            string ticketTimeStamp = ticket.CreationDateTime.Ticks.ToString();
            List<string> parkedTicketsString = [.. File.ReadAllLines(PARKEDTICKETSFILEPATH)];
            int amountRemoved = parkedTicketsString.RemoveAll(x => x.StartsWith(ticketTimeStamp));
            if (amountRemoved > 1) throw new Exception(message: "Meer dan 1 element is verwijderd uit de parked tickets opslag");
            File.WriteAllLinesAsync(PARKEDTICKETSFILEPATH, parkedTicketsString);
        }
        public void RemoveStoredTicketByCeationTime(DateTime creationTime) {
            string ticketTimeStamp = creationTime.Ticks.ToString();
            List<string> parkedTicketsString = [.. File.ReadAllLines(PARKEDTICKETSFILEPATH)];
            int amountRemoved = parkedTicketsString.RemoveAll(x => x.StartsWith(ticketTimeStamp));
            if (amountRemoved > 1) throw new Exception(message: "Meer dan 1 element is verwijderd uit de parked tickets opslag");
            File.WriteAllLinesAsync(PARKEDTICKETSFILEPATH, parkedTicketsString);
        }
    }
}
