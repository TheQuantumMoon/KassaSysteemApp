using WinkelDomein.Interface;

namespace WinkelDomein.Model {
    public class Kassa {
        private readonly IBetaalTerminal _betaalTerminal;

        public Kassa(IBetaalTerminal betaalTerminal) {
            _betaalTerminal = betaalTerminal;
        }


    }
}
