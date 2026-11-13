# eGrađanin FBiH — demonstraciona prva verzija

ASP.NET Core MVC aplikacija za demonstraciju digitalnog šaltera, upravljanja zahtjevima, simuliranog plaćanja, digitalnog arhiva, QR provjere i informativnog AI asistenta.

> **Važno:** Ovo je prototip za razvoj i prezentaciju. Nije zvanični servis organa vlasti. Ne pristupa IDDEEA-i, CIPS-u, matičnim knjigama ili drugim državnim registrima. Uplate su simulirane. PDF dokumenti su demonstracioni i nisu javne isprave; nemaju kvalifikovani elektronski potpis/pečat. Ne unosite stvarne JMBG-ove, lične dokumente, bankovne podatke ili druge osjetljive podatke.

## Tehnologije

- .NET 10 / ASP.NET Core MVC / Razor Views
- ASP.NET Core Identity i role-based authorization
- Entity Framework Core
- SQLite kao najbrži lokalni način pokretanja; PostgreSQL kroz Docker Compose
- QuestPDF za demo PDF; QRCoder za QR kod
- Bootstrap 5 i prilagođeni CSS
- Monolitna aplikacija sa odvojenim kontrolerima, servisima i modelima

## Brzo pokretanje — Windows

Potrebno: .NET 10 SDK.

1. Raspakujte projekat i otvorite folder `eGradjaninFBiH` u terminalu.
2. Pokrenite:

```powershell
dotnet restore
dotnet run
```

3. Otvorite URL koji terminal ispiše (obično `https://localhost:7xxx` ili `http://localhost:5xxx`).
4. SQLite baza `egradjanin.db` automatski se kreira pri prvom pokretanju, kao i demo korisnici i katalog usluga.

## Pokretanje sa PostgreSQL-om

Potrebni su Docker i Docker Compose:

```bash
docker compose up --build
```

Aplikacija će biti dostupna na `http://localhost:8080`. Za stvarno okruženje obavezno promijenite `POSTGRES_PASSWORD`, postavite `ASPNETCORE_ENVIRONMENT=Production`, koristite HTTPS reverse proxy, tajne izvan repozitorija i migracije baze.

## Demo računi

| Uloga | E-mail | Lozinka |
|---|---|---|
| Građanin | `gradjanin@egradjanin.demo` | `Gradjanin123!` |
| Šalterski radnik | `sluzbenik@egradjanin.demo` | `Sluzbenik123!` |
| Administrator | `admin@egradjanin.demo` | `Admin123!` |

Registracijom novog računa automatski se dodjeljuje uloga `Citizen`.

### Početna stranica i usmjeravanje po ulozi

- Neprijavljeni posjetilac koji otvori početnu adresu aplikacije preusmjerava se na ekran prijave/registracije.
- Nakon uspješne prijave, uloga se provjerava na serveru: `Citizen` ide na građanski panel, `Registrar` na šalter, a `Administrator` na administrativni panel.
- Novi nalozi kreirani kroz javnu registraciju uvijek dobijaju samo ulogu `Citizen`.
- Uloge službenika i administratora dodjeljuje administrator kroz upravljanje korisnicima ili se koriste unaprijed kreirani demo računi.
- Kontroleri panela imaju `[Authorize(Roles=...)]` zaštitu; skrivanje linkova u navigaciji nije jedina sigurnosna mjera.

## Šta radi ova verzija

### Građanin
- Registracija, prijava i odjava; Identity čuva hash lozinke.
- Katalog demo usluga: rodni list, državljanstvo, prebivalište i ostale potvrde.
- Podnošenje zahtjeva uz validaciju i opcionalni PDF/JPG/PNG prilog do 10 MB.
- Privatno čuvanje priloga izvan `wwwroot` i autorizovano preuzimanje.
- Lista vlastitih zahtjeva, detalji, status, poruke i obavještenja.
- Simulacija uplate (bez kartičnog procesiranja i bez stvarnog novčanog transfera).
- Preuzimanje demo PDF-a i QR provjera.

