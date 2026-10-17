Stranica za pravljenje ture prikuplja naziv, opis, težinu i oznake, a komandu za pravljenje sme da pošalje tek kada su naziv i opis uneti.

Do sada je korisnik stanje stranice menjao samo klikom na dugme. Forma traži više od toga. Sadržaj svakog elementa za unos treba da stigne u stanje stranice pri svakoj promeni, a svaki unos treba proveriti pre slanja. Uz to bismo morali sami da pamtimo koje je polje korisnik već popunio, da ne bismo prikazivali greške pre nego što je išta uneo.

Vrednosti forme su stanje stranice, pa po pravilu iz lekcije o signalima treba da žive u signalu. Angular oko tog signala gradi objekat koji elemente za unos povezuje sa vrednostima u signalu i za svaku vrednost prati da li je ispravna. Ovde upoznajemo taj objekat.

## Model i forma

Formu čine tri stvari: **model**, sama **forma** i njena **polja**.

**Model** je signal koji drži vrednosti forme. Za formu pravljenja ture to su `name`, `description`, `difficulty` i `tags`. Model je jedini izvor tih vrednosti. Kada korisnik nešto otkuca, menja se model.

**Forma** (engl. *form*) je objekat koji Angular gradi oko modela, pozivom `form(...)`. Za svako svojstvo modela ona pravi po jedno **polje forme** (engl. *form field*). U šablonu polje forme vezujemo za jedan element za unos. Polje o svom svojstvu zna tri stvari: trenutnu vrednost, da li ga je korisnik dodirnuo i koje greške validacije trenutno ima.

**Šema** (engl. *schema*) je drugi argument koji predajemo pozivu `form` tj. funkcija koja pojedinim poljima dodeljuje pravila validacije (na primer, da `name` ne sme da bude prazno). Angular ta pravila proverava pri svakoj promeni modela.

Sledeći kod prikazuje formu stranice za pravljenje ture. Signal-model se predaje funkciji `form` direktno, na licu mesta:

```ts
import { FormField, form, required } from '@angular/forms/signals';

export class CreateTour {
  protected readonly form = form(
    signal({ name: '', description: '', difficulty: 'Easy' as TourDifficulty, tags: '' }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      required(path.description, { message: 'Opis je obavezan.' });
    },
  );
}
```

U datom kodu treba uočiti sledeće:

- Funkcije `form` i `required` i klasa `FormField` uvoze se iz paketa `@angular/forms/signals`.
- Model je objekat koji predajemo funkciji `form`, ovde `signal({ name: '', ... })`. Njegov oblik prevodilac zaključuje iz početnih vrednosti, poseban interfejs za model ne pišemo.
- Tip težine je `TourDifficulty`, imenovana unija literala `'Easy' | 'Moderate' | 'Hard'`. Zapis `'Easy' as TourDifficulty` prevodiocu kaže da je početna vrednost baš te unije, a ne običan `string`; za npr. `'Lako'` prijavio bi grešku.
- Šema prima `path`, objekat sa istim svojstvima kao model, kroz koji se pravilo vezuje za jedno polje forme. Poziv `required(path.name, ...)` polju `name` dodaje pravilo da vrednost ne sme da bude prazna. Drugi argument je poruka koju forma čuva uz grešku.
- Pravila se proveravaju pri svakoj promeni modela. Polja bez pravila, `difficulty` i `tags`, uvek su ispravna.

Model može da stoji i kao **zasebno polje klase**, kada nam treba i van poziva `form`. Sledeći kod prikazuje takav model u formi za dodavanje vremena obilaska ture. Jedno polje forme sme da ima i više pravila, npr. `min` broju dodaje najmanju dozvoljenu vrednost:

```ts
private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

protected readonly form = form(this.model, (path) => {
  required(path.minutes, { message: 'Broj minuta je obavezan.' });
  min(path.minutes, 1, { message: 'Broj minuta mora da bude najmanje 1.' });
});
```

