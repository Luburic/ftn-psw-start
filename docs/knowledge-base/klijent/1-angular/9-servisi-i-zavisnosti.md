Zamislimo aplikaciju za blogove gde u zaglavlju stoji korisničko ime izvučeno iz objekta prijavljenog korisnika. Dalje, na stranici za spisak blogova stoji dugme „Create blog“ koje se prikazuje samo prijavljenom korisniku. Obe komponente, dakle, moraju da znaju ko je prijavljen.

Moguće rešenje je da svaka komponenta drži korisnika u svom signalu. Tada bi postojale dve odvojene kopije istog podatka. Kada korisnik klikne na „Logout“ u zaglavlju, zaglavlje briše svoju kopiju, ali spisak blogova i dalje drži svoju i nastavlja da prikazuje dugme za pravljenje bloga.

Druga ideja je da korisnika drži samo jedna komponenta, npr. korenska komponenta, a da ga ostale komponente dobijaju kroz `input`. Ovde postoje dva problema. Prvo, sve komponente u hijerarhiji bi morale da prosleđuju ovaj podatak kroz stablo čak i kada ga ne koriste. Drugo, ako hijerarhija negde koristi rutiranje, nije moguće propagirati podatak do komponenti koje se prikazuju na osnovu ruta.

Postoji potreba za memorijom koja ne pripada nijednoj komponenti, koja postoji sve vreme dok aplikacija radi i kojoj svaka komponenta može da pristupi. Podatak tada postoji u jednom primerku, pa ga sve komponente vide isto. Angular nudi **servise** kao mehanizam za deljenje stanja i ponašanja između komponenti.

## Servis

**Servis** (engl. *service*) je klasa označena dekoratorom `@Injectable`, čiji objekat pravi kontejner zavisnosti i daje ga svakom ko ga zatraži. Sledeći kod prikazuje isečak servisa koji čuva prijavljenog korisnika:

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

  // login i decode user metode

  logout(): void {
    this.token.set(null);
  }
}
```

U datom kodu treba uočiti sledeće:

- Podešavanje `providedIn: 'root'` znači da za celu aplikaciju postoji jedan objekat ove klase. Kontejner ga pravi kada ga neko prvi put zatraži, a svakom sledećem daje isti objekat.
- Token je tekst koji server izdaje pri prijavi i po kom u svakom narednom zahtevu prepoznaje korisnika. Metoda `login`, koju smo izostavili, dobija token od servera i upisuje ga u signal `token`.
- Signal `token` je `private`, pa ga menjaju samo metode servisa. Komponente čitaju samo izvedene signale `user` i `isLoggedIn`.

## Ubrizgavanje zavisnosti

**Ubrizgavanje zavisnosti** (engl. *dependency injection*) je mehanizam kojim klasa dobija objekat druge klase od kontejnera zavisnosti, umesto da ga sama pravi sa `new`. U Angularu se zavisnost ubrizgava pozivom `inject(Klasa)`. Sledeći kod prikazuje stranicu sa spiskom blogova kojoj se ubrizgava servis `Auth` i deo šablona koji ga čita:

```ts
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

- Poziv `inject` radi samo dok Angular pravi objekat klase, tj. u inicijalizatoru polja ili u konstruktoru. Poziv iz metode, npr. pri kliku, prijavljuje grešku.
- Servis ne pravimo sa `new Auth()`. Da zaglavlje i spisak blogova svaki napravi svoj objekat, postojala bi dva objekta sa dva različita tokena, pa odjava u zaglavlju ne bi uticala na spisak. To je isti problem kao sa signalom u komponenti, sa početka lekcije. Kontejner svima daje isti objekat, pa svi vide isto stanje.
- Polje `auth` je `protected`, jer ga čita šablon. Šablon je čitalac signala servisa `auth.isLoggedIn()`.
