Savremene klijentske aplikacije nisu izgrađene u okviru jedne komponente, već komponuju više komponenti da formiraju prikaz na stranici. Ovakva modularnost omogućava lakše održavanje komponenti, ponovnu iskoristivost i bolje performanse kada aplikacija učita samo one komponente koje su potrebne za dati prikaz.

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

Ovakva kartica u svakom prolazu petlje prikazuje isti naziv, onaj upisan u klasu. Da bi svaka kartica prikazala svoju turu, roditelj mora da joj prosledi podatak.

## Ulaz

**Ulaz** (engl. *input*) je polje deteta koje roditelj popunjava iz svog šablona. Sledeći kod prikazuje karticu koja turu prima kao ulaz:

```ts
import { Component, input } from '@angular/core';
import { TourDto } from '../tour-dto';

@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
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

Dok je HTML roditeljske komponente:
```html
@for (tour of visibleTours(); track tour.id) {
  <app-tour-card [tour]="tour" />
}
```

U datom kodu treba uočiti sledeće:
- Poziv `input.required<TourDto>()` deklariše obavezan ulaz tipa `TourDto`. Prevodilac prijavljuje grešku ako roditelj u šablonu ne veže vrednost za ovaj ulaz. Ulaz sa podrazumevanom vrednošću se piše npr. `input<boolean>(false)` i roditelj ga sme izostaviti.
- Roditelj popunjava ulaz vezivanjem svojstva, `[tour]="tour"`. Leva strana je naziv ulaza u detetu, a desna izraz u šablonu roditelja (u primeru promenljiva petlje).
- Ulaz je signal i čita se pozivom, `tour()`. Kada roditelj prosledi novu vrednost, Angular ponovo proverava šablon kartice, po istom pravilu kao za svaki drugi signal.
- Ulaz je deo javnog „ugovora“ komponente, jer ga koristi roditelj. Zato ga po konvenciji pišemo bez modifikatora pristupa, tj. kao javan, uz `readonly`. Modifikator `protected` ostaje za članove koje koristi samo šablon same komponente.

Ulaz je signal samo za čitanje i nema metode `set` i `update`. Vrednost ulaza određuje isključivo roditelj. Dete ne sme ni da menja objekat koji je dobilo, npr. `this.tour().name = 'Novi naziv'`: to je izmena postojećeg objekta, koju signali ne primećuju, a menja i podatak koji pripada roditelju. Kada dete želi da se podatak promeni, javlja to roditelju, kao što pokazuje naredni odeljak.

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

Dok je HTML roditeljske komponente:
```html
@for (tour of visibleTours(); track tour.id) {
  <app-tour-card [tour]="tour" (deleteTour)="remove($event)" />
}
```

Roditelj u svojoj klasi ima metodu `remove`, koja prima poslatu vrednost i uklanja turu iz spiska:
```ts
export class TourList {
  private readonly tours = signal<TourDto[]>([ ... ]);

  protected remove(tourId: string): void {
    this.tours.update((tours) => tours.filter((tour) => tour.id !== tourId));
  }
}
```

U datom kodu treba uočiti sledeće:
- Poziv `output<string>()` deklariše izlaz koji nosi tekst. Parametrizovan tip je tip vrednosti koju izlaz šalje roditelju.
- Poziv `emit` okida izlaz sa vrednošću. Ovde se poziva iz vezivanja događaja na dugmetu, sa identifikatorom ture. Kartica ne zna ko je sluša niti šta se posle klika dešava. Njen posao se završava pozivom `emit`.
- Roditelj sluša izlaz vezivanjem događaja, `(deleteTour)="remove($event)"`. Naziv u zagradi je naziv izlaza u detetu, a `$event` je vrednost koju je dete poslalo kroz `emit`. Metoda `remove` pripada roditelju.
- Kao i ulaz, izlaz je deo javnog ugovora komponente, pa ga pišemo kao javan, uz `readonly` ključnu reč.

## Podaci naniže, događaji naviše

Prethodna dva odeljka daju pravilo po kom se komponente sastavljaju. Podaci putuju naniže, od roditelja ka detetu, kroz ulaze. Događaji putuju naviše, od deteta ka roditelju, kroz izlaze. Dete drži samo stanje potrebno za sopstveni prikaz i ne zna na kojoj se stranici nalazi. Zato istu karticu mogu da koriste spisak svih tura i spisak tura jednog autora, a svaki roditelj sam odlučuje šta radi kada kartica javi događaj.

## Stranica i kartica

Povežimo pojmove u stranicu koja filtrira ture, prikazuje ih kroz kartice i uklanja turu koju kartica javi. Kartica ima ulaz i izlaz:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  readonly tour = input.required<TourDto>();
  readonly deleteTour = output<string>();
}
```

```html
<article>
  <h3>{{ tour().name }}</h3>
  <p>{{ tour().description }}</p>
  <button type="button" (click)="deleteTour.emit(tour().id)">Obriši</button>
</article>
```

Stranica zadržava filtriranje po težini iz prethodne lekcije i dodaje uklanjanje:

```ts
@Component({
  selector: 'app-tour-list',
  imports: [TourCard],
  templateUrl: './tour-list.html',
  styleUrl: './tour-list.scss',
})
export class TourList {
  private readonly tours = signal<TourDto[]>([
    { id: '1', name: 'Stari grad', description: 'Šetnja kroz tvrđavu.', difficulty: 'Easy', tags: ['istorija'] },
    { id: '2', name: 'Fruška gora', description: 'Planinarenje do manastira.', difficulty: 'Hard', tags: ['priroda'] },
  ]);

  protected readonly difficultyFilter = signal<TourDifficulty | null>(null);

  protected readonly visibleTours = computed(() => {
    // Izvedeni signal čita i `tours` i `difficultyFilter`,
    // pa se preračunava kada se promeni bilo koji od njih
    const difficulty = this.difficultyFilter();
    return this.tours().filter((tour) => difficulty === null || tour.difficulty === difficulty);
  });

  protected remove(tourId: string): void {
    this.tours.update((tours) => tours.filter((tour) => tour.id !== tourId));
  }
}
```

```html
<button type="button" (click)="difficultyFilter.set(null)">Sve</button>
<button type="button" (click)="difficultyFilter.set('Easy')">Lake</button>
<button type="button" (click)="difficultyFilter.set('Moderate')">Srednje</button>
<button type="button" (click)="difficultyFilter.set('Hard')">Teške</button>

@for (tour of visibleTours(); track tour.id) {
  <app-tour-card [tour]="tour" (deleteTour)="remove($event)" />
} @empty {
  <p>Nijedna tura nema izabranu težinu.</p>
}
```

Kada korisnik klikne na dugme za brisanje na kartici ture „Fruška gora“, dešava se sledeće:
1. Vezivanje događaja na dugmetu poziva `deleteTour.emit('2')`.
2. Roditelj je na taj izlaz vezao izraz `remove($event)`, pa se poziva `remove('2')`.
3. Metoda `remove` predaje signalu `tours` nov niz bez te ture.
4. Izvedeni signal `visibleTours` je čitalac signala `tours`, pa se označava za ponovno računanje.
5. Petlja `@for` čita `visibleTours`, pa Angular ponovo proverava šablon stranice. Na osnovu `track tour.id` kartica sa identifikatorom `1` ostaje netaknuta, a kartica sa identifikatorom `2` se uklanja.
