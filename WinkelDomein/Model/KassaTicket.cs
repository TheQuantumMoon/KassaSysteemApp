namespace WinkelDomein.Model {
    public class KassaTicket {

        private readonly List<GescandProduct> _products = [];

        public List<GescandProduct> Products {
            get => _products;
        }
        public string TicketCode {
            get => DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
        }
        public string Date {
            get => DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public void IncreaseProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = _products.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                GescandProduct newGescandProduct = new(product, amount);
                _products.Add(newGescandProduct);
            } else {
                foundGescandProduct.Quantity += amount;
            }
        }
        public void DiminishProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = _products.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                throw new ArgumentException(message: "Het kassaticket bevat dit product niet");
            } else if (foundGescandProduct.Quantity <= amount) {
                Products.Remove(foundGescandProduct);
            } else {
                foundGescandProduct.Quantity -= amount;
            }
        }
        public void IncreaseLastProduct(int amount = 1) {
            if (Products.Count == 0) throw new ArgumentException(message: "Er zijn nog geen producten ingescand");
            GescandProduct gescandProduct = Products[^1];
            IncreaseProduct(gescandProduct.Product, amount);
        }
        public void DiminishLastProduct(int amount = 1) {
            if (Products.Count == 0) throw new ArgumentException(message: "Er zijn nog geen producten ingescand");
            GescandProduct gescandProduct = Products[^1];
            DiminishProduct(gescandProduct.Product, amount);
        }
    }
}
