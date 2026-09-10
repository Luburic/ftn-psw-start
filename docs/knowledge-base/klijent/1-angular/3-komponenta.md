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

Razlika je u podeli. React drži podatke i prikaz u jednoj funkciji, a Angular ih razdvaja na klasu, koja drži podatke i logiku, i šablon, koji drži prikaz. Ovde upoznajemo kako šablon čita podatke iz klase i kako klasi javlja da je korisnik nešto uradio, a na kraju i jedno ograničenje na koje ćemo naići kada se podatak promeni sam od sebe.

Komponentu čine tri datoteke istog naziva u istom direktorijumu:
1. `tour-card.ts` sadrži klasu sa dekoratorom `@Component`.
2. `tour-card.html` sadrži šablon, na koji dekorator upućuje podešavanjem `templateUrl`.
3. `tour-card.scss` sadrži stilove, na koje dekorator upućuje podešavanjem `styleUrl`.

## Interpolacija

**Interpolacija** (engl. *interpolation*) je zapis `{{ izraz }}` u šablonu, koji na tom mestu ispisuje vrednost izraza kao tekst. Izraz najčešće čita polje ili poziva metodu klase. Sledeći kod prikazuje potpunu komponentu kartice, sve tri datoteke, koja ispisuje naziv i opis:

Klasa (`tour-card.ts`):

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
}
```

Šablon (`tour-card.html`):

```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
</article>
```

Stilovi (`tour-card.scss`):

```scss
article {
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
}

h3 {
  margin: 0 0 0.5rem;
}

p {
  margin: 0;
  color: #555;
}
```

U datom kodu treba uočiti sledeće:
- Šablon vidi članove klase po nazivu, bez `this`. Polje `name` u klasi i `name` u šablonu su isto polje.
- Prevodilac šablon prevodi kao deo klase, pa šablon vidi članove označene sa `protected`, a ne vidi one označene sa `private`. Zato su polja koja šablon čita `protected`. Prevodilac proverava šablon isto kao klasu i prijavljuje grešku ako šablon pristupa privatnom članu ili članu koji ne postoji.
- Stilovi iz `tour-card.scss` važe samo za ovu komponentu. Angular ih ograničava na njen šablon, pa selektori `article`, `h3` i `p` ovde ne utiču na iste elemente u drugim komponentama.

## Vezivanje svojstva

Interpolacija ispisuje tekst između oznaka elementa. Kada vrednost iz klase treba da odredi svojstvo elementa, poput toga da li je dugme onemogućeno, koristimo vezivanje svojstva. **Vezivanje svojstva** (engl. *property binding*) se ostvaruje kroz zapis `[svojstvo]="izraz"` na elementu, koji svojstvu elementa dodeljuje vrednost izraza. Sledeći kod prikazuje dugme čija dostupnost zavisi od polja klase:

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

**Vezivanje događaja** (engl. *event binding*) se ostvaruje kroz zapis `(događaj)="izraz"` na elementu, koji izvršava izraz kada se na elementu desi navedeni događaj. Izraz najčešće poziva metodu klase. Sledeći kod prikazuje dugme koje poziva metodu:

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
- Zagrade oko `click` znače da je desna strana izraz koji se izvršava pri kliku.
- Reč `$event` je objekat događaja koji pregledač pravi. Prosleđujemo ga metodi kada joj treba, kada metodi ne treba, poziv je `(click)="publish()"`.

## Kartica ture

Povežimo pojmove u jednu komponentu. Kartica prikazuje naziv i opis ture i ima dugme koje je onemogućeno kada je tura već objavljena:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
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

Kada pokrenemo aplikaciju i kliknemo na dugme, metoda `publish` se izvršava, polje `published` dobija vrednost `true`, a dugme postaje onemogućeno. Prikaz se osvežio jer je promenu izazvao događaj iz šablona (klik).

## Prikaz ne prati svaku promenu

Logično je pomisliti da Angular stalno posmatra polje `published` i osvežava prikaz čim se ono promeni. Međutim, Angular ponovo iscrta komponentu samo kada zna da se nešto promenilo, a najčešći povod za to je upravo događaj iz šablona, na primer klik, unos teksta i slično. Pošto je taj događaj Angular sam pokrenuo, on zna da posle njega treba osvežiti prikaz.

Problem nastaje kada se polje promeni bez takvog događaja. Zamislimo da se tura objavi van šablona tj. da je taj podatak stigao sa servera. To ovde simuliramo tajmerom koji posle jedne sekunde postavi `published` na `true`, a dugme ovoga puta nema klik:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
  protected published = false;

  constructor() {
    setTimeout(() => {
      this.published = true;
    }, 1000);
  }
}
```

```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
  <button type="button" [disabled]="published">Objavi</button>
</article>
```

U datom kodu treba uočiti sledeće:
- Posle jedne sekunde polje `published` zaista dobija vrednost `true`, u to se uverimo ispisom u konzoli unutar tajmera.
- Ipak, dugme ostaje omogućeno. Prikaz je zaostao za podatkom, jer promenu nije izazvao nijedan događaj iz šablona, pa Angular ne zna da treba ponovo da iscrta karticu.

**Napomena:** ovo ponašanje važi u modernom, *zoneless* Angular-u tj. verziji Angulara koja u sebe uvodi signale.

Obično polje, dakle, prikaz prati samo kada uz promenu ide i događaj iz šablona. Nama treba polje čiju svaku promenu Angular primeti, bez obzira odakle promena dolazi. Navedeni zahtev ispunjavaju **signali** i njima se bavimo u [narednoj lekciji o signalima](4-signali.md).
