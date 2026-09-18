Test `Publishes` iz prethodne lekcije šalje zahtev za objavu ture sa identifikatorom `TourSeed.PublishableRiverside.Id` i zatiče tu turu u bazi, sa dovoljno dugim opisom i vremenom transporta. Ostaje pitanje kako je ta tura dospela u bazu, šta se sa njom dešava posle testa i zašto naredni test zatiče bazu u istom stanju kao prethodni. Baza je jedina zavisnost koju integracioni testovi modula dele, pa je ona i jedino mesto na kome jedan test može da poremeti drugi. Uz to, podaci u bazi rastu sa svakom novom funkcionalnošću, pa test koji je juče prolazio može da padne zbog reda koji je neko dodao za drugi test. Ova lekcija opisuje kako testovi dele jednu bazu, a ostaju nezavisni, i kako se početni podaci i provere pišu tako da ih rast modula ne obara.

## Poznato stanje pre svakog testa

U integracionom testu su **početni podaci** (engl. *seed*) skup redova koji se upisuje u testnu bazu pre svakog testa. Test koji čita mora unapred da zna šta je u bazi, a test koji piše ne sme da ostavi trag narednom testu. Oba zahteva ispunjava isti postupak, kojim se pre svakog testa sve tabele modula prazne i ponovo pune istim početnim podacima.

Postupak se izvršava na početku testa, a ne na kraju. Čišćenje na kraju se preskače kada se izvršavanje testa prekine, na primer u debageru, pa zaostali red obara naredne testove. Čišćenje na početku ne može da se preskoči, pa posebna faza čišćenja posle testa ne postoji. Pošto se testovi iste kolekcije izvršavaju jedan za drugim, dva testa nikada ne dele bazu u istom trenutku.

Konstruktor osnovne klase `BaseIntegrationTest` pokreće ovaj postupak:

```cs
protected BaseIntegrationTest(ExplorationApiFactory factory)
{
    Factory = factory;
    Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
    Client = Factory.CreateClient();
}
```

U datom kodu treba uočiti sledeće:

- Test okvir pravi novu instancu test klase za svaku test metodu, pa se konstruktor osnovne klase izvršava pre svakog testa.
- Metoda `Reseed` fabrike prazni sve tabele koje kontekst modula mapira, a zatim upisuje prosleđene objekte. Struktura baze se postavlja jednom po pokretanju testova, a podaci pre svakog testa.
- Poziv `Reseed` je jedini način da podaci uđu u bazu mimo krajnje tačke koja se testira.

## Početni podaci koji preživljavaju rast

Početni podaci rastu sa svakom novom funkcionalnošću i svakim novim testom. Ako svaki test upisuje svoje redove, priprema se ponavlja i testovi postaju dugački. Ako se podaci grade pomoćnim metodama sa parametrima i grananjem, klasa početnih podataka postaje program koji i sam može da sadrži grešku. Početni podaci se zato pišu kao imenovane instance agregata u statičkim klasama. Sledeći kod prikazuje skraćenu klasu `TourSeed` iz projekta:

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

- Za svaki agregat postoji jedna statička klasa sa imenovanim instancama, a ime instance iskazuje njeno stanje. `FortressWalk` je nacrt bez vremena transporta, `PublishableRiverside` ima sve što je potrebno za objavu, a `PublishedVineyards` je objavljena.
- Instanca nastaje kroz konstruktor, a stanje koje konstruktor ne daje dostiže se pozivima domenskih metoda u statičkom konstruktoru klase. Svako tako dobijeno stanje je stanje koje sistem zaista može da dostigne, pa početni podaci ne mogu da naruše pravila domena.
- Klasa sadrži samo niz prostih naredbi, bez grananja, pomoćnih metoda i parametara. Kada je testu potrebna varijacija postojeće instance, dodaje se nova imenovana instanca. Tako klasa ostaje skup podataka, a ne program.
- Testovi pronalaze red preko imenovane instance, na primer `TourSeed.FortressWalk.Id`. To je moguće jer agregat generiše identifikator u konstruktoru, a ne baza pri upisu.
- Svojstvo `All` je niz agregata, a ne niz objekata, pa test može da ga filtrira po stanju ture. Klasa `ExplorationSeed` okuplja nizove svih agregata modula u jedan niz objekata, koji konstruktor osnovne klase prosleđuje metodi `Reseed`.

Kada su testu potrebni redovi drugog modula, oni dolaze iz klasa početnih podataka tog modula. Test projekti smeju u tu svrhu da referenciraju jedni druge.

## Tri kanala

**Kanal** je put kojim integracioni test dodiruje sistem. Test ima tri kanala, a svaki kanal ima jedan smer. Stanje ulazi u bazu isključivo kroz početne podatke. Akcija se izvršava isključivo jednim HTTP zahtevom. Ishod se posmatra isključivo kroz odgovor krajnje tačke i čitanje baze novim kontekstom.

Prvi kanal isključuje dve prečice. Kada bi test pripremao stanje pozivom druge krajnje tačke, greška u toj funkcionalnosti obarala bi i testove funkcionalnosti koja se ispituje. Kada bi test pisao kroz kontekst, zaobišao bi pravila domena i mogao bi da upiše stanje koje sistem ne može da dostigne. Sledeći kod prikazuje test koji poštuje sva tri kanala:

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

- Metoda `CreateContext` fabrike vraća nov kontekst modula. Test ga otvara na mestu upotrebe, unutar naredbe `using`, i koristi ga samo za čitanje.
- Priprema otvara jedan kontekst da pročita broj tura, a provera otvara drugi. Kontekst koji je otvoren pre akcije ne koristi se posle nje, jer bi za objekte koje je već učitao vratio stanje kakvo je bilo pre zahteva. Svaki deo testa ima svoj kontekst, kao što svaka poslovna operacija u aplikaciji ima svoju jedinicu posla.
- Ništa u kodu ne sprečava test da piše kroz kontekst ili da pozove drugu krajnju tačku u pripremi. Pravilo o tri kanala sprovodi se pregledom koda.

## Provere koje novi red ne obara

Provera `tours.Should().HaveCount(3)` prolazi danas i pada čim bilo ko doda novu instancu u klasu `TourSeed`, iako se funkcionalnost nije promenila. To je lažni pozitiv iz druge lekcije, ovog puta izazvan podacima, a ne kodom. Provere se zato pišu tako da ne zavise od tačnog sadržaja početnih podataka.

Test komande proverava broj redova kao razliku. U prethodnom primeru priprema čita `tourCountBefore`, a provera očekuje `tourCountBefore + 1`. Test odbijanja istim obrascem utvrđuje da li je broj ostao isti:

```cs
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
using var assertContext = Factory.CreateContext<ExplorationDbContext>();
assertContext.Tours.Count().Should().Be(tourCountBefore);
```

Test upita izvodi očekivanja iz klase početnih podataka. Sledeći kod prikazuje provere iz testa `GetPublished_returns_only_published_tours`:

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
