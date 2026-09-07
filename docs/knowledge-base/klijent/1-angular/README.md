# Angular

Klijentska aplikacija rešava tehničke probleme koji ne zavise od domena, isto kao i serverska. Mora da prikaže podatke u HTML dokumentu i da prikaz osveži kada se podaci promene, da reaguje na akcije korisnika, da po adresi u pregledaču odluči koji ekran prikazuje, da razmenjuje podatke sa serverom i da sastavlja objekte od kojih se sastoji. Ove probleme rešava radni okvir, a naš projekat koristi Angular.

Čitalac je gradio veb aplikacije u čistom JavaScript-u i poznaje osnove React-a. Angular rešava iste probleme, ali propisuje kako se aplikacija strukturira i kako se elementi koda označavaju da bi ih radni okvir pronašao. Ovde upoznajemo onaj deo Angular-a koji projekat koristi i ništa van njega. Angular nudi više načina za većinu poslova, a mi biramo jedan i njega koristimo svuda.

## Mapa direktorijuma

1. [TypeScript](1-typescript.md) - Najmanji deo jezika TypeScript potreban za čitanje projekta: anotacije tipova, interfejs, unija literala, generički tip, nepostojeća vrednost i klasni modifikatori. Preduslov je za sve naredne lekcije.
2. [Angular](2-angular.md) - Najmanja aplikacija dovoljna za rad, pročitana datoteku po datoteku: pokretanje, konfiguracija aplikacije, tabela ruta i korenska komponenta.
3. [Komponenta](3-komponenta.md) - Klasa, šablon i stil kao jedna celina. Ispis vrednosti u šablonu, vezivanje svojstava i događaja i reakcija na klik. Lekcija se završava primerom u kom se prikaz ne osvežava.
4. [Signali](4-signali.md) - Zašto prikaz ne prati promenu običnog polja klase, signal kao stanje komponente, izvedene vrednosti i poređenje sa mehanizmima React-a koje čitalac poznaje.
5. [Kontrola toka](5-kontrola-toka.md) - Grananje i petlja u šablonu, alias za vrednost koja može da nedostaje i referenca na element šablona.
6. [Sastavljanje komponenti](6-sastavljanje-komponenti.md) - Ugnježdavanje komponenti, ulazi i izlazi, podaci putuju naniže, a događaji naviše.
7. [Rutiranje](7-rutiranje.md) - Kako adresa u pregledaču bira komponentu, parametri rute kao ulazi komponente i lenjo učitavanje delova aplikacije.
8. [Servisi i zavisnosti](8-servisi-i-zavisnosti.md) - Kontejner zavisnosti na klijentu, preuzimanje zavisnosti u komponenti i servis koji drži stanje.
9. [Čitanje podataka](9-citanje-podataka.md) - Resurs koji čita podatke sa servera: adresa kao funkcija signala, vrednost pre učitavanja, stanje učitavanja i greške, ponovno učitavanje.
10. [Komande](10-komande.md) - Slanje komande serveru, čekanje odgovora, prikaz greške koju server prijavi i ponovno učitavanje resursa nakon uspeha.
11. [Forme](11-forme.md) - Forma vezana za signal, šema validacije i stanje polja.
12. [Slanje forme](12-slanje-forme.md) - Slanje forme, dugme onemogućeno dok zahtev traje, navigacija nakon uspeha i vraćanje forme u početno stanje.
13. [Stilovi](13-stilovi.md) - Stilovi ograničeni na komponentu, raspored SCSS datoteka u projektu i kako se menja tema aplikacije.

Nakon ovog direktorijuma čitalac zna da napiše komponentu koja prikazuje podatke sa servera, reaguje na korisnika i šalje izmene nazad. Sledeći korak je pitanje kako se takve komponente organizuju unutar jednog modula, čime se bavi direktorijum o arhitekturi modula.
