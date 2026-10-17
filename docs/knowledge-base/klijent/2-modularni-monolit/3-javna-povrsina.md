Klijentska aplikacija je podeljena na iste feature module kao i serverska, pa svaki tim poseduje i modul na klijentu. Posmatrajmo stranicu modula Payment koja prodaje turu i treba da prikaže naziv i težinu ture koju korisnik kupuje. Prvo što pada na pamet je da stranica uveze karticu ture iz modula Exploration:

```ts
import { TourCard } from '../../../exploration/tour-browsing/tour-card/tour-card';
```

U datom kodu treba uočiti sledeće:

- Putanja prolazi kroz unutrašnjost modula Exploration. Kada tim tog modula preimenuje direktorijum `tour-browsing` ili karticu, stranica modula Payment prestaje da se prevodi, a tim modula Payment za promenu saznaje tek tada.
- Kartica je napravljena za spisak objavljenih tura, pa ima ulaze i izgled tog ekrana. Modul Payment sada zavisi od odluka donetih za tuđi ekran.
- Dva tima dele jednu datoteku, a da se o tome nisu dogovorila.

Isti problem je na serveru rešio kontrakt, javna površina modula namenjena drugim modulima. Na klijentu istu ulogu imaju adrese stranica modula, koje upoznajemo u narednom odeljku. Granicu modula ne prelazi nijedna datoteka, pa ni kartica ni bilo koji drugi deo tuđeg ekrana. Naziv i težina ture do stranice modula Payment stižu sasvim drugim putem, sa njenog sopstvenog servera, o čemu je odeljak o sastavljanju podataka.

## Javna površina

**Javna površina** (engl. *public API*) modula na klijentu je skup adresa njegovih stranica. Drugi modul do ekrana modula stiže adresom, a ne uvozom, pa nijedna datoteka modula ne sme da uveze datoteku drugog modula. Adrese modula određuje njegova tabela ruta. Sledeći kod prikazuje tabelu ruta modula Social:

```ts
export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: ':id', component: BlogDetail },
];
```

U datom kodu treba uočiti sledeće:

- Svaka stavka povezuje deo adrese sa stranicom. Uz prefiks modula, o kom je naredni odeljak, modul Social ima četiri adrese: `/social`, `/social/mine`, `/social/create` i `/social/<id>`.
- Samo tabela ruta aplikacije uvozi tabelu ruta modula iz datoteke `social.routes.ts`, i kroz nju modul ulazi u aplikaciju. To je jedina datoteka modula koju uvozi datoteka van njega, a nijedan drugi modul je ne uvozi.
- Dok adresa ostaje ista, tim modula sme da preimenuje, premesti ili zameni stranicu na njoj, a nijedan drugi modul to ne primećuje.
- Modul drugom modulu ne predaje ni servis ni tip. Servis sa `providedIn: 'root'` je jedan objekat za celu aplikaciju, pa bi dva modula koja ga dele delila i stanje, a nigde ne bi bilo zapisano koji modul to stanje menja. Kada modulu treba podatak drugog modula, dobija ga od svog servera i preslikava u sopstveni tip, o čemu je odeljak o sastavljanju podataka.

Promena javne površine je dogovor dva tima, kao i promena kontrakta na serveru. Tim koji preimenuje ili ukloni adresu ka kojoj vodi veza iz drugog modula dogovara to sa timom tog modula.

## Ulazak modula u aplikaciju

Tabela ruta aplikacije živi u datoteci `core/app.routes.ts` i ima jednu stavku za svaki modul. Sledeći kod prikazuje deo te tabele, sa početnom stranicom i modulom Social:

```ts
export const routes: Routes = [
  { path: '', component: Home },
  {
    path: 'social',
    loadChildren: () => import('../modules/social/social.routes').then((m) => m.socialRoutes),
  },
  // ... ostale stranice i moduli
];
```

U datom kodu treba uočiti sledeće:

- Stavka početne stranice ima svojstvo `component`, koje neposredno bira komponentu `Home`.
- Stavka modula umesto svojstva `component` ima svojstvo `loadChildren`. To je funkcija koja učitava tabelu ruta modula. Svojstvo `path` je prefiks modula: za adresu `/social/42` radni okvir u tabeli aplikacije prepoznaje `social`, a ostatak, `42`, traži u tabeli modula Social.
- Poziv `import('putanja')` sa zagradama učitava datoteku `social.routes.ts` i vraća obećanje. Funkcija prosleđena pozivu `then` izvršava se kada se učitavanje završi. Njen parametar `m` sadrži ono što datoteka izvozi, pa izraz `m.socialRoutes` izdvaja tabelu ruta iz prethodnog odeljka. Obećanje te tabele je rezultat funkcije `loadChildren`; radni okvir sačeka tabelu i zatim u njoj traži odgovarajuću rutu.

Kod modula se ovim putem učitava tek kada korisnik prvi put otvori njegovu adresu. To zovemo **lenjo učitavanje** (engl. *lazy loading*). Alat za prevođenje izdvaja taj kod iz početnog paketa aplikacije, pa internet čitač ne mora odmah da preuzme kod svih modula.

## Korišćenje drugog modula

Kada stranici jednog modula treba nešto iz drugog modula, postoje dva načina:

1. **Navigacija**, kada korisnik treba da ode na tuđi ekran. Stranica ima vezu ka adresi drugog modula i pri tome ne uvozi ništa, jer je adresa običan tekst, a šta se na njoj prikazuje odlučuje taj modul. Kada bi modul Payment imao stranicu sa kupljenim turama, ona bi ka spisku tura vodila vezom `routerLink="/exploration"`.
2. **Podatak spojen na serveru**, kada treba prikazati tuđi podatak, a ne tuđi ekran. Stranica tada i dalje čita samo adresu svog modula, a server joj u odgovoru donosi ono što je uzeo od drugog modula. O tome je naredni odeljak.

Trećeg načina nema. Modul ne uzima komponentu drugog modula. Delovi ekrana ostaju unutar modula koji ih je napravio, jer su napravljeni za njegove ekrane: `TourList` je stranica i ima svoju adresu, a `TourCard` čeka gotov `TourDto`, pa bi ga modul Payment morao sam da dovuče i time saznao tuđu adresu i tuđi tip. Kada modulu Payment treba naziv ture, ne uzima tuđu karticu, nego traži da mu naziv stigne u njegovom odgovoru.

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

Povežimo pojmove. Hod ide kroz dve tabele ruta koje smo videli gore: onu iz jezgra i onu modula Social.

Korisnik je na početnoj stranici i otvara adresu `/social/42`. Dešava se sledeće:

1. Radni okvir iz adrese čita putanju `social/42`.
2. Radni okvir u tabeli aplikacije nalazi stavku sa prefiksom `social` i poziva njenu funkciju `loadChildren`.
3. Poziv `import` učitava kod modula Social ako već nije učitan, a funkcija prosleđena pozivu `then` izdvaja `socialRoutes`.
4. Radni okvir ostatak adrese, `42`, poklapa u tabeli modula. Stavke `''`, `mine` i `create` ne odgovaraju, a `:id` odgovara, pa bira komponentu `BlogDetail` i parametar `id` dobija vrednost `'42'`.
5. Na mestu elementa `router-outlet` uništava početnu stranicu i pravi `BlogDetail`, a parametar upisuje u ulaz `id` te komponente, kao u lekciji o rutiranju.

Nijedna datoteka van modula Social nije pomenula stranicu `BlogDetail`. Aplikacija zna samo da modul Social postoji na prefiksu `social` i u kojoj se datoteci nalazi njegova tabela ruta.
