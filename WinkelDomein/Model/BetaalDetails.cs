namespace WinkelDomein.Model;

public class BetaalDetails(string kaartType, string kaartVariant, string methode,
    string gemaskerdKaartnummer, string transactieReferentie, decimal bedrag) {

    public string KaartType { get; } = kaartType;
    public string KaartVariant { get; } = kaartVariant;
    public string Methode { get; } = methode;
    public string GemaskerdKaartnummer { get; } = gemaskerdKaartnummer;
    public string TransactieReferentie { get; } = transactieReferentie;
    public decimal Bedrag { get; } = bedrag;
    public DateTime Tijdstip { get; } = DateTime.Now;
}
