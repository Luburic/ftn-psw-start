# Konvencije testiranja

Ovaj dokument propisuje kako se pišu testovi modula. Mehanizam je u vlasništvu platformskog tima (projekat `Shared.Tests`), a podaci i testovi su u vlasništvu tima koji je vlasnik modula. Živi primeri su `Identity.Tests` i test projekti modula `Exploration` i `Social`.

Koriste se xUnit i FluentAssertions, zaključan na verziji 7 (poslednja linija pod Apache licencom; prelazak na verziju 8 zahteva prethodni razgovor o licenci). Svaki modul ima jedan test projekat sa direktorijumima `Unit/` i `Integration/`. Jedinični testovi proveravaju ponašanje agregata i domenskih servisa. Integracioni testovi šalju prave HTTP zahteve: `WebApplicationFactory<Program>` podiže host, a test poziva endpointe kroz `HttpClient`.

## Testne baze podataka

Svaki test projekat nasleđuje klasu `ExplorerApiFactory`, koja izvodi ime baze po test projektu (`explorer-test-<modul>`; osnovna konekcija se po potrebi menja promenljivom okruženja `EXPLORER_TEST_DATABASE`), obara i ponovo kreira tu bazu jednom po pokretanju testova i prepušta inicijalizatorima modula da izvrše migracije pri podizanju hosta. Struktura se, dakle, postavlja jednom po pokretanju, a podaci se vraćaju na početno stanje pre svakog testa.

## Podaci za zasejavanje (engl. *seed*)

Svaki test projekat ima direktorijum `Seeds/`: po jedna statička klasa imenovanih instanci za svaki agregat i jedna klasa koja ih objedinjuje kroz svojstvo `All`. Instanca se gradi kroz domen — poziv konstruktora, po potrebi praćen pozivima domenskih metoda u statičkom konstruktoru kada je testu potrebno stanje posle početnog — a stanje je kodirano u imenu (`FortressWalk` je svež nacrt, `PublishedVineyards` je objavljena kroz `Publish()`). Dozvoljeni su samo linearni iskazi: bez grananja, pomoćnih metoda i parametara; varijacija je nova imenovana instanca. Pošto instance prolaze isključivo kroz prave konstruktore i domenske metode, svako zasejano stanje je stanje koje sistem zaista može da dostigne.

Testovi referenciraju zasejane redove preko imenovane instance, npr. `TourSeed.FortressWalk.Id`. Zato agregati generišu identifikator u konstruktoru (`base(Guid.NewGuid())`), nikada kroz EF. Redovi drugog modula dolaze iz seed klasa tog modula; test projekti smeju da referenciraju jedni druge za ovu potrebu.

## Tri kanala

U integracionom testu svaka briga ima tačno jedan kanal, i svaki je jednosmeran: stanje ulazi kroz seedove (`Reseed` je jedini put upisa u bazu), akcija ide kroz HTTP (Act je jedini zahtev koji test šalje), a posmatranje ide kroz bazu (`Factory.CreateContext<TContext>()`, isključivo za čitanje). Stanje se nikada ne priprema pozivanjem drugih endpointa — greška u jednoj funkcionalnosti sme da obori samo testove te funkcionalnosti — i nikada se ne piše kroz kontekst, jer bi to zaobišlo sve domenske invarijante. Ništa strukturno ne sprečava prekršaj (kontekst je dostupan); pravilo se drži konvencijom i pregledom koda.

Konteksti su uvek sveži i kratkotrajni: jedan se otvara u Arrange koraku za početno čitanje, poseban u Assert koraku, svaki u `using` bloku. Ponovna upotreba konteksta preko granice Act koraka vraća zastarele praćene entitete.

## Provere komandi

Prvo se proverava odgovor (statusni kod i vraćeni DTO ako postoji), a zatim se otvara kontekst i proverava upisana posledica — uključujući njeno odsustvo u testovima odbijanja koji stignu do domena. Time testovi komandi ne zavise od upitnih klasa i pokrivaju i komande čije posledice nijedan trenutni upit ne projektuje. Brojevi se proveravaju kao razlike: broj se pročita u Arrange koraku, a u Assert koraku se tvrdi `countBefore + 1` (ili da je nepromenjen).

## Provere upita

Testovi upita ostaju na HTTP nivou — projekcija je ono što se testira — i nikada ne navode broj kao literal, jer svaki upisani popis puca čim bilo ko doda novi seed red. Očekivani brojevi se izvode iz seed klase (`TourSeed.All.Count(...)`; `All` je tipiziran kao niz agregata upravo zbog ovoga) ili se proveravaju oblik i članstvo (`OnlyContain`, `Contain`/`NotContain` po zasejanom identifikatoru).

## Povezivanje

Svaki test projekat ima jednu datoteku `BaseIntegrationTest.cs` koja objedinjuje tri tipa: potklasu fabrike, `[CollectionDefinition("Integration")]` sa fabrikom kao `ICollectionFixture` (jedno podizanje hosta po pokretanju i serijsko izvršavanje — kolekcije su jedinica paralelizma u xUnit-u) i apstraktnu klasu `BaseIntegrationTest` čiji konstruktor poziva `factory.Reseed<TContext>(Seed.All)`, pa svaki test kreće od istovetnog početnog stanja. Direktorijumi integracionih testova prate agregate, sa klasama `<Agregat>CommandTests` i `<Agregat>QueryTests` jedna pored druge.

## Autentifikacija u testovima

Testovi funkcionalnih modula nikada ne diraju modul `Identity`: `ExplorerApiFactory` direktno izdaje JWT tokene razvojnim ključem (`CreateClientFor(userId, role)`), a klasa `WellKnownUsers` sadrži fiksne identifikatore korisnika na koje se seed podaci pozivaju. Samo `Identity.Tests` koristi prave endpointe za registraciju i prijavu.
