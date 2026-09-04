# Infrastrukturni sloj

Aplikacioni sloj je deklarisao interfejse tehničkih sposobnosti koje slučaj korišćenja zahteva, ali nijedan od njih nije implementirao. Repozitorijum agregata obećava cele agregate, repozitorijum za čitanje DTO strukture, a jedinica posla upis svih izmena odjednom. Klase koje ta obećanja ispunjavaju konkretnom tehnologijom čine **infrastrukturni sloj**.

Klase infrastrukturnog sloja poznaju bazu podataka, biblioteke, radne okvire i spoljašnje sisteme. Repozitorijumske klase rade sa skladištem podataka, koje je u našem projektu PostgreSQL baza podataka, pa lekcije ovog direktorijuma govore o bazi tamo gde aplikacioni sloj govori o skladištu. Konektorske klase komuniciraju sa drugim aplikacijama, a stručnjačke klase obavljaju lokalan tehnički posao pomoću biblioteke. Sve tri vrste implementiraju interfejs koji je aplikacioni sloj deklarisao prema potrebama slučaja korišćenja, a sve tehničke zavisnosti ostaju u ovom sloju. Konektorske i stručnjačke klase obrađuje [poslednja lekcija](6-ostali-infrastrukturni-servisi.md) direktorijuma.

Prvih pet lekcija obrađuje repozitorijumske klase, jer svaki modul radi sa bazom podataka, i to kroz objektno-relacioni maper Entity Framework Core. Lekcije definišu pojmove koji važe za svaki maper i pokazuju kako ih Entity Framework Core realizuje.

## Mapa direktorijuma

1. [Objektno-relaciono mapiranje](1-orm.md) - Koliko koda traži rad sa bazom kroz ADO.NET i šta objektno-relacioni maper od tog posla preuzima. Preduslov je za sve naredne lekcije.
2. [Kontekst i model mapiranja](2-efc-kontekst-i-model.md) - Odakle maper zna kako klase izgledaju u bazi. Kontekstna klasa, konvencije, konfiguracija za mesta na kojima konvencije greše i rehidracija objekata mimo javnog konstruktora.
3. [Migracije](3-migracije.md) - Kako baza prati izmene modela. Generisanje migracije iz razlike prema snimku modela, primena pri pokretanju aplikacije i početni podaci kroz domenske konstruktore. Procedura za svakodnevni rad je u protokolu o migracijama.
4. [Repozitorijumi](4-repozitorijumi.md) - Repozitorijum agregata koji učitava agregat u celini, praćenje promena kojim kontekst sam sastavlja naredbe za upis i repozitorijum za čitanje koji projektuje podatke pravo u DTO strukturu.
5. [Jedinica posla](5-jedinica-posla.md) - Zašto repozitorijum ne sme sam da čuva, kako kontekst već jeste jedinica posla i kako se repozitorijum zbog toga skraćuje na učitavanje i dodavanje. Zaokružuje direktorijum praćenjem jedne komande kroz sve uvedene pojmove.
6. [Ostali infrastrukturni servisi](6-ostali-infrastrukturni-servisi.md) - Konektorska klasa koja sposobnost ostvaruje komunikacijom sa drugim sistemom i stručnjačka klasa koja je ostvaruje kroz biblioteku, sa pravilom šta interfejs tehničke sposobnosti sme da sadrži i kada se spoljašnji sistem obaveštava.

Nakon ovog direktorijuma čitalac zna kako se agregat upisuje u bazu i učitava iz nje, kako se šema baze menja tokom razvoja i zašto komanda upisuje sve izmene jednim pozivom. Preostaje [API sloj](../4-api-sloj.md), koji zahtev spoljašnjeg sveta prevodi u poziv aplikacionog sloja.
