# Koji kod zaslužuje koji test

> **Nacrt.** Struktura lekcije sa beleškama po odeljcima. Svaki odeljak navodi definiciju, problem, primer, šta se uočava, izvor u knjizi i planirani obim.

**Preduslovi:** `testovi/2-sta-cini-dobar-test.md`, `server/2-arhitektura-modula/5-čista-arhitektura.md`.
**Ciljevi učenja:** čitalac ume da za svaku klasu modula odluči da li dobija jedinični test, integracioni test ili nijedan, i da prepozna klasu koju treba podeliti pre testiranja. Ovo je glavna ljudska odluka pri testiranju.
**Radni primer:** modul Exploration: `Tour` (domen), `TourAuthoringService` (aplikacioni sloj), `TourRepository` (infrastruktura), `TourAuthoringController` (API), `TourDto`.
**Planirani obim:** oko 1600 reči.

## Uvod (oko 120 reči)

Sidro: lekcija 2 je pokazala da test trivijalnog koda ima vrednost nula. Pitanje: koje klase modula imaju kod vredan testiranja i kojom vrstom testa. Bolje je ne napisati test nego napisati loš, pa je odluka šta se ne testira jednako važna.

## Dve dimenzije koda (oko 300 reči)

- **Definicija:** složenost koda je broj tačaka grananja u njemu; domenski značaj je koliko kod neposredno iskazuje pravilo domena. Saradnik (engl. *collaborator*) je zavisnost koja ima promenljivo stanje ili živi van procesa, poput baze podataka; vrednosni objekti i nepromenljivi ulazi se ne računaju.
- **Problem:** složen ili domenski značajan kod je mesto gde greške nastaju, pa njegov test najviše štiti. Kod sa mnogo saradnika traži dugu pripremu, pa je njegov test skup za pisanje i održavanje. Dve dimenzije su nezavisne: izračunavanje bez ijednog `if` može biti domenski značajno.
- **Primer:** `Tour.Publish()` (tri pravila, nula saradnika) naspram `TourAuthoringService.Publish(...)` (nula pravila, repozitorijum i jedinica posla kao saradnici).
- **Uočiti:** implicitni saradnici se računaju, npr. statički pristup vremenu; što je kod važniji, treba da ima manje saradnika.
- **Izvor:** Khorikov 7.1.1 (bez ciklomatske formule).

## Četiri tipa koda i slojevi modula (oko 600 reči)

Kičma lekcije: tabela dva puta dva, a zatim jedan pododeljak po polju sa preslikavanjem na sloj modula.

### Domenski model: jedinični testovi

- Agregati, entiteti, vrednosni objekti i domenski servisi. Visok značaj, bez saradnika. Testovi su kratki, brzi i najviše štite.
- **Primer:** `TourTests` iz lekcije 1.
- **Uočiti:** test gradi agregat konstruktorom i poziva metode, bez baze i bez zamena za zavisnosti; direktorijum `Unit/` sadrži samo ovakve testove.

### Kontroleri: integracioni testovi

- U knjizi je kontroler svaki kod koji koordinira rad drugih: u modulu su to zajedno aplikacioni servis, repozitorijum i API kontroler. Nizak značaj, mnogo saradnika.
- **Primer:** put komande `Publish` kroz tri klase, prikazan kao niz poziva bez koda.
- **Uočiti:** ovaj kod se ne testira jedinično, jer bi test morao da zameni sve saradnike i proverio bi tri reda orkestracije; testira se kratko, kroz mali broj integracionih testova koji prolaze kroz sve tri klase odjednom (lekcija 4).

### Trivijalan kod: bez testa

- Konstruktori koji dodeljuju, svojstva, DTO strukture, mapiranje na DTO.
- **Primer:** `TourDto`.
- **Uočiti:** test bi izvršio kod bez grananja i bez domenskog pravila; greška u mapiranju se hvata usput, kada integracioni test pročita odgovor.

### Prekomplikovan kod: podeliti

- Kod koji je i značajan i ima mnogo saradnika, npr. aplikacioni servis koji sadrži domensko pravilo, ili agregat koji poziva repozitorijum.
- **Primer:** `TourAuthoringService.Publish` koji proverava dužinu opisa pre poziva `tour.Publish()`; ispravka je premeštanje provere u agregat.
- **Uočiti:** kod može da bude dubok (složen) ili širok (mnogo saradnika), nikada oboje; podela na domen i aplikacioni sloj iz čiste arhitekture je upravo ta podela, pa u modulu koji je poštuje ovo polje ostaje prazno.
- **Izvor za ceo odeljak:** Khorikov 7.1.1, 7.3.1, 7.3.2 (bez imena Humble Object).

## Preduslovi (oko 150 reči)

- **Definicija:** preduslov je provera na ulazu metode koja odbija nedozvoljeno stanje izuzetkom.
- **Problem:** nije svaki preduslov vredan testa.
- **Primer:** `Constructor_rejects_empty_tags` (pravilo domena: tura mora imati oznaku) naspram provere da niz ulaznih podataka ima bar tri elementa (tehnička zaštita).
- **Uočiti:** preduslov sa domenskim značajem je invarijanta agregata i dobija test; tehnički preduslov ne.
- **Izvor:** Khorikov 7.3.3.

## Repozitorijumi i upiti (oko 250 reči)

- **Problem:** dve klase modula izgledaju kao da zaslužuju zaseban test, a ne zaslužuju.
- Repozitorijum pripada polju kontrolera: mala složenost, saradnik van procesa. Zaseban test košta koliko integracioni, a greška u mapiranju se hvata kada integracioni test komande pročita red iz baze.
- Upitna klasa nema domenski sloj, pa nema jedinični test; greška u čitanju ne kvari podatke, pa je prag za njen test viši nego za komande. Kada se testira, testira se samo integraciono, kroz projekciju koju vraća.
- **Uočiti:** ni jedna ni druga klasa nema direktorijum u `Unit/`.
- **Izvor:** Khorikov 10.5.

## Piramida testova (oko 150 reči)

- **Definicija:** piramida testova je odnos u kome jedinični testovi čine većinu, a integracioni manjinu.
- **Problem:** integracioni test je sporiji i skuplji, pa se piše samo tamo gde jedinični ne dopire.
- **Uočiti:** oblik zavisi od modula. Modul sa bogatim agregatom ima piramidu; modul koji uglavnom čuva i čita podatke ima pravougaonik, i to nije nedostatak.
- **Izvor:** Khorikov 8.1.2, 4.5.1.

## Van opsega

Refaktorisanje ka testabilnom kodu korak po korak (Khorikov 7.2), obrasci CanExecute i domenski događaji (7.4), mock objekti.
