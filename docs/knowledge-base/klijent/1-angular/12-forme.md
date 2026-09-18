Stranica za pravljenje ture prikuplja naziv, opis, težinu i oznake, a komandu za pravljenje sme da pošalje tek kada su naziv i opis uneti.

Takvu formu bismo mogli da napravimo i onim što već znamo. U lekciji o kontroli toka vrednost polja za pretragu čitali smo preko reference na element i vezivanja događaja. Za formu sa četiri elementa to znači četiri reference, četiri vezivanja događaja i posebnu proveru svakog unosa pre slanja. Uz to bismo morali sami da pamtimo koje je polje korisnik već popunio, da ne bismo prikazivali greške pre nego što je išta uneo.

Vrednosti forme su stanje stranice, pa po pravilu iz lekcije o signalima treba da žive u signalu. Angular oko tog signala gradi objekat koji elemente za unos povezuje sa vrednostima u signalu i za svaku vrednost prati da li je ispravna. Ovde upoznajemo taj objekat, bez slanja forme, koje dolazi u narednoj lekciji.

> **Napomena:** Forme sa signalima (engl. *Signal Forms*) stabilne su od Angulara 22. Starije verzije koriste *Reactive Forms* (`FormGroup`, `FormControl`) i *template-driven* forme (`ngModel`), pa ćete na internetu često naići na primere sa njima. U projektu koristimo isključivo forme sa signalima.

## Model i forma

Vrednosti forme drži signal koji zovemo **model**. Model je običan signal sa po jednim svojstvom za svaki element za unos. Njegov oblik opisujemo interfejsom:

```ts
interface CreateTourModel {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string;
}
```

**Forma** (engl. *form*) je objekat izgrađen oko modela, koji za svako svojstvo modela pravi **polje forme**. Polje forme zna vrednost svog svojstva, da li ga je korisnik dodirnuo i koje greške validacije ima. **Šema** (engl. *schema*) je funkcija koja poljima forme dodeljuje pravila validacije. Sledeći kod prikazuje, iz projekta, model i formu stranice za pravljenje ture:

```ts
import { Component, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';

export class CreateTour {
  protected readonly model = signal<CreateTourModel>({
    name: '',
    description: '',
    difficulty: 'Easy',
    tags: '',
  });

  protected readonly form = form(this.model, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });
}
```

U datom kodu treba uočiti sledeće:
- Funkcije `form` i `required` i klasa `FormField` uvoze se iz `@angular/forms/signals`, a ne iz `@angular/forms`. Pogrešna putanja je najčešća prva greška sa formama.
- Model je zasebno polje klase, tipa `CreateTourModel`. Tip `TourDifficulty` iz modula Exploration je unija tri teksta, `'Easy' | 'Moderate' | 'Hard'`. Zato prevodilac prihvata početnu vrednost `'Easy'`, a prijavio bi grešku za npr. `'Lako'`.
- Forma nema sopstvenu kopiju vrednosti. Svaki unos korisnika upisuje se direktno u model, pa `this.model()` uvek vraća ono što je korisnik uneo.
- Šema prima `path`, objekat sa istim svojstvima kao model, kroz koji se pravilo vezuje za jedno polje forme. Poziv `required(path.name, ...)` polju `name` dodaje pravilo da vrednost ne sme da bude prazna. Drugi argument je poruka koju forma čuva uz grešku.
- Pravila se proveravaju pri svakoj promeni modela. Polja bez pravila, `difficulty` i `tags`, uvek su ispravna.

Polje forme nije isto što i polje klase. Polje klase je `form` iz klase `CreateTour`, a polja forme su njegovi delovi, `form.name`, `form.description` i ostali, po jedan za svako svojstvo modela.

Jedno polje forme sme da ima više pravila. Pravilo `min` polju sa brojem dodaje najmanju dozvoljenu vrednost. Sledeći kod prikazuje, iz projekta, formu za dodavanje vremena obilaska ture, čiji model ima svojstvo `minutes` sa početnom vrednošću `30`:

```ts
protected readonly visitModel = signal({ minutes: 30 });

protected readonly visitForm = form(this.visitModel, (path) => {
  required(path.minutes, { message: 'Minutes are required.' });
  min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
});
```

Ostala pravila radnog okvira pišu se na isti način: prvo polje forme, zatim argumenti pravila, ako ih ima, i na kraju poruka.

## Četiri oblika zapisa

Do vrednosti forme možemo doći na nekoliko načina, koje je lako pomešati. Forma i svako polje forme čitaju se pozivom, kao signal, pa se u klasi i šablonu pojavljuju u četiri oblika:

| Zapis | Značenje | Kada se koristi |
|---|---|---|
| `form.name` | Polje forme `name` | Vezivanje za element za unos |
| `form.name()` | Stanje polja `name` | Čitanje vrednosti i grešaka jednog polja |
| `form()` | Stanje cele forme | Provera da li je cela forma ispravna |
| `model()` | Vrednost celog modela | Čitanje svih unetih vrednosti odjednom, npr. pri slanju |

Stanje polja i stanje cele forme imaju signale koje čitamo pozivom. U projektu se koriste četiri:
1. `value()` je vrednost polja, istog tipa kao svojstvo modela.
2. `touched()` je tačno kada je polje dodirnuto, odnosno nakon što je korisnik napustio element za unos vezan za polje i na njemu se desio događaj `blur`.
3. `errors()` je niz grešaka polja. Svaka greška ima svojstvo `message` sa porukom iz šeme. Niz je prazan kada polje nema grešaka.
4. `valid()` je tačno kada polje nema grešaka. Za celu formu, `form().valid()` je tačno kada nijedno polje nema grešaka.

