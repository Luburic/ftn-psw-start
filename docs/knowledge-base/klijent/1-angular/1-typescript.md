Klijentski deo našeg projekta napravljen je u radnom okviru **Angular**, koji kao zvanični jezik koristi **TypeScript (TS)**. U nastavku upoznajemo osnove TS-a koje su neophodne za čitanje Angular projekta.

Za početak razmotrimo jednu TS klasu koja čuva spisak tura i identifikator izabrane ture:

```ts
export class TourList {
  private readonly apiUrl = '/api/exploration/tours';
  protected readonly tours: TourDto[] = [];
  protected selectedTourId: string | null = null;

  protected select(tourId: string): void {
    this.selectedTourId = tourId;
  }

  protected async publish(tourId: string): Promise<void> {
    await fetch(`${this.apiUrl}/${tourId}/publish`, { method: 'POST' });
  }
}
```

Čitalac koji poznaje JavaScript (JS) prepoznaje klasu, polja, metode, ključne reči `async` i `await`, kao i poziv funkcije `fetch`. Ipak, u klasi vidimo i novine: reči `private`, `protected` i `readonly` ispred polja i metoda, `: string` iza parametra, `: Promise<void>` iza liste parametara i `: TourDto[]` iza naziva polja.

TS je jezik koji proširuje JS, prvenstveno tipovima. Svaki ispravno napisan JS kod sintaksno je ispravan i u TS-u, ali TS dodatno proverava da li se tipovi vrednosti poklapaju. Internet čitač (engl. *browser*) razume samo JS, pa se TS kod pre izvršavanja prevodi u JS. Alat koji to radi zovemo **prevodilac** (engl. *compiler*). Prevodilac najpre proverava tipove i prijavljuje greške, a zatim iz koda uklanja sve što je specifično za TS, kao što su anotacije, interfejsi i modifikatori. Internet čitač na kraju dobija običan JS.

## Anotacija tipa

**Anotacija tipa** (engl. *type annotation*) je oznaka oblika `: tip` koja se piše iza naziva promenljive, polja ili parametra, kao i iza liste parametara metode. Anotacijom tipa prevodiocu saopštavamo kog tipa je vrednost. Osnovni tipovi su `string`, `number` i `boolean`. Povratni tip metode koja ne vraća vrednost je `void`.

Sledeći kod prikazuje metodu sa anotiranim parametrom i povratnom vrednošću:

```ts
protected select(tourId: string): void {
  this.selectedTourId = tourId;
}
```

U datom kodu treba uočiti sledeće:
- Anotacija `tourId: string` znači da poziv `select(5)` ne prolazi prevođenje, jer je `5` broj, a ne tekst.
- Anotacija `: void` iza liste parametara znači da metoda ništa ne vraća. Ako u telu metode napišemo `return` sa vrednošću, prevodilac prijavljuje grešku.

### Automatsko određivanje tipa

Anotaciju nije uvek potrebno pisati. Kada promenljiva ili polje dobije vrednost odmah pri deklaraciji, prevodilac sam zaključuje tip iz te vrednosti. Ovaj mehanizam zovemo **zaključivanje tipa** (engl. *type inference*).

```ts
let count = 0;   // prevodilac zaključuje tip number
count = 'pet';   // greška: tekst nije broj
```

Zato polje `apiUrl` iz uvodnog primera nema anotaciju, jer prevodilac iz vrednosti vidi da je u pitanju tekst. Sa druge strane, polje `tours` mora da ima anotaciju, jer iz praznog niza `[]` prevodilac ne može da zna kakvi će elementi biti u nizu. Anotaciju pišemo i kod parametara metode, jer njihov tip prevodilac ne može da zaključi.

## Interfejs

TS **interfejs** (engl. *interface*) opisuje oblik objekta, tj. koja polja objekat mora
da sadrži i kog su tipa. Interfejse najčešće koristimo za model podataka na klijentskom
delu veb aplikacije, koji odgovara podacima koji stižu sa servera. Takav model obično
nazivamo **DTO** (engl. *Data Transfer Object*), otuda i naziv `TourDto`.

