# Automatsko testiranje sa xUnit

> **Status: normativan.** Primeri u ovom dokumentu odgovaraju stvarnom kodu projekta i predstavljaju obavezan obrazac koji timovi prate.

Ovaj dokument opisuje kako se automatski testovi pišu u ovom projektu. Prvi deo uvodi osnovne pojmove biblioteke xUnit. Drugi deo opisuje pomoćni kod koji je platformski tim izgradio za integracione testove i način na koji ga timovi koriste.

Pretpostavlja se da čitalac poznaje osnove automatskog testiranja, strukturu testa Arrange, Act, Assert i razliku između jediničnog i integracionog testa.

## Osnovni pojmovi

### Test okvir

Test okvir (engl. *test framework*) je biblioteka koja pronalazi testove u kodu, izvršava ih i prijavljuje rezultat svakog testa. U ovom projektu koristi se test okvir xUnit. Testovi se pokreću komandom `dotnet test` iz direktorijuma `backend`, a mogu se pokrenuti i iz razvojnog okruženja kroz prozor Test Explorer.

### Test metoda i test klasa

Test metoda je metoda koja proverava jedno ponašanje koda. Test metoda se označava atributom `[Fact]`. Test klasa je klasa koja okuplja srodne test metode. Test klasa nema poseban atribut. Test okvir pronalazi sve javne metode sa atributom `[Fact]` i svaku izvršava kao zaseban test.

```csharp
public class TourTests
{
    [Fact]
    public void Constructor_rejects_empty_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, []);

        creation.Should().Throw<DomainException>();
    }
}
```

U primeru treba uočiti sledeće.
- Atribut `[Fact]` označava metodu kao test. Bez njega test okvir metodu ne izvršava.
- Ime test metode opisuje ponašanje koje se proverava. Ime treba da bude razumljivo kada test padne, jer se tada čita u izveštaju.
- Test ne vraća vrednost. Test uspeva ako se izvrši do kraja, a pada ako neka provera ne uspe ili ako kod izbaci neočekivan izuzetak.

### Parametrizovani test

Parametrizovani test je test metoda koja se izvršava više puta, svaki put sa drugim ulaznim vrednostima. Parametrizovani test rešava problem ponavljanja iste test metode za više sličnih slučajeva. Označava se atributom `[Theory]`, a svaki skup ulaznih vrednosti navodi se atributom `[InlineData]`.

```csharp
[Theory]
[InlineData("")]
[InlineData("   ")]
public void Constructor_rejects_a_blank_name(string name)
{
    var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"]);

    creation.Should().Throw<DomainException>();
}
```

U primeru treba uočiti da test okvir izvršava metodu dva puta, jednom za praznu nisku i jednom za nisku sa razmacima. Svako izvršavanje prijavljuje se kao zaseban test.

### Provere

Provera (engl. *assertion*) je naredba koja upoređuje dobijenu vrednost sa očekivanom i obara test ako se vrednosti ne poklapaju. U ovom projektu provere se pišu bibliotekom FluentAssertions. Provera počinje pozivom metode `Should()` nad vrednošću koja se proverava, a nastavlja se metodom koja iskazuje očekivanje.

```csharp
response.StatusCode.Should().Be(HttpStatusCode.OK);
tours.Should().HaveCount(2);
token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role);
```

Prednost ovakvog zapisa je čitljivost provere i jasna poruka o grešci. Kada provera ne uspe, izveštaj sadrži i očekivanu i dobijenu vrednost.

### Životni ciklus test klase

Test okvir pravi novu instancu test klase za svaku test metodu. Konstruktor test klase izvršava se pre svakog testa, a instanca se odbacuje posle testa. Ovo pravilo obezbeđuje da testovi ne dele stanje kroz polja test klase. Zbog ovog pravila se priprema koja važi za svaki test piše u konstruktoru test klase.

### Deljeni objekat

Deljeni objekat (engl. *fixture*) je objekat koji test okvir pravi jednom i prosleđuje većem broju testova. Deljeni objekat rešava problem skupe pripreme. Kada priprema traje dugo, na primer pokretanje cele aplikacije, nije prihvatljivo ponavljati je za svaki test, pa se rezultat pripreme deli.

