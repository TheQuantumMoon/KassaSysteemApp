namespace WinkelDomein.Model {
    public class KassaTicket {

        private static readonly Random _random = new(DateTime.Now.Millisecond);
        private readonly List<GescandProduct> _scannedProducts = [];

        private readonly string _ticketCode = "";
        private readonly string _date = "";
        private readonly string _cashRef = "";

        public static readonly string SHOPNAME = "WARENHUIS OVERFLOW";
        public static readonly string ADDRES = "Stapelplein 1, 9000 Gent";
        public static readonly string TEL = "Tel: 09 234 56 78";
        public static readonly string BTWNUMBER = "BTW: BE 0123.456.789";

        public KassaTicket() {
            DateTime now = DateTime.Now;
            TicketCode = now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
            Date = now.ToString("yyyy-MM-dd HH:mm");
            CashRef = $"CASH-{now:yyyyMMdd}-{_random.Next(100000, 999999)}";
        }

        public string TicketCode {
            get => _ticketCode;
            init => _ticketCode = value;
        }
        public string Date {
            get => _date;
            init => _date = value;
        }
        public string CashRef {
            get => _cashRef;
            init => _cashRef = value;
        }

        public List<GescandProduct> Products => _scannedProducts;
        public bool HasScannedProducts => _scannedProducts.Count != 0;
        public int AmountOfProducts => _scannedProducts.Count;

        public decimal TotalPriceNoBtw {
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
        public decimal TotalPrice => TotalPriceNoBtw + TotalBtw;

        public void IncreaseProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = _scannedProducts.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                GescandProduct newGescandProduct = new(product, amount);
                _scannedProducts.Add(newGescandProduct);
            } else {
                foundGescandProduct.Quantity += amount;
            }
            Logger.LogScanProduct(this, product, amount);
        }
        public void DiminishProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = _scannedProducts.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                throw new ArgumentException(message: "Het kassaticket bevat dit product niet");
            } else if (foundGescandProduct.Quantity <= amount) {
                Products.Remove(foundGescandProduct);
                Logger.LogRemoveProduct(this, product, foundGescandProduct.Quantity);
            } else {
                foundGescandProduct.Quantity -= amount;
                Logger.LogRemoveProduct(this, product, amount);
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

        public override string ToString() {
            return $"#{TicketCode} ({AmountOfProducts} producten)";
        }
    }
}
