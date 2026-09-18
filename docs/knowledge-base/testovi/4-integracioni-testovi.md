U prethodnoj lekciji smo zaključili da aplikacioni servis, repozitorijum i API kontroler dobijaju integracione testove, koji jednim zahtevom na krajnju tačku prolaze kroz sve tri klase odjednom. U drugoj lekciji smo videli jedan takav test, `AddTransportTime_stores_the_time_on_the_tour`, i pratili put HTTP zahteva kroz aplikaciju. Ostaju tri pitanja. Prvo pitanje je koje zavisnosti u takvom testu ostaju prave, a koje se zamenjuju. Drugo pitanje je koje scenarije jedne funkcionalnosti vredi pokriti integracionim testom kada su pravila domena već pokrivena jediničnim testovima. Treće pitanje je kako test dolazi do pokrenute aplikacije, prijavljenog korisnika i podataka u bazi kada ništa od toga nije opisano u samom testu.

Sledeći kod prikazuje test `Publishes` iz klase `TourAuthoringCommandTests`, koji proverava isto ponašanje kao istoimeni jedinični test iz prve lekcije:

```cs
[Fact]
public async Task Publishes()
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

- Jedinični test gradi turu kroz konstruktor i metodu `AddTransportTime`. Integracioni test turu ne gradi, već je zatiče u bazi kao instancu `TourSeed.PublishableRiverside`. Priprema se zato svodi na jedan red, u kom se pravi klijent prijavljen kao korisnik `WellKnownUsers.Explorer` sa ulogom `explorer`.
- Akcija je jedan HTTP zahtev. Odgovor sa statusnim kodom 204 dokazuje da je zahtev prošao proveru identiteta, kontroler i servis bez izuzetka.
- Provera ne staje na statusnom kodu, nego kroz `Factory.CreateContext` otvara kontekst modula i čita turu iz baze. Tek red u bazi dokazuje da je jedinica posla sačuvala izmenu.
- Polje `Factory` dolazi iz osnovne klase koju test klasa nasleđuje, a `TourSeed` je statička klasa sa početnim podacima modula. Ostatak lekcije opisuje kako fabrika i početni podaci nastaju.

## Upravljane i neupravljane zavisnosti

**Zavisnost van procesa** (engl. *out-of-process dependency*) je sistem sa kojim aplikacija komunicira, a koji ne živi u njenom procesu. Drugi uslov jediničnog testa zabranjuje obraćanje takvoj zavisnosti, pa joj se obraćaju jedino integracioni testovi. Pitanje je da li test radi sa pravom zavisnošću ili je zamenjuje.

**Upravljana zavisnost** (engl. *managed dependency*) je zavisnost van procesa kojoj pristupa samo naša aplikacija. Primer je baza podataka modula. Raspored njenih tabela je detalj implementacije, jer ga niko van aplikacije ne vidi, pa se tabele mogu promeniti bez posledica po druge sisteme. Iz istog razloga test sme da radi sa pravom bazom i da proverava stanje u njoj, kao što test `Publishes` čita red ture.

**Neupravljana zavisnost** (engl. *unmanaged dependency*) je zavisnost van procesa koju vide i drugi sistemi. Primer je servis za slanje elektronske pošte ili platni provajder. Poslata poruka ili naplaćen iznos su posledice koje drugi sistem vidi, pa test ne sme da ih izazove. Takva zavisnost se u testu zamenjuje objektom koji beleži pozive, a test proverava da li je poziv upućen sa očekivanim podacima.

Svaki modul projekta danas ima jednu zavisnost van procesa, bazu podataka, i ona je upravljana. Testovi zato rade sa pravim PostgreSQL serverom. Kada bi se pravi server zamenio bazom u memoriji, test bi bio brži, ali bi tada dokazivao ponašanje sistema koji aplikacija ne koristi.

## Scenariji koje integracioni test pokriva

Integracioni test se ne piše za svako pravilo agregata, jer su pravila već pokrivena jediničnim testovima. Integracioni testovi jedne komande pokrivaju tri vrste scenarija, a testovi jednog upita dve.

Prva vrsta je **uspešan scenario** (engl. *happy path*), tok u kome zahtev prolazi kroz sve slojeve i završava upisom u bazu. Za komandu kreiranja ture to je test `Create_stores_a_draft_tour`, koji šalje ispravan zahtev, proverava vraćenu DTO strukturu i čita upisanu turu iz baze. Za komandu objave to je test `Publishes`.

Drugu vrstu čine **odbijanja van domena**, ishodi koje jedinični test agregata ne može da dosegne, jer nastaju u obradi zahteva pre nego što se agregat uopšte pozove. Sledeći kod prikazuje tri takva testa iz klase `TourAuthoringCommandTests`:

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

- Prvi test koristi polje `Client`, klijenta bez tokena, i očekuje odgovor sa statusnim kodom 401. Middleware za proveru identiteta odbija zahtev pre nego što on stigne do kontrolera.
- Drugi test koristi klijenta prijavljenog sa ulogom `administrator` i očekuje statusni kod 403. Atribut `[Authorize(Roles = "explorer")]` na kontroleru odbija zahtev čija uloga iz tokena nije `explorer`, pre nego što pozove akciju kontrolera.
- Treći test koristi klijenta prijavljenog kao nasumičan korisnik i očekuje statusni kod 404, jer servis tretira turu drugog autora kao nepostojeću. Provera zatim čita bazu i potvrđuje da je tura ostala u statusu nacrta, jer odbijena komanda ne sme da ostavi trag.

Treća vrsta je **odbijanje domena**, test koji proverava da li izuzetak iz agregata postaje odgovor sa statusnim kodom 400 i da li baza ostaje nepromenjena. Test `Create_rejects_a_blank_name` šalje zahtev sa praznim imenom ture, a zatim proverava statusni kod i da li je broj tura u bazi ostao isti. Po komandi je dovoljan jedan takav test, jer on proverava pretvaranje izuzetka u odgovor, a ne pravilo. Ostala pravila proverava klasa `TourTests`.

Testovi upita pokrivaju dve vrste scenarija. Prva vrsta je **članstvo i projekcija**, gde se proverava da li odgovor sadrži tačno one redove koji se očekuju, i to u obliku DTO strukture. Druga vrsta je **straničenje** (engl. *paging*), gde se proverava da li upit za zadati redni broj i veličinu stranice vraća očekivani deo rezultata. Za upit objavljenih tura to su testovi `GetPublished_returns_only_published_tours` i `GetPublished_pages_the_results`. Upit ne menja stanje sistema, pa test upita ne čita bazu posle akcije. Proverava se jedino odgovor krajnje tačke.

## Pokretanje aplikacije u testu

Klasa `WebApplicationFactory<Program>` iz biblioteke Microsoft.AspNetCore.Mvc.Testing pokreće celu aplikaciju unutar procesa testa. Njena metoda `CreateClient` vraća `HttpClient` koji zahteve šalje neposredno toj aplikaciji, bez mrežne veze.

Pokretanje aplikacije traje sekunde, pa se ne ponavlja za svaki test. **Deljeni objekat** (engl. *fixture*) je objekat koji test okvir pravi jednom i prosleđuje većem broju testova. Test okvir mora da zna koje test klase dele isti objekat, jer njih ne sme da izvršava uporedo nad istom bazom. **Kolekcija** (engl. *collection*) je imenovana grupa test klasa koje dele isti deljeni objekat. Test klase iz iste kolekcije se izvršavaju jedna za drugom, a test okvir sme uporedo da izvršava test klase iz različitih kolekcija.

Klasa `ExplorerApiFactory` iz projekta `Shared.Tests` nasleđuje `WebApplicationFactory<Program>` i dodaje testnu bazu i prijavljenog korisnika. Svaki modul je nasleđuje jednom praznom klasom i povezuje sa kolekcijom u datoteci `BaseIntegrationTest.cs`:

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

- Prazna klasa `ExplorationApiFactory` postoji da bi fabrika znala kom modulu pripada. Fabrika iz imena test projekta izvodi ime testne baze, `explorer-test-exploration`, pa svaki modul ima svoju bazu i moduli ne ometaju jedni druge. Podrazumevani podaci za konekciju se menjaju promenljivom okruženja `EXPLORER_TEST_DATABASE`.
- Kada nastane, fabrika briše tu bazu i pravi je iznova, a migracije modula se primenjuju kada se aplikacija pokrene.
- Klasa `IntegrationCollection` definiše kolekciju `Integration` i deklariše da je njen deljeni objekat tipa `ExplorationApiFactory`. Test okvir pravi fabriku jednom, pre prve test klase iz kolekcije.
- Atribut `[Collection("Integration")]` na osnovnoj klasi uvodi u kolekciju svaku test klasu koja je nasleđuje. Konstruktor prima fabriku, jer test okvir prosleđuje konstruktoru test klase deljeni objekat njene kolekcije.
- Poziv `Reseed` vraća bazu u početno stanje pre svakog testa. Polje `Client` je klijent bez prijavljenog korisnika.
- Polje `JsonOptions` ponavlja podešavanje servera po kom se enumeracije zapisuju kao stringovi, pa test sa istim podešavanjem čita odgovor.

## Prijavljeni korisnik u testu

Većina krajnjih tačaka traži prijavljenog korisnika. Kada bi se svaki test registrovao i prijavljivao kroz modul Identity, greška u registraciji obarala bi i testove tura. Zato fabrika sama izdaje token:

```cs
var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
```

U datom kodu treba uočiti sledeće:

- Metoda `CreateClientFor` vraća `HttpClient` koji uz svaki zahtev šalje token za zadatog korisnika i ulogu. Fabrika potpisuje token istim ključem koji aplikacija koristi u razvojnom okruženju, pa ga middleware za proveru identiteta prihvata kao pravi.
- Klasa `WellKnownUsers` iz projekta `Shared.Tests` sadrži stalne identifikatore tri korisnika: `Administrator`, `Explorer` i `SecondExplorer`. Isti identifikatori se koriste u početnim podacima, pa test zna ko je autor koje ture.
- Korisnik ne mora da postoji u bazi modula Identity, jer feature moduli poznaju korisnika samo po identifikatoru iz tokena. Krajnje tačke registracije i prijave testira jedino projekat `Identity.Tests`, jer su one predmet testiranja samo u tom modulu.

## Organizacija test projekta

Test projekat modula, `<Ime>.Tests`, ima dva direktorijuma. Direktorijum `Unit/` sadrži testove agregata i domenskih servisa. Direktorijum `Integration/` sadrži datoteku `BaseIntegrationTest.cs`, direktorijum `Seeds/` sa klasama početnih podataka i po jedan direktorijum za svaku grupu slučajeva korišćenja, sa istim imenom kao u aplikacionom sloju.

Za svaki aplikacioni servis postoji jedna test klasa `<Grupa>CommandTests`, na primer `TourAuthoringCommandTests` za `TourAuthoringService`. Za svaku upitnu klasu postoji jedna test klasa `<Grupa>QueryTests`, na primer `TourBrowsingQueryTests` za `TourBrowsingQueries`. Isti testovi se izvršavaju i u sistemu kontinualne integracije, pri svakom slanju izmena na granu `main` i pri svakom zahtevu za spajanje.
