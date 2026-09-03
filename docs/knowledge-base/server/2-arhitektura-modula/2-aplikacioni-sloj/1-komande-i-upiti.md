# Komande i upiti

> **Status: normativan.** Primeri u ovom dokumentu odgovaraju stvarnom kodu projekta i predstavljaju obavezan obrazac koji timovi prate.

Aplikacioni sloj modula objavljuje slučajeve upotrebe koje kontroleri pozivaju. Metoda aplikacionog sloja koja u istom pozivu menja stanje sistema i vraća složene podatke brzo postaje teška za razumevanje. Pozivalac takve metode ne zna da li sme da je pozove ponovo bez posledica, a sama metoda vremenom raste jer služi i izmeni i prikazu, čije se potrebe razlikuju. Zato svaku javnu metodu aplikacionog sloja pišemo tako da radi tačno jedno od ta dva.

**Komanda** (engl. *command*) je metoda aplikacionog sloja koja menja stanje sistema i ne vraća podatke, ili vraća najmanju potvrdu izmene. **Upit** (engl. *query*) je metoda aplikacionog sloja koja vraća podatke i ne menja ništa. Svaka javna metoda aplikacionog sloja je ili komanda ili upit, nikada oboje.

Aplikacioni sloj je organizovan po slučajevima upotrebe. Srodni slučajevi upotrebe čine grupu sa sopstvenim direktorijumom, na primer `TourAuthoring` za autorske slučajeve upotrebe i `TourBrowsing` za pregled objavljenih tura. Svaka grupa ima najviše dve klase. Komandna klasa, na primer `TourAuthoringService`, okuplja komande i kroz konstruktor prima repozitorijum agregata `ITourRepository` i jedinicu posla `IUnitOfWork`. Upitna klasa, na primer `TourBrowsingQueries`, okuplja upite i prima repozitorijum za čitanje `ITourReadRepository`. Oba interfejsa su deklarisana u aplikacionom sloju, a implementirana u infrastrukturnom.

Upiti se javljaju u dva oblika, pa ovaj dokument opisuje tri slučaja:

1. komanda,
2. čist upit,
3. upit koji koristi agregat.

## Slučaj 1: komanda

Komanda se uvek piše u tri koraka: učita agregat kroz repozitorijum, pozove jednu metodu na njemu i sačuva izmene tačno jednim pozivom metode `SaveChangesAsync`. Primer je objava ture iz klase `TourAuthoringService`.

```csharp
public async Task PublishAsync(Guid tourId, Guid authorId)
{
    var tour = await GetOwnedTourAsync(tourId, authorId);

    tour.Publish();
    await _unitOfWork.SaveChangesAsync();
}

private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
{
    var tour = await _tourRepository.GetByIdAsync(tourId);
    if (tour is null || tour.AuthorId != authorId)
    {
        throw new NotFoundException($"Tour {tourId} does not exist.");
    }
    return tour;
}
```

U primeru treba uočiti sledeće.

- Repozitorijum vraća ceo agregat `Tour`, a ne pojedinačne kolone. Komanda radi nad agregatom jer samo agregat sme da menja svoje stanje.
- Poslovna pravila objave, na primer najmanja dužina opisa, žive u metodi `Tour.Publish`. Kada pravilo nije ispunjeno, agregat izbacuje `DomainException`, a middleware taj izuzetak pretvara u HTTP odgovor 400. Komandna metoda zato nema nijedan `if` o poslovnom stanju.
- Tura koja ne postoji i tuđa tura obrađuju se istovetno, izuzetkom `NotFoundException` koji middleware pretvara u odgovor 404. Time se ne otkriva da li identifikator postoji.
- Poziv `SaveChangesAsync` na kraju je granica transakcije: sve što je izmenjeno od učitavanja upisuje se u jednoj transakciji. Komanda ga poziva tačno jednom.

## Slučaj 2: čist upit

**Čist upit** je upit koji se izvršava projekcijom podataka pravo u DTO, bez učitavanja agregata. Ovo je podrazumevani oblik upita i jedini ispravan izbor kada je rezultat spisak ili prikaz podataka. Primer je spisak objavljenih tura. U aplikacionom sloju upitna klasa ograničava parametre stranice i prosleđuje poziv repozitorijumu za čitanje.

```csharp
public Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
{
    page = Math.Max(page, 1);
    pageSize = Math.Clamp(pageSize, 1, 100);

    return _tourReadRepository.GetPublishedAsync(page, pageSize);
}
```

U infrastrukturnom sloju klasa `TourReadRepository` implementira interfejs `ITourReadRepository` i sastavlja upit nad bazom.

