using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace WinkelDomein {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;
        private List<Product> _possibleProducts = [];
        private List<KassaTicket> _tickets = [];

        public List<Product> PossibleProducts { get => _possibleProducts; set => _possibleProducts = value; }

        public Kassa(IBetaalTerminal betaalTerminal) {
            _betaalTerminal = betaalTerminal;
            StartKassa();
        }

        private void StartKassa() {
            ParsePossibleProducts();

        }

        private void ParsePossibleProducts() {
            string productsFilepath = @".\..\..\..\..\Producten.txt";
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
            return newTicket;
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
