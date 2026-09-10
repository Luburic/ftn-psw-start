Stranica sa spiskom tura iz prethodne lekcije ispisuje naziv svake ture u jednom elementu liste. Kada kartica ture dobije opis, težinu, oznake i vremena obilaska, blok petlje naraste na dvadeset redova, a šablon stranice postane teško čitljiv. Isti izgled kartice treba i stranici sa turama koje je korisnik sam napravio. U React-u je čitalac takav deo izdvajao u zasebnu komponentu i podatke joj prosleđivao kroz svojstva (engl. *props*). U Angular-u je postupak isti, uz jedan dodatak za smer od kartice ka stranici.

## Korišćenje komponente u komponenti

Komponenta koristi drugu komponentu tako što njenu klasu navede u podešavanju `imports` i njen selektor napiše u svom šablonu. Komponentu koja koristi drugu zovemo roditeljem, a komponentu koja se koristi detetom. Sledeći kod prikazuje stranicu koja koristi karticu:

```ts
@Component({
  selector: 'app-tour-list',
  imports: [TourCard],
  templateUrl: './tour-list.html',
  styleUrl: './tour-list.scss',
})
export class TourList { ... }
```

```html
@for (tour of visibleTours(); track tour.id) {
  <app-tour-card />
}
```

U datom kodu treba uočiti sledeće:
- Bez klase `TourCard` u podešavanju `imports` prevodilac prijavljuje da element `app-tour-card` nije poznat. Podešavanje `imports` je jedino mesto na kom roditelj pominje klasu deteta.
- Element se piše kao samozatvarajući, jer kartica nema sadržaj između oznaka. Sve što prikazuje dolazi iz njenog šablona.

Ovakva kartica u svakom prolazu petlje prikazuje isti naziv, onaj upisan u klasu. Da bi svaka kartica prikazala svoju turu, roditelj mora da joj prosledi podatak.

## Ulaz

**Ulaz** (engl. *input*) je polje deteta koje roditelj popunjava iz svog šablona. Sledeći kod prikazuje karticu koja turu prima kao ulaz:

```ts
export class TourCard {
  readonly tour = input.required<TourDto>();
}
```

```html
<article>
  <h3>{{ tour().name }}</h3>
  <p>{{ tour().description }}</p>
</article>
```

```html
@for (tour of visibleTours(); track tour.id) {
  <app-tour-card [tour]="tour" />
}
```

U datom kodu treba uočiti sledeće:
- Poziv `input.required<TourDto>()` deklariše obavezan ulaz tipa `TourDto`. Prevodilac prijavljuje grešku ako roditelj u šablonu ne veže vrednost za ovaj ulaz. Ulaz sa podrazumevanom vrednošću se piše `input<string | null>(null)` i roditelj ga sme izostaviti.
- Ulaz je signal i čita se pozivom, `tour()`. Kada roditelj prosledi novu vrednost, kartica se ponovo iscrtava po istom pravilu kao za svaki drugi signal.
- Roditelj popunjava ulaz vezivanjem svojstva, `[tour]="tour"`. Leva strana je naziv ulaza u detetu, a desna izraz u šablonu roditelja, ovde promenljiva petlje.
- Ulaz nije `protected`, jer ga roditelj mora videti. Nije ni `private`, iz istog razloga. Ostaje bez modifikatora pristupa, uz `readonly`.

## Izlaz

Kada korisnik uradi nešto na kartici, na primer klikne na dugme za brisanje, posao brisanja pripada stranici, jer ona zna spisak i zna kako se obraća serveru. Kartica zato roditelju samo javlja da se nešto desilo. **Izlaz** (engl. *output*) je događaj deteta na koji roditelj vezuje izraz u svom šablonu, isto kao na događaj HTML elementa. Sledeći kod prikazuje karticu sa dugmetom za brisanje:

```ts
export class TourCard {
  readonly tour = input.required<TourDto>();
  readonly deleteTour = output<string>();
}
```

```html
<article>
  <h3>{{ tour().name }}</h3>
  <button type="button" (click)="deleteTour.emit(tour().id)">Obriši</button>
</article>
```

```html
@for (tour of visibleTours(); track tour.id) {
  <app-tour-card [tour]="tour" (deleteTour)="remove($event)" />
}
```

Roditelj u svojoj klasi ima metodu `remove`, koja prima poslatu vrednost i uklanja turu iz spiska:

```ts
export class TourList {
  protected remove(tourId: string): void {
    this.tours.update((tours) => tours.filter((tour) => tour.id !== tourId));
  }
}
```

U datom kodu treba uočiti sledeće:
- Poziv `output<string>()` deklariše izlaz koji nosi tekst. Tip u zagradi je tip vrednosti koju izlaz šalje roditelju.
- Poziv `emit` okida izlaz sa vrednošću. Ovde se poziva iz vezivanja događaja na dugmetu, sa identifikatorom ture.
- Roditelj sluša izlaz vezivanjem događaja, `(deleteTour)="remove($event)"`. Naziv u zagradi je naziv izlaza u detetu, a `$event` je vrednost koju je dete poslalo kroz `emit`. Metoda `remove` pripada roditelju.
- Kartica ne zna ko je sluša niti šta se posle klika dešava. Njen posao se završava pozivom `emit`.

## Podaci naniže, događaji naviše

Prethodna dva odeljka daju pravilo po kom se komponente sastavljaju. Podaci putuju naniže, od roditelja ka detetu, kroz ulaze. Događaji putuju naviše, od deteta ka roditelju, kroz izlaze. Dete drži samo stanje potrebno za sopstveni prikaz i ne zna na kojoj se stranici nalazi. Zato istu karticu mogu da koriste spisak svih tura i spisak tura jednog autora, a svaki roditelj sam odlučuje šta radi kada kartica javi događaj.

## Stranica i kartica

Povežimo pojmove u stranicu koja prikazuje kartice i uklanja turu koju kartica javi:

```ts
@Component({
  selector: 'app-tour-list',
  imports: [TourCard],
  templateUrl: './tour-list.html',
  styleUrl: './tour-list.scss',
})
export class TourList {
  protected readonly tours = signal<TourDto[]>([
    { id: '1', name: 'Stari grad', description: 'Šetnja kroz tvrđavu.', difficulty: 'Easy' },
    { id: '2', name: 'Fruška gora', description: 'Planinarenje do manastira.', difficulty: 'Hard' },
  ]);

  protected remove(tourId: string): void {
    this.tours.update((tours) => tours.filter((tour) => tour.id !== tourId));
  }
}
```

```html
@for (tour of tours(); track tour.id) {
  <app-tour-card [tour]="tour" (deleteTour)="remove($event)" />
} @empty {
  <p>Nema tura.</p>
}
```

Kada korisnik klikne na dugme za brisanje na drugoj kartici, dešava se sledeće:
1. Vezivanje događaja na dugmetu poziva `deleteTour.emit('2')`.
2. Roditelj je na taj izlaz vezao izraz `remove($event)`, pa se poziva `remove('2')`.
3. Metoda `remove` predaje signalu `tours` nov niz bez te ture.
4. Petlja `@for` je pretplatnik signala `tours`, pa radni okvir ponovo iscrtava spisak. Kartica sa identifikatorom `1` ostaje, a kartica sa identifikatorom `2` se uklanja.
