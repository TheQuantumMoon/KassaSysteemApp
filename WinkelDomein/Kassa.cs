using WinkelDomein.Enum;
using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace WinkelDomein {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;
        private List<Product> _possibleProducts = [];
        private List<KassaTicket> _tickets = [];
        private KassaTicket _currentTicket;

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
            Logger.SystemLog(PossibleProductsCount + " producten ingeladen.");
            Logger.SystemLog("Geen kortingscodes beschikbaar.");
        }

        private void ParsePossibleProducts() {
            string productsFilepath = @"Producten.txt";
            string[] rawProducts = File.ReadAllLines(productsFilepath);

            foreach (var line in rawProducts) {
                string[] productInfo = line.Split(';');
                string code = productInfo[0];
                string name = productInfo[1];
                decimal price = decimal.Parse(productInfo[2]);
                int btw = int.Parse(productInfo[3]);
                Product newProduct = new(code, name, price, btw);
                _possibleProducts.Add(newProduct);
            }
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

        public void ParkKassaTicket() {
            KassaTicket ticket = CurrentTicket;
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
            CurrentTicket = ticket;
            Logger.LogResumeTicket(ticket);
        }

        private KassaTicket GetLastTicket() {
            KassaTicket ticket = _tickets[^1];
            return ticket;
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
