U ADO.NET kodu je programer u svaku SQL naredbu upisivao naziv tabele i nazive kolona. U [lekciji o objektno-relacionom mapiranju](1-orm.md) su te naredbe nestale iz repozitorijuma, ali baza podataka i dalje ima tabele i kolone, a maper i dalje piše iste naredbe. Maper zato mora da zna kako se klase preslikavaju na tabele. Ovde razmatramo odakle Entity Framework Core (u nastavku EF) dolazi do tog znanja i šta mu saopštavamo tamo gde bi sam pogrešio.

## Model mapiranja

**Model mapiranja** (engl. *model*) je skup pravila po kojima maper preslikava klase na tabele, svojstva na kolone i veze između objekata na strane ključeve. Maper model gradi iz dva izvora. **Konvencije** (engl. *conventions*) su pravila koja maper primenjuje sam, na osnovu oblika klasa, bez ikakvog našeg uputstva. **Konfiguracija** (engl. *configuration*) je skup pravila koja programer izričito navodi tamo gde konvencija ne postoji ili daje pogrešan rezultat.

Konvencije pokrivaju veći deo posla, jer se iz oblika klasa domenskog sloja tabela izvodi neposredno. Konfiguracija ostaje za mesta na kojima domenski model namerno skriva ono što bazi treba, kao što su kolekcije iza polja, vrednosni objekti bez identifikatora i identifikatori koje ne dodeljuje baza.

## Kontekstna klasa

**Kontekstna klasa** (engl. *context*) je klasa koja nasleđuje `DbContext` iz EF biblioteke, drži model mapiranja jednog modula i predstavlja jedinu tačku kroz koju kod modula razmenjuje podatke sa bazom. Sledeći kod prikazuje kontekstnu klasu modula za ankete:

```cs
public sealed class SurveyDbContext : DbContext
{
  public SurveyDbContext(DbContextOptions<SurveyDbContext> options) : base(options) { }

  public DbSet<Survey> Surveys => Set<Survey>();
  public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.HasDefaultSchema("surveys");
    builder.ApplyConfigurationsFromAssembly(typeof(SurveyDbContext).Assembly);
  }
}
```

U datom kodu treba uočiti sledeće:

- Konstruktor prima `DbContextOptions`, objekat koji nosi konekcioni string i izbor baze podataka. Kontekst ga ne pravi sam, već ga dobija od kontejnera zavisnosti pri svakom instanciranju, kako je opisano u bloku na kraju ovog odeljka.
- Svojstvo tipa `DbSet<T>` postoji za svaki koren agregata i samo za koren. Kroz `Surveys` se učitava i dodaje cela anketa, a `Question` i `Option` do konteksta stižu isključivo kroz nju. Unutrašnji objekti nemaju `DbSet` iz istog razloga iz kog nemaju repozitorijum.
- Metoda `OnModelCreating` se izvršava jednom, pri prvoj upotrebi konteksta, i tada EF gradi model mapiranja. Poziv `HasDefaultSchema` smešta sve tabele modula u šemu `surveys`, pa tabele različitih modula ne dele prostor imena u istoj bazi.
- Poziv `ApplyConfigurationsFromAssembly` pronalazi sve klase konfiguracije u projektu infrastrukturnog sloja i primenjuje ih. Te klase upoznajemo u nastavku, po jednu za svaki tip, umesto da cela konfiguracija stane u ovu metodu.

<hr></hr>
<details>
<summary><b>Klikni da vidiš kako kontekst dobija bazu podataka</b></summary>

**Konekcioni string** (engl. *connection string*) je tekst koji sadrži sve podatke potrebne da se otvori konekcija ka bazi: adresu servera, port, naziv baze, korisničko ime i lozinku. Čuva se u konfiguracionoj datoteci aplikacije, pod imenom po kom ga kod traži:

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

Poziv `AddDbContext` registruje kontekst sa životnim vekom jednog zahteva, pa sve klase koje tokom obrade jednog HTTP zahteva zatraže `SurveyDbContext` dobijaju istu instancu. Poziv `UseNpgsql` bira biblioteku koja EF povezuje sa PostgreSQL bazom. U našem projektu se ovaj poziv nalazi u pomoćnoj metodi zajedničkog jezgra, koja uz kontekst navodi i šemu modula, tako da svaki modul svoje tabele drži u sopstvenoj šemi iste baze.

