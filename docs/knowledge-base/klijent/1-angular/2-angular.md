Svaka klijentska aplikacija rešava iste tehničke probleme, nezavisno od toga čemu služi. Mora da:
1. Prikaže podatke u HTML dokumentu.
2. Osveži prikaz kada se podaci promene.
3. Reaguje na akcije korisnika.
4. Po adresi u internet čitaču odluči koju stranicu prikazuje.
5. Razmenjuje podatke sa serverom.

Biblioteke po pravilu rešavaju samo deo ovih problema, pa programer bira i povezuje više njih, zbog čega dve aplikacije retko imaju istu strukturu (čest slučaj sa React-om). Angular rešava svih pet problema i uz to propisuje strukturu aplikacije.

**Angular** je radni okvir za izgradnju klijentskih veb aplikacija u jeziku TypeScript. U nastavku prolazimo kroz minimalnu Angular aplikaciju dovoljnu za rad, datoteku po datoteku.

## Priprema okruženja

Da bi se Angular projekat pokrenuo na računaru, neophodno je instalirati tri alata:

1. **Node.js** je okruženje koje izvršava JavaScript van internet čitača. Na njemu rade Angular CLI i ostali alati iz projekta. Svaka verzija Angular-a podržava tačno određene verzije Node.js-a, pa verziju treba proveriti u tabeli kompatibilnosti na zvaničnom sajtu angular.dev. Instalaciju proveravamo komandom `node --version`.
2. **npm** je menadžer paketa i stiže uz Node.js. Instalaciju proveravamo komandom `npm --version`.
3. **Angular CLI** je alat komandne linije radnog okvira. Instalira se jednom, globalno, komandom `npm install -g @angular/cli`, a instalaciju proveravamo komandom `ng --version`.

## Struktura radnog prostora

Komanda `ng new [naziv-projekta]` pravi nov projekat. U korenu projekta otvaramo dve datoteke:

1. `package.json` sadrži spisak biblioteka koje projekat koristi, podeljen u dve grupe. Grupa `dependencies` su biblioteke koje aplikacija koristi dok radi i koje ulaze u prevedeni kod, npr. `@angular/core`. Grupa `devDependencies` su alati potrebni samo tokom razvoja, npr. Angular CLI i TypeScript prevodilac. Datoteka sadrži i odeljak `scripts` sa skraćenicama za česte komande. Na primer, tu je definisano da `npm start` poziva `ng serve`.
2. `angular.json` je konfiguracija Angular projekta. Sadrži podešavanja za komande `ng build`, `ng serve` i `ng test`, npr. putanje do globalnih stilova i statičkih datoteka.

Ostale datoteke u korenu, poput `package-lock.json`, `.editorconfig` i `tsconfig.json`, u kom je uključen strogi režim iz lekcije o TypeScript-u, podešava alat i ne menjamo ih.

Datoteke u okviru direktorijuma `src` intenzivno menjamo i dodajemo:

1. `index.html` je korenska HTML stranica aplikacije, jedina koju internet čitač učitava.
2. `main.ts` je ulazna tačka od koje kreće izvršavanje aplikacije.
3. `styles.scss` sadrži globalne stilove koji važe za celu aplikaciju.
4. `app/` sadrži kod same aplikacije: komponente, servise i njihovu konfiguraciju. U okviru ovog direktorijuma se nalaze:
    - `app.config.ts`, konfiguracija aplikacije, tj. delovi radnog okvira koji se uključuju pri pokretanju.
    - `app.ts`, korenska komponenta, prva koju Angular iscrtava.
    - `app.html`, šablon korenske komponente.
    - `app.scss`, stilovi korenske komponente.

Pored navedenih, nov projekat sadrži i nekoliko datoteka koje za sada možemo zanemariti, npr. `app.routes.ts` (definicija ruta, tj. koja stranica se prikazuje za koju adresu), `app.spec.ts` (test korenske komponente) i direktorijum `public/` (statičke datoteke, npr. ikonica sajta).

## Pokretanje projekta

Kod preuzimamo kloniranjem repozitorijuma, a zatim u korenu projekta pokrećemo `npm install`. Komanda čita `package.json` i preuzima sve biblioteke u direktorijum `node_modules/`. Taj direktorijum je naveden u `.gitignore` i ne šalje se na repozitorijum, pa komandu ponavljamo posle svakog preuzimanja tuđih izmena (`git pull`), da bi se lokalno instalirale biblioteke koje su saradnici dodali.

