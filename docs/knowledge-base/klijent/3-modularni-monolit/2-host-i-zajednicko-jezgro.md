Na serveru host aplikacija sastavlja module, a zajedničko jezgro drži kod koji koriste svi moduli. Klijent ima ista dva mesta, direktorijume `core` i `shared`. Čitalac je iz oba već uvozio, servis `Auth` iz prvog, a `PageResult` i `serverMessage` iz drugog, bez pravila o tome šta u njima pripada i ko sme da ih menja. Ovde ta dva direktorijuma čitamo do kraja i utvrđujemo ko šta menja.

## Host aplikacija

Na klijentu **host aplikaciju** čine direktorijum `core` i konfiguracija `app.config.ts` iz korena. Host aplikacija pokreće aplikaciju, drži tabelu ruta koja imenuje svaki modul i drži identitet prijavljenog korisnika. Sledeće stablo prikazuje deo projekta van modula:

```
app/
  app.config.ts
  app.ts
  core/
    app.routes.ts
    auth/
      auth.ts
      auth-interceptor.ts
      login/
      register/
    home/
  modules/
  shared/
```

U datom stablu treba uočiti sledeće:

- Datoteka `app.config.ts` uključuje delove radnog okvira: rutiranje sa tabelom iz `core/app.routes.ts` i razmenu sa serverom sa presretačem, koji upoznajemo u narednom odeljku. Korenska komponenta `app.ts` drži zaglavlje i mesto iscrtavanja.
- Tabela `core/app.routes.ts` je tabela aplikacije iz prethodne lekcije, jedino mesto na kom host aplikacija imenuje module.
- Direktorijum `core/auth` drži servis `Auth`, stranice prijave i registracije i presretač. Prijavljeni korisnik, jedino stanje koje nadživljava stranicu, živi ovde.
- Direktorijum `core/home` je početna stranica, koja ne pripada nijednom modulu.
- Modul iz host aplikacije uvozi samo `core/auth`, da bi pročitao prijavljenog korisnika. Ništa drugo iz `core` modul ne uvozi, a host aplikacija modul poznaje samo kroz stavku tabele ruta.

## Presretač

**Presretač** (engl. *interceptor*) je funkcija koju radni okvir poziva sa svakim HTTP zahtevom pre nego što ga pošalje i koja zahtev sme da izmeni. Sledeći kod prikazuje jedini presretač u projektu i red konfiguracije koji ga uključuje:

```ts
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(Auth).accessToken();
  if (token === null) {
    return next(request);
  }
  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
```

```ts
provideHttpClient(withInterceptors([authInterceptor])),
```

U datom kodu treba uočiti sledeće:

- Tip `HttpInterceptorFn` je tip funkcije sa dva parametra: zahtev i funkcija `next`, koja zahtev prosleđuje dalje ka serveru. Presretač mora da pozove `next`, sa istim ili izmenjenim zahtevom, i da vrati njen rezultat.
- Presretač servis preuzima pozivom `inject`, kao i komponenta. Signal `accessToken` servisa `Auth` drži token koji server izdaje pri prijavi.
- Bez tokena zahtev prolazi neizmenjen. Sa tokenom presretač pravi kopiju zahteva sa zaglavljem `Authorization`, jer se zahtev ne sme menjati na mestu, i prosleđuje kopiju.
- Poziv `withInterceptors` u `app.config.ts` vezuje presretač za svaki zahtev koji šalju resurs i `HttpClient`, iz bilo kog modula.
- Posledica za kod modula je da nijedna stranica ni servis ne zna za token. Odgovor sa statusnim kodom 401 znači da korisnik nije prijavljen ili da je token istekao, a ne da stranica nije poslala token. Presretač piše platformski tim, a ovde ga čitamo da bismo znali šta se zahtevu dešava između stranice i servera.

## Zajedničko jezgro

