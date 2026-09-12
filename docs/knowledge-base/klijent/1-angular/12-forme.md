Stranica za pravljenje ture prikuplja naziv, opis, težinu i oznake, a komandu za pravljenje sme da pošalje tek kada su naziv i opis uneti. Čitalac zna HTML formu sa elementima za unos i zna da vrednost elementa pročita iz svojstva `value`, kao u lekciji o kontroli toka. Za formu sa četiri elementa to znači četiri reference na elemente, četiri vezivanja događaja i proveru svakog unosa pre slanja. Vrednosti forme su stanje stranice, pa po pravilu iz lekcije o signalima žive u signalu. Deo radnog okvira za forme oko tog signala gradi objekat koji elemente za unos vezuje za svojstva signala i za svako svojstvo prati da li je uneta vrednost ispravna. Ovde upoznajemo taj objekat, bez slanja forme.

## Forma

**Forma** (engl. *form*) je objekat izgrađen oko signala sa vrednostima za unos, koji za svako svojstvo tog signala čuva polje forme sa vrednošću, oznakom da li ga je korisnik dodirnuo i greškama validacije. Signal oko kog je forma izgrađena zovemo model. **Šema** (engl. *schema*) je funkcija koja poljima forme dodeljuje pravila validacije. Sledeći kod prikazuje, iz projekta, model i formu stranice za pravljenje ture:

```ts
protected readonly form = form(
  signal({ name: '', description: '', difficulty: 'Easy' as TourDifficulty, tags: '' }),
  (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  },
);
```

U datom kodu treba uočiti sledeće:
- Funkcije `form` i `required` su deo radnog okvira. Forma nema sopstvenu kopiju vrednosti, već svaki unos korisnika upisuje u model.
- Model ima po jedno svojstvo za svaki element za unos, sa početnom vrednošću. Tip `TourDifficulty` iz modula Exploration je unija čije su vrednosti tačno tri teksta, `'Easy' | 'Moderate' | 'Hard'`. Zapis `'Easy' as TourDifficulty` prevodiocu tvrdi da je početna vrednost tog tipa, a ne bilo koji tekst, pa i svojstvo modela dobija tip `TourDifficulty`.
- Šema prima `path`, objekat sa istim svojstvima kao model, kroz koji se pravilo vezuje za jedno polje forme. Poziv `required(path.name, ...)` polju `name` dodaje pravilo da vrednost ne sme da bude prazna. Pravila se proveravaju pri svakoj promeni modela. Polja bez pravila, `difficulty` i `tags`, uvek su ispravna.
- Drugi argument pravila je poruka koju forma čuva uz grešku.

Pravilo `min` polju sa brojem dodaje najmanju dozvoljenu vrednost. Sledeći kod prikazuje, iz projekta, šemu forme za vreme obilaska, koju stranica tura korisnika prikazuje ispod tabele i koju smo u ranijim lekcijama izostavili. Njen model ima svojstvo `minutes` sa početnom vrednošću `30`:

```ts
(path) => {
  required(path.minutes, { message: 'Minutes are required.' });
  min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
}
```

Jedno polje sme da ima više pravila. Ostala pravila radnog okvira se pišu na isti način, sa poljem forme, argumentima pravila i porukom.

## Tri oblika zapisa

Forma i svako polje forme se, kao signal, čitaju pozivom. Zato se forma u klasi i šablonu pojavljuje u tri oblika, koje treba razlikovati:

| Zapis | Značenje | Kada se koristi |
|---|---|---|
| `form.name` | Polje forme `name` | Vezivanje za element za unos |
| `form.name()` | Stanje polja `name` | Čitanje vrednosti i grešaka jednog polja |
| `form()` | Stanje cele forme | Provera da li je cela forma ispravna |

Stanje polja i stanje cele forme imaju signale koje čitamo pozivom. U projektu se koriste četiri:
1. `value()` je vrednost polja, istog tipa kao svojstvo modela.
2. `touched()` je tačno kada je polje dodirnuto, odnosno nakon što je korisnik napustio element za unos vezan za polje i na njemu se desio događaj `blur`.
3. `errors()` je niz grešaka polja. Svaka greška ima svojstvo `message` sa porukom iz šeme. Niz je prazan kada polje nema grešaka.
4. `valid()` je tačno kada polje nema grešaka. Za celu formu, `form().valid()`, tačno je kada nijedno polje nema grešaka.

Poziv `this.form.name().value()` u klasi tako vraća uneti naziv.

## Vezivanje elementa za polje

Element za unos se za polje forme vezuje direktivom `FormField`, koja se navodi u podešavanju `imports`, a na elementu se piše kao vezivanje svojstva `[formField]`. Sledeći kod prikazuje, iz projekta, tri elementa za unos sa stranice za pravljenje ture:

```ts
@Component({
  imports: [FormField],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
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
- Desna strana vezivanja je polje forme, `form.name`, bez zagrada. Direktiva upisuje vrednost polja u element, a pri svakom događaju `input` sadržaj elementa u model, kroz signal, pa pretplatnici modela dobijaju obaveštenje. Referenca na element i vezivanje događaja iz lekcije o kontroli toka više nisu potrebni.
- Ista direktiva radi na elementima `input`, `textarea` i `select`. Za `select` u model upisuje `value` izabrane opcije.
- Kada je element `input` tipa `number`, a svojstvo modela broj, direktiva u model upisuje broj, a ne tekst. Element za minute iz forme za vreme obilaska, `<input id="minutes" type="number" [formField]="form.minutes" />`, zato kada korisnik unese 45 u `minutes` upisuje broj `45`, pa pravilo `min` poredi brojeve.

## Prikaz grešaka

Polje sa pravilom `required` ima grešku čim se stranica otvori, jer je početna vrednost prazna. Poruka pre prvog unosa nema smisla za korisnika, pa se greške prikazuju tek kada je polje dodirnuto. Sledeći kod prikazuje, iz projekta, element za naziv sa porukama o greškama:

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
- Naredba `@if` čita `touched` iz stanja polja. Polje ostaje dodirnuto i kada se korisnik vrati u element.
- Petlja prolazi kroz niz grešaka i ispisuje poruku svake. Greške nemaju identifikator, pa se koristi `track $index`.
- Šablon je pretplatnik signala `touched` i `errors`. Kada korisnik unese naziv, greška nestaje iz niza i radni okvir uklanja poruku bez ikakvog koda u klasi.

## Pravljenje ture

Povežimo pojmove u stranicu za pravljenje ture iz projekta, skraćenu na formu i njenu validaciju:

```ts
@Component({
  imports: [FormField],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
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

Kada korisnik otvori stranicu, klikne u element za naziv, pa bez unosa pređe na opis, dešava se sledeće:
1. Pri prvom iscrtavanju direktiva upisuje početne vrednosti modela u elemente. Polje `name` već ima grešku iz pravila `required`, ali nije dodirnuto, pa se poruka ne prikazuje.
2. Korisnik napušta element za naziv. Na događaj `blur` direktiva upisuje tačno u `touched` polja `name`.
3. Šablon je pretplatnik tog signala, pa radni okvir ponovo iscrtava stranicu, sada sa blokom `@if` u kom petlja ispisuje poruku „Name is required.“.
4. Korisnik se vraća i unosi slovo. Na događaj `input` direktiva upisuje sadržaj elementa u svojstvo `name` modela. Pravilo `required` je zadovoljeno, pa `errors` postaje prazan niz.
5. Radni okvir ponovo iscrtava stranicu. Petlja nema elemenata i poruka nestaje. Kada je i opis unet, `form().valid()` postaje tačno.
