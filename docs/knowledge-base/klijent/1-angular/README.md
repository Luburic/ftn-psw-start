# Angular

Klijentska aplikacija rešava tehničke probleme koji ne zavise od domena, isto kao i serverska. Mora da prikaže podatke u HTML dokumentu i da prikaz osveži kada se podaci promene, da reaguje na akcije korisnika, da po adresi u internet čitaču odluči koji ekran prikazuje, da razmenjuje podatke sa serverom i da sastavlja objekte od kojih se sastoji. Ove probleme rešava radni okvir, a naš projekat koristi Angular.

Čitalac je gradio veb aplikacije u čistom JavaScript-u i poznaje osnove React-a. Angular rešava iste probleme, ali propisuje kako se aplikacija strukturira i kako se elementi koda označavaju da bi ih radni okvir pronašao. Ovde upoznajemo onaj deo Angular-a koji projekat koristi i ništa van njega. Angular nudi više načina za većinu poslova, a mi biramo jedan i njega koristimo svuda.

## Mapa direktorijuma

1. [TypeScript](1-typescript.md) - Najmanji deo jezika TypeScript potreban za čitanje projekta: anotacije tipova, interfejs, generički tip, nepostojeća vrednost i klasni modifikatori. Preduslov je za sve naredne lekcije.
2. [Angular](2-angular.md) - Najmanja aplikacija dovoljna za rad, pročitana datoteku po datoteku: pokretanje, konfiguracija aplikacije i korenska komponenta.
3. [Komponenta](3-komponenta.md) - Klasa, šablon i stil kao jedna celina. Ispis vrednosti u šablonu, vezivanje svojstava i događaja i reakcija na klik. Lekcija se završava primerom u kom se prikaz ne osvežava.
4. [Signali](4-signali.md) - Zašto prikaz ne prati promenu običnog polja klase, signal kao stanje komponente i poređenje sa mehanizmom React-a koji čitalac poznaje.
5. [Izvedeni signali](5-izvedeni-signali.md) - Signal čija se vrednost računa iz drugih signala i pravilo o signalu koji čuva niz ili objekat.
6. [Kontrola toka](6-kontrola-toka.md) - Grananje i petlja u šablonu, alias za vrednost koja može da nedostaje i referenca na element šablona.
7. [Sastavljanje komponenti](7-sastavljanje-komponenti.md) - Ugnježdavanje komponenti, ulazi i izlazi, podaci putuju naniže, a događaji naviše.
8. [Rutiranje](8-rutiranje.md) - Tabela ruta, mesto iscrtavanja, veze za navigaciju, parametar rute kao ulaz komponente i tabela ruta modula.
9. [Servisi i zavisnosti](9-servisi-i-zavisnosti.md) - Servis kao klasa koju kontejner zavisnosti pravi jednom, preuzimanje zavisnosti u komponenti i servisu i servis koji drži prijavljenog korisnika.
10. [Čitanje podataka](10-citanje-podataka.md) - Resurs koji čita podatke sa servera: priprema za razmenu sa serverom, adresa kao funkcija signala, vrednost pre učitavanja, stanje učitavanja i greške, ponovno učitavanje.
11. [Komande](11-komande.md) - Slanje komande kroz servis, čekanje odgovora, greška koju server prijavi i obrazac stranice sa komandom koja nakon uspeha ponovo učitava svoj resurs.
12. [Forme](12-forme.md) - Forma izgrađena oko signala, šema sa pravilima validacije, tri oblika zapisa forme, vezivanje elementa za polje i prikaz grešaka.
13. [Slanje forme](13-slanje-forme.md) - Slanje forme kao komanda, navigacija nakon uspeha iz klase i vraćanje forme koja ostaje na ekranu u početno stanje.
14. [Stilovi](14-stilovi.md) - Stilovi ograničeni na komponentu, globalna datoteka sa četiri dela, tokeni i postupak izbora mesta za nov stil.

Nakon ovog direktorijuma čitalac zna da napiše komponentu koja prikazuje podatke sa servera, reaguje na korisnika i šalje izmene nazad. Sledeći korak je pitanje kako se takve komponente organizuju unutar jednog modula, čime se bavi direktorijum o arhitekturi modula.
