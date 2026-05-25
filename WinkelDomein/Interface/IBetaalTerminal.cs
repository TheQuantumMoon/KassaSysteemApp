using WinkelDomein.Model;

namespace WinkelDomein.Interface;

public interface IBetaalTerminal {
    BetaalDetails? VerzoekBetaling(decimal bedrag, string boodschap);
}
