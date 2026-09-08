Klijentski deo našeg projekta je napisan u jeziku TypeScript. Razmotrimo klasu koja drži spisak tura i izabranu turu:

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

Čitalac koji poznaje JavaScript prepoznaje klasu, polja, metode, `async` i `await`, kao i poziv `fetch`. Ostaju delovi koje JavaScript nema: reči `private`, `protected` i `readonly` ispred polja, `: string` iza parametra, `: Promise<void>` iza liste parametara i `: TourDto[]` iza naziva polja. Sve to su oznake tipova. **TypeScript** je jezik koji proširuje JavaScript oznakama tipova, koje prevodilac (engl. *compiler*) proverava pre nego što kod stigne do pregledača. Pregledač izvršava običan JavaScript, jer prevodilac uklanja sve oznake tipova pri prevođenju. Ako je neka oznaka prekršena, prevođenje ne uspeva i aplikacija se ne pokreće.

Ovde upoznajemo samo one delove jezika koji se pojavljuju u svakoj klasi projekta.

## Anotacija tipa

**Anotacija tipa** (engl. *type annotation*) je oznaka oblika `: tip` iza naziva polja, iza naziva parametra ili iza liste parametara metode, koja prevodiocu saopštava kog tipa je vrednost. Osnovni tipovi su `string`, `number` i `boolean`. Metoda koja ne vraća vrednost ima tip `void`.

Sledeći kod prikazuje metodu sa anotiranim parametrom i povratnom vrednošću:

```ts
protected select(tourId: string): void {
  this.selectedTourId = tourId;
}
```

U datom kodu treba uočiti sledeće:
- Anotacija `tourId: string` znači da poziv `select(5)` ne prolazi prevođenje, jer je `5` broj, a ne tekst. Greška se javlja u uređivaču i pri prevođenju, pre pokretanja aplikacije.
- Anotacija `: void` znači da metoda ništa ne vraća. Prevodilac prijavljuje grešku ako u telu metode napišemo `return` sa vrednošću.
- Lokalne promenljive najčešće nemaju anotaciju, jer prevodilac tip zaključuje iz dodeljene vrednosti. Promenljiva `const name = ''` je tipa `string` bez ikakve oznake.

## Interfejs

**Interfejs** (engl. *interface*) je imenovani oblik objekta, odnosno spisak svojstava i njihovih tipova. Kada podatak stiže sa servera u JSON zapisu, interfejs opisuje koja svojstva taj podatak ima. Sledeći kod prikazuje interfejs za podatak o turi:

```ts
export interface TourDto {
  id: string;
  name: string;
  description: string;
  difficulty: string;
}
```

U datom kodu treba uočiti sledeće:
- Interfejs ne postoji u pregledaču. Prevodilac ga koristi samo da proveri kod, a zatim ga uklanja.
- Objekat je tipa `TourDto` ako ima sva navedena svojstva odgovarajućih tipova. Pristup svojstvu koje interfejs nema, na primer `tour.author`, prevodilac prijavljuje kao grešku.
- Tip `TourDto[]` označava niz vrednosti tipa `TourDto`. Na isti način `string[]` označava niz tekstualnih vrednosti.

## Generički tip

**Generički tip** (engl. *generic type*) je tip koji u uglastim zagradama prima drugi tip kao parametar. Zapis `Promise<TourDto>` čitamo kao obećanje tipa `TourDto`. Asinhrona metoda uvek vraća obećanje, pa je njena povratna vrednost uvek generičkog tipa `Promise`. Sledeći kod prikazuje dve asinhrone metode:

```ts
protected async load(): Promise<TourDto[]> {
  const response = await fetch('/api/exploration/tours');
  return await response.json();
}

protected async publish(tourId: string): Promise<void> {
  await fetch(`/api/exploration/tours/${tourId}/publish`, { method: 'POST' });
}
```

U datom kodu treba uočiti sledeće:
- Anotacija `: Promise<TourDto[]>` kaže da poziv `await this.load()` daje niz tipa `TourDto`. Sve što iz tog niza pročitamo prevodilac proverava prema interfejsu `TourDto`.
- Anotacija `: Promise<void>` je isti oblik, sa tipom `void` kao parametrom. Metoda je asinhrona, pa vraća obećanje, ali obećanje ne nosi vrednost.

## Nepostojeća vrednost

U JavaScript-u svaka promenljiva može da bude `null`. U TypeScript-u promenljiva tipa `string` ne sme da bude `null`. Kada vrednost može da nedostaje, to zapisujemo kao **uniju tipova** (engl. *union type*), oblika `string | null`, koju čitamo kao tekst ili ništa. Sledeći kod prikazuje polje koje čuva identifikator izabrane ture i metodu koja ga čita:

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

## Modifikatori članova klase

JavaScript klasa ima polja i metode koji su svima dostupni. TypeScript dodaje modifikatore koji ograničavaju pristup i izmenu:
1. `private` znači da je član vidljiv samo kodu unutar klase.
2. `protected` znači da je član vidljiv kodu unutar klase i kodu klasa koje je nasleđuju.
3. `readonly` znači da se polje dodeljuje jednom i više ne menja.

Sledeći kod prikazuje uobičajen početak klase:

```ts
export class TourList {
  private readonly tours: TourDto[] = [];
  protected selectedTourId: string | null = null;
}
```

U datom kodu treba uočiti sledeće:
- Polja dobijaju vrednost odmah pri deklaraciji, pa klasa nema konstruktor.
- Polje `tours` je `readonly`, jer se niz koji čuva ne zamenjuje drugim nizom. Dodavanje elementa u taj niz `readonly` ne sprečava, jer se time menja sadržaj niza, a ne polje.
- Polje `selectedTourId` nije `readonly`, jer mu metoda `select` dodeljuje novu vrednost.

## Od JavaScript-a do TypeScript-a

Povežimo pojmove tako što jednu JavaScript klasu korak po korak pretvorimo u TypeScript klasu. Polazimo od klase koja čuva izabranu turu i objavljuje je:

```js
export class TourList {
  tours = [];
  selectedTourId = null;

  select(tourId) {
    this.selectedTourId = tourId;
  }

  async publish(tourId) {
    await fetch(`/api/exploration/tours/${tourId}/publish`, { method: 'POST' });
  }
}
```

Anotiramo parametre i povratne vrednosti:

```ts
select(tourId: string): void { ... }
async publish(tourId: string): Promise<void> { ... }
```

Anotiramo polja. Polje `tours` je niz tura, a `selectedTourId` čuva tekst ili ništa:

```ts
tours: TourDto[] = [];
selectedTourId: string | null = null;
```

Dodajemo modifikatore članova. Polje `tours` koristi samo klasa i nikada ga ne zamenjuje, a ostali članovi su dostupni i klasama koje je nasleđuju:

```ts
private readonly tours: TourDto[] = [];
protected selectedTourId: string | null = null;
protected select(tourId: string): void { ... }
protected async publish(tourId: string): Promise<void> { ... }
```

Rezultat je klasa sa početka lekcije. Kod u pregledaču je ostao isti kao na početku, a prevodilac sada proverava svaki poziv metode i svako čitanje polja pre pokretanja.
