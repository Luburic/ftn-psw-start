Resurs iz prethodne lekcije čita podatke sa servera i ništa na njemu ne menja. Stranica sa turama korisnika ima i dugme za objavljivanje, a stranica za pravljenje ture šalje novu turu. Podela komandi i upita iz aplikacionog sloja servera važi i na klijentu. Resurs je klijentska strana *upita*. Ovde upoznajemo kako klijent šalje *komandu*, kako čeka odgovor i kako prikazuje grešku koju server prijavi.

## Slanje zahteva

Angular aplikacija formira HTTP zahteve za proizvoljne metode (npr. POST, PUT i DELETE) uz pomoć ugrađenog `HttpClient` servisa. Sledeći kod prikazuje isečak komponente koja koristi `HttpClient` servis i pravi HTTP POST zahtev uz pomoću njega:

```ts
export class TourAuthoringComponent {
  private readonly http = inject(HttpClient);

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`/api/tours/${id}/publish`, {}));
  }
}
```

U datom kodu treba uočiti sledeće:
- Segment `http = inject(HttpClient)` injektuje `HttpClient` servis u komponentu.
- Metoda `this.http.post` prima adresu i telo zahteva, koje pretvara u JSON. Objavljivanje ture nema telo, pa šalje prazan objekat. Za ostale HTTP metode `HttpClient` ima istoimene metode, npr. `put` i `delete`.
- Parametar generičkog tipa, `post<void>`, je tip koji prevodilac dodeljuje telu odgovora. Objavljivanje ne vraća telo, pa je tip `void`.
- Metoda `post` vraća `Observable`. **Observable** je vrednost koja stiže kasnije i može da stigne više puta.
- Odgovor na HTTP zahtev stiže tačno jednom, pa nam višestruko stizanje ne treba. Funkcija `firstValueFrom`, iz biblioteke `rxjs` koju Angular koristi, prima `Observable` i vraća obećanje prve vrednosti koja stigne. Poziv `HttpClient`-a zato obuhvatamo ovim pozivom, pa se čeka sa `await` kao svaki asinhroni poziv.
- Za razliku od `fetch`, `HttpClient` odgovor sa statusnim kodom greške pretvara u izuzetak. Tada `await` baca izuzetak, a objekat izuzetka nosi telo odgovora.

## Servis sa komandama

Komande koje izdajemo serveru uglavnom ne stoje u komponentama, već u servisu koji grupiše povezane komande (često odgovaraju kontrolerima servera). Komponenta zatim ubrizgava naš servis i preko njega šalje HTTP zahteve. Sledeći kod prikazuje primer takvog servisa:

```ts
const BASE_URL = '/api/tours';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateTourDto): Promise<TourDto> {
    return firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }
}
```

U datom kodu treba uočiti sledeće:
- Konstanta `BASE_URL` drži zajednički početak adrese, a svaka metoda na njega nadovezuje svoj deo. Ovo je klijentski pandan atributu `[Route]` na kontroleru.
- Svaka metoda je jedna komanda i sastoji se od jednog poziva. Metoda `create` šalje DTO strukturu `CreateTourDto`, koja nosi podatke nove ture, i vraća `TourDto` iz odgovora.
- Razlika između `create` i `publish` je u tome da li metoda ima šta da vrati. Metoda `create` ima, pa obećanje prosleđuje naredbom `return`, bez `await`. Ta metoda vraća obećanje, pa ga pozivalac može sačekati. Metoda `publish` nema šta da vrati, ali i dalje mora da sačeka odgovor, pa stoji `await`. Pravilo je da se obećanje mora ili vratiti ili sačekati.
- Servis nema signal i ne zna ni za jedan resurs. Šalje zahtev i vraća odgovor, a obradu odgovora prepušta stranici.

## Stranica sa komandom

Stranica mora da zna da komanda traje, da prikaže grešku ako komanda ne uspe i da nakon uspeha ponovo učita svoj resurs, jer se podaci na serveru razlikuju od prikazanih. Sledeći kod prikazuje deo stranice sa turama korisnika koji objavljuje turu:

```html
@if (error()) {
  <p class="error">{{ error() }}</p>
}

@for (tour of tours.value() ?? []; track tour.id) {
  <h3>{{ tour.name }}</h3>
  @if (tour.status === 'Draft') {
    <button type="button" [disabled]="pending()" (click)="publish(tour.id)">Objavi</button>
  }
}
```

```ts
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/tours/mine');
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async publish(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.publish(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(failure);
    } finally {
      this.pending.set(false);
    }
  }
}
```

U datom kodu treba uočiti sledeće:
- Signal `pending` je tačan dok komanda traje. Sva dugmad su vezana za njega, pa korisnik ne može da pošalje drugu komandu dok prva traje. Blok `finally` ga vraća na netačno i kada komanda uspe i kada ne uspe.
- Signal `error` drži poruku o grešci komande ili `null`, a `tours.error()` grešku čitanja. Na početku svake komande se briše, da poruka od prethodnog pokušaja ne ostane na ekranu.
- Poziv `reload` ponovo šalje zahtev resursa na trenutnu adresu. Stoji iza `await`, pa se izvršava tek kada server potvrdi komandu. Ispred njega nema `await`, jer `reload` nije asinhron i odmah se vraća. Stranica ne menja niz sama, već ponovo čita spisak sa servera.
- Kada zahtev ne uspe, izuzetak hvata blok `catch`, koji poruku servera upisuje u signal `error`.

Kada korisnik klikne na dugme za objavljivanje ture koja nema nijedno vreme obilaska, dešava se sledeće:
1. Vezivanje događaja poziva `publish` sa identifikatorom ture. Signal `pending` postaje tačan i dugmad se onemogućavaju.
2. Servis šalje zahtev na adresu `/api/tours/<id>/publish`, a `await` čeka odgovor.
3. Na serveru domenski sloj baca izuzetak, koji middleware pretvara u odgovor sa statusnim kodom 400 i porukom u polju `title`.
4. Poziv `await` baca izuzetak, pa se `reload` preskače. Blok `catch` iz izuzetka čita poruku servera i upisuje je u signal `error`. Blok `finally` vraća `pending` na netačno.
5. Šablon je čitalac oba signala, pa radni okvir ponovo proverava šablon stranice. Iznad spiska se prikazuje poruka servera, a dugmad su ponovo dostupna.

Kada tura ima vreme obilaska, server vraća odgovor bez greške. Tada se u četvrtom koraku izvršava `reload`, a kada odgovor stigne, tura ima status `Published` i prikazuje se bez dugmeta.