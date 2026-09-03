# ASP.NET Core

Svaka serverska aplikacija rešava iste tehničke probleme, nezavisno od toga čemu služi. Mora da komunicira sa spoljnim svetom po standardnim protokolima, da sastavlja objekte od kojih se sastoji, da obrađuje mnogo korisnika istovremeno i da radi sve to pouzdano i bezbedno. Ovi problemi nemaju veze sa domenom aplikacije, a rešavanje bilo kog od njih od nule je posao za tim stručnjaka. Zato ih rešava **radni okvir** (engl. *framework*) koji programera oslobađa od implementacije mnoštva tehničkih odgovornosti, kako bi se fokusirao na rešavanje problema specifičnih za njegov domen.

Radni okvir se od obične biblioteke razlikuje po tome ko koga poziva. Biblioteku pozivamo iz svog koda kada nam zatreba. Radni okvir drži kontrolu toka, a naš kod poziva na mestima koja smo mu deklarativno označili. Ova inverzija ima cenu. Radni okvir propisuje kako se aplikacija strukturira, kako se klase sastavljaju i kako se logika označava da bi je pronašao. Ta pravila trebamo upoznati kako bismo ih ispravno koristili.

Naš projekat je izgrađen uz pomoć ASP.NET Core radnog okvira za razvoj veb aplikacija na .NET platformi. Ovde se upoznajemo sa nekoliko njegovih aspekata koje ćemo svakodnevno koristiti.

## Mapa direktorijuma

1. [ASP.NET Core](1-aspnet.md) - Šta radni okvir radi pri obradi zahteva i kako izgleda najmanja aplikacija koja nam je dovoljna za rad. Ovde se prvi put sreću kontroleri i kontejner zavisnosti, pa je ova lekcija preduslov za naredne dve.
2. [Kontroleri](2-kontroleri.md) - Pravila po kojima radni okvir bira akciju, popunjava njene parametre i pretvara njenu povratnu vrednost u odgovor. Lekcija uvodi i middleware, mehanizam kojim obradu zajedničku za sve akcije, poput obrade grešaka, izdvajamo na jedno mesto.
3. [Registracija zavisnosti](3-registracija-zavisnosti.md) - Kako kontejner zavisnosti pravi objekte, koliko dugo ti objekti žive i koje oblike registracije koristimo, uključujući pozadinske servise i grupisanje registracija po modulu.
4. [Asinhrono programiranje](4-asinhrono-programiranje.md) - Zašto server ostaje bez slobodnih niti kada sinhrono čeka bazu ili drugi servis, kako `async` i `await` oslobađaju nit tokom čekanja i kako redosled poziva određuje da li se više operacija izvršava odjednom. Ova lekcija nema preduslove i može se čitati nezavisno.

Nakon ovog direktorijuma čitalac zna kako zahtev stiže do njegovog koda i kako radni okvir sastavlja objekte koji taj kod izvršavaju. Sledeći korak je pitanje kako taj kod organizujemo, čime se bavi direktorijum o arhitekturi modula.
