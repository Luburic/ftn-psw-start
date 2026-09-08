Svaka klijentska aplikacija rešava iste tehničke probleme, nezavisno od toga čemu služi. Mora da:
1. Prikaže podatke u HTML dokumentu.
2. Osveži prikaz kada se podaci promene.
3. Reaguje na akcije korisnika.
4. Po adresi u pregledaču odluči koju stranicu prikazuje.
5. Razmenjuje podatke sa serverom.

React, koji čitalac poznaje, rešava prva dva problema. Za ostale programer bira i povezuje dodatne biblioteke, pa dve React aplikacije retko imaju istu strukturu. Angular rešava svih pet problema i propisuje strukturu aplikacije. Razlika između radnog okvira i biblioteke, poznata sa serverske strane, važi i ovde. Biblioteku pozivamo iz svog koda, a radni okvir drži kontrolu toka i poziva naš kod na mestima koja smo mu deklarativno označili.

**Angular** je radni okvir za izgradnju klijentskih veb aplikacija u jeziku TypeScript. Ovde čitamo najmanju Angular aplikaciju dovoljnu za rad, datoteku po datoteku, koristeći skraćene verzije datoteka iz našeg projekta.

## Struktura radnog prostora

Komanda `ng new` pravi radni prostor. Sledeći prikaz daje njegove datoteke, bez onih koje se retko otvaraju:

```
frontend/
  angular.json          Podešavanja alata za prevođenje i pokretanje
  package.json          Zavisnosti i komande projekta
  proxy.conf.json       Prosleđivanje zahteva ka serveru tokom razvoja
  src/
    index.html          Jedina HTML stranica aplikacije
    main.ts             Ulazna tačka
    styles.scss         Globalni stilovi
    app/
      app.config.ts     Konfiguracija aplikacije
      app.routes.ts     Tabela ruta
      app.ts            Korenska komponenta
      app.html          Šablon korenske komponente
```

Datoteke van direktorijuma `src/` su podešavanja alata i menja ih platformski tim. Kod aplikacije živi u `src/app/`.

## Ulazna tačka

Datoteka `index.html` je jedina HTML stranica koju pregledač učitava. Njeno telo sadrži jedan element:

```html
<body>
  <app-root></app-root>
</body>
```

Element `app-root` nije standardan HTML element i pregledač ga ne poznaje. Angular ga pri pokretanju zamenjuje sadržajem korenske komponente. Pokretanje se dešava u datoteci `main.ts`:

```ts
bootstrapApplication(App, appConfig);
```

Poziv `bootstrapApplication` prima klasu korenske komponente i konfiguraciju aplikacije. Od tog trenutka Angular upravlja sadržajem stranice.

## Konfiguracija aplikacije

Datoteka `app.config.ts` sadrži spisak delova radnog okvira koje aplikacija uključuje jednom, pri pokretanju:

```ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(),
  ],
};
```

U datom kodu treba uočiti sledeće:
- Poziv `provideRouter` uključuje rutiranje i prima tabelu ruta. Opcija `withComponentInputBinding` dozvoljava da deo adrese postane ulaz komponente, što obrađuje [lekcija o rutiranju](7-rutiranje.md).
- Poziv `provideHttpClient` uključuje razmenu podataka sa serverom, što obrađuje [lekcija o čitanju podataka](9-citanje-podataka.md).
- Poziv `provideBrowserGlobalErrorListeners` je podrazumevana stavka koju `ng new` upisuje. Ostavljamo je i ne bavimo se njome.

Ova datoteka je klijentski pandan datoteci `Program.cs`. Svaki deo radnog okvira se uključuje ovde, a kod aplikacije ga koristi bez daljeg podešavanja.

## Tabela ruta

Datoteka `app.routes.ts` sadrži tabelu ruta, ovde skraćenu na tri stavke:

```ts
export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
];
```

