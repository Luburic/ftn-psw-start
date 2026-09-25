Test `Tour_is_published_when_all_rules_are_met` iz prethodne lekcije ima mali broj linija koda, ali se oslanja na mnoštvo logike koja se izvršava u pozadini. Pre izvršavanja prvog reda testa, aplikacija je pokrenuta nad testnom bazom, a u bazi postoji tura sa identifikatorom `TourSeed.PublishableRiverside.Id`, koju test objavljuje. Test zatim šalje zahtev kao prijavljeni korisnik i u proveri čita bazu. Isti poslovi se ponavljaju u svakom integracionom testu svakog modula:
1. Pokretanje aplikacije i to tako da radi sa testnom bazom podataka,
2. Vraćanje baze u poznato stanje pre svakog testa,
3. Slanje zahteva kao prijavljeni korisnik i
4. Čitanje baze u proveri.

Kada bi svaki test sam obavljao ove poslove, priprema bi zahtevala desetine ako ne i stotine linije koda. Treći posao bi, na primer, zahtevao da test registruje korisnika kroz modul `Identity`, prijavi ga i preuzme token iz odgovora. Pored mnoštva ponavljajućeg koda, desilo bi se da greška u registraciji (Identity modul) obara testove tura (Exploration modul). Da bismo izbegli ove probleme, deljene poslove preuzima platformski radni okvir. Projekat `Shared.Tests` je deo platformskog radnog okvira namenjen testovima. On stoji između biblioteka (npr. `xUnit`) i testova feature modula, pa test sadrži samo korake koji su specifični za ponašanje koje proverava.

## Biblioteka, platforma i modul

Klasa `WebApplicationFactory<Program>` iz biblioteke `Microsoft.AspNetCore.Mvc.Testing` pokreće celu aplikaciju unutar procesa testa. Njena metoda `CreateClient` vraća `HttpClient`, objekat koji šalje HTTP zahteve toj aplikaciji. Biblioteka ne poznaje odluke našeg projekta, pa ne zna da svaki modul ima svoju testnu bazu, niti kako aplikacija proverava identitet korisnika. Te odluke implementira klasa `ExplorerApiFactory` iz projekta `Shared.Tests`, koja nasleđuje `WebApplicationFactory<Program>`. Ona pokreće aplikaciju nad testnom bazom modula (prvi posao) i nudi po jednu metodu za preostale poslove:
- Metoda `Reseed` vraća bazu u početno stanje (drugi posao).
- Metoda `CreateClientFor` vraća klijenta koji uz svaki zahtev šalje JWT zadatog korisnika i uloge (treći posao). Metoda sama potpisuje token ključem koji aplikacija koristi u razvojnom okruženju, pa testovi tura ne zavise od modula `Identity`. Stalne identifikatore korisnika sadrži klasa `WellKnownUsers` iz istog projekta.
- Metoda `CreateContext` vraća EFC kontekst modula kroz koji provera (Assert) čita bazu (četvrti posao).

Tim koji razvija feature modul koristi platformu kroz njene javne metode, isto kao što koristi ASP.NET, i ne mora da poznaje njihovu implementaciju. Sa platformom ga povezuje datoteka `BaseIntegrationTest.cs`, koju ima svaki modul. Ovde se definiše **roditeljska klasa integracionih testova modula**, koju nasleđuje svaka integraciona test klasa modula. Svakom integracionom testu su potrebni fabrika i baza podataka u početnom stanju, pa se to priprema na jednom mestu, a ne u svakoj test klasi. Skraćen sadržaj te datoteke za `Exploration` modul je prikazan u nastavku:

```cs
public sealed class ExplorationApiFactory : ExplorerApiFactory;

public abstract class BaseIntegrationTest
{
    protected readonly ExplorationApiFactory Factory;

    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected BaseIntegrationTest(ExplorationApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
    }
}
```

U datom kodu treba uočiti sledeće:

