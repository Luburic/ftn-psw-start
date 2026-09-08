Stranica sa spiskom tura mora da prikaže po jednu karticu za svaku turu, a kada tura nema, poruku da je spisak prazan. U React-u je čitalac to pisao kao poziv `tours.map(...)` unutar JSX-a i uslovni izraz za prazan spisak. U Angular-u šablon ima sopstvene naredbe za grananje i petlju, koje zovemo **kontrola toka** (engl. *control flow*). Ovde upoznajemo naredbe koje projekat koristi i način na koji šablon čita vrednost iz elementa.

## Grananje

Naredba `@if` prikazuje deo šablona samo kada je uslov tačan. Uz nju idu `@else if` i `@else`. Sledeći kod prikazuje deo šablona koji zavisi od broja tura:

```html
@if (tours().length === 0) {
  <p>Nema tura.</p>
} @else if (tours().length === 1) {
  <p>Jedna tura.</p>
} @else {
  <p>{{ tours().length }} tura.</p>
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
- Deo `track tour.id` je obavezan. On saopštava radnom okviru po čemu prepoznaje isti element između dva iscrtavanja. Kada se niz promeni, radni okvir ponovo iscrtava samo elemente čiji se identifikator promenio ili koji su novi, a ostale zadržava. Podaci sa servera imaju identifikator, pa je `track tour.id` uobičajen oblik. Kada niz nema identifikator, na primer spisak poruka o greškama, piše se `track $index`, gde je `$index` redni broj elementa.
- Blok `@empty` se prikazuje kada niz nema elemenata. Zamenjuje posebnu naredbu `@if` za prazan spisak.

## Grananje sa aliasom

Vrednost koja može da nedostaje, tipa `T | null`, u šablonu se čita više puta. Svako čitanje bi moralo da proveri `null`. Naredba `@if` zato dozvoljava da vrednost uslova dobije **alias** (engl. *alias*), naziv pod kojim se koristi unutar bloka. Sledeći kod prikazuje prikaz prijavljenog korisnika:

```html
@if (auth.user(); as user) {
  <span>{{ user.email }}</span>
} @else {
  <a routerLink="/login">Prijava</a>
}
```

U datom kodu treba uočiti sledeće:
- Zapis `as user` uvodi promenljivu `user` koja unutar bloka drži vrednost izraza. Blok se prikazuje samo kada vrednost nije `null`, pa je tip promenljive `User`, bez `null`.
- Bez aliasa bi svako čitanje bilo `auth.user()?.email`, a prevodilac ne bi znao da je vrednost unutar bloka sigurno prisutna.

## Referenca na element šablona

Šablon ponekad treba da pročita vrednost koju je korisnik uneo u element, na primer tekst u polju za pretragu. **Referenca na element šablona** (engl. *template reference variable*) je zapis `#naziv` na elementu, koji tom elementu daje ime dostupno u ostatku šablona. Sledeći kod prikazuje polje za pretragu po nazivu:

```html
<input #nameInput type="search" [value]="nameFilter()" (input)="nameFilter.set(nameInput.value)" />
```

U datom kodu treba uočiti sledeće:
- Zapis `#nameInput` daje elementu ime. Ime se odnosi na sam HTML element, pa `nameInput.value` čita njegovo svojstvo `value`, isto kao u čistom JavaScript-u.
- Događaj `input` se dešava pri svakoj promeni sadržaja polja. Vezivanje događaja tada upisuje sadržaj u signal `nameFilter`.
- Vezivanje svojstva `[value]` ide u suprotnom smeru i drži sadržaj polja jednak signalu. Tako signal i polje nikada ne odstupaju jedno od drugog.

## Pretraga tura

Povežimo pojmove u stranicu koja filtrira spisak tura po nazivu. Spisak je ovde upisan u klasu, a kako stiže sa servera obrađuje [lekcija o čitanju podataka](9-citanje-podataka.md):

```ts
@Component({
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  private readonly tours: TourDto[] = [
    { id: '1', name: 'Stari grad', difficulty: 'Easy' },
    { id: '2', name: 'Fruška gora', difficulty: 'Hard' },
  ];

  protected readonly nameFilter = signal('');

  protected readonly visibleTours = computed(() => {
    const name = this.nameFilter().toLowerCase();
    return this.tours.filter((tour) => tour.name.toLowerCase().includes(name));
  });
}
```

```html
<input #nameInput type="search" [value]="nameFilter()" (input)="nameFilter.set(nameInput.value)" />

<ul>
  @for (tour of visibleTours(); track tour.id) {
    <li>{{ tour.name }}</li>
  } @empty {
    <li>Nijedna tura ne odgovara pretrazi.</li>
  }
</ul>
```

Kada korisnik unese slovo u polje za pretragu, dešava se sledeće:
1. Događaj `input` upisuje sadržaj polja u signal `nameFilter`.
2. Izvedeni signal `visibleTours` je pretplatnik signala `nameFilter`, pa se označava za ponovno računanje.
3. Petlja `@for` čita `visibleTours`, pa radni okvir ponovo iscrtava spisak. Ture čiji je identifikator i dalje u nizu zadržava, a ostale uklanja.
4. Kada nijedna tura ne odgovara, niz je prazan i prikazuje se blok `@empty`.
