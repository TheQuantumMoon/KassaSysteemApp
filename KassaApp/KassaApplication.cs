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
                    _kassa.FinishTicketCard(currentKassaTicket, result);
                    currentKassaTicket = _kassa.GetLastTicket();
                    break;

                // Betalen met cash
                case "C":
                    if (!currentKassaTicket.HasScannedProducts) {
                        WriteInColor("  Er zijn nog geen ingescande items", ConsoleColor.Red);
                        continue;
                    }
                    DisplayTicket(currentKassaTicket, TicketSoort.Cash);
                    _kassa.FinishTicketCash(currentKassaTicket);
                    currentKassaTicket = _kassa.GetLastTicket();
                    break;

                // Ticket parkeren
                case "P":
                    currentKassaTicket = _kassa.ParkKassaTicket(currentKassaTicket);
                    break;

                // Ticket hervatten uit lijst van tickets
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
                    currentKassaTicket = _kassa.ResumeTicketByIndex(choice - 1);
                    break;

                // Huidig ticket annuleren (en loggen)
                case "A":
                    _kassa.RemoveTicket(currentKassaTicket);
                    currentKassaTicket = _kassa.GetLastTicket();
                    break;

                // Fallback voor onverwachte inputs
                default:
                    WriteLineInColor("  Input niet herkend", ConsoleColor.Red);
                    break;
            }
        }
    }

    public void DisplayTicket(KassaTicket ticket, TicketSoort soort = TicketSoort.Normaal, BetaalDetails? betaalDetails = null) {
        int ticketWidth = 42;
        int paddingLeft = 2;

        WriteLine(ticket.ToStringLayout(soort, betaalDetails, ticketWidth, paddingLeft));
        if (soort == TicketSoort.Normaal) {
            PrintUserInstructions();
        } else if (soort == TicketSoort.Cash) {
            WriteInColorLeftPadding("✓ Betaling ontvangen - €" + ticket.TotalPrice, ConsoleColor.Green, paddingLeft);
            WriteLine();
        } else if (soort == TicketSoort.Kaart) {
            WriteInColorLeftPadding("✓ Betaling ontvangen - €" + betaalDetails!.Bedrag, ConsoleColor.Green, paddingLeft);
            WriteLine();
        }
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

    public void PrintUserInstructions() {
        int amountOfTickets = _kassa.TicketCount;
        if (amountOfTickets > 1) WriteLineInColor($"[{amountOfTickets - 1} geparkeerd]", ConsoleColor.Cyan);
        WriteLineInColor("<scan barcode> of [barcode]<Enter> | [aantal extra]<Enter>\n" +
            "[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste\n" +
            "[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash\n" +
            "[P]<Enter> = parkeren | [H]<Enter> = hervatten | [A]<Enter> = afbreken", ConsoleColor.DarkGray);
    }
    public static void PrintPaymentByCardPrompt(KassaTicket kassaTicket) {
        string priceInfo = $"Bedrag: €   {kassaTicket.TotalPrice}";
        WriteLine("  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
        WriteLine("  ┃       BETAALTERMINAL        ┃");
        WriteLine($"  ┃   {priceInfo,-26}┃");
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
