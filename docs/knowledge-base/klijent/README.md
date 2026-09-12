# Klijent

Klijentski deo našeg projekta je jedna Angular aplikacija podeljena na iste *feature* module kao i server, pa tim poseduje svoj modul na obe strane. Klijent nema domenska pravila ni bazu podataka, već prikazuje podatke koje server vraća, reaguje na korisnika i šalje izmene nazad. Da bismo takav kod pisali i čitali, treba da razumemo tri stvari: kako radni okvir prikazuje podatke i reaguje na promene, kako je kod jednog modula organizovan i kako se više modula sastavlja u jednu aplikaciju.

## Mapa direktorijuma

1. [Angular](1-angular/README.md) - Radni okvir preuzima tehničke probleme zajedničke svim klijentskim aplikacijama, a zauzvrat propisuje kako se aplikacija strukturira. Lekcije obrađuju najmanji deo TypeScript-a potreban za čitanje projekta, komponentu i signale, rutiranje, servise, čitanje podataka sa servera i slanje komandi, forme i stilove.
2. [Arhitektura modula](2-arhitektura-modula/README.md) - Klijentska arhitektura je namerno tanja od serverske i čine je tri pravila. Lekcije obrađuju podelu komponenti na stranice i prikazne komponente, mesto upita i komande i tipove preslikane sa servera.
3. [Modularni monolit](3-modularni-monolit/README.md) - Više modula se sastavlja u jednu aplikaciju kroz javnu površinu svakog modula, a granice između njih čuva statička analiza. Lekcije obrađuju javnu površinu modula, host aplikaciju sa zajedničkim jezgrom i pravilo o uvozima kao arhitektonski test klijenta.