Projekat zatim pokrećemo komandom `ng serve` (ili `npm start`, koji je samo poziva). Ta komanda pokreće niz koraka koji izvorni kod pretvaraju u aplikaciju koju internet čitač ume da prikaže. U nastavku pratimo taj put, od komande do prikaza na ekranu.

**1. Prevođenje i objedinjavanje (engl. *build*).** Internet čitač ne razume TypeScript ni SCSS, već samo JavaScript, CSS i HTML. Zato Angular CLI prvo prevodi aplikaciju: TypeScript kod (`.ts`) u JavaScript, Angular šablone i dekoratore (`@Component`) u JavaScript koji ume da iscrta prikaz, a SCSS stilove (`.scss`) u CSS. Sav taj kod, zajedno sa delovima biblioteka iz `node_modules/` koje aplikacija koristi, CLI potom objedinjuje u mali broj JavaScript i CSS datoteka. Ovaj korak se naziva **objedinjavanje** (engl. *bundling*), a dobijene datoteke zovemo **objedinjene datoteke** (engl. *bundles*).

**2. Pokretanje razvojnog servera.** Po završenom prevođenju CLI pokreće razvojni veb server koji osluškuje na adresi `localhost:4200`. Server drži prevedene datoteke i isporučuje ih internet čitaču kada ih zatraži. U razvoju se te datoteke drže u memoriji i ne upisuju na disk, pa u projektu nećemo videti nov direktorijum sa rezultatom prevođenja. Komanda `ng build`, koja se koristi za pripremu aplikacije za objavljivanje, rezultat upisuje u direktorijum `dist/`.

**3. Učitavanje stranice u internet čitaču.** Kada u internet čitaču otvorimo `localhost:4200`, server vraća datoteku `index.html`, tj. jedinu HTML stranicu koju internet čitač učitava. Njeno telo sadrži jedan element:

```html
<body>
  <app-root></app-root>
</body>
```

U isporučeni `index.html` CLI je ubacio i `<script>` i `<link>` oznake koje pokazuju na objedinjene JavaScript i CSS datoteke, iako ih u izvornoj datoteci ne vidimo. Internet čitač te datoteke zatim učitava, a JavaScript kod izvršava.

**4. Pokretanje Angular aplikacije.** Izvršavanjem objedinjenog JavaScript koda pokreće se kod iz datoteke `main.ts`, ulazne tačke aplikacije:

```ts
bootstrapApplication(App, appConfig);
```

Poziv `bootstrapApplication` prima klasu korenske komponente (`App`) i konfiguraciju aplikacije (`appConfig` iz `app.config.ts`). Od tog trenutka Angular upravlja sadržajem stranice.

**5. Iscrtavanje korenske komponente.** Element `app-root` nije standardan HTML element i internet čitač ga sam ne poznaje. Angular ga pri pokretanju ne uklanja, već unutar njega iscrtava šablon korenske komponente. Korisnik tada vidi ono što je u tom šablonu opisano.

**6. Osvežavanje pri izmenama (engl. *live reload*).** Razvojni server nastavlja da radi i prati izvorne datoteke. Čim sačuvamo izmenu, on ponovo prevodi samo ono što se promenilo i osvežava stranicu u internet čitaču, pa rezultat vidimo bez ručnog ponovnog pokretanja.

## Korenska komponenta

**Komponenta** (engl. *component*) je osnovni gradivni blok Angular aplikacije: TS klasa sa pridruženim HTML šablonom i stilovima koja upravlja jednim delom stranice. Klasa čuva podatke i logiku (šta se prikazuje i kako se reaguje na akcije), a šablon opisuje izgled (kako se to iscrtava). Stranicu gradimo od jedne ili više komponenti. Na primer, jedna prikazuje zaglavlje, druga spisak tura, treća formu. Korenska komponenta je prva koju Angular iscrtava i unutar sebe smešta sve ostale.

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
- Zapis `@Component({ ... })` je dekorator. **Dekorator** (engl. *decorator*) je oznaka iznad klase kojom radni okvir prepoznaje ulogu klase, slično kao atribut `[ApiController]` na serveru. Objekat u zagradi nosi podešavanja komponente.
- `selector` određuje naziv HTML elementa pod kojim se komponenta koristi. Zato `index.html` sadrži element `app-root`.
- `templateUrl` i `styleUrl` vezuju klasu za šablon i za datoteku stilova.
- Šablon vidi samo članove komponente kojoj pripada. Polja druge komponente nisu mu dostupna.
