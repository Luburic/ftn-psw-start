
Spisak tura iz lekcije o sastavljanju komponenti upisan je u klasu, a u pravoj aplikaciji
ture stižu sa servera. Time dolazimo do petog problema iz lekcije o Angularu: razmene
podataka sa serverom.

Sam zahtev nije teško poslati. Dovoljno je pozvati `fetch`, sačekati odgovor i pročitati
JSON. Međutim, stranici je oko tog poziva potrebno još mnogo toga. Dok odgovor putuje,
treba da prikaže da se podaci učitavaju. Ako server vrati grešku, treba da prikaže poruku
umesto podataka. Kada se promeni podatak od kog zahtev zavisi, na primer identifikator
bloga u adresi, treba da pošalje nov zahtev. Na kraju, prevodilac mora da zna tip odgovora,
da bi šablon mogao da koristi njegova svojstva.

U React-u se sve to piše ručno: `fetch` se poziva unutar `useEffect`, a podaci, učitavanje
i greška čuvaju se u tri odvojena `useState`. Angular sve to objedinjuje u jedan objekat,
koji upoznajemo u ovoj lekciji.

## Priprema za razmenu sa serverom

Deo radnog okvira za razmenu podataka sa serverom, `HttpClient`, od Angulara 21 je dostupan
za ubrizgavanje i bez dodatnog podešavanja. Poziv `provideHttpClient` dodajemo kada HTTP
treba **konfigurisati**, što je slučaj u našem projektu, jer registrujemo interceptor koji uz
svaki zahtev šalje token prijavljenog korisnika:

```ts
providers: [
  provideBrowserGlobalErrorListeners(),
  provideRouter(routes, withComponentInputBinding()),
  provideHttpClient(withInterceptors([authInterceptor])),
],
```

Razvojni server na adresi `localhost:4200` isporučuje samo datoteke aplikacije, a serverska
aplikacija sluša na adresi `localhost:5000`. Datoteka `proxy.conf.json` iz projekta
razvojnom serveru nalaže da svaki zahtev čija adresa počinje sa `/api` prosledi serverskoj
aplikaciji:

```json
{
  "/api": {
    "target": "http://localhost:5000",
    "secure": false
  }
}
```

U datom kodu treba uočiti sledeće:

- Datoteka je povezana sa razvojnim serverom kroz opciju `proxyConfig` u datoteci
  `angular.json`, pa je `ng serve` čita pri svakom pokretanju.
- Zbog prosleđivanja svaka adresa u klijentskom kodu počinje sa `/api` i ne sadrži naziv
  servera. Prosleđivanje postoji samo u razvoju. U produkciji isti posao radi server na kom
  je aplikacija objavljena.
- `authInterceptor` je funkcionalni interceptor iz `core/`, što je zasebna tema, a
  ovde je važno samo da je razlog zbog kog projekat i dalje poziva `provideHttpClient`.

## Resurs

**Resurs** (engl. *resource*) je objekat koji šalje HTTP zahtev na zadatu adresu, a odgovor,
stanje učitavanja i grešku izlaže kao signale. Pravi ga poziv `httpResource`. Sledeći kod
prikazuje, iz projekta, stranicu sa spiskom objavljenih tura, skraćenu na resurs, i tip u
kom server vraća spisak:

```ts
export interface PageResult<T> {
  items: T[];
  totalCount: number;
}
```

```ts
import { Component } from '@angular/core';
import { httpResource } from '@angular/common/http';

@Component({
  selector: 'app-tour-list',
  imports: [TourCard],
  templateUrl: './tour-list.html',
  styleUrl: './tour-list.scss',
})
export class TourList {
  protected readonly tours = httpResource<PageResult<TourDto>>(
    () => '/api/exploration/tours/published?page=1&pageSize=20',
  );
}
```

U datom kodu treba uočiti sledeće:

- Poziv `httpResource` stoji u inicijalizatoru polja, kao i `inject`. Resurs šalje zahtev
  ubrzo nakon što Angular napravi komponentu, bez ikakvog poziva iz klase.
