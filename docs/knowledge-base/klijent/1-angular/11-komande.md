Spisak tura iz lekcije o sastavljanju komponenti upisan je u klasu, a u pravoj aplikaciji ture stižu sa servera. Time dolazimo do petog problema iz lekcije o Angularu: razmene podataka sa serverom.

Sam zahtev nije teško poslati. Dovoljno je pozvati `fetch`, sačekati odgovor i pročitati JSON. Međutim, stranici je oko tog poziva potrebno još mnogo toga. Dok odgovor putuje, treba da prikaže da se podaci učitavaju. Ako server vrati grešku, treba da prikaže poruku umesto podataka. Kada se promeni podatak od kog zahtev zavisi, na primer identifikator ture u adresi, treba da pošalje nov zahtev. Na kraju, prevodilac mora da zna tip odgovora, da bi šablon mogao da koristi njegova svojstva.

U React-u se sve to piše ručno: `fetch` se poziva unutar `useEffect`, a podaci, učitavanje i greška čuvaju se u tri odvojena `useState`. Angular sve to objedinjuje u jedan objekat, koji upoznajemo u ovoj lekciji.

## Priprema za razmenu sa serverom

Deo radnog okvira za razmenu podataka sa serverom, `HttpClient`, od Angulara 21 je podrazumevano dostupan i ne mora posebno da se uključuje. Poziv `provideHttpClient()` potreban je tek kada `HttpClient` želimo da podesimo, npr. da mu dodamo presretač koji uz svaki zahtev šalje token, što radimo u lekciji o prijavi. Konfiguracija projekta zato izgleda ovako:

```ts
providers: [
  provideBrowserGlobalErrorListeners(),
  provideRouter(routes, withComponentInputBinding()),
  provideHttpClient(),
],
```

Razvojni server na adresi `localhost:4200` isporučuje samo datoteke aplikacije, a serverska aplikacija sluša na adresi `localhost:5000`. Datoteka `proxy.conf.json` iz projekta razvojnom serveru nalaže da svaki zahtev čija adresa počinje sa `/api` prosledi serverskoj aplikaciji:

```json
{
  "/api": {
    "target": "http://localhost:5000"
  }
}
```

U datom kodu treba uočiti sledeće:
- Datoteka je povezana sa razvojnim serverom kroz opciju `proxyConfig` u datoteci `angular.json`, pa je `ng serve` čita pri svakom pokretanju.
- Zbog prosleđivanja svaka adresa u klijentskom kodu počinje sa `/api` i ne sadrži naziv servera. Prosleđivanje postoji samo u razvoju. U produkciji isti posao radi server na kom je aplikacija objavljena.

## Resurs

**Resurs** (engl. *resource*) je objekat koji šalje HTTP zahtev na zadatu adresu, a odgovor, stanje učitavanja i grešku izlaže kao signale. Pravi ga poziv `httpResource`.

Adrese servera ne pišemo po komponentama, već ih držimo u servisu. Tako su sve adrese jednog modula na jednom mestu, a komponenta ne zna ništa o HTTP-u, već samo traži podatke od servisa. Sledeći kod prikazuje, iz projekta, servis koji čita ture, za sada sa jednom metodom, i tip u kom server vraća spisak:

```ts
export interface PageResult<T> {
  items: T[];
  totalCount: number;
}
```

```ts
import { Injectable } from '@angular/core';
import { httpResource } from '@angular/common/http';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourQueries {
  published() {
    return httpResource<PageResult<TourDto>>(() => `${BASE_URL}/published?page=1&pageSize=20`);
  }
}
```

Stranica sa spiskom objavljenih tura preuzima servis i od njega dobija resurs:

```ts
import { Component, inject } from '@angular/core';

@Component({
  selector: 'app-tour-list',
  imports: [TourCard],
  templateUrl: './tour-list.html',
  styleUrl: './tour-list.scss',
})
export class TourList {
  private readonly tourQueries = inject(TourQueries);

  protected readonly tours = this.tourQueries.published();
}
```

