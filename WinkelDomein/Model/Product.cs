using WinkelDomein.Enums;

namespace WinkelDomein.Model {
    public class Product {
        private readonly string _code = "";
        private readonly string _name = "";
        private readonly decimal _price;
        private readonly int _btw;
        private readonly ProductCategorie _category;
        private readonly Korting? _reduction;

        public Product(string code, string name, decimal price, int btw, ProductCategorie category, Korting? reduction = null) {
            Code = code;
            Name = name;
            Price = price;
            Btw = btw;
            Category = category;
            Reduction = reduction;
        }

        public string Code {
            get => _code;
            init {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
                if (!IsEan13(value)) throw new ArgumentException(message: "Productcode is niet EAN13-conform");
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
            get {
                if (HasActiveReduction) {
                    decimal adjustedprice = Math.Round(_price * (1m - Reduction!.ReductionPercentage / 100m), 2);
                    return adjustedprice;
                } else {
                    return _price;
                }
            }
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
        public decimal PriceOnlyBtw => Price * (Btw / 100m);
        public ProductCategorie Category {
            get => _category;
            init => _category = value;
        }
        public Korting? Reduction {
            get => _reduction;
            init => _reduction = value;
        }
        public bool HasActiveReduction {
            get {
                if (Reduction == null) return false;
                if (!Reduction.IsActive) return false;
                return true;
            }
        }

        // Checkt of de ingegeven string een valide EAN13 barcode is
        public static bool IsEan13(string barcode) {
            if (string.IsNullOrWhiteSpace(barcode) || barcode.Length != 13 || !barcode.All(char.IsDigit)) return false;
            int[] numbers = [.. barcode.Select(x => x - '0')];

            int sum = 0;
            for (int i = 0; i < 12; i++) {
                int digit = numbers[i];
                if (i % 2 == 0) sum += digit * 1;
                else sum += digit * 3;
            }
            int remainder = sum % 10;
            int calculatedCheckDigit = (remainder == 0) ? 0 : 10 - remainder;
            int actualCheckDigit = numbers[12];

            return calculatedCheckDigit == actualCheckDigit;
        }

        public override string ToString() => $"{Name} ({Code})";
    }
}

