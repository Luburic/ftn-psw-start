# Zahtevi platformskom timu

> **Status: normativan.** Koraci u ovom dokumentu odgovaraju stvarnoj organizaciji projekta i obavezni su za sve članove tima.

## Kada se primenjuje

Kada vam za zadatak treba izmena koda čiji vlasnik niste. Granica vlasništva je prosta: vaš tim menja isključivo `backend/Modules/<Ime>/` (i kasnije `frontend/src/app/modules/<ime>/`). Sve ostalo je platformsko ili tuđe, i tamo se ne commit-uje — čak ni „mala i očigledna" izmena.

Platformsko je, konkretno: `Host.Api`, `Host.Tests`, `Shared/*`, modul `Identity`, `Explorer.slnx`, `Directory.Build.props`, `Directory.Packages.props`, CI konfiguracija u `.github/`, manifest alata u `.config/` i dokumentacija u `docs/`.

## Tri vrste zahteva

**1. Nova biblioteka (NuGet paket).** Verzije svih paketa su centralno pinovane u `Directory.Packages.props`, pa novu biblioteku uvodi platformski tim, a ne vaš `.csproj`. Pre zahteva proverite da li problem rešava nešto što projekat već ima — spisak je upravo ta datoteka. U zahtevu navedite: koji problem rešavate, zašto postojeće nije dovoljno i koji paket predlažete. Računajte da je odgovor „ne" legitiman ishod: svaka biblioteka koju jedan tim uvede postaje deo sistema koji svi održavaju.

**2. Izmena zajedničkog koda (`Shared`, `Host.Api`, CI...).** Uputite zahtev kada naiđete na potrebu koju vaš modul ne može da reši kod sebe: novi tip greške koji middleware treba da mapira, pomoćni kod koji bi po prirodi bio zajednički, podešavanje build-a. U zahtevu opišite potrebu, ne rešenje — platformski tim odlučuje da li je potreba zaista zajednička ili je premeštanje u `Shared` samo pogodnost. Podrazumevani odgovor na „može li ovo u Shared" je „ne, dok se ista potreba ne pojavi u više modula".

**3. Proširenje `Contracts` projekta.** Poseban slučaj: `Contracts` je dogovor **dva** tima, pa zahtev ne ide platformskom timu, nego timu vlasniku modula čiji vam podatak treba. Tražite minimalno: identifikatore i primitivne tipove, samo polja koja stvarno koristite. DTO klase modula ne sele se u `Contracts` — kontrakt je zaseban, dogovoren tip. Oba tima pregledaju PR koji menja `Contracts`. Platformski tim se uključuje samo ako se timovi ne dogovore.

## Kako se zahtev upućuje

1. Formulišite zahtev pisano, na kanalu tima (ne u četiri oka): koji zadatak radite, šta vas blokira, šta predlažete.
2. Ne čekajte blokirani — nastavite deo zadatka koji ne zavisi od zahteva, ili privremeno rešite unutar svog modula pa zabeležite da se prepravi kada zahtev prođe.
3. Kada je zahtev odobren, izmenu izvodi vlasnik (platformski tim ili tim vlasnik kontrakta). Ako se dogovorite da izmenu izvedete vi, PR pregleda i odobrava vlasnik.

## Šta nikada ne raditi

- Ne menjajte verzije ili spisak paketa u `Directory.Packages.props` „samo da proradi".
- Ne dodajte referencu na projekat drugog modula mimo njegovog `Contracts` projekta — arhitektonski testovi to obaraju, i to je namerno.
- Ne isključujte i ne prepravljajte arhitektonske testove u `Host.Tests` da bi build prošao; oni su specifikacija arhitekture, pad znači da je izmena pogrešna.
