using System.Text;
using WinkelDomein.Enum;

namespace WinkelDomein.Model {
    public class KassaTicket {

        private static readonly Random _random = new(DateTime.Now.Millisecond);
        private readonly List<GescandProduct> _scannedProducts = [];

        public const string SHOPNAME = "WARENHUIS OVERFLOW";
        public const string ADDRES = "Stapelplein 1, 9000 Gent";
        public const string TEL = "Tel: 09 234 56 78";
        public const string BTWNUMBER = "BTW: BE 0123.456.789";

        private readonly string _ticketCode = "";
        private readonly string _date = "";
        private readonly string _cashRef = "";

        /*  Ik ben op de hoogte dat er telkens een nieuw ticketnummer moet worden gegenereerd per aanpassing
            van het ticket, maar ik heb ervoor gekozen om dit niet te doen, omdat ik dit niet logisch vind
            in het kader van de log-functionaliteit. Dit zorgt er voor dat elke vermelding van de ticketcode
            in de log, buiten de laatste, nutteloos is */
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

        public decimal TotalPriceNoBtw {
            get {
                decimal totalPrice = 0m;
                foreach (var scannedProduct in _scannedProducts) {
                    totalPrice += scannedProduct.Quantity * scannedProduct.Product.Price;
                }
                return totalPrice;
            }
        }
        public decimal TotalBtw {
            get {
                decimal totalPrice = 0m;
                foreach (var scannedProduct in _scannedProducts) {
                    totalPrice += scannedProduct.Quantity * scannedProduct.Product.Price * (scannedProduct.Product.Btw / 100m);
                }
                return Math.Round(totalPrice, 2);
            }
        }
        public decimal TotalPrice => TotalPriceNoBtw + TotalBtw;
        public List<GescandProduct> ScannedProducts => _scannedProducts;
        public bool HasScannedProducts => _scannedProducts.Count != 0;
        public int AmountOfProducts => _scannedProducts.Count;

        public KassaTicket() {
            DateTime now = DateTime.Now;
            TicketCode = now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
            Date = now.ToString("yyyy-MM-dd HH:mm");
            CashRef = $"CASH-{now:yyyyMMdd}-{_random.Next(100000, 999999)}";
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
            Logger.LogScanProduct(this, product, amount);
        }
        public void DiminishProduct(Product? product, int amount = 1) {
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = _scannedProducts.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                throw new ArgumentException(message: "Het kassaticket bevat dit product niet");
            } else if (foundGescandProduct.Quantity <= amount) {
                ScannedProducts.Remove(foundGescandProduct);
                Logger.LogRemoveProduct(this, product, foundGescandProduct.Quantity);
            } else {
                foundGescandProduct.Quantity -= amount;
                Logger.LogRemoveProduct(this, product, amount);
            }
        }
        public void IncreaseLastProduct(int amount = 1) {
            if (ScannedProducts.Count == 0) throw new ArgumentException(message: "Er zijn nog geen producten ingescand");
            GescandProduct gescandProduct = ScannedProducts[^1];
            IncreaseProduct(gescandProduct.Product, amount);
        }
        public void DiminishLastProduct(int amount = 1) {
            if (ScannedProducts.Count == 0) throw new ArgumentException(message: "Er zijn nog geen producten ingescand");
            GescandProduct gescandProduct = ScannedProducts[^1];
            DiminishProduct(gescandProduct.Product, amount);
        }

        public override string ToString() => $"#{TicketCode} ({AmountOfProducts} producten)";

        public string ToStringLayout(TicketSoort soort = TicketSoort.Normaal, BetaalDetails? betaalDetails = null,
            int ticketWidth = 42, int paddingLeft = 2) {
            string p = new(' ', paddingLeft);
            StringBuilder stringLayout = new(
                $"\n" +
                $"{StringThickLine(ticketWidth)}\n" +
                $"{StringTicketHeader(ticketWidth)}" +
                $"{StringThickLine(ticketWidth)}\n" +
                $"{p}Ticket: {TicketCode}\n" +
                $"{p}Datum:  {Date}\n" +
                $"{StringThinLine(ticketWidth)}\n" +
                $"{StringScannedProducts(ticketWidth, p)}" +
                $"{StringThickLine(ticketWidth)}\n");
            if (soort == TicketSoort.Normaal) {
                // er wordt niets toegevoegd
            } else if (soort == TicketSoort.Cash) {
                stringLayout.Append(
                    $"{p}Contante betaling\n" +
                    $"{p}Bedrag:\t\t€   {TotalPrice}\n" +
                    $"{p}Ref: {CashRef}\n" +
                    $"{StringThickLine(ticketWidth)}\n" +
                    $"\n");
            } else if (soort == TicketSoort.Kaart) {
                stringLayout.Append(
                    $"{p}{betaalDetails!.KaartVariant}\n" +
                    $"{p}{betaalDetails!.GemaskerdKaartnummer}\n" +
                    $"{p}{betaalDetails!.Methode}\n" +
                    $"{p}Bedrag:\t\t€   {betaalDetails!.Bedrag}\n" +
                    $"{p}Ref: {betaalDetails!.TransactieReferentie}\n" +
                    $"{StringThickLine(ticketWidth)}\n" +
                    $"\n");
            }
            return stringLayout.ToString();

            static string StringThinLine(int length) => new('-', length);
            static string StringThickLine(int length) => new('=', length);
            static string CenterString(string text, int totalWidth) {
                int calculatedPadding = (text.Length + totalWidth) / 2;
                return text.PadLeft(calculatedPadding);
            }
            static string StringTicketHeader(int ticketWidth) {
                string result =
                $"{CenterString(SHOPNAME, ticketWidth)}\n" +
                $"{CenterString(ADDRES, ticketWidth)}\n" +
                $"{CenterString(TEL, ticketWidth)}\n" +
                $"{CenterString(BTWNUMBER, ticketWidth)}\n";
                return result;
            }
            string StringScannedProducts(int ticketWidth, string p){
                StringBuilder result = new();
                if (HasScannedProducts) {
                    foreach (var product in ScannedProducts) result.AppendLine($"{p}  {product}");
                    result.Append(
                        $"{StringThinLine(ticketWidth)}\n" +
                        $"{p}Subtotaal excl. BTW:\t€   {TotalPriceNoBtw}\n" +
                        $"{p}  --> BTW:\t\t€   {TotalBtw}\n" +
                        $"{StringThinLine(ticketWidth)}\n" +
                        $"{p}TOTAAL:\t\t€   {TotalPrice}\n");
                } else {
                    result.Append($"{p}(leeg)\n");
                }
                return result.ToString();
            }
        }
    }
}
