using WinkelDomein.Enums;

namespace WinkelDomein.Model {
    public class Korting {
        private readonly ProductCategorie _category;
        private readonly DateOnly _startDate;
        private readonly DateOnly _endDate;

        public Korting(ProductCategorie category, DateOnly startDate, DateOnly endDate) {
            Category = category;
            StartDate = startDate;
            EndDate = endDate;
            if (StartDate >= EndDate) throw new Exception(message: "Startdatum mag niet na einddatum komen");
        }

        public ProductCategorie Category { 
            get => _category; 
            init => _category = value; 
        }
        public DateOnly StartDate { 
            get => _startDate;
            init => _startDate = value; 
        }
        public DateOnly EndDate { 
            get => _endDate;
            init => _endDate = value; 
        }
    }
}
