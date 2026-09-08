Prethodna lekcija se završila komponentom čiji prikaz ne prati promenu polja klase:

```ts
protected published = false;
protected publish(): void { this.published = true; }
```

Klik izvršava metodu, polje dobija novu vrednost, a dugme vezano za to polje ostaje dostupno. Ovde objašnjavamo zašto se to dešava i kako se piše polje čiju promenu prikaz prati.

## Detekcija promena

Nakon svake promene podataka radni okvir mora da odluči koje delove stranice ponovo iscrtava. Taj postupak zovemo **detekcija promena** (engl. *change detection*). Angular je istorijski to rešavao tako što je nakon svakog događaja ponovo proveravao svaki izraz u svakom šablonu. Taj pristup radi bez ikakve oznake na poljima, ali loše podnosi rast aplikacije, jer klik na jedno dugme proverava celu stranicu.

Naš projekat koristi novije, podrazumevano ponašanje. Radni okvir ponovo iscrtava samo šablone čiji su signali promenili vrednost. **Signal** (engl. *signal*) je omotač oko vrednosti koji beleži ko ga čita i obaveštava te pretplatnike kada se vrednost promeni. Šablon koji pročita signal postaje njegov pretplatnik. Kada se signal promeni, radni okvir zna tačno koji šablon treba osvežiti. Obično polje klase ne beleži ništa, pa njegova promena nikome ne stiže. Zato je dugme ostalo dostupno.

Iz toga sledi jedno pravilo. Svako stanje koje šablon prikazuje je signal.

## Signal

Sledeći kod prikazuje popravljenu karticu:

```ts
export class TourCard {
  protected readonly published = signal(false);
  protected readonly likes = signal(0);

  protected publish(): void {
    this.published.set(true);
  }

  protected like(): void {
    this.likes.update((count) => count + 1);
  }
}
```

```html
<button type="button" [disabled]="published()" (click)="publish()">Objavi</button>
<button type="button" (click)="like()">Sviđa mi se ({{ likes() }})</button>
```

U datom kodu treba uočiti sledeće:
- Poziv `signal(false)` pravi signal sa početnom vrednošću. Tip vrednosti prevodilac zaključuje iz početne vrednosti, pa `signal(0)` čuva broj.
- Signal se čita pozivom, `published()` u šablonu i `this.published()` u klasi. Poziv vraća trenutnu vrednost i beleži pretplatnika.
- Metoda `set` zamenjuje vrednost novom. Metoda `update` prima funkciju koja od stare vrednosti pravi novu. Obe obaveštavaju pretplatnike.
- Polje je `readonly`, jer se sam signal ne menja. Menja se vrednost u njemu.

Kada početna vrednost ne određuje tip, tip navodimo u uglastim zagradama iza naziva funkcije, isto kao parametar generičkog tipa. Sledeći kod prikazuje signal koji čuva identifikator izabrane ture:

```ts
protected readonly selectedTourId = signal<string | null>(null);
```

Iz početne vrednosti `null` prevodilac ne bi znao da polje kasnije čuva tekst. Parametar `<string | null>` mu to saopštava, pa `set('5')` prolazi prevođenje, a `set(5)` ne.

Čitalac koji poznaje React prepoznaje ulogu `useState`. Razlika je u tome što signal živi kao polje klase, a ne kao poziv unutar funkcije, i što se čita pozivom, a ne kao obična promenljiva.

## Reaktivni kontekst

Beleženje pretplatnika se dešava samo unutar reaktivnog konteksta. **Reaktivni kontekst** (engl. *reactive context*) je mesto na kom radni okvir prati koji se signali čitaju. Šablon je takvo mesto. Čitanje signala u običnoj metodi klase, poput `this.likes()` u metodi `like`, vraća vrednost i ne beleži ništa. To je očekivano. Metoda se izvršava jednom, kada je pozovemo, a ne ponovo kada se signal promeni. Ponovno izvršavanje pri promeni dobija samo kod u reaktivnom kontekstu.

## Kartica sa signalima

Povežimo pojmove u karticu iz prethodne lekcije, sada sa stanjem koje prikaz prati:

```ts
@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly published = signal(false);
  protected readonly likes = signal(0);

  protected publish(): void {
    this.published.set(true);
  }

  protected like(): void {
    this.likes.update((count) => count + 1);
  }
}
```

```html
<article>
  <h3>{{ name }}</h3>
  <button type="button" [disabled]="published()" (click)="publish()">Objavi</button>
  <button type="button" (click)="like()">Sviđa mi se ({{ likes() }})</button>
</article>
```

Kada korisnik klikne na drugo dugme, dešava se sledeće:
1. Vezivanje događaja poziva metodu `like`.
2. Metoda poziva `update` na signalu `likes`, koji dobija novu vrednost i obaveštava pretplatnike.
3. Šablon je pretplatnik signala `likes`, jer ga čita u interpolaciji, pa radni okvir ponovo iscrtava komponentu.
4. Pri iscrtavanju šablon ponovo čita `likes` i ispisuje novu vrednost.

Polje `name` je ostalo običan tekst, jer se nikada ne menja. Signal je potreban samo za stanje koje se menja dok komponenta živi.
