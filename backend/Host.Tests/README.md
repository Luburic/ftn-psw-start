# Arhitektonski testovi

## Problem koji rešavaju

Dobra arhitektura definiše skup pravila o tome ko sme da zavisi od koga. Na primer:
- Domenski model ne sme da zna za bazu podataka.
- Endpoint ne sme direktno da pristupi bazi podataka.
- Jedan modul ne sme da poseže za unutrašnjošću drugog modula.

Deo ovih pravila čuva kompajler:
- Projekat koji nema referencu na EF Core fizički ne može da ga koristi.
- Projekat koji nema referencu na drugi projekat fizički ne može da koristi njegove tipove.

Međutim, referenca između projekata se dodaje jednom linijom u `.csproj` datoteci. Takvu izmenu kompajler ne brani.

Tako programer ili agent naprave male prečice koje u trenutku deluju razumno.
Rok je blizu, a podatak koji vam treba se nalazi u tabeli drugog modula.
"Ispravan" put kroz njegov javni interfejs deluje kao nepotrebna procedura.
Prečica se napravi, funkcionalnost proradi i arhitektura se naruši. Cena stiže kasnije.
Drugi tim izmeni svoju šemu baze i vaš kôd prestane da radi, a niko ne razume zašto.
Sve postaje uvezano i teško je ispratiti gde se jedna funkcionalnost završava, a druga počinje.

Ovaj problem jednako pogađa ljude i programerske agente, jer i jedni i drugi programiraju tako što čitaju kod koji već postoji.

## Opšte rešenje

Arhitektonski test pretvara pravilo o zavisnostima u automatski test, koji se crveni kada je pravilo prekršeno.

Kada kompajler prevede C# kod, uz svaki tip se u prevedenom projektu čuvaju metapodaci o njegovim zavisnostima.
Za svaku klasu se čuvaju informacije o klasi koju nasleđuje, kog tipa su polja i parametri, koje metode poziva.

Biblioteka za arhitektonsko testiranje (kod nas ArchUnitNET) učitava prevedene projekte,
čita te metapodatke i od njih gradi graf:
- čvorovi su svi tipovi u sistemu,
- grana postoji od tipa ka svakom tipu od kog on zavisi.
 
Automatski test proverava tvrdnje o tom grafu. Na primer, "nijedan tip iz projekta `Games.Domain` ne zavisi ni od jednog tipa iz projekta `Payment.Infrastructure`".

Kompajler i arhitektonski testovi dele posao. Reference između projekata čine da se većina nedozvoljenih zavisnosti *ne može ni prevesti*. Arhitektonski testovi hvataju slučaj kada neko izmeni `.csproj` datoteku i doda referencu koju pravila zabranjuju.

## Grupe testova

Svaka klasa u ovom projektu čuva jednu vrstu granice. Sve nasleđuju zajedničku osnovu.

### `BaseArchitectureTests`

Osnovna klasa iz koje ne nastaje nijedan test, već zajednička infrastruktura za ostale. Sadrži:
- spisak modula (jedino mesto koje se menja uvođenjem novog modula),
- spisak slojeva unutar stereotipnih modula (Exploration, Games, Social, Payment),
- spisak `Shared` projekata
- Pomoćne metode `AssertNoDependency` i `AssertNoNamespaceDependency` kojima se pravila iskazuju.

### `ModuleLayerTests`

Čuva raspodelu odgovornosti po slojevima *unutar* jednog modula. Na primer:
- Sloj `Domain` zavisi samo od projekta `Shared.Domain` (bez drugih slojeva i tehnoloških detalja poput EF Core-a i ASP.NET-a)
- Sloj `Application` ne vidi ni `Infrastructure` ni veb okruženje, kako bi se koraci slučajeva korišćenja apstrahovali od tehnoloških detalja
- Sloj `Contracts` ne zavisi ni od čega, jer je to površina koju drugi moduli koriste i time se sakriva unutrašnjost modula od drugih modula
- Sloj `Api` ne sme da koristi tipove iz sloja `Domain` (iako ih kroz reference projekata tranzitivno vidi) kako endpoint ne bi vratio domenski entitet spoljnom svetu umesto DTO-a

### `ModuleIsolationTests`

Čuva granice *između* modula.

Modul sme da zavisi od `Contracts` projekta drugog modula i ni od čega drugog iz njega.

Ova grupa takođe proverava da nijedan modul ne zavisi od modula `Identity`. Moduli trenutno prijavljenog korisnika poznaju samo kao vrednost `UserId` koju dobijaju kroz JWT.

### `SharedKernelTests`

Ovi testovi proveravaju da nijedan `Shared` projekat ne zavisi ni od jednog modula.
Ako bi zajednički kod znao za funkcionalnost jednog tima, prestao bi da bude zajednički.

### `ApplicationConventionTests`

Za razliku od prethodnih grupa, koje čuvaju zavisnosti između projekata, ova grupa čuva
konvencije na nivou klasa.

Prvo pravilo: klasa čije se ime završava na `Queries` ne sme da zavisi od interfejsa
`IUnitOfWork`. Upit ne menja stanje i nikada ne čuva izmene, pa upitna klasa nema šta da
traži od jedinice posla. Konvencija je opisana u dokumentu
`docs/knowledge-base/server/arhitektura/komande-i-upiti.md`, a ovaj test je pretvara u
crveni build umesto primedbe na pregledu koda.

### `HostCompositionTests`

Čuva projekat `Host.Api`, jedini kome je dozvoljeno da referencira sve module.
Ova grupa proverava da host dodiruje modul isključivo kroz njegove dve tačke povezivanja, a to su metode `AddXxxModule` i `AddXxxControllers`.
