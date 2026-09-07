# Modularni monolit

Prethodni segment je pokazao kako je jedan stereotipan *feature modul* izgrađen. Ostaje pitanje kako se više modula sastavlja u jednu aplikaciju. Svaki tim treba da razvija svoj deo bez stalnog usklađivanja sa ostalima, a aplikacija treba da ostane jedna celina koju je jednostavno pokrenuti i testirati.

Monolit bez granica tu podelu ne nudi. Svaka klasa može da koristi svaku drugu, pa se delovi sistema vremenom prepliću i promena jednog tima lomi kod drugog. Mikroservisna arhitektura nudi granice kroz zasebne aplikacije, ali donosi operativni trošak, jer se pokreće više procesa, komunikacija ide preko mreže, a podaci su raspodeljeni po više baza. **Modularni monolit** (engl. *modular monolith*) je arhitektura u kojoj se aplikacija razvija i pokreće kao jedna celina, a njen kod je podeljen na module sa strogim granicama. Zadržava jednostavnost jedne aplikacije, a granice uvodi u samom kodu.

## Feature modul

**Feature modul** je deo aplikacije koji realizuje jednu poslovnu sposobnost i poseduje sav kod i podatke potrebne za nju.

Posmatrajmo softver za istraživanje javnog mnjenja. Njegovi moduli mogu da budu:
- Ankete, koji poseduje ankete i odgovore ispitanika, i
- Nagrade, koji ispitanicima dodeljuje poene za popunjene ankete.

Svaki modul poseduje svoje domenske klase, svoje slučajeve korišćenja i svoju šemu u bazi podataka. Tada jedan modul referencira entitete drugog modula kroz identifikator, pa modul Nagrade čuva identifikator ankete, a ne njene tabele ili klase.

Svaki modul enkapsulira svoju unutrašnjost i nudi skup pažljivo definisanih operacija spoljašnjem svetu. Unutrašnjost čine domenske klase, servisi i pristup infrastrukturi. Nju drugi moduli ne vide. Dostupna površina modula su kontroleri, namenjeni eksternim klijentskim aplikacijama, i kontrakti, namenjeni drugim modulima.

## Sastavljanje aplikacije

U modularnom monolitu se svi moduli sastavljaju u jednu aplikaciju. **Host aplikacija** je projekat koji sastavlja module tako što pri pokretanju pozove metodu proširenja svakog modula, kojom modul registruje svoje klase u kontejner zavisnosti. U našem projektu je to `Host.Api`, koji za svaki modul poziva `AddXxxModule` za registraciju servisa i `AddXxxControllers` za registraciju kontrolera. Nakon toga zahtevi stižu u jedan proces, a granice modula postoje u kodu, ne između procesa.

## Mapa direktorijuma

Podela na module otvara tri pitanja, kojima se bave tri lekcije ovog direktorijuma:

1. [Kontrakti](1-kontrakti.md) - Kako modul dolazi do podatka koji poseduje drugi modul, a da ne poseže u njegovu unutrašnjost. Kontrakt kao interfejs i DTO strukture namenjene drugim modulima, gde živi, ko ga implementira i kako ga drugi modul poziva.
2. [Gradivni elementi](2-gradivni-elementi.md) - Koji kod dele svi moduli i ko ga poseduje. Zajedničko jezgro, gradivni elementi po slojevima i pravilo koje ističe kada se jezgro proširuje.
3. [Arhitektonski testovi](3-arhitektonski-testovi.md) - Kako se pravila o zavisnostima između slojeva i modula proveravaju automatski, umesto primedbom na pregledu koda. Tri vrste pravila i oblik testa za svaku.

Nakon ovog direktorijuma čitalac zna da modul svog tima poveže sa modulom drugog tima kroz kontrakt, da prepozna kod koji pripada zajedničkom jezgru i da pročita arhitektonski test koji je oborio build.