### Šalterski radnik
- Pregled i filtriranje zahtjeva.
- Pregled podataka i preuzimanje priloga.
- Kontrolisane promjene statusa: zaprimljen → obrada/dopuna/odbijen; obrada → odobren/odbijen/dopuna; dopuna → obrada/odbijen; odobren → završen.
- Poruka građaninu i automatsko demo obavještenje.
- Odobravanje kreira demo dokument i jedinstveni QR token. AI ne donosi odluke.

### Administrator
- Osnovne statistike.
- Pregled korisnika i promjena uloga.
- Aktiviranje/deaktiviranje kataloga usluga.
- Pregled revizijskog traga.

### AI i dokumenti
- AI asistent koristi malu, kontrolisanu lokalnu FAQ bazu; kada nema pouzdan odgovor, upućuje korisnika službeniku.
- Nije povezan s vanjskim LLM servisom i ne prima korisničke priloge.
- PDF jasno nosi oznaku demo primjerka; QR stranica potvrđuje samo postojanje demo zapisa.

## Prvi end-to-end demo scenarij

1. Prijavite se kao građanin.
2. Izaberite „Izvod iz matične knjige rođenih“ i pošaljite zahtjev.
3. Ako postoji demo naknada, simulirajte uplatu.
4. Odjavite se i prijavite kao šalterski radnik.
5. Otvorite predmet, prebacite ga u „U obradi“, a zatim „Odobren“.
6. Odjavite se i vratite kao građanin; otvorite zahtjev i preuzmite PDF.
7. Skenirajte QR kod ili otvorite URL iz QR-a; vidjet ćete demo verifikaciju.
8. Prijavite se kao administrator i pregledajte statistiku i revizijski trag.

## Sigurnosne i tehničke napomene prije javnog hostinga

Ova verzija je razvojni prototip, ne produkcijski sistem. Prije javnog hostinga potrebno je najmanje:

- zamijeniti `EnsureCreated` migracijama (`dotnet ef migrations add InitialCreate`, zatim `dotnet ef database update`); za to dodati `Microsoft.EntityFrameworkCore.Design` i alat `dotnet-ef`;
- dodati rate limiting, CSRF/security headers, antivirusnu provjeru uploadova, strožiju MIME/content validaciju, log redaction i centralizovano upravljanje tajnama;
- dodati testove (unit, integration, authorization, upload, status transition) i CI pipeline;
- dodati formalnu politiku zadržavanja/brisanja podataka, backup/restore i monitoring;
- pregledati pravne osnove, DPIA/procjenu uticaja i ugovore sa dobavljačima prije bilo kakve obrade stvarnih ličnih podataka;
- integrisati e-ID/2FA, službene registre, e-potpis/e-pečat i stvarni payment gateway samo uz odobrenje i zvanične API-je nadležnih institucija;
- koristiti produkcijski storage za priloge, antivirus, enkripciju i najmanje privilegije.

## Poznate granice prve verzije

- Nema državnog e-ID identiteta ni verifikacije punoljetnosti.
- Nema zvanične interoperabilnosti, matičnih evidencija, CIPS/IDDEEA integracije ni stvarnog izdavanja dokumenata.
- Nema kvalifikovanog potpisa/pečata niti pravno validne elektronske isprave.
- Nema stvarnog kartičnog procesiranja ili e-uplatnice.
- Nema produkcijskog OCR-a, RAG-a, predikcije rokova ni glasovnog asistenta; te funkcije su faze kasnijeg razvoja.
- Prilozi se čuvaju na lokalnom disku u `App_Data/uploads`; za višestruke instance potreban je centralni privatni storage.

## Struktura

```text
Controllers/   MVC kontroleri za račune, zahtjeve, šalter, admin, AI i dokumente
Data/          EF Core DbContext
Models/        Entiteti i statusi
Services/      Workflow, audit, PDF/QR, AI FAQ
ViewModels/    Validirani ulazni modeli
Views/         Razor stranice po modulu
wwwroot/       CSS i JavaScript
App_Data/      Privatni uploadovi i SQLite baza (nastaju pri radu)
```
