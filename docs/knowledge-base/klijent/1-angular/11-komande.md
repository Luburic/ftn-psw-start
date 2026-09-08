Resurs iz prethodne lekcije čita podatke sa servera i ništa na njemu ne menja. Stranica sa turama korisnika ima i dugme za objavljivanje, a stranica za pravljenje ture šalje novu turu. Podela komandi i upita iz aplikacionog sloja servera važi i na klijentu. Resurs je klijentska strana upita. Ovde upoznajemo kako klijent šalje komandu, kako čeka odgovor i kako prikazuje grešku koju server prijavi.

## Slanje zahteva

Komandu šalje klasa `HttpClient`, koju servis preuzima od kontejnera. Sledeći kod prikazuje poziv koji objavljuje blog, onako kako stoji u servisu iz projekta koji čitamo u sledećem odeljku:

```ts
await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
```

U datom kodu treba uočiti sledeće:
- Metoda `post` prima adresu i telo zahteva, koje pretvara u JSON. Objavljivanje nema telo, pa šalje prazan objekat.
- Parametar generičkog tipa, `post<void>`, je tip koji prevodilac dodeljuje telu odgovora. Objavljivanje ne vraća telo, pa je tip `void`.
- Metoda `post` ne vraća obećanje, već `Observable`. **Observable** je vrednost koja stiže kasnije i može da stigne više puta. Naziv ne prevodimo. Odgovor na HTTP zahtev stiže tačno jednom, pa nam višestruko stizanje ne treba.
- Funkcija `firstValueFrom`, iz biblioteke `rxjs` koju Angular koristi, prima `Observable` i vraća obećanje prve vrednosti koja stigne. U projektu je svaki poziv metode `post` obuhvaćen ovim pozivom, pa se čeka sa `await` kao svaki asinhroni poziv.
- Za razliku od `fetch`, `HttpClient` odgovor sa statusnim kodom greške pretvara u izuzetak. Tada `await` baca izuzetak, a objekat izuzetka nosi telo odgovora.

## Servis sa komandama

Komande ne stoje u stranici, već u servisu koji pripada stranicama za pisanje blogova, pa ih svaka od tih stranica poziva na isti način. Sledeći kod prikazuje, iz projekta, ceo taj servis:

```ts
const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class BlogAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateBlogDto): Promise<BlogDto> {
    return firstValueFrom(this.http.post<BlogDto>(BASE_URL, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }
}
```

U datom kodu treba uočiti sledeće:
- Konstanta `BASE_URL` drži zajednički početak adrese, a svaka metoda na njega nadovezuje svoj deo. Ovo je klijentski pandan atributu `[Route]` na kontroleru.
- Svaka metoda je jedna komanda i sastoji se od jednog poziva. Metoda `create` šalje DTO strukturu `CreateBlogDto`, koja nosi podatke nove ture, i vraća `BlogDto` iz odgovora. Obećanje vraća bez `await`, jer asinhrona metoda sme da vrati obećanje, a pozivalac ga čeka isto.
- Servis nema signal i ne zna ni za jedan resurs. Šalje zahtev i vraća odgovor, a obradu odgovora prepušta stranici.

## Greška servera

Na serveru middleware za obradu grešaka pretvara izuzetak u odgovor sa statusnim kodom greške i telom koje u polju `title` nosi poruku izuzetka. Poruka je namenjena korisniku, na primer da tura bez ijednog vremena obilaska ne može da se objavi. Stranica zato prikazuje poruku koju je server poslao, a unapred upisanu poruku samo kada telo nema `title`, na primer kada zahtev nije stigao do servera. Sledeći kod prikazuje potpis pomoćne funkcije iz projekta koja to radi:

```ts
export function serverMessage(error: unknown, fallback: string): string
```

U datom kodu treba uočiti sledeće:
- Prvi parametar je tipa `unknown`, što znači bilo koja vrednost, jer blok `catch` ne zna tip onoga što je uhvatio. Funkcija proverava da li je uhvaćeni izuzetak odgovor servera i vraća `title` iz njegovog tela.
- Drugi parametar je poruka koju funkcija vraća u svakom drugom slučaju.
- Funkcija živi u direktorijumu `shared/util`, jer je koristi svaki modul. Njeno telo ovde izostavljamo.

