# Struktura serverske aplikacije (backend)

Serverska aplikacija je modularni monolit izgrađen na platformi ASP.NET Core. Ceo sistem se prevodi u jednu aplikaciju i koristi jednu PostgreSQL bazu podataka, pri čemu svaki modul ima sopstvenu šemu u toj bazi.

Deo koda je u vlasništvu platformskog tima, a deo u vlasništvu timova koji razvijaju funkcionalne module. Vlasništvo je navedeno uz svaku stavku. Kod koji je u vlasništvu platformskog tima ostali timovi ne menjaju, već platformskom timu prijavljuju potrebu za izmenom.

## Explorer.slnx

Šredstavlja rešenje (engl. *solution*) koje okuplja sve projekte serverske aplikacije.

Datoteka je u vlasništvu platformskog tima. Menja se samo kada se u rešenje dodaje nov projekat ili kada se postojeći uklanja.

## Directory.Build.props

Sadrži podešavanja prevođenja (engl. *build*) koja važe za sve projekte u rešenju. U njoj je definisana ciljna verzija platforme (.NET 10), uključena je provera referenci koje mogu biti `null` i podešeno je da se upozorenja prevodioca tretiraju kao greške. Zahvaljujući ovoj datoteci, pojedinačni projekti ne moraju da ponavljaju ista podešavanja.

Datoteka je u vlasništvu platformskog tima. Menja se samo kada se donosi odluka koja važi za ceo sistem.

## Directory.Packages.props

Sadrži spisak svih spoljnih biblioteka (NuGet paketa) koje rešenje koristi, zajedno sa njihovim verzijama. Verzije se određuju isključivo na ovom mestu, pa pojedinačni projekti navode samo ime paketa, bez verzije. Time se sprečava da različiti moduli koriste različite verzije iste biblioteke.

Datoteka je u vlasništvu platformskog tima. Menja se kada se u sistem uvodi nova biblioteka ili kada se postojeća prevodi na novu verziju. Uvođenje nove biblioteke je odluka koja se donosi u dogovoru sa platformskim timom, a ne samostalno.

## Host.Api

Sadrži projekat koji predstavlja ulaznu tačku i kompozicioni koren cele aplikacije. To je jedini projekat koji se pokreće. U njemu se rešava registracija svih modula i osnovna konfiguracija procesa obrade HTTP odgovora i zahteva (provera JWT tokena, middleware koji izuzetke pretvara u HTTP odgovore). Tu su i konfiguracione datoteke `appsettings.json` i `appsettings.Development.json`, u kojima su podaci za povezivanje sa bazom i ključ za potpisivanje tokena.

Projekat je u vlasništvu platformskog tima. Menja se kada se u sistem dodaje nov modul koji treba registrovati ili kada se menja ponašanje koje važi za celu aplikaciju, na primer obrada grešaka ili provera tokena.

## Host.Tests

Sadrži projekat sa arhitektonskim testovima. Ti testovi proveravaju da li projekti poštuju pravila zavisnosti (npr. da domenski sloj ne zavisi od tehnoloških detalja i da jedan modul ne pristupa unutrašnjosti drugog modula). Testovi se izvršavaju i lokalno i u sistemu kontinualne integracije, pa narušavanje pravila obara izgradnju.

Projekat je u vlasništvu platformskog tima. Menja se samo kada se menja arhitektura. Ako ovi testovi prijave grešku, ispravlja se kod, a ne test.

## Shared

Sadrži zajednički kod koji koriste svi moduli. Podeljen je na tri projekta:
- `Shared.Api` sadrži pomoćni kod za kontrolerski sloj (npr. metodu `GetUserId`, kojom kontroler čita identifikator prijavljenog korisnika iz tokena).
- `Shared.Domain` sadrži klase `Entity` i `AggregateRoot`, koje nasleđuju domenske klase (vrednosni objekat se modeluje kroz C# record). Izuzeci `DomainException` i `NotFoundException` domen prijavljuje narušavanje pravila i nepostojanje traženog podatka.
- `Shared.Infrastructure` sadrži pomoćni kod za sloj infrastrukture (npr. metoda `AddModuleDbContext`, koja registruje bazu podataka modula tako da modul dobije sopstvenu šemu i sopstvenu tabelu istorije migracija).

Sva tri projekta su u vlasništvu platformskog tima. Menjaju se samo kada se pojavi potreba koja je zaista zajednička za više modula. Premeštanje koda u zajedničke projekte je odluka koja se donosi u dogovoru sa platformskim timom, jer svaki dodatak ovde postaje zavisnost svih modula.

## Modules

Ovaj direktorijum sadrži module aplikacije. Ovde se odvija najveći deo rada timova. Svaki tim je vlasnik tačno jednog direktorijuma i menja isključivo njega.

### Identity

Modul `Identity` je izuzetak od strukture ostalih modula. To je projekat u vlasništvu platformskog tima, zadužen za registraciju i prijavu korisnika i za izdavanje JWT tokena. Ostali moduli ga ne referenciraju, već korisnika pamte samo preko njegovog identifikatora. Uz njega postoji i projekat `Identity.Tests`, koji služi kao ugledni primer integracionih testova.

### Funkcionalni moduli

Funkcionalni moduli su `Exploration`, `Games`, `Social` i `Payment`. Svaki od njih ima istovetnu strukturu od šest projekata:
- `<Ime>.Api` sadrži kontrolere koji primaju HTTP zahtev, pozivaju jednu metodu aplikacionog sloja i vraćaju HTTP odgovor.
- `<Ime>.Application` sadrži aplikacione servise koji opisuju kako se koordinišu domenski objekti i tehničke sposobnosti da se ispuni slučaj korišćenja. Tu su i DTO klase, čije instance prihvataju i vraćaju aplikacioni servisi, profili za transliranje DTO u domenski objekat i obratno, kao i interfejsi od infrastrukturnih servisa.
- `<Ime>.Contracts` sadrži interfejs koji modul nudi drugim modulima. To je jedini deo modula koji drugi moduli smeju da referenciraju. Sadrži samo interfejse i proste tipove. Svaka izmena ovog projekta je dogovor između dva tima, pa se najavljuje, a ne izvodi tiho.
- `<Ime>.Domain` sadrži domenske objekte (koren agregata, entiteti, vrednosni objekti) i domenske servise. Ovde se implementiraju domenski koncepti i pravila.
- `<Ime>.Infrastructure` sadrži implementacije infrastrukturnih servisa, poput repozitorijuma, konektorskih klasa i tehničkih stručnjačkih klasa koje koriste biblioteke. U njemu su `DbContext`, EF konfiguracije, migracije, implementacije repozitorijuma i upita.
- `<Ime>.Tests` sadrži testove modula, podeljene na direktorijume `Unit` i `Integration`. Jedinični testovi proveravaju ponašanje agregata i domenskih servisa, a integracioni testovi šalju prave HTTP zahteve i proveravaju rad severske aplikacije u interakciji sa testnom bazom podataka.

Ovih šest projekata menja tim koji je vlasnik modula, pri svakom razvoju nove funkcionalnosti.
