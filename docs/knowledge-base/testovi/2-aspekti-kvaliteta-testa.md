U prethodnoj lekciji smo videli test `Publishes`, koji gradi turu, objavljuje je i proverava status i vreme objave. Zamislimo dva druga testa iste klase:
1. Provera da tura posle kreiranja ima ime koje joj je dato u konstruktoru.
2. Provera `Publish` baca izuzetak kada se pozove nad objavljenom turom i da taj izuzetak sadrži poruku sa tačno određenim tekstom.

Dati testovi nisu preterano korisni. Pitanje je kako da procenimo koje automatske testove vredi pisati. Ovo će nas služiti kod pisanja naših testova, kao i pri pregledu testova koje je napisao neko drugi.

## Cilj testiranja

Cilj automatskog testiranja je **održiv rast** projekta, stanje u kome izmena koda posle godinu dana rada košta koliko i na početku. Bez testova svaka izmena rizikuje **regresiju** (engl. *regression*), grešku u funkcionalnosti koja je ranije radila. Tim tada usporava, jer posle svake izmene proverava ručno, ili ne usporava i isporučuje regresije.

Sa ovim ciljem, deluje da bi najbolje bilo da testovi testiraju sav naš kod. Tada bismo maksimizovali metriku koju zovemo **pokrivenost koda** (engl. *code coverage*), što je procenat redova koje testovi izvrše. Ova metrika je parcijalno korisna. Niska pokrivenost je siguran znak da nešto nije testirano, ali visoka pokrivenost nije garancija kvaliteta. Pokrivenost samo beleži da je red izvršen, a ne da testirana jedinica radi kako treba. Test bez ijedne provere ili sa glupavim proverama daje istu pokrivenost kao test sa kvalitetnim proverama. Dakle, vrednost testa je vezana za kvalitet njegovih provera.

Sa druge strane, testovi unose trošak. Pored što testiraju kod, testovi su isto kod. Pišu se, čitaju, menjaju kada se menja kod koji proveravaju, i ponekad padaju bez razloga. Svaki test ima trošak.

Iz navedenog, testovi treba da donose više vrednosti nego troškova, a za to moramo da pišemo kvalitetne testove koji ispunjavaju sledeće karakteristike:
- Pružaju dobru zaštitu od regresija
- Otporni su na refaktorisanje jedinice ponašanja koju testiraju
- Brzo se izvršavaju
- Laki su za održavanje

### Zaštita od regresija

**Zaštita od regresija** je sposobnost testa da otkrije grešku u sistemu. Test koji ispituje rad jedne male metode ili pravi trivijalne provere će retko otkriti grešku. Sledeći kod prikazuje primer takvog trivijalnog testa:

```cs
[Fact]
public void New_tour_has_appropriate_name()
{
  var tour = new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, ["istorija"]);

  tour.Name.Should().Be("Šetnja tvrđavom");
}
```

Logika konstruktora klase `Tour` je da direktno postavi vrednosti svojstava spram argumenata. Test izvršava taj trivijalan konstruktor i proverava ishod direktne dodele vrednosti svojstva. Teško je zamisliti izmenu koda koja bi tu dodelu pokvarila, a da je programer ne primeti odmah.

Sa druge strane, analiziramo test koji aktivira metodu koja poziva mnoštvo objekata, a zatim proverava sve bitne ishode izvršenog ponašanja:

```cs
[Fact]
public async Task AddTransportTime_stores_the_time_on_the_tour()
{
  var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
  var request = new TransportTimeDto(TransportMode.Walking, 120);

  var response = await client.PostAsJsonAsync(
    $"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times", request);

  response.StatusCode.Should().Be(HttpStatusCode.NoContent);
  using var assertContext = Factory.CreateContext<ExplorationDbContext>();
  var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
  stored.TransportTimes.Should().ContainSingle(
    time => time.Transport == TransportMode.Walking && time.Minutes == 120);
}
```

Dati kod predstavlja integracioni test koji ćemo kasnije bolje upoznati. Za sada treba uočiti sledeće:
- Poziv `PostAsJsonAsync` pravi HTTP POST zahtev koji će aktivirati serversku aplikaciju, sve middleware komponente, odgovarajući kontroler, a onda kroz njega servis, repozitorijum, `Tour` agregat i jedinicu posla (`UnitOfWork`), pre nego što se vrati odgovor u vidu HTTP odgovora.
- Provera statusnog koda utvrđuje da li je stigao odgovarajući HTTP odgovor, a zatim gleda da li se sadržaj baze podataka izmenio na očekivan način. Sa prvom proverom osiguravamo da bi klijentska aplikacija dobila ono što očekuje, a sa drugom da se desila transformacija sistema koju smo očekivali.
- Ako bilo koja karika u lancu ima grešku, ovaj test će je uhvatiti.

