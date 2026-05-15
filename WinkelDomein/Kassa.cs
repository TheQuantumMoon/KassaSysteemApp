using WinkelDomein.Enum;
using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace WinkelDomein {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;
        private List<Product> _possibleProducts = [];
        private List<KassaTicket> _tickets = [];

        public bool HasTickets {
            get => _tickets.Count != 0;
        }
        public int TicketCount {
            get => _tickets.Count;
        }
        public int PossibleProductsCount => _possibleProducts.Count;
        public List<KassaTicket> Tickets { get => _tickets; set => _tickets = value; }

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

        public KassaTicket GenerateNewKassaTicket() {
            KassaTicket newTicket = new();
            _tickets.Add(newTicket);
            Logger.LogNewTicket(newTicket);
            return newTicket;
        }

        public KassaTicket ParkKassaTicket(KassaTicket kassaTicket) {
            KassaTicket newTicket = GenerateNewKassaTicket();
            Logger.LogParkTicket(kassaTicket);
            return newTicket;
        }

        public void RemoveTicket(KassaTicket ticket) {
            bool succes = _tickets.Remove(ticket);
            if (!succes) throw new ArgumentException(message: "Ticket niet verwijderd");
            Logger.LogCancelTicket(ticket);
            Logger.SaveTicket(ticket);
        }

        public void FinishTicketCard(KassaTicket ticket, BetaalDetails betaalDetails) {
            bool isRemoved = _tickets.Remove(ticket);
            if (!isRemoved) throw new ArgumentException(message: "Ticket niet afegrond");
            Logger.LogPaidTicketCard(ticket, betaalDetails);
            Logger.SaveTicket(ticket, TicketSoort.Kaart, betaalDetails);
        }

        public void FinishTicketCash(KassaTicket ticket) {
            bool succes = _tickets.Remove(ticket);
            if (!succes) throw new ArgumentException(message: "Ticket niet afegrond");
            Logger.LogPaidTicketCash(ticket);
            Logger.SaveTicket(ticket, TicketSoort.Cash);
        }

        public KassaTicket ResumeTicketByIndex(int index) {
            KassaTicket ticket = _tickets[index];
            Logger.LogResumeTicket(ticket);
            return ticket;
        }

        public KassaTicket GetLastTicket() {
            if (HasTickets) {
                KassaTicket ticket = ResumeTicketByIndex(TicketCount - 1);
                return ticket;
            } else {
                return null!;
            }
        }

        public Product? GetProductByCode(string code) {
            Product product = _possibleProducts.Find((product) => product.Code == code)!;
            if (product == default) {
                return null;
            } else {
                return product;
            }
        }

        public BetaalDetails? VerzoekBetaling(decimal bedrag, string boodschap) {
            return _betaalTerminal.VerzoekBetaling(bedrag, boodschap);
        }
    }
}
