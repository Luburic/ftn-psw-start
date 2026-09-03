# Aplikacioni sloj

Domenski sloj zna domen problema, njegove koncepte i pravila, ali nijedan njegov objekat ne zna kada je stigao zahtev, odakle se agregat učitava niti šta se sa njim radi nakon promene. Neko mora da učita prave agregate, da ih pozove dobrim redosledom i da sačuva rezultat, sa ciljem ispunjenja slučaja korišćenja korisnika. Taj posao koordinacije koraka za ispunjenje slučaja korišćenja pripada aplikacionom sloju.

**Aplikacioni servis** je klasa aplikacionog sloja čije javne metode koordinišu rad sa domenskim objektima, infrastrukturnim servisima i drugim modulima da se podrži slučaj korišćenja. Servis ne poseže u unutrašnjost agregata da bi sam tražio podatke ili menjao kolekcije i ne zna kako repozitorijum dolazi do podataka. U lekcijama ćemo videti da se aplikacioni servisi u kodu javljaju kao dve vrste klasa, komandne i upitne.

Tri lekcije ovog direktorijuma redom obrađuju pravilo po kom oblikujemo metode servisa, kako to pravilo izgleda u kodu i kako podaci prelaze granicu sloja.

## Mapa direktorijuma

1. [Komande i upiti](1-komande-i-upiti.md) - Princip razdvajanja metoda koje menjaju stanje (engl. *command*) od metoda koje vraćaju podatke (engl. *query*) i oblik koji to pravilo daje klasama u ovom sloju. Preduslov je za naredne dve lekcije.
2. [Aplikacioni servis](2-aplikacioni-servis.md) - Struktura aplikacionog sloja u projektu i tri oblika metoda aplikacionog servisa, komanda, čist upit i upit koji koristi agregat, sa postupkom kojim za nov zahtev biramo oblik.
3. [DTO strukture i mapiranje](3-dto-i-mapiranje.md) - Zašto podaci preko granice sloja putuju u DTO strukturama, kako se ulazna struktura prevodi u domenski objekat, a izlazna popunjava projekcijom ili maperom.

Nakon ovog direktorijuma čitalac zna kako aplikacioni servis koordiniše slučaj korišćenja i kojim podacima se granica sloja prelazi. Interfejse koje ovaj sloj deklariše implementira [infrastrukturni sloj](../3-infrastrukturni-sloj/README.md).
