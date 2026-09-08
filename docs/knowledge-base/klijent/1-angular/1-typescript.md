Klijentski deo našeg projekta je napisan u jeziku TypeScript. Razmotrimo klasu jedne stranice iz projekta, skraćenu na deo koji nam je sada potreban:

```ts
export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);

  protected readonly blogs = httpResource<BlogDto[]>(() => '/api/social/blogs/mine');
  protected readonly error = signal<string | null>(null);

  protected async publish(blogId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogAuthoring.publish(blogId);
      this.blogs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the blog.'));
    }
  }
}
```

Čitalac koji poznaje JavaScript prepoznaje klasu, polja, metodu, `async` i `await`, kao i `try` i `catch`. Ostaju delovi koje JavaScript nema: reči `private`, `protected` i `readonly` ispred polja, `: string` iza parametra, `: Promise<void>` iza liste parametara i `<BlogDto[]>` iza naziva funkcije. Sve to su oznake tipova. **TypeScript** je jezik koji proširuje JavaScript oznakama tipova, koje prevodilac (engl. *compiler*) proverava pre nego što kod stigne do pregledača. Pregledač izvršava običan JavaScript, jer prevodilac uklanja sve oznake tipova pri prevođenju. Ako je neka oznaka prekršena, prevođenje ne uspeva i aplikacija se ne pokreće.

Ovde upoznajemo samo one delove jezika koji se pojavljuju u svakoj komponenti projekta.

## Anotacija tipa

**Anotacija tipa** (engl. *type annotation*) je oznaka oblika `: tip` iza naziva parametra ili iza liste parametara metode, koja prevodiocu saopštava kog tipa je vrednost. Osnovni tipovi su `string`, `number` i `boolean`. Metoda koja ne vraća vrednost ima tip `void`. Asinhrona metoda koja ne vraća vrednost ima tip `Promise<void>`, jer `async` metoda uvek vraća obećanje.

Sledeći kod prikazuje metodu sa anotiranim parametrom i povratnom vrednošću:

```ts
protected select(tourId: string): void {
  this.selectedTourId.set(tourId);
}
```

U datom kodu treba uočiti sledeće:
- Anotacija `tourId: string` znači da poziv `select(5)` ne prolazi prevođenje, jer je `5` broj, a ne tekst. Greška se javlja u uređivaču i pri prevođenju, pre pokretanja aplikacije.
- Anotacija `: void` znači da metoda ništa ne vraća. Prevodilac prijavljuje grešku ako u telu metode napišemo `return` sa vrednošću.
- Lokalne promenljive najčešće nemaju anotaciju, jer prevodilac tip zaključuje iz dodeljene vrednosti. Promenljiva `const name = ''` je tipa `string` bez ikakve oznake.

## Interfejs

**Interfejs** (engl. *interface*) je imenovani oblik objekta, odnosno spisak svojstava i njihovih tipova. Kada podatak stiže sa servera u JSON zapisu, interfejs opisuje koja svojstva taj podatak ima. Sledeći kod prikazuje interfejs za podatak o blogu, iz direktorijuma `api/` modula `social`:

```ts
export interface BlogDto {
  id: string;
  title: string;
  description: string;
  images: string[];
}
```

U datom kodu treba uočiti sledeće:
- Interfejs ne postoji u pregledaču. Prevodilac ga koristi samo da proveri kod, a zatim ga uklanja.
- Objekat je tipa `BlogDto` ako ima sva navedena svojstva odgovarajućih tipova. Pristup svojstvu koje interfejs nema, na primer `blog.author`, prevodilac prijavljuje kao grešku.
- Tip `string[]` označava niz tekstualnih vrednosti.

## Generički tip

**Generički tip** (engl. *generic type*) je tip koji u uglastim zagradama prima drugi tip kao parametar. Zapis `Promise<TourDto>` čitamo kao obećanje tipa `TourDto`. Anotacija `: Promise<void>` iz uvodnog primera je isti oblik, sa tipom `void` kao parametrom. Generički tip se pojavljuje i pri pozivu funkcije, kada funkcija ne može sama da zaključi kog tipa je vrednost sa kojom radi. Sledeći kod prikazuje dva takva poziva iz projekta:

```ts
protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

readonly tour = input.required<TourDto>();
```

