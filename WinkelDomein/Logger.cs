using WinkelDomein.Enums;
using WinkelDomein.Model;

namespace WinkelDomein {
    public static class Logger {
        private const string LOGFILEPATH = @"Log.txt";
        private const string SAVEDTICKETSPATH = @"SavedTickets";
        private const string SYSTEMTAG = "[SYSTEEM]";
        private const string REGISTERTAG = "[KASSA]";
        private static string Now => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ff}]";

        // Check bij het opstarten of er al een logfile bestaat, zowel maak hem leeg, zoniet, maak hem aan
        public static void StartLogger() {
            File.Create(LOGFILEPATH).Dispose();
            GeneralLog("KassaSysteem opgestart");
        }

        // Voor het loggen van algemene variabele systeemboodschappen
        public static void GeneralLog(string text) {
            string message = $"{Now} {SYSTEMTAG} {text}";
            AddLog(message);
        }

        // Voor het wegschrijven van kassatickets (stringlayout) naar apparte tekstfiles op de opgegeven savedticketspath
        public static void SaveTicket(KassaTicket kassaTicket, TicketSoort soort = TicketSoort.Normaal, BetaalDetails? betaalDetails = null) {
            string fileName = Path.Combine(SAVEDTICKETSPATH, $"kassaticket-{kassaTicket.TicketCode}.txt");
            File.WriteAllText(fileName, kassaTicket.ToStringLayout(soort, betaalDetails));
            string message = $"{Now} {SYSTEMTAG} Ticket opgeslagen {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Voor het loggen van een niet-kritieke fout
        public static void LogError(string text) {
            string message = $"{Now} {SYSTEMTAG} ERROR! {text}";
            AddLog(message);
        }

        // Voor het loggen van de creatie van een nieuw kassaticket
        public static void LogNewTicket(KassaTicket kassaTicket) {
            string message = $"{Now} {REGISTERTAG} NIEUW TICKET {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Voor het loggen van een geparkeerd kassaticket
        public static void LogParkTicket(KassaTicket kassaTicket) {
            string message = $"{Now} {REGISTERTAG} PARKEREN ticket {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Voor het loggen van een geparkeerd kassaticket dat hervat wordt
        public static void LogResumeTicket(KassaTicket kassaTicket) {
            string message = $"{Now} {REGISTERTAG} HERVATTEN ticket {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Voor het loggen van een kassaticket dat met de kaart is betaald
        public static void LogPaidTicketCard(KassaTicket kassaTicket, BetaalDetails betaalDetails) {
            string message = $"{Now} {REGISTERTAG} BETALING ticket {kassaTicket.TicketCode}: €{betaalDetails.Bedrag} ({betaalDetails.Methode})";
            AddLog(message);
        }

        // Voor het loggen van een kassaticket dat met cash is betaald
        public static void LogPaidTicketCash(KassaTicket kassaTicket) {
            string message = $"{Now} {REGISTERTAG} BETALING ticket {kassaTicket.TicketCode}: €{kassaTicket.TotalPrice} (Cash)";
            AddLog(message);
        }

        // Voor het loggen van een gecanceld kassaticket
        public static void LogCancelTicket(KassaTicket kassaTicket) {
            string message = $"{Now} {REGISTERTAG} ANNULERING ticket {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Voor het loggen van een product dat gescand wordt en wordt toegevoegd aan een kassaticket
        public static void LogScanProduct(KassaTicket kassaTicket, Product product, int amount) {
            string message = $"{Now} {REGISTERTAG} SCAN {amount}x {product} op ticket {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Voor het loggen van een product en aantal dat wordt verwijderd van een kassaticket
        public static void LogRemoveProduct(KassaTicket kassaTicket, Product product, int amount) {
            string message = $"{Now} {REGISTERTAG} VERWIJDER {amount}x {product} op ticket {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Voor het loggen van een undo action
        public static void LogUndo(KassaTicket kassaTicket) {
            string message = $"{Now} {REGISTERTAG} UNDO laatste actie op ticket {kassaTicket.TicketCode}";
            AddLog(message);
        }

        // Schrijf een string weg naar de logfile
        private static void AddLog(string message) {
            File.AppendAllText(LOGFILEPATH, message + Environment.NewLine);
        }
    }
}
