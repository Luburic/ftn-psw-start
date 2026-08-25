# Arhitektonski testovi

## Problem koji rešavamo

Ovaj sistem počiva na pravilima o tome ko sme da zavisi od koga. Domenski model ne sme da
zna za bazu podataka. Endpoint ne sme direktno da pristupi klasi `DbContext`. Jedan modul
ne sme da poseže za unutrašnjošću drugog modula. Deo ovih pravila sprovodi sâm kompajler:
projekat koji nema referencu na EF Core fizički ne može da ga koristi. Ali referenca
između projekata se dodaje jednom linijom u `.csproj` datoteci, i tu liniju kompajler ne
brani. Pravilo koje je važilo mesecima nestaje jednom izmenom.

Arhitektura se ne narušava krupnim odlukama, već malim prečicama koje u trenutku deluju
razumno. Rok je blizu, podatak koji vam treba nalazi se u tabeli drugog modula, a
"ispravan" put kroz njegov javni interfejs deluje kao nepotrebna procedura. Prečica se
napravi, funkcionalnost proradi. Cena stiže kasnije i pada na nekog drugog: drugi tim
izmeni svoju šemu baze i vaš kôd prestane da radi, a niko ne razume zašto, jer nigde u
javnom opisu vašeg modula ne piše da od njihovog zavisite.

Ovaj problem jednako pogađa ljude i programerske agente, jer i jedni i drugi uče pravila
na isti način — čitanjem koda koji već postoji. Student oponaša obrasce koje vidi u
postojećim modulima; agent radi isto, samo brže i u većem obimu. Jedno kršenje pravila
koje preživi pregled koda postaje primer koji sledeći autor kopira. Narušavanje se zato
gomila: drugu prečicu je lakše opravdati nego prvu, jer presedan već postoji. Pravilo
koje živi samo u dokumentaciji je molba. Da bi važilo, mora da ga proverava mašina, pri
svakoj izmeni, bez izuzetka.

## Opšte rešenje

Arhitektonski test pretvara pravilo o zavisnostima u običan test, koji obara build kada
je pravilo prekršeno.

Mehanizam je jednostavan. Kada se C# kôd prevede, uz svaki tip se u prevedenom projektu
čuvaju metapodaci o njegovim zavisnostima: koje klase nasleđuje, kog tipa su njegova
polja i parametri, koje metode poziva. Biblioteka za arhitektonsko testiranje (ovde je to
ArchUnitNET) učitava prevedene projekte, čita te metapodatke i od njih gradi graf: čvorovi
su svi tipovi u sistemu, a grana postoji od tipa ka svakom tipu od kog on zavisi. Test je
tada tvrdnja o tom grafu — na primer, "nijedan tip iz projekta `Games.Domain` ne zavisi
ni od jednog tipa iz projekta `Payment.Infrastructure`" — koju biblioteka proverava
obilaskom grana i za koju prijavljuje svako pronađeno kršenje.

Tri osobine čine da ovakva provera bude sprovođenje pravila, a ne predlog:

- **Proverava se ono što je prevedeno, a ne ono što je napisano.** Pravila se izvršavaju
  nad metapodacima prevedenog koda, pa zavisnost nije moguće sakriti načinom pisanja.
  Ako kôd koristi neki tip, grana u grafu postoji.
- **Provera se izvršava tamo gde se kôd spaja.** Ovi testovi se pokreću u CI okruženju
  pri svakom pull request-u. Kršenje pravila nije komentar u pregledu koda oko kog se
  može raspravljati, već crven build koji ne može da se spoji u glavnu granu.
- **Greška pokazuje tačno mesto.** Ispis imenuje tip koji je prekršio pravilo i tip od
  kog nedozvoljeno zavisi, pa ispravka počinje od konkretne lokacije, a ne od apstraktnog
  principa.

Kompajler i arhitektonski testovi dele posao. Reference između projekata čine da se
većina nedozvoljenih zavisnosti *ne može ni prevesti*. Arhitektonski testovi hvataju
preostali slučaj: da neko izmeni `.csproj` datoteku i doda referencu koju pravila
zabranjuju. Pravila o referencama u `CLAUDE.md` opisuju arhitekturu; ovaj projekat je ono
što taj opis čini istinitim.

## Grupe testova

Svaka klasa u ovom projektu čuva jednu vrstu granice. Sve nasleđuju zajedničku osnovu.

### `BaseArchitectureTests`

