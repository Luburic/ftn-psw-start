Zaglavlje korenske komponente prikazuje adresu e-pošte prijavljenog korisnika. Spisak blogova prikazuje vezu „Create blog“, ali samo prijavljenom korisniku. Obe komponente, dakle, moraju da znaju ko je prijavljen.

Prvo što nam pada na pamet je da svaka komponenta drži korisnika u svom signalu. Tada bi postojale dve odvojene kopije istog podatka. Kada korisnik klikne na „Logout“ u zaglavlju, zaglavlje briše svoju kopiju, ali spisak blogova i dalje drži svoju i nastavlja da prikazuje vezu za pravljenje bloga. Dve kopije jednog podatka pre ili kasnije se razilaze.

Druga ideja je da korisnika drži samo jedna komponenta, npr. spisak blogova, a da ga ostale čitaju od nje. Ni to ne radi. Spisak blogova je komponenta rute, pa je Angular uništava čim korisnik pređe na drugu stranicu, a sa njom nestaje i signal. Pored toga, zaglavlje nije ni roditelj ni dete spiska blogova, pa ne može da čita njegove signale ni kroz ulaze ni kroz izlaze.

Korisniku je zato potrebno mesto koje ne pripada nijednoj komponenti, koje postoji sve vreme dok aplikacija radi i kojem svaka komponenta može da pristupi. Podatak tada postoji u jednom primerku, pa ga sve komponente vide isto, a odjava na jednom mestu odmah važi svuda. Takvo mesto u Angularu je **servis**.

## Servis

**Servis** (engl. *service*) je klasa označena dekoratorom `@Injectable`, čiji objekat pravi kontejner zavisnosti i daje ga svakom ko ga zatraži. **Kontejner zavisnosti** (u Angularu *injector*) je deo radnog okvira koji pravi objekte servisa i daje ih klasama koje ih traže. Sledeći kod prikazuje servis koji čuva prijavljenog korisnika:

```ts
import { Injectable, computed, signal } from '@angular/core';

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
- Podešavanje `providedIn: 'root'` znači da za celu aplikaciju postoji jedan objekat ove klase. Kontejner ga pravi kada ga neko prvi put zatraži, a svakom sledećem daje isti objekat.
- Token je tekst koji server izdaje pri prijavi i po kom u svakom narednom zahtevu prepoznaje korisnika. Metoda `login`, koju smo izostavili, dobija token od servera i upisuje ga u signal `token`. Funkcija `decodeUser` iz iste datoteke iz teksta tokena čita objekat tipa `User`, a za `null` vraća `null`.
- Funkcija `decodeUser` samo čita podatke iz tokena, a ne proverava da li je token ispravan. Tu proveru radi server pri svakom zahtevu. Podatak o korisniku na klijentu služi za prikaz, a ne kao dokaz da je korisnik prijavljen.
- Signal `token` je `private`, pa ga menjaju samo metode servisa. Komponente čitaju samo izvedene signale `user` i `isLoggedIn`.
- Servis nema selektor ni šablon, već drži podatak i metode nad njim.

## Preuzimanje zavisnosti

**Preuzimanje zavisnosti** (engl. *dependency injection*, u literaturi i „ubrizgavanje zavisnosti“) je mehanizam kojim klasa dobija objekat druge klase od kontejnera zavisnosti, umesto da ga sama pravi sa `new`. U Angularu se zavisnost preuzima pozivom `inject(Klasa)`. Sledeći kod prikazuje stranicu sa spiskom blogova koja preuzima servis `Auth` i deo šablona koji ga čita:

```ts
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
// import servisa Auth

