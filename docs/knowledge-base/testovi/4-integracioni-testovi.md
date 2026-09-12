# Integracioni testovi

Prethodna lekcija je zaključila da aplikacioni servis, repozitorijum i API kontroler dobijaju mali broj integracionih testova koji prolaze kroz sve tri klase odjednom. Ostaje pitanje kako takav test izgleda, šta u njemu zaista radi, a šta je zamenjeno, i koje scenarije pokriva. Ova lekcija odgovara na ta tri pitanja i opisuje pomoćni kod koji platformski tim održava da bi integracioni test modula bio kratak.

## Šta je integracioni test

**Integracioni test** (engl. *integration test*) je automatski test koji ne ispunjava bar jedan uslov jediničnog testa: proverava više jedinica ponašanja odjednom, nije brz ili nije nezavisan od drugih testova. U projektu integracioni test šalje HTTP zahtev pokrenutoj aplikaciji i proverava odgovor i stanje baze podataka.

Jedinični test dokazuje pravilo agregata u izolaciji. Ništa ne dokazuje da je agregat učitan iz baze, da je izmena sačuvana i da je krajnja tačka izložila ispravan odgovor. Integracioni test prolazi kroz API kontroler, aplikacioni servis, agregat, EF Core i bazu, pa hvata greške na svakom spoju. Istovremeno je udaljen od koda, jer vidi samo HTTP zahtev i red u bazi, pa preživljava refaktorisanje bilo koje od tih klasa. Sledeći kod prikazuje test iz klase `TourAuthoringCommandTests` istog imena kao test iz prve lekcije:

```cs
[Fact]
public async Task Publish_publishes_a_complete_tour()
{
    var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

    var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    using var assertContext = Factory.CreateContext<ExplorationDbContext>();
    var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
    stored.Status.Should().Be(TourStatus.Published);
    stored.PublishedAt.Should().NotBeNull();
}
```

U datom kodu treba uočiti sledeće:

- Test ima ista tri dela kao jedinični. Priprema pravi klijenta prijavljenog kao istraživač, akcija šalje jedan `POST` zahtev, provera čita odgovor i bazu.
- Akcija ne poziva metodu, nego krajnju tačku. Zahtev prolazi kroz istu obradu kao zahtev iz klijentske aplikacije, uključujući proveru tokena i pretvaranje izuzetka u statusni kod.
- Provera prvo gleda statusni kod, a zatim otvara kontekst i čita red ture iz baze. Odgovor `204 No Content` ne dokazuje da je izmena sačuvana, a red u bazi dokazuje.
- Tura `PublishableRiverside` je unapred upisana u bazu sa dovoljno dugim opisom i vremenom transporta. Odakle dolazi i kako se baza vraća u isto stanje pre svakog testa razmatra naredna lekcija.

## Upravljane i neupravljane zavisnosti

**Zavisnost van procesa** (engl. *out-of-process dependency*) je sistem sa kojim aplikacija komunicira, a koji nije deo njenog procesa. **Upravljana zavisnost** (engl. *managed dependency*) je zavisnost van procesa kojoj pristupa samo naša aplikacija, poput baze podataka. **Neupravljana zavisnost** (engl. *unmanaged dependency*) je zavisnost van procesa koju vide i drugi sistemi, poput servisa za slanje pošte ili platnog provajdera.

Razlika određuje šta u integracionom testu zaista radi. Komunikacija sa upravljanom zavisnošću je detalj implementacije, jer niko van aplikacije ne zna kako su tabele organizovane, pa se one mogu promeniti bez posledica po druge sisteme. Zato test koristi pravu instancu i proverava krajnje stanje u njoj. Komunikacija sa neupravljanom zavisnošću je vidljiva spolja, jer su poslata poruka ili naplaćen iznos posledica koju drugi sistem vidi. Zato se takva zavisnost u testu zamenjuje objektom koji beleži pozive, a test proverava da je poziv upućen.

