**Čista arhitektura** (srodna obrascima *ports & adapters*, *hexagonal* i *onion*) je višeslojna arhitektura koja prati princip inverzije zavisnosti, čiji je osnovni cilj da domenski sloj ostane izolovan od tehničkih detalja. DDD čista arhitektura poseduje bogati domenski model razdeljen u agregate. Sagledaćemo konkretan primer funkcionalnosti koja prožima sve slojeve, nakon čega ćemo detaljnije analizirati svaki sloj.

Posmatrajmo softver u kojem ispitanik popunjava anketu i predaje svoje odgovore. U ovom delu domena postoje dva agregata:

- `Survey`, koji modeluje anketu, njena pitanja i pravila za prihvatanje odgovora i
- `SurveyResponse`, koji modeluje odgovore jednog ispitanika i njihov životni ciklus.

Struktura ovih agregata je ilustrovana kroz sledeći dijagram:

![](https://luburic.github.io/ftn-tutor-images/images/ddd/clean-arch-struct.png)

Zamislimo da korisnik ima mogućnost da postepeno popunjava anketu i da se odgovor na svako pitanje zasebno čuva. Kada ispitanik odgovori na pitanje, sistem treba da:

1. primi HTTP zahtev koji sadrži odgovor na pitanje,
2. učita korisnikov odgovor na celokupnu anketu, gde će pridodati odgovor na pitanje,
3. učita anketu,
4. pita anketu da li je dozvoljeno odgovarati na nju,
5. ako jeste, evidentira odgovor na pitanje u okviru celokupnog odgovora,
6. sačuva promenjeni agregat i
7. vrati rezultat.

Serverska aplikacija bi svih sedam koraka mogla da realizuje u okviru metode kontrolera. Ipak, takav potez bi rezultovao složenom metodom koja narušava više heuristika za pisanje održivog koda. Umesto toga, logiku razlažemo po slojevima, što sledeći dijagram ilustruje:

![](https://luburic.github.io/ftn-tutor-images/images/ddd/clean-arch-data-flow.png)

Vidimo da:

1. Kontroler prima HTTP zahtev i prevodi ga u poziv aplikacionog servisa.
2. Aplikacioni servis preko repozitorijuma učitava `SurveyResponse` i `Survey` agregate.
3. Aplikacioni servis pita `Survey` agregat da li je operacija dozvoljena, pozivajući njegovu metodu koja izvodi domenski značajnu informaciju.
4. Aplikacioni servis zatim poziva metodu za kontrolisanu promenu stanja `SurveyResponse` agregata.
5. Na kraju se izmenjen odgovor čuva u skladištu putem repozitorijuma i formira se odgovor za klijentsku aplikaciju.

U ovom toku postoje četiri vrste odgovornosti:

- API sloj obrađuje HTTP zahtev i formira HTTP odgovor.
- Aplikacioni sloj koordiniše korake slučaja korišćenja.
- Domenski sloj primenjuje domenska pravila da izvodi domenski značajne informacije i kontrolisano menja stanje agregata.
- Infrastrukturni sloj radi sa skladištem podataka.

Svaki sloj i klase koje mu pripadaju detaljno analizira zaseban dokument:

1. [Domenski sloj](slojevi/1-domenski-sloj.md), koji sadrži agregate i domenske servise.
2. [Aplikacioni sloj](slojevi/2-aplikacioni-sloj.md), koji sadrži aplikacione servise, DTO strukture i interfejse tehničkih sposobnosti.
3. [Infrastrukturni sloj](slojevi/3-infrastrukturni-sloj.md), koji sadrži repozitorijumske, konektorske i stručnjačke klase.
4. [API sloj](slojevi/4-api-sloj.md), koji sadrži kontrolerske klase.

## Zavisnosti između slojeva

Zavisnost jednog sloja od drugog postoji kada klasa jednog sloja direktno koristi tip iz drugog sloja. To se u kodu vidi kroz tip polja, parametra, povratne vrednosti, implementiranog interfejsa ili objekta koji se kreira.

Tok izvršavanja i smer zavisnosti nisu ista stvar. Tok izvršavanja prati redosled poziva tokom rada programa:

```text
SurveyResponseController
    -> SurveyResponseService
        -> SurveyResponseRepository
            -> Skladište podataka
```

Aplikacioni servis tokom izvršavanja poziva infrastrukturni repozitorijum. Ipak, `SurveyResponseService` ne referencira klasu `SurveyResponseRepository`. On referencira interfejs `ISurveyResponseRepository` iz aplikacionog sloja.

Čista arhitektura je višeslojna arhitektura koja prati princip inverzije zavisnosti. Rezultat ove postavke je da:
- Domenski sloj ne zavisi ni od jednog drugog sloja.
- Aplikacioni sloj zavisi isključivo od domenskog sloja jer direktno koristi njegove tipove.
- Infrastrukturni sloj referencira domenski sloj jer referencira tipove agregata. Referencira i aplikacioni sloj čije interfejse tehničkih sposobnosti, uključujući interfejse repozitorijuma, implementira. Ovaj sloj dodatno referencira konkretne biblioteke, radne okvire i SDK pakete potrebne za tehnički rad. Te zavisnosti se zadržavaju u infrastrukturi.
- API sloj zavisi isključivo od aplikacionog sloja, gde poziva metode servisa i radi sa DTO instancama. Kontroler koristi i tipove HTTP radnog okvira (ili biblioteke za drugi protokol) zato što adaptira protokol. Ti tipovi pripadaju API sloju i ne prelaze u aplikacioni ili domenski sloj.

Kada se prethodno sabere sa vrstama klasa koje pronalazimo u svakom sloju, dobijamo prikaz elemenata i zavisnosti slojeva poput sledećeg:

![](https://luburic.github.io/ftn-tutor-images/images/ddd/clean-arch-gen-struct.png)

Slika ilustruje bitno opšte pravilo koje pronalazimo u čistoj arhitekturi, a to je da smer zavisnosti ide od spoljašnjih slojeva ka unutrašnjim, gde u centru pronalazimo domenski sloj, izvan njega aplikacioni, a izvan njega API i infrastrukturni sloj.
