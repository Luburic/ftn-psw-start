# Server

Serverski deo našeg projekta je jedna ASP.NET Core aplikacija podeljena na *feature* module, a svaki tim poseduje jedan modul. Server nosi domenska pravila i podatke: prima zahteve klijenta, proverava pravila domena, upisuje izmene u bazu podataka i vraća odgovor. Da bismo takav kod pisali i čitali, treba da razumemo tri stvari: kako radni okvir dovodi zahtev do našeg koda, kako je kod jednog modula organizovan i kako se više modula sastavlja u jednu aplikaciju.

## Mapa direktorijuma

1. [ASP.NET Core](1-aspnet/README.md) - Radni okvir preuzima tehničke probleme zajedničke svim serverskim aplikacijama, a zauzvrat propisuje kako se aplikacija strukturira. Lekcije obrađuju obradu zahteva, kontrolere i middleware, kontejner zavisnosti i asinhrono programiranje.
2. [Arhitektura modula](2-arhitektura-modula/README.md) - Svaki modul prati čistu arhitekturu, u kojoj svaki sloj nosi jednu vrstu odgovornosti, a sve zavisnosti vode ka domenskom sloju. Lekcije obrađuju domenski, aplikacioni, infrastrukturni i API sloj, a zatim sva četiri sloja zajedno.
3. [Modularni monolit](3-modularni-monolit/README.md) - Više modula se sastavlja u jednu aplikaciju, a granice između njih proveravaju arhitektonski testovi. Lekcije obrađuju kontrakt kao javnu površinu modula prema drugim modulima, zajedničko jezgro sa gradivnim elementima i arhitektonske testove.
