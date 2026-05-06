using System;
using System.Collections.Generic;
using System.Text;

namespace WinkelDomein.Model {
    public class KassaTicket {

        private readonly List<GescandProduct> _products = [];

        public List<GescandProduct> Products {
            get => _products;
        }
        public string TicketCode {
            get => DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
        }
        public string Date {
            get =>  DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public void AddProduct(GescandProduct gescandProduct) {
            _products.Add(gescandProduct);
        }
        public void RemoveProduct(GescandProduct gescandProduct) {
            _products.Remove(gescandProduct);
        }
        public void IncreaseAmountOfProduct(GescandProduct gescandProduct, int amount) {
            gescandProduct.Quantity += amount;
        }
        public void DiminishAmountOfProduct(GescandProduct gescandProduct, int amount) {
            gescandProduct.Quantity -= amount;
        }

    }
}
