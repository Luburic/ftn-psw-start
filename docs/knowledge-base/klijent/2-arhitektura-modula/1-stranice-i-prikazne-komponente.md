Prethodni segment je za svaku stranicu projekta pisao jednu komponentu. Posmatrajmo šta se dešava kada stranica raste. Stranica bloga prikazuje blog, ispod njega spisak komentara, uz svaki komentar prijavljenog korisnika dugmad za izmenu i brisanje, a na dnu formu za nov komentar. Sledeći kod prikazuje članove klase koja bi sve to radila sama, gde su tela metoda izostavljena:

```ts
export class BlogDetail {
  private readonly http = inject(HttpClient);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);
  protected readonly error = signal<string | null>(null);
  protected readonly form = form(signal({ text: '' }), (path) => { ... });

  protected async add(event: Event): Promise<void> { ... }
  protected async edit(commentId: string, text: string): Promise<void> { ... }
  protected async remove(commentId: string): Promise<void> { ... }
}
```

U datom kodu treba uočiti sledeće:

- Klasa zna adresu servera, čeka odgovor na komandu, drži formu i u šablonu iscrtava spisak komentara. Menja se kada server promeni adresu, kada se promeni izgled komentara i kada se promeni pravilo validacije, a nijedna od te tri promene nema veze sa drugom.
- Spisak komentara sa svojom dugmadi i formom ne može da se prikaže ni na jednom drugom mestu, jer je zapisan u šablonu ove klase, zajedno sa naslovom i opisom bloga.
- Klasa se ne može proveriti bez servera, jer sve što radi počinje resursom i završava se komandom.

Ovo je isti miris koji na serveru ima kontroler koji sam učitava podatke, proverava pravila i upisuje izmene. Na klijentu je odgovor jednostavniji od slojeva, jer nema domenskih pravila koja treba izolovati. Komponente delimo na dve vrste i razdvajamo ono što razgovara sa serverom od onoga što samo prikazuje.

## Stranica i prikazna komponenta

**Stranica** (engl. *page*) je komponenta koju tabela ruta imenuje, koja preuzima servise i čije metode šablon poziva na akcije korisnika. **Prikazna komponenta** (engl. *presentational component*) je komponenta koja podatke prima kroz ulaze, akcije korisnika prijavljuje kroz izlaze i ne preuzima ništa. Stranica zna odakle podaci dolaze i kome se komanda šalje. Prikazna komponenta ne zna ni jedno ni drugo, pa se može koristiti na svakom mestu koje joj popuni ulaze.

Podela ne zabranjuje prikaznoj komponenti da ima stanje. Forma za nov komentar, izabrani red tabele ili otvoren panel su stanje prikaza, koje ne nadživljava komponentu i ne zanima nikoga van nje. Prikazna komponenta takvo stanje drži u signalu, kao i svaka druga. Granica je da ne preuzima servis i ne zna adresu servera.

Podela nije vidljiva u nazivu datoteke ni u nazivu direktorijuma. Vidi se u kodu: tabela ruta modula imenuje svaku stranicu, stranica preuzima servis, a prikazna komponenta ne preuzima ništa. Sledeća tabela sažima razliku:

| | Stranica | Prikazna komponenta |
|---|---|---|
| Ko je pravi | Radni okvir, kada se adresa poklopi sa rutom | Šablon roditelja, kroz selektor |
| Odakle podaci | Resurs koji sama deklariše | Ulazi koje roditelj popunjava |
| Kuda akcije korisnika | Metoda klase, koja poziva servis | Izlaz, na koji roditelj vezuje izraz |
| Šta preuzima | Servise koje koristi | Ništa |
| Stanje | Resurs, greška komande, filter | Stanje prikaza, poput forme |

## Grupa slučajeva korišćenja

Aplikacioni sloj servera svoje klase deli po grupama slučajeva korišćenja. Klijent tu podelu preuzima kao raspored direktorijuma. **Grupa slučajeva korišćenja** na klijentu je skup stranica koje služe jednom cilju korisnika i nosi ime istoimene grupe aplikacionog sloja servera. Modul je ravna lista grupa. Grupa je ravna lista direktorijuma komponenti i najviše jedna datoteka servisa. Sledeće stablo prikazuje modul Social iz projekta:

```
modules/social/
  api/
  blog-authoring/
    create-blog/
    my-blogs/
    blog-authoring.ts
  blog-reading/
    blog-card/
    blog-comments/
    blog-detail/
    blog-list/
    blog-reading.ts
  public-api.ts
  social.routes.ts
```

U datom stablu treba uočiti sledeće:

- Grupa `blog-authoring` sadrži dve stranice i servis. Grupa `blog-reading` sadrži dve stranice, dve prikazne komponente i servis. Koja je komponenta stranica saznaje se iz tabele ruta `social.routes.ts`, koja imenuje `BlogList`, `MyBlogs`, `CreateBlog` i `BlogDetail`.
- Svaka komponenta ima sopstveni direktorijum sa tri datoteke, tačno kako ih pravi naredba `ng generate component`. Prikazna komponenta stoji pored stranica koje je koriste, a ne u zasebnom direktorijumu.
- Ne postoje direktorijumi `pages`, `components` ni `services`. Zvanični vodič za stil Angular-a propisuje da se kod grupiše po sposobnosti, a ne po vrsti datoteke. Raspored koji usvajamo je stroži od uobičajenog, jer takve direktorijume ne dozvoljava ni unutar grupe.
- Direktorijum `api` i datoteka `public-api.ts` ne pripadaju nijednoj grupi. U ovoj lekciji ih ne razmatramo.