Za rad sa EF projekat referencira tri biblioteke: `Microsoft.EntityFrameworkCore` sa kontekstom i modelom, `Npgsql.EntityFrameworkCore.PostgreSQL` sa prevođenjem na PostgreSQL i `Microsoft.EntityFrameworkCore.Design` sa alatima za migracije. Verzije sve tri biblioteke održava platformski tim.

</details>
<hr></hr>

## Konvencije

Kada bismo kontekst ostavili bez ijedne klase konfiguracije, EF bi iz oblika klasa izveo sledeća pravila:

- Za svaki `DbSet` nastaje tabela sa nazivom svojstva, pa `Surveys` daje tabelu `Surveys`. Klasa do koje EF stigne kroz svojstvo neke druge klase, a nema sopstveni `DbSet`, dobija tabelu sa nazivom klase, pa `Question` daje tabelu `Question`.
- Svojstvo sa nazivom `Id` je primarni ključ. Za ključ tipa `Guid` EF pretpostavlja da vrednost generiše on sam, pri dodavanju objekta, ako vrednost nije postavljena.
- Svako svojstvo prostog tipa daje kolonu istog naziva, sa tipom kolone izvedenim iz tipa svojstva. Tako `Guid` daje `uuid`, `string` daje `text`, `bool` daje `boolean`, a nabrojivi tip daje `integer` sa rednim brojem vrednosti.
- Svojstvo čiji tip dopušta `null`, poput `string?`, daje kolonu koja dopušta `NULL`. Bez upitnika kolona dobija ograničenje `NOT NULL`.
- Svojstvo tipa kolekcije druge klase sa identifikatorom daje vezu jedan prema više, pri čemu tabela te klase dobija strani ključ ka tabeli klase koja kolekciju sadrži. Strani ključ dopušta `NULL`, pa veza nije obavezna.

Dva pravila domenskog sloja imaju neposredan odraz u konvencijama. Svojstvo `SurveyId` klase `SurveyResponse` je tipa `Guid`, pa daje običnu kolonu, bez stranog ključa i bez veze između tabela, jer agregat na drugi agregat upućuje samo identifikatorom. Svojstvo `Questions` klase `Survey` je kolekcija entiteta, pa `Question` dobija sopstvenu tabelu sa stranim ključem ka anketi, jer je unutrašnji entitet deo agregata.

## Konfiguracija

**Klasa konfiguracije** (engl. *entity type configuration*) je klasa koja implementira interfejs `IEntityTypeConfiguration<T>` i u metodi `Configure` navodi pravila mapiranja za tip `T`. Pravila se izražavaju ulančanim pozivima metoda nad parametrom te metode. Sledeći kod prikazuje konfiguraciju korena agregata ankete:

```cs
public sealed class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
  public void Configure(EntityTypeBuilder<Survey> builder)
  {
    builder.Property(survey => survey.Title).HasMaxLength(200);
    builder.Property(survey => survey.Status).HasConversion<string>().HasMaxLength(20);

    builder.HasMany(survey => survey.Questions)
      .WithOne()
      .HasForeignKey("SurveyId")
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);
  }
}
```

U datom kodu treba uočiti sledeće:

- Poziv `HasMaxLength` menja tip kolone iz `text` u `character varying(200)`. Konvencija ne zna koliko naslov sme da bude dug, a baza koja dužinu zna odbija predugačak zapis.
- Poziv `HasConversion<string>` čuva naziv vrednosti nabrojivog tipa umesto njenog rednog broja. Kolona `Status` tako sadrži `Published`, a ne `1`, pa se čita bez uvida u kod. Dodavanje nove vrednosti u sredinu nabrajanja pri tome ne menja značenje već sačuvanih redova.
- Poziv `HasMany` opisuje kolekciju pitanja, a `WithOne` bez argumenta saopštava da pitanje nema svojstvo koje upućuje nazad na anketu. Poziv `HasForeignKey` imenuje kolonu stranog ključa, koja postoji u tabeli `Questions`, a ne u klasi `Question`.
- Pozivi `IsRequired` i `OnDelete` ispravljaju konvenciju po kojoj veza nije obavezna. Prvi stranom ključu dodaje ograničenje `NOT NULL`, jer pitanje bez ankete ne postoji. Drugi, sa vrednošću `Cascade`, nalaže bazi da pri brisanju ankete obriše i njena pitanja.

