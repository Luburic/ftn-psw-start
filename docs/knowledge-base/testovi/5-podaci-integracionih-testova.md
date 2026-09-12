# Podaci integracionih testova

U prethodnoj lekciji test `Publish_publishes_a_complete_tour` šalje zahtev za objavu ture sa identifikatorom `TourSeed.PublishableRiverside.Id`. Ostaje pitanje odakle je ta tura u bazi, šta se sa njom dešava posle testa i zašto naredni test zatiče istu bazu kao prethodni. Baza je jedina zavisnost koju integracioni testovi dele, pa je ona i jedini put kojim jedan test može da pokvari drugi. Ova lekcija opisuje kako testovi modula dele jednu bazu, a ostaju nezavisni, i kako se početni podaci i provere pišu tako da ih rast modula ne obara.

## Poznato stanje pre svakog testa

**Početni podaci** (engl. *seed*) su skup redova koji se upisuje u testnu bazu pre svakog testa. Test koji čita mora unapred da zna šta je u bazi, a test koji piše ne sme da ostavi trag narednom testu. Oba zahteva rešava isti postupak, kojim se pre svakog testa sve tabele modula isprazne i napune istim početnim podacima.

Postupak se izvršava na početku testa, a ne na kraju. Čišćenje na kraju se preskače kada se izvršavanje testa prekine, na primer u debageru, pa zaostali red obara naredne testove. Čišćenje na početku ne može da se preskoči, pa posebna faza čišćenja posle testa ne postoji. Testovi u kolekciji se izvršavaju jedan za drugim, pa dva testa nikada ne dele bazu u istom trenutku.

Postupak pokreće konstruktor osnovne klase, prikazan u prethodnoj lekciji:

```cs
protected BaseIntegrationTest(ExplorationApiFactory factory)
{
    Factory = factory;
    Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
    Client = Factory.CreateClient();
}
```

U datom kodu treba uočiti sledeće:

- Konstruktor osnovne klase se izvršava pre svakog testa, jer test okvir pravi novu instancu test klase za svaku test metodu.
- Metoda `Reseed` fabrike prazni sve tabele koje kontekst modula mapira i upisuje prosleđene objekte. Struktura baze se postavlja jednom po pokretanju testova, a podaci pre svakog testa.
- Poziv `Reseed` je jedini put kojim podaci ulaze u bazu mimo krajnje tačke koja se testira.

## Početni podaci koji preživljavaju rast

Početni podaci rastu sa svakom novom funkcionalnošću i svakim novim testom. Ako svaki test upisuje svoje redove, priprema se ponavlja i testovi postaju dugi. Ako se podaci grade pomoćnim metodama sa parametrima i grananjem, klasa početnih podataka postaje program koji i sam može da sadrži grešku. Početni podaci se zato pišu kao imenovane instance u statičkim klasama. Sledeći kod prikazuje skraćenu klasu `TourSeed` iz projekta:

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

- Za svaki agregat postoji jedna statička klasa sa imenovanim instancama, a ime beleži stanje instance. `FortressWalk` je nacrt bez vremena transporta, `PublishableRiverside` ima sve što je potrebno za objavu, a `PublishedVineyards` je objavljena.
- Instanca nastaje kroz konstruktor i domenske metode agregata. Kada je potrebno stanje koje konstruktor ne daje, ono se dostiže pozivima domenskih metoda u statičkom konstruktoru klase. Svako posejano stanje je zato stanje koje sistem zaista može da dostigne, pa početni podaci ne mogu da naruše pravila domena.
- Klasa sadrži samo linearne iskaze: bez grananja, bez pomoćnih metoda i bez parametara. Kada je potrebna varijacija postojeće instance, dodaje se nova imenovana instanca. To je cena koja se plaća da klasa ostane podaci, a ne kod.
- Testovi na red upućuju imenom, na primer `TourSeed.FortressWalk.Id`, pa agregat generiše identifikator u konstruktoru, a ne baza pri upisu.
- Spisak `All` je niz agregata, a ne niz objekata, pa testovi iz njega izvode očekivane brojeve. Klasa `ExplorationSeed` okuplja spiskove svih agregata modula u jedan, koji konstruktor osnovne klase prosleđuje metodi `Reseed`.
- Redovi drugog modula dolaze iz klasa početnih podataka tog modula. Test projekti smeju da referenciraju jedni druge za tu potrebu.