Svaki modul projekta danas ima jednu zavisnost van procesa, bazu podataka, i ona je upravljana. Testovi zato rade sa pravim PostgreSQL serverom. Zamena pravog servera bazom u memoriji bi test učinila bržim, ali bi test dokazivao ponašanje sistema koji aplikacija ne koristi. Modul Payment će dobiti prvu neupravljanu zavisnost, platnog provajdera, i tada će mu biti potreban objekat koji provajdera zamenjuje u testu.

## Šta integracioni test pokriva

Integracioni test je sporiji i duži od jediničnog, pa se ne piše za svako pravilo agregata. Pravila su već pokrivena jediničnim testovima. Integracioni testovi jedne komande pokrivaju tri vrste scenarija, a testovi jednog upita dve.

Prva vrsta je **srećan put** (engl. *happy path*), najduži uspešan scenario, koji prolazi kroz sve slojeve i završava upisom u bazu. Za komandu kreiranja ture to je test `Create_stores_a_draft_tour`, koji šalje ispravan zahtev, proverava vraćeni DTO i čita upisanu turu.

Druga vrsta su **rubni slučajevi van domena**, ishodi koje jedinični test agregata ne može da dosegne, jer nastaju u API kontroleru i obradi zahteva pre nego što se agregat uopšte pozove. Sledeći kod prikazuje tri takva testa iz klase `TourAuthoringCommandTests`:

```cs
[Fact]
public async Task Create_requires_authentication()
{
    var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

    var response = await Client.PostAsJsonAsync("/api/exploration/tours", request);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}

[Fact]
public async Task Create_requires_the_explorer_role()
{
    var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
    var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

    var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}

[Fact]
public async Task Publish_rejects_another_authors_tour()
{
    var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

    var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    using var assertContext = Factory.CreateContext<ExplorationDbContext>();
    var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
    stored.Status.Should().Be(TourStatus.Draft);
}
```

U datom kodu treba uočiti sledeće:

- Prvi test koristi klijenta bez tokena i očekuje `401 Unauthorized`. Zahtev ne stiže do API kontrolera, pa jedinični test ovo ne može da proveri.
- Drugi test koristi klijenta sa ulogom administratora i očekuje `403 Forbidden`. Atribut `[Authorize(Roles = "explorer")]` na API kontroleru odbija zahtev pre akcije.
- Treći test koristi klijenta prijavljenog kao nasumičan korisnik i očekuje `404 Not Found`, jer servis turu drugog autora ne pronalazi. Provera zatim čita bazu i potvrđuje da je tura ostala u statusu nacrta, jer odbijena komanda ne sme da ostavi trag.
- Svaki scenario je zaseban test sa jednom akcijom, kao i kod jediničnih testova.

Treća vrsta je **jedno odbijanje domena**, jedan test kojim se proverava da izuzetak iz agregata postaje odgovor `400 Bad Request` i da baza ostaje nepromenjena. Test `Create_rejects_a_blank_name` šalje zahtev sa praznim imenom, proverava statusni kod i proverava da li se broj tura promenio. Jedan takav test po komandi je dovoljan, jer se time proverava pretvaranje izuzetka u odgovor, a ne pravilo. Ostala pravila ostaju u klasi `TourTests`.

Testovi upita proveravaju dve stvari. **Članstvo i projekcija** znači da odgovor sadrži tačno očekivane redove, u obliku DTO strukture. **Straničenje** (engl. *paging*) znači da parametri strane i veličine strane vraćaju očekivani deo rezultata. Za upit objavljenih tura to su testovi `GetPublished_returns_only_published_tours` i `GetPublished_pages_the_results`. Test upita ne menja ništa, pa ne čita bazu posle akcije. Odgovor krajnje tačke je ono što se testira.

## Pokretanje aplikacije u testu

Klasa `WebApplicationFactory<Program>` iz biblioteke Microsoft.AspNetCore.Mvc.Testing pokreće celu aplikaciju u procesu testa. Njena metoda `CreateClient()` vraća `HttpClient` čiji zahtevi odlaze pravo u tako pokrenutu aplikaciju, bez mreže.

Pokretanje aplikacije traje sekunde, pa se ne ponavlja za svaki test. **Deljeni objekat** (engl. *fixture*) je objekat koji test okvir pravi jednom i prosleđuje većem broju testova. **Kolekcija** je imenovana grupa test klasa koje dele jedan deljeni objekat i izvršavaju se jedna za drugom, a ne uporedo. Kolekcija je jedinica paralelizma u test okviru xUnit, pa test klase iz različitih kolekcija test okvir sme da izvršava uporedo.

