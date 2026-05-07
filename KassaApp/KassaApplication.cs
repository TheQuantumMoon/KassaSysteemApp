using WinkelDomein;
using WinkelDomein.Enum;
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
        KassaTicket currentKassaTicket = _kassa.GenerateNewKassaTicket();
        while (true) {
            if (!_kassa.HasTickets) currentKassaTicket = _kassa.GenerateNewKassaTicket();

            DisplayTicket(currentKassaTicket);
            Write("> ");
            string input = AskInput().Trim().ToUpper();

            // Check of de input een productcode is, zoja, voeg het product toe aan het ticket
            Product? product = _kassa.GetProductByCode(input);
            if (product != null) {
                currentKassaTicket.IncreaseProduct(product);
                continue;
            }

            // Check of de input een int is, zoja pas het aantal van het laast ingegeven product aan
            bool isInt = int.TryParse(input, out int amount);
            if (isInt) {
                try { currentKassaTicket.IncreaseLastProduct(amount); } catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                continue;
            }

            // Check of de input een van de optie letters is
            switch (input) {
                // Verwijderen
                case "D":
                    Write("Barcode: ");
                    input = AskInput().Trim();
                    product = _kassa.GetProductByCode(input);
                    try { currentKassaTicket.DiminishProduct(product); } catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                    break;

                // Verwijder laast toegevoegde product
                case "Z":
                    try { currentKassaTicket.DiminishLastProduct(); } catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                    break;

                // Betalen met kaart
                case "K":
                    PrintPaymentByCardPrompt(currentKassaTicket);
                    BetaalDetails? result = _kassa.VerzoekBetaling(currentKassaTicket.TotalPrice, "Betaling met de kaart");
                    if (result == null) {
                        WriteLineInColor("  X Betaling mislukt", ConsoleColor.Red);
                        continue;
                    }
                    DisplayTicket(currentKassaTicket, TicketSoort.Kaart, result);
                    _kassa.FinishTicket(currentKassaTicket);
                    break;

                // Betalen met cash
                case "C":
                    if (!currentKassaTicket.HasScannedProducts) {
                        WriteInColor("  Er zijn nog geen ingescande items", ConsoleColor.Red);
                        continue;
                    }
                    DisplayTicket(currentKassaTicket, TicketSoort.Cash);
                    _kassa.FinishTicket(currentKassaTicket);
                    currentKassaTicket = _kassa.GetLastTicket();
                    break;

                // Ticket parkeren
                case "P":
                    currentKassaTicket = _kassa.ParkKassaTicket(currentKassaTicket);
                    break;

                case "H":
                    List<KassaTicket> currentTickets = _kassa.Tickets;
                    WriteLine("  Gepakeerde tickets:");
                    for (int i = 0; i < currentTickets.Count; i++) WriteLine($"    {i + 1}. {currentTickets[i]}");
                    Write("Keuze: ");
                    input = AskInput().Trim();
                    isInt = int.TryParse(input, out int choice);
                    if (!isInt || choice < 1 || choice > currentTickets.Count) {
                        WriteLineInColor("Verkeerde input", ConsoleColor.Red);
                        continue;
                    }
                    currentKassaTicket = currentTickets[choice - 1];
                    break;

                case "A":
                    _kassa.RemoveKassaTicket(currentKassaTicket);
                    currentKassaTicket = _kassa.GetLastTicket();
                    break;

                default:
                    WriteLineInColor("  Input niet herkend", ConsoleColor.Red);
                    break;
            }

        }
    }

    public void DisplayTicket(KassaTicket ticket, TicketSoort soort = TicketSoort.Normaal, BetaalDetails? betaalDetails = default) {
        int ticketWidth = 42;
        int paddingLeft = 2;

        WriteLine();
        PrintThickLine(ticketWidth);
        PrintTicketHeader(ticketWidth);
        PrintThickLine(ticketWidth);
        WriteLineLeftPadding($"Ticket: {ticket.TicketCode}", paddingLeft);
        WriteLineLeftPadding($"Datum: {ticket.Date}", paddingLeft);
        PrintThinLine(ticketWidth);
        PrintScannedProducts(ticket, ticketWidth, paddingLeft);
        PrintThickLine(ticketWidth);
        if (soort == TicketSoort.Normaal) {
            WriteLine();
            PrintUserInstructions();
        } else if (soort == TicketSoort.Cash) {
            WriteLineLeftPadding("Contante betaling", paddingLeft);
            WriteLineLeftPadding("Bedrag:\t\t€   " + ticket.TotalPrice, paddingLeft);
            WriteLineLeftPadding("Ref: " + ticket.CashRef, paddingLeft);
            PrintThickLine(ticketWidth);
            WriteLine();
            WriteInColorLeftPadding("✓ Betaling ontvangen - €" + ticket.TotalPrice, ConsoleColor.Green, paddingLeft);
            WriteLine();
        } else if (soort == TicketSoort.Kaart) {
            WriteLineLeftPadding(betaalDetails!.KaartVariant, paddingLeft);
            WriteLineLeftPadding(betaalDetails!.GemaskerdKaartnummer, paddingLeft);
            WriteLineLeftPadding(betaalDetails!.Methode, paddingLeft);
            WriteLineLeftPadding("Bedrag:\t\t€   " + betaalDetails!.Bedrag, paddingLeft);
            WriteLineLeftPadding("Ref: " + betaalDetails!.TransactieReferentie, paddingLeft);
            PrintThickLine(ticketWidth);
            WriteLine();
            WriteInColorLeftPadding("✓ Betaling ontvangen - €" + betaalDetails.Bedrag, ConsoleColor.Green, paddingLeft);
            WriteLine();
        }
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
    public static void WriteInColorLeftPadding(string text, ConsoleColor color, int padding) {
        ForegroundColor = color;
        WriteLineLeftPadding(text, padding);
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
    public static void PrintScannedProducts(KassaTicket ticket, int ticketWidth, int paddingLeft) {
        List<GescandProduct> products = ticket.Products;
        if (products.Count != 0) {
            foreach (var product in products) WriteLineLeftPadding(product.ToString(), paddingLeft);
            PrintThinLine(ticketWidth);
            WriteLineLeftPadding("Subtotaal excl. BTW:\t€   " + ticket.TotalPriceNoBtw, paddingLeft);
            WriteLineLeftPadding("--> BTW:\t\t€   " + ticket.TotalBtw, paddingLeft + 2);
            PrintThinLine(ticketWidth);
            WriteLineLeftPadding("TOTAAL:\t\t€   " + ticket.TotalPrice, paddingLeft);
        } else {
            WriteLineLeftPadding("(leeg)", paddingLeft);
        }
    }
    public void PrintUserInstructions() {
        int amountOfTickets = _kassa.TicketCount;
        if (amountOfTickets > 1) WriteLineInColor($"[{amountOfTickets - 1} geparkeerd]", ConsoleColor.Cyan);
        WriteLineInColor("<scan barcode> of [barcode]<Enter> | [aantal extra]<Enter>\n" +
            "[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste\n" +
            "[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash\n" +
            "[P]<Enter> = parkeren | [H]<Enter> = hervatten | [A]<Enter> = afbreken", ConsoleColor.DarkGray);
    }
    public static void PrintPaymentByCardPrompt(KassaTicket kassaTicket) {
        WriteLine("  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
        WriteLine("  ┃       BETAALTERMINAL        ┃");
        WriteLine($"  ┃   Bedrag: €   {kassaTicket.TotalPrice}         ┃");
        WriteLine("  ┃   Bied uw kaart aan...      ┃");
        WriteLine("  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");

    }

    public static string AskInput() {
        ForegroundColor = ConsoleColor.Yellow;
        string input = ReadLine()!;
        ResetColor();
        return input;
    }
}
