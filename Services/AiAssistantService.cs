namespace eGradjaninFBiH.Services;
public record AiAnswer(string Text, string Source, bool IsFallback=true);
public class AiAssistantService
{
    private readonly (string[] Terms,string Answer,string Source)[] _knowledge =
    [
        (new[]{"rodni list","rođen"},"U demo verziji otvorite Digitalni šalter, izaberite ‘Izvod iz matične knjige rođenih’, popunite obrazac i priložite traženi dokument. Zahtjev zatim možete pratiti u rubrici ‘Moji zahtjevi’. Stvarni uslovi, takse i rokovi zavise od nadležnog organa.","Demo baza znanja: uputstvo za Digitalni šalter"),
        (new[]{"državljanstvo"},"Izaberite uslugu ‘Uvjerenje o državljanstvu’ u Digitalnom šalteru. Provjerite listu potrebnih priloga prikazanu uz uslugu. Ovaj demo ne provjerava službene registre.","Demo baza znanja: katalog usluga"),
        (new[]{"prebivalište","prebivaliste"},"U katalogu usluga odaberite ‘Uvjerenje o prebivalištu’. Uslovi i nadležnost moraju se potvrditi kod stvarnog nadležnog organa.","Demo baza znanja: katalog usluga"),
        (new[]{"status","pratiti","zahtjev"},"Nakon prijave otvorite ‘Moji zahtjevi’. Tamo se prikazuju referentni broj, datum podnošenja, status i poruke službenika.","Demo baza znanja: praćenje zahtjeva"),
        (new[]{"plaćanje","placanje","taksa","uplata"},"U ovoj demonstraciji plaćanje je simulirano. Pritisak na ‘Simuliraj uplatu’ evidentira demo transakciju; ne tereti bankovnu karticu i nije stvarna uplata javne takse.","Demo baza znanja: simulirano plaćanje"),
        (new[]{"qr","verifikacija","provjera"},"QR kod na demo dokumentu vodi na javnu stranicu provjere. Stranica prikazuje samo osnovne podatke o demo dokumentu i ne potvrđuje pravnu valjanost javne isprave.","Demo baza znanja: QR provjera")
    ];
    public AiAnswer Ask(string question)
    {
        var q=question.ToLowerInvariant();
        var match=_knowledge.FirstOrDefault(x=>x.Terms.Any(q.Contains));
        return match.Answer is not null ? new(match.Answer,match.Source) : new("Nemam dovoljno pouzdanu informaciju u odobrenoj demo bazi znanja. Molimo obratite se nadležnoj općinskoj/kantonalnoj službi. AI asistent je informativan i ne donosi službene odluke.","Nema pouzdanog izvora – upućivanje službeniku");
    }
}