Na klijentu je **zajedničko jezgro** direktorijum `shared`, koji drži kod koji koriste svi moduli. Sledeće stablo prikazuje ceo direktorijum iz projekta:

```
shared/
  api/
    page-result.ts
    problem-details.ts
  util/
    server-message.ts
```

Direktorijum `shared/api` drži preslikane tipove struktura koje server vraća svim modulima. Pored `PageResult` za spisak sa stranama, tu je `ProblemDetails`, preslikani tip tela odgovora sa greškom koje pravi middleware servera:

```ts
export interface ProblemDetails {
  type: string | null;
  title: string | null;
  status: number | null;
  detail: string | null;
  instance: string | null;
}
```

Direktorijum `shared/util` drži funkciju `serverMessage`, čiji potpis znamo iz lekcije o komandama. Sledeći kod prikazuje njeno telo:

```ts
export function serverMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as ProblemDetails | null;
    if (typeof problem?.title === 'string') {
      return problem.title;
    }
  }
  return fallback;
}
```

U datom kodu treba uočiti sledeće:

- Pravilo o izmeni preslikanih tipova važi i za `shared/api`. Struktura `ProblemDetails` se menja samo kada se promeni telo odgovora sa greškom na serveru.
- Klasa `HttpErrorResponse` je izuzetak koji `HttpClient` baca za odgovor sa statusnim kodom greške. Provera `instanceof` sužava tip `unknown` na taj izuzetak, pa funkcija sme da čita njegovo svojstvo `error`, koje nosi telo odgovora.
- Telo se tumači kao `ProblemDetails` zapisom `as`, jer prevodilac ne zna šta je server poslao. Kada telo ima `title`, funkcija vraća njega, a u svakom drugom slučaju drugi parametar.

Kod ulazi u `shared` **promocijom**, kao i u zajedničko jezgro na serveru. Kada drugi modul zatraži isto što jedan modul već ima, platformski tim to premešta iz modula u `shared`. Globalna klasa u datoteci `_components.scss` prati isto pravilo, jer je i ona zajednički kod. Pomoćna funkcija koju koristi jedan modul ostaje u tom modulu.

## Vlasništvo

Sledeća tabela sažima ko koji direktorijum menja i šta modul iz njega sme da uveze:

| Direktorijum | Ko menja | Šta modul uvozi |
|---|---|---|
| `core` | Platformski tim | Samo `core/auth` |
| `shared` | Platformski tim | Sve |
| `styles` | Platformski tim | Ništa, globalne klase se koriste u šablonu |
| `modules/<naziv>` | Tim tog modula | Samo `public-api.ts` drugog modula |

## Put jednog zahteva

Povežimo pojmove. Prijavljeni korisnik otvori spisak objavljenih blogova, a stranica `BlogList` modula Social ima resurs sa adresom `/api/social/blogs/published?page=1&pageSize=20`. Dešava se sledeće:

1. Resurs pri prvom iscrtavanju stranice sastavlja zahtev na tu adresu.
2. Radni okvir pre slanja poziva presretač iz `core/auth`, koji zahtevu dodaje zaglavlje `Authorization` sa tokenom iz servisa `Auth`.
3. Server iz tokena prepoznaje korisnika i vraća stranicu blogova. Resurs odgovor upisuje u `value` kao `PageResult<BlogDto>`, gde je `PageResult` iz `shared/api`, a `BlogDto` iz direktorijuma `api` modula Social.
4. Kada bi server umesto toga vratio odgovor sa greškom, resurs bi grešku upisao u signal `error`, a stranica koja šalje komandu bi poruku iz tela pročitala funkcijom `serverMessage` iz `shared/util`, koja telo tumači kao `ProblemDetails` iz `shared/api`.

Na celom putu je tim modula Social napisao samo stranicu i njen preslikani tip. Presretač, token, tipove odgovora i čitanje poruke o grešci dobio je od host aplikacije i zajedničkog jezgra, isto kao i svaki drugi modul.
