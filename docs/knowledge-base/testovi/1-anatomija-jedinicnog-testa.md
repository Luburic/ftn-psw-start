U modulu `Exploration` autor pravi turu kao agregat `Tour`. Tura ima autora, ime, opis, težinu, tagove i status. Konstruktor odbija turu bez imena, bez opisa ili bez ijednog taga, a novu turu postavlja u status nacrta. Autor zatim turi dodaje vremena transporta, odnosno koliko minuta obilazak traje pešice, biciklom ili automobilom. Kada je tura spremna, autor je objavljuje pozivom metode `Publish`, koja menja status ture u objavljen i beleži vreme objave. Metoda `Publish` sprovodi tri pravila:
1. Tura ne sme već biti objavljena,
2. Opis mora imati bar sto znakova i
3. Mora postojati bar jedno vreme transporta.

Ova pravila definiše domen problema, odnosno poslovni kontekst u kom se koristi naša aplikacija. Ako pravila nisu ispoštovana, tura nije validna i ne treba da bude dostupna turistima. Zbog toga fiksiramo pravila u kodu i danas imamo tu garanciju. Pitanje je kako da osiguramo da će ova pravila ostati u kodu i nastaviti da se primenjuju sutra, nakon mnogih izmena koje čine (nespretni) programeri i agenti.

Jedna opcija je da sa svakom izmenom koda izvršimo ručnu proveru svih starih funkcionalnosti. Ovo traži da se pokrene aplikacija, prijavi korisnik, unese tura i pošalje zahtev za objavu, a zatim da se sve to ponovi za svako pravilo. Ovo je **manualno testiranje** i u praksi zahteva mnogo strpljivog ljudstva da se sprovodi konzistentno.

**Automatski test** je kod koji izvršava deo sistema i proverava da li je ishod očekivan, bez učešća čoveka. Tako možemo definisati automatski test za svaku funkcionalnost sistema i pokretati sve testove koje imamo nakon svake izmene. Rezultate dobijamo brzo (mereno u sekundima ili minutima) i sa njima određenu garanciju da nismo poremetili stari kod. Jedan automatski test se preslikava na jednu funkciju koju mi definišemo, a koju pokreće test okvir.

U praksi se definišu automatski testovi za razne vrste aplikacija, uključujući one koji proveravaju rad serverske aplikacije, klijentske aplikacije i oba. Za početak ćemo se fokusirati na testove serverske aplikacije.

## Test okvir i test metoda

**Test okvir** (engl. *test framework*) je biblioteka koja pronalazi testove u kodu, izvršava ih i prijavljuje rezultat svakog testa. U našem projektu koristimo test okvir xUnit za proveru rada serverske aplikacije. Testovi se pokreću komandom `dotnet test` iz direktorijuma `backend`, a mogu se pokrenuti i iz razvojnog okruženja. Izveštaj sadrži ime svakog testa i njegov ishod.

Test je obična metoda koju test okvir prepoznaje po atributu i izvršava kao test. Najjednostavniji takav atribut je `[Fact]`. Pošto u OOP metode žive u klasama, test metode se definišu u okviru **test klase**, koja okuplja srodne test metode. Sledeći kod prikazuje jednu test metodu iz klase `TourTests`:

```cs
public class TourTests
{
    [Fact]
    public void New_tour_requires_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, []);

        creation.Should().Throw<DomainException>();
    }
}
```

U datom kodu treba uočiti sledeće:

- Atribut `[Fact]` je jedini znak da je metoda test. Test okvir pronalazi sve javne metode sa tim atributom i svaku izvršava kao zaseban test. Metoda bez atributa se ne izvršava.
- Konvencija za pisanje test metoda je Snake_case, gde je naziv testa rečenica o domenu koja prenosi poslovnu nameru (u primeru "Nove ture zahtevaju definisan spisak tagova"). Naziv zato ne sadrži imena klasa ni metoda. Ovo je apstraktnije od opisa tehničkog konstrukta koji se proverava (npr. "Constructor_rejects_empty_tags" ili "Publish_rejects_published_tour") i otpornije je na refaktorisanje (ako sutra zamenimo poziv konstruktora sa fabričkom metodom koja pravi objekat, naziv testa se ne menja), a uz to je čitljivije klijentu koji razume poslovnu nameru.
- Test metoda ne vraća vrednost. Test uspeva ako se metoda izvrši do kraja, a pada ako neka provera ne uspe ili ako kod izbaci neočekivan izuzetak.
- Test klasa `TourTests` je ulazna tačka za testove agregata `Tour`. Ime klase ne ograničava šta test proverava, jer test proverava ponašanje, a ponašanje može da obuhvati više klasa.

