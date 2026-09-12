Prethodna lekcija je stranici dodelila resurs i servis, a prikaznoj komponenti ulaze i izlaze. Ostaje pitanje zašto resurs stoji u stranici, a ne u servisu, kada oboje razgovaraju sa serverom. Posmatrajmo suprotan izbor. Servis drži spisak tura autora u signalu, a stranice ga čitaju. Sledeći kod prikazuje takav servis, gde je telo metode koja spisak učitava izostavljeno:

```ts
@Injectable({ providedIn: 'root' })
export class Tours {
  private readonly http = inject(HttpClient);

  readonly mine = signal<TourDto[]>([]);

  async loadMine(): Promise<void> { ... }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`/api/exploration/tours/${id}/publish`, {}));
    await this.loadMine();
  }
}
```

U datom kodu treba uočiti sledeće:

- Komanda `publish` nakon uspeha ponovo učitava spisak `mine`, jer je objavljena tura promenila status. Isti spisak menja i komanda koja pravi turu i komanda koja dodaje vreme prevoza, pa svaka od njih mora da zna za `mine`.
- Objavljena tura se pojavljuje i u spisku objavljenih tura, koji čita druga stranica. Ako taj spisak takođe živi u servisu, komanda `publish` mora da učita i njega. Svaka komanda modula nosi spisak spiskova koje čini zastarelim, a taj spisak raste sa svakom novom stranicom.
- Stranica koja otvori `mine` pre nego što je iko pozvao `loadMine` prikazuje prazan niz. Stranica koja ga otvori posle vidi ono što je poslednja komanda ostavila, što može biti stanje od pre nekoliko minuta. Ko poziva `loadMine` i kada, nije zapisano nigde.

Arhitektura klijenta ovaj problem uklanja pravilom o mestu upita i komande.

## Upit u stranici, komanda u servisu grupe

Upit je resurs koji stranica deklariše u svom polju. Radni okvir pravi stranicu kada se adresa poklopi sa rutom i uništava je kada korisnik otvori drugu adresu. Resurs deli životni vek stranice, pa svaki dolazak na stranicu šalje nov zahtev i prikazuje ono što server trenutno ima. Nema spiska koji može da zastari, jer nijedan spisak ne nadživljava svoju stranicu.

**Servis grupe** (engl. *group service*) je servis koji sadrži komande jedne grupe slučajeva korišćenja i ništa drugo. Svaka metoda je jedna komanda, koja šalje zahtev i vraća obećanje odgovora. Servis nema signal i ne zna da resursi postoje. Grupa bez komandi nema servis, pa grupa `tour-browsing` iz projekta sadrži samo stranicu i prikaznu komponentu.

Iz dva pravila sledi obrazac koji je čitalac već pratio: stranica čeka komandu i zatim osvežava svoj resurs. Stranica koja je pokrenula komandu je ista stranica koja poseduje spisak, pa je osvežavanje jedan poziv metode `reload`, a nijedan servis ne zna koji spiskovi postoje. Ovo je slika pravila razdvajanja komandi i upita sa servera. Komanda menja stanje i ne vraća prikaz, a prikaz se dobija upitom koji se sme pozvati bilo kada.

Zvanični vodič Angular-a preporučuje da se sav razgovor sa serverom, uključujući čitanje, zatvori u servise. Naš projekat od te preporuke odstupa za čitanje, jer je resurs vezan za stranicu upravo ono što čini osvežavanje trivijalnim. Za komande preporuku sledimo.

## Gde stanje živi

Isto pitanje se postavlja za svaki signal. **Stanje stranice** je stanje koje ima smisla samo dok je stranica otvorena i živi u polju stranice. **Stanje koje nadživljava stranicu** je stanje koje mora da bude isto na svakoj stranici i živi u signalu servisa sa `providedIn: 'root'`. Sledeća tabela daje primere iz projekta:

| Stanje | Gde živi | Primer |
|---|---|---|
| Stanje stranice | Polje stranice | Resurs, filter spiska, izabrani red, `pending`, `error`, model forme |
| Stanje koje nadživljava stranicu | Signal servisa u `core` | Prijavljeni korisnik u servisu `Auth` |

Prijavljeni korisnik je jedino stanje projekta koje nadživljava stranicu. Moduli ga čitaju kroz servis `Auth`, a menja ga samo taj servis, pri prijavi i odjavi. Svako drugo stanje koje bi neko poželeo da stavi u servis, poput izabranog filtera koji treba da preživi navigaciju, je odluka koja se donosi sa platformskim timom, a ne po navici.

## Grupa za autorstvo tura

Povežimo pojmove čitanjem cele grupe `tour-authoring` iz projekta. Sledeći kod prikazuje servis grupe:

```ts
const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateTourDto): Promise<TourDto> {
    return firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async addTransportTime(id: string, dto: TransportTimeDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/transport-times`, dto));
  }
}
```

Sledeći kod prikazuje stranicu `MyTours`, gde je izostavljena metoda koja dodaje vreme prevoza, jer prati oblik metode `publish`:

```ts
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly selectedTourId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly form = form(this.model, (path) => { ... });

  protected select(tourId: string): void {
    this.error.set(null);
    this.selectedTourId.set(tourId);
    this.form().reset({ transport: 'Walking', minutes: 30 });
  }

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

  protected async addTransportTime(event: Event): Promise<void> { ... }
}
```

Sledeći kod prikazuje članove stranice `CreateTour`, gde je izostavljeno sastavljanje DTO strukture iz forme:

```ts
export class CreateTour {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ name: '', description: '', difficulty: 'Easy' as TourDifficulty, tags: '' }), (path) => { ... });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.create({ ... });
      await this.router.navigate(['/exploration/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the tour.'));
    } finally {
      this.pending.set(false);
    }
  }
}
```

U datom kodu treba uočiti sledeće:

- Dve stranice preuzimaju isti servis. Servis ima jedno polje, `http`, i tri komande. Ne zna da postoje resurs `tours` ni stranica `CreateTour`.
- `MyTours` posle komande `publish` osvežava resurs `tours`, koji sama poseduje. Kada bi objavljena tura trebalo da se pojavi i na spisku objavljenih tura, ne bi bilo šta da se radi, jer stranica `TourList` pravi svoj resurs pri sledećem otvaranju.
- `CreateTour` posle komande `create` ne osvežava ništa, jer ne poseduje spisak. Otvara adresu stranice `MyTours`, koju radni okvir tada pravi zajedno sa novim resursom, pa nova tura stiže sa servera.
- `selectedTourId`, `pending`, `error` i model forme su stanje stranice. Kada korisnik otvori drugu adresu, radni okvir uništava stranicu i sa njom sva ta polja. Nijedno od njih ne bi imalo smisla u servisu, jer se odnosi na ono što korisnik trenutno radi na ovom ekranu.
- Nijedna od dve stranice ne čita stanje one druge. Sve što dele je servis bez stanja i server, koji je jedini izvor istine o turama.