Deljeni objekat na nivou jedne test klase deklariše se interfejsom `IClassFixture<T>`. Test okvir tada pravi jednu instancu tipa `T` za celu test klasu i prosleđuje je konstruktoru test klase.

### Kolekcija testova

Kolekcija je imenovana grupa test klasa. Kolekcija ima dva dejstva. Prvo, deljeni objekat deklarisan interfejsom `ICollectionFixture<T>` pravi se jednom za celu kolekciju, pa ga dele sve test klase u njoj. Drugo, test klase u istoj kolekciji izvršavaju se jedna za drugom, a ne uporedo. Kolekcija je jedinica paralelizma u xUnit. Test klase koje nisu u istoj kolekciji test okvir sme da izvršava uporedo.

Kolekcija se definiše praznom klasom sa atributom `[CollectionDefinition]`, a test klasa joj pristupa atributom `[Collection]` sa istim imenom. Primer definicije i upotrebe nalazi se u nastavku dokumenta, u odeljku o povezivanju integracionih testova.

## Integracioni testovi u ovom projektu

Integracioni test u ovom projektu šalje pravi HTTP zahtev pokrenutoj serverskoj aplikaciji i proverava odgovor. Aplikacija pri tome radi sa pravom testnom bazom podataka PostgreSQL. Ovakav test proverava celu putanju zahteva, od kontrolera, preko aplikacionog i domenskog sloja, do baze i nazad.

Da bi ovakvi testovi bili jednostavni za pisanje, platformski tim održava projekat `Shared.Tests`. On sadrži mehanizam za pokretanje aplikacije, upravljanje testnom bazom i prijavljivanje korisnika u testovima. Timovi taj mehanizam koriste, a sami pišu testove i početne podatke svog modula.

### Pokretanje aplikacije u testu

Klasa `WebApplicationFactory<Program>` iz biblioteke Microsoft.AspNetCore.Mvc.Testing pokreće celu serversku aplikaciju u memoriji test procesa. Metoda `CreateClient()` vraća objekat `HttpClient` čiji zahtevi odlaze pravo u tako pokrenutu aplikaciju, bez mreže. Pokretanje aplikacije je skupa priprema, pa se factory deli kroz kolekciju, kako je opisano u prethodnom odeljku.

### Testna baza modula

Klasa `ExplorerApiFactory` iz projekta `Shared.Tests` nasleđuje `WebApplicationFactory<Program>` i dodaje upravljanje testnom bazom. Svaki test projekat dobija sopstvenu bazu, čije ime se izvodi iz imena projekta. Na primer, projekat `Identity.Tests` koristi bazu `explorer-test-identity`. Zahvaljujući tome, testovi različitih modula ne mogu da ometaju jedni druge.

Pri prvom pokretanju u okviru jednog izvršavanja testova, `ExplorerApiFactory` briše testnu bazu svog projekta i pravi je iznova, a migracije se primenjuju kada se aplikacija podigne. Struktura baze se tako postavlja jednom po izvršavanju, a podaci se vraćaju na početno stanje pre svakog testa, kako je opisano u nastavku.

Svaki modul nasleđuje ovu klasu jednom praznom klasom, na primer `public sealed class ExplorationApiFactory : ExplorerApiFactory;`. Podrazumevani pristupni podaci za bazu mogu se zameniti promenljivom okruženja `EXPLORER_TEST_DATABASE`.

### Početni podaci

Početni podaci (engl. *seed*) su skup podataka koji se upisuje u testnu bazu pre svakog testa. Početni podaci rešavaju problem poznatog polaznog stanja. Test koji čita podatke mora unapred da zna šta se u bazi nalazi, a test koji menja podatke ne sme da ošteti polazno stanje narednih testova. Zato se pre svakog testa sve tabele modula prazne i pune istim početnim podacima, pozivom metode `Reseed` iz klase `ExplorerApiFactory`.

Početni podaci se pišu u direktorijumu `Integration/Seeds` test projekta. Za svaki agregat postoji po jedna statička klasa koja sadrži imenovane instance, po jednu naredbu za svaku instancu. Pored njih postoji jedna klasa koja ih okuplja u jedinstven spisak.

