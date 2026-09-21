
Zaglavlje korenske komponente prikazuje adresu e-pošte prijavljenog korisnika. Spisak
blogova prikazuje vezu „Create blog“, ali samo prijavljenom korisniku. Obe komponente,
dakle, moraju da znaju ko je prijavljen.

Prvo što nam pada na pamet je da svaka komponenta drži korisnika u svom signalu. Tada bi
postojale dve odvojene kopije istog podatka. Kada korisnik klikne na „Logout“ u zaglavlju,
zaglavlje briše svoju kopiju, ali spisak blogova i dalje drži svoju i nastavlja da
prikazuje vezu za pravljenje bloga. Dve kopije jednog podatka pre ili kasnije se razilaze.

Druga ideja je da korisnika drži samo jedna komponenta, npr. spisak blogova, a da ga
ostale čitaju od nje. Ni to ne radi. Spisak blogova je komponenta rute, pa je Angular
uništava čim korisnik pređe na drugu stranicu, a sa njom nestaje i signal. Pored toga,
zaglavlje nije ni roditelj ni dete spiska blogova, pa ne može da čita njegove signale ni
kroz ulaze ni kroz izlaze.

Korisniku je zato potrebno mesto koje ne pripada nijednoj komponenti, koje postoji sve
vreme dok aplikacija radi i kojem svaka komponenta može da pristupi. Podatak tada postoji
u jednom primerku, pa ga sve komponente vide isto, a odjava na jednom mestu odmah važi
svuda. Takvo mesto u Angularu je **servis**.

## Servis

**Servis** (engl. *service*) je klasa označena dekoratorom `@Injectable`, čiji objekat
pravi kontejner zavisnosti i daje ga svakom ko ga zatraži. **Kontejner zavisnosti** (u
Angularu *injector*) je deo radnog okvira koji pravi objekte servisa i daje ih klasama
koje ih traže. Sledeći kod prikazuje servis koji čuva prijavljenog korisnika, klasu `Auth`
iz `core/auth/auth.ts`, u pojednostavljenom obliku:

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

- Podešavanje `providedIn: 'root'` znači da za celu aplikaciju postoji jedan objekat ove
  klase. Kontejner ga pravi kada ga neko prvi put zatraži, a svakom sledećem daje isti
  objekat.
- Token je tekst koji server izdaje pri prijavi i po kom u svakom narednom zahtevu
  prepoznaje korisnika. Metoda `login`, koju smo izostavili, dobija token od servera i
  upisuje ga u signal `token`. Funkcija `decodeUser` iz iste datoteke iz teksta tokena
  čita objekat tipa `User`, a za `null` vraća `null`.
- Funkcija `decodeUser` samo čita podatke iz tokena, a ne proverava da li je token
  ispravan. Tu proveru radi server pri svakom zahtevu. Podatak o korisniku na klijentu
  služi za prikaz, a ne kao dokaz da je korisnik prijavljen.
- Signal `token` je `private`, pa ga menjaju samo metode servisa. Komponente čitaju samo
  izvedene signale `user` i `isLoggedIn`.
- Servis nema selektor ni šablon, već drži podatak i metode nad njim.

> **Napomena o projektu:** Pravi `Auth` ima nekoliko detalja koje ovde izostavljamo, jer
> nisu tema ove lekcije: `User` sadrži i `roles` (uloge iz tokena); token se čuva u
> `localStorage`, pa prijava opstaje i posle osvežavanja stranice (`logout` ga i briše); a
> `login`/`register` šalju zahtev serveru preko `HttpClient` (lekcija o komunikaciji sa
> serverom). Suština je ista: jedan `providedIn: 'root'` servis sa privatnim `token`
> signalom i izvedenim `user`/`isLoggedIn`.

## Ubrizgavanje zavisnosti

**Ubrizgavanje zavisnosti** (engl. *dependency injection*) je mehanizam kojim klasa dobija
objekat druge klase od kontejnera zavisnosti, umesto da ga sama pravi sa `new`. U Angularu
se zavisnost ubrizgava pozivom `inject(Klasa)`. Sledeći kod prikazuje stranicu sa spiskom
blogova kojoj se ubrizgava servis `Auth` i deo šablona koji ga čita:

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

