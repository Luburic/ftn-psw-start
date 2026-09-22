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

Kada stranici jednog modula treba nešto iz drugog modula, biramo ovim redom:

1. **Navigacija**, kada korisnik treba da ode na tuđi ekran. Stranica ima vezu ka adresi drugog modula i pri tome ne uvozi ništa, jer je adresa običan tekst, a šta se na njoj prikazuje odlučuje taj modul. Kada bi modul Payment imao stranicu sa kupljenim turama, ona bi ka spisku tura vodila vezom `routerLink="/exploration"`.
2. **Podatak spojen na serveru**, kada treba prikazati tuđi podatak, a ne tuđi ekran. Podaci dva modula se ne skupljaju na klijentu, o čemu je naredni odeljak.
3. **Ugrađivanje**, kada je potreban ceo komad tuđeg ekrana, zajedno sa ponašanjem koje održava tuđi tim.

Kod ugrađivanja treba biti precizan, jer se lako pomisli da se time nešto postojeće ponovo upotrebljava. Ne upotrebljava se. Postojeće komponente modula Exploration ne mogu da odu u tuđi modul: `TourList` je stranica i ima svoju adresu, a `TourCard` je prikazna komponenta koja čeka gotov `TourDto`, pa bi ga modul Payment morao sam da dovuče i time saznao tuđu adresu i tuđi tip. Zato tim modula Exploration, kada ga neko zatraži, **piše novu komponentu namenjenu izvozu**: ona kroz ulaz prima identifikator, podatke dovlači sama, kroz izlaze prijavljuje akcije korisnika, a servise svog modula sama dobija ubrizgavanjem.

Oblik joj, dakle, ne diktiraju ekrani sopstvenog modula, nego ograničenje onoga ko je koristi. Otud i to što se ne uklapa u podelu iz prethodne lekcije: nije stranica, jer je ne imenuje tabela ruta, ni prikazna komponenta, jer ne čeka da joj neko preda podatke. Spolja se koristi kao prikazna — staviš je u šablon, daš joj ulaz, slušaš izlaze — a iznutra se snabdeva kao stranica.

Kada bi modul Exploration izvezao takvu komponentu, `TourSummary`, koja po identifikatoru čita turu sa servera i ispisuje naziv i težinu, stranica modula Payment bi je koristila ovako:

```ts
import { TourSummary } from '../../exploration/public-api';
```

```html
<app-tour-summary [tourId]="purchase().tourId" />
```

U datom kodu treba uočiti sledeće:

- Uvoz ide kroz javnu površinu, pa preimenovanje unutar modula Exploration ne dotiče modul Payment sve dok javna površina izvozi isto ime.
- Payment predaje `tourId`, svoje polje iz svoje kupovine, a ne podatke o turi. Ne vidi nijedan `TourDto`, ne zna nijednu adresu modula Exploration i ne uvozi nijedan njegov tip; jedini uvoz je klasa komponente.
- Navigacija ide prva jer ne stvara nikakvu zavisnost u kodu. Ugrađivanje stvara uvoz javne površine, traži dogovor dva tima i znači da izmene tuđeg tima stižu na tvoj ekran bez tvog učešća. To je ujedno razlog da se bira i razlog da se bira retko.

> **Napomena o projektu:** U početnom projektu nijedan modul ne koristi drugi. Nijedna datoteka modula ne uvozi tuđu javnu površinu — `public-api.ts` uvozi jedino tabela ruta aplikacije, zbog lenjog učitavanja — a nijedna veza iz modula ne vodi na adresu drugog modula; veze ka modulima stoje samo u zaglavlju korenske komponente. Moduli Payment i Games za sada imaju po jednu praznu stranicu, pa su `TourSummary`, kupovina i njena stranica izmišljeni za ovaj primer. Pravila iz ovog odeljka opisuju šta raditi kada se takva potreba prvi put pojavi.

## Sastavljanje podataka na serveru

Stranica modula Payment prikazuje kupovine, a uz svaku treba i naziv ture. Kupovine dolaze sa adrese modula Payment, a nazivi tura pripadaju modulu Exploration. Prvo rešenje koje pada na pamet jeste da stranica dovuče oboje i spoji sama:

```ts
// ovako ne radimo
protected readonly purchases = httpResource<PurchaseDto[]>(() => '/api/payment/purchases');
// pa za svaku kupovinu još jedan zahtev na /api/exploration/tours/<tourId>,
// pa spajanje naziva u izvedenom signalu
```

Takvo rešenje ima tri mane:

- Broj zahteva raste sa brojem kupovina. Za dvadeset kupovina odlazi dvadeset i jedan zahtev, a stranica se popunjava u talasima.
- Stranica modula Payment zna adrese modula Exploration i njegov tip, dakle tačno ono što javna površina treba da spreči.
- Pravilo o tome koja tura pripada kojoj kupovini postoji na dva mesta. Na serveru ga poseduje modul Payment, a sada ga klijent ponavlja svojom petljom. Dve kopije jednog pravila se pre ili kasnije raziđu.

Zato se podaci dva modula spajaju na serveru. Modul Payment na serveru pita modul Exploration kroz njegov kontrakt, spoji podatke i vrati jednu DTO strukturu u kojoj već stoji naziv ture. Stranica time čita jednu adresu, i to svog modula:

```ts
protected readonly purchases = httpResource<PurchaseDto[]>(() => '/api/payment/purchases');
```

a `PurchaseDto` preslikava u svoj direktorijum `api`, sa svojstvom koje nosi naziv ture i koje je server već popunio. Modul Exploration se na klijentu ne pominje nijednom.

Pravilo koje iz ovoga sledi važi i šire: podatke spaja onaj ko poseduje pravilo spajanja, a to je server.

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