Svaka stavka povezuje deo adrese iza naziva servera sa klasom komponente. Adresa `localhost:4200/login` prikazuje komponentu `Login`, a adresa bez dodatnog dela prikazuje komponentu `Home`. Prava tabela ruta u projektu ima još četiri stavke, po jednu za svaki modul. Svaka od njih učitava svoj modul tek kada korisnik prvi put ode na njegovu adresu. Njih obrađuje [lekcija o rutiranju](7-rutiranje.md).

## Korenska komponenta

**Komponenta** (engl. *component*) je klasa sa pridruženim HTML šablonom koja upravlja jednim delom stranice. Korenska komponenta upravlja celom stranicom. Sledeći kod prikazuje njenu klasu, skraćenu na deo koji nam je sada potreban:

```ts
@Component({
  imports: [RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {}
```

Sledeći kod prikazuje njen šablon, iz datoteke `app.html`:

```html
<header>
  <nav>
    <a routerLink="/">Home</a>
    <a routerLink="/exploration">Exploration</a>
    <a routerLink="/login">Login</a>
  </nav>
</header>

<main>
  <router-outlet />
</main>
```

U datom kodu treba uočiti sledeće:
- Zapis `@Component({ ... })` je dekorator. **Dekorator** (engl. *decorator*) je oznaka iznad klase kojom radni okvir prepoznaje ulogu klase, isto kao atribut `[ApiController]` na serveru. Objekat u zagradi nosi podešavanja komponente.
- Podešavanje `selector` određuje naziv HTML elementa pod kojim se komponenta koristi. Zato `index.html` sadrži element `app-root`.
- Podešavanja `templateUrl` i `styleUrl` vezuju klasu za šablon i za datoteku stilova. Tri datoteke istog naziva, sa nastavcima `.ts`, `.html` i `.scss`, čine jednu komponentu.
- Podešavanje `imports` nabraja šta šablon sme da koristi pored običnog HTML-a. Element `router-outlet` i atribut `routerLink` dolaze iz rutiranja, pa su ovde navedene klase `RouterOutlet` i `RouterLink`.
- Element `router-outlet` je mesto na koje rutiranje ubacuje komponentu izabranu po adresi. Ostatak šablona je zajednički za sve stranice.
- Prava klasa `App` ima jedno polje i šablon ima jedan uslov koji zavisi od toga da li je korisnik prijavljen. Polje obrađuje [lekcija o servisima i zavisnostima](8-servisi-i-zavisnosti.md), a uslov [lekcija o kontroli toka](5-kontrola-toka.md).

## Od adrese do prikaza

Kada korisnik otvori adresu `localhost:4200/login`, dešava se sledeće:
1. Pregledač učitava `index.html` i u njemu nalazi element `app-root`.
2. Izvršava se `main.ts`, koji poziva `bootstrapApplication` sa klasom `App` i konfiguracijom iz `app.config.ts`.
3. Angular pravi korensku komponentu i njenim šablonom zamenjuje element `app-root`.
4. Rutiranje čita deo adrese `login`, u tabeli ruta nalazi komponentu `Login` i ubacuje je na mesto elementa `router-outlet`.
5. Korisnik vidi zaglavlje sa navigacijom iz korenske komponente i formu za prijavu iz komponente `Login`.

Kada korisnik klikne na vezu u navigaciji, rutiranje menja adresu bez učitavanja nove stranice i ponavlja korak 4 sa novom komponentom.

## Pokretanje

Komanda `npm start` prevodi aplikaciju i pokreće razvojni server na adresi `localhost:4200`. Datoteka `proxy.conf.json` sadrži uputstvo da se svaki zahtev čija adresa počinje sa `/api` prosledi serverskoj aplikaciji na portu 5000. Zato u klijentskom kodu adrese pišemo kao `/api/social/blogs`, bez naziva servera.

Greške se pojavljuju na dva mesta. Greške prevođenja, poput prekršene anotacije tipa, prijavljuje terminal u kom je pokrenut `npm start`. Greške pri izvršavanju, poput neuspelog zahteva ka serveru, prijavljuje konzola pregledača.
