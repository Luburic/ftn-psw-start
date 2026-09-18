Resurs iz prethodne lekcije čita podatke sa servera i ništa na njemu ne menja. Stranica sa turama korisnika ima i dugme za objavljivanje, a stranica za pravljenje ture šalje novu turu. Podela na komande i upite iz aplikacionog sloja servera važi i na klijentu: resurs je klijentska strana upita. Ovde upoznajemo kako klijent šalje komandu, kako reaguje na odgovor i kako prikazuje grešku koju server prijavi.

## Slanje zahteva

Komandu šaljemo klasom `HttpClient`, koju servis preuzima od kontejnera zavisnosti. Najlakše je razumeti je poređenjem sa funkcijom `fetch`, koju već poznajemo. Sledeći kod objavljuje blog pomoću `fetch`:

```ts
const response = await fetch(`/api/social/blogs/${id}/publish`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({}),
});
if (!response.ok) {
  throw new Error('Request failed');
}
```

Isti zahtev pomoću `HttpClient` izgleda ovako:

```ts
const request = this.http.post<void>(`/api/social/blogs/${id}/publish`, {});

request.subscribe({
  next: () => console.log('Blog je objavljen.'),
  error: (failure) => console.error(failure),
});
```

Pogledajmo šta radi svaki od ova dva dela.

**Prvi deo opisuje zahtev.** Metoda `post` prima adresu i telo zahteva. Telo sama pretvara u JSON i sama postavlja zaglavlje `Content-Type`, što smo kod `fetch` pisali ručno. Objavljivanje ne šalje nikakve podatke, pa je telo prazan objekat `{}`. Zapis `<void>` iza naziva metode kaže prevodiocu kakav odgovor očekujemo. Server na objavljivanje ne vraća nikakve podatke, pa je tip `void`. Da server vraća npr. novi blog, napisali bismo `post<BlogDto>`.

Važno je da prvi deo **još ne šalje zahtev**. Metoda `post` ne vraća obećanje (engl. *promise*), kao `fetch`, već objekat tipa `Observable`. **Observable** je izvor vrednosti koje stižu kasnije, možda i više puta. `Observable` je definisan u biblioteci **RxJS** (paket `rxjs`), koja se instalira uz svaki Angular projekat i koju Angular koristi u mnogim delovima radnog okvira.

**Drugi deo pokreće zahtev.** Metoda `subscribe` pokreće zahtev i prima dve funkcije. Funkciju `next` izvršava kada stigne odgovor, a funkciju `error` kada zahtev ne uspe. Kažemo da se pozivom `subscribe` pretplaćujemo na `Observable`.

U projektu se ta dva dela pišu kao jedan izraz:

```ts
this.http.post<void>(`${BASE_URL}/${id}/publish`, {}).subscribe({
  next: () => { /* uspeh */ },
  error: (failure) => { /* greška */ },
});
```

U datom kodu treba uočiti sledeće:
- Poziv `this.http.post(...)` bez `subscribe` ne prijavljuje nikakvu grešku, ali zahtev nikada ne ode na server, jer ga niko nije pokrenuo.
- Kod `fetch` smo morali sami da proverimo `response.ok`, jer `fetch` odgovor sa greškom, npr. statusnim kodom 400, smatra uspešnim. `HttpClient` to radi umesto nas: kada server vrati statusni kod greške, izvršava se funkcija `error`, a ne `next`. Objekat greške nosi i telo odgovora, pa iz njega možemo da pročitamo poruku servera, kao što pokazuje odeljak o grešci servera.
- Na HTTP zahtev stiže tačno jedan odgovor, pa se `Observable` iz `HttpClient` nakon njega sam završava. Pretplatu zato ne moramo ručno da prekidamo.

## Servis sa komandama

Komande ne pišemo u stranici, već u servisu, pa ih svaka stranica za pisanje blogova poziva na isti način. Sledeći kod prikazuje, iz projekta, ceo taj servis:

```ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class BlogAuthoring {
  private readonly http = inject(HttpClient);

  create(dto: CreateBlogDto): Observable<BlogDto> {
    return this.http.post<BlogDto>(BASE_URL, dto);
  }

  publish(id: string): Observable<void> {
    return this.http.post<void>(`${BASE_URL}/${id}/publish`, {});
  }
}
```

U datom kodu treba uočiti sledeće:
- Konstanta `BASE_URL` drži zajednički početak adrese, a svaka metoda na njega nadovezuje svoj deo. Ovo je klijentski pandan atributu `[Route]` na kontroleru.
- Svaka metoda je jedna komanda i sastoji se od jednog poziva. Metoda `create` šalje DTO strukturu `CreateBlogDto`, koja nosi podatke novog bloga, a odgovor će nositi `BlogDto` novog bloga.
- Metode vraćaju `Observable` i same ne pozivaju `subscribe`. Servis dakle samo opisuje zahtev, a pokreće ga stranica, jer samo ona zna šta treba da uradi kada odgovor stigne.
- Servis nema signal i ne zna ni za jedan resurs. Obradu odgovora prepušta stranici.

## Greška servera

Na serveru middleware za obradu grešaka pretvara izuzetak u odgovor sa statusnim kodom greške i telom koje u polju `title` nosi poruku izuzetka. Poruka je namenjena korisniku, na primer da tura bez ijednog vremena obilaska ne može da se objavi. Stranica zato prikazuje poruku koju je server poslao, a unapred upisanu poruku samo kada telo nema `title`, na primer kada zahtev nije stigao do servera. Sledeći kod prikazuje pomoćnu funkciju iz projekta koja to radi:

```ts
import { HttpErrorResponse } from '@angular/common/http';

export function serverMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse && typeof error.error?.title === 'string') {
    return error.error.title;
  }
  return fallback;
}
```

U datom kodu treba uočiti sledeće:
- Prvi parametar je tipa `unknown`, što znači bilo koja vrednost. Funkcija `error` iz poziva `subscribe` ne zna unapred šta je tačno pošlo naopako: zahtev je mogao da stigne do servera i vrati grešku, ali je mogao i da ne stigne uopšte.
- Provera `instanceof HttpErrorResponse` sužava tip sa `unknown` na odgovor servera sa greškom, isto kao što provera `=== null` sužava tip u lekciji o TypeScript-u. Svojstvo `error` tog objekta je telo odgovora, pa `error.error.title` čita poruku servera.
- Drugi parametar je poruka koju funkcija vraća u svakom drugom slučaju: kada greška nije odgovor servera ili kada telo nema polje `title`.
- Funkcija je zajednička za sve module.

## Stranica sa komandom

Stranica mora da zna da komanda traje, da prikaže grešku ako komanda ne uspe i da nakon uspeha ponovo učita svoj resurs, jer se podaci na serveru razlikuju od prikazanih. Sledeći kod prikazuje, iz projekta, deo stranice sa blogovima korisnika koji objavljuje blog:

```ts
import { finalize } from 'rxjs';

export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);

  protected readonly blogs = httpResource<BlogDto[]>(() => '/api/social/blogs/mine');
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected publish(id: string): void {
    this.pending.set(true);
    this.error.set(null);
    this.blogAuthoring
      .publish(id)
      .pipe(finalize(() => this.pending.set(false)))
      .subscribe({
        next: () => this.blogs.reload(),
        error: (failure) => this.error.set(serverMessage(failure, 'Could not publish the blog.')),
      });
  }
}
```

```html
@if (error()) {
  <p class="error">{{ error() }}</p>
}

@if (blogs.hasValue()) {
  @for (blog of blogs.value(); track blog.id) {
    <button type="button" [disabled]="pending()" (click)="publish(blog.id)">Publish</button>
  }
}
```

