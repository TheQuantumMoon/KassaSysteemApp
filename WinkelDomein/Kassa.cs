using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace WinkelDomein {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;
        private List<Product> _products = [];

        public List<Product> Products { get => _products; set => _products = value; }

        public Kassa(IBetaalTerminal betaalTerminal) {
            _betaalTerminal = betaalTerminal;
            StartKassa();
        }

        private void StartKassa() {
            ParseProducts();
        }

        private void ParseProducts() {
            string productsFilepath = @".\..\..\..\..\Producten.txt";
            string[] rawProducts = File.ReadAllLines(productsFilepath);

            foreach (var line in rawProducts) {
                string[] productInfo = line.Split(';');
                string code = productInfo[0];
                string name = productInfo[1];
                decimal price = decimal.Parse(productInfo[2]);
                int btw = int.Parse(productInfo[3]);
                Product newProduct = new(code, name, price, btw);
                _products.Add(newProduct);
            }
        }

        public BetaalDetails? VerzoekBetaling(decimal bedrag, string boodschap) {
            return _betaalTerminal.VerzoekBetaling(bedrag, boodschap);
        }
    }
}
