
Stranica za pravljenje ture prikuplja naziv, opis, težinu i oznake, a komandu za pravljenje
sme da pošalje tek kada su naziv i opis uneti.

Takvu formu bismo mogli da napravimo i onim što već znamo. U lekciji o kontroli toka
vrednost polja za pretragu čitali smo preko reference na element i vezivanja događaja. Za
formu sa četiri elementa to znači četiri reference, četiri vezivanja događaja i posebnu
proveru svakog unosa pre slanja. Uz to bismo morali sami da pamtimo koje je polje korisnik
već popunio, da ne bismo prikazivali greške pre nego što je išta uneo.

Vrednosti forme su stanje stranice, pa po pravilu iz lekcije o signalima treba da žive u
signalu. Angular oko tog signala gradi objekat koji elemente za unos povezuje sa
vrednostima u signalu i za svaku vrednost prati da li je ispravna. Ovde upoznajemo taj
objekat, bez slanja forme, koje dolazi u narednoj lekciji.

> **Napomena:** Forme sa signalima (engl. *Signal Forms*) stabilne su od Angulara 22.
> Starije verzije koriste *Reactive Forms* (`FormGroup`, `FormControl`) i *template-driven*
> forme (`ngModel`), pa ćete na internetu često naići na primere sa njima. U projektu
> koristimo isključivo forme sa signalima.

## Model i forma

Formu čine tri stvari: **model**, sama **forma** i njena **polja**.

**Model** je signal koji drži vrednosti forme tj. po jedno svojstvo svako.
Za formu pravljenja ture to su `name`, `description`, `difficulty` i `tags`. Model je
jedini izvor tih vrednosti: kada korisnik nešto otkuca, menja se upravo model.

**Forma** (engl. *form*) je objekat koji Angular gradi oko modela, pozivom `form(...)`. Za
svako svojstvo modela ona pravi po jedno **polje forme** (engl. *form field*). Polje forme
je ono što u šablonu vezujemo za jedan element za unos, i ono o svom svojstvu zna tri
stvari: trenutnu vrednost, da li ga je korisnik dodirnuo i koje greške validacije trenutno
ima.

**Šema** (engl. *schema*) je drugi argument koji predajemo pozivu `form` tj. funkcija koja
pojedinim poljima dodeljuje pravila validacije (na primer, da `name` ne sme da bude
prazno). Angular ta pravila proverava pri svakoj promeni modela.

Sledeći kod prikazuje, iz projekta, formu stranice za pravljenje ture. Signal-model se
predaje funkciji `form` direktno, na licu mesta:

```ts
import { Component, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { TourDifficulty } from '../../api/exploration-api-types';

export class CreateTour {
  protected readonly form = form(
    signal({ name: '', description: '', difficulty: 'Easy' as TourDifficulty, tags: '' }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
    },
  );
}
```

Njen šablon (`create-tour.html`) svaki element za unos vezuje za odgovarajuće polje forme,
zapisom `[formField]="form.<svojstvo>"`. Ovde ga dajemo skraćeno, bez labela i poruka o
greškama, koje dolaze u kasnijim odeljcima:

```html
<form>
  <input id="name" type="text" [formField]="form.name" />

  <textarea id="description" rows="4" [formField]="form.description"></textarea>

  <select id="difficulty" [formField]="form.difficulty">
    <option value="Easy">Easy</option>
    <option value="Moderate">Moderate</option>
    <option value="Hard">Hard</option>
  </select>

  <input id="tags" type="text" [formField]="form.tags" />
</form>
```

U datom kodu treba uočiti sledeće:

- Funkcije `form` i `required` i klasa `FormField` uvoze se iz `@angular/forms/signals`, a
  ne iz `@angular/forms`. Pogrešna putanja je najčešća prva greška sa formama.
- Model je objekat koji predajemo funkciji `form`, ovde `signal({ name: '', ... })`. Njegov
  oblik prevodilac zaključuje iz početnih vrednosti, poseban interfejs za model ne pišemo.
