using System.Globalization;
using WinkelDomein.Enums;
using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace WinkelDomein {
    public class Kassa {
        private readonly IBetaalTerminal _terminal;
        private readonly IDataOpslag _repo;
        private readonly List<Korting> _reductions = [];
        private readonly List<Product> _possibleProducts = [];
        private readonly List<KassaTicket> _tickets = [];
        private KassaTicket _currentTicket = default!;

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

        public Kassa(IBetaalTerminal betaalTerminal, IDataOpslag repo) {
            _terminal = betaalTerminal;
            _repo = repo;
            _reductions = _repo.ParseReductions();
            _possibleProducts = _repo.ParsePossibleProducts(_reductions);
            _tickets = _repo.ParseParkedTickets(_possibleProducts);
            Start();
        }

        private void Start() {
            GenerateNewKassaTicket();
        }

        public void GenerateNewKassaTicket() {
            KassaTicket newTicket = new();
            _tickets.Add(newTicket);
            CurrentTicket = newTicket;
            Logger.LogNewTicket(newTicket);
        }

        public void IncreaseProduct(Product? product, int amount = 1, bool actionHistory = true) => CurrentTicket.IncreaseProduct(product, amount, actionHistory);
        public void IncreaseLastProduct(int amount = 1, bool actionHistory = true) => CurrentTicket.IncreaseLastProduct(amount, actionHistory);
        public void DiminishProduct(Product? product, int amount = 1, bool actionHistory = true) => CurrentTicket.DiminishProduct(product, amount, actionHistory);
        public void DiminishLastProduct(int amount = 1, bool actionHistory = true) => CurrentTicket.DiminishLastProduct(amount, actionHistory);
        public void UndoLastProductAmountChange() => CurrentTicket.UndoLastProductAmountChange();

        public void ParkTicket() {
            KassaTicket ticket = CurrentTicket;
            _repo.StoreTicket(ticket);
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
            _repo.StoreTicket(CurrentTicket);
            CurrentTicket = ticket;
            _repo.RemoveStoredTicket(ticket);
            Logger.LogResumeTicket(ticket);
        }

        private KassaTicket GetLastTicket() {
            KassaTicket ticket = _tickets[^1];
            _repo.RemoveStoredTicket(ticket);
            return ticket;
        }

        public static bool IsEan13(string code) => Product.IsEan13(code);

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
            return _terminal.VerzoekBetaling(CurrentTicket.TotalPrice, boodschap);
        }
    }
}
