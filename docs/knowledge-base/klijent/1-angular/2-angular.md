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
  src/
    index.html          Jedina HTML stranica aplikacije
    main.ts             Ulazna tačka
    styles.scss         Globalni stilovi
    app/
      app.config.ts     Konfiguracija aplikacije
      app.ts            Korenska komponenta
      app.html          Šablon korenske komponente
      app.scss          Stilovi korenske komponente
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
  ],
};
```

Stavka `provideBrowserGlobalErrorListeners` je podrazumevana stavka koju `ng new` upisuje. Ostavljamo je i ne bavimo se njome. Svaki deo radnog okvira koji aplikacija koristi uključuje se ovde, a kod aplikacije ga zatim koristi bez daljeg podešavanja. Ova datoteka je klijentski pandan datoteci `Program.cs`.

## Korenska komponenta

**Komponenta** (engl. *component*) je klasa sa pridruženim HTML šablonom koja upravlja jednim delom stranice. Korenska komponenta upravlja celom stranicom. Sledeći kod prikazuje njenu klasu:

```ts
@Component({
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
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
- Podešavanja `templateUrl` i `styleUrl` vezuju klasu za šablon i za datoteku stilova. Tri datoteke istog naziva, sa nastavcima `.ts`, `.html` i `.scss`, čine jednu komponentu.
- Klasa je prazna, jer šablon ne prikazuje nijedan podatak iz nje. Sve što korisnik vidi upisano je u šablon.

## Od pokretanja do prikaza

Kada korisnik otvori adresu `localhost:4200`, dešava se sledeće:
1. Pregledač učitava `index.html` i u njemu nalazi element `app-root`.
2. Izvršava se `main.ts`, koji poziva `bootstrapApplication` sa klasom `App` i konfiguracijom iz `app.config.ts`.
3. Angular pravi korensku komponentu i njenim šablonom zamenjuje element `app-root`.
4. Korisnik vidi zaglavlje i poruku iz šablona korenske komponente.

## Pokretanje

Komanda `npm start` prevodi aplikaciju i pokreće razvojni server na adresi `localhost:4200`. Nakon svake izmene datoteke razvojni server ponovo prevodi aplikaciju i osvežava stranicu u pregledaču.

Greške se pojavljuju na dva mesta. Greške prevođenja, poput prekršene anotacije tipa, prijavljuje terminal u kom je pokrenut `npm start`. Greške pri izvršavanju prijavljuje konzola pregledača.
