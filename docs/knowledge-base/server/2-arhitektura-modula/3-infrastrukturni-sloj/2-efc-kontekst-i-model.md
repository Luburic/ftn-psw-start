U ADO.NET kodu je programer u svaku SQL naredbu upisivao naziv tabele i nazive kolona. Uvođenjem ORM-a ti detalji nestaju iz koda repozitorijuma. Međutim, baza podataka i dalje ima tabele i kolone, a maper ispod haube i dalje piše njihove nazive. Maper zato mora da zna kako se klase preslikavaju na tabele. Ovde razmatramo odakle Entity Framework Core (u nastavku EFC) dolazi do tog znanja.

## Model mapiranja

**Model mapiranja** je skup pravila po kojima ORM preslikava klase na tabele, svojstva na kolone i veze između objekata na strane ključeve. ORM gradi model iz dva izvora:
1. **Konvencije** su pravila koja maper primenjuje sam, na osnovu oblika klasa, bez ikakvog našeg uputstva.
2. **Konfiguracija** je skup pravila koja programer izričito navodi kako bi izmenio ponašanje podrazumevano konvencijama.

Konvencije pokrivaju veći deo posla, jer se često tabela izvodi direktno iz oblika klasa domenskog sloja. Konfiguracija ostaje za mesta na kojima domenski model namerno skriva ono što bazi treba, kao što su kolekcije iza polja, vrednosni objekti bez identifikatora i identifikatori koje ne dodeljuje baza.

## Kontekstna klasa

**Kontekstna klasa** (engl. *context*) je klasa koja nasleđuje `DbContext` iz EFC biblioteke, drži model mapiranja jednog modula i predstavlja jedinu tačku kroz koju kod modula razmenjuje podatke sa bazom. Sledeći kod prikazuje kontekstnu klasu modula za ankete:

```cs
public sealed class SurveyDbContext : DbContext
{
  public SurveyDbContext(DbContextOptions<SurveyDbContext> options) : base(options) { }

  public DbSet<Survey> Surveys => Set<Survey>();
  public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.HasDefaultSchema("surveys");
    builder.ConfigureSurveys();
    builder.ConfigureResponses();
  }
}
```

U datom kodu treba uočiti sledeće:

- Svojstvo tipa `DbSet<T>` postoji za svaki koren agregata i samo za koren. Kroz `Surveys` se učitava i dodaje cela anketa, a `Question` i `Option` do konteksta stižu isključivo kroz nju. Unutrašnji objekti nemaju `DbSet`.
- Metoda `OnModelCreating` se izvršava jednom, pri prvoj upotrebi konteksta, i tada EFC gradi model mapiranja. Poziv `HasDefaultSchema` smešta sve tabele modula u šemu `surveys`. Ovaj mehanizam nam omogućava da u modularnom monolitu svaki modul ima sopstvenu šemu sa svojim tabelama.
- Pozivi `ConfigureSurveys` i `ConfigureResponses` su naše *metoda proširenja*, po jedna za svaki agregat modula, i svaka je opisana u nastavku. Ovako razlažemo konfiguraciju na više klasa sa značajnim nazivima, umesto da cela konfiguracija stane u ovu jednu metodu. Nije neophodno definisati metodu za svaki agregat, ali će se onda mapiranje izvršiti po konvencijama.

<hr></hr>
<details>
<summary><b>Klikni za detalje o tome kako kontekst zna sa kojom bazom podataka radi</b></summary>

U prethodnom kodu vidimo da konstruktor prima `DbContextOptions`, objekat koji nosi konekcioni string i izbor baze podataka. **Konekcioni string** (engl. *connection string*) je tekst koji sadrži sve podatke potrebne da se otvori konekcija ka bazi: adresu servera, port, naziv baze, korisničko ime i lozinku. Čuva se u konfiguracionoj datoteci aplikacije, pod imenom po kom ga kod traži:

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=explorer;Username=postgres;Password=admin"
  }
}
```

Kontekst se registruje u kontejner zavisnosti ([Registracija zavisnosti](../../1-aspnet/3-registracija-zavisnosti.md)) pozivom koji iz konfiguracije čita konekcioni string i bira biblioteku za konkretnu bazu:

```cs
services.AddDbContext<SurveyDbContext>(options =>
  options.UseNpgsql(configuration.GetConnectionString("Database")));