Ostala pravila pišu se na isti način: prvo polje forme, zatim argumenti pravila, ako ih ima, i na kraju poruka.

Do vrednosti forme možemo doći na nekoliko načina, koje je lako pomešati. Forma i svako polje forme čitaju se pozivom, kao signal, pa se u klasi i šablonu pojavljuju u više oblika:

| Zapis | Značenje | Kada se koristi |
|---|---|---|
| `form.name` | Polje forme `name` | Vezivanje za element za unos |
| `form.name()` | Stanje polja `name` | Čitanje vrednosti i grešaka jednog polja |
| `form()` | Stanje cele forme | Provera da li je cela forma ispravna |

Stanje polja i stanje cele forme imaju signale koje čitamo pozivom. Najčešće se koriste četiri:

1. `value()` je vrednost polja, istog tipa kao svojstvo modela.
2. `touched()` je tačno kada je polje dodirnuto, odnosno nakon što je korisnik napustio element za unos vezan za polje i na njemu se desio događaj `blur`.
3. `errors()` je niz grešaka polja. Svaka greška ima svojstvo `message` sa porukom iz šeme. Niz je prazan kada polje nema grešaka.
4. `valid()` je tačno kada polje nema grešaka. Za celu formu, `form().valid()` je tačno kada nijedno polje nema grešaka.

Vrednost jednog polja čitamo kao `this.form.name().value()`. Kada model stoji kao zasebno polje, njegova cela vrednost je dostupna i kao `this.model()`.

## Vezivanje elementa za polje

Element za unos vezuje se za polje forme vezivanjem svojstva `[formField]`, a klasa `FormField` navodi se u podešavanju `imports`. Sledeći kod prikazuje tri elementa za unos sa stranice za pravljenje ture:

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
  <option value="Easy">Laka</option>
  <option value="Moderate">Srednja</option>
  <option value="Hard">Teška</option>
</select>
```

U datom kodu treba uočiti sledeće:

- Ako `FormField` nije naveden u podešavanju `imports`, prevodilac prijavljuje grešku da `formField` nije poznato svojstvo elementa.
- Desna strana vezivanja je polje forme, `form.name`, bez zagrada. Angular upisuje vrednost polja u element, a pri svakom događaju `input` upisuje sadržaj elementa u model. Model je signal, pa sve što ga čita dobija obaveštenje o promeni.
- Isto vezivanje radi na elementima `input`, `textarea` i `select`. Za `select` u model upisuje `value` izabrane opcije, npr. `'Easy'`, a ne tekst `Laka` koji korisnik vidi.
- Kada je element `input` tipa `number`, a svojstvo modela broj, Angular u model upisuje broj, a ne tekst. Element za minute iz forme za vreme obilaska, `<input id="minutes" type="number" [formField]="form.minutes" />`, zato kada korisnik unese 45 upisuje broj `45`, pa pravilo `min` poredi brojeve.

## Prikaz grešaka

Polje sa pravilom `required` ima grešku čim se stranica otvori, jer je početna vrednost prazna. Poruka pre prvog unosa nema smisla za korisnika, pa se greške prikazuju tek kada je polje dodirnuto. Sledeći kod prikazuje element za naziv sa porukama o greškama:

```html
<div class="field">
  <label for="name">Naziv</label>
  <input id="name" type="text" [formField]="form.name" />
  @if (form.name().touched()) {
    @for (message of form.name().errors(); track $index) {
      <p class="error">{{ message.message }}</p>
    }
  }
</div>
```

U datom kodu treba uočiti sledeće:

- Petlja prolazi kroz niz grešaka i ispisuje poruku svake. Greške nemaju identifikator, pa se koristi `track $index`, kao za poruke o greškama u lekciji o kontroli toka. Svaka greška ima svojstvo `message`, pa je ispisujemo kao `{{ message.message }}`.
- Šablon je čitalac signala `touched` i `errors`. Kada korisnik unese naziv, greška nestaje iz niza i Angular uklanja poruku, bez ikakvog koda u klasi.
