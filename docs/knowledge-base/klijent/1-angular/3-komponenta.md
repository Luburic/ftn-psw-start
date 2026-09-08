Komponenta je klasa sa pridruženim HTML šablonom koja upravlja jednim delom stranice. U React-u je istu ulogu imala funkcija koja vraća JSX. Sledeći kod prikazuje jednu karticu u oba oblika:

```jsx
function TourCard() {
  const name = 'Stari grad';
  return <h3>{name}</h3>;
}
```

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
})
export class TourCard {
  protected readonly name = 'Stari grad';
}
```

```html
<h3>{{ name }}</h3>
```

Razlika je u podeli. React drži podatke i prikaz u jednoj funkciji, a Angular ih razdvaja na klasu, koja drži podatke i logiku, i šablon, koji drži prikaz. Ovde upoznajemo kako šablon čita podatke iz klase i kako klasi javlja da je korisnik nešto uradio.

## Tri datoteke komponente

Komponentu čine tri datoteke istog naziva u istom direktorijumu:
1. `tour-card.ts` sadrži klasu sa dekoratorom `@Component`.
2. `tour-card.html` sadrži šablon, na koji dekorator upućuje podešavanjem `templateUrl`.
3. `tour-card.scss` sadrži stilove, na koje dekorator upućuje podešavanjem `styleUrl`.

Podešavanje `selector` određuje naziv HTML elementa pod kojim se komponenta koristi u šablonu druge komponente. Naziv obavezno sadrži crticu, jer pregledač tako razlikuje elemente aplikacije od standardnih HTML elemenata. U projektu svaki naziv počinje sa `app-`.

## Interpolacija

**Interpolacija** (engl. *interpolation*) je zapis `{{ izraz }}` u šablonu, koji na tom mestu ispisuje vrednost izraza kao tekst. Izraz najčešće čita polje ili poziva metodu klase. Sledeći kod prikazuje klasu i šablon kartice koja ispisuje naziv i opis:

```ts
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
}
```

```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
</article>
```

U datom kodu treba uočiti sledeće:
- Šablon vidi članove klase po nazivu, bez `this`. Polje `name` u klasi i `name` u šablonu su isto polje.
- Prevodilac šablon prevodi kao deo klase, pa šablon vidi članove označene sa `protected`, a ne vidi one označene sa `private`. Zato su polja koja šablon čita `protected`. Prevodilac proverava šablon isto kao klasu i prijavljuje grešku ako šablon pristupa privatnom članu ili članu koji ne postoji.

## Vezivanje svojstva

Interpolacija ispisuje tekst između oznaka elementa. Kada vrednost iz klase treba da odredi svojstvo elementa, poput toga da li je dugme onemogućeno, koristimo vezivanje svojstva. **Vezivanje svojstva** (engl. *property binding*) je zapis `[svojstvo]="izraz"` na elementu, koji svojstvu elementa dodeljuje vrednost izraza. Sledeći kod prikazuje dugme čija dostupnost zavisi od polja klase:

```ts
export class TourCard {
  protected readonly published = true;
}
```

```html
<button type="button" [disabled]="published">Objavi</button>
```

U datom kodu treba uočiti sledeće:
- Uglaste zagrade oko `disabled` znače da se desna strana tumači kao izraz, a ne kao tekst. Bez zagrada bi `disabled="published"` bio običan HTML atribut sa tekstom `published`.
- Kada je `published` tačno, dugme je onemogućeno. Isti zapis radi za svako svojstvo elementa, na primer `[value]` za sadržaj polja za unos.

## Vezivanje događaja

**Vezivanje događaja** (engl. *event binding*) je zapis `(događaj)="izraz"` na elementu, koji izvršava izraz kada se na elementu desi navedeni događaj. Izraz najčešće poziva metodu klase. Sledeći kod prikazuje dugme koje poziva metodu:

```ts
export class TourCard {
  protected publish(event: Event): void {
    console.log(event.target);
  }
}
```

```html
<button type="button" (click)="publish($event)">Objavi</button>
```

U datom kodu treba uočiti sledeće:
- Oble zagrade oko `click` znače da je desna strana izraz koji se izvršava pri kliku. Naziv događaja je isti kao u čistom JavaScript-u, bez prefiksa `on`.
- Reč `$event` je objekat događaja koji pregledač pravi, isti onaj koji `addEventListener` prosleđuje slušaocu. Prosleđujemo ga metodi kada joj treba. Kada metodi ne treba, poziv je `(click)="publish()"`.

## Kartica ture

Povežimo pojmove u jednu komponentu. Kartica prikazuje naziv i opis ture i ima dugme koje je onemogućeno kada je tura već objavljena:

```ts
@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
  protected published = false;

  protected publish(): void {
    this.published = true;
  }
}
```

```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
  <button type="button" [disabled]="published" (click)="publish()">Objavi</button>
</article>
```

U datom kodu treba uočiti sledeće:
- Interpolacija ispisuje dva polja, vezivanje svojstva veže dostupnost dugmeta za treće polje, a vezivanje događaja poziva metodu koja to polje menja.
- Polje `published` nije `readonly`, jer ga metoda menja.
- Naziv i opis ture su upisani u klasu, pa svaka kartica prikazuje istu turu.

Kada pokrenemo aplikaciju i kliknemo na dugme, metoda `publish` se izvršava i polje `published` dobija vrednost `true`. Dugme ostaje dostupno. Prikaz ne prati promenu običnog polja klase. Zašto se to dešava i kako se piše polje čiju promenu prikaz prati obrađuje [lekcija o signalima](4-signali.md).