```

Poziv `AddDbContext` registruje kontekst sa životnim vekom jednog zahteva, pa sve klase koje tokom obrade jednog HTTP zahteva zatraže `SurveyDbContext` dobijaju istu instancu. Poziv `UseNpgsql` bira biblioteku koja EFC povezuje sa PostgreSQL bazom.

Za rad sa EFC, projekat referencira tri biblioteke:
- `Microsoft.EntityFrameworkCore`, koji uvodi koncepte konteksta i modela mapiranja,
- `Npgsql.EntityFrameworkCore.PostgreSQL` koji definiše specifičnosti prevođenja na PostgreSQL i
- `Microsoft.EntityFrameworkCore.Design` koji definiše alate za migracije, sa kojim ćemo se upoznati kasnije.

</details>
<hr></hr>

## Konvencije

Kada bismo kontekst ostavili bez ijedne klase konfiguracije, EFC bi iz oblika klasa izveo sledeća pravila:

- Za svaki `DbSet` nastaje tabela sa nazivom svojstva, pa `Surveys` daje tabelu `Surveys`.
- Svojstvo sa nazivom `Id` je primarni ključ. Za ključ tipa `Guid` EFC pretpostavlja da vrednost generiše on sam, pri dodavanju objekta, ako vrednost nije postavljena.
- Svako svojstvo prostog tipa daje kolonu istog naziva, sa tipom kolone izvedenim iz tipa svojstva. Tako `Guid` daje `uuid`, `string` daje `text`, `bool` daje `boolean`, a enumeracija daje `integer` sa rednim brojem vrednosti.
- Svojstvo čiji tip dopušta `null`, poput `string?`, daje kolonu koja dopušta `NULL`. Bez upitnika kolona dobija ograničenje `NOT NULL`.

### Konvencije za povezane klase

EFC prati određene konvencije kada naiđe na klasu koja ima asocijaciju ka drugoj klasi. Posmatrajmo klase **A** i **B**:
- Ako klasa **A** ima svojstvo tipa **B** i **B** ima svojstvo `Id`, pravi se tabela za klasu B, čak i ako ne postoji `DbSet<B>` (tada se za naziv tabele koristi naziv klase).
- Ako klasa **A** ima svojstvo tipa **B**, EFC tretira `A` kao zavisnu stranu veze 1:N i u tabeli `A` pravi kolonu `BId`.
- Ako klasa **A** ima kolekcijsko svojstvo `List<B>`, a **B** nema kolekcijsko svojstvo ka `A`, EFC vezu tretira kao **1:N**. Strani ključ ka `A` se nalazi u tabeli za `B`.
- Ako klasa **A** ima svojstvo tipa `List<B>`, a klasa **B** ima svojstvo tipa `List<A>`, EFC vezu tretira kao **M:N** i automatski pravi međutabelu (join tabelu). Tabela se zove **AB** i ima kolone za strane ključeve ka **A** i **B**.

## Konfiguracija

EFC mapiranje konfigurišemo kada želimo da izmenimo konvencije. U našem projektu uvodimo **klasu konfiguracije mapiranja** za svaki agregat, da na jednom mestu grupišemo pravila mapiranja jednog agregata. Sledeći kod prikazuje oblik takve klase za agregat ankete:

```cs
internal static class SurveyConfiguration
{
  public static void ConfigureSurveys(this ModelBuilder modelBuilder)
  {
    ConfigureSurvey(modelBuilder.Entity<Survey>());
    ConfigureQuestion(modelBuilder.Entity<Question>());
  }

  private static void ConfigureSurvey(EntityTypeBuilder<Survey> builder) { ... }

