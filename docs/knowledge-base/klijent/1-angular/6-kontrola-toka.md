Stranica sa spiskom tura mora da prikaže po jednu karticu za svaku turu, a kada tura nema, poruku da je spisak prazan. U React-u se to piše kao poziv `tours.map(...)` unutar JSX-a i uslovni izraz za prazan spisak. U Angularu šablon ima sopstvene naredbe za grananje i petlju, koje zovemo **kontrola toka** (engl. *control flow*). Ovde upoznajemo naredbe koje projekat koristi, a za primer pretrage na kraju lekcije i način na koji šablon čita vrednost iz elementa.

> **Napomena:** Naredbe `@if` i `@for` postoje od Angulara 17. Starije verzije za isto ponašanje koriste direktive `*ngIf` i `*ngFor`, pa ćete na internetu često naići na primere sa njima. U projektu koristimo isključivo novi zapis.

## Grananje

Naredba `@if` prikazuje deo šablona samo kada je uslov ispunjen. Uz nju idu `@else if` i `@else`. Sledeći kod prikazuje deo šablona koji zavisi od broja tura:

```html
@if (tours().length === 0) {
  <p>Nema tura.</p>
} @else if (tours().length === 1) {
  <p>Pronađena je jedna tura.</p>
} @else {
  <p>Broj pronađenih tura: {{ tours().length }}</p>
}
```

U datom kodu treba uočiti sledeće:
- Uslov u zagradi je izraz istog oblika kao u interpolaciji. Ovde čita signal `tours`, pa se grana ponovo bira kada se signal promeni.
- Deo šablona u grani koja nije izabrana ne postoji u dokumentu. Nije skriven stilom, već uopšte nije napravljen.

## Petlja

Naredba `@for` ponavlja deo šablona za svaki element niza. Sledeći kod prikazuje spisak tura:

```html
<ul>
  @for (tour of tours(); track tour.id) {
    <li>{{ tour.name }}</li>
  } @empty {
    <li>Nema tura.</li>
  }
</ul>
```

U datom kodu treba uočiti sledeće:
- Zapis `tour of tours()` uvodi promenljivu `tour` koja u svakom prolazu drži jedan element niza. Promenljiva postoji samo unutar bloka petlje.
- Deo `track tour.id` je obavezan. On saopštava Angularu po čemu prepoznaje isti element pre i posle promene niza. Kada se niz promeni, Angular pravi elemente za nove identifikatore, uklanja elemente čijih identifikatora više nema, a postojeće zadržava i samo osvežava vrednosti u njima. Podaci sa servera imaju identifikator, pa je `track tour.id` uobičajen oblik. Kada elementi nemaju identifikator, na primer u spisku poruka o greškama, piše se `track $index`, gde je `$index` redni broj elementa.
- Blok `@empty` se prikazuje kada niz nema elemenata. Zamenjuje posebnu naredbu `@if` za prazan spisak.

## Grananje sa aliasom

Polje `selectedTour` je signal koji drži izabranu turu, ili `null` kada nijedna tura nije izabrana:

```ts
protected readonly selectedTour = signal<TourDto | null>(null);
```

Kada izabranu turu prikazujemo, čitamo je više puta, za naziv i za opis:

```html
@if (selectedTour()) {
  <h2>{{ selectedTour()?.name }}</h2>
  <p>{{ selectedTour()?.description }}</p>
}
```

Ovakav zapis ima dva nedostatka. Prvi je ponavljanje: isti poziv `selectedTour()` pišemo u svakom redu. Drugi je važniji. Iako smo već unutar bloka `@if`, prevodilac svaki poziv `selectedTour()` i dalje vidi kao mogući `null`, jer ne zna da će signal pri drugom pozivu vratiti istu vrednost. Zato `.name` prolazi samo uz upitnik `?.`, iako znamo da tura sigurno postoji kada smo ušli u blok.

Oba nedostatka rešava alias. **Alias** je drugi naziv koji vrednosti uslova dajemo zapisom `as` i pod kojim je koristimo unutar bloka:

```html
@if (selectedTour(); as tour) {
  <h2>{{ tour.name }}</h2>
  <p>{{ tour.description }}</p>
} @else {
  <p>Nijedna tura nije izabrana.</p>
}
```

U datom kodu treba uočiti sledeće:
- Zapis `as tour` smešta vrednost signala `selectedTour` u promenljivu `tour`. Dalje u bloku čitamo `tour`, bez ponovnog pozivanja signala.
- Promenljiva `tour` postoji samo unutar bloka `@if`, isto kao promenljiva petlje `@for`.

