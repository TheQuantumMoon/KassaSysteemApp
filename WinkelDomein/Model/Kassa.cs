using WinkelDomein.Interface;

namespace WinkelDomein.Model {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;
        private readonly List<Product> _products = [
            new("5000357103749", "Ricola Lemon Mint", 0.99m),
            new("7610700947388", "Fisherman's Friend Original", 2.99m),
            new("5449000000996", "Coca-Cola Classic 330ml", 1.25m),
            new("5410013108004", "Spa Blauw 500ml", 0.85m),
            new("8710398157058", "Lay's Naturel Chips", 1.99m),
            new("8711327370558", "Unox Stevige Erwtensoep", 2.49m),
            new("8718452119234", "AH Halfvolle Melk 1L", 1.15m),
            new("8710400015841", "Douwe Egberts Aroma Rood 500g", 6.49m),
            new("5410126706937", "Lotus Biscoff Speculoos", 1.79m),
            new("7613034926838", "Maggi Basis voor Tomatensoep", 0.89m),
            new("8712100325946", "Knorr Wereldgerechten Burritos", 3.19m),
            new("8710447821320", "Calvé Pindakaas 350g", 3.45m),
            new("5410076452205", "Côte d'Or Reep Melk", 1.10m),
            new("8715700110113", "Heinz Tomato Ketchup", 2.89m),
            new("8710403031022", "Pickwick English Blend 20st", 1.65m),
            new("4000517004246", "Haribo Goudberen", 1.85m),
            new("8710847926190", "Hertog Jan Pilsener 6-pack", 6.99m),
            new("8712800512196", "Blue Band Goede Start", 2.15m),
            new("3068320113594", "Evian Mineraalwater 1.5L", 1.39m),
            new("8714100613013", "Dove Deeply Nourishing Douchegel", 3.95m)
        ];

        public Kassa(IBetaalTerminal betaalTerminal) {
            _betaalTerminal = betaalTerminal;
        }

        public List<Product> Products { get => _products; }
    }
}
