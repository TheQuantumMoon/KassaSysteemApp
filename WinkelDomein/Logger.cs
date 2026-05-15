using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using WinkelDomein.Enum;
using WinkelDomein.Model;

namespace WinkelDomein {
    public static class Logger {
        private static readonly string _logFilePath = @"Log.txt";
        private static readonly string _savedTicketsFolderPath = @"SavedTickets";

        private static string Now => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ff}]";

        public static void StartLogger() {
            if (!File.Exists(_logFilePath)) File.Create(_logFilePath);
            else File.WriteAllText(_logFilePath, "");
            SystemLog("KassaSysteem opgestart.");
        }

        public static void SystemLog(string text) {
            string message = $"{Now} [SYSTEEM] {text}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void SaveTicket(KassaTicket kassaTicket, TicketSoort soort = TicketSoort.Normaal, BetaalDetails betaalDetails = default) {
            string fileName = Path.Combine(_savedTicketsFolderPath, $"kassaticket-{kassaTicket.TicketCode}.txt");
            File.WriteAllText(fileName, kassaTicket.ToStringLayout(soort, betaalDetails));
            string message = $"{Now} [SYSTEEM] Ticket opgeslagen {kassaTicket.TicketCode}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogNewTicket(KassaTicket kassaTicket) {
            string message = $"{Now} [KASSA] NIEUW TICKET {kassaTicket.TicketCode}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogParkTicket(KassaTicket kassaTicket) {
            string message = $"{Now} [KASSA] PARKEREN ticket {kassaTicket.TicketCode}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogResumeTicket(KassaTicket kassaTicket) {
            string message = $"{Now} [KASSA] HERVATTEN ticket {kassaTicket.TicketCode}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogPaidTicketCard(KassaTicket kassaTicket, BetaalDetails betaalDetails) {
            string message = $"{Now} [KASSA] BETALING ticket {kassaTicket.TicketCode}: {betaalDetails.Bedrag} ({betaalDetails.Methode})";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogPaidTicketCash(KassaTicket kassaTicket) {
            string message = $"{Now} [KASSA] BETALING ticket {kassaTicket.TicketCode}: {kassaTicket.TotalPrice} (Cash)";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogCancelTicket(KassaTicket kassaTicket) {
            string message = $"{Now} [KASSA] ANNULERING ticket {kassaTicket.TicketCode}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogScanProduct(KassaTicket kassaTicket, Product product, int amount) {
            string message = $"{Now} [KASSA] SCAN {amount}x {product} op ticket {kassaTicket.TicketCode}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }

        public static void LogRemoveProduct(KassaTicket kassaTicket, Product product, int amount) {
            string message = $"{Now} [KASSA] VERWIJDER {amount}x {product} op ticket {kassaTicket.TicketCode}";
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }
    }
}
