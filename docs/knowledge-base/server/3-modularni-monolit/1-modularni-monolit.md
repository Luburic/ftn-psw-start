**Modularni monolit** (engl. *modular monolith*) je arhitektura u kojoj se aplikacija razvija i pokreće kao jedna celina, a njen kod je podeljen na module sa strogim granicama.

Kada veću aplikaciju razvija više timova, postavlja se pitanje kako podeliti kod na delove koje timovi mogu razvijati bez stalnog međusobnog usklađivanja. Monolit bez granica tu podelu ne nudi: svaka klasa može da koristi svaku drugu, pa se delovi sistema vremenom prepliću i promena jednog tima lomi kod drugog. Mikroservisna arhitektura nudi granice kroz zasebne aplikacije, ali donosi operativni trošak, jer se pokreće više procesa, komunikacija ide preko mreže, a podaci su raspodeljeni. Modularni monolit zadržava jednostavnost jedne aplikacije, a granice uvodi u samom kodu.

## Feature moduli

**Feature modul** (engl. *feature module*) je deo aplikacije koji realizuje jednu poslovnu sposobnost i poseduje sav kod i podatke potrebne za nju.

Posmatrajmo softver za istraživanje javnog mnjenja. Njegovi moduli mogu da budu Ankete, koji poseduje ankete i odgovore ispitanika, i Nagrade, koji ispitanicima dodeljuje poene za popunjene ankete. Svaki modul poseduje svoje domenske klase, svoje slučajeve korišćenja i svoj deo baze podataka. Podatak drugog modula modul pamti samo kao identifikator. Modul Nagrade tako čuva identifikator ankete, a ne njene tabele ili klase.

Granica modula deli njegovu unutrašnjost od javne površine. Unutrašnjost čine domenske klase, servisi i pristup bazi, i nju drugi moduli ne smeju da koriste. Javnu površinu čini kontrakt modula, opisan u dokumentu [Kontrakti](kontrakti.md).

U teoriji, svaki modul unutar svojih granica može da prati bilo koju arhitekturu, jer granica krije unutrašnju strukturu od ostatka sistema. U praksi svi moduli jedne aplikacije prate istu arhitekturu, jer programer tada svaki modul čita na isti način. U ovim lekcijama moduli unutar granica prate [čistu arhitekturu](čista-arhitektura.md).

## Zajednički kod

Mali deo koda koriste svi moduli, na primer bazne klase domenskih objekata i pomoćne strukture. Taj kod čini **zajedničko jezgro** (engl. *shared kernel*). Zajedničko jezgro se drži namerno malim, jer svaka njegova promena pogađa sve module. Dodavanje koda u zajedničko jezgro je zato odluka koja se donosi za ceo sistem, a ne pogodnost jednog modula. Sadržaj zajedničkog jezgra detaljnije opisuje dokument [Gradivni elementi](gradivni-elementi.md).

## Sastavljanje aplikacije

Iako moduli imaju granice u kodu, pokreće se jedna aplikacija. Glavna aplikacija sastavlja module tako što pri pokretanju svaki modul registruje svoje klase u kontejner zavisnosti ([Registracija zavisnosti](../registracija-zavisnosti.md)). Nakon toga zahtevi stižu u jedan proces, a granice modula postoje u kodu, a ne između procesa.