- Modul definiše praznu klasu `ExplorationApiFactory` koja nasleđuje `ExplorerApiFactory` (prvi posao). Ovo omogućuje klasi `ExplorerApiFactory` da zna kom modulu pripada i da kreira testnu bazu podataka (npr. `explorer-test-exploration`) specifično za proveru rada tog modula, kako moduli ne bi remetili jedni druge tokom razvoja. Pri pokretanju testova fabrika briše bazu i pravi je iznova, nakon čega se primenjuju migracije.
- Polje `Factory` je fabrika modula. Testovi preko njega pozivaju metode `CreateClient`, `CreateClientFor` i `CreateContext`.
- Polje `JsonOptions` ponavlja podešavanje servera po kom se enumeracije zapisuju kao stringovi, pa test sa istim podešavanjem čita odgovor.
- Konstruktor `BaseIntegrationTest` poziva metodu `Reseed` i prosleđuje joj podatke modula, u primeru `ExplorationSeed.All`. Kako se ti podaci definišu opisuje naredni odeljak.

Test klasa nasleđuje roditeljsku klasu i kroz svoj konstruktor joj prosleđuje fabriku:

```cs
public class TourAuthoringCommandTests : BaseIntegrationTest
{
    public TourAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }
}
```

Test okvir pravi novu instancu test klase za svaku test metodu i njenom konstruktoru prosleđuje fabriku. Konstruktor roditeljske klase se zato izvršava pre svakog testa i pozivom `Reseed` vraća bazu u početno stanje.

## Definisanje početnih podataka

U integracionom testu su **početni podaci** (engl. *seed*) skup redova koji se upisuje u testnu bazu pre svakog testa. Baza je jedina zavisnost koju integracioni testovi modula dele, pa je ona i jedino mesto na kome jedan test može da poremeti drugi. Test koji čita mora unapred da zna šta je u bazi, a test koji piše ne sme da ostavi trag narednom testu. Oba zahteva ispunjava poziv `Reseed` pre svakog testa, kojim se sve tabele modula prazne i ponovo pune istim početnim podacima.

Struktura baze se postavlja jednom po pokretanju testova, a podaci pre svakog testa. Poziv `Reseed` je jedini dozvoljeni način da podaci uđu u bazu mimo krajnje tačke koja se testira. Test zato ne priprema stanje pozivom druge krajnje tačke, jer bi greška u toj funkcionalnosti obarala i testove funkcionalnosti koja se ispituje. Test takođe ne upisuje podatke kroz kontekst, jer bi zaobišao pravila domena i mogao bi da upiše stanje koje sistem ne može da dostigne.

Većina testova zahteva određen skup podataka da se pronađe u bazi podataka. Kada bi svaki test prvo upisivao u bazu šta mu je potrebno, imali bismo mnogo dupliranog koda. Ovaj problem možemo rešiti izdvajanjem deljene pripreme u metode, ali tada nastaje problem da su početni podaci testne baze podataka rasuti po svim testnim klasama. Rešenje za ovaj problem je da se početni podaci pišu kao imenovane instance agregata u statičkim klasama. Sledeći kod prikazuje skraćenu klasu `TourSeed` iz projekta:

```cs
internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"]);
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"]);
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"]);
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.Publish();
    }

    public static Tour[] All => [FortressWalk, PublishableRiverside, PublishedVineyards];
}

internal static class ExplorationSeed
{
    public static object[] All => [.. TourSeed.All];
}
```

U datom kodu treba uočiti sledeće:

