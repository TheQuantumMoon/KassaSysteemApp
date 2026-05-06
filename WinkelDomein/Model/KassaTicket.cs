using System.Security.Cryptography.X509Certificates;

namespace WinkelDomein.Model {
    public class KassaTicket {

        private readonly List<GescandProduct> _scannedProducts = [];

        public static readonly string SHOPNAME = "WARENHUIS OVERFLOW";
        public static readonly string ADDRES = "Stapelplein 1, 9000 Gent";
        public static readonly string TEL = "Tel: 09 234 56 78";
        public static readonly string BTWNUMBER = "BTW: BE 0123.456.789";

        public List<GescandProduct> Products {
            get => _scannedProducts;
        }
        public string TicketCode {
            get => DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
        }
        public string Date {
            get => DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
        public bool HasScannedProducts {
            get => _scannedProducts.Count != 0;
        }
        public decimal TotalPrice {
            get {
                if (_scannedProducts.Count == 0) return 0m;
                decimal totalPrice = 0m;
                foreach (var scannedProduct in _scannedProducts) {
                    totalPrice += scannedProduct.Quantity * scannedProduct.Product.Price;
                }
                return totalPrice;
            }
        }
        public decimal TotalBtw {
            get {
                if (_scannedProducts.Count == 0) return 0m;
                decimal totalPrice = 0m;
                foreach (var scannedProduct in _scannedProducts) {
                    totalPrice += scannedProduct.Quantity * scannedProduct.Product.Price * (scannedProduct.Product.Btw / 100m);
                }
                return Math.Round(totalPrice, 2);
            }
        }

        public void IncreaseProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = _scannedProducts.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                GescandProduct newGescandProduct = new(product, amount);
                _scannedProducts.Add(newGescandProduct);
            } else {
                foundGescandProduct.Quantity += amount;
            }
        }
        public void DiminishProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = _scannedProducts.Find((gescandProduct) => gescandProduct.Product == product)!;
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
