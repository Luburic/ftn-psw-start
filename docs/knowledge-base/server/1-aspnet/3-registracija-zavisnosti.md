> **Tip: lekcija.** Primeri u dokumentu su pojednostavljeni radi učenja koncepta i ne prate konvencije projekta do kraja.

ASP.NET Core nudi kontejner zavisnosti kao komponentu radnog okvira koja pravi objekte i popunjava njihove zavisnosti na osnovu registrovanih klasa. Pitanje je kako kontejner radi, koliko dugo žive objekti koje pravi i koje oblike registracije koristimo.

## Kako kontejner radi

U datoteci `Program.cs`, pre pokretanja aplikacije, registruju se klase koje radni okvir treba da pravi. Registracija ne pravi nijedan objekat. Ona samo beleži uputstvo koje kaže:
1. Koji tip se registruje,
2. Koja klasa se za njega instancira i
3. Koliko dugo napravljen objekat živi.

Kontejner je spisak ovakvih uputstava. Objekti nastaju tek tokom obrade zahteva. Kada rutiranje izabere akciju, kontejner instancira njen kontroler. Pri tome čita konstruktor kontrolera i za svaki parametar traži uputstvo u spisku registracija. Ako klasa iz uputstva i sama ima zavisnosti u konstruktoru, kontejner ponavlja isti postupak za njih, sve dok ne napravi i poveže ceo lanac. Ako za neki parametar ne postoji registracija, aplikacija pri obradi zahteva baca izuzetak koji navodi tip koji nedostaje. Kada je zahtev obrađen i odgovor poslat, kontejner oslobađa objekte koje je za taj zahtev napravio.

Podrazumevano se objekti prave iznova za svaki zahtev, ali to nije jedino moguće ponašanje. Koliko dugo objekat živi određujemo pri registraciji.

## Životni vek

**Životni vek** registracije određuje koliko dugo kontejner koristi jednom napravljen objekat pre nego što napravi novi. Pri registraciji biramo jedan od tri životna veka, kroz tri metode:

```cs
builder.Services.AddTransient<BookService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddSingleton<BookService>();
```

U datom kodu vidimo:
- `AddTransient` znači da kontejner pravi po jedan nov objekat za svako mesto gde je potreban. Kada dve klase u istom zahtevu zavise od iste registrovane klase, svaka dobija svoj objekat.
- `AddScoped` znači da kontejner pravi jedan objekat po HTTP zahtevu. Sve klase u lancu istog zahteva dele taj objekat, a naredni zahtev dobija nov objekat.
- `AddSingleton` znači da kontejner pravi jedan objekat za ceo život aplikacije. Taj objekat dele svi zahtevi svih korisnika.

Za aplikacione i infrastrukturne servisne klase je uobičajen izbor `AddScoped`. Životni vek `AddSingleton` biramo retko i pažljivo, jer objekat koji dele svi zahtevi ne sme da čuva podatke koji su potrebni zahtevima pojedinačnih korisnika.

## Tipične registracije

U nastavku razmatramo oblike registracije koje ćemo sretati u projektu.

### Registracija klase

Registracija oblika `AddScoped<BookService>()` kaže kontejneru da klasu `BookService` sam instancira kada je neki konstruktor zatraži, po pravilima izabranog životnog veka. Da bi instanciranje uspelo, klasa mora imati javni konstruktor čije sve parametre kontejner ume da napravi.

### Registracija interfejsa i implementacije

Registracija oblika `AddScoped<IBookRepository, BookDbRepository>()` određuje koja klasa stoji iza interfejsa od kog neka klasa zavisi. Kada bismo knjige čuvali u datoteci umesto u bazi podataka, promenili bismo samo registraciju u `AddScoped<IBookRepository, BookFileRepository>()`, a klasa `BookService` i sve ostale klase bi ostale netaknute.

### Registracija pozadinskog servisa

Sve dosadašnje registracije prave objekte tokom obrade zahteva. Neki poslovi, međutim, nisu vezani ni za jedan zahtev. Primer je priprema baze podataka pri pokretanju aplikacije, gde se definiše šema baze podataka i upisuju početni podaci pre nego što prvi zahtev stigne.

