# Modularni monolit

Serverska aplikacija je podeljena na feature module sa strogim granicama, a svaki tim poseduje svoj modul. Klijentska aplikacija prati istu podelu, pa tim poseduje modul na obe strane. Pitanja su ista kao na serveru: kako je modul iznutra organizovan, kako modul koristi ono što poseduje drugi modul, koji kod dele svi moduli i kako se granice proveravaju automatski.

Prethodni segment je pokazao kako se piše komponenta koja prikazuje podatke dobijene sa servera i omogućuje korisniku da unosi podatke kroz forme i izdaje komande ka serveru. Ostaje pitanje kako komponente jednog modula organizujemo kada ih ima više desetina. Najkraći put je da svaka komponenta sama zove server, drži svoje podatke i sadrži sav prikaz. Kako modul raste, dolazi do dupliranja, gde se ista lista pravi na tri mesta ili ista greška obrađuje na tri načina ili do formiranja previše složenih komponenti koje pokrivaju sve slučajeve.

Klijentska arhitektura je jednostavnija od serverske. Na klijentu nema domenskih pravila koja treba izolovati ni skladišta koje treba apstrahovati. Unutar modula važe tri pravila:
1. Modul je ravna lista grupa slučajeva korišćenja,
2. Komponente se dele na stranice i prikazne komponente,
3. Podaci koje modul prikazuje dolaze u obliku koji je odredio server, pa tipove tih podataka preslikavamo sa servera.

Između modula granicu ne čuva prevodilac, već pravilo alata za statičku analizu. Ulogu glavne aplikacije ima direktorijum `core`, čija tabela ruta povezuje tabele ruta modula sa aplikacijom. Modul se u aplikaciju uključuje jednom stavkom u toj tabeli i ničim drugim.

## Mapa direktorijuma

1. [Stranice i prikazne komponente](1-stranice-i-prikazne-komponente.md) - Stranici se ubrizgavaju servisi, a ona vodi interakciju, prikazna komponenta prima ulaze i prijavljuje izlaze. Grupa slučajeva korišćenja kao direktorijum modula i postupak kojim se novoj komponenti bira mesto.
2. [Preslikani tipovi](2-preslikani-tipovi.md) - DTO struktura aplikacionog sloja servera kao ugovor između dve strane, datoteka sa tipovima po modulu koja preslikava serverske DTO strukture, pravila preslikavanja serverskih tipova u TypeScript i pravilo da se datoteka menja samo kada se promeni server.
3. [Javna površina](3-javna-povrsina.md) - Adrese stranica kao javna površina modula prema drugim modulima, tabela ruta kroz koju modul ulazi u aplikaciju i dva načina da modul iskoristi drugi modul. Sastavljanje podataka više modula ostaje na serveru.
4. [Host aplikacija i zajedničko jezgro](4-host-i-zajednicko-jezgro.md) - Direktorijum `core` kao host aplikacija i `shared` kao zajedničko jezgro, presretač koji svakom zahtevu dodaje token, pravilo promocije i tabela ko šta menja.
5. [Granica i statička analiza](5-granica-i-staticka-analiza.md) - Pravilo o dozvoljenim uvozima kao arhitektonski test klijenta, čitanje prijave koja je oborila build i zašto je granica slabija od serverske.

Nakon ovog direktorijuma čitalac zna kako je jedan modul klijenta iznutra izgrađen. Zna i da modul svog tima poveže sa modulom drugog tima kroz javnu površinu, da prepozna kod koji pripada zajedničkom delu aplikacije i da pročita prijavu statičke analize koja je oborila build.
