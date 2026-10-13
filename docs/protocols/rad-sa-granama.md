# Rad sa granama i pull request-ovima

> **Status: normativan.** Koraci u ovom dokumentu odgovaraju stvarnoj konfiguraciji projekta i obavezni su za sve članove tima.

## Kada se primenjuje

Svakodnevno. Svaka izmena ulazi u `development` isključivo kroz pull request; direktan push na `development` ne postoji kao opcija.

## Grane

- Grana se pravi od svežeg `development`-a i živi kratko — cilj je da se spoji u roku od nekoliko dana. Duga grana znači da je zadatak prevelik; podelite ga.
- Ime grane: `<modul>/<kratak-opis>`, npr. `exploration/ocena-ture`. Iz imena se vidi koji tim je vlasnik.
- Jedna grana nosi jednu zaokruženu izmenu. Model podataka i njegova migracija idu zajedno, u istoj grani.

## Dnevna higijena

Bar jednom dnevno, i obavezno pre otvaranja pull request-a, povucite `development` u svoju granu. Sve komande se pokreću iz korena repozitorijuma. Prvo preuzmite stanje sa servera:

```
git fetch origin
```

Ako vaša grana sadrži migraciju vašeg modula, pre spajanja proverite da li je u `development` u međuvremenu stigla migracija istog modula (primer je za modul `Exploration`):

```
git diff --name-only HEAD...origin/development -- backend/Modules/Exploration/Exploration.Infrastructure/Persistence/Migrations
```

Ako komanda ispiše ijednu datoteku, ne spajajte, već pratite protokol [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md). U suprotnom spojite `development`:

```
git merge origin/development
```

Time konflikte rešavate dok su mali i dok pamtite kontekst. Ako spajanje ipak prijavi konflikt na datoteci snimka modela migracija, prekinite ga i pratite isti protokol — taj konflikt se ne rešava ručnim spajanjem.

Posle spajanja pokrenite `dotnet tool restore` iz korena repozitorijuma i `npm install` iz direktorijuma `frontend`, jer je spajanje možda donelo nov alat ili novu biblioteku.

## Pre otvaranja pull request-a

Prođite kroz ovu listu lokalno; CI proverava isto, ali je krug kroz CI sporiji od lokalne provere:

1. `development` je povučen u granu, konflikti rešeni.
2. `dotnet build` prolazi iz direktorijuma `backend` (upozorenja su greške).
3. `dotnet test` prolazi — svi moduli, ne samo vaš. Arhitektonski testovi u `Host.Tests` proveravaju pravila zavisnosti; ako oni padnu, ispravlja se kod, ne test.
4. `npm run lint` i `npm run build` prolaze iz direktorijuma `frontend`. Linter čuva granicu modula na klijentu; ako prijavi uvoz iz drugog modula, ispravlja se kod, ne `eslint.config.js`.
5. `git status` ne prikazuje izmene izvan direktorijuma vašeg modula (`backend/Modules/<Ime>/` i `frontend/src/app/modules/<ime>/`). Izmena tuđeg ili platformskog koda u vašem PR-u je znak da nešto nije u redu — vidite protokol [Zahtevi platformskom timu](zahtevi-platformskom-timu.md).

## Pull request

- Opis navodi nameru funkcionalnosti (ko ima korist i koju), dizajn u glavnim crtama (tok korišćenja, domenski model, operacije i testovi) i način na koji je izmena proverena. Veština `/psw-implementacija` na kraju rada ispisuje takav pregled, koji možete da prenesete u opis.
- PR pregleda bar jedan član vašeg tima koji nije pisao izmenu. Ako PR dira `Contracts`, pregleda ga i tim koji taj contract koristi.
- Mali PR se pregleda za deset minuta, veliki se odlaže danima — veličina PR-a je vaš uticaj na brzinu tima.

## Kada CI padne

CI izvršava isto što i vi lokalno, u tri odvojena posla. Posao `build-and-test` prevodi i testira serversku aplikaciju (restore, build, testovi sa PostgreSQL servisom), posao `frontend` izvršava `npm ci`, `npm run lint` i `npm run build`, a posao `migrations`, samo za pull request, ponavlja proveru migracija iz dnevne higijene. Sva tri moraju da prođu. Zato je prvi korak uvek reprodukcija lokalno:

1. Otvorite u GitHub Actions posao koji je pao, pa log palog koraka, i pročitajte prvu grešku, ne poslednju.
2. Pokrenite isti korak lokalno: iz `backend` (`dotnet build`, pa `dotnet test`) ili iz `frontend` (`npm run lint`, pa `npm run build`). Ako lokalno prolazi, a na CI pada, najčešći uzrok je datoteka koja nije commit-ovana ili push-ovana — proverite `git status`. Pad posla `migrations` se ne reprodukuje: poruka greške imenuje vašu migraciju i migraciju iz ciljne grane koja joj nedostaje, a ispravka je protokol [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md).
3. Popravka ide kao novi commit na istu granu; CI se pokreće ponovo sam.
4. PR sa crvenim CI se ne pregleda i ne spaja. Ako ne umete da protumačite pad, tražite pomoć odmah — ne ostavljajte crven PR da čeka.