> **Važno:** Uslov `@if` ne proverava da li vrednost postoji, već da li je *istinita* (engl. *truthy*), isto kao `if` u JavaScript-u. Neistinite su `null` i `undefined`, ali i `0`, prazan tekst `''` i `false`.
>
> Zato alias koristimo samo tamo gde „nema vrednosti" zaista znači `null`, kao kod izabrane ture. Za broj sviđanja bi bio zamka: `@if (likes(); as count)` ne bi prikazao ništa kada je broj nula, iako je nula vrednost koju želimo da vidimo. Takvu vrednost čitamo direktno, bez `@if`:
>
> ```html
> <span>{{ likes() }}</span>
> ```

## Referenca na element šablona

Šablon ponekad treba da pročita vrednost koju je korisnik uneo u element, na primer tekst u polju za pretragu. **Referenca na element šablona** (engl. *template reference variable*) je zapis `#naziv` na elementu, koji tom elementu daje ime dostupno u ostatku šablona. Sledeći kod prikazuje polje za pretragu po nazivu i dugme koje briše pretragu:

```html
<input #nameInput type="search" [value]="nameFilter()" (input)="nameFilter.set(nameInput.value)" />
<button type="button" (click)="nameFilter.set('')">Obriši pretragu</button>
```

U datom kodu treba uočiti sledeće:
- Zapis `#nameInput` daje elementu ime. Ime se odnosi na sam HTML element, pa `nameInput.value` čita njegovo svojstvo `value`, isto kao u čistom JavaScript-u.
- Događaj `input` se dešava pri svakoj promeni sadržaja polja. Vezivanje događaja tada upisuje sadržaj polja u signal `nameFilter`.
- Vezivanje svojstva `[value]` ide u suprotnom smeru: kada se signal promeni, sadržaj polja se usklađuje sa njim. Bez njega bi dugme „Obriši pretragu“ ispraznilo signal, ali bi tekst ostao u polju. Sa oba vezivanja signal i polje ne odstupaju jedno od drugog.

## Pretraga tura

Povežimo pojmove u stranicu koja filtrira spisak tura po nazivu. Spisak je za sada upisan u klasu, a u jednoj od narednih lekcija stizaće sa servera:

```ts
import { Component, computed, signal } from '@angular/core';
import { TourDto } from './tour-dto';

@Component({
  selector: 'app-tour-list',
  templateUrl: './tour-list.html',
  styleUrl: './tour-list.scss',
})
export class TourList {
  private readonly tours = signal<TourDto[]>([
    { id: '1', name: 'Stari grad', description: 'Šetnja kroz tvrđavu.', difficulty: 'Easy', tags: ['istorija'] },
    { id: '2', name: 'Fruška gora', description: 'Planinarenje do manastira.', difficulty: 'Hard', tags: ['priroda'] },
  ]);

  protected readonly nameFilter = signal('');

  protected readonly visibleTours = computed(() => {
    const name = this.nameFilter().toLowerCase();
    return this.tours().filter((tour) => tour.name.toLowerCase().includes(name));
  });
}
```

```html
<input #nameInput type="search" [value]="nameFilter()" (input)="nameFilter.set(nameInput.value)" />
<button type="button" (click)="nameFilter.set('')">Obriši pretragu</button>

<ul>
  @for (tour of visibleTours(); track tour.id) {
    <li>{{ tour.name }}</li>
  } @empty {
    <li>Nijedna tura ne odgovara pretrazi.</li>
  }
</ul>
```

U datom kodu treba uočiti sledeće:
- Polje `tours` je signal, iako se u ovom primeru ne menja. Kada ture budu stizale sa servera, spisak će se menjati, a izvedeni signal `visibleTours` će tada automatski pratiti i tu promenu.
- Polje `tours` je `private`, jer ga šablon ne čita. Šablon prikazuje samo `visibleTours`.

Kada korisnik unese slovo u polje za pretragu, dešava se sledeće:
1. Događaj `input` upisuje sadržaj polja u signal `nameFilter`.
2. Izvedeni signal `visibleTours` je čitalac signala `nameFilter`, pa se označava za ponovno računanje.
3. Petlja `@for` čita `visibleTours`, pa Angular ponovo proverava šablon. Pri tome `visibleTours` računa novi niz.
4. Na osnovu `track tour.id` Angular zadržava elemente spiska za ture koje su i dalje u nizu, a uklanja ostale.
5. Kada nijedna tura ne odgovara pretrazi, niz je prazan i prikazuje se blok `@empty`.

Klik na dugme „Obriši pretragu“ prolazi isti put: signal `nameFilter` dobija prazan tekst, polje za unos se prazni, a spisak ponovo prikazuje sve ture.

Naredbe `@if` i `@for` čitaju signale isto kao interpolacija, pa su njihovi čitaoci. Grana i spisak se zato ponovo biraju čim se ti signali promene.