```csharp
public async Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
{
    var published = _dbContext.Tours
        .Where(tour => tour.Status == TourStatus.Published)
        .OrderByDescending(tour => tour.PublishedAt);

    var totalCount = await published.CountAsync();
    var items = await ProjectToDtos(published)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PageResult<TourDto>(items, totalCount);
}

private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
{
    return tours
        .AsNoTracking()
        .Select(tour => new TourDto(
            tour.Id,
            tour.AuthorId,
            tour.Name,
            ...));
}
```

U primeru treba uočiti sledeće.

- Poziv `AsNoTracking` isključuje praćenje promena u Entity Framework alatu. Učitani podaci se ne mogu izmeniti ni sačuvati, pa upit ni greškom ne može da promeni stanje.
- Metoda `Select` prevodi se u SQL projekciju, pa baza vraća samo kolone koje DTO sadrži. Agregat se nikada ne materijalizuje u memoriji.
- Spisak koji vremenom raste bez granice vraća se kroz tip `PageResult<T>` iz projekta `Shared.Domain`, koji nosi jednu stranicu rezultata i ukupan broj redova. Upitna klasa pre prosleđivanja ograničava brojeve stranice na razumne vrednosti, jer te vrednosti stižu iz HTTP zahteva.
- Prirodno mali spisak, na primer ture jednog autora, sme da ostane bez straničenja.

## Slučaj 3: upit koji koristi agregat

Ovaj obrazac trenutno nema primer u kodu projekta. Kada se ovakav upit prvi put pojavi, piše se na način opisan u nastavku i taj primer tada postaje deo ovog dokumenta.

**Izvedena vrednost** je podatak koji se izračunava iz stanja jednog agregata, na primer prosečno vreme obilaska ture. Logika izračunavanja pripada agregatu, kao metoda bez propratnih efekata: metoda čita sopstveno stanje, vraća vrednost i ne menja ništa. Kada slučaj upotrebe traži izvedenu vrednost, upitna klasa učitava agregat kroz običan repozitorijum, pozove metodu i upakuje rezultat u DTO.

```csharp
public async Task<TourStatisticsDto> GetStatisticsAsync(Guid tourId)
{
    var tour = await _tourRepository.GetByIdAsync(tourId);
    if (tour is null)
    {
        throw new NotFoundException($"Tour {tourId} does not exist.");
    }
    return new TourStatisticsDto(tour.Id, tour.AverageTransportMinutes());
}
```

U primeru treba uočiti sledeće.

- Upitna klasa za ovaj slučaj kroz konstruktor prima i `ITourRepository`, pored repozitorijuma za čitanje.
- Metoda `AverageTransportMinutes` živi u klasi `Tour` i izračunava vrednost iz njenog stanja. Ista logika se ne piše ponovo kao SQL projekcija, jer bi tada postojala na dva mesta.
- Metoda nikada ne poziva `SaveChangesAsync`. Učitani agregat se odbacuje na kraju obrade zahteva, pa i slučajna izmena stanja ne bi bila upisana.

Ovaj oblik je izuzetak, a ne podrazumevani izbor. Kada rezultat služi prikazu spiska ili obuhvata više agregata, važi slučaj 2. Metode za izvedene vrednosti ne pišu se unapred, nego tek kada ih neki slučaj upotrebe zatraži.

## Kako odlučiti

Za svaki novi zahtev postavljaju se tri pitanja, redom:

1. Da li zahtev menja stanje? Ako da, piše se komanda: učitaj agregat, pozovi metodu, sačuvaj jednom.
2. Da li je rezultat spisak ili prikaz podataka? Ako da, piše se čist upit: projekcija u DTO kroz repozitorijum za čitanje.
3. Da li je rezultat izvedena vrednost koju jedan agregat izračunava iz svog stanja? Ako da, piše se upit koji koristi agregat.

Razmotrimo redom zahteve nad turama. Objava ture menja stanje, pa je to komanda i staje na prvom pitanju. Spisak objavljenih tura ne menja stanje i jeste prikaz spiska, pa je to čist upit i staje na drugom pitanju. Prosečno vreme obilaska jedne ture ne menja stanje, nije spisak, a izračunava se iz stanja jedne ture, pa je to upit koji koristi agregat.

Iza sva tri pitanja stoji jedno tvrdo pravilo: upit ne menja stanje i nikada ne poziva `SaveChangesAsync`. Posledica u kodu je vidljiva iz konstruktora, jer upitna klasa nikada ne prima `IUnitOfWork`. Ovo pravilo čuva i arhitektonski test u projektu `Host.Tests`.
