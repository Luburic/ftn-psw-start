# Arhitektura modula

Prethodni segment je pokazao kako zahtev stiže do našeg koda i kako radni okvir sastavlja objekte koji taj kod izvršavaju. Ostaje pitanje kako taj kod organizujemo. Najkraći put je da kontroler primi zahtev, sam učita podatke iz baze, proveri pravila domena, upiše izmenu i vrati odgovor. Za mali softver sa malo pravila to radi. Kako pravila rastu, klasa koja istovremeno poznaje HTTP protokol, domen problema i bazu podataka postaje teška za održavanje, jer se svaka promena tehnologije i svaka promena pravila slivaju na isto mesto, a nijedno pravilo se ne može proveriti bez pokretanja cele aplikacije.

Odgovor je da kod grupišemo po **slojevima**, gde svaki sloj nosi jednu vrstu odgovornosti i ne bavi se detaljima ostalih slojeva. U našem projektu svaki modul prati istu višeslojnu arhitekturu, koju zovemo čista arhitektura.

## Čista arhitektura

**Čista arhitektura** (engl. *clean architecture*, srodna obrascima *ports & adapters*, *hexagonal* i *onion*) je višeslojna arhitektura čiji je osnovni cilj da domenska pravila ostanu izolovana od tehničkih detalja. Ovo postiže prateći princip inverzije zavisnosti i enkapsulirajući domenski model. Kod aplikacije koja prati čistu arhitekturu je raspoređen u četiri sloja:

- **Domenski sloj** sadrži koncepte i pravila domena problema. Njegove klase koriste jezik domena i ne znaju ništa o tehničkim detaljima poput HTTP zahteva, SQL upita ili metoda biblioteka. U našem projektu ovaj sloj oblikujemo taktičkim obrascima dizajna vođenog domenom, pa se sastoji od agregata i domenskih servisa.
- **Aplikacioni sloj** koordiniše korake jednog slučaja korišćenja: radi sa domenskim objektima i infrastrukturnim servisima i poziva njihove metode u dobrom redosledu. Domenske objekte ne izlaže kodu koji ga poziva, već ih oblikuje u DTO strukture koje prihvata i vraća.
- **Infrastrukturni sloj** implementira interfejse aplikacionog sloja konkretnom tehnologijom. Ovde žive klase koje rade sa bazom podataka, komuniciraju sa drugim sistemima ili koriste specijalizovane biblioteke.
- **API sloj** pretvara poruke koje šalje spoljašnji svet u pozive aplikacije i vraća odgovor spram rezultata. Tako kontroler iz HTTP zahteva izdvaja podatke koje aplikacioni servis očekuje, poziva ga i njegov rezultat prevodi u HTTP odgovor.

Izolacija domenskih pravila se postiže kroz pravilo da domenski sloj ne referencira nijedan drugi sloj. Enkapsulacija domenskog modela se postiže tako što aplikacioni servisi prihvataju i vraćaju DTO strukture umesto domenskih objekata, pa kod van aplikacionog sloja domenske objekte ne vidi i ne može da pozove njihove metode. Princip inverzije zavisnosti znači da aplikacioni sloj ne zavisi od infrastrukturnog sloja, čije servise aktivira, već od njihovih interfejsa koje sam deklariše, a da infrastrukturni sloj zavisi od tih interfejsa. Uz navedeno, API sloj zavisi od aplikacionog sloja jer ga aktivira, dok ne postoji zavisnost u obrnutom smeru. Sve zavisnosti tako vode ka domenskom sloju, a nijedna od njega.

## Mapa direktorijuma

Slojeve upoznajemo počev od domenskog i svaki naredni sloj definišemo kroz ono što radi za prethodni. Poslednji dokument sve slojeve sagledava zajedno.

1. [Domenski sloj](1-domenski-sloj/README.md) - Pet lekcija o taktičkim obrascima dizajna vođenog domenom, redom od najprostijeg ka najsloženijem: zašto biramo bogat domenski model isečen na celine, vrednosni objekat, entitet, agregat kao granica konzistentnosti sa korenom kao jedinom tačkom izmene i domenski servis za pravila koja obuhvataju više agregata. Preduslov je za sve naredne direktorijume.
2. [Aplikacioni sloj](2-aplikacioni-sloj/README.md) - Tri lekcije o aplikacionom servisu: princip razdvajanja komandi od upita i oblik koji daje klasama sloja, tri oblika metoda servisa sa postupkom kojim za nov zahtev biramo oblik i DTO strukture kojima podaci prelaze granicu sloja, uključujući maper.
3. [Infrastrukturni sloj](3-infrastrukturni-sloj/README.md) - Tri vrste klasa koje implementiraju tehničke sposobnosti (repozitorijumske, konektorske i stručnjačke) i lekcije o objektno-relacionom mapiranju kroz Entity Framework Core i o repozitorijumima i jedinici posla koji ga koriste.
4. [API sloj](4-api-sloj.md) - Kontroler kao adapter protokola: koje korake obavlja za jedan zahtev i zašto ne sadrži ništa više. Oslanja se na [lekciju o kontrolerima](../1-aspnet/2-kontroleri.md).
5. [Čista arhitektura](5-čista-arhitektura.md) - Sva četiri sloja sagledana zajedno na jednom slučaju korišćenja: tok podataka kroz slojeve, četiri vrste odgovornosti i precizno pravilo o zavisnostima između slojeva. Čita se poslednji, kao zaokruženje direktorijuma.

Nakon ovog direktorijuma čitalac zna kako je jedan modul iznutra izgrađen, kojoj vrsti klase pripada koji deo koda i u kom smeru smeju da idu zavisnosti. Sledeći korak je pitanje kako se više takvih modula sastavlja u jednu aplikaciju i kako se granice između njih čuvaju, čime se bavi segment o modularnom monolitu.
