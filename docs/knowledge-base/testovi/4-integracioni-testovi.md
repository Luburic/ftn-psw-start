# Integracioni testovi

> **Nacrt.** Struktura lekcije sa beleškama po odeljcima. Svaki odeljak navodi definiciju, problem, primer, šta se uočava, izvor u knjizi i planirani obim.

**Preduslovi:** `testovi/3-koji-kod-zasluzuje-koji-test.md`, `server/1-aspnet/2-kontroleri.md`.
**Ciljevi učenja:** čitalac zna šta integracioni test u projektu izvršava, ume da za novu komandu i novi upit odluči koje scenarije integracioni test pokriva, i ume da pročita pomoćni kod koji platformski tim održava za pokretanje aplikacije i prijavu korisnika u testu.
**Radni primer:** `TourAuthoringCommandTests` i `TourBrowsingQueryTests` iz modula Exploration; `ExplorerApiFactory` i `WellKnownUsers` iz `Shared.Tests`.
**Planirani obim:** oko 1600 reči.

## Uvod (oko 120 reči)

Sidro: lekcija 3 je zaključila da aplikacioni servis, repozitorijum i kontroler dobijaju mali broj integracionih testova. Pitanje: kako takav test izgleda, šta u njemu radi zaista, a šta je zamenjeno, i koje scenarije pokriva.

## Šta je integracioni test (oko 250 reči)

- **Definicija:** integracioni test je test koji ne ispunjava bar jedan uslov jediničnog: proverava više jedinica ponašanja, nije brz ili nije nezavisan od drugih testova. U projektu integracioni test šalje HTTP zahtev pokrenutoj aplikaciji i proverava odgovor i stanje baze.
- **Problem:** jedinični test dokazuje pravilo agregata u vakuumu; ništa ne dokazuje da je agregat učitan, sačuvan i izložen ispravno. Integracioni test prolazi kroz kontroler, aplikacioni servis, agregat, EF Core i bazu, pa hvata greške na svakom spoju, a istovremeno je udaljen od koda, pa preživljava refaktorisanje.
- **Primer:** `Publish_publishes_a_complete_tour` iz `TourAuthoringCommandTests`, uporedo sa istoimenim testom iz `TourTests`.
- **Uočiti:** isti naziv, drugačiji put: prvi poziva metodu, drugi šalje `POST` na `/api/exploration/tours/{id}/publish`; drugi proverava i statusni kod i red u bazi; drugi traži pokrenutu bazu i traje duže.
- **Izvor:** Khorikov 8.1.1, 8.1.2 (prvi deo); xunit.md (uvod u integracione testove).

## Upravljane i neupravljane zavisnosti (oko 250 reči)

- **Definicija:** upravljana zavisnost (engl. *managed dependency*) je sistem van procesa kome pristupa samo naša aplikacija, poput baze podataka. Neupravljana zavisnost (engl. *unmanaged dependency*) je sistem van procesa koji vide i drugi, poput servisa za slanje pošte ili platnog provajdera.
- **Problem:** šta u testu radi zaista, a šta se zamenjuje. Komunikacija sa upravljanom zavisnošću je detalj implementacije, pa se koristi prava instanca i proverava krajnje stanje. Komunikacija sa neupravljanom zavisnošću je vidljiva spolja, pa se zamenjuje i proverava se sam poziv.
- **Primer:** projekat danas ima samo bazu, pa nema zamena; test radi sa pravim PostgreSQL serverom, a baza u memoriji se ne koristi jer bi test prolazio na jednom, a padao na drugom sistemu.
- **Uočiti:** modul Payment će dobiti prvu neupravljanu zavisnost; do tada ovaj odeljak ima jedan zaključak, prava baza.
- **Izvor:** Khorikov 8.2.1, 8.2.3, 10.3.3.

## Šta integracioni test pokriva (oko 400 reči)

Kičma odeljka: tri vrste scenarija za komandu i dve za upit.