## Struktura koda test metode

Izvorni kod svakog testa tipično ima tri segmenta:
1. **Priprema** (engl. *Arrange*) gradi objekte čije ponašanje testiramo i dovodi ih u stanje potrebno za test,
2. **Akcija** (engl. *Act*) izvršava ponašanje koje se proverava i
3. **Provera** (engl. *Assert*) upoređuje ishod ponašanja sa očekivanim.

Sledeći kod prikazuje test iz klase `TourTests`, gde su tri segmenta razdvojena praznim redom:

```cs
[Fact]
public void Tour_is_published_when_all_rules_are_met()
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
- Provera ima dva reda, jer ponašanje "objava ture" ima dva ishoda, promenu statusa i beleženje vremena objave.

Priprema je najčešće najduži deo testa i ima do tri koraka:
1. Instancira objekte čije ponašanje testiramo i dovodi ih u željeno početno stanje,
2. Ako metoda koja će se testirati prihvata složene objekte, priprema objekte koji se prosleđuju kao argumenti metodi i
3. Ako se očekuje složeniji ishod testiranog ponašanja, priprema objekte koji se koriste u proveri ishoda testa.

Prvi korak se ponavlja u većini testova jedne klase, pa se izdvaja u zasebnu metodu, kako bi se čitava priprema svela na poziv te metode. Klasa `TourTests` za to ima metodu `CreateTour`, koja gradi turu sa zadatim opisom. Provera zavisi od vrste metode koju akcija poziva. Ako metoda vraća vrednost, provera utvrđuje da li je stvarna povratna vrednost jednaka očekivanoj. Ako metoda menja stanje, provera utvrđuje da li je stvarno stanje objekta nakon poziva jednako očekivanom.

Jasna greška u kodu testa je kada izvršavamo više od jedne akcije u centralnom delu. Postoje dva moguća uzroka ovog problema:
1. Ako bi test morao da pozove dve metode da bi tura bila objavljena, i aplikacioni servis bi morao, a zaboravljen drugi poziv bi ostavio turu u nevalidnom stanju. Tada agregatu nedostaje metoda koja oba koraka drži zajedno.
2. Ako je agregat ispravno definisan, test izvršava dve promene i proverava dva ponašanja. Takav test delimo na dva.

## Meta testa - Jedinica ponašanja

Najprostija vrsta automatskog testa je jedinični test. **Jedinični test** (engl. *unit test*) je automatski test koji proverava jednu *jedinicu ponašanja*. Teško je precizno definisati šta je jedinica ponašanja, odnosno šta su njene granice. Svaki automatski test će u akciji pozvati konstruktor ili metodu objekta. Ponašanje koje se proverava je ponašanje te metode. Međutim, metode se razlikuju po složenosti koja stoji iza njih. Na primer, jedna metoda može proveriti jedan uslov i, kada je ispunjen, izmeniti stanje objekta, sve u par linija koda. Druga metoda može imati složenu logiku koja podrazumeva pozive metoda mnoštva drugih objekata, kako bi kroz 50 linija koda iskoordinisala ispunjenje nekog zahteva. Oba primera mogu biti jedinica ponašanja.

Segment logike definišemo kao jedinicu ponašanja koju testira jedinični test kada:
1. Predstavlja semantički uokvirenu sposobnost sistema koju koriste drugi delovi sistema
2. Može da se izvrši u procesu testa, bez obraćanja bazi podataka ili drugom sistemu
3. Može da se izoluje kako bi jedan test mogao da proveri ponašanje, nezavisno od rada drugih testova

Prvi uslov je najizazovniji za razumevanje jer traži analizu semantike. Zato ga u projektu svodimo na pravilo. Jedinica ponašanja počinje javnom metodom ili konstruktorom koji poziva kod iz drugog sloja ili klijent putem HTTP zahteva, a obuhvata sav kod koji se pri tom pozivu izvrši. Takve su akcije kontrolera, metode aplikacionih servisa i repozitorijuma, kao i konstruktori i metode agregata, domenskih servisa i lokalnih tehničkih stručnjaka. Na primer, "Objava ture je moguća za neobjavljene ture sa adekvatnim opisom i dužinom trajanja i tada se evidentira vreme objave" je jedno ponašanje. Test to ponašanje proverava kroz javnu metodu agregata `Publish`, koju poziva aplikacioni servis. Vrednosni objekat `TransportTime` odbija vreme transporta koje nije pozitivno, ali njega pravi agregat u metodi `AddTransportTime`, pa se to pravilo proverava kroz tu metodu.

Jedinice ponašanja se ugnježdavaju. Objava ture je sposobnost koju nudi metoda agregata. Međutim, objava ture je i metoda kontrolera, koja zatim poziva servis, koji radi sa agregatom i repozitorijumom. Ova šira objava ugnježdava sitniju objavu. Od ugnježdenih jedinica, jedinični test proverava onu koja ispunjava drugi i treći uslov. Za objavu ture to je metoda `Publish` agregata, jer servis i kontroler rade sa bazom podataka.

Primer kršenja drugog uslova vidimo kod infrastrukturnog servisa čiji zadatak je da dobavi podatke od drugog sistema putem HTTP zahteva (konektorska klasa). Test ne kontroliše ni dostupnost tog sistema ni sadržaj njegovog odgovora.

Za kršenje trećeg uslova možemo zamisliti klasu `TourTests` koja, radi kraće pripreme, čuva jednu turu u statičkom polju, pa sve test metode rade sa istim objektom. Test `Tour_is_published_when_all_rules_are_met` objavljuje tu turu. Ako se posle njega izvrši test koji očekuje turu u statusu nacrta, taj test bi pao, što treba da bude znak da je logika poremećena. Međutim, u ovom slučaju je to problem koji je nastao zbog međuzavisnosti između testova.

## Izraz provere

**Izraz provere** (engl. *assertion*) je naredba koja upoređuje dobijenu vrednost sa očekivanom i obara test ako se ne poklapaju. U projektu se provere pišu uz pomoć biblioteke `FluentAssertions`. Provera počinje pozivom metode `Should()` nad vrednošću koja se proverava, a nastavlja se metodom koja iskazuje očekivanje:

```cs
tour.Status.Should().Be(TourStatus.Draft);
tour.TransportTimes.Should().BeEmpty();
creation.Should().Throw<DomainException>();
```

U datom kodu treba uočiti sledeće:

- Izraz provere se čita gotovo kao normalna rečenica. Za prvi primer to je "Status ture treba da bude draft".
- Kada provera ne uspe, poruka o grešci navodi i očekivanu i dobijenu vrednost, pa se uzrok pada često vidi iz izveštaja, bez pokretanja testa u debageru.

## Parametrizovani test

**Parametrizovani test** (engl. *parameterized test*) je test metoda označena atributom `[Theory]` koja se izvršava više puta, gde se sa svakim izvršavanjem postavljaju druge vrednosti parametra. Skup ulaznih vrednosti za jedno izvršavanje se navodi atributom `[InlineData]`. Sledeći primer prikazuje test koji će se tri puta izvršiti:

```cs
[Theory]
[InlineData("")]
[InlineData("               ")]
[InlineData("\n\n\n")]
public void New_tour_requires_a_description(string description)
{
    var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"]);

    creation.Should().Throw<DomainException>();
}
```

Za primer će test okvir izvršiti metodu jednom za svaki `[InlineData]`, gde prvo prosleđuje prazan string, pa string sa puno razmaka i na kraju string sa tri karaktera za nov red. Kada bi test imao više parametara, `[InlineData]` bi sadržalo više vrednosti, po jednu za svaki parametar. Parametri testa tako zamenjuju drugi i treći korak pripreme, kada su argumenti i očekivani ishodi proste vrednosti.
