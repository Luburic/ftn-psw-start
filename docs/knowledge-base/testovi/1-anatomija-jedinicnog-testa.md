# Anatomija jediničnog testa

U lekciji o agregatu smo videli klasu `Tour` čija metoda `Publish` sprovodi tri pravila: tura ne sme već biti objavljena, opis mora imati bar sto znakova i mora postojati bar jedno vreme transporta. Ostaje pitanje kako proveriti da ta pravila zaista važe. Ručna provera traži da se pokrene aplikacija, prijavi korisnik, unese tura i pošalje zahtev za objavu, a zatim da se sve to ponovi za svako pravilo i posle svake izmene koda. Posle nekoliko izmena provera se preskače, a pravilo koje je nekada važilo tiho prestaje da važi.

**Automatski test** je kod koji izvršava deo sistema i proverava da li je ishod očekivan, bez učešća čoveka. **Jedinični test** (engl. *unit test*) je automatski test koji proverava jedno ponašanje domenskog objekta, izvršava se brzo i ne zavisi od drugih testova. Ova lekcija razlaže jedan jedinični test iz projekta na delove i objašnjava oblik svakog dela.

## Test okvir i test metoda

**Test okvir** (engl. *test framework*) je biblioteka koja pronalazi testove u kodu, izvršava ih i prijavljuje rezultat svakog testa. Test je obična metoda, koju test okvir pokreće bez `Main` metode i bez pokretanja aplikacije. U projektu se koristi test okvir xUnit.

**Test metoda** je metoda koju test okvir prepoznaje po atributu i izvršava kao test. Najjednostavniji takav atribut je `[Fact]`. **Test klasa** je klasa koja okuplja srodne test metode. Sledeći kod prikazuje jednu test metodu iz klase `TourTests` u projektu:

```cs
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

U datom kodu treba uočiti sledeće:

- Atribut `[Fact]` je jedini znak da je metoda test. Test okvir pronalazi sve javne metode sa tim atributom i svaku izvršava kao zaseban test. Metoda bez atributa se ne izvršava.
- Test metoda ne vraća vrednost. Test uspeva ako se metoda izvrši do kraja, a pada ako neka provera ne uspe ili ako kod izbaci neočekivan izuzetak.
- Test klasa `TourTests` je ulazna tačka za testove agregata `Tour`. Ime klase ne ograničava šta test proverava, jer test proverava ponašanje, a ponašanje može da obuhvati više klasa.
- Testovi se pokreću komandom `dotnet test` iz direktorijuma `backend`, a mogu se pokrenuti i iz razvojnog okruženja. Izveštaj sadrži ime svakog testa i njegov ishod.

## Jedinica ponašanja

Jedinični test proverava jednu **jedinicu ponašanja**, odnosno jedan zahtev domena koji ima smisla domenskom stručnjaku. Jedinica nije klasa ni metoda. Kada bi jedinica bila metoda, testovi bi pratili strukturu koda i morali bi da se menjaju pri svakom preimenovanju ili podeli metode. Kada je jedinica ponašanje, test preživljava sve izmene koda koje ponašanje ne menjaju.

Na primer, "objavljena tura mora imati bar jedno vreme transporta" je jedno ponašanje. Nije bitno da li ga sprovodi metoda `Publish`, konstruktor ili pomoćna metoda koju `Publish` poziva. Test to ponašanje proverava kroz javne metode agregata, isto kao što agregat koristi i aplikacioni servis. Ovo pravilo određuje sve ostale delove testa, od broja akcija u testu do njegovog imena.

## Tri dela testa

Svaki test se sastoji od tri dela, tim redom: **priprema** (engl. *arrange*) gradi objekte i dovodi ih u stanje potrebno za test, **akcija** (engl. *act*) izvršava ponašanje koje se proverava, a **provera** (engl. *assert*) upoređuje ishod sa očekivanim. Delovi se razdvajaju praznim redom. Sledeći kod prikazuje test iz klase `TourTests` sa sva tri dela:

```cs
[Fact]
public void Publish_publishes_a_complete_tour()
{
    var tour = CreateTour(LongDescription);
    tour.AddTransportTime(TransportMode.Bicycle, 45);

    tour.Publish();

    tour.Status.Should().Be(TourStatus.Published);
    tour.PublishedAt.Should().NotBeNull();
}
```

U datom kodu treba uočiti sledeće:

- Priprema gradi turu sa dovoljno dugim opisom i dodaje joj vreme transporta. To je najmanje stanje u kome objava može da uspe.
- Akcija je jedan red, poziv metode `Publish`. Čitalac testa nalazi akciju bez čitanja pripreme, jer je to jedini red između dva prazna reda.
- Provera ima dva reda, jer ponašanje "objava ture" ima dva ishoda, promenu statusa i beleženje vremena objave. Više provera za jedno ponašanje nije greška. Greška je provera koja pripada drugom ponašanju.
- Akcija duža od jednog reda ukazuje na nedostatak u agregatu. Ako bi test morao da pozove dve metode da bi tura bila objavljena, i aplikacioni servis bi morao, a zaboravljen drugi poziv bi ostavio turu u nevalidnom stanju. Tada agregatu nedostaje metoda koja oba koraka drži zajedno.
- Dve akcije u jednom testu znače dva ponašanja. Takav test se deli na dva.

## Nezavisnost testova

Test okvir pravi novu instancu test klase za svaku test metodu. Konstruktor test klase se izvršava pre svakog testa, a instanca se odbacuje posle njega. Polja test klase zato ne prenose stanje između testova, pa svaki test kreće od nule.

Ovo pravilo dopušta da se zajednička priprema smesti u konstruktor i polja klase, što je prirodna prva ideja kada više testova gradi isti objekat. Takva priprema ima dve posledice. Prvo, izmena pripreme za potrebe jednog testa menja pretpostavke svih ostalih testova u klasi, pa promena jednog testa obara druge. Drugo, čitalac testa više ne vidi celu sliku, jer mora da pogleda konstruktor da bi znao sa kakvom turom test radi.

Umesto konstruktora, zajednička priprema se izdvaja u **fabričku metodu** (engl. *factory method*), privatnu statičku metodu test klase koja gradi objekat sa zadatim svojstvima. Sledeći kod prikazuje fabričku metodu iz klase `TourTests` i dva testa koji je koriste:

```cs
private static readonly string LongDescription = new('o', 100);

