# Zahtevi platformskom timu

## Kada se primenjuje

Kada vam za zadatak treba izmena koda koji ne pripada vašem timu. Granica vlasništva je prosta: vaš tim menja isključivo `backend/Modules/<Ime>/` i `frontend/src/app/modules/<ime>/`. Sve ostalo je platformsko ili tuđe, i tamo ne trebate vi da vršite izmene i commitove, čak ni za „male i očigledne" izmene.

Platformsko timu pripadaju projekti: `Host.Api`, `Host.Tests`, `Shared/*`, modul `Identity`, `Explorer.slnx`, `Directory.Build.props`, `Directory.Packages.props`, `.csproj` datoteke svih projekata, CI konfiguracija u `.github/`, manifest alata u `.config/` i dokumentacija u `docs/`. Na klijentu su platformski direktorijumi `src/app/core`, `src/app/shared` i `src/styles`, datoteke `app.config.ts`, `styles.scss` i `eslint.config.js` i konfiguracija projekta (`package.json`, `package-lock.json`, `angular.json`, `proxy.conf.json`).

## Tri vrste zahteva

**1. Nova biblioteka (NuGet ili npm paket).** Verzije svih serverskih paketa su centralno pinovane u `Directory.Packages.props`, a klijentskih u `frontend/package.json`, pa novu biblioteku uvodi platformski tim, a ne vaš `.csproj` ili vaš `npm install`. Pre zahteva proverite da li problem rešava nešto što projekat već ima — spisak su upravo te dve datoteke. U zahtevu navedite: koji problem rešavate, zašto postojeće nije dovoljno i koji paket predlažete. Računajte da je odgovor „ne" legitiman ishod: svaka biblioteka koju jedan tim uvede postaje deo sistema koji svi održavaju.

**2. Izmena zajedničkog koda (`Shared`, `Host.Api`, `core` i `shared` na klijentu, CI...).** Uputite zahtev kada naiđete na potrebu koju vaš modul ne može da reši kod sebe: novi tip greške koji middleware treba da mapira, pomoćni kod ili globalna klasa stila koji bi po prirodi bili zajednički, podešavanje build-a. U zahtevu opišite potrebu, ne rešenje — platformski tim odlučuje da li je potreba zaista zajednička i premeštanje u `Shared` je opravdano. Podrazumevani odgovor na „može li ovo u Shared" (ili u `shared` na klijentu) je „ne, dok se ista potreba ne pojavi u više modula".

**3. Proširenje `Contracts` projekta.** Poseban slučaj: `Contracts` je dogovor **dva** tima, pa zahtev ne ide platformskom timu, nego timu vlasniku modula čiji vam podatak treba. Tražite minimalno: identifikatore ili polja koja stvarno koristite. `Contracts` može da sadrži DTO klase koje definišu strukturu podataka koje dostavlja ili prihvata drugi modul. Oba tima pregledaju PR koji menja `Contracts`. Platformski tim se uključuje samo ako se timovi ne dogovore.

## Kako se zahtev upućuje

1. Formulišite zahtev pisano, na kanalu tima: koji zadatak radite, šta vas blokira, šta predlažete.
2. Ne čekajte blokirani — nastavite deo zadatka koji ne zavisi od zahteva, ili privremeno rešite unutar svog modula pa zabeležite da se prepravi kada zahtev prođe.
3. Kada je zahtev odobren, izmenu izvodi vlasnik (platformski tim ili tim vlasnik kontrakta). Ako se dogovorite da izmenu izvedete vi, PR pregleda i odobrava vlasnik.

## Šta nikada ne raditi

- Ne menjajte verzije ili spisak paketa u `Directory.Packages.props` „samo da proradi".
- Ne dodajte referencu na projekat drugog modula mimo njegovog `Contracts` projekta jer arhitektonski testovi to obaraju. Izuzetak su test projekti: test projekat sme da referencira test projekat drugog modula da bi koristio njegove početne podatke. I takvu referencu, kao i svaku izmenu `.csproj` datoteke, dodaje platformski tim na vaš zahtev.
- Ne uvozite kod iz drugog modula na klijentu jer će linter to oboriti. Do drugog modula se stiže njegovim rutama.
- Ne isključujte i ne prepravljajte arhitektonske testove u `Host.Tests` ni pravila u `eslint.config.js` da bi build prošao; oni su specifikacija arhitekture i njihov pad znači da je izmena pogrešna.
