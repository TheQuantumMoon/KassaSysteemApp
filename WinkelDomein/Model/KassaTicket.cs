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

        public void AddProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentNullException("Dit product bestaat niet");
            GescandProduct foundGescandProduct = _products.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                GescandProduct newGescandProduct = new(product, amount);
                _products.Add(newGescandProduct);
            } else {
                foundGescandProduct.Quantity += amount;
            }
        }
        public void RemoveProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentNullException("Dit product bestaat niet");
            GescandProduct foundGescandProduct = _products.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                throw new ArgumentNullException("Het kassaticket bevat dit product niet");
            } else if (foundGescandProduct.Quantity <= amount) {
                Products.Remove(foundGescandProduct);
            } else {
                foundGescandProduct.Quantity -= amount;
            }
        }
        public void IncreaseAmountOfLastProduct(int amount) {
            if (Products.Count != 0) {
                GescandProduct product = Products[^1];
                product.Quantity += amount;
            } else throw new ArgumentException("Er zijn nog geen producten ingescand");
        }
        public void DiminishAmountOfProduct(GescandProduct gescandProduct, int amount) {
            gescandProduct.Quantity -= amount;
        }

    }
}
