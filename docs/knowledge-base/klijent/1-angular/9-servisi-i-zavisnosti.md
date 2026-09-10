# Servisi i zavisnosti

Zaglavlje korenske komponente prikazuje adresu e-pošte prijavljenog korisnika, a spisak blogova prikazuje vezu za pravljenje bloga samo prijavljenom korisniku. Obe komponente čitaju istog korisnika. Nijedna ne može da ga drži kao svoj signal, jer bi odjava u zaglavlju ostavila spisak blogova sa starim korisnikom, a signal u komponenti rute nestaje kada radni okvir komponentu uništi. Podatak koji nadživljava stranice mora da živi u objektu koji je u svakom momentu dostupan komponentama. Za te potrebe najčešće koristimo servis.

## Servis

**Servis** (engl. *service*) je klasa označena dekoratorom `@Injectable`, čiji jedan objekat kontejner zavisnosti pravi i daje svakom ko ga zatraži. Sledeći kod prikazuje servis koji čuva prijavljenog korisnika:

```ts
export interface User {
  id: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class Auth {
  private readonly token = signal<string | null>(null);

  readonly user = computed(() => decodeUser(this.token()));
  readonly isLoggedIn = computed(() => this.user() !== null);

  logout(): void {
    this.token.set(null);
  }
}
```

U datom kodu treba uočiti sledeće:
- Podešavanje `providedIn: 'root'` znači da za celu aplikaciju postoji jedan objekat ove klase. Kontejner ga pravi kada ga prvi put neko zatraži, a svakom sledećem daje isti objekat.
- Token je string koji server izdaje pri prijavi i po kom u svakom narednom zahtevu prepoznaje korisnika. Metoda `login`, koju smo izostavili, dobija token od servera i upisuje ga u signal `token`. Funkcija `decodeUser` iz istog fajla iz teksta tokena čita objekat tipa `User`, a za `null` vraća `null`.
- Signal `token` je `private`, pa ga menjaju samo metode servisa.
- Servis nema selektor ni šablon, već drži podatak i metode nad njim.

## Injektovanje

**Injektovanje** (engl. *injection*) je poziv `inject(Klasa)`, kojim druga klasa dobija objekat navedene klase umesto da ga pravi sa `new`. Sledeći kod prikazuje, iz projekta, stranicu sa spiskom blogova koja preuzima servis `Auth` i deo šablona koji ga čita:

```ts
@Component({
  imports: [RouterLink],
  selector: 'app-blog-list',
  styleUrl: './blog-list.scss',
  templateUrl: './blog-list.html',
})
export class BlogList {
  protected readonly auth = inject(Auth);
}
```

```html
@if (auth.isLoggedIn()) {
  <a routerLink="/social/create">Create blog</a>
}
```

U datom kodu treba uočiti sledeće:
- Poziv `inject(Auth)` stoji u inicijalizatoru polja, odnosno u vrednosti koju polje dobija pri deklaraciji.
- Šablon čita signal servisa kroz polje, `auth.isLoggedIn()`, i postaje njegov pretplatnik.

## Servis koji drži podatke

Servis ne mora da čuva stanje aplikacije kao `Auth`. Može da drži i domenske podatke koje više komponenata čita. Spisak blogova prikazuje sve blogove, a stranica bloga prikazuje jedan, po `id`-u iz adrese. Oba čitaju iste blogove, pa oni žive u jednom servisu:

```ts
export interface Blog {
  id: string;
  title: string;
  author: string;
  body: string;
}

@Injectable({ providedIn: 'root' })
export class BlogService {
  private readonly blogs = signal<Blog[]>([
    { id: '1', title: 'Prvi blog', author: 'Ana', body: '...' },
    { id: '2', title: 'Drugi blog', author: 'Marko', body: '...' },
  ]);

  readonly all = this.blogs.asReadonly();

  byId(id: string): Blog | undefined {
    return this.blogs().find((blog) => blog.id === id);
  }
}
```

Stranica bloga preuzima servis i od `id`-a iz rute računa blog:

```ts
export class BlogDetail {
  readonly id = input.required<string>();

  private readonly service = inject(BlogService);

  protected readonly blog = computed(() => this.service.byId(this.id()));
}
```

```html
@if (blog(); as blog) {
  <article>
    <h1>{{ blog.title }}</h1>
    <p>{{ blog.body }}</p>
  </article>
} @else {
  <p>Blog nije pronađen.</p>
}
```

U datom kodu treba uočiti sledeće:
- Blogovi su u primeru iznad zakucanu u kodu. U narednoj lekciji servis iste blogove dobavlja sa servera, a komponente koje kontaktiraju servis ostaju nepromenjene.
- `BlogList` i `BlogDetail` preuzimaju isti objekat servisa, kao što `App` i `BlogList` dele isti `Auth`. Kontejner obema daje isti objekat, pa čitaju iste blogove.
- `blog` je izvedeni signal koji čita `id()`. Pošto je `id` signal, prelaskom sa `/social/1` na `/social/2` menja se `id`, `blog` se ponovo računa i šablon prikazuje drugi blog, bez pravljenja nove komponente. Time se ostvaruje ono što je stranica bloga u prethodnom poglavlju najavila.

## Zaglavlje sa korisnikom

Povežimo pojmove u korensku komponentu projekta, koja u zaglavlju prikazuje prijavljenog korisnika ili veze za prijavu:

```ts
@Component({
  imports: [RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);
}
```

```html
<header>
  <nav>
    <a routerLink="/">Home</a>
    <a routerLink="/social">Social</a>

    @if (auth.user(); as user) {
      <span>{{ user.email }}</span>
      <button type="button" (click)="auth.logout()">Logout</button>
    } @else {
      <a routerLink="/login">Login</a>
      <a routerLink="/register">Register</a>
    }
  </nav>
</header>

<main>
  <router-outlet />
</main>
```

Kada prijavljeni korisnik na spisku blogova klikne na dugme za odjavu, dešava se sledeće:
1. Vezivanje događaja poziva `auth.logout()` na objektu servisa koji je korenska komponenta preuzela.
2. Metoda `logout` upisuje `null` u signal `token`, koji obaveštava pretplatnike.
3. Izvedeni signal `user` je pretplatnik signala `token`, a `isLoggedIn` je pretplatnik signala `user`, pa se oba označavaju za ponovno računanje.
4. Šablon zaglavlja je pretplatnik signala `user`, jer ga čita u naredbi `@if`, pa radni okvir ponovo iscrtava korensku komponentu. Pri iscrtavanju `user` računa novu vrednost, `null`, i prikazuje se grana `@else`.
5. Šablon spiska blogova je pretplatnik signala `isLoggedIn`, jer je preuzeo isti objekat servisa, pa radni okvir ponovo iscrtava i njega i uklanja vezu za pravljenje bloga.
