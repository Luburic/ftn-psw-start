# Arhitektura modula

Prethodni segment je pokazao kako se piše komponenta koja prikazuje podatke i reaguje na korisnika. Ostaje pitanje kako komponente jednog modula organizujemo kada ih ima više desetina. Najkraći put je da svaka komponenta sama zove server, drži svoje podatke i sadrži sav prikaz. Kako modul raste, ista lista se učitava na tri mesta, ista greška se obrađuje na tri načina, a komponenta koja istovremeno zna adresu servera, stanje učitavanja i izgled tabele postaje teška za čitanje i testiranje.

Klijentska arhitektura je namerno tanja od serverske. Na klijentu nema domenskih pravila koja treba izolovati ni skladišta koje treba apstrahovati. Pravila su dva: podela komponenti na stranice i prikazne komponente i stanje modula u jednom servisu. Podaci koje modul prikazuje dolaze u obliku koji je odredio server, pa tipove tih podataka ne pišemo, već generišemo.

## Mapa direktorijuma

1. [Stranice i prikazne komponente](1-stranice-i-prikazne-komponente.md) - Stranica preuzima zavisnosti i vodi interakciju, prikazna komponenta prima vrednosti i prijavljuje događaje. Zašto podela postoji i kako se za novu komponentu bira strana.
2. [Stanje u servisu](2-stanje-u-servisu.md) - Servis modula kao jedino mesto koje razgovara sa serverom i drži stanje, uključujući stanje učitavanja i greške koje svaka stranica prikazuje na isti način.
3. [Generisani tipovi](3-generisani-tipovi.md) - DTO struktura aplikacionog sloja servera kao ugovor između dve strane, generisanje tipova iz OpenAPI dokumenta po modulu i pravilo da se generisana datoteka ne menja rukom.

Nakon ovog direktorijuma čitalac zna kako je jedan modul klijenta iznutra izgrađen. Sledeći korak je pitanje kako se više modula sastavlja u jednu aplikaciju i kako se granice između njih čuvaju, čime se bavi segment o modularnom monolitu na klijentu.
