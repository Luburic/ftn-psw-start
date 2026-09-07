# Server

Serverski deo našeg projekta je jedna ASP.NET Core aplikacija podeljena na *feature* module. Svaki modul realizuje jednu poslovnu sposobnost, poseduje sav kod i podatke potrebne za nju i iznutra prati istu višeslojnu arhitekturu. Da bismo takav kod pisali i čitali, treba da razumemo tri stvari: kako radni okvir dovodi zahtev do našeg koda, kako je kod jednog modula organizovan po slojevima i kako se više modula sastavlja u jednu aplikaciju.

## Mapa direktorijuma

1. [ASP.NET Core](1-aspnet/README.md) - Radni okvir preuzima tehničke probleme zajedničke svim serverskim aplikacijama, a zauzvrat propisuje kako se aplikacija strukturira. Lekcije obrađuju obradu zahteva i najmanju aplikaciju dovoljnu za rad, kontrolere i middleware, kontejner zavisnosti sa životnim vekovima i registracijom po modulu i asinhrono programiranje.
2. [Arhitektura modula](2-arhitektura-modula/README.md) - Kod jednog modula grupišemo po slojevima čiste arhitekture, gde svaki sloj nosi jednu vrstu odgovornosti i sve zavisnosti vode ka domenskom sloju. Segment redom obrađuje domenski sloj oblikovan taktičkim obrascima dizajna vođenog domenom, aplikacioni sloj sa komandama, upitima i DTO strukturama, infrastrukturni sloj sa objektno-relacionim maperom, migracijama, repozitorijumima i jedinicom posla, API sloj sa kontrolerima i na kraju sva četiri sloja zajedno.
3. [Modularni monolit](3-modularni-monolit/README.md) - Više modula se razvija i pokreće kao jedna aplikacija, a granice između njih postoje u samom kodu. Tri lekcije obrađuju kontrakte kroz koje modul dolazi do podataka drugog modula, zajedničko jezgro koje svi moduli dele i arhitektonske testove koji pravila o zavisnostima proveravaju automatski.
