using System.Globalization;
using System.Xml.Linq;
using WinkelDomein.Enums;
using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace WinkelDomein {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;
        private readonly List<Korting> _reductions = [];
        private readonly List<Product> _possibleProducts = [];
        private readonly List<KassaTicket> _tickets = [];
        private KassaTicket _currentTicket = default!;
        private const string PRODUCTSFILEPATH = @"Producten.txt";
        private const string PARKEDTICKETSFILEPATH = @"ParkedTickets.txt";
        private const string REDUCTIONSFILEPATH = @"Kortingen.txt";

        public KassaTicket CurrentTicket {
            get {
                if (!HasTickets) GenerateNewKassaTicket();
                if (!_tickets.Contains(_currentTicket)) CurrentTicket = GetLastTicket();
                return _currentTicket;
            }
            set => _currentTicket = value;
        }
        public bool HasTickets => _tickets.Count != 0;
        public int TicketCount => _tickets.Count;
        public int PossibleProductsCount => _possibleProducts.Count;

        public Kassa(IBetaalTerminal betaalTerminal) {
            _betaalTerminal = betaalTerminal;
            Start();
        }

        private void Start() {
            ParseReductions();
            ParsePossibleProducts();
            ParseParkedTickets();

            GenerateNewKassaTicket();
        }

        private void ParseReductions() {
            if (!File.Exists(REDUCTIONSFILEPATH)) File.Create(REDUCTIONSFILEPATH);
            string[] rawReductions = File.ReadAllLines(REDUCTIONSFILEPATH);
            if (rawReductions.Length == 0) {
                Logger.GeneralLog("Geen kortingscodes beschikbaar");
                return;
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
                _reductions.Add(newReduction);
            }
            Logger.GeneralLog($"{_reductions.Count} kortingscode(s) ingeladen");
        }

        private void ParsePossibleProducts() {
            if (!File.Exists(PRODUCTSFILEPATH)) File.Create(PRODUCTSFILEPATH);
            string[] rawProducts = File.ReadAllLines(PRODUCTSFILEPATH);
            if (rawProducts.Length == 0) {
                Logger.GeneralLog("Geen producten beschikbaar");
                return;
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
                Korting? possibleReduction = _reductions.Find((x) => x.Category == category);

                Product newProduct;
                try { newProduct = new(code, name, price, btw, category, possibleReduction); }
                catch { Logger.LogError("Fout bij aanmaken product"); continue; }
                _possibleProducts.Add(newProduct);
            }
            Logger.GeneralLog(PossibleProductsCount + " producten ingeladen");
        }

        private void ParseParkedTickets() {
            if (!File.Exists(PARKEDTICKETSFILEPATH)) File.Create(PARKEDTICKETSFILEPATH);
            string[] rawTickets = File.ReadAllLines(PARKEDTICKETSFILEPATH);

            for (int i = 0; i < rawTickets.Length; i++) {
                string[] rawTicket = rawTickets[i].Split(';');
                if (!long.TryParse(rawTicket[0], out long dateAsNumber)) {
                    Logger.LogError("Foute creatietijd in geparkeerde tickets");
                    continue;
                }
                DateTime creationTime;
                try { creationTime = new(dateAsNumber); }
                catch { Logger.LogError("Foute creatietijd in geparkeerde tickets"); continue; }

                List<GescandProduct> scannedProducts = [];
                for (int j = 1; j < rawTicket.Length; j += 2) {
                    string productCode = rawTicket[j];
                    if (!int.TryParse(rawTicket[j + 1], out int amount)) {
                        Logger.LogError("Fout aantal in gescande producten in geparkeerde tickets");
                        continue;
                    }
                    Product? product = _possibleProducts.Find(x => (x.Code == productCode));
                    GescandProduct scannedProduct;
                    try { scannedProduct = new(product!, amount); }
                    catch { Logger.LogError("Foute gescande product in geparkeerde tickets"); continue; }
                    scannedProducts.Add(scannedProduct);
                }

                KassaTicket ticket;
                try { ticket = new(creationTime, scannedProducts); }
                catch { Logger.LogError("Fout bij aanmaken ticket"); continue; }
                _tickets.Add(ticket);
            }
            Logger.GeneralLog(_tickets.Count + " geparkeerde tickets ingeladen");
        }

        private static void StoreTicket(KassaTicket ticket) {
            string ticketToRecord = ticket.ToRecordString();
            File.AppendAllText(PARKEDTICKETSFILEPATH, $"{ticketToRecord}\n");
        }
        private static void RemoveStoredTicket(KassaTicket ticket) {
            string ticketTimeStamp = ticket.CreationDateTime.Ticks.ToString();
            List<string> parkedTicketsString = [.. File.ReadAllLines(PARKEDTICKETSFILEPATH)];
            int amountRemoved = parkedTicketsString.RemoveAll(x => x.StartsWith(ticketTimeStamp));
            if (amountRemoved > 1) throw new Exception(message: "Meer dan 1 element is verwijderd uit de parked tickets opslag");
            File.WriteAllLinesAsync(PARKEDTICKETSFILEPATH, parkedTicketsString);
        }

        public void GenerateNewKassaTicket() {
            KassaTicket newTicket = new();
            _tickets.Add(newTicket);
            CurrentTicket = newTicket;
            Logger.LogNewTicket(newTicket);
        }

        public void IncreaseProduct(Product? product, int amount = 1, bool actionHistory = true) {
            CurrentTicket.IncreaseProduct(product, amount, actionHistory);
        }
        public void IncreaseLastProduct(int amount = 1, bool actionHistory = true) {
            CurrentTicket.IncreaseLastProduct(amount, actionHistory);
        }
        public void DiminishProduct(Product? product, int amount = 1, bool actionHistory = true) {
            CurrentTicket.DiminishProduct(product, amount, actionHistory);
        }
        public void DiminishLastProduct(int amount = 1, bool actionHistory = true) {
            CurrentTicket.DiminishLastProduct(amount, actionHistory);
        }
        public void UndoLastProductAmountChange() {
            CurrentTicket.UndoLastProductAmountChange();
        }

        public void ParkTicket() {
            KassaTicket ticket = CurrentTicket;
            StoreTicket(ticket);
            GenerateNewKassaTicket();
            Logger.LogParkTicket(ticket);
        }

        public void RemoveTicket() {
            KassaTicket ticket = CurrentTicket;
            bool succes = _tickets.Remove(ticket);
            if (!succes) throw new ArgumentException(message: "Ticket niet verwijderd");
            Logger.LogCancelTicket(ticket);
            Logger.SaveTicket(ticket);
        }

        public void FinishTicketCard(BetaalDetails betaalDetails) {
            KassaTicket ticket = CurrentTicket;
            bool isRemoved = _tickets.Remove(ticket);
            if (!isRemoved) throw new ArgumentException(message: "Ticket niet afegrond");
            Logger.LogPaidTicketCard(ticket, betaalDetails);
            Logger.SaveTicket(ticket, TicketSoort.Kaart, betaalDetails);
        }

        public void FinishTicketCash() {
            KassaTicket ticket = CurrentTicket;
            bool succes = _tickets.Remove(ticket);
            if (!succes) throw new ArgumentException(message: "Ticket niet afegrond");
            Logger.LogPaidTicketCash(ticket);
            Logger.SaveTicket(ticket, TicketSoort.Cash);
        }

        public void ResumeTicketByIndex(int index) {
            KassaTicket ticket = _tickets[index];
            StoreTicket(CurrentTicket);
            CurrentTicket = ticket;
            RemoveStoredTicket(ticket);
            Logger.LogResumeTicket(ticket);
        }

        private KassaTicket GetLastTicket() {
            KassaTicket ticket = _tickets[^1];
            RemoveStoredTicket(ticket);
            return ticket;
        }

        public static bool IsEan13(string code) {
            return Product.IsEan13(code);
        }

        public List<string> GetTicketListString() {
            List<string> output = [];
            foreach (var ticket in _tickets) {
                output.Add(ticket.ToString());
            }
            return output;
        }

        public Product? GetProductByCode(string code) {
            Product product = _possibleProducts.Find((product) => product.Code == code)!;
            if (product == default) {
                return null;
            } else {
                return product;
            }
        }

        public BetaalDetails? VerzoekBetaling(string boodschap) {
            return _betaalTerminal.VerzoekBetaling(CurrentTicket.TotalPrice, boodschap);
        }
    }
}
