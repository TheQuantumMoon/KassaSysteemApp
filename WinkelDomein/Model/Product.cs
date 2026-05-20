using WinkelDomein.Enums;

namespace WinkelDomein.Model {
    public class Product {
        private readonly string _code = "";
        private readonly string _name = "";
        private readonly decimal _price;
        private readonly int _btw;
        private readonly ProductCategorie _category;

        public Product(string code, string name, decimal price, int btw, ProductCategorie category) {
            Code = code;
            Name = name;
            Price = price;
            Btw = btw;
            Category = category;
        }

        public string Code {
            get => _code;
            init {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
                _code = value;
            }
        }
        public string Name {
            get => _name;
            init {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
                _name = value;
            }
        }
        public decimal Price {
            get => _price;
            init {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 0m);
                _price = value;
            }
        }
        public int Btw {
            get => _btw;
            init {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
                _btw = value;
            }
        }

        public ProductCategorie Category {
            get => _category;
            init => _category = value;
        }

        public override string ToString() => $"{Name} ({Code})";
    }
}