## Tri kanala

Integracioni test sa sistemom komunicira kroz tri kanala, a svaki kanal ima jedan smer. Stanje ulazi u bazu isključivo kroz početne podatke. Akcija se izvršava isključivo jednim HTTP zahtevom. Ishod se posmatra isključivo kroz odgovor krajnje tačke i čitanje baze kroz svež kontekst.

Prva dva pravila štite nezavisnost funkcionalnosti. Kada bi test stanje pripremao pozivom druge krajnje tačke, greška u toj funkcionalnosti bi obarala i testove ove. Kada bi test pisao kroz kontekst, zaobišao bi pravila domena i mogao bi da poseje stanje koje sistem ne može da dostigne. Sledeći kod prikazuje test koji poštuje sva tri pravila:

```cs
[Fact]
public async Task Create_stores_a_draft_tour()
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

- Metoda `CreateContext` fabrike vraća svež kontekst modula. Test ga otvara na mestu upotrebe, unutar naredbe `using`, i koristi ga samo za čitanje.
- Priprema otvara jedan kontekst da pročita broj tura, a provera otvara drugi. Kontekst otvoren pre akcije se ne čita posle nje, jer EF Core prati jednom učitane objekte i vratio bi stanje od pre zahteva. Svaki deo testa ima svoj kontekst, kao što svaka poslovna operacija u aplikaciji ima svoju jedinicu posla.
- Server serijalizuje enumeracije kao niske, pa test pri čitanju odgovora prosleđuje `JsonOptions` sa istim podešavanjem. Polje je definisano u osnovnoj klasi.
- Ništa u kodu ne sprečava test da piše kroz kontekst ili da pozove drugu krajnju tačku u pripremi. Pravilo o tri kanala se drži pregledom koda.

## Provere koje novi red ne obara

Provera `tours.Should().HaveCount(3)` prolazi danas i pada čim bilo ko doda novu instancu u klasu `TourSeed`, iako se funkcionalnost nije promenila. To je lažni pozitiv iz druge lekcije, ovog puta izazvan podacima, a ne kodom. Provere se zato pišu tako da ne zavise od tačnog sadržaja početnih podataka.

Test komande broj redova proverava kao razliku. U prethodnom primeru priprema čita `tourCountBefore`, a provera očekuje `tourCountBefore + 1`. Test odbijanja istim obrascem proverava da se broj nije promenio:

```cs
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
using var assertContext = Factory.CreateContext<ExplorationDbContext>();
assertContext.Tours.Count().Should().Be(tourCountBefore);
```

Test upita ostaje na HTTP nivou, jer je projekcija ono što testira, a očekivanja izvodi iz klase početnih podataka. Sledeći kod prikazuje provere iz testa `GetPublished_returns_only_published_tours`:

```cs
tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
```

U datom kodu treba uočiti sledeće:

- Prva provera proverava oblik rezultata, da su svi vraćeni redovi objavljeni, bez obzira na to koliko ih ima.
- Druga i treća provera proveravaju članstvo po imenovanoj instanci, da objavljena tura jeste u rezultatu, a tura u statusu nacrta nije. Nova instanca u početnim podacima ne menja ni jednu ni drugu.
- Četvrta provera očekivani ukupan broj izvodi iz spiska `All` istim uslovom koji upit primenjuje. Kada neko doda još jednu objavljenu turu u početne podatke, obe strane provere se pomere za jedan.
