# Klijent

Klijentski deo našeg projekta je jedna Angular aplikacija podeljena na iste *feature* module kao i server, tako da razvojni tim poseduje svoj modul na obe strane. Klijent nema domenska pravila ni bazu podataka, već prikazuje podatke koje server vraća, reaguje na korisnika i šalje izmene nazad. Da bismo takav kod pisali i čitali, treba da se upoznamo sa glavnim konceptima Angular radnog okvira, pa zatim da vidimo kako je arhitektura našeg projekta organizovana.

## Mapa direktorijuma

1. [Angular](1-angular/README.md) - Radni okvir preuzima tehničke probleme zajedničke svim klijentskim aplikacijama, a zauzvrat propisuje kako se aplikacija strukturira. Lekcije obrađuju najmanji deo TypeScript-a potreban za čitanje projekta, komponentu i signale, rutiranje, servise, čitanje podataka sa servera i slanje komandi, forme i stilove.
2. [Modularni monolit](2-modularni-monolit/README.md) - Klijentska arhitektura je tanja od serverske. Prati arhitekturu modularnog monolita, ali svaki modul ima tri značajna arhitekturalna pravila. Lekcije obrađuju podelu komponenti na stranice i prikazne komponente, tipove preslikane sa servera, javnu površinu modula, host aplikaciju sa zajedničkim jezgrom i pravilo o uvozima kao arhitektonski test klijenta.
