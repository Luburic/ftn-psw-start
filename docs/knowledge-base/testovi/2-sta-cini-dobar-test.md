# Šta čini dobar test

> **Nacrt.** Struktura lekcije sa beleškama po odeljcima. Svaki odeljak navodi definiciju, problem, primer, šta se uočava, izvor u knjizi i planirani obim.

**Preduslovi:** `testovi/1-anatomija-jedinicnog-testa.md`.
**Ciljevi učenja:** čitalac ume da za dati test kaže da li vredi zadržati: da li proverava ishod ili korake, da li bi pao pri refaktorisanju koje ponašanje ne menja i da li uopšte može da otkrije grešku. Ovo je lupa kroz koju se pregledaju testovi koje je napisao neko drugi, uključujući agenta.
**Radni primer:** testovi agregata `Tour` iz lekcije 1; jedan namerno loš test napisan za ovu lekciju.
**Planirani obim:** oko 1400 reči.

## Uvod (oko 150 reči)

Sidro: u lekciji 1 čitalac je video `Publish_publishes_a_complete_tour`. Pitanje: šta bi taj test učinilo beskorisnim, a da i dalje prolazi. Tri odgovora najavljuju lekciju: test koji proverava kako je `Publish()` napisan umesto šta je uradio, test koji pada kada se `Publish()` prepiše a ponašanje ostane isto, test koji proverava nešto što ne može da bude pogrešno.

## Cilj testiranja (oko 200 reči)

- **Definicija:** cilj automatskog testiranja je održiv rast projekta, odnosno da promena koda posle više meseci rada košta koliko i na početku.
- **Problem:** bez testova svaka izmena rizikuje regresiju, grešku u funkcionalnosti koja je radila; tim usporava jer proverava ručno ili ne proverava. Testovi su i sami kod: pišu se, čitaju, održavaju i lažno padaju. Test ima vrednost i trošak, i zadržava se samo ako je vrednost veća.
- **Primer:** narativni. Modul sa deset sprintova rada, novo pravilo o ključnoj tački obara objavljivanje ture, niko ne primećuje do demonstracije.
- **Uočiti:** pokrivenost koda (procenat izvršenih redova) je samo negativan pokazatelj: niska pokrivenost je siguran znak problema, visoka ne dokazuje ništa, jer meri da je red izvršen, a ne da je ishod proveren. Jedan pasus, bez metrika i primera.
- **Izvor:** Khorikov 1.2, 1.3 (samo zaključak), 1.4.

## Zaštita od regresija (oko 200 reči)

- **Definicija:** zaštita od regresija je mera koliko test može da otkrije grešku, i raste sa količinom i složenošću koda koji test izvršava, uključujući kod biblioteka.
- **Problem:** test trivijalnog koda (svojstvo sa jednim redom, konstruktor koji samo dodeljuje) ne može da nađe grešku, jer greške tamo nema, a košta koliko i svaki drugi.
- **Primer:** test koji proverava da `new Tour(...)` ima zadato ime, naspram `Publish_requires_a_transport_time`.
- **Uočiti:** prvi test izvršava jedan red bez grananja; drugi izvršava pravilo koje sutra neko može da izmeni pogrešno.
- **Izvor:** Khorikov 4.1.1, 4.4.2.

## Otpornost na refaktorisanje (oko 350 reči)

- **Definicija:** refaktorisanje je izmena koda koja ne menja ponašanje. Otpornost na refaktorisanje je mera koliko test preživljava takve izmene bez lažnog pozitiva, odnosno pada testa dok funkcionalnost radi.
- **Problem:** lažni pozitivi navikavaju tim da ignoriše pale testove i obeshrabruju refaktorisanje, jer svaka promena lomi testove. Uzrok je sprega testa sa detaljima implementacije: test proverava kako je nešto urađeno, a ne šta je urađeno.
- **Primer:** loš test koji proverava da `Publish()` postavlja `PublishedAt` pre nego što promeni `Status` (redosled koraka), ili koji čita privatnu listu preko refleksije, naspram `Publish_publishes_a_complete_tour` koji proverava samo krajnje stanje.
- **Uočiti:** prvi test pada kada se dva reda zamene mestima, a tura se i dalje ispravno objavljuje; drugi test pada samo kada se ponašanje promeni. Test gleda na agregat kao klijent, kroz javne metode i svojstva (crna kutija). Otpornost je binarna: test ili ima spregu sa implementacijom ili nema, pa se ova osobina ne razmenjuje za druge.
- **Izvor:** Khorikov 4.1.2 do 4.1.4, 4.4.5 (drugi deo), 4.5.2.

## Brzina i održivost (oko 200 reči)

- **Definicija:** brza povratna informacija je vreme izvršavanja testa; održivost je koliko je test teško razumeti (dužina) i pokrenuti (spoljne zavisnosti).
- **Problem:** spor test se retko pokreće, pa greška živi duže; dug test se ne čita, pa se popravlja nasumice.
- **Primer:** poređenje testa agregata (milisekunde, bez zavisnosti) i testa kroz HTTP sa bazom iz lekcije 4 (sekunde, potreban PostgreSQL).
- **Uočiti:** obe vrste testa imaju mesto; razlika u brzini je razlog da se većina ponašanja proverava na nivou agregata.
- **Izvor:** Khorikov 4.3.

## Vrednost testa (oko 200 reči)

- **Definicija:** vrednost testa je proizvod četiri ocene, pa nula na bilo kojoj čini test bezvrednim.
- **Problem:** nijedan test ne može da ima najviše ocene na svemu; zaštita od regresija i brzina se razmenjuju (test kroz više koda je sporiji), a otpornost i održivost se ne razmenjuju, nego se uvek drže na najvišem nivou.
- **Primer:** tabela sa tri testa iz prethodnih odeljaka (trivijalan, sa spregom, `Publish_publishes_a_complete_tour`) i njihove ocene.
- **Uočiti:** kada se pregleda tuđi test, prvo se traži nula: da li proverava nešto što ne može da bude pogrešno, da li proverava korake umesto ishoda.
- **Izvor:** Khorikov 4.4 (bez slika tri ekstrema), 4.4.5.

## Van opsega

Tabela signal i šum, pokrivenost linija i grana, piramida testova (lekcija 3), mock objekti.
