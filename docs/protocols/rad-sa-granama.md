# Rad sa granama i pull request-ovima

> **Status: normativan.** Koraci u ovom dokumentu odgovaraju stvarnoj konfiguraciji projekta i obavezni su za sve članove tima.

## Kada se primenjuje

Svakodnevno. Svaka izmena ulazi u `main` isključivo kroz pull request; direktan push na `main` ne postoji kao opcija.

## Grane

- Grana se pravi od svežeg `main`-a i živi kratko — cilj je da se spoji u roku od nekoliko dana. Duga grana znači da je zadatak prevelik; podelite ga.
- Ime grane: `<modul>/<kratak-opis>`, npr. `exploration/ocena-ture`. Iz imena se vidi koji tim je vlasnik.
- Jedna grana nosi jednu zaokruženu izmenu. Model podataka i njegova migracija idu zajedno, u istoj grani.

## Dnevna higijena

Bar jednom dnevno, i obavezno pre otvaranja pull request-a, povucite `main` u svoju granu:

```
git fetch origin
git merge origin/main
```

Time konflikte rešavate dok su mali i dok pamtite kontekst. Ako spajanje prijavi konflikt na datoteci snimka modela migracija, prekinite ga i pratite protokol [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md) — taj konflikt se ne rešava ručnim spajanjem.

## Pre otvaranja pull request-a

Prođite kroz ovu listu lokalno; CI proverava isto, ali je krug kroz CI sporiji od lokalne provere:

1. `main` je povučen u granu, konflikti rešeni.
2. `dotnet build` prolazi iz direktorijuma `backend` (upozorenja su greške).
3. `dotnet test` prolazi — svi moduli, ne samo vaš. Arhitektonski testovi u `Host.Tests` proveravaju pravila zavisnosti; ako oni padnu, ispravlja se kod, ne test.
4. `git status` ne prikazuje izmene izvan direktorijuma vašeg modula (`backend/Modules/<Ime>/`). Izmena tuđeg ili platformskog koda u vašem PR-u je znak da nešto nije u redu — vidite protokol [Zahtevi platformskom timu](zahtevi-platformskom-timu.md).

## Pull request

- Opis odgovara na dva pitanja: šta je izmenjeno i kako je provereno. Dovoljne su po dve-tri rečenice.
- PR pregleda bar jedan član vašeg tima koji nije pisao izmenu. Ako PR dira `Contracts`, pregleda ga i tim koji taj contract koristi.
- Mali PR se pregleda za deset minuta, veliki se odlaže danima — veličina PR-a je vaš uticaj na brzinu tima.

## Kada CI padne

CI izvršava isto što i vi lokalno: restore, build, testovi (sa PostgreSQL servisom). Zato je prvi korak uvek reprodukcija lokalno:

1. Otvorite log palog koraka u GitHub Actions i pročitajte prvu grešku, ne poslednju.
2. Pokrenite isti korak lokalno iz `backend` (`dotnet build`, pa `dotnet test`). Ako lokalno prolazi, a na CI pada, najčešći uzrok je datoteka koja nije commit-ovana ili push-ovana — proverite `git status`.
3. Popravka ide kao novi commit na istu granu; CI se pokreće ponovo sam.
4. PR sa crvenim CI se ne pregleda i ne spaja. Ako ne umete da protumačite pad, tražite pomoć odmah — ne ostavljajte crven PR da čeka.
