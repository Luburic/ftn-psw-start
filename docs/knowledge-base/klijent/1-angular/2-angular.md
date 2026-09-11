# Angular

Svaka klijentska aplikacija rešava iste tehničke probleme, nezavisno od toga čemu služi. Mora da:
1. Prikaže podatke u HTML dokumentu.
2. Osveži prikaz kada se podaci promene.
3. Reaguje na akcije korisnika.
4. Po adresi u internet čitaču odluči koju stranicu prikazuje.
5. Razmenjuje podatke sa serverom.

Biblioteke po pravilu rešavaju samo deo ovih problema, pa programer bira i povezuje više njih, zbog čega dve aplikacije retko imaju istu strukturu (čest slučaj sa React-om). Angular rešava svih pet problema i uz to propisuje strukturu aplikacije.

**Angular** je radni okvir za izgradnju klijentskih veb aplikacija u jeziku TypeScript. U nastavku prolazimo kroz minimalnu Angular aplikaciju dovoljnu za rad, datoteku po datoteku.

## Priprema okruženja

Da bi se Angular projekat pokrenuo na tvom računaru, neophodno je da instaliraš:

1. **Node.js** — okruženje koje izvršava JavaScript van internet čitača. Na njemu rade Angular CLI i ostali alati iz projekta (provera: `node --version`). Svaka verzija Angulara podržava tačno određene verzije Node.js-a, pa verziju treba proveriti u tabeli kompatibilnosti na zvaničnom sajtu angular.dev.
2. **npm** — menadžer paketa, stiže uz Node.js (provera: `npm --version`).
3. **Angular CLI** — alat komandne linije radnog okvira, instalira se jednom, globalno: `npm install -g @angular/cli` (provera: `ng --version`).

## Struktura radnog prostora

Komanda `ng new [naziv-projekta]` kreira novi projekat. Kreiranjem novog projekta dobijamo skup datoteka i direktorijuma koji čine naš projekat, a koji su opisani u nastavku.

Iako datoteke van `src` direktorijuma retko menjamo, vredi znati čemu služe:

1. <b>.editorconfig</b> je datoteka koja definiše pravila formatiranja koda. Datoteka je deo repozitorijuma, pa je svi članovi tima imaju istu. Editor koda koji podržava EditorConfig (WebStorm ugrađeno, VS Code uz dodatak EditorConfig) automatski primenjuje ta pravila, pa svi formatiraju kod na isti način i git ne prijavljuje razlike nastale samo zbog formatiranja.
2. <b>.gitignore</b> sadrži putanje do datoteka koje git ne treba da prati i koje se ne šalju na repozitorijum. Obratiti pažnju da je i `node_modules` (folder koji sadrži sve biblioteke potrebne za pokretanje našeg projekta) u `.gitignore`, stoga je potrebno uvek nakon pull-a projekta pokrenuti `npm install` kako bi se instalirale sve potrebne biblioteke u okviru `node_modules` foldera.
3. <b>angular.json</b> - datoteka za konfiguraciju Angular projekta. Sadrži podešavanja za komande `ng build`, `ng serve` i `ng test`, npr. putanje do globalnih stilova i statičkih datoteka i direktorijum u koji se smešta rezultat prevođenja.
4. <b>package-lock.json</b> - beleži tačne verzije svih instaliranih biblioteka, uključujući i biblioteke od kojih one zavise. Zahvaljujući njoj svi članovi tima dobijaju identične verzije.
5. <b>package.json</b> - sadrži spisak biblioteka koje projekat koristi, podeljen u dve grupe. `dependencies` su biblioteke koje aplikacija koristi dok radi i koje ulaze u prevedeni kod (npr. `@angular/core`). `devDependencies` su alati potrebni samo tokom razvoja (npr. Angular CLI i TypeScript prevodilac). Datoteka sadrži i odeljak `scripts` sa skraćenicama za česte komande. Na primer, tu je definisano da `npm start` poziva `ng serve`.
6. <b>README.md</b> - najčešće opisuje korake koji su potrebni za pokretanje projekta.
7. <b>tsconfig.app.json/tsconfig.json</b> - konfiguracija TypeScript-a. Ovde je uključen strogi režim (`"strict": true`), u kom prevodilac strože proverava kod, npr. ne dozvoljava `null` tamo gde tip to ne navodi.
8. <b>node_modules</b> - folder u okviru kog su instalirane sve biblioteke za naš projekat.

Sa druge strane, datoteke u okviru `src` direktorijuma ćemo intenzivno menjati i dodavati:

1. <b>index.html</b> - korenska HTML stranica aplikacije, jedina koju internet čitač učitava.
2. <b>main.ts</b> - ulazna tačka od koje kreće izvršavanje aplikacije; pokreće Angular aplikaciju.
3. <b>styles.css</b> - globalni stilovi koji važe za celu aplikaciju.
4. <b>app/</b> - kod same aplikacije: komponente, servisi i njihova konfiguracija. U okviru ovog direktorijuma se nalaze:
    - <b>app.config.ts</b> - konfiguracija aplikacije: delovi radnog okvira koji se uključuju pri pokretanju.
    - <b>app.ts</b> - korenska komponenta, prva koju Angular iscrtava.
    - <b>app.html</b> - šablon korenske komponente (šta se prikazuje).
    - <b>app.css</b> - stilovi korenske komponente.

