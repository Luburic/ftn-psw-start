Spisak tura iz lekcije o sastavljanju komponenti je upisan u klasu. Aplikacija projekta ture čita sa servera, čime dolazimo do petog problema iz lekcije o Angular-u. Čitalac zna da pozove `fetch`, sačeka odgovor i pročita JSON. Stranici oko tog poziva treba još nekoliko stvari. Dok odgovor putuje, mora da prikaže da učitava. Kada server vrati grešku, mora da je prikaže umesto podataka. Kada se promeni parametar po kom čita, na primer identifikator bloga u adresi, mora da pošalje nov zahtev. Tip odgovora mora da bude poznat prevodiocu, da bi šablon mogao da čita njegova svojstva. U React-u je čitalac sve to pisao sam, pozivom `fetch` unutar `useEffect` i sa posebnim `useState` za podatke, za učitavanje i za grešku. Angular sve to daje kroz jedan objekat, koji ovde upoznajemo.

## Priprema za razmenu sa serverom

Deo radnog okvira za razmenu podataka sa serverom uključen je u konfiguraciju aplikacije projekta, kao i rutiranje:

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
    "target": "http://localhost:5000",
    "secure": false
  }
}
```

Zato svaka adresa u klijentskom kodu počinje sa `/api` i ne sadrži naziv servera.

## Resurs

**Resurs** (engl. *resource*) je objekat koji šalje HTTP zahtev na zadatu adresu i odgovor, stanje učitavanja i grešku izlaže kao signale. Pravi ga poziv `httpResource`. Sledeći kod prikazuje, iz projekta, stranicu sa spiskom objavljenih tura, skraćenu na resurs, i tip u kom server vraća spisak:

```ts
export interface PageResult<T> {
  items: T[];
  totalCount: number;
}
```

```ts
export class TourList {
  protected readonly tours = httpResource<PageResult<TourDto>>(
    () => '/api/exploration/tours/published?page=1&pageSize=20',
  );
}
```

```html
@for (tour of tours.value()?.items ?? []; track tour.id) {
  <app-tour-card [tour]="tour" />
}
```

U datom kodu treba uočiti sledeće:
- Poziv `httpResource` stoji u inicijalizatoru polja, kao i `inject`. Resurs šalje zahtev pri prvom iscrtavanju komponente, bez ikakvog poziva iz klase.
- Argument je funkcija adrese, a ne sama adresa. Zašto je tako, objašnjava sledeći odeljak.
- Parametar generičkog tipa, `PageResult<TourDto>`, je tip koji prevodilac dodeljuje odgovoru. Server spisak sa stranama vraća u strukturi `PageResult`, sa svojstvima `items` i `totalCount`, pa je tip `PageResult<TourDto>`, a ne `TourDto[]`. Interfejs `PageResult` je zajednički za sve module i živi u direktorijumu `shared/api`. Interfejs sa parametrom `T` je generički tip koji sami deklarišemo, po istom obrascu po kom smo do sada koristili `Promise<T>`.
- Signal `value` drži odgovor. Pre nego što odgovor stigne, vrednost mu je `undefined`, JavaScript vrednost za ono što još nije dodeljeno, koju prevodilac razlikuje od `null`. Zapis `tours.value()?.items ?? []` daje prazan niz dok odgovora nema, a spisak tura kada stigne. Šablon prihvata operatore `?.` i `??` kao i JavaScript.

## Adresa kao funkcija signala

Funkcija adrese se izvršava u reaktivnom kontekstu, pa je resurs pretplatnik svakog signala koji ona pročita. Kada se neki od njih promeni, funkcija se ponovo izvršava, a resurs šalje nov zahtev i prekida zahtev koji još putuje. Adresa spiska tura ne čita nijedan signal, pa se zahtev šalje jednom. Sledeći kod prikazuje, iz projekta, stranicu bloga, čija adresa čita parametar rute:

```ts
export class BlogDetail {
  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);
}
```

U datom kodu treba uočiti sledeće:
- Funkcija čita ulaz `id`. Pri prvom iscrtavanju ulaz je već upisan, pa prvi zahtev ide na adresu bloga iz adrese pregledača. Kada korisnik otvori drugi blog, radni okvir zadržava komponentu i upisuje novu vrednost u ulaz, funkcija vraća novu adresu i resurs šalje nov zahtev.
- Tip `BlogDto` je tip jednog bloga, jer server jedan blog vraća bez strukture `PageResult`.

## Stanja resursa

Resurs pored signala `value` izlaže i signale `isLoading` i `error`. Šablon ih čita redom, pa za svako stanje prikazuje drugi deo. Sledeći kod prikazuje, iz projekta, šablon spiska tura:

```html
@if (tours.isLoading()) {
  <p>Loading tours...</p>
} @else if (tours.error()) {
  <p class="error">Could not load tours. Log in and try again.</p>
} @else {
  @for (tour of tours.value()?.items ?? []; track tour.id) {
    <app-tour-card [tour]="tour" />
  } @empty {
    <p>No tours yet.</p>
  }
}
```

U datom kodu treba uočiti sledeće:
- Signal `isLoading` je tačan dok zahtev putuje, i pri svakom ponovnom slanju.
- Signal `error` drži grešku kada zahtev ne uspe, na primer kada server vrati odgovor sa statusnim kodom greške, a `undefined` u svakom drugom slučaju. Dok je greška prisutna, čitanje signala `value` baca grešku, pa grana sa spiskom dolazi tek iza grane sa greškom.
- Sva tri signala menja resurs, pa se šablon ponovo iscrtava pri svakoj promeni stanja. Klasa ništa ne prati.
- Metoda `reload`, poziv `this.tours.reload()`, ponovo šalje zahtev na trenutnu adresu. Klasa je poziva nakon što sama promeni podatke na serveru.

## Moje ture

Povežimo pojmove u stranicu sa turama prijavljenog korisnika iz modula Exploration u projektu, skraćenu na čitanje:

```ts
@Component({
  imports: [RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
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
  <table>
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

Kada korisnik otvori adresu `/exploration/mine`, dešava se sledeće:
1. Radni okvir pravi komponentu `MyTours` na mestu iscrtavanja. Inicijalizator polja `tours` pravi resurs.
2. Šablon se iscrtava prvi put. Resurs izvršava funkciju adrese i šalje zahtev na `/api/exploration/tours/mine`, a signal `isLoading` je tačan, pa se prikazuje poruka o učitavanju.
3. Razvojni server zahtev prosleđuje serverskoj aplikaciji, jer adresa počinje sa `/api`.
4. Odgovor stiže. Resurs upisuje niz tipa `TourDto[]` u `value` i netačno u `isLoading`. Šablon je pretplatnik signala `isLoading`, pa se ponovo iscrtava.
5. Grana sa učitavanjem se uklanja, a petlja čita `value` i ispisuje po jedan red za svaku turu ili blok `@empty` kada korisnik nema tura.
