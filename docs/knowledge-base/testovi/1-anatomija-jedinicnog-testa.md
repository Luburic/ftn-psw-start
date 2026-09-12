# Anatomija jediničnog testa

> **Nacrt.** Struktura lekcije sa beleškama po odeljcima. Svaki odeljak navodi definiciju, problem, primer, šta se uočava, izvor u knjizi i planirani obim.

**Preduslovi:** `server/2-arhitektura-modula/1-domenski-sloj/4-agregat.md`.
**Ciljevi učenja:** čitalac prepoznaje delove jednog testa u projektu, zna zašto testovi ne dele stanje i ume da napiše test agregata koji se čita kao rečenica o domenu.
**Radni primer:** agregat `Tour` iz modula Exploration i njegov test `TourTests` (`backend/Modules/Exploration/Exploration.Tests/Unit/Tours/TourTests.cs`).
**Planirani obim:** oko 1400 reči.

## Uvod (oko 120 reči)

Sidro: student je u lekciji o agregatu napisao `Tour.Publish()` sa pravilima o opisu, vremenu transporta i statusu. Pitanje: kako proveriti da pravila važe, a da se ne pokreće cela aplikacija i ručno klika. Odgovor je automatski test, metoda koja pozove `Publish()` i proveri ishod. Lekcija razlaže jedan takav test na delove.

## Test okvir i test metoda (oko 200 reči)

- **Definicija:** test okvir (engl. *test framework*) je biblioteka koja pronalazi testove, izvršava ih i prijavljuje rezultat. Test metoda je metoda označena atributom `[Fact]`; test klasa okuplja srodne test metode.
- **Problem:** test je običan kod, a nešto mora da ga pronađe i pokrene bez `Main` metode.
- **Primer:** `Constructor_rejects_empty_tags` iz `TourTests`, u celosti (tri reda).
- **Uočiti:** atribut je jedini znak da je metoda test; test uspeva ako se izvrši do kraja i pada na neuspeloj proveri ili neočekivanom izuzetku; `dotnet test` iz `backend` pokreće sve testove; test klasa `TourTests` je ulazna tačka, ne granica onoga što test proverava.
- **Izvor:** xunit.md (Test okvir, Test metoda i test klasa); Khorikov 3.2.

## Jedinica ponašanja (oko 150 reči)

- **Definicija:** jedinični test proverava jednu jedinicu ponašanja, jedan zahtev domena koji ima smisla domenskom stručnjaku, brzo i nezavisno od drugih testova.
- **Problem:** ako je jedinica klasa ili metoda, testovi prate strukturu koda i lome se pri svakom preimenovanju; ako je jedinica ponašanje, test preživljava promene koda koje ponašanje ne menjaju.
- **Primer:** narativni. "Objavljena tura mora imati vreme transporta" je jedno ponašanje, bez obzira na to koliko metoda učestvuje.
- **Uočiti:** ovo pravilo određuje sve dalje odeljke, od broja akcija do imena testa.
- **Izvor:** Khorikov 2.1 (samo definicija), 3.4.1 poslednji pasus.

## Tri dela testa (oko 250 reči)

- **Definicija:** svaki test ima pripremu, akciju i proveru (engl. *arrange, act, assert*), tim redom, razdvojene praznim redom.
- **Problem:** test bez oblika se čita red po red; sa oblikom čitalac odmah nalazi akciju i zna šta test proverava.
- **Primer:** `Publish_publishes_a_complete_tour` iz `TourTests`.
- **Uočiti:** priprema gradi turu i dodaje vreme transporta; akcija je jedan red, poziv `Publish()`; provera obuhvata sve ishode tog ponašanja, status i vreme objave. Više provera za jedno ponašanje nije greška. Akcija duža od jednog reda ukazuje da agregatu nedostaje metoda koja dva koraka drži zajedno. Dve akcije u jednom testu znače dva ponašanja, dakle dva testa.
- **Izvor:** Khorikov 3.1.1 do 3.1.5, 3.1.8.

## Nezavisnost testova (oko 300 reči)

- **Definicija:** test okvir pravi novu instancu test klase za svaku test metodu, pa polja klase ne prenose stanje između testova.
- **Problem:** kada više testova deli istu pripremu, prirodno je premestiti je u konstruktor ili polje. Tada izmena pripreme za jedan test menja pretpostavke svih ostalih, a čitalac testa ne vidi celu sliku bez skakanja po klasi.
- **Primer:** `CreateTour(string description)` iz `TourTests` i dva testa koji ga koriste sa različitim opisom (`Publish_publishes_a_complete_tour`, `Publish_rejects_a_short_description`).
- **Uočiti:** fabrička metoda je privatna i statička; parametar je samo ono što je testu bitno, a ostalo je sakriveno; svaki test i dalje sadrži svoju pripremu, samo kraću; polje `LongDescription` je konstanta, ne stanje. Konstruktor je prihvatljiv samo za pripremu koju traži svaki test, što se sreće kod integracionih testova (lekcija 5).
- **Izvor:** xunit.md (Životni ciklus test klase); Khorikov 3.3.

## Ime testa (oko 200 reči)

- **Definicija:** ime testa je rečenica koja opisuje ponašanje, sa podvlakama između reči.
- **Problem:** ime se čita u izveštaju kada test padne, često nedeljama posle pisanja i od strane nekog drugog. Šifrovano ime (`Publish_ShortDescription_Throws`) traži dešifrovanje; rečenica ne.
- **Primer:** imena iz `TourTests`, npr. `Publish_rejects_an_already_published_tour`, `Constructor_rejects_a_blank_name`.
- **Uočiti:** ime opisuje ishod, ne mehanizam (ne pominje `DomainException`); ime ne sadrži reči poput `Returns` ili `Should`; ime bi razumeo neko ko poznaje domen tura, a ne kod.
- **Izvor:** Khorikov 3.4.

## Parametrizovani test i provere (oko 180 reči)

- **Definicija:** parametrizovani test je test metoda označena atributom `[Theory]` koja se izvršava jednom za svaki skup vrednosti naveden atributom `[InlineData]`. Provera (engl. *assertion*) je naredba koja poredi dobijenu i očekivanu vrednost; u projektu se piše bibliotekom FluentAssertions, pozivom `Should()` nad vrednošću.
- **Problem:** isti test za praznu nisku i nisku sa razmacima bio bi dva prepisana testa; provera oblika `Assert.Equal(expected, actual)` obrće redosled iz rečenice.
- **Primer:** `Constructor_rejects_a_blank_description` sa dva `[InlineData]`; jedna provera `tour.Status.Should().Be(TourStatus.Draft)`.
- **Uočiti:** svaki skup vrednosti je zaseban test u izveštaju; ime testa postaje opštije, pa se srećan slučaj ostavlja kao zaseban `[Fact]`; poruka o grešci iz FluentAssertions navodi i očekivanu i dobijenu vrednost.
- **Izvor:** xunit.md (Parametrizovani test, Provere); Khorikov 3.5, 3.6.

## Van opsega

Deljeni objekti i kolekcije (lekcija 4), pitanje šta se testira a šta ne (lekcija 3), `[MemberData]`, imenovanje objekta pod testom sa `sut`, teardown.