  private static void ConfigureQuestion(EntityTypeBuilder<Question> builder) { ... }
}
```

U datom kodu treba uočiti sledeće:

- Javna metoda je metoda proširenja nad `ModelBuilder`-om, na šta ukazuje reč `this` ispred prvog parametra. Zahvaljujući tome je kontekst poziva kao `builder.ConfigureSurveys()`, kao da pripada samom `ModelBuilder`-u. Isti postupak koristimo pri registraciji zavisnosti modula.
- Poziv `modelBuilder.Entity<T>()` vraća objekat tipa `EntityTypeBuilder<T>`, nad kojim se navode pravila za tip `T`. Javna metoda time samo nabraja tipove agregata, a pravila svakog tipa drži zasebna privatna metoda.

Sada ćemo analizirati tela metoda za konfiguraciju navedenih agregata. Ovaj kod treba posmatrati kao primer mogućih konfiguracija, ne kao strogo pravilo koje se mora primeniti i na vaše agregate.

Pravila korena agregata su sledeća:

```cs
private static void ConfigureSurvey(EntityTypeBuilder<Survey> builder)
{
  builder.Property(survey => survey.Title).HasMaxLength(200);
  builder.Property(survey => survey.Status).HasConversion<string>();

  builder.HasMany(survey => survey.Questions)
    .WithOne()
    .IsRequired()
    .OnDelete(DeleteBehavior.Cascade);
}
```

U datom kodu treba uočiti sledeće:

- Poziv `HasMaxLength` menja tip kolone iz `text` u `character varying(200)`. Konvencija ne zna koliko naslov sme da bude dug, a baza koja dužinu zna odbija predugačak zapis.
- Poziv `HasConversion<string>` čuva naziv vrednosti enumeracije umesto njenog rednog broja. Kolona `Status` tako sadrži `Published`, a ne `1`, pa se čita bez uvida u kod. Dodavanje nove vrednosti u sredinu nabrajanja pri tome ne menja značenje već sačuvanih redova.
- Vezu između ankete i pitanja je konvencija već napravila, zajedno sa kolonom stranog ključa `SurveyId`. Ovaj lanac poziva je ne uvodi, već ispravlja dve njene osobine po sledećim pravilima:
   - Preduslov je da prvo imenujemo vezu o kojoj govorimo. Pozivi `HasMany` i `WithOne` imenuju tu vezu, svaki sa svoje strane: anketa ima više pitanja, a pitanje pripada jednoj anketi. `WithOne` je bez argumenta jer klasa `Question` nema svojstvo `Survey` kojim bi upućivala nazad; da ga ima, pisali bismo `WithOne(question => question.Survey)`.
   - Poziv `IsRequired` stranom ključu dodaje ograničenje `NOT NULL`. Konvencija vezu ostavlja neobaveznom, jer iz oblika klasa ne može da zna da pitanje bez ankete ne postoji.
   - Poziv `OnDelete`, sa vrednošću `Cascade`, nalaže bazi da pri brisanju ankete obriše i njena pitanja. Anketa i njena pitanja čine jedan agregat, pa nastaju i nestaju zajedno.

Pravila unutrašnjeg entiteta ispravljaju dve konvencije i uvode pravilo za vrednosni objekat:

```cs
private static void ConfigureQuestion(EntityTypeBuilder<Question> builder)
{
  builder.ToTable("Questions");
  builder.Property(question => question.Id).ValueGeneratedNever();

  builder.OwnsMany(question => question.Options, options => options.ToJson());
}
```

U datom kodu treba uočiti sledeće:

- Poziv `ToTable` daje tabeli naziv u množini, kakav imaju tabele korena.
- Poziv `ValueGeneratedNever` saopštava da vrednost ključa ne treba da dodeli EFC (u našem slučaju dodeljuje konstruktor). Bez toga EFC za pitanje koje se pojavilo u kolekciji ankete pretpostavlja da već postoji u bazi, jer mu je ključ popunjen, pa pri čuvanju umesto unosa pokušava izmenu nepostojećeg reda i prijavljuje grešku. Koren agregata se kontekstu predaje izričito, pa za njega ta pretpostavka ne važi.
- Poziv `OwnsMany` saopštava da `Option` nije entitet, već vrednosni objekat koji pripada pitanju. Vrednosni objekat nema identifikator, pa mu ne dajemo sopstvenu tabelu. Poziv `ToJson` sve opcije jednog pitanja smešta u jednu kolonu `Options` tipa `jsonb`, gde je svaka opcija JSON objekat sa svojim svojstvima. Isto pravilo mapira `Answer` objekte unutar `SurveyResponse` agregata.

## Rehidracija

**Rehidracija** (engl. *rehydration*) je postupak kojim ORM od sačuvanih podataka pravi domenski objekat. EFC pri tome pozove konstruktor bez parametara, a zatim vrednosti kolona upiše neposredno u svojstva i polja klase, uključujući svojstva sa privatnim `set` pristupnikom i privatna polja iza kolekcija.

Svaka klasa koju EFC mapira zato dobija privatni konstruktor bez parametara. Kada takav konstruktor ne bi postojao, EFC bi pozvao javni konstruktor čiji parametri odgovaraju svojstvima klase, pa bi domenska pravila proveravao pri svakom čitanju. Konstruktor je privatan, pa ga kod domenskog i aplikacionog sloja ne vidi. Sledeći kod naglašava ove elemente koda:

```cs
public sealed class Survey
{
  private Survey() { }

  public Survey(string title) { ... }

  // ... prethodno definisana svojstva i metode
}

public sealed record Option
{
  public string Value { get; private init; }

  private Option() { }

  public Option(string value) { ... }
}
```

U datom kodu treba uočiti sledeće:

- Privatni konstruktor bez parametara ne dodeljuje ništa. Svaku vrednost upisuje EFC nakon što ga pozove.
- Svojstvo vrednosnog objekta dobija `init` pristupnik, jer EFC ne može da upiše vrednost u svojstvo bez pristupnika za upis. Nepromenljivost je očuvana, jer `init` dopušta upis samo tokom konstrukcije objekta, a to je jedini trenutak u kom EFC upisuje vrednost.

## Tabele modula

Kada se konvencije i konfiguracija saberu, modul za ankete u šemi `surveys` dobija tri tabele. Kolona `SurveyId` u tabeli `Questions` je strani ključ, a istoimena kolona u tabeli `SurveyResponses` nije.

| Tabela | Kolone |
| --- | --- |
| `Surveys` | `Id` uuid, `Title` character varying(200), `Status` character varying(20) |
| `Questions` | `Id` uuid, `Text` text, `IsArchived` boolean, `Options` jsonb, `SurveyId` uuid |
| `SurveyResponses` | `Id` uuid, `SurveyId` uuid, `Answers` jsonb |

Ovaj prikaz opisuje šta model mapiranja kaže, a ne stanje baze. Kako se od modela dolazi do stvarnih tabela i kako one prate izmene modela tokom razvoja obrađuje [lekcija o migracijama](3-migracije.md).
