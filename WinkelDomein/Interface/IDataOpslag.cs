using System;
using System.Collections.Generic;
using System.Text;
using WinkelDomein.Model;

namespace WinkelDomein.Interface {
    public interface IDataOpslag {
        List<KassaTicket> ParseParkedTickets(List<Product> possibleProducts);
        List<Product> ParsePossibleProducts(List<Korting> reductions);
        List<Korting> ParseReductions();
        void RemoveStoredTicket(KassaTicket ticket);
        void StoreTicket(KassaTicket ticket);
    }
}
