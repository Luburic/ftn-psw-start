Svaka klijentska aplikacija rešava iste tehničke probleme, nezavisno od toga čemu služi. Mora da:
1. Prikaže podatke u HTML dokumentu.
2. Osveži prikaz kada se podaci promene.
3. Reaguje na akcije korisnika.
4. Po adresi u internet čitaču odluči koju stranicu prikazuje.
5. Razmenjuje podatke sa serverom.

Biblioteke po pravilu rešavaju samo deo ovih problema, pa programer bira i povezuje više njih, zbog čega dve aplikacije retko imaju istu strukturu (čest slučaj sa React-om). Angular rešava svih pet problema i uz to propisuje strukturu aplikacije.

**Angular** je radni okvir za izgradnju klijentskih veb aplikacija u jeziku TypeScript. Ovde čitamo najmanju Angular aplikaciju dovoljnu za rad, datoteku po datoteku, koristeći skraćene verzije datoteka iz našeg projekta.

## Struktura radnog prostora

Komanda `ng new [naziv-projekta]` kreira novi projekat. Sledeći prikaz daje početne datoteke jednog projekta, pri čemu su izostavljene datoteke koje se retko menjaju:

```
  angular.json          Podešavanje alata za prevođenje i pokretanje
  package.json          Spisak biblioteka od kojih projekat zavisi i komande za pokretanje (npr. start)
  src/                  Izvorni kod aplikacije; u okviru ovog direktorijuma se dešava najviše izmena u toku razvoja projekta
    index.html          Korenska HTML stranica aplikacije, jedina koju čitač učitava
    main.ts             Ulazna tačka od koje kreće izvršavanje projekta; pokreće Angular aplikaciju
    styles.scss         Globalni stilovi koji važe za celu aplikaciju
    app/                Kod same aplikacije: komponente, servisi i njihova konfiguracija
      app.config.ts     Konfiguracija aplikacije: delovi radnog okvira koji se uključuju pri pokretanju
      app.ts            Korenska komponenta, prva koju Angular iscrtava
      app.html          Šablon korenske komponente (šta se prikazuje)
      app.scss          Stilovi korenske komponente
```

## Ulazna tačka

Datoteka `index.html` je jedina HTML stranica koju internet čitač učitava. Njeno telo sadrži jedan element:

```html
<body>
  <app-root></app-root>
</body>
```

Element `app-root` nije standardan HTML element i internet čitač ga ne poznaje. Angular ga pri pokretanju ne uklanja, već unutar njega iscrtava sadržaj korenske komponente. Pokretanje se dešava u datoteci `main.ts`:

```ts
bootstrapApplication(App, appConfig);
```

Poziv `bootstrapApplication` prima klasu korenske komponente i konfiguraciju aplikacije. Od tog trenutka Angular upravlja sadržajem stranice.

## Korenska komponenta

**Komponenta** (engl. *component*) je osnovni gradivni blok Angular aplikacije: klasa sa pridruženim HTML šablonom i CSSom koja upravlja jednim delom stranice. Klasa čuva podatke i logiku (šta se prikazuje i kako se reaguje na akcije), a šablon opisuje izgled (kako se to iscrtava). Stranicu gradimo od više komponenti. Na primer jedna prikazuje zaglavlje, druga spisak tura, treća formu. Korenska komponenta je prva koju Angular iscrtava i unutar sebe smešta sve ostale.

Komponentu čine tri datoteke istog naziva: `.ts` (klasa), `.html` (šablon) i `.scss` (stilovi). Sledeći kod prikazuje klasu korenske komponente:

```ts
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
```

Sledeći kod prikazuje njen šablon, iz datoteke `app.html`:

```html
<header>
  <h1>Explorer</h1>
</header>

<main>
  <p>Dobro došli.</p>
</main>
```