- Parametar generičkog tipa, `PageResult<TourDto>`, je tip koji prevodilac dodeljuje
  odgovoru. Server spisak sa stranama vraća u strukturi `PageResult`, sa svojstvima `items`
  i `totalCount`, pa je tip `PageResult<TourDto>`, a ne `TourDto[]`. Interfejs `PageResult`
  je zajednički za sve module. To je generički tip koji sami deklarišemo, po istom obrascu
  kao funkcija `first<T>` iz lekcije o TypeScript-u.
- Signal `value` drži odgovor. Pre nego što odgovor stigne, njegova vrednost je `undefined`,
  pa je tip signala `PageResult<TourDto> | undefined`. Zato ga u kodu čitamo uz `?.` i
  podrazumevanu vrednost, npr. `tours.value()?.items ?? []`.
- Kako se `value` bezbedno prikazuje u šablonu, zajedno sa stanjem učitavanja i greškom,
  pokazuje odeljak o stanjima resursa.

Pretraga iz lekcije o kontroli toka ostaje ista, samo izvedeni signal sada čita ture iz
resursa, tačno kako to radi `TourList` u projektu:

```ts
protected readonly visibleTours = computed<TourDto[]>(() => {
  const all = this.tours.value()?.items ?? [];
  const name = this.nameFilter().toLowerCase();
  return all.filter((tour) => tour.name.toLowerCase().includes(name));
});
```

Izvedeni signal je čitalac resursa, pa se preračunava i kada korisnik kuca u polje za
pretragu i kada stigne odgovor servera.

## Adresa kao funkcija signala

Resursu ne predajemo samu adresu, već funkciju koja vraća adresu. Razlog je taj što adresa
često zavisi od podatka koji se menja dok je stranica otvorena. Na primer, stranica bloga
čita blog čiji je identifikator u adresi internet čitača:

```ts
export class BlogDetail {
  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);
}
```

Resurs izvršava funkciju adrese i pri tome pamti koje signale ona poziva. Ovde funkcija
poziva ulaz `id`, pa resurs zna da adresa zavisi od njega. Kada se `id` promeni, resurs
ponovo izvršava funkciju, dobija novu adresu i šalje nov zahtev. Ako prethodni zahtev još
nije završen, prekida ga, jer njegov odgovor više nije potreban.

Da smo resursu predali samo tekst adrese, adresa bi se izračunala jednom, u trenutku
pravljenja resursa, i resurs ne bi saznao da se `id` kasnije promenio. Funkciju, sa druge
strane, može ponovo da izvrši kad god zatreba.

U datom kodu treba uočiti sledeće:

- Kada korisnik otvori `/social/1`, ulaz `id` ima vrednost `'1'`, pa resurs šalje zahtev na
  `/api/social/blogs/1`. Kada zatim otvori `/social/2`, Angular zadržava istu komponentu i
  u ulaz upisuje `'2'`. Resurs ponovo izvršava funkciju i šalje zahtev na
  `/api/social/blogs/2`.
- Tip odgovora je `BlogDto`, a ne `PageResult<BlogDto>`, jer server jedan blog vraća
  direktno, bez strukture za spisak sa stranama.
- Funkcija adrese spiska tura, `() => '/api/exploration/tours/published?page=1&pageSize=20'`,
  ne poziva nijedan signal. Resurs tada nema od čega da zavisi, pa zahtev šalje samo jednom.

Funkcija adrese sme i da vrati `undefined`, i tada resurs ne šalje zahtev. To je korisno kad
podatak od kog adresa zavisi još ne postoji (primer nije iz projekta, ali pokazuje
mogućnost):

```ts
protected readonly selectedTour = httpResource<TourDto>(() => {
  const id = this.selectedTourId();
  return id === null ? undefined : `/api/exploration/tours/${id}`;
});
```

Dok je `selectedTourId` jednak `null`, zahtev se ne šalje. Čim se postavi identifikator,
funkcija vraća adresu i resurs šalje zahtev.

## Stanja resursa

Pored signala `value`, resurs izlaže i signale `isLoading` i `error`. Šablon proverava
stanja redom i za svako prikazuje drugi deo. Sledeći kod prikazuje, iz projekta, šablon
spiska tura (`tour-list.html`), skraćen na deo koji zavisi od resursa:

