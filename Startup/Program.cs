using BetaalSysteemMock;
using DataOpslag;
using KassaApp;
using System.Text;
using WinkelDomein;
using WinkelDomein.Interface;

namespace Startup {
    internal class Program {
        static void Main() {

            Console.OutputEncoding = Encoding.UTF8;
            Logger.StartLogger();

            IDataOpslag repo = new DataRepo();
            IBetaalTerminal snelleTerminal = new MockBetaalTerminal(500, 2000);
            Kassa kassa = new(snelleTerminal, repo);
            KassaApplication _ = new(kassa);

        }
    }
}