```html
@if (tours.hasValue()) {
  @for (tour of tours.value().items; track tour.id) {
    <app-tour-card [tour]="tour" />
  }
}
```

U datom kodu treba uočiti sledeće:
- Metoda `published` ne šalje zahtev sama, već pravi i vraća resurs. Resurs šalje zahtev ubrzo nakon što nastane, bez ikakvog poziva iz klase.
- Stranica poziva metodu `published` u inicijalizatoru polja, kao i `inject`. Zato resurs pripada stranici, iako je napravljen u servisu. Kada korisnik napusti stranicu i Angular je uništi, uništava se i resurs, a zahtev koji još putuje se prekida. Metodu zato ne pozivamo iz drugih metoda klase, npr. pri kliku.
- Polje `tourQueries` je deklarisano pre polja `tours`, jer se inicijalizatori polja izvršavaju redom. Da je redosled obrnut, `this.tourQueries` bi u trenutku pravljenja resursa još bio `undefined`.
- Resurs nije signal, pa se ne čita pozivom `tours()`. To je objekat koji u sebi drži više signala, kao što servis `Auth` drži signale `user` i `isLoggedIn`. Zato čitamo njegove delove, npr. `tours.value()`.
- Parametar generičkog tipa, `PageResult<TourDto>`, je tip koji prevodilac dodeljuje odgovoru. Server spisak sa stranama vraća u strukturi `PageResult`, sa svojstvima `items` i `totalCount`, pa je tip `PageResult<TourDto>`, a ne `TourDto[]`. Interfejs `PageResult` je zajednički za sve module. To je generički tip koji sami deklarišemo, po istom obrascu kao funkcija `first<T>` iz lekcije o TypeScript-u.
- Signal `value` drži odgovor. Pre nego što odgovor stigne, njegova vrednost je `undefined`, pa je tip signala `PageResult<TourDto> | undefined`.
- Metoda `hasValue()` vraća tačno kada odgovor postoji. Unutar bloka `@if (tours.hasValue())` prevodilac zna da `value()` nije `undefined`, pa pišemo `tours.value().items`, bez `?.`. To je isto sužavanje tipa kao kod aliasa u lekciji o kontroli toka.
- Ovaj šablon za sada ne prikazuje ništa dok se ture učitavaju ni kada server vrati grešku. Kako se ta stanja prikazuju, pokazuje odeljak o stanjima resursa.

Pretraga iz lekcije o kontroli toka ostaje ista, samo izvedeni signal sada čita ture iz resursa:

```ts
protected readonly visibleTours = computed(() => {
  if (!this.tours.hasValue()) {
    return [];
  }
  const name = this.nameFilter().toLowerCase();
  return this.tours.value().items.filter((tour) => tour.name.toLowerCase().includes(name));
});
```

Dok odgovor nije stigao, ili ako je server vratio grešku, izvedeni signal vraća prazan spisak. Kada odgovor stigne, vraća ture čiji naziv sadrži tekst iz polja za pretragu, bez obzira na velika i mala slova. Izvedeni signal zavisi i od resursa i od polja za pretragu, pa se preračunava i kada korisnik kuca i kada stigne odgovor servera.

## Adresa kao funkcija signala

Resursu ne predajemo samu adresu, već funkciju koja vraća adresu. Razlog je taj što adresa često zavisi od podatka koji se menja dok je stranica otvorena. Na primer, stranica jedne ture čita turu čiji je identifikator u adresi internet čitača, isto kao stranica bloga iz lekcije o ruteru. Servis `TourQueries` za to dobija metodu `byId`:

```ts
byId(id: Signal<string | null>) {
  return httpResource<TourDto>(() => {
    const value = id();
    return value === null ? undefined : `${BASE_URL}/${value}`;
  });
}
```

