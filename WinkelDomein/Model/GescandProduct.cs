using System;
using System.Collections.Generic;
using System.Text;

namespace WinkelDomein.Model {
    public class GescandProduct {

        private int _quantity;
        private readonly Product _product;

        public GescandProduct(Product product, int quantity) {
            Product = product;
            Quantity = quantity;
        }

        public Product Product { 
            get => _product; 
            init => _product = value; 
        }
        public int Quantity { 
            get => _quantity; 
            set {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                _quantity = value;
            } 
        }
    }
}