Pored navedenih, novi projekat sadrži i nekoliko datoteka koje za sada možemo zanemariti, npr. `app.routes.ts` (definicija ruta, tj. koja stranica se prikazuje za koju adresu), `app.spec.ts` (test korenske komponente) i direktorijum `public/` (statičke datoteke, npr. ikonica sajta).

## Preuzimanje i pokretanje projekta

Kod preuzimamo kloniranjem repozitorijuma, a zatim se u korenu projekta pokreće:

1. `npm install` čita `package.json` i preuzima sve biblioteke u direktorijum `node_modules/`.
2. `ng serve` (ili `npm start`, koji ga najčešće samo poziva) prevodi aplikaciju i pokreće razvojni server na adresi `localhost:4200`. Šta se pri tome tačno dešava opisano je u narednom odeljku „Pokretanje projekta“.

**Napomena:** Direktorijum `node_modules/` naveden je u `.gitignore` i ne šalje se na repozitorijum. Zato posle svakog preuzimanja tuđih izmena (`git pull`) treba ponovo pokrenuti `npm install`, kako bi se lokalno instalirale eventualne nove biblioteke koje su saradnici dodali.

## Pokretanje projekta

Projekat pokrećemo iz njegovog korena komandom `ng serve` (ili `npm start`, koji je najčešće samo poziva). Ta komanda pokreće niz koraka koji izvorni kod pretvaraju u aplikaciju koju internet čitač ume da prikaže. U nastavku pratimo taj put, od komande do prikaza na ekranu.

**1. Prevođenje i objedinjavanje (engl. *build*).** Internet čitač ne razume TypeScript ni SCSS, već samo JavaScript, CSS i HTML. Zato Angular CLI prvo prevodi aplikaciju: TypeScript kod (`.ts`) u JavaScript, Angular šablone i dekoratore (`@Component`) u JavaScript koji ume da iscrta prikaz, a SCSS stilove (`.scss`) u CSS. Sav taj kod, zajedno sa delovima biblioteka iz `node_modules/` koje aplikacija koristi, CLI potom objedinjuje u mali broj JavaScript i CSS datoteka. Ovaj korak se naziva **objedinjavanje** (engl. *bundling*), a dobijene datoteke zovemo **objedinjene datoteke** (engl. *bundles*).

**2. Pokretanje razvojnog servera.** Po završenom prevođenju CLI pokreće razvojni veb server koji osluškuje na adresi `localhost:4200`. Server drži prevedene datoteke i isporučuje ih internet čitaču kada ih zatraži. U razvoju se te datoteke drže u memoriji i ne upisuju na disk, pa u projektu nećemo videti novi direktorijum sa rezultatom prevođenja. (Komanda `ng build`, koja se koristi za pripremu aplikacije za objavljivanje, rezultat upisuje u direktorijum `dist/`.)

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

Ukratko:
1. `ng serve` prevodi i objedinjuje aplikaciju, pa pokreće razvojni server na `localhost:4200`.
2. Internet čitač otvara tu adresu i dobija `index.html` sa elementom `app-root` i ubačenim `<script>` i `<link>` oznakama.
3. Internet čitač učitava objedinjene datoteke, izvršava se `main.ts`, koji poziva `bootstrapApplication(App, appConfig)`.
4. Angular pravi korensku komponentu i njen šablon iscrtava unutar elementa `app-root`.
5. Korisnik vidi pokrenutu aplikaciju, a svaka naredna izmena koda automatski osvežava prikaz.

Kao vežbu možeš kreirati jedan Angular projekat komandom `ng new [naziv-projekta]`, pokrenuti ga komandom `ng serve`, a potom kroz internet čitač proveriti šta se nalazi na `localhost:4200`.

## Korenska komponenta

**Komponenta** (engl. *component*) je osnovni gradivni blok Angular aplikacije: TS klasa sa pridruženim HTML šablonom i stilovima koja upravlja jednim delom stranice. Klasa čuva podatke i logiku (šta se prikazuje i kako se reaguje na akcije), a šablon opisuje izgled (kako se to iscrtava). Stranicu gradimo od jedne ili više komponenti. Na primer, jedna prikazuje zaglavlje, druga spisak tura, treća formu. Korenska komponenta je prva koju Angular iscrtava i unutar sebe smešta sve ostale.

Komponentu čine tri datoteke istog naziva: `.ts` (klasa), `.html` (šablon) i `.css` (stilovi). Sledeći kod prikazuje klasu korenske komponente:

```ts
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
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

## Podatak iz klase u šablonu

Prazna klasa retko je korisna. Komponenta obično čuva podatke koje prikazuje. Vrednost iz klase u šablon prenosimo zapisom `{{ ... }}`, koji nazivamo **interpolacija** (engl. *interpolation*). Sledeći kod dodaje polje u klasu:

```ts
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
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
- Zapis `{{ title }}` u šablonu zamenjuje se vrednošću polja `title` iz klase. U internet čitaču se prikazuje `Explorer`.
- Polje `title` je `protected`, jer ga koristi šablon. Šablon može da koristi `public` i `protected` članove klase, ali ne i `private` članove. Zato članove koje šablon koristi označavamo sa `protected`.
- Šablon vidi samo članove komponente kojoj pripada. Polja druge komponente nisu mu dostupna.

Naredna lekcija će detaljno razraditi koncept komponente.
