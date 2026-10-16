Klijentski deo našeg projekta napravljen je u radnom okviru **Angular**, koji kao zvanični jezik koristi TypeScript. **TypeScript (TS)** je jezik koji proširuje JavaScript (JS), prvenstveno tipovima. Svaki ispravno napisan JS kod sintaksno je ispravan i u TS-u, ali TS dodatno proverava da li se tipovi vrednosti poklapaju. Internet čitač razume samo JS, pa se TS kod pre izvršavanja prevodi u JS. Prevodilac najpre proverava tipove i prijavljuje greške, a zatim iz koda uklanja sve što je specifično za TS, kao što su anotacije, interfejsi i modifikatori. Internet čitač na kraju dobija običan JS.

U nastavku upoznajemo osnove TS-a koje su neophodne za čitanje Angular projekta. Za početak razmotrimo jednu TS klasu koja čuva spisak tura i identifikator izabrane ture:

```ts
export class TourList {
  readonly tours: TourDto[] = [];
  private selectedTourId: string | null = null;

  add(tour: TourDto): void {
    this.tours.push(tour);
  }

  select(tourId: string): void {
    this.selectedTourId = tourId;
  }

  remove(tourId: string): void { /* ... */ }

  removeSelected(): void {
    const tourId = this.selectedTourId;
    if (tourId === null) {
      return;
    }
    this.remove(tourId);
  }
}
```

Čitalac koji poznaje JS prepoznaje klasu, polja i metode, kao i naredbe u telima metoda. Ipak, u klasi vidimo i novine: reči `readonly` i `private` ispred polja, `: TourDto[]` i `: string | null` iza naziva polja, `: string` i `: TourDto` iza parametara i `: void` iza liste parametara.

## Anotacija tipa

**Anotacija tipa** (engl. *type annotation*) je oznaka oblika `: tip` koja se piše iza naziva promenljive, polja ili parametra, kao i iza liste parametara metode, gde označava tip povratne vrednosti. Anotacijom tipa prevodiocu saopštavamo kog tipa je vrednost. Osnovni tipovi su `string`, `number` i `boolean`.

Sledeći kod prikazuje metodu sa anotiranim parametrom i povratnom vrednošću:

```ts
select(tourId: string): void {
  this.selectedTourId = tourId;
}
```

U datom kodu treba uočiti sledeće:
- Anotacija `tourId: string` znači da poziv `select(5)` ne prolazi prevođenje, jer je `5` broj, a ne tekst.
- Anotacija `: void` iza liste parametara znači da metoda ništa ne vraća. Ako u telu metode napišemo `return` sa vrednošću, prevodilac prijavljuje grešku.

### Zaključivanje tipa

Anotaciju nije uvek potrebno pisati. Kada promenljiva ili polje dobije vrednost odmah pri deklaraciji, prevodilac sam zaključuje tip iz te vrednosti. Ovaj mehanizam zovemo **zaključivanje tipa** (engl. *type inference*).

```ts
let count = 0;   // prevodilac zaključuje tip number
count = 'pet';   // greška: tekst nije broj
```

Polja iz uvodnog primera ipak imaju anotacije, jer njihove početne vrednosti ne govore dovoljno. Iz praznog niza `[]` prevodilac ne može da zna kakvi će elementi biti u nizu. Iz početne vrednosti `null` prevodilac ne može da zna da će polje `selectedTourId` kasnije čuvati tekst.

## Nepostojeća vrednost

JS ima dve vrednosti koje označavaju odsustvo vrednosti:
- `undefined` označava da vrednost nije dodeljena. Tu vrednost ima promenljiva kojoj nismo dodelili vrednost, kao i element niza koji ne postoji.
- `null` označava da vrednost namerno ne postoji. Ovu vrednost uvek dodeljujemo sami.

U JS-u bilo koja promenljiva može da sadrži `null` ili `undefined`. U TS-u to nije slučaj, jer promenljiva tipa `string` može da sadrži samo tekst. Kada vrednost može da nedostaje, to moramo eksplicitno da navedemo pomoću **unije tipova** (engl. *union type*). Unija se piše uspravnom crtom `|` i znači da vrednost može biti bilo kog od navedenih tipova. Tip `string | null` čitamo kao „tekst ili `null`“.

Sledeći kod prikazuje polje klase `TourList` koje čuva identifikator izabrane ture i metodu koja ga koristi:

```ts
private selectedTourId: string | null = null;

removeSelected(): void {
  const tourId = this.selectedTourId;
  if (tourId === null) {
    return;
  }
  this.remove(tourId);
}
```

U datom kodu treba uočiti sledeće:
- Polje `selectedTourId` čuva identifikator izabrane ture ili `null`. Početna vrednost je `null`, jer nijedna tura još nije izabrana.
- Konstanta `tourId` je tipa `string | null`, a metoda `remove` prima `string`. Zato prevodilac ne dozvoljava poziv `this.remove(tourId)` sve dok proverom ne isključimo `null`. Nakon provere `if (tourId === null)` sa naredbom `return`, prevodilac zna da je preostali tip `string`. Ovaj mehanizam zovemo **sužavanje tipa** (engl. *type narrowing*).

## Interfejs

TS **interfejs** (engl. *interface*) opisuje oblik objekta, tj. koja polja objekat mora da sadrži i kog su tipa. Interfejse najčešće koristimo za model podataka na klijentskom delu veb aplikacije, koji odgovara podacima koji stižu sa servera. Takav model obično nazivamo **DTO** (engl. *Data Transfer Object*), otuda i naziv `TourDto`.

