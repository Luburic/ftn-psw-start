Forma iz prethodne lekcije prikuplja podatke o turi i proverava ih, ali ih ne šalje. Stranica za pravljenje ture mora da pošalje komandu za pravljenje, a nakon uspeha da otvori spisak tura korisnika. Ovde upoznajemo kako se forma šalje, kako klasa nakon uspeha menja adresu i kako se forma koja ostaje na ekranu vraća u početno stanje.

## Komanda za pravljenje ture

Stranica šalje komandu kroz servis `TourAuthoring`, koji ima isti oblik kao servis `BlogAuthoring` iz lekcije o komandama. Metoda `create` prima DTO strukturu sa podacima nove ture:

```ts
export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
}
```

```ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);

  create(dto: CreateTourDto): Observable<TourDto> {
    return this.http.post<TourDto>(BASE_URL, dto);
  }

  publish(id: string): Observable<void> {
    return this.http.post<void>(`${BASE_URL}/${id}/publish`, {});
  }
}
```

U datom kodu treba uočiti sledeće:
- Metoda `create` šalje DTO strukturu na adresu `/api/exploration/tours` i vraća `Observable` sa napravljenom turom. Kao i svaka komanda iz lekcije o komandama, zahtev se šalje tek kada stranica pozove `subscribe`.
- Metoda `publish` je ista ona koju stranica `MyTours` koristi u lekciji o komandama. Obe komande za ture stoje u istom servisu, a čitanje tura u servisu `TourQueries`.
- Interfejs `CreateTourDto` ima ista svojstva kao model forme iz prethodne lekcije, osim svojstva `tags`. U modelu su oznake jedan tekst, onako kako ih korisnik kuca u element za unos, npr. „priroda, šetnja“. Server ih očekuje kao niz tekstova. Zato stranica pre slanja mora da pretvori model u DTO strukturu.

## Slanje forme

Kada korisnik klikne na dugme tipa `submit` unutar elementa `form`, ili pritisne taster Enter u elementu za unos, na elementu `form` se desi događaj `submit`. Internet čitač tada podrazumevano šalje formu i učitava nov dokument. To bi ponovo učitalo celu aplikaciju i izbrisalo sve što je korisnik uneo. Zato kod koji obrađuje događaj mora da pozove `preventDefault`, kao i u običnom JavaScript-u.

Sledeći kod prikazuje, iz projekta, metodu za slanje, za sada bez prelaska na drugu stranicu, i deo šablona koji je poziva:

```ts
private readonly tourAuthoring = inject(TourAuthoring);

protected readonly pending = signal(false);
protected readonly error = signal<string | null>(null);

protected submit(event: Event): void {
  event.preventDefault();
  this.pending.set(true);
  this.error.set(null);
  const value = this.model();
  this.tourAuthoring
    .create({
      name: value.name,
      description: value.description,
      difficulty: value.difficulty,
      tags: value.tags.split(',').map((tag) => tag.trim()).filter((tag) => tag !== ''),
    })
    .subscribe({
      next: () => {
        this.pending.set(false);
      },
      error: (failure) => {
        this.pending.set(false);
        this.error.set(serverMessage(failure, 'Could not create the tour.'));
      },
    });
}
```

```html
<form (submit)="submit($event)">
  @if (error()) {
    <p class="error">{{ error() }}</p>
  }

  <button type="submit" [disabled]="!form().valid() || pending()">Create</button>
</form>
```

U datom kodu treba uočiti sledeće:
- Vezivanje događaja `(submit)` stoji na elementu `form`, a ne na dugmetu. Tako se metoda poziva i na klik i na taster Enter. Metoda prima objekat događaja i prvo poziva `preventDefault`. Naziv metode ne mora da bude isti kao naziv događaja.
- Sve unete vrednosti čitamo odjednom, pozivom `this.model()`, kao što je najavila tabela iz prethodne lekcije. Konstanta `value` drži vrednost modela u trenutku slanja.
- Naziv, opis i težina prelaze iz modela u DTO strukturu bez izmene. Tekst sa oznakama delimo na zarezima (`split`), sa svakog dela uklanjamo razmake sa krajeva (`trim`) i odbacujemo prazne delove (`filter`). Unos „priroda, šetnja,“ tako postaje niz `['priroda', 'šetnja']`.
- Ostatak metode je obrazac stranice sa komandom iz lekcije o komandama: `pending` postaje tačan pre slanja, `subscribe` šalje zahtev, a funkcije `next` i `error` vraćaju `pending` na netačno.
- Dugme je onemogućeno dok forma nije ispravna i dok komanda traje. Server podatke proverava ponovo, jer se klijentu ne sme verovati, i grešku prijavljuje kao i za svaku komandu.

Forme sa signalima imaju i sopstvene pomoćne alate za slanje, koje ćete sretati u dokumentaciji. U projektu formu šaljemo ručno, na način koji već poznajemo iz lekcije o komandama.

## Navigacija nakon uspeha

Nakon što server napravi turu, korisnik očekuje da vidi spisak svojih tura. Do sada je adresu menjala samo veza, na klik korisnika. Iz klase adresu menjamo kroz klasu `Router` iz paketa `@angular/router`. Poziv `provideRouter` iz konfiguracije aplikacije prijavljuje je kontejneru zavisnosti, pa je klasa preuzima pozivom `inject`, kao i svaki servis. Sledeći kod prikazuje metodu za slanje sa navigacijom:

```ts
private readonly router = inject(Router);

protected submit(event: Event): void {
  // ...
  this.tourAuthoring
    .create({ ... })
    .subscribe({
      next: () => {
        this.pending.set(false);
        this.router.navigate(['/exploration/mine']);
      },
      error: (failure) => {
        this.pending.set(false);
        this.error.set(serverMessage(failure, 'Could not create the tour.'));
      },
    });
}
```

U datom kodu treba uočiti sledeće:
- Metoda `navigate` prima niz istog oblika kao veza sa parametrom iz lekcije o ruteru. Angular od niza sastavlja adresu i dalje radi isto kao pri kliku na vezu.
- Poziv stoji u funkciji `next`, pa se izvršava samo kada server potvrdi komandu. Kada komanda ne uspe, izvršava se funkcija `error` i korisnik ostaje na formi sa porukom o grešci.
- Pri navigaciji Angular uništava komponentu stranice za pravljenje, a sa njom i formu. Formu zato ne treba vraćati u početno stanje.

## Vraćanje forme u početno stanje

Forma koja nakon slanja ostaje na ekranu mora da se vrati u početno stanje, da bi korisnik mogao da unese sledeću vrednost. Sledeći kod prikazuje, iz projekta, formu za nov komentar iz komponente `BlogComments`. Komponenta je dete stranice bloga `BlogDetail` i novi komentar javlja roditelju kroz izlaz:

```ts
readonly addComment = output<string>();

protected readonly model = signal({ text: '' });

protected readonly form = form(this.model, (path) => {
  required(path.text, { message: 'A comment cannot be empty.' });
});

protected submitComment(event: Event): void {
  event.preventDefault();
  this.addComment.emit(this.model().text);
  this.model.set({ text: '' });
  this.form().reset();
}
```

```html
<form (submit)="submitComment($event)">
  <input type="text" [formField]="form.text" />
  @if (form.text().touched()) {
    @for (error of form.text().errors(); track $index) {
      <p class="error">{{ error.message }}</p>
    }
  }
  <button type="submit" [disabled]="!form().valid()">Comment</button>
</form>
```

U datom kodu treba uočiti sledeće:
- Poziv `this.model.set({ text: '' })` upisuje praznu vrednost u model. Forma nema sopstvenu kopiju vrednosti, pa element za unos odmah postaje prazan.
- Metoda `reset` iz stanja cele forme ne menja vrednost, već svako polje označava kao nedodirnuto. Bez nje bi polje ostalo dodirnuto, pa bi se odmah prikazala poruka „A comment cannot be empty.“, iako korisnik još nije ni počeo da piše sledeći komentar.
- Nakon oba poziva polje ponovo ima grešku iz pravila `required`, ali se poruka ne prikazuje, a dugme je onemogućeno do sledećeg unosa. Forma je u istom stanju kao kada se stranica prvi put otvori.
- Komponenta `BlogComments` ne šalje komandu, već kroz izlaz javlja roditelju, koji komandu šalje i ponovo učitava svoj resurs, po pravilu „podaci naniže, događaji naviše“. Metoda zato nema `pending` ni `subscribe`.

## Pravljenje ture

Povežimo pojmove u celu stranicu za pravljenje ture iz projekta:

```ts
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
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
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(event: Event): void {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    const value = this.model();
    this.tourAuthoring
      .create({
        name: value.name,
        description: value.description,
        difficulty: value.difficulty,
        tags: value.tags.split(',').map((tag) => tag.trim()).filter((tag) => tag !== ''),
      })
      .subscribe({
        next: () => {
          this.pending.set(false);
          this.router.navigate(['/exploration/mine']);
        },
        error: (failure) => {
          this.pending.set(false);
          this.error.set(serverMessage(failure, 'Could not create the tour.'));
        },
      });
  }
}
```

```html
<h1>Create tour</h1>

<form (submit)="submit($event)">
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

  @if (error()) {
    <p class="error">{{ error() }}</p>
  }

  <button type="submit" [disabled]="!form().valid() || pending()">Create</button>
</form>
```

Kada korisnik unese naziv i opis i klikne na dugme za pravljenje, dešava se sledeće:
1. Na elementu `form` se desi događaj `submit`, a vezivanje događaja poziva metodu `submit` sa objektom događaja. Metoda poziva `preventDefault`, pa internet čitač ne učitava nov dokument, i upisuje tačno u `pending`, pa se dugme onemogućava.
2. Metoda pozivom `this.model()` čita sve unete vrednosti, pretvara ih u DTO strukturu i poziva komandu `create`. Servis vraća opisan zahtev, a poziv `subscribe` ga šalje na `/api/exploration/tours`.
3. Server pravi turu i vraća je u odgovoru. Izvršava se funkcija `next`, koja vraća `pending` na netačno i poziva `navigate` sa adresom spiska tura korisnika. Odgovor servera ovde ne koristimo.
4. Angular upisuje adresu `/exploration/mine` u internet čitač, bira rutu, uništava komponentu `CreateTour`, a sa njom i formu, i na mestu iscrtavanja pravi komponentu `MyTours`.
5. Stranica `MyTours` od servisa `TourQueries` dobija resurs, koji šalje nov zahtev za spisak tura korisnika. Odgovor sada sadrži i novu turu, pa je tabela prikazuje.

Kada server odbije komandu, npr. zato što tura sa istim nazivom već postoji, u trećem koraku izvršava se funkcija `error` umesto `next`. Ona upisuje poruku servera u signal `error`, a korisnik ostaje na stranici za pravljenje sa svim unetim vrednostima i može da ih ispravi i pošalje ponovo.