- Poziv `inject(Auth)` stoji u inicijalizatoru polja, odnosno u vrednosti koju polje dobija
  pri deklaraciji. Poziv `inject` radi samo dok Angular pravi objekat klase, tj. u
  inicijalizatoru polja ili u konstruktoru. Poziv iz metode, npr. pri kliku, prijavljuje
  grešku. Zato zavisnost ubrizgavamo jednom, u polje, i posle je koristimo kroz to polje.
- Servis ne pravimo sa `new Auth()`. Da zaglavlje i spisak blogova svaki napravi svoj
  objekat, postojala bi dva objekta sa dva različita tokena, pa odjava u zaglavlju ne bi
  uticala na spisak. To je isti problem kao sa signalom u komponenti, sa početka lekcije.
  Kontejner svima daje isti objekat, pa svi vide isto stanje.
- Polje `auth` je `protected`, jer ga čita šablon. Šablon čita signal servisa kroz polje,
  `auth.isLoggedIn()`, i postaje njegov čitalac.

## Šta ide u servis, a šta u stranicu

`Auth` drži **stanje koje nadživljava stranicu**. Servis, međutim, ne mora da drži stanje. Češće drži **komande** tj. operacije koje menjaju
podatke na serveru. Takav je, na primer, `BlogReading` servis:

```ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateCommentDto, UpdateCommentDto } from '../api/social-api-types';

const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class BlogReading {
  private readonly http = inject(HttpClient);

  async addComment(id: string, dto: CreateCommentDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/comments`, dto));
  }

  async updateComment(id: string, commentId: string, dto: UpdateCommentDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/comments/${commentId}`, dto));
  }

  async deleteComment(id: string, commentId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/comments/${commentId}`));
  }
}
```

Stranica koja pokreće te komande dobija servis ubrizgavanjem, isto kao `Auth`, kroz
`inject`:

```ts
export class BlogDetail {
  private readonly blogReading = inject(BlogReading);
  // ...
}
```

U datom kodu treba uočiti sledeće:

- `BlogReading` i sam koristi ubrizgavanje: ubrizgava mu se `HttpClient` da bi slao zahteve
  serveru. Servisima se tako ubrizgavaju drugi servisi, jednako kao komponentama.
- Servis drži samo komande, ne i podatke koje stranica prikazuje.

> **Napomena o projektu:** Za razliku od stanja o korisniku, **domenske podatke (spisak
> blogova, jedan blog, spisak tura) projekat ne drži u zajedničkom servisu.** Svaka
> stranica ih čita direktno sa servera, u samoj stranici, preko `httpResource` (lekcija o
> komunikaciji sa serverom). Na primer, `BlogList` ima `blogs = httpResource(...)`, a
> stranica bloga `detail = httpResource(() => '/api/social/blogs/' + id())`, koji se
> ponovo dovlači kada se `id` iz rute promeni. Pravilo je: **upiti žive u stranici (`httpResource`),
> komande u servisu.** Podatke između modula ne spajamo na klijentu, već na serveru.

## Zaglavlje sa korisnikom

Povežimo pojmove u korensku komponentu projekta, koja u zaglavlju prikazuje prijavljenog
korisnika ili veze za prijavu:

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

Kada prijavljeni korisnik, dok je na stranici sa spiskom blogova, klikne na dugme za odjavu
u zaglavlju, dešava se sledeće:

1. Vezivanje događaja poziva `auth.logout()` na objektu servisa koji je ubrizgan korenskoj
   komponenti.
2. Metoda `logout` upisuje `null` u signal `token`, koji obaveštava svoje čitaoce.
3. Izvedeni signal `user` je čitalac signala `token`, a `isLoggedIn` je čitalac signala
   `user`, pa se oba označavaju za ponovno računanje.
4. Šablon zaglavlja je čitalac signala `user`, jer ga čita u naredbi `@if`, pa Angular
   ponovo proverava šablon korenske komponente. Pri proveri `user` računa novu vrednost,
   `null`, i prikazuje se grana `@else` sa vezama za prijavu i registraciju.
5. Šablon spiska blogova je čitalac signala `isLoggedIn` **istog objekta servisa**, pa
   Angular ponovo proverava i njega i uklanja vezu za pravljenje bloga.
