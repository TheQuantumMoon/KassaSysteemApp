using WinkelDomein;
using WinkelDomein.Enum;
using WinkelDomein.Model;
using static System.Console;

namespace KassaApp;

public class KassaApplication {
    private readonly Kassa _kassa;

    public KassaApplication(Kassa kassa) {
        _kassa = kassa;
        Start();
    }

    // Start de applicatie
    public void Start() {
        while (true) {
            DisplayTicket();
            Write("> ");
            string input = AskInput().Trim().ToUpper();

            // Check of de input een productcode is, zoja, voeg het product toe aan het ticket
            Product? product = _kassa.GetProductByCode(input);
            if (product != null) {
                _kassa.IncreaseProduct(product);
                continue;
            }

            // Check of de input een int is, zoja pas het aantal van het laast ingegeven product aan
            bool isInt = int.TryParse(input, out int amount);
            if (isInt) {
                try { _kassa.IncreaseLastProduct(amount); } catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                continue;
            }

            // Check of de input een van de optie letters is
            switch (input) {
                // Verwijderen
                case "D":
                    Write("Barcode: ");
                    input = AskInput().Trim();
                    product = _kassa.GetProductByCode(input);
                    try { _kassa.DiminishProduct(product); } catch (ArgumentException ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                    break;

                // Undo de laatste scan, toevoeging van aantal of verwijdering van een product
                case "Z":
                    try { _kassa.UndoLastProductAmountChange(); } catch (Exception ex) { WriteLineInColor(ex.Message, ConsoleColor.Red); }
                    break;

                // Betalen met kaart
                case "K":
                    PrintPaymentByCardPrompt();
                    BetaalDetails? result = _kassa.VerzoekBetaling("Betaling met de kaart");
                    if (result == null) {
                        WriteLineInColor("  X Betaling mislukt", ConsoleColor.Red);
                        continue;
                    }
                    DisplayTicket(TicketSoort.Kaart, result);
                    _kassa.FinishTicketCard(result);
                    break;

                // Betalen met cash
                case "C":
                    if (!_kassa.CurrentTicket.HasScannedProducts) {
                        WriteInColor("Er zijn nog geen ingescande items", ConsoleColor.Red);
                        continue;
                    }
                    DisplayTicket(TicketSoort.Cash);
                    _kassa.FinishTicketCash();
                    break;

                // Ticket parkeren
                case "P":
                    _kassa.ParkTicket();
                    break;

                // Ticket hervatten uit lijst van tickets
                case "H":
                    List<string> ticketsString = _kassa.GetTicketListString();
                    WriteLine("  Gepakeerde tickets:");
                    for (int i = 0; i < ticketsString.Count; i++) WriteLine($"    {i + 1}. {ticketsString[i]}");
                    Write("Keuze: ");
                    input = AskInput().Trim();
                    isInt = int.TryParse(input, out int choice);
                    if (!isInt || choice < 1 || choice > ticketsString.Count) {
                        WriteLineInColor("Verkeerde input", ConsoleColor.Red);
                        continue;
                    }
                    _kassa.ResumeTicketByIndex(choice - 1);
                    break;

                // Huidig ticket annuleren (en loggen)
                case "A":
                    _kassa.RemoveTicket();
                    break;

                // Fallback voor onverwachte inputs
                default:
                    WriteLineInColor("  Input niet herkend", ConsoleColor.Red);
                    break;
            }
        }
    }

    // Print een stringlayout van een kassaticket op de console. Aan de hand van de ticketsoort en de betaaldetails wordt er extra stuk toegevoegd met betaalinformatie
    public void DisplayTicket(TicketSoort soort = TicketSoort.Normaal, BetaalDetails? betaalDetails = null) {
        KassaTicket ticket = _kassa.CurrentTicket;
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

    // Print een string op de console met aan de linkerkant een megegeven aantal spaties
    public static void WriteLineLeftPadding(string text, int padding) {
        int calculatedPadding = text.Length + padding;
        WriteLine(text.PadLeft(calculatedPadding));
    }
    // Print een string op de console met als forgroundcolor een opgegeven consolecolor
    public static void WriteInColor(string text, ConsoleColor color) {
        ForegroundColor = color;
        Write(text);
        ResetColor();
    }
    // Combineert de methode WriteLineLeftPadding en WriteInColor
    public static void WriteInColorLeftPadding(string text, ConsoleColor color, int padding) {
        ForegroundColor = color;
        WriteLineLeftPadding(text, padding);
        ResetColor();
    }
    // Print een string en een enter op de console als forgroundcolor een opgegeven consolecolor
    public static void WriteLineInColor(string text, ConsoleColor color) {
        ForegroundColor = color;
        WriteLine(text);
        ResetColor();
    }

    // Print een blok tekst op de console die de gebruiker uitlegt welke inputs hij kan geven
    public void PrintUserInstructions(int padding = 2) {
        string p = new(' ', padding);
        int amountOfTickets = _kassa.TicketCount;
        if (amountOfTickets > 1) WriteLineInColor($"{p}[{amountOfTickets - 1} geparkeerd]", ConsoleColor.Cyan);
        WriteLineInColor("  <scan barcode> of [barcode]<Enter> | [aantal extra]<Enter>\n" +
            $"{p}[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste\n" +
            $"{p}[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash\n" +
            $"{p}[P]<Enter> = parkeren | [H]<Enter> = hervatten | [A]<Enter> = afbreken", ConsoleColor.DarkGray);
    }
    // Print een blok tekst die de betaalpromt voor kaart moet voorstellen
    public void PrintPaymentByCardPrompt() {
        KassaTicket ticket = _kassa.CurrentTicket;
        string priceInfo = $"Bedrag: €   {ticket.TotalPrice}";
        WriteLine("  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
        WriteLine("  ┃       BETAALTERMINAL        ┃");
        WriteLine($"  ┃   {priceInfo,-26}┃");
        WriteLine("  ┃   Bied uw kaart aan...      ┃");
        WriteLine("  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");

    }

    // Een gestylde wrapper voor Console.ReadLine met input karakter en gekleurde input
    public static string AskInput() {
        ForegroundColor = ConsoleColor.Yellow;
        string input = ReadLine()!;
        ResetColor();
        return input;
    }
}
