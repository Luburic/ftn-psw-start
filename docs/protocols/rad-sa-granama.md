# Rad sa granama i pull request-ovima

## Kada se primenjuje

Svakodnevno. Svaka izmena ulazi u `development` isključivo kroz pull request; direktan push na `development` ili `main` nije dozvoljen.

## Grana po funkcionalnosti

Pre nego što se pristupi razvoju nove funkcionalnosti, pravi se nova grana od svežeg `development`-a. Veština za implementaciju (`psw-implementacija`) ovo radi automatski. Ime grane prati obrazac `<modul>/<kratak-opis>` (npr. `exploration/tour-review`). Jedna grana nosi jednu zaokruženu izmenu, a ne nedovršen ili parcijalan razvoj.

## Dnevna higijena

Preporuka je da na početku svake sesije razvoja povučete `development` u svoju granu kroz sledeće komande (sve se pokreću iz korena repozitorijuma):

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

Time konflikte rešavate dok su mali i dok pamtite kontekst. Ako spajanje ipak prijavi konflikt na datoteci snimka modela migracija, prekinite ga i pratite isti protokol jer se taj konflikt se ne rešava ručnim spajanjem.

Posle spajanja pokrenite `dotnet tool restore` iz korena repozitorijuma i `npm install` iz direktorijuma `frontend`, jer je spajanje možda donelo nov alat ili novu biblioteku.

Ovaj proces je obavezno da ispratite pre formiranje pull request-a.

## Pre otvaranja pull request-a

Prođite kroz sledeću listu:

1. `development` je povučen u granu i konflikti su rešeni.
2. `dotnet build` prolazi iz direktorijuma `backend` (upozorenja su greške).
3. `dotnet test` prolazi za sve module, ne samo vaš. Arhitekturalni testovi u `Host.Tests` proveravaju pravila zavisnosti i ako oni padnu, ispravlja se kod, a ne test.
4. `npm run lint` i `npm run build` prolaze iz direktorijuma `frontend`. Linter čuva granicu modula na klijentu; ako prijavi uvoz iz drugog modula, ispravlja se kod, ne `eslint.config.js`.
5. `git status` ne prikazuje izmene izvan direktorijuma vašeg modula (`backend/Modules/<Ime>/` i `frontend/src/app/modules/<ime>/`). Izmena tuđeg ili platformskog koda u vašem PR-u je znak da nešto nije u redu. Vidite protokol [Zahtevi platformskom timu](zahtevi-platformskom-timu.md).

## Pull request

- Opis navodi nameru funkcionalnosti (ko ima korist i koju), dizajn u glavnim crtama (tok korišćenja, domenski model, operacije i testovi) i način na koji je izmena proverena. Veština `/psw-implementacija` na kraju rada ispisuje takav pregled, koji možete da prenesete u opis.
- PR pregleda bar jedan član vašeg tima koji nije pisao izmenu. Ako PR dira `Contracts`, pregleda ga i tim koji taj contract koristi.

## Kada CI padne

CI izvršava isto što i vi lokalno, u tri odvojena posla. Posao `build-and-test` prevodi i testira serversku aplikaciju (restore, build, testovi sa PostgreSQL servisom), posao `frontend` izvršava `npm ci`, `npm run lint` i `npm run build`, a posao `migrations`, samo za pull request, ponavlja proveru migracija iz dnevne higijene. Sva tri moraju da prođu. Zato je prvi korak uvek reprodukcija lokalno:

1. Otvorite u GitHub Actions posao koji je pao, pa log palog koraka, i pročitajte prvu grešku.
2. Pokrenite isti korak lokalno: iz `backend` (`dotnet build`, pa `dotnet test`) ili iz `frontend` (`npm run lint`, pa `npm run build`). Ako lokalno prolazi, a na CI pada, najčešći uzrok je datoteka koja nije commit-ovana ili push-ovana, što proveravate sa `git status`. Pad posla `migrations` se ne reprodukuje: poruka greške imenuje vašu migraciju i migraciju iz ciljne grane koja joj nedostaje, a ispravka je protokol [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md).
3. Popravka ide kao novi commit na istu granu; CI se pokreće ponovo sam.
4. PR sa crvenim CI se ne pregleda i ne spaja. Ako ne umete da protumačite pad, tražite pomoć odmah. Ne ostavljajte crven PR da čeka.
