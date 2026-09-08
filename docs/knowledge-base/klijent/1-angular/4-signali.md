Prethodna lekcija se završila komponentom čiji prikaz ne prati promenu polja klase:

```ts
protected published = false;
protected publish(): void { this.published = true; }
```

Klik izvršava metodu, polje dobija novu vrednost, a dugme vezano za to polje ostaje dostupno. Ovde objašnjavamo zašto se to dešava i kako se piše polje čiju promenu prikaz prati.

## Detekcija promena

Nakon svake promene podataka radni okvir mora da odluči koje delove stranice ponovo iscrtava. Taj postupak zovemo **detekcija promena** (engl. *change detection*). Angular je istorijski to rešavao tako što je nakon svakog događaja ponovo proveravao svaki izraz u svakom šablonu. Taj pristup radi bez ikakve oznake na poljima, ali loše podnosi rast aplikacije, jer klik na jedno dugme proverava celu stranicu.

Naš projekat koristi novije, podrazumevano ponašanje. Radni okvir ponovo iscrtava samo šablone čiji su signali promenili vrednost. **Signal** (engl. *signal*) je omotač oko vrednosti koji beleži ko ga čita i obaveštava te pretplatnike kada se vrednost promeni. Šablon koji pročita signal postaje njegov pretplatnik. Kada se signal promeni, radni okvir zna tačno koji šablon treba osvežiti. Obično polje klase ne beleži ništa, pa njegova promena nikome ne stiže. Zato je dugme ostalo dostupno.

Dva podešavanja projekta čine ovo pravilo strogim. Aplikacija radi bez biblioteke `zone.js`, koja je ranije presretala svaki događaj u pregledaču i pokretala proveru cele stranice. Svaka komponenta koristi strategiju `OnPush`, koja proveru komponente pokreće samo kada se promeni signal koji njen šablon čita. Oba podešavanja upisuje `ng new` i ne menjamo ih. Iz njih sledi jedno pravilo. Svako stanje koje šablon prikazuje je signal.

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
- Poziv `signal(false)` pravi signal sa početnom vrednošću. Tip vrednosti prevodilac zaključuje iz početne vrednosti, pa `signal(0)` čuva broj. Kada početna vrednost ne određuje tip, tip navodimo, kao kod `signal<string | null>(null)`.
- Signal se čita pozivom, `published()` u šablonu i `this.published()` u klasi. Poziv vraća trenutnu vrednost i beleži pretplatnika.
- Metoda `set` zamenjuje vrednost novom. Metoda `update` prima funkciju koja od stare vrednosti pravi novu. Obe obaveštavaju pretplatnike.
- Polje je `readonly`, jer se sam signal ne menja. Menja se vrednost u njemu.

Čitalac koji poznaje React prepoznaje ulogu `useState`. Razlika je u tome što signal živi kao polje klase, a ne kao poziv unutar funkcije, i što se čita pozivom, a ne kao obična promenljiva.

## Izvedeni signal

Prikazu je često potrebna vrednost koja se računa iz drugih signala, na primer tekst dugmeta koji zavisi od broja sviđanja. **Izvedeni signal** (engl. *computed signal*) je signal čija se vrednost računa funkcijom iz drugih signala i koji se sam ponovo računa kada se neki od njih promeni. Sledeći kod prikazuje izvedeni signal za tekst dugmeta:

```ts
protected readonly likes = signal(0);
protected readonly likesLabel = computed(() => `Sviđa mi se (${this.likes()})`);
```

```html
<button type="button" (click)="like()">{{ likesLabel() }}</button>
```

U datom kodu treba uočiti sledeće:
- Poziv `computed` prima funkciju koja čita druge signale i vraća vrednost. Radni okvir beleži koje signale je funkcija pročitala.
- Izvedeni signal se čita pozivom kao i običan, ali nema `set` ni `update`. Njegova vrednost se menja samo kada se promeni signal od kog zavisi.
- Vrednost se računa tek pri prvom čitanju i zatim pamti. Dok se `likes` ne promeni, svako čitanje `likesLabel` vraća zapamćenu vrednost bez ponovnog računanja.

Ulogu izvedenog signala je u React-u imao `useMemo`, sa ručno navedenom listom zavisnosti. Ovde listu ne navodimo, jer je radni okvir sam beleži pri čitanju.

## Reaktivni kontekst

Beleženje pretplatnika se dešava samo unutar reaktivnog konteksta. **Reaktivni kontekst** (engl. *reactive context*) je mesto na kom radni okvir prati koji se signali čitaju. Takva mesta su šablon i funkcija koju prima `computed`. Čitanje signala u običnoj metodi klase, poput `this.likes()` u metodi `like`, vraća vrednost i ne beleži ništa. To je očekivano. Metoda se izvršava jednom, kada je pozovemo, a ne ponovo kada se signal promeni. Ponovno izvršavanje pri promeni dobija samo kod u reaktivnom kontekstu.

## Nizovi i objekti u signalu

Signal obaveštava pretplatnike kada metoda `set` ili `update` dodeli novu vrednost. Kada signal čuva niz ili objekat, izmena unutar te vrednosti ne prolazi kroz signal i pretplatnici ne dobijaju obaveštenje. Sledeći kod prikazuje pogrešan i ispravan način dodavanja oznake:

```ts
protected readonly tags = signal<string[]>([]);

protected addTagWrong(tag: string): void {
  this.tags().push(tag);
}

protected addTag(tag: string): void {
  this.tags.update((tags) => [...tags, tag]);
}
```

U datom kodu treba uočiti sledeće:
- Metoda `addTagWrong` menja niz koji signal već drži. Signal ne zna za tu izmenu, jer `set` ni `update` nisu pozvani, pa spisak na ekranu ostaje star.
- Metoda `addTag` pravi nov niz sa dodatom oznakom i predaje ga signalu. Signal vidi novu vrednost i obaveštava pretplatnike.

U našem projektu svaki spisak stiže sa servera i ponovo se učitava nakon svake izmene, pa ovaj slučaj retko srećemo. Pravilo ipak treba znati, jer je izmena postojećeg niza najčešći uzrok prikaza koji se ne osvežava.

## Efekat

Angular nudi i **efekat** (engl. *effect*), funkciju koja se izvršava pri svakoj promeni signala koje čita. Čitalac koji poznaje React prepoznaje ulogu `useEffect`, u kom je navikao da učitava podatke sa servera. U našem projektu efekat ne koristimo. Podatke sa servera čita resurs, koji obrađuje [lekcija o čitanju podataka](9-citanje-podataka.md), a vrednost koja zavisi od drugih signala je izvedeni signal. Kada se u kodu komponente pojavi efekat, to je znak da se traži rešenje koje jedan od ta dva mehanizma već nudi.

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
  protected readonly likesLabel = computed(() => `Sviđa mi se (${this.likes()})`);

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
  <button type="button" (click)="like()">{{ likesLabel() }}</button>
</article>
```

Kada korisnik klikne na drugo dugme, dešava se sledeće:
1. Vezivanje događaja poziva metodu `like`.
2. Metoda poziva `update` na signalu `likes`, koji dobija novu vrednost i obaveštava pretplatnike.
3. Izvedeni signal `likesLabel` je pretplatnik signala `likes`, pa se označava za ponovno računanje.
4. Šablon je pretplatnik signala `likesLabel`, pa radni okvir ponovo iscrtava komponentu. Pri iscrtavanju šablon čita `likesLabel`, koji tek tada računa novu vrednost.

Polje `name` je ostalo običan tekst, jer se nikada ne menja. Signal je potreban samo za stanje koje se menja dok komponenta živi.
