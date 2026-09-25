# Testovi

Svaki modul projekta ima test projekat sa dva direktorijuma. `Unit/` sadrži testove agregata i domenskih servisa, a `Integration/` testove koji šalju HTTP zahtev pokrenutoj aplikaciji sa pravom bazom. Alati danas pišu većinu takvih testova. Ono što ostaje čoveku je da odluči šta se testira i kojom vrstom testa, da prepozna test koji ne vredi zadržati i da skup testova oblikuje tako da preživi rast modula. Ovaj segment uči te tri veštine, a uz njih opisuje kod testova kako bismo mogli da ih čitamo i menjamo.

## Mapa direktorijuma

1. [Anatomija jediničnog testa](1-anatomija-jedinicnog-testa.md) - Šta je automatski test i od čega se sastoji. Test okvir, tri dela testa, nezavisnost testova, ime kao rečenica o domenu, parametrizovani test i provere.
2. [Aspekti kvaliteta testa](2-aspekti-kvaliteta-testa.md) - Kako se za dati test odlučuje da li ga vredi zadržati. Cilj testiranja, zaštita od regresija, otpornost na refaktorisanje i lažni pozitivi, brzina i lakoća održavanja, ocena kvaliteta testa.
3. [Mapa testiranja serverskih komponenti](3-mapa-testiranja-serverskih-komponenti.md) - Kako se za svaku klasu modula odlučuje da li dobija jedinični test, integracioni test ili nijedan. Dve dimenzije koda, četiri tipa koda preslikana na komponente modula, hibridne klase koje se refaktorišu pre testiranja.
4. [Integracioni testovi](4-integracioni-testovi.md) - Šta integracioni test izvršava i koje scenarije pokriva. Upravljane i neupravljane zavisnosti, uspešan scenario i odbijanja van domena.
5. [Platformski radni okvir za testove](5-platformski-radni-okvir-za-testove.md) - Kako platforma preuzima poslove koji se ponavljaju u svakom integracionom testu. Biblioteka, platforma i modul, roditeljska klasa integracionih testova, početni podaci koji preživljavaju rast, provere koje novi red ne obara.

Nakon ovog segmenta čitalac ume da za novu funkcionalnost svog modula odluči koje testove piše, da pregleda testove koje je napisao neko drugi i da proširi početne podatke bez obaranja postojećih testova.
