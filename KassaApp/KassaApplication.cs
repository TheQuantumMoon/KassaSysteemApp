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
            DisplayTicket(kassaTicket);
            string input = ReadLine()!.Trim().ToUpper();

            // Check of de input een productcode is, zoja, voeg het product toe aan het ticket
            Product? product = _kassa.GetProductByCode(input);
            if (product != null) {
                kassaTicket.AddProduct(product);
                continue;
            }

            // Check of de input een int is, zoja pas het aantal van het laast ingegeven product aan
            bool isInt = int.TryParse(input, out int amount);
            if (isInt) {
                try { kassaTicket.IncreaseAmountOfLastProduct(amount); }
                catch (Exception ex) { WriteLine(ex.Message); }
                continue;
            }

            // Check of de input een van de optie letters is
            switch (input) {
                // Verwijderen
                case "D":
                    break;

                case "Z":
                    break;

                case "K":
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

    public static void DisplayTicket(KassaTicket ticket, int ticketWidth = 50, int paddingLeft = 2) {

        WriteLine();
        PrintThickLine(ticketWidth);
        PrintTicketHeader(ticketWidth);
        PrintThickLine(ticketWidth);
        WriteLineLeftPadding($"Ticket: {ticket.TicketCode}", paddingLeft);
        WriteLineLeftPadding($"Datum: {ticket.Date}", paddingLeft);
        PrintThinLine(ticketWidth);
        PrintGescandeProducten(ticket.Products, paddingLeft);
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
    public static void PrintThinLine(int length) {
        string thinLine = new('-', length);
        WriteLine(thinLine);
    }
    public static void PrintThickLine(int length) {
        string thickLine = new('=', length);
        WriteLine(thickLine);
    }
    public static void PrintTicketHeader(int ticketWidth) {
        WriteLineCenter("WARENHUIS OVERFLOW", ticketWidth);
        WriteLineCenter("Stapelplein 1, 9000 Gent", ticketWidth);
        WriteLineCenter("Tel: 09 234 56 78", ticketWidth);
        WriteLineCenter("BTW: BE 0123.456.789", ticketWidth);
    }
    public static void PrintGescandeProducten(List<GescandProduct> products, int paddingLeft = 0) {
        if (products.Count != 0) {
            foreach (var product in products) WriteLineLeftPadding(product.ToString(), paddingLeft);
        } else {
            WriteLineLeftPadding("(leeg)", paddingLeft);
        }
    }
    public static void PrintUserInstructions() {
        ForegroundColor = ConsoleColor.DarkGray;
        WriteLine("<scan barcode> of [barcode]<Enter> | [aantal extra]<Enter>\n" +
            "[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste\n" +
            "[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash\n" +
            "[P]<Enter> = parkeren | [H]<Enter> = hervatten | [A]<Enter> = afbreken\n");
        ResetColor();
    }
}
