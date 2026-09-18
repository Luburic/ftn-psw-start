Kartica iz prethodne lekcije ispisuje broj sviđanja unutar teksta dugmeta. Pretpostavimo da isti natpis treba i u naslovu kartice, kao i da uz broj treba pravilo o tome kako se reč menja: jedan „like“, više „likes“. Šablon tada izgleda ovako:

```html
<h3>{{ name }} ({{ likes() }} {{ likes() === 1 ? 'like' : 'likes' }})</h3>
<button type="button" (click)="like()">
  {{ likes() }} {{ likes() === 1 ? 'like' : 'likes' }}
</button>
```

Isti izraz sa uslovom ponavlja se na dva mesta. Ako pravilo promenimo, moramo da ga promenimo svuda, a lako je da neko mesto propustimo. Vrednost koja se računa iz drugih signala treba da živi na jednom mestu u klasi. Ovde upoznajemo signal čija se vrednost računa iz drugih signala, kao i pravilo o signalu koji čuva niz ili objekat.

## Izvedeni signal

**Izvedeni signal** (engl. *computed signal*) je signal čija se vrednost računa funkcijom iz drugih signala i koji se sam ponovo računa kada se neki od njih promeni. Sledeći kod prikazuje karticu sa izvedenim signalom za natpis o sviđanjima:

```ts
import { Component, computed, signal } from '@angular/core';

@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly likes = signal(0);
  protected readonly likesLabel = computed(() => {
    const count = this.likes();
    return `${count} ${count === 1 ? 'like' : 'likes'}`;
  });

  protected like(): void {
    this.likes.update((count) => count + 1);
  }
}
```

```html
<h3>{{ name }} ({{ likesLabel() }})</h3>
<button type="button" (click)="like()">{{ likesLabel() }}</button>
```

U datom kodu treba uočiti sledeće:
- Poziv `computed` prima funkciju koja čita druge signale i vraća vrednost. Signali koje funkcija pročita postaju izvori izvedenog signala. Ovde je to signal `likes`.
- Izvedeni signal se čita pozivom kao i običan, ali nema metode `set` i `update`. Njegov tip je `Signal`, a ne `WritableSignal`, pa prevodilac prijavljuje grešku ako pokušamo da ih pozovemo. Vrednost izvedenog signala menja se samo kada se promeni neki od izvora.
- Vrednost se računa tek pri prvom čitanju i zatim se pamti. Dok se `likes` ne promeni, svako čitanje `likesLabel` vraća zapamćenu vrednost bez ponovnog računanja, iako ga šablon čita na dva mesta.

Funkcija koju prosleđujemo pozivu `computed` treba samo da izračuna vrednost iz izvora. U njoj ne menjamo druge signale, ne šaljemo zahteve serveru i ne radimo ništa osim računanja. Ako funkcija pokuša da promeni signal, Angular prijavljuje grešku.

## Nizovi i objekti u signalu

Signal obaveštava čitaoce samo kada dobije vrednost različitu od prethodne. Pri tome ne poredi sadržaj, već proverava da li je u pitanju ista vrednost. Za nizove i objekte to znači isti niz, odnosno isti objekat u memoriji. Zato izmena unutar niza ili objekta ne obaveštava čitaoce, jer signal i dalje drži isti niz.

Sledeći kod prikazuje dva pogrešna i jedan ispravan način dodavanja oznake:

```ts
protected readonly tags = signal<string[]>([]);
protected readonly tagsLabel = computed(() => this.tags().join(', '));

protected addTagWrong(tag: string): void {
  this.tags().push(tag);
}

protected addTagAlsoWrong(tag: string): void {
  this.tags.update((tags) => {
    tags.push(tag);
    return tags;
  });
}

protected addTag(tag: string): void {
  this.tags.update((tags) => [...tags, tag]);
}
```

