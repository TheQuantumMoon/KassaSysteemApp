using BetaalSysteemMock;
using KassaApp;
using System.Text;
using WinkelDomein;
using WinkelDomein.Interface;

namespace Startup {
    internal class Program {
        static void Main() {

            Console.OutputEncoding = Encoding.UTF8;

            IBetaalTerminal snelleTerminal = new MockBetaalTerminal(50, 200);
            Kassa kassa = new(snelleTerminal);
            KassaApplication _ = new(kassa);

            Console.ReadKey();
        }
    }
}