U datom kodu treba uočiti sledeće:
- Funkcija `httpResource` čita podatke sa zadate adrese. Ona ne zna kog su oblika podaci koje će server vratiti, pa joj oblik saopštavamo parametrom `<TourDto[]>`. Sve što kasnije pročitamo iz `tours` prevodilac proverava kao niz tipa `TourDto`.
- Funkcija `input.required` prima vrednost od druge komponente. Parametar `<TourDto>` određuje kog tipa ta vrednost mora biti. Namenu ove dve funkcije obrađuju naredne lekcije. Ovde je bitan samo zapis tipa.

## Nepostojeća vrednost

U JavaScript-u svaka promenljiva može da bude `null`. U TypeScript-u promenljiva tipa `string` ne sme da bude `null`. Kada vrednost može da nedostaje, to zapisujemo kao **uniju tipova** (engl. *union type*), oblika `string | null`, koju čitamo kao tekst ili ništa. Sledeći kod prikazuje polje koje čuva identifikator izabrane ture i metodu koja ga čita:

```ts
protected readonly selectedTourId = signal<string | null>(null);

protected async addTransportTime(): Promise<void> {
  const tourId = this.selectedTourId();
  if (tourId === null) {
    return;
  }
  await this.tourAuthoring.addTransportTime(tourId, { transport: 'Walking', minutes: 30 });
}
```

U datom kodu treba uočiti sledeće:
- Parametar `<string | null>` kaže da polje čuva tekst ili `null`. Početna vrednost je `null`, jer nijedna tura još nije izabrana.
- Vrednost `tourId` je tipa `string | null`, a metoda `addTransportTime` prima `string`. Prevodilac odbija poziv sve dok se `null` ne isključi proverom. Nakon naredbe `if (tourId === null) return;` prevodilac zna da je preostali tip `string`.

## Modifikatori članova klase

JavaScript klasa ima polja i metode koji su svima dostupni. TypeScript dodaje modifikatore koji ograničavaju pristup i izmenu:
1. `private` znači da je član vidljiv samo kodu unutar klase.
2. `protected` znači da je član vidljiv kodu unutar klase i šablonu komponente. Šta je šablon obrađuje [lekcija o komponenti](3-komponenta.md).
3. `readonly` znači da se polje dodeljuje jednom i više ne menja.

Sledeći kod prikazuje uobičajen početak klase komponente:

```ts
export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);
  protected readonly error = signal<string | null>(null);
}
```

U datom kodu treba uočiti sledeće:
- Polja dobijaju vrednost odmah pri deklaraciji, pa klasa nema konstruktor.
- Polje `blogAuthoring` koristi samo kod klase, pa je `private`. Polje `error` čita i šablon, pa je `protected`.
- Oba polja su `readonly`, jer se objekat koji čuvaju ne menja. Menja se vrednost unutar objekta, što `readonly` ne sprečava.

## Od JavaScript-a do TypeScript-a

Povežimo pojmove tako što jednu JavaScript klasu korak po korak pretvorimo u TypeScript klasu. Polazimo od klase koja čuva izabranu turu i objavljuje je:

```js
export class MyTours {
  tourAuthoring = inject(TourAuthoring);
  selectedTourId = signal(null);

  select(tourId) {
    this.selectedTourId.set(tourId);
  }

  async publish(tourId) {
    await this.tourAuthoring.publish(tourId);
  }
}
```

Anotiramo parametre i povratne vrednosti:

```ts
select(tourId: string): void { ... }
async publish(tourId: string): Promise<void> { ... }
```

Polje `selectedTourId` dobija generički parametar sa unijom tipova, jer čuva tekst ili ništa:

```ts
selectedTourId = signal<string | null>(null);
```

Dodajemo modifikatore članova. Polje `tourAuthoring` koristi samo klasa, a `selectedTourId` čita i šablon:

```ts
private readonly tourAuthoring = inject(TourAuthoring);
protected readonly selectedTourId = signal<string | null>(null);
protected select(tourId: string): void { ... }
protected async publish(tourId: string): Promise<void> { ... }
```

Rezultat je klasa istog oblika kao stranice u projektu. Kod u pregledaču je ostao isti kao na početku, a prevodilac sada proverava svaki poziv metode i svako čitanje polja pre pokretanja.
