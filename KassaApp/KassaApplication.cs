using WinkelDomein;
using WinkelDomein.Model;
using static System.Console;

namespace KassaApp;

public class KassaApplication {
    private readonly Kassa _kassa;

    public KassaApplication(Kassa kassa) {
        _kassa = kassa;
        StartApplication();
    }

    public void StartApplication() {
        KassaTicket kassaTicket = _kassa.GenerateNewKassaTicket();
        while (true) {
            //Clear();
            DisplayTicket(kassaTicket);
            string input = AskInput().Trim().ToUpper();

            // Check of de input een productcode is, zoja, voeg het product toe aan het ticket
            Product? product = _kassa.GetProductByCode(input);
            if (product != null) {
                kassaTicket.IncreaseProduct(product);
                continue;
            }

            // Check of de input een int is, zoja pas het aantal van het laast ingegeven product aan
            bool isInt = int.TryParse(input, out int amount);
            if (isInt) {
                try { kassaTicket.IncreaseLastProduct(amount); }
                catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                continue;
            }

            // Check of de input een van de optie letters is
            switch (input) {
                // Verwijderen
                case "D":
                    Write("Barcode: ");
                    input = ReadLine()!.Trim();
                    product = _kassa.GetProductByCode(input);
                    try { kassaTicket.DiminishProduct(product); }
                    catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                    break;

                // Verwijder laast toegevoegde product
                case "Z":
                    try { kassaTicket.DiminishLastProduct(); }
                    catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                    break;

                // Betalen met kaart
                case "K":
                    BetaalDetails? result = _kassa.VerzoekBetaling(kassaTicket.TotalPrice, "Betaling met de kaart");
                    ProcessPayment(result);
                    break;

                case "C":
                    break;

                case "P":
                    break;

                case "H":
                    break;

                case "A":
                    break;

                default:
                    break;
            }

        }
    }

    public static void DisplayTicket(KassaTicket ticket, int ticketWidth = 42, int paddingLeft = 2) {

        WriteLine();
        PrintThickLine(ticketWidth);
        PrintTicketHeader(ticketWidth);
        PrintThickLine(ticketWidth);
        WriteLineLeftPadding($"Ticket: {ticket.TicketCode}", paddingLeft);
        WriteLineLeftPadding($"Datum: {ticket.Date}", paddingLeft);
        PrintThinLine(ticketWidth);
        PrintGescandeProducten(ticket, ticketWidth, paddingLeft);
        PrintThickLine(ticketWidth);
        WriteLine();
        PrintUserInstructions();
    }

    public static void WriteLineCenter(string text, int totalWidth) {
        int calculatedPadding = (text.Length + totalWidth) / 2;
        WriteLine(text.PadLeft(calculatedPadding));
    }
    public static void WriteLineLeftPadding(string text, int padding) {
        int calculatedPadding = text.Length + padding;
        WriteLine(text.PadLeft(calculatedPadding));
    }
    public static void WriteInColor(string text, ConsoleColor color) {
        ForegroundColor = color;
        Write(text);
        ResetColor();
    }
    public static void WriteLineInColor(string text, ConsoleColor color) {
        ForegroundColor = color;
        WriteLine(text);
        ResetColor();
    }
    public static void PrintThinLine(int length) {
        string thinLine = new('-', length);
        WriteLine(thinLine);
    }
    public static void PrintThickLine(int length) {
        string thickLine = new('=', length);
        WriteLine(thickLine);
    }
    public static void PrintTicketHeader(int ticketWidth) {
        WriteLineCenter(KassaTicket.SHOPNAME, ticketWidth);
        WriteLineCenter(KassaTicket.ADDRES, ticketWidth);
        WriteLineCenter(KassaTicket.TEL, ticketWidth);
        WriteLineCenter(KassaTicket.BTWNUMBER, ticketWidth);
    }
    public static void PrintGescandeProducten(KassaTicket ticket, int ticketWidth, int paddingLeft) {
        List<GescandProduct> products = ticket.Products;
        if (products.Count != 0) {
            foreach (var product in products) WriteLineLeftPadding(product.ToString(), paddingLeft);
            PrintThinLine(ticketWidth);
            WriteLineLeftPadding("Subtotaal excl. BTW:\t€   " + ticket.TotalPrice, paddingLeft);
            WriteLineLeftPadding("--> BTW:\t\t€   " + ticket.TotalBtw, paddingLeft + 2);
            PrintThinLine(ticketWidth);
            WriteLineLeftPadding("TOTAAL:\t\t€   " + (ticket.TotalPrice + ticket.TotalBtw), paddingLeft);
        } else {
            WriteLineLeftPadding("(leeg)", paddingLeft);
        }
    }
    public static void PrintUserInstructions() {
        ForegroundColor = ConsoleColor.DarkGray;
        WriteLine("<scan barcode> of [barcode]<Enter> | [aantal extra]<Enter>\n" +
            "[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste\n" +
            "[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash\n" +
            "[P]<Enter> = parkeren | [H]<Enter> = hervatten | [A]<Enter> = afbreken");
        ResetColor();
    }
    public static string AskInput() {
        Write("> ");
        ForegroundColor = ConsoleColor.Yellow;
        string input = ReadLine()!;
        ResetColor();
        return input;
    }

    public static void ProcessPayment(BetaalDetails? result) {
        if (result == null) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Betaling mislukt of geweigerd.");
            Console.ResetColor();
        } else {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Betaling geslaagd!");
            Console.ResetColor();
            Console.WriteLine($"  Kaart:      {result.KaartType} {result.KaartVariant}");
            Console.WriteLine($"  Nummer:     {result.GemaskerdKaartnummer}");
            Console.WriteLine($"  Methode:    {result.Methode}");
            Console.WriteLine($"  Bedrag:     €{result.Bedrag:F2}");
            Console.WriteLine($"  Referentie: {result.TransactieReferentie}");
            Console.WriteLine($"  Tijdstip:   {result.Tijdstip:yyyy-MM-dd HH:mm:ss}");
        }

        // ─── Stap 2: Snelle terminal voor bulktesten ────────────────────

        Console.WriteLine();
        Console.WriteLine("── Test 2: 20 snelle pogingen (50-200ms) ──");
        Console.WriteLine("   (~10% kans op weigering per poging)");
        Console.WriteLine();
    }
}
