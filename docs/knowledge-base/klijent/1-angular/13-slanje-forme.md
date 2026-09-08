Forma iz prethodne lekcije prikuplja podatke o turi i proverava ih, ali ih ne šalje. Stranica za pravljenje ture mora da pošalje komandu za pravljenje, a nakon uspeha da otvori spisak tura korisnika. Ovde upoznajemo kako se forma šalje, kako se nakon uspeha menja adresa iz klase i kako se forma koja ostaje na ekranu vraća u početno stanje.

## Slanje forme

Čitalac zna da se na elementu `form` pri kliku na dugme tipa `submit` desi događaj `submit` i da pregledač tada učitava nov dokument, osim ako kod koji događaj obrađuje to ne spreči pozivom `preventDefault`. Isto važi i ovde, jer radni okvir nema sopstveni događaj za slanje forme. Sledeći kod prikazuje, iz projekta, metodu za slanje, za sada bez navigacije, i deo šablona koji je poziva:

```ts
private readonly tourAuthoring = inject(TourAuthoring);

protected readonly pending = signal(false);
protected readonly error = signal<string | null>(null);

protected async submit(event: Event): Promise<void> {
  event.preventDefault();
  this.pending.set(true);
  this.error.set(null);
  try {
    await this.tourAuthoring.create({
      name: this.form.name().value(),
      description: this.form.description().value(),
      difficulty: this.form.difficulty().value(),
      tags: this.form.tags().value().split(',').map((tag) => tag.trim()).filter((tag) => tag !== ''),
    });
  } catch (failure) {
    this.error.set(serverMessage(failure, 'Could not create the tour.'));
  } finally {
    this.pending.set(false);
  }
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
- Vezivanje događaja `(submit)` stoji na elementu `form`, a ne na dugmetu. Metoda prima objekat događaja i prvo poziva `preventDefault`, da pregledač ne bi učitao nov dokument i time pokrenuo radni okvir ispočetka. Naziv metode ne mora da bude isti kao naziv događaja.
- Ostatak metode je obrazac stranice sa komandom iz lekcije o komandama, bez izmene. Komanda `create` servisa `TourAuthoring` prima DTO strukturu `CreateTourDto`, sa istim svojstvima kao model, osim što je `tags` niz, i vraća napravljenu turu.
- Vrednosti za DTO strukturu se čitaju iz stanja polja, pozivom `value()`. Tekst sa oznakama se deli na zarezu, a prazni delovi odbacuju.
- Dugme je onemogućeno dok forma nije ispravna i dok komanda traje. Server podatke proverava ponovo i grešku prijavljuje kao i za svaku komandu.

## Navigacija nakon uspeha

Nakon što server napravi turu, korisnik očekuje da vidi spisak svojih tura. Do sada je adresu menjala samo veza, na klik korisnika. Klasa adresu menja kroz klasu radnog okvira `Router`, koju poziv `provideRouter` iz konfiguracije aplikacije prijavljuje kontejneru, pa je klasa preuzima kao i servis. Sledeći kod prikazuje deo metode za slanje sa navigacijom:

```ts
private readonly router = inject(Router);

try {
  await this.tourAuthoring.create({ ... });
  await this.router.navigate(['/exploration/mine']);
} catch (failure) {
  this.error.set(serverMessage(failure, 'Could not create the tour.'));
} finally {
  this.pending.set(false);
}
```

U datom kodu treba uočiti sledeće:
- Metoda `navigate` prima niz istog oblika kao veza sa parametrom. Radni okvir od niza sastavlja adresu i dalje radi isto kao pri kliku na vezu.
- Poziv stoji iza `await` komande, pa se izvršava samo kada komanda uspe. Kada komanda ne uspe, korisnik ostaje na formi sa porukom o grešci.
- Metoda `navigate` vraća obećanje, jer navigacija uključuje pravljenje komponente nove rute, pa je čekamo sa `await`.
- Radni okvir pri navigaciji uništava komponentu stranice za pravljenje, a sa njom i formu. Formu zato ne treba vraćati u početno stanje.

## Vraćanje forme u početno stanje

Forma koja nakon slanja ostaje na ekranu mora da se vrati u početno stanje, da bi korisnik uneo sledeću vrednost. Sledeći kod prikazuje, iz projekta, formu za nov komentar iz komponente `BlogComments`, deteta stranice bloga `BlogDetail`, koja komentar javlja roditelju kroz izlaz:

```ts
readonly addComment = output<string>();

protected readonly form = form(signal({ text: '' }), (path) => {
  required(path.text, { message: 'A comment cannot be empty.' });
});

protected submitComment(event: Event): void {
  event.preventDefault();
  this.addComment.emit(this.form.text().value());
  this.form().reset({ text: '' });
}
```

```html
<form (submit)="submitComment($event)">
  <input type="text" [formField]="form.text" />
  <button type="submit" [disabled]="!form().valid()">Comment</button>
</form>
```

U datom kodu treba uočiti sledeće:
- Metoda `reset` iz stanja cele forme upisuje primljenu vrednost u model i svako polje označava kao nedodirnuto. Element za unos postaje prazan, polje ponovo ima grešku iz pravila `required`, ali se poruka ne prikazuje, a dugme je onemogućeno do sledećeg unosa.
- Komponenta `BlogComments` ne šalje komandu, već kroz izlaz javlja roditelju, koji komandu šalje i ponovo učitava svoj resurs. Metoda zato nije asinhrona i nema `pending`.

## Pravljenje ture

Povežimo pojmove u celu stranicu za pravljenje ture iz projekta:

```ts
@Component({
  imports: [FormField],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
})
export class CreateTour {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ name: '', description: '', difficulty: 'Easy' as Difficulty, tags: '' }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        difficulty: this.form.difficulty().value(),
        tags: this.form
          .tags()
          .value()
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
      });
      await this.router.navigate(['/exploration/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the tour.'));
    } finally {
      this.pending.set(false);
    }
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

  @if (error()) {
    <p class="error">{{ error() }}</p>
  }

  <button type="submit" [disabled]="!form().valid() || pending()">Create</button>
</form>
```

Kada korisnik unese naziv i opis i klikne na dugme za pravljenje, dešava se sledeće:
1. Na elementu `form` se desi događaj `submit`, a vezivanje događaja poziva metodu `submit` sa objektom događaja. Metoda poziva `preventDefault` i upisuje tačno u `pending`.
2. Metoda iz stanja polja čita vrednosti, sastavlja DTO strukturu i poziva komandu `create`. Servis šalje zahtev na `/api/exploration/tours`, a `await` čeka odgovor.
3. Server pravi turu i vraća je u odgovoru. Metoda odgovor ne koristi, već poziva `navigate` sa adresom spiska tura korisnika.
4. Radni okvir upisuje adresu `/exploration/mine` u pregledač, bira rutu, uništava komponentu `CreateTour` i na mestu iscrtavanja pravi komponentu `MyTours`.
5. Resurs stranice `MyTours` šalje zahtev pri prvom iscrtavanju i u odgovoru dobija i novu turu, pa tabela prikazuje i nju.