U datom kodu treba uočiti sledeće:
- Signal `pending` je tačan dok komanda traje. Sva dugmad su vezana za njega, pa korisnik ne može da pošalje drugu komandu dok prva traje. Zbog toga se onemogućavaju i dugmad blogova na koje korisnik nije kliknuo. To je namerno pojednostavljenje.
- Metoda `pipe` dodaje na `Observable` **operatore**, funkcije iz biblioteke RxJS koje menjaju ili dopunjuju njegovo ponašanje. Operator `finalize` izvršava zadatu funkciju kada se zahtev završi, bez obzira na to da li je uspeo. Zato vraća `pending` na netačno i posle uspeha i posle greške.
- Signal `error` drži poruku o grešci komande ili `null`, a `blogs.error()` grešku čitanja. Na početku svake komande se briše, da poruka od prethodnog pokušaja ne ostane na ekranu.
- Funkcija `next` se izvršava tek kada server potvrdi komandu, pa tek tada poziva `reload`. Stranica ne menja niz sama, već ponovo čita spisak sa servera.
- Kada zahtev ne uspe, izvršava se funkcija `error`, koja poruku servera upisuje u signal `error`.
- Spisak se prikazuje samo kada `blogs.hasValue()` vraća tačno, kao u prethodnoj lekciji. Bez te provere bi čitanje `blogs.value()` bacilo grešku kada čitanje spiska ne uspe.

## Objavljivanje ture

Povežimo pojmove u stranicu sa turama korisnika iz projekta, sada sa komandom za objavljivanje. Servis `TourAuthoring` ima isti oblik kao `BlogAuthoring`, sa adresom `/api/exploration/tours`, a projektni `TourDto` ima i svojstvo `status` tipa `'Draft' | 'Published'`. Pošto je tip unija tačno navedenih vrednosti, prevodilac prijavljuje grešku ako u šablonu napišemo npr. `tour.status === 'draft'`.

```ts
import { Component, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-my-tours',
  imports: [RouterLink],
  templateUrl: './my-tours.html',
  styleUrl: './my-tours.scss',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected publish(tourId: string): void {
    this.pending.set(true);
    this.error.set(null);
    this.tourAuthoring
      .publish(tourId)
      .pipe(finalize(() => this.pending.set(false)))
      .subscribe({
        next: () => this.tours.reload(),
        error: (failure) => this.error.set(serverMessage(failure, 'Could not publish the tour.')),
      });
  }
}
```

```html
@if (error()) {
  <p class="error">{{ error() }}</p>
}

@if (tours.hasValue()) {
  <table>
    <tbody>
      @for (tour of tours.value(); track tour.id) {
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
} @else if (tours.error()) {
  <p class="error">Could not load your tours. Log in and try again.</p>
} @else if (tours.isLoading()) {
  <p>Loading your tours...</p>
}
```

Kada korisnik klikne na dugme za objavljivanje ture koja nema nijedno vreme obilaska, dešava se sledeće:
1. Vezivanje događaja poziva `publish` sa identifikatorom ture. Signal `pending` postaje tačan i dugmad se onemogućavaju.
2. Servis vraća opisan zahtev, a poziv `subscribe` ga šalje na adresu `/api/exploration/tours/<id>/publish`.
3. Na serveru domenski sloj baca izuzetak, koji middleware pretvara u odgovor sa statusnim kodom 400 i porukom u polju `title`.
4. Pošto je statusni kod greška, izvršava se funkcija `error`, a ne `next`, pa se `reload` ne poziva. Funkcija `error` čita poruku servera i upisuje je u signal `error`. Zatim operator `finalize` vraća `pending` na netačno.
5. Šablon je čitalac oba signala, pa Angular ponovo proverava šablon. Iznad tabele se prikazuje poruka servera, a dugmad su ponovo dostupna.

Kada tura ima vreme obilaska, server vraća odgovor bez greške. Tada se u četvrtom koraku izvršava funkcija `next`, koja poziva `reload`. Resurs dok ponovo čita zadržava prethodni spisak, pa `hasValue()` ostaje tačno i tabela ostaje na ekranu. Kada odgovor stigne, status ture postaje `Published` i dugme nestaje.
