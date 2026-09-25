Klijentska aplikacija je podeljena na iste feature module kao i serverska, pa svaki tim poseduje i modul na klijentu. Posmatrajmo stranicu modula Payment koja prodaje turu i treba da prikaže naziv i težinu ture koju korisnik kupuje. Prvo što pada na pamet je da stranica uveze karticu ture iz modula Exploration:

```ts
import { TourCard } from '../../exploration/tour-browsing/tour-card/tour-card';
```

U datom kodu treba uočiti sledeće:

- Putanja prolazi kroz unutrašnjost modula Exploration. Kada tim tog modula preimenuje direktorijum `tour-browsing` ili karticu, stranica modula Payment prestaje da se prevodi, a tim modula Payment za promenu saznaje tek tada.
- Kartica je napravljena za spisak objavljenih tura, pa ima ulaze i izgled tog ekrana. Modul Payment sada zavisi od odluka donetih za tuđi ekran.
- Dva tima dele jednu datoteku, a da se o tome nisu dogovorila.

Isti problem je na serveru rešio kontrakt, javna površina modula namenjena drugim modulima. Na klijentu istu ulogu ima jedna datoteka, koju upoznajemo u narednom odeljku. Ona kaže šta sme da pređe granicu modula, a to nije ni kartica ni bilo koji drugi deo tuđeg ekrana. Naziv i težina ture do stranice modula Payment stižu sasvim drugim putem, sa njenog sopstvenog servera, o čemu je odeljak o sastavljanju podataka.

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
- Javna površina nikada ne izvozi servis. Servis sa `providedIn: 'root'` je jedan objekat za celu aplikaciju, pa bi dva modula koja ga preuzmu delila stanje kroz taj objekat, a nigde ne bi bilo zapisano koji modul to stanje menja.

Proširenje javne površine je dogovor dva tima, kao i proširenje kontrakta na serveru. Tim kome treba tip drugog modula traži od vlasnika modula da ga izveze, a vlasnik odlučuje šta izvozi i u kom obliku.

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
- Poziv `import('putanja')` sa zagradama je uvoz koji se izvršava tek kada se funkcija pozove, a ne pri pokretanju aplikacije. Ovakav uvoz zovemo **lenjo učitavanje** (engl. *lazy loading*). Alat za prevođenje kod tako uvezene datoteke, zajedno sa svim što ona uvozi, objedinjuje u zasebnu datoteku, koju internet čitač preuzima tek kada korisnik prvi put otvori adresu sa prefiksom modula.

## Korišćenje drugog modula

Kada stranici jednog modula treba nešto iz drugog modula, postoje dva načina:

1. **Navigacija**, kada korisnik treba da ode na tuđi ekran. Stranica ima vezu ka adresi drugog modula i pri tome ne uvozi ništa, jer je adresa običan tekst, a šta se na njoj prikazuje odlučuje taj modul. Kada bi modul Payment imao stranicu sa kupljenim turama, ona bi ka spisku tura vodila vezom `routerLink="/exploration"`.
2. **Podatak spojen na serveru**, kada treba prikazati tuđi podatak, a ne tuđi ekran. Stranica tada i dalje čita samo adresu svog modula, a server joj u odgovoru donosi ono što je uzeo od drugog modula. O tome je naredni odeljak.

Trećeg načina nema. Modul ne uzima komponentu drugog modula, pa ni kroz javnu površinu. Delovi ekrana ostaju unutar modula koji ih je napravio, jer su napravljeni za njegove ekrane: `TourList` je stranica i ima svoju adresu, a `TourCard` čeka gotov `TourDto`, pa bi ga modul Payment morao sam da dovuče i time saznao tuđu adresu i tuđi tip. Kada modulu Payment treba naziv ture, ne uzima tuđu karticu, nego traži da mu naziv stigne u njegovom odgovoru.

> **Napomena o projektu:** U početnom projektu nijedan modul ne koristi drugi. Nijedna datoteka modula ne uvozi tuđu javnu površinu tj. `public-api.ts` uvozi jedino tabela ruta aplikacije, zbog lenjog učitavanja. Ni preslikani tipovi koje javne površine izvoze, `TourDto` i `BlogDto`, zasad nemaju nijednog korisnika van svog modula.

## Sastavljanje podataka na serveru

Stranica modula Payment prikazuje kupovine, a uz svaku treba i naziv ture. Kupovine dolaze sa adrese modula Payment, a nazivi tura pripadaju modulu Exploration. Prvo rešenje koje pada na pamet jeste da stranica dovuče oboje i spoji sama:

```ts
// ovako ne radimo
protected readonly purchases = httpResource<PurchaseDto[]>(() => '/api/payment/purchases');
// pa za svaku kupovinu još jedan zahtev na /api/exploration/tours/<tourId>,
// pa spajanje naziva u izvedenom signalu
```

Takvo rešenje ima tri mane:

- Broj zahteva raste sa brojem kupovina. Za dvadeset kupovina odlazi dvadeset i jedan zahtev.
- Stranica modula Payment zna adrese modula Exploration i njegov tip.
- Pravilo o tome koja tura pripada kojoj kupovini postoji na dva mesta. Na serveru ga poseduje modul Payment, a sada ga klijent ponavlja svojom petljom.

Zato se podaci dva modula spajaju na serveru. Modul Payment na serveru pita modul Exploration kroz njegov kontrakt, spoji podatke i vrati jednu DTO strukturu u kojoj već stoji naziv ture. Stranica time čita jednu adresu, i to svog modula:

```ts
protected readonly purchases = httpResource<PurchaseDto[]>(() => '/api/payment/purchases');
```

a `PurchaseDto` preslikava u svoj direktorijum `api`, sa svojstvom koje nosi naziv ture i koje je server već popunio. Modul Exploration se na klijentu ne pominje nijednom.

## Od adrese do stranice modula

Povežimo pojmove. Hod ide kroz dve tabele ruta: onu iz jezgra, koju smo videli gore, i onu modula Social, koju njegova javna površina izvozi:

```ts
export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: ':id', component: BlogDetail },
];
```

Prijavljeni korisnik je na početnoj stranici i klikne na vezu ka adresi `/social/mine`. Dešava se sledeće:

1. Veza upisuje adresu `/social/mine` u internet čitač.
2. Radni okvir u tabeli aplikacije nalazi stavku sa prefiksom `social` i poziva njenu funkciju `loadChildren`.
3. Poziv `import` preuzima objedinjenu datoteku modula Social, ako je internet čitač već nema, a `then` iz njene javne površine čita `socialRoutes`.
4. Radni okvir ostatak adrese, `mine`, poklapa u tabeli modula. Poklapa se druga stavka, pa bira komponentu `MyBlogs`.
5. Na mestu iscrtavanja uništava početnu stranicu i pravi `MyBlogs`, čiji resurs šalje zahtev na `/api/social/blogs/mine`.

Nijedna datoteka van modula Social nije pomenula stranicu `MyBlogs`. Aplikacija zna samo da modul Social postoji na prefiksu `social` i da njegovu tabelu ruta dobija iz javne površine.