- Za svaki agregat postoji jedna statička klasa sa imenovanim instancama, a ime instance iskazuje njeno stanje. `FortressWalk` je tura u pripremi bez vremena transporta, `PublishableRiverside` ima sve što je potrebno za objavu, a `PublishedVineyards` je objavljena.
- Instanca nastaje kroz konstruktor, pa se po potrebi pozivaju domenske metode u statičkom konstruktoru klase kako bi se agregat doveo u željeno stanje. Tako dobijeno stanje je stanje koje sistem zaista može da dostigne, pa početni podaci ne mogu da naruše pravila domena.
- Klasa sadrži samo niz prostih naredbi, bez grananja, pomoćnih metoda i parametara. Kada je testu potrebna varijacija postojeće instance, dodaje se nova imenovana instanca. Tako klasa ostaje skup podataka, a ne program.
- Testovi pronalaze red preko imenovane instance, na primer `TourSeed.FortressWalk.Id`. To je moguće jer agregat generiše identifikator u konstruktoru, a ne baza pri upisu.
- Svojstvo `All` je niz agregata, pa test može da ga filtrira po stanju ture. Klasa `ExplorationSeed` okuplja nizove svih agregata modula u jedan niz objekata, koji konstruktor roditeljske klase prosleđuje metodi `Reseed`.

Kada su testu potrebni redovi drugog modula, oni dolaze iz klasa početnih podataka tog modula. Test projekti smeju u tu svrhu da referenciraju jedni druge.

## Definisanje provera koje su otporne na izmenu testnih podataka

Provera `tours.Should().HaveCount(3)` prolazi danas i pada čim bilo ko doda novu instancu u klasu `TourSeed`, iako se funkcionalnost nije promenila. To je lažni pozitiv koji je izazvan izmenama testnih podataka, a ne koda. Provere se zato pišu tako da ne zavise od tačnog sadržaja početnih podataka.

Ako test registruje nov entitet, korisno je proveriti izmenu u broju entiteta u bazi kao relativan broj, odnosno da je broj entiteta uvećan za 1, a ne da je tačno 3. Priprema čita broj tura pre akcije, a provera ga poredi sa brojem posle akcije. Sledeći kod prikazuje takav test:

```cs
[Fact]
public async Task Explorer_creates_a_draft_tour()
{
    var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
    var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"]);
    using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
    var tourCountBefore = arrangeContext.Tours.Count();

    var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
    created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
    using var assertContext = Factory.CreateContext<ExplorationDbContext>();
    assertContext.Tours.Count().Should().Be(tourCountBefore + 1);
    var stored = assertContext.Tours.Single(tour => tour.Id == created.Id);
    stored.Status.Should().Be(TourStatus.Draft);
    stored.TransportTimes.Should().BeEmpty();
}
```

U datom kodu treba uočiti sledeće:

- Priprema čita `tourCountBefore`, a provera očekuje `tourCountBefore + 1`. Nova instanca u početnim podacima ne utiče na ovaj test.
- Priprema otvara jedan kontekst da pročita broj tura, a provera otvara drugi. Oba se otvaraju na mestu upotrebe, unutar naredbe `using`, i koriste samo za čitanje. Kontekst koji je otvoren pre akcije ne koristi se posle nje, jer bi za objekte koje je već učitao vratio stanje kakvo je bilo pre zahteva.

Test odbijanja istim obrascem utvrđuje da li je broj ostao isti:

```cs
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
using var assertContext = Factory.CreateContext<ExplorationDbContext>();
assertContext.Tours.Count().Should().Be(tourCountBefore);
```

Test upita izvodi očekivanja iz klase početnih podataka. Sledeći kod prikazuje provere iz testa `Only_published_tours_are_listed`:

```cs
tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
```

U datom kodu treba uočiti sledeće:

- Prva provera utvrđuje da li su svi vraćeni redovi objavljeni, bez obzira na njihov broj.
- Druga i treća provera utvrđuju članstvo po imenovanoj instanci, tako da objavljena tura jeste u rezultatu, a tura u statusu nacrta nije. Nova instanca u početnim podacima ne menja ni jednu ni drugu.
- Četvrta provera izvodi očekivani ukupan broj iz svojstva `All` istim uslovom koji primenjuje i upit. Kada neko doda još jednu objavljenu turu u početne podatke, obe strane provere uvećaju se za jedan.