@Component({
  selector: 'app-blog-list',
  imports: [RouterLink],
  templateUrl: './blog-list.html',
  styleUrl: './blog-list.scss',
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
- Poziv `inject(Auth)` stoji u inicijalizatoru polja, odnosno u vrednosti koju polje dobija pri deklaraciji. Poziv `inject` radi samo dok Angular pravi objekat klase, tj. u inicijalizatoru polja ili u konstruktoru. Poziv iz metode, npr. pri kliku, prijavljuje grešku. Zato servis preuzimamo jednom, u polje, i posle ga koristimo kroz to polje.
- Servis ne pravimo sa `new Auth()`. Da zaglavlje i spisak blogova svaki napravi svoj objekat, postojala bi dva objekta sa dva različita tokena, pa odjava u zaglavlju ne bi uticala na spisak. To je isti problem kao sa signalom u komponenti, sa početka lekcije. Kontejner svima daje isti objekat, pa svi vide isto stanje.
- Polje `auth` je `protected`, jer ga čita šablon. Šablon čita signal servisa kroz polje, `auth.isLoggedIn()`, i postaje njegov čitalac.

## Servis koji drži podatke

Servis ne mora da čuva samo stanje aplikacije, kao `Auth`. Može da drži i domenske podatke koje čita više komponenata. Spisak blogova prikazuje sve blogove, a stranica bloga prikazuje jedan, po `id`-u iz adrese. Obe čitaju iste blogove, pa oni žive u jednom servisu. Blog opisuje interfejs `BlogDto` iz lekcije o ruteru, dopunjen poljem `author`:

```ts
export interface BlogDto {
  id: string;
  title: string;
  author: string;
  content: string;
}
```

```ts
import { Injectable, signal } from '@angular/core';
// import interfejsa BlogDto

@Injectable({ providedIn: 'root' })
export class BlogService {
  private readonly blogs = signal<BlogDto[]>([
    { id: '1', title: 'Zimski uspon na Rtanj', author: 'Ana', content: 'Opis uspona po snegu.' },
    { id: '2', title: 'Vikend na Tari', author: 'Marko', content: 'Vidikovci i staze uz Drinu.' },
  ]);

  readonly all = this.blogs.asReadonly();

  byId(id: string): BlogDto | undefined {
    return this.blogs().find((blog) => blog.id === id);
  }
}
```

U datom kodu treba uočiti sledeće:
- Poziv `asReadonly` vraća isti signal, ali samo za čitanje, bez metoda `set` i `update`. Komponente tako čitaju blogove, a menja ih samo servis, isto kao što je `token` u servisu `Auth` privatan.
- Metoda `byId` pronalazi blog po identifikatoru, a vraća `undefined` kada blog ne postoji.

Stranica bloga preuzima servis i od `id`-a iz rute računa blog:

```ts
import { Component, computed, inject, input } from '@angular/core';
// import servisa BlogService

@Component({
  selector: 'app-blog-detail',
  templateUrl: './blog-detail.html',
  styleUrl: './blog-detail.scss',
})
export class BlogDetail {
  readonly id = input.required<string>();

  private readonly blogService = inject(BlogService);

  protected readonly currentBlog = computed(() => this.blogService.byId(this.id()));
}
```

```html
@if (currentBlog(); as blog) {
  <article>
    <h1>{{ blog.title }}</h1>
    <p>Autor: {{ blog.author }}</p>
    <p>{{ blog.content }}</p>
  </article>
} @else {
  <p>Blog ne postoji.</p>
}
```

U datom kodu treba uočiti sledeće:
- U prethodnoj lekciji stranica bloga je držala sopstveni spisak blogova, odvojen od spiska na stranici `BlogList`. Sada obe komponente preuzimaju isti objekat servisa, kao što `App` i `BlogList` dele isti `Auth`, pa čitaju iste blogove. Novi blog dodat kroz servis odmah bi videle obe.
- Polje `blogService` je `private`, jer ga šablon ne čita. Šablon čita samo izvedeni signal `currentBlog`.
- Izvedeni signal `currentBlog` čita ulaz `id`, ali postaje čitalac i signala `blogs` iz servisa, iako ga ne čita direktno. Metoda `byId` unutar sebe poziva `this.blogs()`, a za izvedeni signal je bitno koji su signali pročitani dok se računao, a ne gde je poziv napisan. Zato se stranica bloga osvežava i kada se promeni `id` i kada se promeni spisak u servisu.
- Blogovi su u primeru upisani u servis. U narednoj lekciji servis iste blogove dobavlja sa servera, a komponente koje koriste servis ostaju nepromenjene.

## Zaglavlje sa korisnikom

Povežimo pojmove u korensku komponentu projekta, koja u zaglavlju prikazuje prijavljenog korisnika ili veze za prijavu:

```ts
import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
// import servisa Auth

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
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

Kada prijavljeni korisnik, dok je na stranici sa spiskom blogova, klikne na dugme za odjavu u zaglavlju, dešava se sledeće:
1. Vezivanje događaja poziva `auth.logout()` na objektu servisa koji je korenska komponenta preuzela.
2. Metoda `logout` upisuje `null` u signal `token`, koji obaveštava svoje čitaoce.
3. Izvedeni signal `user` je čitalac signala `token`, a `isLoggedIn` je čitalac signala `user`, pa se oba označavaju za ponovno računanje.
4. Šablon zaglavlja je čitalac signala `user`, jer ga čita u naredbi `@if`, pa Angular ponovo proverava šablon korenske komponente. Pri proveri `user` računa novu vrednost, `null`, i prikazuje se grana `@else` sa vezama za prijavu i registraciju.
5. Šablon spiska blogova je čitalac signala `isLoggedIn` istog objekta servisa, pa Angular ponovo proverava i njega i uklanja vezu za pravljenje bloga.
