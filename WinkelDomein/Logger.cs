using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using WinkelDomein.Model;

namespace WinkelDomein {
    public static class Logger {
        private static readonly string _logFilePath = @"Log.txt";

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

        public static void LogNewTicket(KassaTicket kassaTicket) {
            string message = $"{Now} [KASSA] NIEUW TICKET {kassaTicket.TicketCode}";
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
