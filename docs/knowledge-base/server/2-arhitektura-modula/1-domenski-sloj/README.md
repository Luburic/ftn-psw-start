# Domenski sloj

Svaki softver koji rešava stvaran problem sadrži koncepte i pravila tog problema. Pravila određuju koje operacije su dozvoljene, šta je ispravno stanje podataka i kako se iz postojećih podataka izvode novi. Ta pravila ne zavise od toga da li aplikacija radi kroz veb ili konzolu, da li podatke čuva u relacionoj bazi ili datoteci, niti kojim radnim okvirom je izgrađena. Ona bi važila i kada bi se posao obavljao ručno, olovkom i papirom. Klase koje ta pravila i podatke predstavljaju u kodu čine **domenski sloj**.

Domenski sloj ne modeluje ceo domen problema, već onaj njegov deo koji je relevantan za rad softvera. Domen istraživanja javnog mnjenja obuhvata mnogo više pojmova nego što treba jednoj aplikaciji za sprovođenje anketa. Aplikacija modeluje ankete, pitanja, ponuđene opcije, odgovore ispitanika i pravila koja određuju kada je odgovor dozvoljen. Izbor šta ulazi u model, a šta ostaje van njega, je odluka koju donosimo za svaki modul posebno.

Klase domenskog sloja koriste jezik domena i ne znaju ništa o tehničkom okruženju u kom se izvršavaju. Ne opisuju HTTP zahteve, tabele u bazi niti način serijalizacije podataka. Ta nezavisnost omogućava da se domenska pravila čitaju, održavaju i testiraju bez ijedne tehničke zavisnosti. U našem projektu svaki modul ima svoj domenski sloj i on je prvo mesto na kom tim odlučuje šta modul zaista radi.

U našem projektu primenjujemo naprednu tehniku za modelovanje domenskog sloja koju nazivamo *dizajn vođen domenom*. Pratimo taktičke obrasce ove metodologije koji propisuju nekoliko vrsta gradivnih elemenata i pravila po kojima se oni povezuju. Ovde se upoznajemo sa tim elementima, redom od najprostijeg ka najsloženijem.

## Mapa direktorijuma

1. [Taktički obrasci](1-takticki-obrasci.md) - Dva pitanja koja postavljamo pri dizajnu domenskog modela: gde živi domenska logika i kako su objekti povezani. Lekcija poredi anemičan i bogat model, potpuno povezan graf i graf isečen na celine, i objašnjava zašto biramo kombinaciju koju propisuju DDD taktički obrasci. Preduslov je za sve naredne lekcije.
2. [Vrednosni objekat](2-vrednosni-objekat.md) - Najprostiji gradivni element: domenski značajna vrednost čiji je identitet određen vrednostima svojstava, koja je nepromenljiva, validira se pri kreiranju i može da izvodi informacije iz svog stanja.
3. [Entitet](3-entitet.md) - Domenski koncept sa životnim ciklusom i nepromenljivim identifikatorom, čije se stanje menja isključivo kroz metode koje brane invarijante. Ovde se uvodi pojam invarijante i način na koji se prekršeno pravilo prijavljuje spoljašnjim slojevima.
4. [Agregat](4-agregat.md) - Grupa entiteta i vrednosnih objekata koja se drži konzistentnom kao celina, sa korenom kao jedinom tačkom izmene i identifikatorima kao jedinom vezom ka drugim agregatima. Lekcija zaokružuje raspodelu pravila po nivoima agregata.
5. [Domenski servis](5-domenski-servis.md) - Klasa za domensko pravilo koje zahteva uvid u više agregata i ne pripada prirodno nijednom od njih.