- Tip težine je `TourDifficulty` iz modula Exploration, unija tri teksta
  `'Easy' | 'Moderate' | 'Hard'`. Zapis `'Easy' as TourDifficulty` prevodiocu kaže da je
  početna vrednost baš te unije, a ne običan `string`; za npr. `'Lako'` prijavio bi grešku.
- Svaki unos korisnika upisuje se direktno u signal-model, pa polje forme uvek vraća ono što je korisnik uneo.
- Šema prima `path`, objekat sa istim svojstvima kao model, kroz koji se pravilo vezuje za
  jedno polje forme. Poziv `required(path.name, ...)` polju `name` dodaje pravilo da
  vrednost ne sme da bude prazna. Drugi argument je poruka koju forma čuva uz grešku.
- Pravila se proveravaju pri svakoj promeni modela. Polja bez pravila, `difficulty` i
  `tags`, uvek su ispravna.

Model može da stoji i kao **zasebno polje klase**, kada nam treba i van poziva `form`
(npr. da bismo formu vratili na početne vrednosti). Tako je u `MyTours`, u formi za
dodavanje vremena obilaska. Jedno polje forme sme da ima i više pravila npr. `min` broju
dodaje najmanju dozvoljenu vrednost:

```ts
private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

protected readonly form = form(this.model, (path) => {
  required(path.minutes, { message: 'Minutes are required.' });
  min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
});
```

Ostala pravila pišu se na isti način: prvo polje forme, zatim argumenti pravila, ako ih ima, i na
kraju poruka.

Do vrednosti forme možemo doći na nekoliko načina, koje je lako pomešati. Forma i svako
polje forme čitaju se pozivom, kao signal, pa se u klasi i šablonu pojavljuju u više
oblika:

| Zapis | Značenje | Kada se koristi |
|---|---|---|
| `form.name` | Polje forme `name` | Vezivanje za element za unos |
| `form.name()` | Stanje polja `name` | Čitanje vrednosti i grešaka jednog polja |
| `form()` | Stanje cele forme | Provera da li je cela forma ispravna |

Stanje polja i stanje cele forme imaju signale koje čitamo pozivom. U projektu se koriste
četiri:

1. `value()` je vrednost polja, istog tipa kao svojstvo modela.
2. `touched()` je tačno kada je polje dodirnuto, odnosno nakon što je korisnik napustio
   element za unos vezan za polje i na njemu se desio događaj `blur`.
3. `errors()` je niz grešaka polja. Svaka greška ima svojstvo `message` sa porukom iz šeme.
   Niz je prazan kada polje nema grešaka.
4. `valid()` je tačno kada polje nema grešaka. Za celu formu, `form().valid()` je tačno
   kada nijedno polje nema grešaka.

Vrednost jednog polja čitamo kao `this.form.name().value()`. Tako u projekto i čitamo unos pri
slanju, polje po polje (`this.form.name().value()`, `this.form.description().value()`,
…), a ne ceo model odjednom. Kada model stoji kao zasebno polje, kao u `MyTours`, njegova
cela vrednost je dostupna i kao `this.model()`.

## Vezivanje elementa za polje

Element za unos vezuje se za polje forme vezivanjem svojstva `[formField]`, a klasa
`FormField` navodi se u podešavanju `imports`. Sledeći kod prikazuje, iz projekta, tri
elementa za unos sa stranice za pravljenje ture:

```ts
@Component({
  selector: 'app-create-tour',
  imports: [FormField],
  templateUrl: './create-tour.html',
  styleUrl: './create-tour.scss',
})
```

```html
<input id="name" type="text" [formField]="form.name" />

<textarea id="description" rows="4" [formField]="form.description"></textarea>

<select id="difficulty" [formField]="form.difficulty">
  <option value="Easy">Easy</option>
  <option value="Moderate">Moderate</option>
  <option value="Hard">Hard</option>
</select>
```