```ts
export class TourDetail {
  readonly id = input.required<string>();

  private readonly tourQueries = inject(TourQueries);

  protected readonly tour = this.tourQueries.byId(this.id);
}
```

Resurs izvršava funkciju adrese i pri tome pamti koje signale ona poziva. Ovde funkcija poziva signal `id`, pa resurs zna da adresa zavisi od njega. Kada se `id` promeni, resurs ponovo izvršava funkciju, dobija novu adresu i šalje nov zahtev. Ako prethodni zahtev još nije završen, prekida ga, jer njegov odgovor više nije potreban.

Da smo resursu predali samo tekst adrese, adresa bi se izračunala jednom, u trenutku pravljenja resursa, i resurs ne bi saznao da se `id` kasnije promenio. Funkciju, sa druge strane, može ponovo da izvrši kad god zatreba.

U datom kodu treba uočiti sledeće:
- Stranica metodi `byId` predaje **sam signal** `this.id`, a ne njegovu vrednost `this.id()`. Samo tako funkcija adrese može da poziva signal i da resurs prati njegove promene. Da smo predali `this.id()`, metoda bi dobila samo tekst `'1'` i resurs nikada ne bi saznao da je korisnik otvorio drugu turu.
- Kada korisnik otvori `/exploration/1`, ulaz `id` ima vrednost `'1'`, pa resurs šalje zahtev na `/api/exploration/tours/1`. Kada zatim otvori `/exploration/2`, Angular zadržava istu komponentu i u ulaz upisuje `'2'`. Resurs ponovo izvršava funkciju i šalje zahtev na `/api/exploration/tours/2`.
- Tip odgovora je `TourDto`, a ne `PageResult<TourDto>`, jer server jednu turu vraća direktno, bez strukture za spisak sa stranama.
- Funkcija adrese metode `published` ne poziva nijedan signal. Resurs tada nema od čega da zavisi, pa zahtev šalje samo jednom.

Metoda `byId` prima signal tipa `string | null`, jer podatak od kog adresa zavisi ponekad još ne postoji. Na primer, stranica sa spiskom može da prikaže detalje ture tek kada je korisnik izabere:

```ts
protected readonly selectedTourId = signal<string | null>(null);
protected readonly selectedTour = this.tourQueries.byId(this.selectedTourId);
```

Dok je `selectedTourId` jednak `null`, funkcija adrese vraća `undefined`, a resurs ne šalje zahtev. Čim korisnik izabere turu, funkcija vraća adresu i resurs šalje zahtev. Ulaz `id` sa stranice ture je tipa `string`, ali ga metoda i dalje prihvata, jer je svaki tekst ujedno i vrednost tipa `string | null`.

## Stanja resursa

Pored signala `value`, resurs izlaže i signale `isLoading` i `error`. Šablon proverava stanja redom i za svako prikazuje drugi deo. Sledeći kod prikazuje, iz projekta, šablon spiska tura:

```html
@if (tours.hasValue()) {
  @for (tour of tours.value().items; track tour.id) {
    <app-tour-card [tour]="tour" />
  } @empty {
    <p>No tours yet.</p>
  }
} @else if (tours.error()) {
  <p class="error">Could not load tours. Log in and try again.</p>
} @else if (tours.isLoading()) {
  <p>Loading tours...</p>
}
```

U datom kodu treba uočiti sledeće:
- Grana sa spiskom je prva i čuva je `hasValue()`. Kada zahtev ne uspe, čitanje signala `value` baca grešku, a `hasValue()` tada vraća netačno. Zato `value()` čitamo samo unutar te grane.
- Signal `error` drži grešku kada zahtev ne uspe, na primer kada server vrati odgovor sa statusnim kodom greške, a `undefined` u svakom drugom slučaju.
- Signal `isLoading` je tačan dok zahtev putuje, i pri svakom ponovnom slanju.
- Sva tri signala menja resurs, pa Angular ponovo proverava šablon pri svakoj promeni stanja. Klasa ništa ne prati.
- Metoda `reload`, poziv `this.tours.reload()`, ponovo šalje zahtev na trenutnu adresu. Klasa je poziva nakon što sama promeni podatke na serveru.