```html
@if (tours.isLoading()) {
  <p>Loading tours...</p>
} @else if (tours.error()) {
  <p class="error">Could not load tours. Log in and try again.</p>
} @else if (visibleTours().length === 0) {
  <p>No tours match the filter.</p>
} @else {
  <div class="grid">
    @for (tour of visibleTours(); track tour.id) {
      <app-tour-card [tour]="tour" />
    }
  </div>
}
```

U datom kodu treba uočiti sledeće:

- Redosled grana je bitan. Prvo proveravamo `isLoading()`, pa `error()`, i tek u poslednjoj
  grani čitamo vrednost. Signal `value` u stanju greške baca grešku pri čitanju, pa ga
  namerno čitamo tek kada znamo da nema ni učitavanja ni greške.
- Signal `isLoading` je tačan dok zahtev putuje, i pri svakom ponovnom slanju.
- Signal `error` drži grešku kada zahtev ne uspe, na primer kada server vrati odgovor sa
  statusnim kodom greške, a `undefined` u svakom drugom slučaju.
- Sva tri signala menjaju resurs, pa Angular ponovo proverava šablon pri svakoj promeni
  stanja. Klasa ništa ne prati.
- Poziv `this.tours.reload()`, ponovo šalje zahtev na trenutnu adresu.
  Klasa je poziva nakon što sama promeni podatke na serveru.

Resurs služi samo za čitanje podataka. Zahtevi koji menjaju podatke na serveru, poput
pravljenja ili brisanja ture, šalju se direktno kroz `HttpClient`, a nakon njih klasa poziva `reload` da bi resurs pročitao nove
podatke.

## Moje ture

Povežimo pojmove u stranicu sa turama prijavljenog korisnika iz modula Exploration u
projektu, skraćenu na čitanje:

```ts
import { Component } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-tours',
  imports: [RouterLink],
  templateUrl: './my-tours.html',
  styleUrl: './my-tours.scss',
})
export class MyTours {
  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');
}
```

```html
<h1>My tours</h1>

<p><a routerLink="/exploration/create">Create tour</a></p>

@if (tours.isLoading()) {
  <p>Loading your tours...</p>
} @else if (tours.error()) {
  <p class="error">Could not load your tours. Log in and try again.</p>
} @else {
  <table class="table">
    <tbody>
      @for (tour of tours.value() ?? []; track tour.id) {
        <tr>
          <td>{{ tour.name }}</td>
          <td>{{ tour.difficulty }}</td>
        </tr>
      } @empty {
        <tr>
          <td colspan="2">You have no tours yet.</td>
        </tr>
      }
    </tbody>
  </table>
}
```

U datom kodu treba uočiti sledeće:

- Ovde je tip odgovora `TourDto[]`, a ne `PageResult<TourDto>`, jer server rutu
  `/api/exploration/tours/mine` vraća kao običan niz. Zato u petlji stoji
  `tours.value() ?? []`, a ne `tours.value()?.items ?? []`.

Kada korisnik otvori adresu `/exploration/mine`, dešava se sledeće:

1. Angular pravi komponentu `MyTours` na mestu iscrtavanja. Inicijalizator polja `tours`
   pravi resurs.
2. Ubrzo zatim resurs izvršava funkciju i šalje zahtev na
   `/api/exploration/tours/mine`. Signal `isLoading` je tačan, pa se prikazuje poruka o
   učitavanju.
3. Odgovor stiže. Resurs upisuje niz tipa `TourDto[]` u `value`, a u `isLoading` netačno.
   Šablon je čitalac ovih signala, pa Angular ponovo proverava šablon.
4. Pošto `isLoading` i `error` sada nisu tačni, prikazuje se poslednja grana sa tabelom.
   Petlja ispisuje po jedan red za svaku turu, ili blok `@empty` kada korisnik nema tura.

Da server umesto spiska vrati grešku, npr. zato što korisnik nije prijavljen, u koraku 4
resurs bi upisao grešku u `error`, pa bi se prikazala druga grana sa porukom o grešci, a do
čitanja `value` uopšte ne bi ni došlo.