```csharp
internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"]);
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"]);

    public static object[] All => [FortressWalk, CityStroll];
}

internal static class ExplorationSeed
{
    public static object[] All => [.. TourSeed.All];
}
```

U primeru treba uočiti sledeće.
- Svaka instanca ima ime. Testovi se pozivaju na instancu preko imena, na primer `TourSeed.FortressWalk.Id`, i nikada ne zapisuju identifikatore.
- Instance nastaju pozivom pravih konstruktora domenskih klasa, pa početni podaci ne mogu da naruše domenska pravila.
- Klase početnih podataka sadrže samo podatke, bez metoda. Kada je testu potrebno stanje koje početni podaci ne sadrže, test ga sam pravi u svom koraku Arrange. Ovo pravilo sprečava da klase početnih podataka vremenom narastu.

### Povezivanje

Svaki test projekat sadrži datoteku `BaseIntegrationTest.cs` koja povezuje prethodne pojmove. Datoteka sadrži tri male klase.

```csharp
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

U primeru treba uočiti sledeće.
- Kolekcija `Integration` obezbeđuje da se aplikacija i testna baza pripreme jednom za ceo test projekat i da se test klase izvršavaju jedna za drugom.
- Konstruktor osnovne klase izvršava se pre svakog testa, jer test okvir pravi novu instancu test klase za svaku test metodu. Poziv metode `Reseed` zato pre svakog testa vraća bazu na početno stanje.
- Test klase modula nasleđuju `BaseIntegrationTest` i time dobijaju spreman `HttpClient` i poznato stanje baze, bez sopstvene pripreme.
- Server serijalizuje enumeracije kao niske, pa test koji čita odgovor prosleđuje `JsonOptions` sa istim konverterom, na primer `ReadFromJsonAsync<TourDto>(JsonOptions)`.

### Prijavljeni korisnik u testu

Većina krajnjih tačaka zahteva prijavljenog korisnika. Umesto da test registruje i prijavljuje korisnika kroz modul Identity, klasa `ExplorerApiFactory` sama izdaje važeći token. Metoda `CreateClientFor(userId, role)` vraća `HttpClient` koji uz svaki zahtev šalje token za zadatog korisnika i ulogu. Klasa `WellKnownUsers` iz projekta `Shared.Tests` sadrži stalne identifikatore korisnika koje početni podaci i testovi zajednički koriste.

```csharp
[Fact]
public async Task Create_stores_a_draft_tour()
{
    var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

    var response = await client.PostAsJsonAsync("/api/exploration/tours",
        new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"]));

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var tour = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
    tour!.Status.Should().Be(TourStatus.Draft);
}
```

Ovakav pristup je ispravan jer funkcionalni moduli korisnika poznaju samo preko njegovog identifikatora, pa korisnik ne mora da postoji u bazi modula Identity. Krajnje tačke registracije i prijave testira jedino projekat `Identity.Tests`, jer su tamo one predmet testa.

## Organizacija testova modula

Testovi modula žive u projektu `<Ime>.Tests`, u dva direktorijuma. Direktorijum `Unit` sadrži jedinične testove agregata i domenskih servisa. Ti testovi ne koriste bazu ni HTTP i ne nasleđuju `BaseIntegrationTest`. Direktorijum `Integration` sadrži integracione testove, početne podatke i datoteku `BaseIntegrationTest.cs`.

Integracioni testovi se grupišu po grupi slučajeva upotrebe i prate podelu aplikacionog sloja. Za svaku komandnu klasu postoji po jedna test klasa, na primer `TourAuthoringCommandTests` za `TourAuthoringService`. Za svaku upitnu klasu postoji po jedna test klasa, na primer `TourBrowsingQueryTests` za `TourBrowsingQueries`. Komandni test menja stanje i proverava ishod promene. Upitni test čita početne podatke i ne menja ništa.

## Pokretanje testova

Testovi se pokreću komandom `dotnet test` iz direktorijuma `backend`. Za integracione testove neophodno je da PostgreSQL server radi lokalno, sa pristupnim podacima iz podrazumevane konfiguracije. Testne baze se prave i brišu automatski, pa ih nije potrebno ručno održavati. Ista komanda izvršava se i u sistemu kontinualne integracije pri svakoj izmeni na grani `main`.