Klasa `ExplorerApiFactory` iz projekta `Shared.Tests` nasleđuje `WebApplicationFactory<Program>` i dodaje upravljanje testnom bazom. Svaki modul je nasleđuje jednom praznom klasom i povezuje sa kolekcijom u datoteci `BaseIntegrationTest.cs`:

```cs
public sealed class ExplorationApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<ExplorationApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly ExplorationApiFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(ExplorationApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
        Client = Factory.CreateClient();
    }
}
```

U datom kodu treba uočiti sledeće:

- Prazna klasa `ExplorationApiFactory` postoji da bi fabrika znala kom modulu pripada. Fabrika iz imena test projekta izvodi ime baze, `explorer-test-exploration`, pa svaki modul ima svoju testnu bazu i moduli ne ometaju jedni druge.
- Fabrika pri prvom pokretanju briše tu bazu i pravi je iznova, a migracije se primenjuju kada se aplikacija pokrene. Struktura baze se tako postavlja jednom po pokretanju testova. Podrazumevani pristupni podaci se menjaju promenljivom okruženja `EXPLORER_TEST_DATABASE`.
- Klasa `IntegrationCollection` definiše kolekciju `Integration` i deklariše da njen deljeni objekat ima tip `ExplorationApiFactory`. Test okvir fabriku pravi jednom za ceo test projekat.
- Atribut `[Collection("Integration")]` na osnovnoj klasi uvodi u tu kolekciju svaku test klasu koja je nasleđuje. Konstruktor osnovne klase prima deljeni objekat, jer test okvir konstruktoru test klase prosleđuje deljene objekte njene kolekcije.
- Konstruktor osnovne klase je mesto za pripremu koju traži svaki test. Poziv `Reseed` vraća bazu u početno stanje i razmatra se u narednoj lekciji.

## Prijavljeni korisnik u testu

Većina krajnjih tačaka traži prijavljenog korisnika. Registracija kroz modul Identity bi svaki test vezala za tuđu funkcionalnost, pa bi greška u registraciji obarala testove tura. Umesto toga, fabrika sama izdaje token:

```cs
var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
```

U datom kodu treba uočiti sledeće:

- Metoda `CreateClientFor(userId, role)` vraća `HttpClient` koji uz svaki zahtev šalje token za zadatog korisnika i ulogu. Token je potpisan razvojnim ključem aplikacije, pa ga aplikacija prihvata kao pravi.
- Klasa `WellKnownUsers` iz projekta `Shared.Tests` sadrži stalne identifikatore korisnika: `Administrator`, `Explorer` i `SecondExplorer`. Isti identifikatori se koriste u početnim podacima, pa test zna ko je autor koje ture.
- Korisnik ne mora da postoji u bazi modula Identity, jer feature moduli korisnika poznaju samo kroz identifikator iz tokena. Krajnje tačke registracije i prijave testira jedino projekat `Identity.Tests`, jer su tamo one predmet testa.

## Organizacija testova modula

Test projekat modula, `<Ime>.Tests`, ima dva direktorijuma. Direktorijum `Unit/` sadrži testove agregata i domenskih servisa. Direktorijum `Integration/` sadrži datoteku `BaseIntegrationTest.cs`, direktorijum `Seeds/` sa početnim podacima i po jedan direktorijum za svaku grupu slučajeva korišćenja, sa istim imenom kao u aplikacionom sloju.

Za svaki aplikacioni servis postoji jedna test klasa `<Grupa>CommandTests`, na primer `TourAuthoringCommandTests` za `TourAuthoringService`. Za svaku upitnu klasu postoji jedna test klasa `<Grupa>QueryTests`, na primer `TourBrowsingQueryTests` za `TourBrowsingQueries`. Integracioni testovi traže lokalno pokrenut PostgreSQL server. Komanda `dotnet test` se izvršava u sistemu kontinualne integracije pri svakoj izmeni na grani `main`.