### Otpornost na refaktorisanje

**Refaktorisanje** je izmena strukture koda koja ne menja njegovo ponašanje. **Otpornost na refaktorisanje** je mera koliko test preživljava izmene testirane jedinice ponašanja bez da mora kod testa da se menja i bez da proizvodi lažne pozitivne rezultate (pad testa iako funkcionalnost koju test proverava radi ispravno). Lažni pozitivi imaju dve posledice. Prvo, tim se navikava da pali testovi ne znače 'greška' i prestaje da ih čita. Drugo, tim izbegava refaktorisanje, jer svaka promena obara testove koje zatim treba popravljati. Skup testova sa mnogo lažnih pozitiva vremenom prestaje da se pokreće.

Testovi koji testiraju malu jedinicu ponašanja (npr. metodu vrednosnog objekta), prirodno su spregnuti za sitnu površinu, gde je visoka verovatnoća da će test morati da se modifikuje ako se refaktoriše jedinica ponašanja. Na ranijem primeru `New_tour_has_appropriate_name`, izmena naziva svojstva `Name` u `Title` zahteva korekciju testa. Naspram toga, `AddTransportTime_stores_the_time_on_the_tour` je spregnut samo sa HTTP ugovorom i sadržajem baze, a ne sa načinom na koji je logika između njih napisana, te je visoka verovatnoća da refaktorisanje te logike neće zahtevati izmenu testa.

Prethodna karakteristika nam govori da je korisno da testiramo apstraktnije metode jer ćemo ređe morati da menjamo njihove testove. Uz to, korisno je da pazimo na ishode testa koje proveravamo, gde želimo da proverimo najbitnije ishode, a ne svaki detalj. Sledeći kod prikazuje test koji proverava detalj implementacije:

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

### Brzina

**Brzina** je mera koliko brzo se test izvršava. Spor test se retko pokreće, pa greška koju bi otkrio živi duže. Test agregata traje milisekunde i ne traži ništa osim koda. Test koji šalje HTTP zahtev aplikaciji sa bazom traje sekunde i traži pokrenut PostgreSQL server.

### Lakoća održavanja

**Lakoća održavanja** (engl. *maintainability*) je mera koliko je test lako razumeti i pokrenuti. Komplikovan kod testa otežava rad i povećava verovatnoću da će test proveriti pogrešan ishod.

### Ocena kvaliteta testa

Kvalitet testa se ocenjuje kroz četiri ocene: zaštitu od regresija, otpornost na refaktorisanje, brzinu i lakoću održavanja. Nula na bilo kojoj oceni čini test bezvrednim.

Nijedan test nema najviše ocene na svemu. Zaštita od regresija i brzina se često sukobe, jer test koji prolazi kroz više koda otkriva više grešaka, ali i traje duže. Lakoću održavanja težimo da održimo na najvišem nivou u svakom testu, jer se dobija načinom pisanja, a ne izborom šta se testira.

Sledeća tabela ocenjuje dva testa iz ove lekcije i test iz prethodne:

| Test | Zaštita od regresija | Otpornost na refaktorisanje | Brzina | Lakoća održavanja |
|---|---|---|---|---|
| `New_tour_has_appropriate_name` | nikakva | umerena | visoka | visoka |
| `Publish_rejects_an_already_published_tour` sa proverom poruke | niska | niska | visoka | visoka |
| `AddTransportTime_stores_the_time_on_the_tour` | visoka | visoka | umerena | umerena (kada naučimo šta kod radi) |

U datoj tabeli treba uočiti sledeće:

- Prvi test je beskoristan jer proverava trivijalan ishod
- Drugi test proverava korektan ishod, a niska je zaštita od regresije samo zato što proverava usko ponašanje. Zaštita od regresija za Publish metodu agregata je relativno visoka, ali je relativno niska za Publish funkcionalnost sistema (što uključuje i kontroler, servis, repozitorijum...). Ovo nije mana, posebno ne kod jediničnih testova koji se fokusiraju na male jedinice ponašanja. Otpornost na refaktorisanje je niska, delom jer je testirana funkcija sitna, ali najviše jer se nepotrebno proverava poruka izuzetka.
- Glavna mana integracionog testa je brzina izvršavanja, kao i lakoća održavanja. Pri tom, sam kod testa je relativno lako održavati (kada ga naučimo), već je teže održavati podatke u testnoj bazi podataka, što ćemo videti kasnije.