Pre samog interfejsa, uvodimo još jedan alat. Kada tip treba da bude „jedna od nekoliko
tačno određenih tekstualnih vrednosti“, koristimo **imenovanu uniju literala** i dajemo
joj naziv ključnom rečju `type`. Tako u projektu, umesto običnog `string`, težina ture
ima svoj tip:

```ts
export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';
export type TourStatus = 'Draft' | 'Published';
export type TransportMode = 'Walking' | 'Bicycle' | 'Car';
```

Vrednost tipa `TourDifficulty` može biti samo `'Easy'`, `'Moderate'` ili `'Hard'`,
svaki drugi tekst prevodilac odbija. Time tip precizno opisuje šta je dozvoljeno, pa
grešku hvatamo pri prevođenju, a ne u izvršavanju.

Sledi interfejs koji opisuje turu, tačno onako kako je definisan u projektu (datoteka
`modules/exploration/api/exploration-api-types.ts`):

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

- Polje `difficulty` nije običan `string`, već `TourDifficulty`, pa je `'Hard'` ispravna
  vrednost, dok bi `'Teško'` bila greška. Isto važi za `status`.
- Polje `transportTimes` je niz ugneždenih objekata tipa `TransportTimeDto`. Interfejs
  tako može da opiše i objekat sastavljen od drugih objekata.
- Sva navedena polja su obavezna. Ako izostavimo neko od njih, npr. `difficulty`,
  prevodilac prijavljuje grešku. Isto važi i ako navedemo polje koje interfejs ne
  poznaje.
- Polje `publishedAt` je tipa `string | null` (tekst ili `null`), pa mora biti navedeno,
  ali sme imati vrednost `null`. Tura koja još nije objavljena ima `publishedAt: null`.
- Ključna reč `export` omogućava da se interfejs (i tip) koristi i u drugim datotekama.

### Opciono polje

Postoji i način da polje bude potpuno neobavezno. Znak `?` iza naziva polja označava
**opciono polje**, koje objekat ne mora da sadrži:

```ts
interface Draft {
  title: string;
  note?: string;   // objekat ovo polje ne mora da ima
}

const d: Draft = { title: 'Bez beleške' };   // ispravno
```

Opciono polje `note?: string` zapravo je tipa `string | undefined`, o čemu više govorimo
u narednom odeljku.

> **Napomena o projektu:** U našim DTO-ima odsustvo vrednosti se **ne** modeluje znakom
> `?`, već unijom sa `null` (npr. `publishedAt: string | null`). Razlog je što DTO
> preslikava tačno ono što server šalje: server uvek pošalje polje `publishedAt`, samo
> mu vrednost može biti `null`. Zato u DTO-ima projekta znak `?` gotovo i ne viđamo.

Interfejs može da opiše i metodu. Sledeći interfejs zahteva da objekat ima metodu
`greet`, koja prima tekst i vraća tekst:

```ts
interface Person {
  name: string;
  surname: string;
  greet: (prefix: string) => string;
}

const pera: Person = {
  name: 'Pera',
  surname: 'Perić',
  greet(prefix) {
    return `${prefix} ${this.name} ${this.surname}`;
  },
};
```

Zapis `(prefix: string) => string` je tip funkcije: levo od strelice su parametri, a
desno povratni tip. Parametar `prefix` u objektu `pera` nema anotaciju, jer prevodilac
njegov tip zaključuje iz interfejsa.

> **Važno:** Interfejs postoji samo za prevodioca i pri prevođenju u JS potpuno nestaje.
> Zato TS ne može da proveri da li server zaista šalje podatke koji odgovaraju interfejsu
> `TourDto`. Kada odgovor servera označimo kao `TourDto`, prevodilac tu tvrdnju prihvata
> kao tačnu. Ako se model na klijentu razlikuje od onoga što server šalje, greška se ne
> vidi pri prevođenju, već tek kada se aplikacija izvršava. Zbog toga u projektu važi
> pravilo da DTO tipovi preslikavaju serverske podatke jedan na jedan i da se moraju
> menjati istovremeno kada se promeni server.


