Zaglavlje korenske komponente prikazuje adresu e-pošte prijavljenog korisnika, a spisak blogova prikazuje vezu za pravljenje bloga samo prijavljenom korisniku. Obe komponente čitaju istog korisnika. Nijedna ne može da ga drži kao svoj signal, jer bi odjava u zaglavlju ostavila spisak blogova sa starim korisnikom, a signal u komponenti rute nestaje kada radni okvir komponentu uništi. Podatak koji nadživljava stranice mora da živi u objektu koji postoji jednom i koji svaka komponenta može da dobije. Na serveru je takve objekte pravio i delio kontejner zavisnosti. Angular ima kontejner zavisnosti sa istom ulogom, kao deo radnog okvira koji pravi komponente i objekte koje one traže. Ovde upoznajemo kako se klasa prijavljuje kontejneru i kako komponenta od njega dobija objekat.

## Servis

**Servis** (engl. *service*) je klasa označena dekoratorom `@Injectable`, čiji jedan objekat kontejner zavisnosti pravi i daje svakom ko ga zatraži. Sledeći kod prikazuje, iz projekta, servis koji čuva prijavljenog korisnika, skraćen na članove koje komponente čitaju:

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
- Token je tekst koji server izdaje pri prijavi i po kom u svakom narednom zahtevu prepoznaje korisnika. Metoda `login`, koju smo izostavili, dobija token od servera i upisuje ga u signal `token`. Funkcija `decodeUser` iz istog fajla iz teksta tokena čita objekat tipa `User`, a za `null` vraća `null`.
- Signal `token` je `private`, pa ga menjaju samo metode servisa. Signal je zato što izvedeni signali `user` i `isLoggedIn` zavise od njega i moraju da saznaju kada se promeni. Ta dva polja su bez modifikatora pristupa, pa ih komponente čitaju.
- Servis nema selektor ni šablon, već drži podatak i metode nad njim.

Servis `Auth` živi u direktorijumu `core`, jer ga čita svaki modul.

## Preuzimanje zavisnosti

**Preuzimanje zavisnosti** (engl. *injection*) je poziv `inject(Klasa)`, kojim klasa od kontejnera dobija objekat navedene klase umesto da ga pravi sa `new`. Sledeći kod prikazuje, iz projekta, stranicu sa spiskom blogova koja preuzima servis `Auth` i deo šablona koji ga čita:

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
- Poziv `inject(Auth)` stoji u inicijalizatoru polja, odnosno u vrednosti koju polje dobija pri deklaraciji. Izvršava se dok kontejner pravi komponentu, pa kontejner zna ko ga zove. Poziv sme da stoji samo tu ili u konstruktoru, koji projekat ne koristi. Poziv iz metode klase, koja se izvršava kasnije, radni okvir prijavljuje kao grešku pri izvršavanju, jer tada nema ko da ga obradi.
- Šablon čita signal servisa kroz polje, `auth.isLoggedIn()`, i postaje njegov pretplatnik kao za svaki drugi signal.
- Servis takođe preuzima zavisnosti. Neskraćeni servis `Auth` počinje poljem `private readonly http = inject(HttpClient)`, gde je `HttpClient` klasa radnog okvira za razmenu podataka sa serverom, kojom metoda `login` šalje zahtev za prijavu. Kontejner pravi servis na isti način kao komponentu, pa polje sa `inject` sme da stoji i u njemu.

Čitalac koji poznaje kontejner zavisnosti sa servera prepoznaje isti postupak. Razlika je u tome što se zavisnost ne navodi kao parametar konstruktora, već kao polje sa pozivom `inject`, i što se klasa ne prijavljuje u konfiguraciji aplikacije, već dekoratorom na samoj klasi.

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