private static Tour CreateTour(string description) =>
    new(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"]);

[Fact]
public void Publish_requires_a_transport_time()
{
    var tour = CreateTour(LongDescription);

    var publishing = () => tour.Publish();

    publishing.Should().Throw<DomainException>();
}

[Fact]
public void Publish_rejects_a_short_description()
{
    var tour = CreateTour("Kratak opis.");
    tour.AddTransportTime(TransportMode.Walking, 120);

    var publishing = () => tour.Publish();

    publishing.Should().Throw<DomainException>();
}
```

U datom kodu treba uočiti sledeće:

- Parametar fabričke metode je samo ono što je testovima bitno, opis ture. Autor, ime, težina i oznake su isti u svim testovima i sakriveni su u fabričkoj metodi.
- Svaki test i dalje sadrži svoju pripremu, samo kraću. Čitalac iz poziva `CreateTour("Kratak opis.")` vidi da test radi sa kratkim opisom, bez gledanja u fabričku metodu.
- Polje `LongDescription` je konstanta, ne stanje. Nijedan test ga ne menja, pa ne može da utiče na druge testove.
- Akcija koja treba da izbaci izuzetak zapisuje se kao lambda izraz i dodeljuje promenljivoj. Provera zatim izvršava lambda izraz i proverava da li je izuzetak izbačen. Da je metoda pozvana neposredno, izuzetak bi oborio test pre provere.

Konstruktor ostaje prihvatljiv za pripremu koju traži svaki test u klasi, poput pokretanja aplikacije kod integracionih testova. Tada priprema pripada osnovnoj klasi, što se razmatra u lekciji o integracionim testovima.

## Ime testa

Ime testa je rečenica koja opisuje ponašanje, sa podvlakama između reči. Ime u izveštaju čita neko ko test nije napisao, često nedeljama posle pisanja, kada test padne. Ime `Publish_ShortDescription_Throws` traži da čitalac zna kod da bi ga razumeo. Ime `Publish_rejects_a_short_description` razume svako ko poznaje domen tura.

Imena iz klase `TourTests` pokazuju obrazac:

```cs
public void Creation_produces_a_draft()
public void Constructor_rejects_a_blank_name(string name)
public void AddTransportTime_rejects_a_duplicate_transport()
public void Publish_rejects_an_already_published_tour()
```

U datom kodu treba uočiti sledeće:

- Ime opisuje ishod ponašanja, ne mehanizam. Ime ne pominje `DomainException`, jer je vrsta izuzetka detalj koda, a odbijanje je ponašanje.
- Ime ne sadrži reči poput `Returns`, `Should` ili `Test`. Te reči ne nose informaciju o domenu.
- Ime počinje operacijom agregata na koju se ponašanje odnosi, a kada je to kreiranje kroz konstruktor, rečju `Creation` ili `Constructor`. Tako se testovi jednog agregata u izveštaju grupišu po operaciji.

## Parametrizovani test

**Parametrizovani test** (engl. *parameterized test*) je test metoda označena atributom `[Theory]` koja se izvršava jednom za svaki skup ulaznih vrednosti naveden atributom `[InlineData]`. Bez njega bi test za praznu nisku i test za nisku sa razmacima bili dve prepisane metode. Sledeći kod prikazuje parametrizovani test iz klase `TourTests`:

```cs
[Theory]
[InlineData("")]
[InlineData("   ")]
public void Constructor_rejects_a_blank_description(string description)
{
    var creation = () => CreateTour(description);

    creation.Should().Throw<DomainException>();
}
```

U datom kodu treba uočiti sledeće:

- Test okvir izvršava metodu dva puta, jednom za svaki `[InlineData]`, i svako izvršavanje prijavljuje kao zaseban test sa vrednošću parametra u imenu.
- Ime testa je opštije nego kod običnog testa, jer mora da važi za sve skupove vrednosti. Zato se srećan put, tura sa ispravnim opisom, piše kao zaseban `[Fact]`, a ne kao još jedan red `[InlineData]`.

## Provere

Provera je naredba koja upoređuje dobijenu vrednost sa očekivanom i obara test ako se ne poklapaju. U projektu se provere pišu bibliotekom FluentAssertions. Provera počinje pozivom metode `Should()` nad vrednošću koja se proverava, a nastavlja se metodom koja iskazuje očekivanje:

```cs
tour.Status.Should().Be(TourStatus.Draft);
tour.TransportTimes.Should().BeEmpty();
creation.Should().Throw<DomainException>();
```

U datom kodu treba uočiti sledeće:

- Provera se čita istim redom kao rečenica: vrednost, pa očekivanje. Zapis `Assert.Equal(TourStatus.Draft, tour.Status)` iskazuje isto, ali obrnutim redom.
- Kada provera ne uspe, poruka o grešci navodi i očekivanu i dobijenu vrednost, pa se uzrok pada često vidi iz izveštaja, bez pokretanja testa u debageru.
