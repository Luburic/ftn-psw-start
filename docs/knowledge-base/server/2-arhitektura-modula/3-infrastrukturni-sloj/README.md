# Infrastrukturni sloj

Aplikacioni sloj je deklarisao interfejse tehničkih sposobnosti koje slučaj korišćenja zahteva. Infrastrukturni sloj implementira te sposobnosti.

Klase infrastrukturnog sloja poznaju bazu podataka, biblioteke, radne okvire i spoljašnje sisteme. Repozitorijumske klase rade sa skladištem podataka. Konektorske klase komuniciraju sa drugim aplikacijama, a lokalni tehnički stručnjaci rade sa specijalizovanim bibliotekama. Sve tri vrste implementiraju interfejs koji je aplikacioni sloj deklarisao prema potrebama slučaja korišćenja, a sve tehničke zavisnosti ostaju u ovom sloju.

## Mapa direktorijuma

1. [Objektno-relaciono mapiranje](1-orm.md) - Objektno-relacioni maper je skup mehanizama koji predstavljaju most između OOP koncepata i koncepata relacionih baza podataka. Razumevanje ove apstrakcije je preduslov za naredne segmente.
2. [Kontekst i model mapiranja](2-efc-kontekst-i-model.md) - Odakle maper zna kako klase izgledaju u bazi. Kontekstna klasa, konvencije, konfiguracija za mesta na kojima konvencije greše i rehidracija objekata iz redova baze podataka.
3. [Migracije](3-migracije.md) - Kako baza prati izmene modela. Generisanje migracije iz razlike prema snimku modela, primena pri pokretanju aplikacije i početni podaci kroz domenske konstruktore. Procedura za svakodnevni rad je u protokolu o migracijama.
4. [Repozitorijumi](4-repozitorijumi.md) - Repozitorijum agregata koji učitava agregat u celini, praćenje promena kojim kontekst sam sastavlja naredbe za upis i repozitorijum za čitanje koji projektuje podatke pravo u DTO strukturu.
5. [Jedinica posla](5-jedinica-posla.md) - Zašto repozitorijum ne treba da direktno šalje komande bazi i kako prepušta taj posao jedinici posla.
6. [Ostali infrastrukturni servisi](6-ostali-infrastrukturni-servisi.md) - Konektorske klase koje interaguju sa drugim eksternim sistemima i lokalni tehnički stručnjaci koji rade sa bibliotekama.

Nakon ovog direktorijuma čitalac zna da koristi objektno relacione mapere i da definiše implementacije tehničkih mogućnosti infrastrukturnog sloja. Preostaje [API sloj](../4-api-sloj.md), koji zahtev spoljašnjeg sveta prevodi u poziv aplikacionog sloja.
