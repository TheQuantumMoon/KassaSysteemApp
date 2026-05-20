using System.Net.NetworkInformation;
using WinkelDomein.Enums;
using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace WinkelDomein {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;
        private List<Product> _possibleProducts = [];
        private List<KassaTicket> _tickets = [];
        private KassaTicket _currentTicket;
        private const string PRODUCTSFILEPATH = @"Producten.txt";
        private const string PARKEDTICKETSFILEPATH = @"ParkedTickets.txt";

        public KassaTicket CurrentTicket {
            get {
                if (!HasTickets) GenerateNewKassaTicket();
                if (!_tickets.Contains(_currentTicket)) CurrentTicket = GetLastTicket();
                return _currentTicket;
            }
            set => _currentTicket = value;
        }
        public bool HasTickets {
            get => _tickets.Count != 0;
        }
        public int TicketCount {
            get => _tickets.Count;
        }
        public int PossibleProductsCount => _possibleProducts.Count;

        public Kassa(IBetaalTerminal betaalTerminal) {
            _betaalTerminal = betaalTerminal;
            StartKassa();
        }

        private void StartKassa() {
            ParsePossibleProducts();
            ParseParkedTickets();
            Logger.SystemLog(PossibleProductsCount + " producten ingeladen.");
            Logger.SystemLog("Geen kortingscodes beschikbaar.");
            GenerateNewKassaTicket();
        }

        private void ParsePossibleProducts() {
            if (!File.Exists(PRODUCTSFILEPATH)) File.Create(PRODUCTSFILEPATH);
            string[] rawProducts = File.ReadAllLines(PRODUCTSFILEPATH);

            foreach (var line in rawProducts) {
                string[] productInfo = line.Split(';');
                string code = productInfo[0];
                if (!IsEan13(code)) throw new Exception(message: "Barcode is niet conform met de EAN13-standaard");
                string name = productInfo[1];
                decimal price = decimal.Parse(productInfo[2]);
                int btw = int.Parse(productInfo[3]);
                ProductCategorie category = Enum.Parse<ProductCategorie>(productInfo[4]);
                Product newProduct = new(code, name, price, btw, category);
                _possibleProducts.Add(newProduct);
            }
        }

        private void ParseParkedTickets() {
            if (!File.Exists(PARKEDTICKETSFILEPATH)) File.Create(PARKEDTICKETSFILEPATH);
            string[] rawTickets = File.ReadAllLines(PARKEDTICKETSFILEPATH);

            for (int i = 0; i < rawTickets.Length; i++) {
                string[] rawTicket = rawTickets[i].Split(';');
                DateTime creationTime = new(long.Parse(rawTicket[0]));
                List<GescandProduct> scannedProducts = [];
                for (int j = 1; j < rawTicket.Length; j += 2) {
                    string productCode = rawTicket[j];
                    int amount = int.Parse(rawTicket[j + 1]);
                    Product product = _possibleProducts.Find(x => (x.Code == productCode)) ?? throw new Exception(message: "Productcode niet herkend bij geparkeerde tickets");
                    GescandProduct scannedProduct = new(product, amount);
                    scannedProducts.Add(scannedProduct);
                }
                KassaTicket ticket = new(creationTime, scannedProducts);
                _tickets.Add(ticket);
            }
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

        // Checkt of de ingegeven string een valide EAN13 barcode is
        public static bool IsEan13(string barcode) {
            if (string.IsNullOrWhiteSpace(barcode) || barcode.Length != 13 || !barcode.All(char.IsDigit)) return false;
            int[] numbers = [.. barcode.Select(x => x - '0')];

            int sum = 0;
            for (int i = 0; i < 12; i++) {
                int digit = numbers[i];
                if (i % 2 == 0) sum += digit * 1;
                else sum += digit * 3;
            }
            int remainder = sum % 10;
            int calculatedCheckDigit = (remainder == 0) ? 0 : 10 - remainder;
            int actualCheckDigit = numbers[12];

            return calculatedCheckDigit == actualCheckDigit;
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
