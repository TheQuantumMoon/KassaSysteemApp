using static System.Console;
using WinkelDomein;

namespace KassaApp;

public class KassaApplication
{
    private readonly Kassa _kassa;

    public KassaApplication(Kassa kassa) {
        _kassa = kassa;
        StartApplication();
    }

    public void StartApplication()
    {
        DisplayTicket();
        ReadKey();
    }

    public static void DisplayTicket(int ticketWidth = 42, int paddingLeft = 2) {
        
        string ticketCode = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        PrintThickLine(ticketWidth);
        PrintTicketHeader(ticketWidth);
        PrintThickLine(ticketWidth);
        WriteLineLeftPadding($"Ticket: {ticketCode}", paddingLeft);
        WriteLineLeftPadding($"Datum: {date}", paddingLeft);
        PrintThinLine(ticketWidth);
        WriteLineLeftPadding("(leeg)", paddingLeft);
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
    public static void PrintUserInstructions() {
        ForegroundColor = ConsoleColor.DarkGray;
        WriteLine("<scan barcode> of [barcode]<Enter> | [aantal extra]<Enter>\n" +
            "[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste\n" +
            "[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash\n" +
            "[P]<Enter> = parkeren | [H]<Enter> = hervatten | [A]<Enter> = afbreken\n");
        ResetColor();
    }
}
 