Resurs služi samo za čitanje podataka. Zahtevi koji menjaju podatke na serveru, poput pravljenja ili brisanja ture, šalju se direktno kroz `HttpClient`, iz posebnog servisa, što je tema naredne lekcije. Nakon takvog zahteva klasa poziva `reload`, da bi resurs pročitao nove podatke.

## Moje ture

Povežimo pojmove u stranicu sa turama prijavljenog korisnika iz modula Exploration u projektu, skraćenu na čitanje. Servis `TourQueries` dobija metodu `mine`, pa ceo servis izgleda ovako:

```ts
import { Injectable, Signal } from '@angular/core';
import { httpResource } from '@angular/common/http';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourQueries {
  published() {
    return httpResource<PageResult<TourDto>>(() => `${BASE_URL}/published?page=1&pageSize=20`);
  }

  mine() {
    return httpResource<TourDto[]>(() => `${BASE_URL}/mine`);
  }

  byId(id: Signal<string | null>) {
    return httpResource<TourDto>(() => {
      const value = id();
      return value === null ? undefined : `${BASE_URL}/${value}`;
    });
  }
}
```

```ts
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-tours',
  imports: [RouterLink],
  templateUrl: './my-tours.html',
  styleUrl: './my-tours.scss',
})
export class MyTours {
  private readonly tourQueries = inject(TourQueries);

  protected readonly tours = this.tourQueries.mine();
}
```

```html
<h1>My tours</h1>

<p><a routerLink="/exploration/create">Create tour</a></p>

@if (tours.hasValue()) {
  <table>
    <tbody>
      @for (tour of tours.value(); track tour.id) {
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
} @else if (tours.error()) {
  <p class="error">Could not load your tours. Log in and try again.</p>
} @else if (tours.isLoading()) {
  <p>Loading your tours...</p>
}
```

U datom kodu treba uočiti sledeće:
- Konstanta `BASE_URL` drži zajednički početak adrese, a svaka metoda na njega nadovezuje svoj deo. Adrese modula Exploration tako su na jednom mestu.
- Svaka metoda servisa pravi nov resurs. Dve stranice koje pozovu `mine()` dobijaju dva odvojena resursa, svaki vezan za svoju stranicu.
- Stranica ne zna adresu `/api/exploration/tours/mine`. Zna samo da od servisa traži ture korisnika.

Kada korisnik otvori adresu `/exploration/mine`, dešava se sledeće:
1. Angular pravi komponentu `MyTours` na mestu iscrtavanja. Inicijalizator polja `tourQueries` preuzima servis, a inicijalizator polja `tours` poziva metodu `mine`, koja pravi resurs.
2. Ubrzo zatim resurs izvršava funkciju adrese i šalje zahtev na `/api/exploration/tours/mine`. Signal `isLoading` je tačan, a `hasValue()` netačno, pa se prikazuje poruka o učitavanju.
3. Razvojni server prosleđuje zahtev serverskoj aplikaciji, jer adresa počinje sa `/api`.
4. Odgovor stiže. Resurs upisuje niz tipa `TourDto[]` u `value`, a u `isLoading` netačno. Šablon je čitalac ovih signala, pa Angular ponovo proverava šablon.
5. Sada `hasValue()` vraća tačno, pa se prikazuje tabela. Petlja ispisuje po jedan red za svaku turu, ili blok `@empty` kada korisnik nema tura.

Da server umesto spiska vrati grešku, npr. zato što korisnik nije prijavljen, u koraku 4 resurs bi upisao grešku u `error`, `hasValue()` bi ostao netačan i prikazala bi se poruka o grešci.
