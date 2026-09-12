Prethodna lekcija se završila komponentom kojoj se prikaz nije osvežio kada se polje promeni samo od sebe:

```ts
protected published = false;

constructor() {
  setTimeout(() => {
    this.published = true;
  }, 1000);
}
```

Posle jedne sekunde polje `published` dobija vrednost `true`, ali dugme vezano za to polje (`[disabled]="published"`) ostaje omogućeno. Videli smo i suprotan slučaj: kada isto polje promeni klik na dugme, prikaz se osveži. Ovde objašnjavamo odakle ta razlika i kako se piše polje čiju promenu prikaz uvek prati.

## Detekcija promena

Nakon promene podataka radni okvir mora da odluči da li i koje delove stranice ponovo iscrtava. Taj postupak zovemo **detekcija promena** (engl. *change detection*). Postavlja se pitanje kako radni okvir uopšte sazna da se vrednost promenila?

Događaj iz šablona je jedan takav znak. Kada korisnik klikne na dugme sa `(click)`, radni okvir sam poziva našu metodu, pa zna da odmah zatim treba da proveri prikaz. Zato se dugme iz prethodne lekcije onemogućilo kada je klik postavio polje, promenu je pratio događaj o kome radni okvir zna.

Promena iz tajmera nema takav znak. Kada `setTimeout` postavi isto polje, radni okvir za to ne sazna, pa prikaz ostaje zaostao. Tu se vidi razlika u pristupu detekciji promena:

- Angular je istorijski uz sebe držao biblioteku (Zone.js) koja presreće događaje, tajmere i mrežne pozive, i posle svakog od njih ponovo proverava svaki izraz u svakom šablonu. Uz taj pristup i promena iz tajmera bi osvežila prikaz, ali cena je provera cele stranice posle svake sitnice, što loše podnosi rast aplikacije.
- Naš projekat koristi novije ponašanje, bez te biblioteke. Radni okvir tada sazna za promenu samo iz dva izvora: događaja iz šablona i signala. **Signal** (engl. *signal*) je omotač oko vrednosti koji beleži ko ga čita i obaveštava te čitaoce kada se vrednost promeni. Šablon koji pročita signal postaje njegov čitalac, pa radni okvir zna tačno koju komponentu da osveži kada se signal promeni, bez obzira odakle promena stiže, pa i iz tajmera.

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
- Signal se čita pozivom, `published()` u šablonu i `this.published()` u klasi. Poziv vraća trenutnu vrednost i beleži čitaoca.
- Metoda `set` zamenjuje vrednost novom. Metoda `update` prima funkciju koja od stare vrednosti pravi novu. Obe obaveštavaju čitaoce.
- Polje je `readonly`, jer se sam signal ne menja. Menja se vrednost u njemu.

Kada početna vrednost ne određuje tip, tip navodimo u uglastim zagradama iza naziva funkcije, isto kao parametar generičkog tipa. Sledeći kod prikazuje signal koji čuva identifikator izabrane ture:

```ts
protected readonly selectedTourId = signal<string | null>(null);
```

Iz početne vrednosti `null` prevodilac ne bi znao da polje kasnije čuva tekst. Parametar `<string | null>` mu to saopštava, pa `set('5')` prolazi prevođenje, a `set(5)` ne.

## Kartica sa signalima

Povežimo pojmove u karticu iz prethodne lekcije, sada sa stanjem koje prikaz prati:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
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
2. Metoda poziva `update` na signalu `likes`, koji dobija novu vrednost i obaveštava čitaoce.
3. Šablon je čitalac signala `likes`, jer ga čita u interpolaciji, pa radni okvir ponovo iscrtava komponentu.
4. Pri iscrtavanju šablon ponovo čita `likes` i ispisuje novu vrednost.

Za razliku od običnog polja, signal obaveštava prikaz o promeni bez obzira odakle ona stiže. Da je `published` sa početka lekcije bio signal, i promena iz tajmera bi osvežila dugme.

Polje `name` je ostalo običan tekst, jer se nikada ne menja. Signal je potreban samo za stanje koje se menja dok komponenta živi.

## Vrednost izvedena iz signala

Signali drže stanje koje se menja. Prikaz, međutim, često traži vrednost koja se *izračunava* iz tog stanja, na primer oznaku „popularno" koja se pojavljuje tek kada broj sviđanja pređe deset:

```html
<button type="button" (click)="like()">Sviđa mi se ({{ likes() }})</button>
<span>{{ likes() >= 10 ? 'popularno' : '' }}</span>
```

Za jedan ovako kratak uslov ovo je u redu. Ali kada izvedena vrednost postane složenija, koristi se na više mesta ili spaja više signala, ne želimo da logiku ponavljamo po šablonu niti da je pišemo u običnoj metodi, metoda se izvršava iznova pri svakom iscrtavanju i nije ni sama signal. Treba nam vrednost koja čita druge signale, sama se preračunava kada se oni promene i pamti rezultat dok se ništa ne promeni. Takva vrednost zove se **izvedeni signal** (engl. *computed signal*) i njome se bavi [naredna lekcija o izvedenim signalima](izvedeni-signali.md).