## Stranica sa komandom

Stranica mora da zna da komanda traje, da prikaže grešku ako komanda ne uspe i da nakon uspeha ponovo učita svoj resurs, jer se podaci na serveru razlikuju od prikazanih. Sledeći kod prikazuje, iz projekta, deo stranice sa blogovima korisnika koji objavljuje blog:

```ts
export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);

  protected readonly blogs = httpResource<BlogDto[]>(() => '/api/social/blogs/mine');
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async publish(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.publish(id);
      this.blogs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
```

```html
@if (error()) {
  <p class="error">{{ error() }}</p>
}

@for (blog of blogs.value() ?? []; track blog.id) {
  <button type="button" [disabled]="pending()" (click)="publish(blog.id)">Publish</button>
}
```

U datom kodu treba uočiti sledeće:
- Signal `pending` je tačan dok komanda traje. Sva dugmad su vezana za njega, pa korisnik ne može da pošalje drugu komandu dok prva traje. Blok `finally` ga vraća na netačno i kada komanda uspe i kada ne uspe.
- Signal `error` drži poruku o grešci komande ili `null`, a `blogs.error()` grešku čitanja. Na početku svake komande se briše, da poruka od prethodnog pokušaja ne ostane na ekranu.
- Poziv `reload` stoji iza `await`, pa se izvršava tek kada server potvrdi komandu. Stranica ne menja niz sama, već ponovo čita spisak sa servera.
- Kada zahtev ne uspe, izuzetak hvata blok `catch`, koji poruku servera upisuje u signal `error`.

## Objavljivanje ture

Povežimo pojmove u stranicu sa turama korisnika iz projekta, sada sa komandom za objavljivanje. Servis `TourAuthoring` ima isti oblik kao `BlogAuthoring`, sa adresom `/api/exploration/tours`, a projektni `TourDto` ima i svojstvo `status`:

```ts
@Component({
  imports: [RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async publish(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.publish(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.pending.set(false);
    }
  }
}
```

```html
@if (error()) {
  <p class="error">{{ error() }}</p>
}

@if (tours.isLoading()) {
  <p>Loading your tours...</p>
} @else if (tours.error()) {
  <p class="error">Could not load your tours. Log in and try again.</p>
} @else {
  <table>
    <tbody>
      @for (tour of tours.value() ?? []; track tour.id) {
        <tr>
          <td>{{ tour.name }}</td>
          <td>{{ tour.status }}</td>
          <td>
            @if (tour.status === 'Draft') {
              <button type="button" [disabled]="pending()" (click)="publish(tour.id)">Publish</button>
            }
          </td>
        </tr>
      } @empty {
        <tr>
          <td colspan="3">You have no tours yet.</td>
        </tr>
      }
    </tbody>
  </table>
}
```

Kada korisnik klikne na dugme za objavljivanje ture koja nema nijedno vreme obilaska, dešava se sledeće:
1. Vezivanje događaja poziva `publish` sa identifikatorom ture. Signal `pending` postaje tačan i dugmad se onemogućavaju.
2. Servis šalje zahtev na adresu `/api/exploration/tours/<id>/publish`, a `await` čeka odgovor.
3. Na serveru domenski sloj baca izuzetak, koji middleware pretvara u odgovor sa statusnim kodom 400 i porukom u polju `title`.
4. Poziv `await` baca izuzetak, pa se `reload` preskače. Blok `catch` iz izuzetka čita poruku servera i upisuje je u signal `error`. Blok `finally` vraća `pending` na netačno.
5. Šablon je pretplatnik oba signala, pa radni okvir ponovo iscrtava stranicu. Iznad tabele se prikazuje poruka servera, a dugmad su ponovo dostupna.

Kada tura ima vreme obilaska, server vraća odgovor bez greške. Tada se u četvrtom koraku izvršava `reload`, pa se umesto tabele prikazuje poruka o učitavanju, a kada odgovor stigne, tabela sa turom čiji je status `Published`, bez dugmeta.