Konfiguracija unutrašnjeg entiteta ispravlja dve konvencije i uvodi pravilo za vrednosni objekat:

```cs
public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
  public void Configure(EntityTypeBuilder<Question> builder)
  {
    builder.ToTable("Questions");
    builder.Property(question => question.Id).ValueGeneratedNever();

    builder.OwnsMany(question => question.Options, options => options.ToJson());
  }
}
```

U datom kodu treba uočiti sledeće:

- Poziv `ToTable` daje tabeli naziv u množini, kakav imaju tabele korena.
- Poziv `ValueGeneratedNever` saopštava da vrednost ključa dodeljuje konstruktor, kako je opisano u [lekciji o entitetu](../1-domenski-sloj/3-entitet.md). Bez toga EF za pitanje koje se pojavilo u kolekciji ankete pretpostavlja da već postoji u bazi, jer mu je ključ popunjen, pa pri čuvanju umesto unosa pokušava izmenu nepostojećeg reda i prijavljuje grešku. Koren agregata se kontekstu predaje izričito, pa za njega ta pretpostavka ne važi.
- Poziv `OwnsMany` saopštava da `Option` nije entitet, već vrednosni objekat koji pripada pitanju. Vrednosni objekat nema identifikator, pa mu ne dajemo sopstvenu tabelu. Poziv `ToJson` sve opcije jednog pitanja smešta u jednu kolonu `Options` tipa `jsonb`, gde je svaka opcija JSON objekat sa svojim svojstvima. Isto pravilo mapira `Answer` objekte unutar `SurveyResponse` agregata.

## Rehidracija

Konstruktor domenske klase prima podatke, proverava pravila i tek onda formira objekat. Kada EF čita red iz baze, taj postupak nema smisla, jer su pravila proverena kada je objekat prvi put nastao, a sve izmene od tada su prošle kroz metode koje pravila brane. **Rehidracija** (engl. *rehydration*) je postupak kojim maper od sačuvanih podataka ponovo pravi objekat bez provere domenskih pravila. EF pri tome pozove konstruktor bez parametara, a zatim vrednosti kolona upiše neposredno u svojstva i polja klase, uključujući svojstva sa privatnim `set` pristupnikom i privatna polja iza kolekcija.

Svaka klasa koju EF mapira zato dobija privatni konstruktor bez parametara. Kada takav konstruktor ne bi postojao, EF bi pozvao javni konstruktor čiji parametri odgovaraju svojstvima klase, pa bi domenska pravila proveravao pri svakom čitanju. Konstruktor je privatan, pa ga kod domenskog i aplikacionog sloja ne vidi. Jedini način da nastane nov objekat ostaje javni konstruktor sa proverom pravila:

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

- Privatni konstruktor bez parametara ne dodeljuje ništa. Svaku vrednost upisuje EF nakon što ga pozove.
- Svojstvo vrednosnog objekta dobija `init` pristupnik, kog u [lekciji o vrednosnom objektu](../1-domenski-sloj/2-vrednosni-objekat.md) nije imalo, jer EF ne može da upiše vrednost u svojstvo bez pristupnika za upis. Nepromenljivost je očuvana, jer `init` dopušta upis samo tokom konstrukcije objekta, a to je jedini trenutak u kom EF upisuje vrednost.

## Tabele modula

Kada se konvencije i konfiguracija saberu, modul za ankete u šemi `surveys` dobija tri tabele. Kolona `SurveyId` u tabeli `Questions` je strani ključ, a istoimena kolona u tabeli `SurveyResponses` nije.

| Tabela | Kolone |
| --- | --- |
| `Surveys` | `Id` uuid, `Title` character varying(200), `Status` character varying(20) |
| `Questions` | `Id` uuid, `Text` text, `IsArchived` boolean, `Options` jsonb, `SurveyId` uuid |
| `SurveyResponses` | `Id` uuid, `SurveyId` uuid, `Answers` jsonb |

Ovaj prikaz opisuje šta model mapiranja kaže, a ne stanje baze. Kako se od modela dolazi do stvarnih tabela i kako one prate izmene modela tokom razvoja obrađuje [lekcija o migracijama](3-migracije.md).
