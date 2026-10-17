Analizirajmo sada komponentu za prikaz tura. U ranijim primerima smo ugradili listu tura u kod Angular komponente, dok u praksi najčešće ovakve podatke dobijamo sa servera, putem HTTP zahteva.

Bez ikakve Angular magije, mogli bismo pozvati JS `fetch` da formiramo HTTP zahteva, sačekamo odgovor i pročitamo JSON. Međutim, komponenti je oko tog poziva potrebno još nekoliko stvari. Dok odgovor putuje, treba da prikaže da se podaci učitavaju. Ako server vrati grešku, treba da prikaže poruku umesto podataka. Kada se promeni podatak od kog zahtev zavisi, na primer parametar pretrage tura, treba da pošalje nov zahtev. Na kraju, potrebno je definisati tip odgovora, da bi šablon mogao da koristi njegova svojstva.

Angular sve to objedinjuje u jedan objekat, koji upoznajemo u ovoj lekciji.

## Resurs

**Resurs** (engl. *resource*) je objekat koji šalje HTTP zahtev na zadatu adresu, a odgovor, stanje učitavanja i grešku izlaže kao signale. Pravi ga poziv `httpResource`. Sledeći kod prikazuje deo komponente sa spiskom tura koji se fokusira na resurs:

```ts
@Component({
  selector: 'app-tour-list',
  imports: [TourCard],
  templateUrl: './tour-list.html',
  styleUrl: './tour-list.scss',
})
export class TourList {
  protected readonly tours = httpResource<TourDto[]>(() => '/api/tours');

  // Ostatak koda
}
```

U datom kodu treba uočiti sledeće:

- Poziv `httpResource` stoji u inicijalizatoru polja, jer, kao i `inject`, radi samo dok Angular pravi objekat klase. Resurs šalje zahtev nakon što Angular napravi komponentu, bez ikakvog poziva iz klase.
- Adresa počinje sa `/api` i ne sadrži adresu servera (ona je navedena u konfiguraciji aplikacije).
- Parametar generičkog tipa, `TourDto[]`, je tip koji prevodilac dodeljuje odgovoru.

Da bi se pristupilo podacima koji su odbijeni iz HTTP zahteva, poziva se signal `value` nad `tours` resursom. Sledeći kod ovo ilustruje:

```ts
protected readonly visibleTours = computed(() => {
  const all = this.tours.value() ?? [];
  const difficulty = this.difficultyFilter();
  return all.filter((tour) => difficulty === null || tour.difficulty === difficulty);
});
```

Signal `value` drži odgovor. Pre nego što odgovor stigne, njegova vrednost je `undefined`. Zato ga u kodu čitamo uz podrazumevanu vrednost, npr. `tours.value() ?? []`. U primeru izvedeni signal `visibleTours` je čitalac signala `value`, pa se preračunava i kada korisnik izabere težinu i kada stigne odgovor servera.

## Adresa kao funkcija signala

Resursu ne predajemo samu adresu, već funkciju koja vraća adresu. Razlog je taj što adresa često zavisi od podatka koji se menja dok je stranica otvorena. Na primer, stranica bloga čita blog čiji je identifikator u adresi internet čitača:

```ts
export class BlogDetail {
  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/blogs/${this.id()}`);
}
```

Resurs izvršava funkciju adrese i pri tome pamti koje signale ona poziva. Ovde funkcija poziva ulaz `id`, pa resurs zna da adresa zavisi od njega. Kada se `id` promeni, resurs ponovo izvršava funkciju, dobija novu adresu i šalje nov zahtev. Ako prethodni zahtev još nije završen, prekida ga, jer njegov odgovor više nije potreban.

Da smo resursu predali samo tekst adrese, adresa bi se izračunala jednom, u trenutku pravljenja resursa, i resurs ne bi saznao da se `id` kasnije promenio. Funkciju, sa druge strane, može ponovo da izvrši kad god zatreba. Tako kada korisnik otvori `/blogs/1`, ulaz `id` ima vrednost `'1'`, pa resurs šalje zahtev na `/api/blogs/1`. Kada zatim otvori `/blogs/2`, Angular zadržava istu komponentu i u ulaz upisuje `'2'`. Resurs ponovo izvršava funkciju i šalje zahtev na `/api/blogs/2`.

Funkcija adrese sme i da vrati `undefined`, i tada resurs ne šalje zahtev. To je korisno kad podatak od kog adresa zavisi još ne postoji:

```ts
protected readonly selectedTour = httpResource<TourDto>(() => {
  const id = this.selectedTourId();
  return id === null ? undefined : `/api/tours/${id}`;
});
```

Dok je `selectedTourId` jednak `null`, zahtev se ne šalje. Čim se postavi identifikator, funkcija vraća adresu i resurs šalje zahtev.

## Stanja resursa

Pored signala `value`, resurs izlaže i signale `isLoading` i `error`. Šablon proverava stanja redom i za svako prikazuje drugi deo. Sledeći kod prikazuje šablon spiska tura (`tour-list.html`), skraćen na deo koji zavisi od resursa:

```html
@if (tours.isLoading()) {
  <p>Učitavanje tura...</p>
} @else if (tours.error()) {
  <p class="error">Ture nije moguće učitati.</p>
} @else {
  @for (tour of visibleTours(); track tour.id) {
    <app-tour-card [tour]="tour" />
  } @empty {
    <p>Nijedna tura nema izabranu težinu.</p>
  }
}
```

U datom kodu treba uočiti sledeće:

- Redosled grana je bitan. Prvo proveravamo `isLoading()`, pa `error()`, i tek u poslednjoj grani, preko izvedenog signala `visibleTours`, čitamo vrednost. Signal `value` u stanju greške baca grešku pri čitanju, pa ga namerno čitamo tek kada znamo da nema ni učitavanja ni greške.
- Signal `isLoading` je tačan dok zahtev putuje, i pri svakom ponovnom slanju.
- Signal `error` drži grešku kada zahtev ne uspe, na primer kada server vrati odgovor sa statusnim kodom greške, a `undefined` u svakom drugom slučaju.
- Sva tri signala menja resurs, pa Angular ponovo proverava šablon pri svakoj promeni stanja.

Kada korisnik otvori stranicu sa spiskom tura, dešava se sledeće:

1. Angular pravi komponentu `TourList`. Inicijalizator polja `tours` pravi resurs.
2. Ubrzo zatim resurs izvršava funkciju i šalje zahtev na `/api/tours`. Signal `isLoading` je tačan, pa se prikazuje poruka o učitavanju.
3. Odgovor stiže. Resurs upisuje niz tipa `TourDto[]` u `value`, a u `isLoading` netačno. Šablon je čitalac ovih signala, pa Angular ponovo proverava šablon.
4. Pošto `isLoading` i `error` sada nisu tačni, prikazuje se poslednja grana. Petlja ispisuje po jednu karticu za svaku turu iz izvedenog signala `visibleTours`, ili blok `@empty` kada je taj niz prazan.

Da server umesto spiska vrati grešku, u koraku 3 resurs bi upisao grešku u `error`, pa bi se prikazala druga grana sa porukom o grešci, a do čitanja `value` uopšte ne bi ni došlo.
