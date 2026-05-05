using BetaalSysteemMock;
using KassaApp;
using WinkelDomein.Interface;
using WinkelDomein.Model;

namespace Startup {
    internal class Program {
        static void Main() {

            IBetaalTerminal snelleTerminal = new MockBetaalTerminal(50, 200);
            Kassa kassa = new(snelleTerminal);
            KassaApplication _ = new(kassa);

            Console.ReadKey();
        }
    }
}