## Mesto nove komponente

Kada pišemo novu komponentu, mesto joj određujemo odgovorom na tri pitanja, ovim redom:

1. Da li je imenuje tabela ruta ili je koristi šablon druge komponente? U prvom slučaju je stranica, u drugom prikazna komponenta.
2. Kom cilju korisnika služi ekran na kom se pojavljuje? To je njena grupa. Ako grupa ne postoji, pravimo je i dajemo joj ime grupe aplikacionog sloja servera koju ekran poziva.
3. Ako je prikazna komponenta, da li se pojavljuje samo na jednom ekranu? Ako da, živi u grupi tog ekrana, pored stranice koja je koristi.

Treće pitanje objašnjava zašto klijent ima manje grupa od servera. Server ima grupu za komentarisanje, jer su dodavanje, izmena i brisanje komentara njegovi slučajevi korišćenja. Klijent nema ekran za komentarisanje, jer se komentari pišu na stranici bloga. Zato `BlogComments` živi u grupi `blog-reading`, a servis te grupe nosi komande za komentare. Klijent grupiše ekrane, a server operacije.

## Stranica bloga i komentari

Povežimo pojmove čitanjem koda iz projekta koji je nastao podelom klase sa početka lekcije. Sledeći kod prikazuje stranicu `BlogDetail`, gde su tela metoda `edit` i `remove` izostavljena jer prate oblik metode `add`:

```ts
@Component({
  imports: [BlogComments, RouterLink],
  selector: 'app-blog-detail',
  styleUrl: './blog-detail.scss',
  templateUrl: './blog-detail.html',
})
export class BlogDetail {
  private readonly blogReading = inject(BlogReading);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);

  protected readonly error = signal<string | null>(null);

  protected async add(text: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.addComment(this.id(), text);
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> { ... }

  protected async remove(commentId: string): Promise<void> { ... }
}
```

Sledeći kod prikazuje deo šablona stranice u kom ona koristi prikaznu komponentu:

```html
@if (detail.value(); as blog) {
  <h1>{{ blog.title }}</h1>
  <p>{{ blog.description }}</p>

  <app-blog-comments
    [comments]="blog.comments"
    [currentUserId]="auth.user()?.id ?? null"
    (addComment)="add($event)"
    (editComment)="edit($event)"
    (deleteComment)="remove($event)"
  />
}
```

Sledeći kod prikazuje prikaznu komponentu `BlogComments`:

```ts
export interface CommentEdit {
  commentId: string;
  text: string;
}

@Component({
  imports: [FormField],
  selector: 'app-blog-comments',
  styleUrl: './blog-comments.scss',
  templateUrl: './blog-comments.html',
})
export class BlogComments {
  readonly comments = input.required<CommentDto[]>();
  readonly currentUserId = input<string | null>(null);

  readonly addComment = output<string>();
  readonly editComment = output<CommentEdit>();
  readonly deleteComment = output<string>();

  protected readonly form = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A comment cannot be empty.' });
  });

  protected submitComment(event: Event): void {
    event.preventDefault();
    this.addComment.emit(this.form.text().value());
    this.form().reset({ text: '' });
  }
}
```

U datom kodu treba uočiti sledeće:

- Tabela ruta `social.routes.ts` imenuje `BlogDetail` uz deo adrese `:id`, a `BlogComments` ne imenuje. Radni okvir pravi stranicu kada se adresa poklopi, a prikaznu komponentu pravi šablon stranice.
- Stranica preuzima servis `BlogReading`, koji nosi komande za komentare, i servis `Auth`, iz kog čita prijavljenog korisnika. Prikazna komponenta ne preuzima ništa. Ko je prijavljen saznaje kroz ulaz `currentUserId`, koji stranica popunjava iz servisa `Auth`.
- Spisak komentara stiže kroz ulaz `comments` iz vrednosti resursa `detail`. Prikazna komponenta ne zna da resurs postoji ni sa koje adrese je spisak stigao.
- Svaka akcija korisnika nad komentarom je izlaz. Prikazna komponenta prijavljuje tekst novog komentara, identifikator i nov tekst izmenjenog ili identifikator obrisanog, a šta se sa tim dešava odlučuje stranica, koja poziva servis i osvežava resurs.
- Forma za nov komentar živi u prikaznoj komponenti. To je stanje prikaza, koje se prazni čim je tekst prijavljen kroz izlaz, pa stranica za formu ne zna.
- Tip `CommentEdit` postoji samo da bi izlaz `editComment` mogao da nosi dve vrednosti odjednom, pa je izvezen iz datoteke komponente koja ga prijavljuje.

Klasa sa početka lekcije je podeljena na stranicu od tri komande i prikaznu komponentu od tri izlaza. Spisak komentara se sada može iscrtati na svakom mestu koje mu preda niz komentara, a stranica se čita kao spisak toga šta se dešava na svaku akciju korisnika.