- **Problem:** integracioni test je skup, pa se ne piše za svako pravilo agregata; pravila su već pokrivena jediničnim testovima.
- **Komanda, srećan put:** najduži uspešan scenario koji prolazi kroz sve slojeve. Primer `Create_stores_a_draft_tour`.
- **Komanda, rubni slučajevi van domena:** stanja koja jedinični test agregata ne može da dosegne, jer nastaju u kontroleru i cevovodu. Primer `Create_requires_authentication` (401), `Create_requires_the_explorer_role` (403), `Publish_rejects_another_authors_tour` (404).
- **Komanda, jedno odbijanje domena:** jedan test kojim se proverava da izuzetak iz agregata postaje odgovor 400 i da baza ostaje nepromenjena. Primer `Create_rejects_a_blank_name`. Ostala pravila ostaju u `TourTests`.
- **Upit:** članstvo i projekcija (`GetPublished_returns_only_published_tours`) i straničenje (`GetPublished_pages_the_results`).
- **Uočiti:** svaki scenario je zaseban test sa jednom akcijom; test upita ne menja ništa; test komande čita bazu posle odgovora, jer telo odgovora ne dokazuje da je upis obavljen.
- **Izvor:** Khorikov 8.1.3, 8.3.1, 8.5.4, 10.5.1.

## Pokretanje aplikacije u testu (oko 350 reči)

- **Definicija:** `WebApplicationFactory<Program>` pokreće celu aplikaciju u procesu testa, a `CreateClient()` vraća `HttpClient` čiji zahtevi idu pravo u nju, bez mreže. Deljeni objekat (engl. *fixture*) je objekat koji test okvir pravi jednom i daje većem broju testova. Kolekcija je imenovana grupa test klasa koje dele deljeni objekat i izvršavaju se jedna za drugom.
- **Problem:** pokretanje aplikacije traje sekunde, pa se ne ponavlja za svaki test; deli se kroz kolekciju, koja je u xUnit jedinica paralelizma.
- **Primer:** `ExplorerApiFactory` iz `Shared.Tests` (nasleđuje fabriku, izvodi ime testne baze iz imena projekta, briše je i pravi iznova jednom po pokretanju) i tri klase iz `BaseIntegrationTest.cs` modula Exploration.
- **Uočiti:** svaki test projekat ima svoju bazu (`explorer-test-exploration`), pa se moduli ne ometaju; migracije se primenjuju kada se aplikacija podigne; `[CollectionDefinition("Integration")]` sa `ICollectionFixture<ExplorationApiFactory>` daje jedno podizanje po projektu; `[Collection("Integration")]` na osnovnoj klasi uvodi svaku test klasu u tu kolekciju; promenljiva okruženja `EXPLORER_TEST_DATABASE` menja pristupne podatke.
- **Izvor:** xunit.md (Deljeni objekat, Kolekcija testova, Pokretanje aplikacije u testu, Testna baza modula, Povezivanje); Khorikov 3.3.3 (izuzetak za osnovnu klasu).

## Prijavljeni korisnik u testu (oko 150 reči)

- **Problem:** većina krajnjih tačaka traži prijavljenog korisnika, a registracija kroz modul Identity bi svaki test vezala za tuđu funkcionalnost.
- **Primer:** `Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer")` u testu i `WellKnownUsers` iz `Shared.Tests`.
- **Uočiti:** fabrika sama potpisuje token razvojnim ključem; funkcionalni moduli korisnika poznaju samo kroz identifikator, pa korisnik ne mora da postoji u bazi; `WellKnownUsers` su stalni identifikatori na koje se pozivaju i početni podaci (lekcija 5).
- **Izvor:** xunit.md (Prijavljeni korisnik u testu).

## Organizacija testova modula (oko 100 reči)

- Direktorijum `Unit/` sa testovima agregata, direktorijum `Integration/` sa početnim podacima, datotekom `BaseIntegrationTest.cs` i po jednim direktorijumom za grupu slučajeva korišćenja. Za svaku komandnu klasu jedna klasa `<Grupa>CommandTests`, za svaku upitnu klasu jedna `<Grupa>QueryTests`.
- Pokretanje: `dotnet test` iz `backend`, uz lokalni PostgreSQL; ista komanda u kontinualnoj integraciji.
- **Izvor:** xunit.md (Organizacija testova modula, Pokretanje testova).

## Van opsega

Početni podaci, tri kanala i provere nad bazom (lekcija 5); mock objekti i test neupravljane zavisnosti (buduća lekcija uz modul Payment); testovi kroz ceo sistem (engl. *end-to-end*); testiranje logovanja.