U datom kodu treba uočiti sledeće:
- Zapis `@Component({ ... })` je dekorator. **Dekorator** (engl. *decorator*) je oznaka iznad klase kojom radni okvir prepoznaje ulogu klase, isto kao atribut `[ApiController]` na serveru. Objekat u zagradi nosi podešavanja komponente.
- Podešavanje `selector` određuje naziv HTML elementa pod kojim se komponenta koristi. Zato `index.html` sadrži element `app-root`.
- Podešavanja `templateUrl` i `styleUrl` vezuju klasu za šablon i za datoteku stilova.
- Klasa je prazna, jer šablon ne prikazuje nijedan podatak iz nje. Sve što korisnik vidi upisano je u šablon.

## Podatak iz klase u šablonu

Prazna klasa retko je korisna — komponenta obično čuva podatke koje prikazuje. Vrednost iz klase u šablon prenosimo zapisom `{{ ... }}`, koji nazivamo **interpolacija** (engl. *interpolation*). Sledeći kod dodaje polje u klasu:

```ts
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = 'Explorer';
}
```

Šablon zatim čita to polje umesto tvrdo upisanog teksta:

```html
<header>
  <h1>{{ title }}</h1>
</header>
```

U datom kodu treba uočiti sledeće:
- Zapis `{{ title }}` u šablonu prevodi se u vrednost polja `title` iz klase. U internet čitaču se prikazuje `Explorer`.
- Šablon vidi samo članove komponente kojoj pripada; polja druge komponente nisu mu dostupna.

Naredna lekcija će detaljno razraditi koncept komponente.

## Od pokretanja do prikaza

Kada korisnik otvori adresu `localhost:4200`, dešava se sledeće:
1. Internet čitač učitava `index.html` i u njemu nalazi element `app-root`.
2. Izvršava se `main.ts`, koji poziva `bootstrapApplication` sa klasom `App` i konfiguracijom iz `app.config.ts`.
3. Angular pravi korensku komponentu i njen šablon iscrtava unutar elementa `app-root`.
4. Korisnik vidi zaglavlje i poruku iz šablona korenske komponente.

## Priprema okruženja

Da bi se Angular projekat pokrenuo na tvojoj mašini, neophodno je da instaliraš:

1. **Node.js** — okruženje u kom se izvršava alat za prevođenje (provera: `node --version`). Preporučuje se aktuelna LTS verzija.
2. **npm** — menadžer paketa, stiže uz Node.js (provera: `npm --version`).
3. **Angular CLI** — alat komandne linije radnog okvira, instalira se jednom, globalno: `npm install -g @angular/cli` (provera: `ng --version`).

## Preuzimanje i pokretanje projekta

Kod tima preuzimamo klonom repozitorijuma, a zatim se u korenu projekta pokreće:

1. `git clone <url>` — preuzima kod projekta na lokalnu mašinu.
2. `npm install` — čita `package.json` i preuzima sve biblioteke u direktorijum `node_modules/`.
3. `ng serve` (ili `npm start`, koji ga najčešće samo poziva) — prevodi aplikaciju i pokreće razvojni server na adresi `localhost:4200`.

Razvojni server nakon svake izmene datoteke ponovo prevodi aplikaciju i osvežava stranicu u internet čitaču, pa se rezultat vidi bez ručnog ponovnog pokretanja.

Direktorijum `node_modules/` naveden je u `.gitignore` i ne šalje se na repozitorijum. Zato posle svakog preuzimanja tuđih izmena (`git pull`) treba ponovo pokrenuti `npm install`, kako bi se lokalno instalirale eventualne nove biblioteke koje su saradnici dodali.

## Generisanje koda alatom CLI

Delove aplikacije ne pravimo ručno, datoteku po datoteku, nego ih generiše Angular CLI. Komande se pokreću iz korena projekta:

- `ng generate component putanja/naziv` (skraćeno `ng g c putanja/naziv`) pravi komponentu, sa sve tri njene datoteke (`.ts`, `.html`, `.scss`) u istoimenom direktorijumu.
- `ng generate service putanja/naziv` (skraćeno `ng g s putanja/naziv`) pravi servis.
