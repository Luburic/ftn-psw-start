# TypeScript

Klijentski deo našeg projekta je kreiran kroz **Angular** radni okvir koji zvanično koristi **TypeScript (TS)** jezik. U nastavku ćete upoznati osnove TS programskog jezika koje su neophodne da biste mogli da čitate Angular projekat.

Za početak razmotrimo jednu TS klasu koja drži spisak tura i izabranu turu:

```ts
export class TourList {
  private readonly tours: TourDto[] = [];
  protected selectedTourId: string | null = null;

  protected select(tourId: string): void {
    this.selectedTourId = tourId;
  }

  protected async publish(tourId: string): Promise<void> {
    await fetch(`/api/exploration/tours/${tourId}/publish`, { method: 'POST' });
  }
}
```

Čitalac koji poznaje JavaScript (JS) prepoznaje klasu, polja, metode, `async`, `await`, kao i poziv `fetch`. Međutim u klasi vidimo i novine poput `private`, `protected` i `readonly` ispred polja, `: string` iza parametra, `: Promise<void>` iza liste parametara i `: TourDto[]` iza naziva polja.

TS je jezik koji proširuje JS prvenstveno tipovima ali i drugim mehanizmima. Sve što važi u JSu važi i u TSu, ali uz dodatne mogućnosti koje TS uvodi. Internet čitač (engl. browser) i dalje isključivo samo razume JS, što znači da se TS transpajlira u JS pre nego što ga internet čitač obradi.

## Anotacija tipa

**Anotacija tipa** (engl. *type annotation*) je oznaka oblika `: tip` iza naziva polja, parametra itd. koja prevodiocu saopštava kog tipa je vrednost. Osnovni tipovi su `string`, `number` i `boolean`. Metoda koja ne vraća vrednost ima povratni tip `void`.

Sledeći kod prikazuje metodu sa anotiranim parametrom i povratnom vrednošću:

```ts
protected select(tourId: string): void {
  this.selectedTourId = tourId;
}
```

U datom kodu treba uočiti sledeće:
- Anotacija `tourId: string` znači da poziv `select(5)` ne prolazi prevođenje, jer je `5` broj, a ne tekst.
- Anotacija `: void` znači da metoda ništa ne vraća. Prevodilac prijavljuje grešku ako u telu metode napišemo `return` sa vrednošću.

## Interfejs

TS **interfejs** definiše strukturu koju objekat mora da ima tj. koja polja mora sadržati. Interfejse najčešće koristimo kada definišemo model podataka na klijentskom delu veb aplikacije, koji odgovara podacima koji stižu sa servera.

Sledi primer interfejsa za osobu:

```typescript
interface Person {
    name: string;
    surname: string;
    age: number;
    previousJobs: string[];
    greet: (prefix: string) => string;
}

const pera: Person = {
    name: 'Pera',
    surname: 'Peric',
    age: 34,
    previousJobs: ['Programmer', 'Teacher', 'Farmer'],
    greet(prefix) { 
        return `${prefix} ${this.name} ${this.surname}`;
    }
}
```
Interesantan je red sa `greet`, gde se ističe da objekat koji odgovara ovom interfejsu mora da ima metodu `greet`, koja prihvata string (prefix) i vraća string.

Dodatno možemo specijalnim karakterom ```?``` naglasiti da neko polje interfejsa ne mora biti prisutno u samom objektu:

```typescript
interface Person {
    name: string;
    surname: string;
    age?: number;
    previousJobs?: string[];
    greet: (prefix: string) => string;
}

const pera: Person = {
    name: 'Pera',
    surname: 'Peric', // nije definisan 'age' ni 'previousJobs'
    greet(prefix) { 
        return `${prefix} ${this.name} ${this.surname}`;
    }
}
```

## Generički tip

Generici (engl. *generics*) predstavljaju mehanizam koji omogućava da kreiramo funkcije, klase ili interfejse koje rade sa bilo kojim tipom podataka, ali na siguran način. Umesto da unapred „zakucamo" jedan tip (npr. `string` ili `number`), koristimo *placeholder* tj. tipski parametar (najčešće `T`) koji prevodilac popuni tek kada se funkcija ili klasa pozove. Oznaku smo već sreli u `Promise<void>` i `Array<TourDto>`. Vrednost u zagradama `<...>` određuje tip sa kojim se radi.

Sledeći kod prikazuje funkciju koja vraća prvi element niza:

```ts
function prvi<T>(niz: T[]): T | undefined {
  return niz[0];
}

const tura = prvi(tours);        // T je TourDto  ->  TourDto | undefined
const naziv = prvi(['a', 'b']);  // T je string   ->  string | undefined
```

U datom kodu treba uočiti sledeće:
- `<T>` uvodi tipski parametar koji povezuje ulaz (`niz: T[]`) i izlaz (`T | undefined`).
- `T` ne pišemo pri pozivu već ga prevodilac sam zaključuje iz argumenta (za `tours` je `TourDto`, za niz stringova `string`).

## Nepostojeća vrednost

U JavaScript-u svaka promenljiva može da bude `null`. U TypeScript-u promenljiva tipa `string` ne sme da bude `null`. Kada vrednost može da nedostaje, njen tip pišemo kao uniju sa `null`, oblika `string | null`, koju čitamo kao tekst ili ništa. Sledeći kod prikazuje polje koje čuva identifikator izabrane ture i metodu koja ga čita:

```ts
protected selectedTourId: string | null = null;

protected async publishSelected(): Promise<void> {
  const tourId = this.selectedTourId;
  if (tourId === null) {
    return;
  }
  await this.publish(tourId);
}
```

U datom kodu treba uočiti sledeće:
- Anotacija `: string | null` kaže da polje čuva tekst ili `null`. Početna vrednost je `null`, jer nijedna tura još nije izabrana.
- Vrednost `tourId` je tipa `string | null`, a metoda `publish` prima `string`. Prevodilac odbija poziv sve dok se `null` ne isključi proverom. Nakon naredbe `if (tourId === null) return;` prevodilac zna da je preostali tip `string`.

## Modifikatori klase

JS klasa ima polja i metode koji su svima dostupni. TS dodaje modifikatore koji ograničavaju pristup i izmenu:
1. `private` znači da je član vidljiv samo kodu unutar klase.
2. `protected` znači da je član vidljiv kodu unutar klase i kodu klasa koje je nasleđuju.
3. `readonly` znači da se polje dodeljuje jednom i više ne menja.

Sledeći kod prikazuje jednu klasu sa modifikatorima pristupa:

```ts
export class TourList {
  private readonly tours: TourDto[] = [];
  protected selectedTourId: string | null = null;
}
```

U datom kodu treba uočiti sledeće:
- Polja dobijaju vrednost odmah pri deklaraciji, pa klasa nema konstruktor.
- Polje `tours` je `readonly`. Dodavanje elementa u taj niz `readonly` ne sprečava, jer se time menja sadržaj niza, a ne polje.