U datom kodu treba uočiti sledeće:
- Metoda `addTagWrong` menja niz koji signal već drži, bez poziva `set` ili `update`. Signal ne zna za tu izmenu, pa `tagsLabel` i dalje vraća zapamćenu, staru vrednost.
- Metoda `addTagAlsoWrong` poziva `update`, ali vraća isti niz koji je izmenila. Signal poredi staru i novu vrednost, vidi da je u pitanju isti niz i ne obaveštava čitaoce. Poziv `update` dakle nije dovoljan, već vrednost mora da bude nova.
- Metoda `addTag` pravi nov niz sa dodatom oznakom pomoću operatora `...` i predaje ga signalu. Signal vidi drugi niz i obaveštava čitaoce, pa `tagsLabel` računa novu vrednost.

Isto pravilo važi i za objekte. Umesto da menjamo polje postojećeg objekta, pravimo nov objekat sa izmenjenim poljem:

```ts
protected readonly tour = signal({ name: 'Stari grad', difficulty: 'Easy' });

protected rename(name: string): void {
  this.tour.update((tour) => ({ ...tour, name }));
}
```

U datom kodu treba uočiti sledeće:
- Zapis `{ ...tour, name }` pravi nov objekat koji sadrži sva polja objekta `tour`, s tim što polje `name` dobija novu vrednost.
- Objekat koji funkcija vraća mora biti u običnim zagradama. Bez njih bi prevodilac vitičastu zagradu protumačio kao početak tela funkcije, a ne kao objekat.

> **Važno:** Izmena postojećeg niza ili objekta je najčešći uzrok prikaza koji se ne osvežava. Dodatno zbunjuje to što ponekad deluje kao da radi. Ako se pogrešna metoda pozove klikom, sam klik pokreće proveru šablona, pa šablon koji direktno čita `tags()` može da prikaže novu oznaku. Na to ne smemo da se oslanjamo: izvedeni signali iz tog niza ostaju stari, a izmena koja ne dolazi iz događaja u šablonu, npr. iz tajmera ili odgovora servera, uopšte se ne prikazuje. Zato signalu uvek predajemo nov niz ili nov objekat.

## Kartica sa sviđanjima i oznakama

Povežimo pojmove u karticu koja prikazuje broj sviđanja i oznake ture i dozvoljava dodavanje nove oznake:

```ts
import { Component, computed, signal } from '@angular/core';

@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';

  protected readonly likes = signal(0);
  protected readonly likesLabel = computed(() => {
    const count = this.likes();
    return `${count} ${count === 1 ? 'like' : 'likes'}`;
  });

  protected readonly tags = signal<string[]>([]);
  protected readonly tagsLabel = computed(() => this.tags().join(', '));

  protected like(): void {
    this.likes.update((count) => count + 1);
  }

  protected addTag(tag: string): void {
    this.tags.update((tags) => [...tags, tag]);
  }
}
```

```html
<article>
  <h3>{{ name }} ({{ likesLabel() }})</h3>
  <p>{{ tagsLabel() }}</p>
  <button type="button" (click)="like()">{{ likesLabel() }}</button>
  <button type="button" (click)="addTag('istorija')">Dodaj oznaku</button>
</article>
```

Kada korisnik klikne na dugme „Dodaj oznaku“, dešava se sledeće:
1. Vezivanje događaja poziva metodu `addTag`.
2. Metoda predaje signalu `tags` nov niz, pa signal obaveštava svoje čitaoce.
3. Izvedeni signal `tagsLabel` je čitalac signala `tags`, pa se označava za ponovno računanje. Novu vrednost još ne računa.
4. Šablon je čitalac signala `tagsLabel`, pa Angular ponovo proverava šablon komponente. Pri proveri šablon čita `tagsLabel`, koji tek tada računa novu vrednost.
5. Angular menja samo tekst pasusa sa oznakama. Naslov i dugme za sviđanja ostaju netaknuti, jer se `likes` nije promenio, pa `likesLabel` vraća zapamćenu vrednost.

Signal drži izvor stanja, a izvedeni signal drži vrednost izračunatu iz njega. Kratak izraz, poput jednog poređenja, može da ostane u šablonu. Kada je izvedena vrednost složenija, koristi se na više mesta ili je potrebna i u klasi, poput natpisa, filtriranog spiska ili zbira, pišemo je kao izvedeni signal. Tako stanje ima jedan izvor istine, a vrednosti koje iz njega slede same se održavaju u skladu sa njegovim promenama.
