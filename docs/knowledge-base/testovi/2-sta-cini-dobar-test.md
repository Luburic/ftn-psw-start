# Šta čini dobar test

U prethodnoj lekciji smo videli test `Publish_publishes_a_complete_tour`, koji gradi turu, objavljuje je i proverava status i vreme objave. Zamislimo dva druga testa iste klase. Prvi proverava da tura posle kreiranja ima ime koje joj je dato u konstruktoru. Drugi proverava tačan tekst poruke kojom `Publish` odbija već objavljenu turu. Oba testa prolaze, oba se pokreću i nijedan ne vredi.

Ova lekcija daje merila kojima se za dati test odlučuje da li ga vredi zadržati. Merila važe za svaki automatski test, a primenjuju se najčešće pri pregledu testova koje je napisao neko drugi.

## Cilj testiranja

Cilj automatskog testiranja je **održiv rast** projekta, stanje u kome izmena koda posle godinu dana rada košta koliko i na početku. Bez testova svaka izmena rizikuje **regresiju** (engl. *regression*), grešku u funkcionalnosti koja je ranije radila. Tim tada usporava, jer posle svake izmene proverava ručno, ili ne usporava i isporučuje regresije.

Testovi su i sami kod. Pišu se, čitaju, menjaju kada se menja kod koji proveravaju, i ponekad padaju bez razloga. Svaki test ima trošak i vrednost i zadržava se samo ako je vrednost veća od troška. Cilj nije što više testova, nego skup testova u kome svaki test nešto štiti.

Iz istog razloga **pokrivenost koda** (engl. *code coverage*), procenat redova koje testovi izvrše, ne meri kvalitet testova. Pokrivenost beleži da je red izvršen, a ne da je njegov ishod proveren. Test bez ijedne provere daje istu pokrivenost kao test sa proverama. Niska pokrivenost je siguran znak da nešto nije testirano, dok visoka pokrivenost ne dokazuje ništa.

## Zaštita od regresija

**Zaštita od regresija** (engl. *protection against regressions*) je mera verovatnoće da test otkrije grešku. Raste sa količinom i složenošću koda koji test izvršava, uključujući i kod biblioteka kroz koje taj kod prolazi.

Kod bez grananja i bez domenskog pravila ne može da sadrži grešku koju bi test otkrio. Sledeći kod prikazuje test takvog koda:

```cs
[Fact]
public void Creation_keeps_the_name()
{
    var tour = new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, ["istorija"]);

    tour.Name.Should().Be("Šetnja tvrđavom");
}
```

U datom kodu treba uočiti sledeće:

- Test izvršava jednu dodelu u konstruktoru. Nema izmene koda koja bi tu dodelu pokvarila, a da je programer ne primeti odmah.
- Test `Publish_requires_a_transport_time` iz prethodne lekcije izvršava pravilo koje sutra neko može da izmeni pogrešno, na primer da uslov `_transportTimes.Count == 0` zameni uslovom `_transportTimes.Count < 0`. Taj test ima zaštitu od regresija, ovaj nema.
- Trošak oba testa je isti. Piše se, čita se i održava se.

## Otpornost na refaktorisanje

**Refaktorisanje** (engl. *refactoring*) je izmena koda koja ne menja njegovo ponašanje. **Otpornost na refaktorisanje** je mera koliko test preživljava takve izmene bez pada. **Lažni pozitiv** (engl. *false positive*) je pad testa iako funkcionalnost koju test proverava radi ispravno.

Lažni pozitivi imaju dve posledice. Prvo, tim se navikava da pali testovi ne znače grešku i prestaje da ih čita. Drugo, tim izbegava refaktorisanje, jer svaka promena obara testove koje zatim treba popravljati. Skup testova sa mnogo lažnih pozitiva vremenom prestaje da se pokreće.

Uzrok lažnih pozitiva je sprega testa sa **detaljima implementacije** (engl. *implementation details*), načinom na koji je ponašanje ostvareno. Sledeći kod prikazuje test koji proverava detalj implementacije:

```cs
[Fact]
public void Publish_rejects_an_already_published_tour()
{
    var tour = CreateTour(LongDescription);
    tour.AddTransportTime(TransportMode.Car, 30);
    tour.Publish();

    var publishing = () => tour.Publish();

    publishing.Should().Throw<DomainException>()
        .WithMessage("The tour is already published.");
}
```

U datom kodu treba uočiti sledeće:

- Test proverava tekst poruke izuzetka. Ako neko poruku prepravi, na primer doda identifikator ture, tura se i dalje ispravno odbija, a test pada.
- Tekst poruke je detalj implementacije. Ponašanje je da se druga objava odbija, i to ponašanje aplikacioni servis prepoznaje po vrsti izuzetka, ne po tekstu.
- Test istog imena iz klase `TourTests` proverava samo da je izuzetak vrste `DomainException` izbačen. On pada samo kada se ponašanje promeni.

Test proverava ishod, a ne korake. Test posmatra agregat kao što ga posmatra aplikacioni servis, kroz javne metode i svojstva, i ne zna kako je metoda napisana. Kada se test piše tako, otpornost na refaktorisanje je obezbeđena. Kada test proverava korake, otpornosti nema. Između ta dva slučaja nema prelaza, pa se ova osobina ne razmenjuje za druge.

## Brzina i održivost

**Brzina** (engl. *fast feedback*) je mera koliko brzo se test izvršava. Spor test se retko pokreće, pa greška koju bi otkrio živi duže. Test agregata traje milisekunde i ne traži ništa osim koda. Test koji šalje HTTP zahtev aplikaciji sa bazom traje sekunde i traži pokrenut PostgreSQL server.

**Održivost** (engl. *maintainability*) je mera koliko je test lako razumeti i pokrenuti. Dug test se ne čita, pa se popravlja nasumice. Test sa spoljnim zavisnostima ne radi bez njih, pa se preskače kada zavisnost nije pri ruci.

Obe vrste testova imaju mesto u projektu, a razlika u brzini je razlog da se većina ponašanja proverava na nivou agregata, a manji broj kroz celu aplikaciju. Koji kod dobija koju vrstu testa razmatra naredna lekcija.

## Vrednost testa

Vrednost testa je proizvod četiri ocene: zaštite od regresija, otpornosti na refaktorisanje, brzine i održivosti. Proizvod znači da nula na bilo kojoj oceni čini test bezvrednim, bez obzira na ostale.

Nijedan test nema najviše ocene na svemu. Zaštita od regresija i brzina se razmenjuju, jer test koji prolazi kroz više koda otkriva više grešaka, ali i traje duže. Otpornost na refaktorisanje i održivost se ne razmenjuju, nego se drže na najvišem nivou u svakom testu, jer se dobijaju načinom pisanja, a ne izborom šta se testira.

Sledeća tabela ocenjuje dva testa iz ove lekcije i test iz prethodne po prve tri ocene, jer je održivost sva tri testa ista:

| Test | Zaštita od regresija | Otpornost na refaktorisanje | Brzina | Vrednost |
|---|---|---|---|---|
| `Creation_keeps_the_name` | nula | visoka | visoka | nula |
| `Publish_rejects_an_already_published_tour` sa proverom poruke | visoka | nula | visoka | nula |
| `Publish_publishes_a_complete_tour` | visoka | visoka | visoka | visoka |

U datoj tabeli treba uočiti sledeće:

- Prva dva testa imaju nulu na po jednoj oceni, i to je dovoljno da ne vrede. Nije bitno što su brzi i što prolaze.
- Pri pregledu testa se prvo traži nula. Pitanja su dva: da li test proverava nešto što ne može da bude pogrešno i da li proverava korake umesto ishoda. Test bez nule se zatim ocenjuje po tome koliko koda štiti i koliko brzo.