Osnovna klasa iz koje ne nastaje nijedan test, već zajednička infrastruktura za sve
ostale: spisak modula, slojeva i `Shared` projekata, graf zavisnosti i pomoćne metode
`AssertNoDependency` i `AssertNoNamespaceDependency` kojima se pravila iskazuju. Graf se
gradi jednom po pokretanju, jer je učitavanje svih projekata najskuplji korak, pa je polje
statičko iako testovi do njega dolaze nasleđivanjem.

Dve posledice zaslužuju pažnju. Prvo, spiskovi modula i slojeva su jedino mesto koje se
menja kada se sistem proširi: novi modul znači jednu stavku u nizu `Modules`, a sva
postojeća pravila počinju da važe i za njega. Drugo, pravilo nad projektom koji još nema
nijedan tip tiho se preskače — prazan projekat nema grana u grafu pa ni šta da prekrši —
što znači da testovi nad slojevima `Domain`, `Application` i `Contracts` postaju aktivni
sami od sebe, onog trenutka kada se u tim projektima pojavi prvi kôd.

### `ModuleLayerTests`

Čuva raspodelu odgovornosti po slojevima *unutar* jednog modula. Sloj `Domain` zavisi
samo od projekta `Shared.Domain` — bez EF Core-a, bez ASP.NET-a, bez drugih slojeva — pa
poslovna pravila ostaju odvojena od infrastrukture i mogu da se testiraju bez baze
podataka. Sloj `Application` ne vidi ni `Infrastructure` ni veb okruženje, čime strelica
zavisnosti ostaje okrenuta ka unutra: infrastruktura implementira interfejse koje
aplikacioni sloj propisuje, nikada obrnuto. Sloj `Contracts` ne zavisi ni od čega, jer je
to površina koju drugi moduli koriste, pa se svaka njegova zavisnost prenosi na sve
korisnike. Sloj `Api` ne sme da koristi tipove iz sloja `Domain` — iako ih kroz reference
projekata tranzitivno vidi — pa endpoint ne može da vrati domenski entitet spoljnom svetu
umesto DTO-a. Sloj `Api` takođe ne može da dohvati `Infrastructure` ni EF Core, i upravo to pravilo
čini nemogućim da endpoint direktno čita iz baze. Ova pravila preslikavaju reference
između projekata, pa njihovo kršenje gotovo uvek znači da je izmenjena neka `.csproj`
datoteka.

### `ModuleIsolationTests`

Čuva granice *između* modula — razlog zbog kog je ovaj sistem modularni monolit, a ne
samo monolit. Modul sme da zavisi od `Contracts` projekta drugog modula i ni od čega
drugog iz njega: ne od njegovih entiteta, ne od njegovog `DbContext`-a, ne od njegovih
servisa. Na podatak u drugom modulu upućuje se identifikatorom, a dohvata se kroz javni
interfejs tog modula, nikada direktnim spajanjem tabela ili korišćenjem njegovih tipova.
Ova grupa takođe proverava da nijedan modul ne zavisi od modula `Identity`: autentifikacija
je deo platforme, a moduli trenutno prijavljenog korisnika poznaju samo kao vrednost
`UserId` koja im se prosleđuje.

### `SharedKernelTests`

Čuva zajednički kôd sa suprotne strane. Projekti u folderu `Shared` nalaze se ispod svih
modula, pa svaku zavisnost koju dobiju nasleđuje ceo sistem. Ovi testovi proveravaju da
nijedan `Shared` projekat ne zavisi ni od jednog modula — onog trenutka kada zajednički
kôd sazna za funkcionalnost jednog tima, on prestaje da bude zajednički i postaje
skriveno mesto sprege — kao i da `Shared.Domain`, osnova koju nasleđuje svaki domenski
model, ostaje bez EF Core-a i ASP.NET-a. Za razliku od ostalih grupa, ova čuva sistem od
grešaka platformskog tima, a ne timova koji razvijaju module.

### `HostCompositionTests`

Čuva projekat `Host.Api`, jedini kome je dozvoljeno da referencira sve module. On je zato
mesto gde je najlakše prokrijumčariti logiku koja pripada nekom modulu. Ova grupa
proverava da host dodiruje modul isključivo kroz njegove dve tačke povezivanja — metode
`AddXxxModule` i `AddXxxControllers` — i da nikada ne koristi tipove iz slojeva `Domain`,
`Application` ili `Contracts`. Host sklapa aplikaciju; on u njoj ne učestvuje.