**Pozadinski servis** (engl. *hosted service*) je klasa koju radni okvir sam instancira i poziva pri pokretanju i gašenju aplikacije. Pozadinski servis implementira interfejs `IHostedService`, koji propisuje metode `StartAsync` i `StopAsync`. Radni okvir pri pokretanju aplikacije instancira svaki registrovani pozadinski servis i poziva njegovu metodu `StartAsync`, redosledom registracije, pre nego što aplikacija počne da obrađuje zahteve. Objekat zatim ostaje u memoriji do gašenja aplikacije, kada radni okvir poziva metodu `StopAsync`. Po životnom veku pozadinski servis liči na singleton, ali se od njega razlikuje po tome ko ga poziva. Singleton objekat čeka da ga neki zahtev zatraži kao zavisnost i njegov kod se izvršava samo tokom obrade zahteva, dok kod pozadinskog servisa radni okvir izvršava bez ijednog zahteva. Pozadinski servis tako može da odradi jednokratan posao pri pokretanju, ali i da tokom celog rada aplikacije izvršava poslove u pozadini.

Sledeći kod prikazuje pozadinski servis koji pri pokretanju aplikacije upisuje početne podatke:

```cs
public class BookModuleInitializer : IHostedService
{
  private readonly IServiceProvider _serviceProvider;

  public BookModuleInitializer(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();

    if (repository.GetAll().Count == 0)
    {
      repository.Create(new Book(1, "Na Drini ćuprija", "Ivo Andrić"));
    }
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

Pozadinski servis je potrebno registrovati putem metode `AddHostedService`:
```cs
// U datoteci Program.cs
builder.Services.AddHostedService<BookModuleInitializer>();
```

U datom kodu treba uočiti sledeće:
- Pozadinski servis živi koliko i aplikacija, pa kroz konstruktor ne sme da primi objekat sa životnim vekom vezanim za zahtev, poput repozitorijuma. U trenutku pokretanja nijedan zahtev ne postoji. Zato servis prima `IServiceProvider`, objekat kroz koji se od kontejnera direktno traže objekti.
- Poziv `CreateScope` pravi nov **scope**, područje unutar kog kontejner pravi objekte i na čijem kraju ih oslobađa. Radni okvir inače pravi po jedan scope za svaki HTTP zahtev, pa `AddScoped` znači jedan objekat po scope-u. Pošto pri pokretanju nema zahteva, scope pravimo sami: `GetRequiredService` u njemu traži repozitorijum od kontejnera, a naredba `using` obezbeđuje da se scope i njegovi objekti oslobode na kraju metode.
- Metoda `StopAsync` nema posao pri gašenju aplikacije, pa samo vraća završen zadatak.

### Grupisanje registracija

Sa rastom aplikacije raste i broj registracija, pa bi datoteka `Program.cs` vremenom postala nepregledan spisak. Zato se registracije jedne celine grupišu u metodu proširenja (engl. *extension method*), odnosno statičku metodu koju pozivamo kao da pripada tipu njenog prvog parametra. Sledeći kod prikazuje grupisane registracije za rad sa knjigama:

```cs
public static class BookModuleExtensions
{
    public static IServiceCollection AddBookModule(this IServiceCollection services)
    {
        services.AddScoped<BookService>();
        services.AddScoped<IBookRepository, BookDbRepository>();
        services.AddHostedService<BookModuleInitializer>();
        return services;
    }
}
```

```cs
// U datoteci Program.cs
builder.Services.AddBookModule(); // Services je tipa IServiceCollection (parametar metode iznad)
```

U datom kodu treba uočiti sledeće:
- Ključna reč `this` uz prvi parametar čini metodu metodom proširenja, pa se poziva kao `builder.Services.AddBookModule()`.
- Datoteka `Program.cs` sadrži jednu liniju po celini, a spisak registracija celine živi pored koda na koji se odnosi.

U našem projektu svaki modul ima ovakvu metodu, na primer `AddIdentityModule`, kojom registruje sve svoje klase.