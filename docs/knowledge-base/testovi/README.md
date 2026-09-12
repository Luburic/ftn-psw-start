# Testovi

Svaki modul projekta ima test projekat sa dva direktorijuma. `Unit/` sadrži testove agregata i domenskih servisa, a `Integration/` testove koji šalju HTTP zahtev pokrenutoj aplikaciji sa pravom bazom. Alati danas pišu većinu takvih testova. Ono što ostaje čoveku je da odluči šta se testira i kojom vrstom testa, da prepozna test koji ne vredi zadržati i da skup testova oblikuje tako da preživi rast modula. Ovaj segment uči te tri veštine, a uz njih daje mehaniku potrebnu da se testovi projekta čitaju i menjaju.

## Mapa direktorijuma

1. [Anatomija jediničnog testa](1-anatomija-jedinicnog-testa.md) - Šta je automatski test i od čega se sastoji. Test okvir, tri dela testa, nezavisnost testova, ime kao rečenica o domenu, parametrizovani test i provere.
2. [Šta čini dobar test](2-sta-cini-dobar-test.md) - Kako se za dati test odlučuje da li ga vredi zadržati. Cilj testiranja, zaštita od regresija, otpornost na refaktorisanje i lažni pozitivi, brzina i održivost, vrednost kao proizvod.
3. [Koji kod zaslužuje koji test](3-koji-kod-zasluzuje-koji-test.md) - Kako se za svaku klasu modula odlučuje da li dobija jedinični test, integracioni test ili nijedan. Dve dimenzije koda, četiri tipa koda preslikana na slojeve modula, preduslovi, repozitorijumi i upiti, piramida testova.
4. [Integracioni testovi](4-integracioni-testovi.md) - Šta integracioni test izvršava i koje scenarije pokriva. Upravljane i neupravljane zavisnosti, srećan put i rubni slučajevi van domena, pokretanje aplikacije u testu, prijavljeni korisnik, organizacija test projekta.
5. [Podaci integracionih testova](5-podaci-integracionih-testova.md) - Kako testovi dele jednu bazu, a ostaju nezavisni. Poznato stanje pre svakog testa, početni podaci koji preživljavaju rast, tri kanala, provere koje novi red ne obara.

Nakon ovog segmenta čitalac ume da za novu funkcionalnost svog modula odluči koje testove piše, da pregleda testove koje je napisao neko drugi i da proširi početne podatke bez obaranja postojećih testova.
