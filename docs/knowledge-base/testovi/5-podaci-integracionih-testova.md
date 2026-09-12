# Podaci integracionih testova

> **Nacrt.** Struktura lekcije sa beleškama po odeljcima. Svaki odeljak navodi definiciju, problem, primer, šta se uočava, izvor u knjizi i planirani obim.

**Preduslovi:** `testovi/4-integracioni-testovi.md`, `server/2-arhitektura-modula/3-infrastrukturni-sloj/5-jedinica-posla.md`.
**Ciljevi učenja:** čitalac razume kako integracioni testovi dele jednu bazu a ostaju nezavisni, ume da doda novu imenovanu instancu u početne podatke modula i ume da napiše proveru koju novi red u početnim podacima ne obara. Ovo je lekcija o tome kako skup testova preživljava rast modula.
**Radni primer:** `TourSeed`, `ExplorationSeed` i `BaseIntegrationTest` iz modula Exploration; `Reseed` i `CreateContext` iz `ExplorerApiFactory`; `Create_stores_a_draft_tour` i `GetPublished_returns_only_published_tours`.
**Planirani obim:** oko 1500 reči.

## Uvod (oko 120 reči)

Sidro: u lekciji 4 test `Publish_publishes_a_complete_tour` šalje zahtev za turu `TourSeed.PublishableRiverside.Id`. Pitanje: odakle ta tura u bazi, šta se dešava sa njom posle testa i zašto naredni test zatiče istu bazu kao i prethodni. Baza je jedina zavisnost koju testovi dele, pa je ona jedini put kojim jedan test može da pokvari drugi.

## Poznato stanje pre svakog testa (oko 300 reči)

- **Definicija:** početni podaci (engl. *seed*) su skup redova koji se upisuje u testnu bazu pre svakog testa. `Reseed` prazni sve tabele modula i upisuje početne podatke.
- **Problem:** test koji čita mora unapred da zna šta je u bazi; test koji piše ne sme da ostavi trag narednom. Čišćenje na kraju testa se preskače kada test pukne ili se prekine u debageru; zato se čisti na početku, a posebna faza čišćenja ne postoji. Testovi u kolekciji se izvršavaju jedan za drugim, pa dva testa nikada ne dele bazu u istom trenutku.
- **Primer:** konstruktor `BaseIntegrationTest` sa pozivom `Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All)`.
- **Uočiti:** konstruktor osnovne klase se izvršava pre svakog testa, jer test okvir pravi novu instancu po testu (lekcija 1); struktura baze se postavlja jednom po pokretanju, a podaci pre svakog testa; `Reseed` je jedini put kojim podaci ulaze u bazu mimo krajnje tačke pod testom.
- **Izvor:** xunit.md (Početni podaci, Povezivanje); Khorikov 10.3.1, 10.3.2.

## Početni podaci koji preživljavaju rast (oko 400 reči)

- **Problem:** početni podaci rastu sa svakom novom funkcionalnošću i svakim novim testom. Ako svaki test upisuje svoje redove, priprema se ponavlja i testovi postaju dugi; ako se podaci grade pomoćnim metodama sa parametrima i granjanjem, klasa početnih podataka postaje program koji se sam mora testirati.
- **Primer:** skraćen `TourSeed` sa `FortressWalk`, `PublishableRiverside`, `PublishedVineyards`, statičkim konstruktorom i spiskom `All`; `ExplorationSeed.All`.
- **Uočiti:**
  - Jedna statička klasa po agregatu, sa imenovanim instancama; ime beleži stanje (`PublishedVineyards` je objavljena, `FortressWalk` je sveža).
  - Instanca nastaje kroz konstruktor i domenske metode, nikada kroz zaobilaženje pravila, pa je svako posejano stanje ono koje sistem zaista može da dostigne.
  - Samo linearni iskazi: bez grananja, bez pomoćnih metoda, bez parametara. Varijacija je nova imenovana instanca. Ovo je cena koja se plaća da klasa ostane podaci, a ne kod.
  - Test se poziva na red imenom (`TourSeed.FortressWalk.Id`), pa agregat generiše identifikator u konstruktoru, a ne baza.
  - `All` je tipiziran nizom agregata, pa se iz njega izvode očekivani brojevi (naredni odeljci).
  - Redovi drugog modula dolaze iz klasa početnih podataka tog modula.
- **Izvor:** xunit.md (Početni podaci); Khorikov 10.4.1 (ideja izdvajanja pripreme, bez imena obrasca).

## Tri kanala (oko 300 reči)

- **Definicija:** integracioni test sa sistemom komunicira kroz tri jednosmerna kanala: stanje ulazi kroz početne podatke, akcija ide kroz jedan HTTP zahtev, ishod se posmatra čitanjem baze kroz svež kontekst.
- **Problem:** ako test priprema stanje pozivom druge krajnje tačke, greška u toj funkcionalnosti obara i testove ove; ako test upisuje kroz kontekst, zaobilazi domenska pravila i može da poseje stanje koje sistem ne može da dostigne.
- **Primer:** `Create_stores_a_draft_tour`: kontekst u pripremi za broj redova, jedan `PostAsJsonAsync`, poseban kontekst u proveri.
- **Uočiti:** `CreateContext` vraća svež kontekst, uvek u `using` bloku i samo za čitanje; kontekst otvoren pre akcije se ne čita posle nje, jer EF Core prati jednom učitane objekte i vratio bi zastarelo stanje, pa svaki korak testa ima svoj kontekst kao što svaka poslovna operacija u produkciji ima svoju jedinicu posla; ništa strukturno ne sprečava kršenje, pravilo drži pregled koda.
- **Izvor:** xunit.md (Tri kanala integracionog testa); Khorikov 10.2.2.

## Provere koje novi red ne obara (oko 300 reči)

- **Problem:** provera `tours.Should().HaveCount(3)` prolazi danas i pada čim bilo ko doda novu instancu u `TourSeed`, iako se funkcionalnost nije promenila. To je lažni pozitiv iz lekcije 2, ovog puta izazvan podacima.
- **Primer:** provere iz `Create_stores_a_draft_tour` (`tourCountBefore + 1`, `Count().Should().Be(tourCountBefore)` u testu odbijanja) i iz `GetPublished_returns_only_published_tours` (`OnlyContain`, `Contain` i `NotContain` po identifikatoru, `TotalCount` iz `TourSeed.All.Count(...)`).
- **Uočiti:** test komande prvo proverava odgovor, zatim upisanu posledicu, uključujući njeno odsustvo kod odbijanja; brojevi se proveravaju kao razlike u odnosu na stanje pročitano u pripremi; test upita ostaje na HTTP nivou jer je projekcija ono što testira, a očekivani broj izvodi iz klase početnih podataka ili proverava članstvo po imenovanoj instanci.
- **Izvor:** xunit.md (Provere komandi, Provere upita).

## Van opsega

Paralelno izvršavanje integracionih testova, baze u memoriji (pomenuto u lekciji 4), migracije i referentni podaci (lekcija o migracijama), pomoćne metode za korak akcije i proširenja za provere.
