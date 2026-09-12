# Arhitektura modula

Prethodni segment je pokazao kako se piše komponenta koja prikazuje podatke i reaguje na korisnika. Ostaje pitanje kako komponente jednog modula organizujemo kada ih ima više desetina. Najkraći put je da svaka komponenta sama zove server, drži svoje podatke i sadrži sav prikaz. Kako modul raste, ista lista se učitava na tri mesta, ista greška se obrađuje na tri načina, a komponenta koja istovremeno zna adresu servera, stanje učitavanja i izgled tabele postaje teška za čitanje i testiranje.

Klijentska arhitektura je namerno tanja od serverske. Na klijentu nema domenskih pravila koja treba izolovati ni skladišta koje treba apstrahovati. Pravila su tri: modul je ravna lista grupa slučajeva korišćenja, komponente se dele na stranice i prikazne komponente, a upit živi u stranici dok komanda živi u servisu grupe. Podaci koje modul prikazuje dolaze u obliku koji je odredio server, pa tipove tih podataka ne osmišljavamo, već preslikavamo sa servera po utvrđenim pravilima.

## Mapa direktorijuma

1. [Stranice i prikazne komponente](1-stranice-i-prikazne-komponente.md) - Stranica preuzima zavisnosti i vodi interakciju, prikazna komponenta prima ulaze i prijavljuje izlaze. Grupa slučajeva korišćenja kao direktorijum modula i postupak kojim se novoj komponenti bira mesto.
2. [Upiti i komande](2-upiti-i-komande.md) - Zašto resurs živi u stranici, a komande u servisu grupe, i šta time dobijamo kada stranica nakon komande osvežava svoj resurs. Pravilo o tome gde stanje živi.
3. [Preslikani tipovi](3-preslikani-tipovi.md) - DTO struktura aplikacionog sloja servera kao ugovor između dve strane, datoteka sa tipovima po modulu koja preslikava serverske DTO strukture, pravila preslikavanja serverskih tipova u TypeScript i pravilo da se datoteka menja samo kada se promeni server.

Nakon ovog direktorijuma čitalac zna kako je jedan modul klijenta iznutra izgrađen. Sledeći korak je pitanje kako se više modula sastavlja u jednu aplikaciju i kako se granice između njih čuvaju, čime se bavi segment o modularnom monolitu na klijentu.