## Nepostojeća vrednost

JS ima dve vrednosti koje označavaju odsustvo vrednosti:
- `undefined` označava da vrednost nije dodeljena. Tu vrednost ima promenljiva kojoj nismo dodelili vrednost, opciono polje koje objekat ne sadrži, kao i element niza koji ne postoji.
- `null` označava da vrednost namerno ne postoji. Ovu vrednost uvek dodeljujemo sami.

U JS-u bilo koja promenljiva može da sadrži `null` ili `undefined`. U TS-u (uz strogi režim) to nije slučaj: promenljiva tipa `string` može da sadrži samo tekst. Kada vrednost može da nedostaje, to moramo eksplicitno da navedemo pomoću **unije tipova** (engl. *union type*). Unija se piše uspravnom crtom `|` i znači da vrednost može biti bilo kog od navedenih tipova. Tip `string | null` čitamo kao „tekst ili `null`“. Opciono polje `publishedAt?: string` iz prethodnog odeljka zapravo je tipa `string | undefined`.

Sledeći kod prikazuje polje klase `TourList` koje čuva identifikator izabrane ture i metodu koja ga koristi:

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
- Polje `selectedTourId` čuva identifikator izabrane ture ili `null`. Početna vrednost je `null`, jer nijedna tura još nije izabrana.
- Konstanta `tourId` je tipa `string | null`, a metoda `publish` prima `string`. Zato prevodilac ne dozvoljava poziv `this.publish(tourId)` sve dok proverom ne isključimo `null`. Nakon provere `if (tourId === null)` sa naredbom `return`, prevodilac zna da je preostali tip `string`. Ovaj mehanizam zovemo **sužavanje tipa** (engl. *type narrowing*).
- Vrednost polja najpre prepisujemo u lokalnu konstantu. Tako smo sigurni da proveravamo i koristimo istu vrednost, jer polje može da promeni drugi deo koda, a konstantu ne može.

## Generički tip

Oznaku sa izlomljenim zagradama `<...>` već smo sreli u povratnom tipu `Promise<void>`. U pitanju je **generički tip** (engl. *generic type*), gde vrednost u zagradama određuje tip sa kojim se radi. Na primer, `Promise<void>` je obećanje koje ne donosi vrednost, a `Promise<TourDto>` obećanje koje donosi turu. Slično tome, zapis `TourDto[]` je skraćeni oblik generičkog tipa `Array<TourDto>`, tj. niza tura.

Generici omogućavaju da napišemo funkcije, klase ili interfejse koji rade sa različitim tipovima podataka, a da prevodilac i dalje proverava tipove. Umesto da unapred „zakucamo“ jedan tip (npr. `string` ili `number`), koristimo **tipski parametar** (najčešće se zove `T`), koji prevodilac popunjava konkretnim tipom tek kada se funkcija pozove ili klasa upotrebi.

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
2. `protected` – član je dostupan kodu unutar klase i kodu klasa koje je nasleđuju.
3. `private` – član je dostupan samo kodu unutar klase.

Pored njih postoji i modifikator `readonly`, koji ne ograničava pristup, već izmenu. Polju označenom sa `readonly` vrednost se dodeljuje jednom, pri deklaraciji ili u konstruktoru, i posle se ne može promeniti.

Sledeći kod prikazuje polja klase `TourList` sa modifikatorima:

```ts
export class TourList {
  private readonly apiUrl = '/api/exploration/tours';
  protected readonly tours: TourDto[] = [];
  protected selectedTourId: string | null = null;
}
```

U datom kodu treba uočiti sledeće:
- Sva polja dobijaju vrednost odmah pri deklaraciji, pa klasi nije potreban konstruktor.
- Polje `apiUrl` je `private`, jer ga koristi samo kod unutar klase.
- Modifikator `readonly` sprečava da se polju `tours` dodeli novi niz (npr. `this.tours = []`), ali ne sprečava izmenu postojećeg niza (npr. `this.tours.push(tour)`).
