using System.Text;
using WinkelDomein.Enums;

namespace WinkelDomein.Model {
    public class KassaTicket {

        public const string SHOPNAME = "WARENHUIS OVERFLOW";
        public const string ADDRES = "Stapelplein 1, 9000 Gent";
        public const string TEL = "Tel: 09 234 56 78";
        public const string BTWNUMBER = "BTW: BE 0123.456.789";

        private readonly DateTime _creationDateTime;
        private static readonly Random _random = new(DateTime.Now.Millisecond);
        private readonly List<GescandProduct> _scannedProducts = [];
        private readonly List<KeyValuePair<Product, int>> _actionHistory = [];
        private string _cashRef = "";

        public DateTime CreationDateTime {
            get => _creationDateTime;
            init => _creationDateTime = value;
        }
        /*  Ik ben op de hoogte dat er telkens een nieuw ticketnummer moet worden gegenereerd per aanpassing
   van het ticket, maar ik heb ervoor gekozen om dit niet te doen, omdat ik dit niet logisch vind
   in het kader van de log-functionaliteit. Dit zorgt er voor dat elke vermelding van de ticketcode
   in de log, buiten de laatste, nutteloos is */
        public string TicketCode => _creationDateTime.ToString("yyyy.MM.dd.HH.mm.ss.fff");
        public string Date => _creationDateTime.ToString("yyyy-MM-dd HH:mm");
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
                    totalPrice += scannedProduct.Quantity * scannedProduct.Product.PriceOnlyBtw;
                }
                return Math.Round(totalPrice, 2);
            }
        }
        public decimal TotalPrice => TotalPriceNoBtw + TotalBtw;
        public List<GescandProduct> ScannedProducts {
            get => _scannedProducts;
            init => _scannedProducts = value;
        }

        public bool HasScannedProducts => _scannedProducts.Count != 0;
        public int AmountOfProducts => _scannedProducts.Count;

        public KassaTicket() {
            CreationDateTime = DateTime.Now;
            CashRef = $"CASH-{_creationDateTime:yyyyMMdd}-{_random.Next(100000, 999999)}";
        }
        public KassaTicket(DateTime creationTime, List<GescandProduct> scannedProducts) : this() {
            CreationDateTime = creationTime;
            ScannedProducts = scannedProducts;
        }

        /* Voegt een nieuw product toe aan het ticket moest het nog niet in ScannedProducts zitten,
        of verhoogt het aantal van het product in ScannedProducts dat overeenkomt met het gegeven product, met het opgegeven aantal */
        public void IncreaseProduct(Product? product, int amount = 1, bool actionHistory = true) {
            if (amount == 0) return;
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = ScannedProducts.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                GescandProduct newGescandProduct = new(product, amount);
                ScannedProducts.Add(newGescandProduct);
            } else {
                foundGescandProduct.Quantity += amount; ;
            }
            if (actionHistory) _actionHistory.Add(new(product, amount));
            Logger.LogScanProduct(this, product, amount);
        }
        /* Verminderd het aantal van het product in ScannedProducts dat overeenkomt met het opgegeven product met het gegeven aantal,
        Verwijdert het product volledig uit ScannedProducts moest het opgegeven aantal gelijk of meer zijn aan het te verminderen aantal*/
        public void DiminishProduct(Product? product, int amount = 1, bool actionHistory = true) {
            if (amount == 0) return;
            if (product == null) throw new ArgumentException(message: "Dit product bestaat niet");
            GescandProduct foundGescandProduct = ScannedProducts.Find((gescandProduct) => gescandProduct.Product == product)!;
            if (foundGescandProduct == default) {
                throw new ArgumentException(message: "Het kassaticket bevat dit product niet");
            } else if (foundGescandProduct.Quantity <= amount) {
                ScannedProducts.Remove(foundGescandProduct);
                if (actionHistory) _actionHistory.Add(new(product, foundGescandProduct.Quantity * -1));
                Logger.LogRemoveProduct(this, product, foundGescandProduct.Quantity);
            } else {
                foundGescandProduct.Quantity -= amount;
                if (actionHistory) _actionHistory.Add(new(product, amount * -1));
                Logger.LogRemoveProduct(this, product, amount);
            }
        }

        public void IncreaseLastProduct(int amount = 1, bool actionHistory = true) {
            if (AmountOfProducts == 0) throw new ArgumentException(message: "Er zijn nog geen producten ingescand");
            GescandProduct gescandProduct = ScannedProducts[^1];
            IncreaseProduct(gescandProduct.Product, amount, actionHistory);
        }
        public void DiminishLastProduct(int amount = 1, bool actionHistory = true) {
            if (AmountOfProducts == 0) throw new ArgumentException(message: "Er zijn nog geen producten ingescand");
            GescandProduct gescandProduct = ScannedProducts[^1];
            DiminishProduct(gescandProduct.Product, amount, actionHistory);
        }

        public void UndoLastProductAmountChange() {
            if (_actionHistory.Count == 0) throw new Exception(message: "Geen actiegeschiedenis beschikbaar");
            KeyValuePair<Product, int> lastAction = _actionHistory[^1];
            Product product = lastAction.Key;
            int amount = lastAction.Value;

            if (amount > 0) { // verwijder weer te toegevoegde zaken
                Logger.LogUndo(this);
                DiminishProduct(product, amount, actionHistory: false);
            } else if (amount < 0) { // voeg te verwijderde zaken weer toe
                Logger.LogUndo(this);
                int adjustedAmount = -amount;
                IncreaseProduct(product, adjustedAmount, actionHistory: false);
            } else {
                throw new Exception(message: "Actiegeschiedenis kan geen 0 bevatten");
            }
            _actionHistory.RemoveAt(_actionHistory.Count - 1);
        }

        public override string ToString() => $"#{TicketCode} ({AmountOfProducts} producten)";

        public string ToRecordString() {
            StringBuilder output = new();
            output.Append($"{_creationDateTime.Ticks}");
            foreach (var scannedProduct in ScannedProducts) {
                output.Append($";{scannedProduct.Product.Code};{scannedProduct.Quantity}");
            }
            return output.ToString();
        }

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
            string StringScannedProducts(int ticketWidth, string p) {
                StringBuilder result = new();
                if (HasScannedProducts) {
                    foreach (var product in ScannedProducts) {
                        result.AppendLine($"{p}  {product}");
                        if (product.Product.HasActiveReduction) result.AppendLine($"{p}  {product.Product.Reduction}");
                    }
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
