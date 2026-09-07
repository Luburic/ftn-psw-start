# Modularni monolit

Serverska aplikacija je podeljena na feature module sa strogim granicama, a svaki tim poseduje svoj modul. Klijentska aplikacija prati istu podelu, pa tim poseduje modul na obe strane. Pitanja su ista kao na serveru: kako modul koristi ono što poseduje drugi modul, koji kod dele svi moduli i kako se granice proveravaju automatski. Odgovori se razlikuju, jer na klijentu granicu ne čuva prevodilac, već pravilo alata za statičku analizu.

Jedna aplikacija se pokreće i ovde. Ulogu glavne aplikacije ima direktorijum `core`, koji drži tabelu ruta i za svaki modul lenjo učitava njegove rute. Modul se u aplikaciju uključuje jednom stavkom u toj tabeli i ničim drugim.

## Mapa direktorijuma

1. [Javna površina](1-javna-povrsina.md) - Datoteka `public-api.ts` kao kontrakt modula prema drugim modulima: šta se izvozi, šta se nikada ne izvozi i dva načina da modul iskoristi drugi modul, navigacija na njegovu rutu ili ugrađivanje komponente koju izvozi. Sastavljanje podataka više modula ostaje na serveru.
2. [Gradivni elementi](2-gradivni-elementi.md) - Direktorijumi `core` i `shared`, globalni stilovi kao deljeni kod, pravilo promocije i stanje prijavljenog korisnika kao jedini izuzetak od zabrane deljenog stanja.
3. [Granica i statička analiza](3-granica-i-staticka-analiza.md) - Pravilo o dozvoljenim uvozima kao arhitektonski test klijenta, čime se razlikuje od provere prevodioca na serveru i kako se čita prijava koja je oborila build.

Nakon ovog direktorijuma čitalac zna da modul svog tima poveže sa modulom drugog tima kroz javnu površinu, da prepozna kod koji pripada zajedničkom delu aplikacije i da pročita prijavu statičke analize koja je oborila build.
