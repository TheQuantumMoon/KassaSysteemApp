using WinkelDomein;
using BetaalSysteemMock;
using static System.Console;

namespace KassaApp;

public class Program
{
    public static void Main(string[] args)
    {
        IBetaalTerminal snelleTerminal = new MockBetaalTerminal(50, 200);
        //var kassa = new Kassa(..., terminal, ...);

        DisplayTicket();
        ReadKey();
    }

    public static void DisplayTicket(int ticketWidth = 42, int paddingLeft = 2) {
        
        string ticketCode = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        WriteThickLine(ticketWidth);
        PrintTicketHeader(ticketWidth);
        WriteThickLine(ticketWidth);
        WriteLineLeftPadding($"Ticket: {ticketCode}", paddingLeft);
        WriteLineLeftPadding($"Datum: {date}", paddingLeft);
        WriteThinLine(ticketWidth);
        WriteLineLeftPadding("(leeg)", paddingLeft);
        WriteThickLine(ticketWidth);
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
    public static void WriteThinLine(int length) {
        string thinLine = new('-', length);
        WriteLine(thinLine);
    }
    public static void WriteThickLine(int length) {
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
 