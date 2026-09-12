Klijentska aplikacija je podeljena na iste feature module kao i serverska, pa svaki tim poseduje i modul na klijentu. Posmatrajmo stranicu modula Payment koja prodaje turu i treba da prikaže naziv i težinu ture koju korisnik kupuje. Najkraći put je da stranica uveze karticu ture iz modula Exploration:

```ts
import { TourCard } from '../../exploration/tour-browsing/tour-card/tour-card';
```

U datom kodu treba uočiti sledeće:

- Putanja prolazi kroz unutrašnjost modula Exploration. Kada tim tog modula preimenuje direktorijum `tour-browsing` ili karticu, stranica modula Payment prestaje da se prevodi, a tim modula Payment za promenu saznaje tek tada.
- Kartica je napravljena za spisak objavljenih tura, pa ima ulaze i izgled tog ekrana. Modul Payment sada zavisi od odluka donetih za tuđi ekran.
- Dva tima dele jednu datoteku, a da se o tome nisu dogovorila.

Isti problem je na serveru rešio kontrakt, javna površina modula namenjena drugim modulima. Na klijentu istu ulogu ima jedna datoteka.

## Javna površina

**Javna površina** (engl. *public API*) modula je datoteka `public-api.ts` u korenu modula, jedina datoteka modula koju sme da uveze datoteka van njega. Sledeći kod prikazuje javnu površinu modula Social iz projekta:

```ts
export { socialRoutes } from './social.routes';
export type { BlogDto } from './api/social-api-types';
```

U datom kodu treba uočiti sledeće:

- Datoteka ne sadrži sopstveni kod, već samo izvozi ono što je definisano na drugim mestima u modulu. Naredba `export ... from` čini vrednost iz druge datoteke dostupnom pod putanjom ove datoteke.
- Prvi red izvozi tabelu ruta modula. Kroz nju modul ulazi u aplikaciju, što razmatramo u narednom odeljku.
- Drugi red izvozi preslikani tip, uz ključnu reč `type`. Tip postoji samo za prevodioca i pri prevođenju u JS nestaje, pa uvoz tipa ne unosi kod drugog modula u modul koji ga koristi.
- Pored tabele ruta i tipova, javna površina sme da izvozi komponente namenjene drugim modulima. Modul Social ih još nema.
- Javna površina nikada ne izvozi servis. Servis sa `providedIn: 'root'` je jedan objekat za celu aplikaciju, pa bi dva modula koja ga preuzmu delila stanje kroz taj objekat, a nigde ne bi bilo zapisano koji modul to stanje menja.

Proširenje javne površine je dogovor dva tima, kao i proširenje kontrakta na serveru. Tim kome treba komponenta ili tip drugog modula traži od vlasnika modula da ga izveze, a vlasnik odlučuje šta izvozi i u kom obliku.

## Ulazak modula u aplikaciju

Tabela ruta aplikacije živi u datoteci `core/app.routes.ts` i ima jednu stavku za svaki modul. Sledeći kod prikazuje tu tabelu iz projekta:

```ts
export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  {
    path: 'exploration',
    loadChildren: () =>
      import('../modules/exploration/public-api').then((m) => m.explorationRoutes),
  },
  {
    path: 'games',
    loadChildren: () => import('../modules/games/public-api').then((m) => m.gamesRoutes),
  },
  {
    path: 'social',
    loadChildren: () => import('../modules/social/public-api').then((m) => m.socialRoutes),
  },
  {
    path: 'payment',
    loadChildren: () => import('../modules/payment/public-api').then((m) => m.paymentRoutes),
  },
];
```

U datom kodu treba uočiti sledeće:

- Prve tri stavke imaju svojstvo `component`, jer početna stranica, prijava i registracija ne pripadaju nijednom modulu.
- Stavka modula umesto svojstva `component` ima svojstvo `loadChildren`, funkciju koja vraća tabelu ruta modula. Svojstvo `path` je prefiks modula. Kada adresa počinje prefiksom, radni okvir poziva funkciju i ostatak adrese poklapa u tabeli koju ona vrati.
- Poziv `import('putanja')` sa zagradama je uvoz koji se izvršava tek kada se funkcija pozove, a ne pri pokretanju aplikacije, i vraća obećanje. Ovakav uvoz zovemo **lenjo učitavanje** (engl. *lazy loading*). Alat za prevođenje kod tako uvezene datoteke, zajedno sa svim što ona uvozi, objedinjuje u zasebnu objedinjenu datoteku, koju internet čitač preuzima tek kada korisnik prvi put otvori adresu sa prefiksom modula.
- Poziv `then` iz obećanja čita izvezenu tabelu ruta. Parametar `m` je objekat sa svim izvozima javne površine.
- Putanja uvoza se završava na `public-api`. Tabela aplikacije modul poznaje samo kroz javnu površinu, a modul ulazi u aplikaciju jednom stavkom i ničim drugim. Stranice modula, njegovi servisi i direktorijum `api` nigde van modula nisu pomenuti.

## Korišćenje drugog modula

Kada stranici jednog modula treba nešto iz drugog modula, postoje dva načina, koji se biraju ovim redom:

1. **Navigacija.** Stranica ima vezu ka adresi drugog modula. Stranica sa kupljenim turama u modulu Payment ima vezu `routerLink="/exploration"` ka spisku tura. Modul Payment pri tome ne uvozi ništa, jer je adresa tekst, a šta se na toj adresi prikazuje odlučuje modul Exploration.
2. **Ugrađivanje.** Stranica u svom šablonu koristi komponentu koju drugi modul izvozi kroz javnu površinu. Takva komponenta kroz ulaz prima identifikator, kroz izlaze prijavljuje akcije korisnika, a servise svog modula preuzima sama. Stranica koja je ugrađuje tako ostaje stranica svog modula i ne preuzima ništa iz tuđeg.

Kada bi modul Exploration izvozio komponentu `TourSummary`, koja po identifikatoru sama čita turu sa servera i ispisuje naziv i težinu, stranica modula Payment bi je koristila ovako:

```ts
import { TourSummary } from '../../exploration/public-api';
```

```html
<app-tour-summary [tourId]="purchase().tourId" />
```

U datom kodu treba uočiti sledeće:

- Uvoz ide kroz javnu površinu, pa preimenovanje unutar modula Exploration ne dotiče modul Payment sve dok javna površina izvozi isto ime.
- Ugrađena komponenta je izuzetak od podele na stranice i prikazne komponente. Nije stranica, jer je ne imenuje tabela ruta, a preuzima servis i deklariše resurs, jer podatke ne sme da dobije od stranice tuđeg modula.
- Navigacija ide prva jer ne stvara zavisnost u kodu. Ugrađivanje stvara uvoz javne površine i traži dogovor dva tima.

## Sastavljanje podataka na serveru

Stranica modula Payment koja prikazuje kupovine sa nazivom ture mogla bi da čita adresu `/api/payment/purchases`, pa za svaku kupovinu adresu `/api/exploration/tours/...`, i da nazive spaja u izvedenom signalu. Takva stranica šalje jedan zahtev više po kupovini, zna adrese tuđeg modula i ponavlja pravilo o tome koja tura pripada kupovini, koje na serveru poseduje modul Payment. Podaci dva modula se zato sastavljaju na serveru. Modul Payment na serveru pita modul Exploration kroz njegov kontrakt i vraća jednu DTO strukturu sa nazivom ture, a stranica čita jednu adresu svog modula i preslikava tu strukturu u svoj direktorijum `api`.

## Od adrese do stranice modula

Povežimo pojmove. Prijavljeni korisnik je na početnoj stranici i klikne na vezu ka adresi `/social/mine`. Dešava se sledeće:

1. Veza upisuje adresu `/social/mine` u internet čitač.
2. Radni okvir u tabeli aplikacije nalazi stavku sa prefiksom `social` i poziva njenu funkciju `loadChildren`.
3. Poziv `import` preuzima objedinjenu datoteku modula Social, ako je internet čitač već nema, a `then` iz njene javne površine čita `socialRoutes`.
4. Radni okvir ostatak adrese, `mine`, poklapa u tabeli modula i bira komponentu `MyBlogs`.
5. Na mestu iscrtavanja uništava početnu stranicu i pravi `MyBlogs`, čiji resurs šalje zahtev na `/api/social/blogs/mine`.

Nijedna datoteka van modula Social nije pomenula stranicu `MyBlogs`. Aplikacija zna samo da modul Social postoji na prefiksu `social` i da njegovu tabelu ruta dobija iz javne površine.