U datom kodu treba uočiti sledeće:

- Ako `FormField` nije naveden u podešavanju `imports`, vezivanje `[formField]` ne radi. Elementi se prikazuju, ali ništa što korisnik unese ne
  stiže u model.
- Desna strana vezivanja je polje forme, `form.name`, bez zagrada. Angular upisuje vrednost
  polja u element, a pri svakom događaju `input` upisuje sadržaj elementa u model. Model je
  signal, pa sve što ga čita dobija obaveštenje o promeni. Referenca na element i vezivanje
  događaja iz lekcije o kontroli toka više nisu potrebni.
- Isto vezivanje radi na elementima `input`, `textarea` i `select`. Za `select` u model
  upisuje `value` izabrane opcije.
- Kada je element `input` tipa `number`, a svojstvo modela broj, Angular u model upisuje
  broj, a ne tekst. Element za minute iz forme za vreme obilaska,
  `<input id="minutes" type="number" [formField]="form.minutes" />`, zato kada korisnik
  unese 45 upisuje broj `45`, pa pravilo `min` poredi brojeve.

## Prikaz grešaka

Polje sa pravilom `required` ima grešku čim se stranica otvori, jer je početna vrednost
prazna. Poruka pre prvog unosa nema smisla za korisnika, pa se greške prikazuju tek kada je
polje dodirnuto. Sledeći kod prikazuje, iz projekta, element za naziv sa porukama o
greškama:

```html
<div class="field">
  <label for="name">Name</label>
  <input id="name" type="text" [formField]="form.name" />
  @if (form.name().touched()) {
    @for (message of form.name().errors(); track $index) {
      <p class="error">{{ message.message }}</p>
    }
  }
</div>
```

U datom kodu treba uočiti sledeće:

- Petlja prolazi kroz niz grešaka i ispisuje poruku svake. Greške nemaju identifikator, pa
  se koristi `track $index`, kao za poruke o greškama u lekciji o kontroli toka. Svaka
  greška ima svojstvo `message`, pa je ispisujemo kao `{{ message.message }}`.
- Šablon je čitalac signala `touched` i `errors`. Kada korisnik unese naziv, greška nestaje
  iz niza i Angular uklanja poruku, bez ikakvog koda u klasi.

## Pravljenje ture

Povežimo pojmove u stranicu za pravljenje ture iz projekta, skraćenu na formu i njenu
validaciju:

```ts
import { Component, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { TourDifficulty } from '../../api/exploration-api-types';

@Component({
  selector: 'app-create-tour',
  imports: [FormField],
  templateUrl: './create-tour.html',
  styleUrl: './create-tour.scss',
})
export class CreateTour {
  protected readonly form = form(
    signal({ name: '', description: '', difficulty: 'Easy' as TourDifficulty, tags: '' }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
    },
  );
}
```

```html
<h1>Create tour</h1>

<form>
  <div class="field">
    <label for="name">Name</label>
    <input id="name" type="text" [formField]="form.name" />
    @if (form.name().touched()) {
      @for (message of form.name().errors(); track $index) {
        <p class="error">{{ message.message }}</p>
      }
    }
  </div>

  <div class="field">
    <label for="description">Description</label>
    <textarea id="description" rows="4" [formField]="form.description"></textarea>
    @if (form.description().touched()) {
      @for (message of form.description().errors(); track $index) {
        <p class="error">{{ message.message }}</p>
      }
    }
  </div>

  <div class="field">
    <label for="difficulty">Difficulty</label>
    <select id="difficulty" [formField]="form.difficulty">
      <option value="Easy">Easy</option>
      <option value="Moderate">Moderate</option>
      <option value="Hard">Hard</option>
    </select>
  </div>

  <div class="field">
    <label for="tags">Tags, separated by commas</label>
    <input id="tags" type="text" [formField]="form.tags" />
  </div>
</form>
```
