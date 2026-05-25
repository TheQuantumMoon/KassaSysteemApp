namespace WinkelDomein.Model {
    public class GescandProduct {

        private int _quantity;
        private readonly Product _product = default!;

        public GescandProduct(Product product, int quantity) {
            Product = product;
            Quantity = quantity;
        }

        public Product Product {
            get => _product;
            init {
                ArgumentNullException.ThrowIfNull(value);
                _product = value;
            }
        }
        public int Quantity {
            get => _quantity;
            set {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                _quantity = value;
            }
        }

        public override string ToString() => $"{Quantity}x {Product.Name} €{Product.Price} {Product.Btw}%";
    }
}