Poziv `this.form.name().value()` u klasi tako vraća uneti naziv, isto kao `this.model().name`.

## Vezivanje elementa za polje

Element za unos vezuje se za polje forme vezivanjem svojstva `[formField]`, a klasa `FormField` navodi se u podešavanju `imports`. Sledeći kod prikazuje, iz projekta, tri elementa za unos sa stranice za pravljenje ture:

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
- Ako `FormField` nije naveden u podešavanju `imports`, vezivanje `[formField]` ne radi, a prevodilac ne prijavljuje grešku. Elementi se prikazuju, ali ništa što korisnik unese ne stiže u model.
- Desna strana vezivanja je polje forme, `form.name`, bez zagrada. Angular upisuje vrednost polja u element, a pri svakom događaju `input` upisuje sadržaj elementa u model. Model je signal, pa sve što ga čita dobija obaveštenje o promeni. Referenca na element i vezivanje događaja iz lekcije o kontroli toka više nisu potrebni.
- Isto vezivanje radi na elementima `input`, `textarea` i `select`. Za `select` u model upisuje `value` izabrane opcije.
- Kada je element `input` tipa `number`, a svojstvo modela broj, Angular u model upisuje broj, a ne tekst. Element za minute iz forme za vreme obilaska, `<input id="minutes" type="number" [formField]="visitForm.minutes" />`, zato kada korisnik unese 45 upisuje broj `45`, pa pravilo `min` poredi brojeve.

## Prikaz grešaka

Polje sa pravilom `required` ima grešku čim se stranica otvori, jer je početna vrednost prazna. Poruka pre prvog unosa nema smisla za korisnika, pa se greške prikazuju tek kada je polje dodirnuto. Sledeći kod prikazuje, iz projekta, element za naziv sa porukama o greškama:

```html
<div class="field">
  <label for="name">Name</label>
  <input id="name" type="text" [formField]="form.name" />
  @if (form.name().touched()) {
    @for (error of form.name().errors(); track $index) {
      <p class="error">{{ error.message }}</p>
    }
  }
</div>
```

U datom kodu treba uočiti sledeće:
- Naredba `@if` čita `touched` iz stanja polja. Polje ostaje dodirnuto i kada se korisnik vrati u element.
- Petlja prolazi kroz niz grešaka i ispisuje poruku svake. Greške nemaju identifikator, pa se koristi `track $index`, kao za poruke o greškama u lekciji o kontroli toka.
- Šablon je čitalac signala `touched` i `errors`. Kada korisnik unese naziv, greška nestaje iz niza i Angular uklanja poruku, bez ikakvog koda u klasi.

## Pravljenje ture

Povežimo pojmove u stranicu za pravljenje ture iz projekta, skraćenu na formu i njenu validaciju:

```ts
import { Component, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';

interface CreateTourModel {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string;
}

@Component({
  selector: 'app-create-tour',
  imports: [FormField],
  templateUrl: './create-tour.html',
  styleUrl: './create-tour.scss',
})
export class CreateTour {
  protected readonly model = signal<CreateTourModel>({
    name: '',
    description: '',
    difficulty: 'Easy',
    tags: '',
  });

  protected readonly form = form(this.model, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });
}
```

```html
<h1>Create tour</h1>

<form>
  <div class="field">
    <label for="name">Name</label>
    <input id="name" type="text" [formField]="form.name" />
    @if (form.name().touched()) {
      @for (error of form.name().errors(); track $index) {
        <p class="error">{{ error.message }}</p>
      }
    }
  </div>

  <div class="field">
    <label for="description">Description</label>
    <textarea id="description" rows="4" [formField]="form.description"></textarea>
    @if (form.description().touched()) {
      @for (error of form.description().errors(); track $index) {
        <p class="error">{{ error.message }}</p>
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

U datom kodu treba uočiti sledeće:
- Polje klase `model` je deklarisano pre polja `form`, jer ga inicijalizator polja `form` koristi, a inicijalizatori se izvršavaju redom.
- Forma za sada nema dugme za slanje. Kada ga u narednoj lekciji dodamo, pritisak na dugme ili na taster Enter pokrenuo bi ugrađeno slanje forme u internet čitaču, koje ponovo učitava stranicu. Kako se to sprečava, pokazuje naredna lekcija.

Kada korisnik otvori stranicu, klikne u element za naziv, pa bez unosa pređe na opis, dešava se sledeće:
1. Pri prvoj proveri šablona Angular upisuje početne vrednosti modela u elemente. Polje `name` već ima grešku iz pravila `required`, ali nije dodirnuto, pa se poruka ne prikazuje.
2. Korisnik napušta element za naziv. Na događaj `blur` Angular upisuje tačno u `touched` polja `name`.
3. Šablon je čitalac tog signala, pa Angular ponovo proverava šablon. Sada se prikazuje blok `@if`, u kom petlja ispisuje poruku „Name is required.“.
4. Korisnik se vraća i unosi slovo. Na događaj `input` Angular upisuje sadržaj elementa u svojstvo `name` modela. Pravilo `required` je zadovoljeno, pa `errors` postaje prazan niz.
5. Angular ponovo proverava šablon. Petlja nema elemenata i poruka nestaje. Kada je unet i opis, `form().valid()` postaje tačno.
