# Komponente

Komponenta je klasa sa pridruženim HTML šablonom koja upravlja jednim delom stranice. U React-u istu ulogu ima funkcija koja vraća JSX. Sledeći kod prikazuje jednu karticu u oba oblika:

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
3. `tour-card.css` sadrži stilove, na koje dekorator upućuje podešavanjem `styleUrl`.

Ove datoteke ne pravimo ručno, već komandom `ng generate component tour-card` (skraćeno `ng g c tour-card`). Komanda pravi direktorijum `src/app/tour-card/` sa sve tri datoteke, kao i datotekom `tour-card.spec.ts` u kojoj se piše test komponente.

## Interpolacija

**Interpolacija** (engl. *interpolation*) je zapis `{{ izraz }}` u šablonu, koji na tom mestu ispisuje vrednost izraza kao tekst. Izraz najčešće čita polje ili poziva metodu klase. Sledeći kod prikazuje potpunu komponentu kartice, sve tri datoteke, koja ispisuje naziv i opis:

Klasa (`tour-card.ts`):

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.css',
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

Stilovi (`tour-card.css`):

```css
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
- Šablon vidi članove označene sa `public` i `protected`, a ne vidi one označene sa `private`. Zato su polja koja šablon čita `protected`. Angular prevodilac proverava i šablon i prijavljuje grešku ako šablon pristupa privatnom članu ili članu koji ne postoji.
- Stilovi iz `tour-card.css` važe samo za ovu komponentu. Angular ih ograničava na njen šablon, pa selektori `article`, `h3` i `p` ovde ne utiču na iste elemente u drugim komponentama.

## Upotreba komponente

Napravljena komponenta se ne prikazuje sama od sebe. Da bi se kartica pojavila na stranici, treba da je upotrebimo u šablonu neke druge komponente, na primer korenske. Sledeći kod prikazuje korensku komponentu koja koristi karticu:

```ts
import { Component } from '@angular/core';
import { TourCard } from './tour-card/tour-card';

@Component({
  selector: 'app-root',
  imports: [TourCard],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
```

```html
<main>
  <app-tour-card></app-tour-card>
</main>
```

U datom kodu treba uočiti sledeće:
- Niz `imports` u dekoratoru navodi komponente koje šablon korenske komponente sme da koristi. Klasu `TourCard` najpre uvozimo naredbom `import`, a zatim je navodimo u tom nizu. Ako to izostavimo, prevodilac prijavljuje grešku da ne poznaje element `app-tour-card`.
- Naziv elementa `app-tour-card` odgovara vrednosti `selector` iz dekoratora kartice. Na tom mestu Angular iscrtava šablon kartice, isto kao što šablon korenske komponente iscrtava unutar elementa `app-root`.

## Vezivanje svojstva

Kada vrednost iz klase treba da odredi svojstvo elementa, poput toga da li je dugme onemogućeno, koristimo vezivanje svojstva. **Vezivanje svojstva** (engl. *property binding*) se ostvaruje kroz zapis `[svojstvo]="izraz"` na elementu, koji svojstvu elementa dodeljuje vrednost izraza. Sledeći kod prikazuje dugme čija dostupnost zavisi od polja klase:

```ts
export class TourCard {
  protected readonly published = true;
}
```

```html
<button type="button" [disabled]="published">Objavi</button>
```

U datom kodu treba uočiti sledeće:
- Uglaste zagrade oko `disabled` znače da se desna strana tumači kao izraz, a ne kao tekst. Bez zagrada bi `disabled="published"` bio običan HTML atribut sa tekstom `published`. Tada bi dugme bilo uvek onemogućeno, jer za atribut `disabled` nije bitna vrednost, već samo to da li je naveden.
- Kada je `published` tačno, dugme je onemogućeno. Isti zapis radi za svako svojstvo elementa, na primer `[value]` za sadržaj polja za unos.

## Vezivanje događaja

**Vezivanje događaja** (engl. *event binding*) se ostvaruje kroz zapis `(događaj)="izraz"` na elementu, koji izvršava izraz kada se na elementu desi navedeni događaj. Izraz najčešće poziva metodu klase. Sledeći kod prikazuje dugme koje poziva metodu:

```ts
export class TourCard {
  protected logClick(event: Event): void {
    console.log(event.target);
  }
}
```

```html
<button type="button" (click)="logClick($event)">Objavi</button>
```

U datom kodu treba uočiti sledeće:
- Zagrade oko `click` znače da je desna strana izraz koji se izvršava pri kliku.
- Reč `$event` je objekat događaja koji pravi internet čitač. Prosleđujemo ga metodi kada joj treba. Kada metodi ne treba, poziv je `(click)="logClick()"`.

## Kartica ture

Povežimo pojmove u jednu komponentu. Kartica prikazuje naziv i opis ture i ima dugme koje je onemogućeno kada je tura već objavljena:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.css',
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

Kada pokrenemo aplikaciju i kliknemo na dugme, metoda `publish` se izvršava, polje `published` dobija vrednost `true`, a dugme postaje onemogućeno. Prikaz se automatski osvežio jer je promenu izazvao događaj iz šablona (klik).

## Prikaz ne prati svaku promenu

Logično je pomisliti da Angular stalno posmatra polje `published` i osvežava prikaz čim se ono promeni. Međutim, Angular osvežava prikaz komponente samo kada zna da se nešto promenilo, a najčešći povod za to je upravo događaj iz šablona, na primer klik, unos teksta i slično. Pošto je Angular sam postavio osluškivač za taj događaj, on zna da posle njega treba osvežiti prikaz. Pri tome Angular ne iscrtava komponentu iznova, već menja samo one delove stranice čija se vrednost promenila.

Problem nastaje kada se polje promeni bez takvog događaja. Zamislimo da se tura objavi van šablona, tj. da je taj podatak stigao sa servera. To ovde simuliramo tajmerom koji posle jedne sekunde postavi `published` na `true`, a dugme ovoga puta nema klik:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.css',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
  protected published = false;

  constructor() {
    setTimeout(() => {
      this.published = true;
      console.log('published:', this.published);
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
- Posle jedne sekunde polje `published` zaista dobija vrednost `true`. U to se uveravamo ispisom u konzoli unutar tajmera.
- Ipak, dugme ostaje omogućeno. Prikaz je zaostao za podatkom, jer promenu nije izazvao nijedan događaj iz šablona, pa Angular ne zna da treba da osveži karticu.

**Napomena:** Opisano ponašanje važi za najnovije verzije Angulara, koje ne koriste biblioteku Zone.js (engl. *zoneless*) i u kojima komponente podrazumevano osvežavaju prikaz samo kada Angular dobije obaveštenje da se nešto promenilo (strategija *OnPush*). Starije verzije Angulara koristile su Zone.js, biblioteku koja presreće tajmere, zahteve ka serveru i slične operacije i posle svake od njih osvežava prikaz. U takvim projektima bi se dugme iz primera ipak osvežilo. Zato ćete na internetu naići na starije primere koji menjaju obično polje i očekuju da se prikaz sam osveži.

Obično polje, dakle, prikaz prati samo kada uz promenu ide i događaj iz šablona. Nama treba polje čiju svaku promenu Angular primeti, bez obzira odakle promena dolazi. Navedeni zahtev ispunjavaju **signali** i njima se bavimo u [narednoj lekciji o signalima](4-signali.md).
