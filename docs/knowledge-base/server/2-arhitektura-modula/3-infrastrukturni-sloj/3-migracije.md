[Model mapiranja](2-efc-kontekst-i-model.md) određuje koje tabele i kolone treba da postoje. Baza podataka postoji nezavisno od njega, na računaru svakog člana tima i na serveru za integraciju, i sve te baze bi trebalo da imaju iste tabele. Model se pri tome menja tokom celog života projekta, jer svaki nov slučaj korišćenja dodaje svojstvo, klasu ili vezu. Ovde razmatramo kako baza prati model, a da niko ne piše `CREATE TABLE` i `ALTER TABLE` naredbe ručno i ne prenosi ih na svaki računar.

## Migracija

**Migracija** (engl. *migration*) je zapisana izmena šeme baze podataka koja šemu prevodi iz oblika koji odgovara jednoj verziji modela u oblik koji odgovara sledećoj. Uz nju je zapisana i obrnuta izmena, koja je poništava. Migracije se ređaju po redosledu nastanka i primenjuju istim redom, pa se svaka baza od praznog stanja dovodi do trenutnog modela primenom istog niza migracija. Migracije se u tom smislu odnose prema šemi baze kao što se commit-ovi odnose prema izvornom kodu.

Migraciju ne piše programer, već je EF izvodi iz razlike između trenutnog modela i modela zabeleženog pri prethodnoj migraciji. Programer izmeni klase i konfiguraciju, zatraži migraciju i pregleda šta je EF izveo.

## Generisanje migracije

Migracija se generiše komandom alata `dotnet ef`, koja se pokreće iz direktorijuma `backend`:

```
dotnet ef migrations add InitialSurveys --project Modules/Surveys/Surveys.Infrastructure --startup-project Host.Api
```

Komandi se prosleđuju naziv migracije, projekat u kom se nalazi kontekst i projekat kojim se aplikacija pokreće, jer alat iz njegove registracije zavisnosti saznaje kako se kontekst pravi. Za prvu migraciju modula komanda u direktorijumu `Persistence/Migrations` infrastrukturnog projekta pravi tri datoteke. Prva je klasa migracije, čiji naziv čine vreme nastanka i naziv iz komande, poput `20260827103224_InitialSurveys.cs`:

```cs
public partial class InitialSurveys : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.EnsureSchema(name: "surveys");

    migrationBuilder.CreateTable(
      name: "Surveys",
      schema: "surveys",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
        Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_Surveys", x => x.Id);
      });

    // ... CreateTable za Questions i SurveyResponses
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(name: "SurveyResponses", schema: "surveys");
    migrationBuilder.DropTable(name: "Questions", schema: "surveys");
    migrationBuilder.DropTable(name: "Surveys", schema: "surveys");
  }
}
```

U datom kodu treba uočiti sledeće:

- Metoda `Up` opisuje izmenu šeme, a metoda `Down` njeno poništavanje. Obe koriste `MigrationBuilder`, čije metode EF pri primeni prevodi u SQL naredbe za konkretnu bazu. Svaka tabela, kolona i tip kolone iz metode `Up` odgovara pravilu modela mapiranja iz prethodne lekcije.
- Vreme u nazivu datoteke određuje redosled primene. Kada dva člana tima u istom periodu dodaju po jednu migraciju, redosled je onaj u kom su nastale, a ne onaj u kom su stigle u zajedničku granu.
- Klasa je označena kao `partial`, jer je druga datoteka druga polovina iste klase, sa opisom modela u trenutku migracije. Tu datoteku programer ne čita.

Treća datoteka je `SurveyDbContextModelSnapshot.cs`. **Snimak modela** (engl. *snapshot*) je datoteka koja opisuje model nakon poslednje migracije, a EF je prepisuje pri svakoj novoj migraciji. Pri sledećem pozivu komande EF gradi model iz koda, poredi ga sa snimkom i razliku zapisuje kao novu migraciju. Snimak je zato zbir svih migracija i sa njima ide u paru. Migracija se dodaje i uklanja isključivo komandama alata. Ručna izmena bilo koje od tih datoteka razdvojila bi snimak od stvarnog niza migracija, pa bi sledeća migracija opisala pogrešnu razliku.

## Primena migracije

Generisana migracija menja samo kod. Baza se menja tek kada se migracija primeni, a EF pri tome mora da zna koje migracije je ta baza već primila. Zato EF u svakoj bazi koju održava vodi tabelu `__EFMigrationsHistory`, sa jednim redom za svaku primenjenu migraciju. Primena migracija je postupak u kom EF pročita tu tabelu, redom izvrši metodu `Up` svake migracije koja u tabeli nije zabeležena i za svaku upiše red. Baza koja je već u koraku sa kodom ne dobija nijednu naredbu.

Primenu može da zatraži programer, komandom `dotnet ef database update` sa istim parametrima kao pri generisanju. U našem projektu se primena ne traži ručno, već je izvodi aplikacija pri pokretanju. **Inicijalizator modula** je klasa koja se izvršava pri pokretanju aplikacije i dovodi bazu modula u stanje potrebno za rad:

```cs
public sealed class SurveyModuleInitializer : IHostedService
{
  private readonly IServiceProvider _serviceProvider;

  public SurveyModuleInitializer(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();

    await dbContext.Database.MigrateAsync();
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

U datom kodu treba uočiti sledeće:

- Klasa implementira `IHostedService`, interfejs radnog okvira za posao koji se izvršava pri pokretanju aplikacije, pre nego što ona počne da prima zahteve. Registruje se pozivom `AddHostedService` uz ostale klase modula.
- Kontekst je registrovan sa životnim vekom jednog zahteva, a pri pokretanju nema zahteva. Zato inicijalizator sam otvara opseg pozivom `CreateScope` i iz njega traži kontekst, kao što bi to radni okvir uradio za jedan zahtev.
- Poziv `MigrateAsync` izvodi primenu migracija opisanu iznad. Svaki modul ima sopstveni inicijalizator, sopstveni kontekst i sopstvenu tabelu `__EFMigrationsHistory` u svojoj šemi, pa migracije jednog modula ne znaju za migracije drugog.

Posledica ovakve postavke je da član tima nikada ne dovodi bazu u red ručno. Pokretanje aplikacije nad praznom bazom pravi sve šeme i tabele svih modula, a pokretanje nad zastarelom bazom primenjuje samo ono što nedostaje.

## Početni podaci

Prazna baza je ispravna, ali nepogodna za rad. Programer koji razvija pregled objavljenih anketa mora prvo da napravi anketu, doda pitanja i objavi je, pa tek onda vidi rezultat svog rada. **Početni podaci** (engl. *seed data*) su podaci koje aplikacija sama unosi u praznu bazu da bi razvoj mogao da počne od smislenog stanja. Unosi ih isti inicijalizator, nakon primene migracija:

```cs
public async Task StartAsync(CancellationToken cancellationToken)
{
  using var scope = _serviceProvider.CreateScope();
  var dbContext = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();

  await dbContext.Database.MigrateAsync();

  if (_environment.IsDevelopment() && !await dbContext.Surveys.AnyAsync())
  {
    dbContext.Surveys.AddRange(CreateInitialSurveys());
    await dbContext.SaveChangesAsync();
  }
}

private static List<Survey> CreateInitialSurveys()
{
  var satisfaction = new Survey("Zadovoljstvo nastavom");
  satisfaction.AddQuestion("Koliko ste zadovoljni tempom predavanja?");
  satisfaction.AddQuestion("Koliko ste zadovoljni vežbama?");
  satisfaction.Publish();

  var draft = new Survey("Anketa u pripremi");

  return [satisfaction, draft];
}
```

U datom kodu treba uočiti sledeće:

- Podaci se unose samo u razvojnom okruženju, što inicijalizator saznaje kroz `IHostEnvironment` koji, kao i `IServiceProvider`, prima u konstruktoru, i samo kada je tabela prazna. Baza na serveru nikada ne dobija početne podatke, a ponovno pokretanje aplikacije ih ne udvostručuje.
- Ankete nastaju kroz javni konstruktor i metode korena agregata, a ne kroz neposredan upis u tabele. Početni podaci tako prolaze iste provere kao podaci koje unosi korisnik, pa u bazi ne može da se nađe objavljena anketa bez pitanja.

## Izmena modela tokom razvoja

Povežimo pojmove praćenjem jedne izmene modela, od koda do baze svakog člana tima.

1. Programer klasi `Survey` dodaje svojstvo `Description` i pokreće komandu `dotnet ef migrations add AddedSurveyDescription`.
2. EF gradi model iz koda, poredi ga sa snimkom modela, u novu datoteku migracije upisuje metodu `Up` sa pozivom `AddColumn` i metodu `Down` sa pozivom `DropColumn`, a snimak prepisuje tako da sadrži novu kolonu.
3. Programer pregleda generisanu datoteku i proverava da li menja samo ono što je nameravao.
4. Programer pokreće aplikaciju. Inicijalizator modula poziva `MigrateAsync`, EF u tabeli `__EFMigrationsHistory` ne nalazi novu migraciju, izvršava njenu metodu `Up` i upisuje red. Tabela `Surveys` ima novu kolonu. Početni podaci se ne unose, jer tabela nije prazna.
5. Programer predaje izmenu klase, datoteku migracije i snimak modela zajedno, kao jednu celinu.
6. Drugi član tima preuzima izmenu i pokreće aplikaciju. Njegova baza nema red za novu migraciju, pa se ista metoda `Up` izvršava i na njoj.

Protokol [Migracije: lokalni rad](../../../../protocols/migracije-lokalni-rad.md) propisuje postupak za vraćanje pogrešne migracije i za potpuno brisanje lokalne baze, a protokol [Migracije: rešavanje konflikata](../../../../protocols/migracije-resavanje-konflikata.md) postupak za slučaj kada dve grane sadrže različite migracije istog modula.