Pre samog interfejsa, uvodimo još jedan oblik unije. Kada tip treba da bude „jedna od nekoliko tačno određenih tekstualnih vrednosti“, koristimo **imenovanu uniju literala** i dajemo joj naziv ključnom rečju `type`. Tako u projektu, umesto običnog `string`, težina ture ima svoj tip:

```ts
export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';
export type TourStatus = 'Draft' | 'Published';
export type TransportMode = 'Walking' | 'Bicycle' | 'Car';
```

Vrednost tipa `TourDifficulty` može biti samo `'Easy'`, `'Moderate'` ili `'Hard'`, svaki drugi tekst prevodilac odbija. Time tip precizno opisuje šta je dozvoljeno, pa grešku hvatamo pri prevođenju, a ne u izvršavanju.

Sledi interfejs koji opisuje turu, onako kako je definisan u početnom projektu (datoteka `modules/exploration/api/exploration-api-types.ts`):

```ts
export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  transportTimes: TransportTimeDto[];
}
```

Jedan objekat koji odgovara ovom interfejsu:

```ts
const tour: TourDto = {
  id: 't-1',
  authorId: 'u-42',
  name: 'Fruška gora',
  description: 'Planinarenje do manastira.',
  difficulty: 'Hard',
  tags: ['priroda', 'šetnja'],
  status: 'Draft',
  publishedAt: null,
  transportTimes: [],
};
```

U datom kodu treba uočiti sledeće:

- Polje `difficulty` nije običan `string`, već `TourDifficulty`, pa je `'Hard'` ispravna vrednost, dok bi `'Teško'` bila greška. Isto važi za `status`.
- Polje `transportTimes` je niz ugneždenih objekata tipa `TransportTimeDto`. Interfejs tako može da opiše i objekat sastavljen od drugih objekata.
- Sva navedena polja su obavezna. Ako izostavimo neko od njih, npr. `difficulty`, prevodilac prijavljuje grešku. Isto važi i ako navedemo polje koje interfejs ne poznaje.
- Polje `publishedAt` je tipa `string | null`, pa mora biti navedeno, ali sme imati vrednost `null`. Tura koja još nije objavljena ima `publishedAt: null`.
- Ključna reč `export` omogućava da se interfejs (i tip) koristi i u drugim datotekama.

## Generički tip

Tip niza tura iz uvodnog primera, `TourDto[]`, može se zapisati i kao `Array<TourDto>`. Ovaj zapis sa izlomljenim zagradama `<...>` pripada **generičkom tipu** (engl. *generic type*), gde vrednost u zagradama određuje tip sa kojim se radi. Tip `Array<TourDto>` je niz tura, a `Array<string>` niz tekstova.

Generički tip omogućava da napišemo funkcije, klase ili interfejse koji rade sa različitim tipovima podataka, a da prevodilac i dalje proverava tipove. Umesto da unapred „zakucamo“ jedan tip (npr. `string` ili `number`), koristimo **tipski parametar** (najčešće se zove `T`), koji prevodilac popunjava konkretnim tipom tek kada se funkcija pozove ili klasa upotrebi.

Sledeći kod prikazuje funkciju koja vraća prvi element niza:

```ts
function first<T>(items: T[]): T | undefined {
  return items[0];
}

const tours: TourDto[] = [];
const firstTour = first(tours);       // T je TourDto -> TourDto | undefined
const firstTag = first(['a', 'b']);   // T je string  -> string | undefined
```

U datom kodu treba uočiti sledeće:
- `<T>` iza naziva funkcije uvodi tipski parametar, koji povezuje tip ulaza (`items: T[]`) sa tipom izlaza (`T | undefined`). Kog god tipa da su elementi niza, funkcija vraća vrednost istog tog tipa.
- Povratni tip je unija `T | undefined`, jer niz može biti prazan, a tada `items[0]` ima vrednost `undefined`.
- Tip `T` ne navodimo pri pozivu, već ga prevodilac zaključuje iz argumenta. Za niz `tours` to je `TourDto`, a za `['a', 'b']` je `string`. Tip je moguće navesti i eksplicitno, npr. `first<string>(['a', 'b'])`, ali to je retko potrebno.

## Modifikatori članova klase

U JS-u su polja i metode klase podrazumevano dostupni svakom kodu koji ima pristup objektu. TS dodaje **modifikatore pristupa** (engl. *access modifiers*), kojima ograničavamo ko sme da koristi član klase:
1. `public` – član je dostupan svuda. Ovo je podrazumevano ponašanje, pa se `public` retko piše.
2. `private` – član je dostupan samo kodu unutar klase.

Pored njih postoji i modifikator `readonly`, koji ne ograničava pristup, već izmenu. Polju označenom sa `readonly` vrednost se dodeljuje jednom, pri deklaraciji ili u konstruktoru, i posle se ne može promeniti.

Sledeći kod prikazuje polja klase `TourList` sa modifikatorima i kod van klase koji ih koristi:

```ts
export class TourList {
  readonly tours: TourDto[] = [];
  private selectedTourId: string | null = null;
  // ...
}

const list = new TourList();
list.select('t-1');            // ispravno
list.selectedTourId = 't-1';   // greška: polje je private
list.tours.push(tour);         // ispravno
list.tours = [];               // greška: polje je readonly
```

U datom kodu treba uočiti sledeće:
- Sva polja dobijaju vrednost odmah pri deklaraciji, pa klasi nije potreban konstruktor.
- Polje `tours` i sve metode nemaju modifikator pristupa, pa su dostupni svuda.
- Polje `selectedTourId` je `private`, pa kod van klase izabranu turu menja samo pozivom metode `select`.
- Modifikator `readonly` sprečava da se polju `tours` dodeli novi niz, ali ne sprečava izmenu postojećeg niza metodom `push`.
