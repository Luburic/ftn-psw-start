Aplikacioni sloj je za svaki agregat deklarisao dva interfejsa. Repozitorijum agregata obećava da vraća i prima cele agregate, a repozitorijum za čitanje da vraća DTO strukture spremne za prikaz ([Komande i upiti](../2-aplikacioni-sloj/1-komande-i-upiti.md)). Infrastrukturni sloj ta obećanja ispunjava pomoću kontekstne klase. Ovde razmatramo tri pitanja: kako kontekst vraća agregat u celini, kako saznaje šta je metoda agregata promenila i kako se pri čitanju sve to izbegava.

## Repozitorijum agregata

Repozitorijum agregata implementira interfejs aplikacionog sloja tako što svaku metodu prevodi u rad sa `DbSet` svojstvom korena. Sledeći kod prikazuje repozitorijum ankete u obliku koji sledi iz [lekcije o objektno-relacionom mapiranju](1-orm.md):

```cs
public sealed class SurveyRepository : ISurveyRepository
{
  private readonly SurveyDbContext _dbContext;

  public SurveyRepository(SurveyDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<Survey?> GetByIdAsync(Guid id)
  {
    return _dbContext.Surveys.FirstOrDefaultAsync(survey => survey.Id == id);
  }

  public async Task CreateAsync(Survey survey)
  {
    _dbContext.Surveys.Add(survey);
    await _dbContext.SaveChangesAsync();
  }

  public async Task UpdateAsync(Survey survey)
  {
    await _dbContext.SaveChangesAsync();
  }

  public async Task DeleteAsync(Survey survey)
  {
    _dbContext.Surveys.Remove(survey);
    await _dbContext.SaveChangesAsync();
  }
}
```

U datom kodu treba uočiti sledeće:

- Metoda `FirstOrDefaultAsync` se prevodi u `SELECT` naredbu sa uslovom i vraća rehidriran objekat `Survey` ili `null`, što je tačno ono što interfejs obećava komandi.
- Vraćena anketa ima naslov i status, ali je njena kolekcija pitanja prazna. Red tabele `Surveys` ne sadrži pitanja, jer ona žive u tabeli `Questions`, a upit je pročitao samo jednu tabelu. Komanda koja nad ovakvom anketom pozove `Publish` dobija izuzetak da anketa nema nijedno pitanje, iako ih u bazi ima.
- Pozivi `Add` i `Remove` anketu samo predaju kontekstu. Tek poziv `SaveChangesAsync` sastavlja SQL naredbe i izvršava ih, u metodi `CreateAsync` po jedan `INSERT` za anketu i za svako njeno pitanje, a u metodi `DeleteAsync` odgovarajuće `DELETE` naredbe.
- Metoda `UpdateAsync` ne radi ništa sa prosleđenom anketom, već samo poziva `SaveChangesAsync`. Zašto je to dovoljno objašnjava odeljak o praćenju promena u nastavku.
- Svaka metoda koja upisuje završava se istim pozivom `SaveChangesAsync`. Ovaj oblik, u kom repozitorijum sam odlučuje kada se upisuje, privremen je i menja ga [lekcija o jedinici posla](5-jedinica-posla.md).

## Učitavanje povezanih objekata

Maper podrazumevano čita samo tabelu na koju se upit odnosi. **Učitavanje povezanih objekata** (engl. *eager loading*) je način učitavanja pri kom upit unapred navodi koje povezane redove treba pročitati zajedno sa glavnim, tako da se rezultat sastavi u jednom obraćanju bazi. U EF se to navodi metodom `Include`:

```cs
public Task<Survey?> GetByIdAsync(Guid id)
{
  return _dbContext.Surveys
    .Include(survey => survey.Questions)
    .FirstOrDefaultAsync(survey => survey.Id == id);
}
```

U datom kodu treba uočiti sledeće:

- Poziv `Include` imenuje kolekciju koju treba popuniti. EF upit proširuje spajanjem tabela `Surveys` i `Questions` po stranom ključu `SurveyId`. Jedan `SELECT` tako vraća red ankete ponovljen uz svaki red pitanja, a od tih redova EF sastavlja jednu anketu sa popunjenom kolekcijom.
- Vrednosni objekti ne traže `Include`, jer nemaju sopstvenu tabelu. Opcije su deo reda pitanja i stižu sa njim.

Agregat se po definiciji učitava u celini, pa bi svaki upit nad anketom morao da ponovi isti `Include`, a upit koji ga zaboravi vraća agregat koji ne ume da brani svoja pravila. EF zato dopušta da se učitavanje kolekcije proglasi obaveznim u konfiguraciji, jednom za sve upite:

```cs
public sealed class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
  public void Configure(EntityTypeBuilder<Survey> builder)
  {
    // ... prethodno definisana pravila

    builder.Navigation(survey => survey.Questions).AutoInclude();
  }
}
```

Poziv `AutoInclude` saopštava da svaki upit nad anketom učitava i pitanja, pa se repozitorijum vraća u prvobitni oblik bez poziva `Include` i ne može da vrati nepotpun agregat. U našem projektu se svaka kolekcija unutrašnjih entiteta konfiguriše na ovaj način.

## Praćenje promena

Komanda učita agregat, pozove metodu korena i sačuva izmenu, a nigde ne kaže šta je metoda promenila. Neko ipak mora da sastavi `UPDATE`, `INSERT` i `DELETE` naredbe koje tu promenu prenose u bazu. **Praćenje promena** (engl. *change tracking*) je sposobnost konteksta da pamti svaki objekat koji je vratio iz upita i njegovo stanje u trenutku čitanja, tako da pri čuvanju sam otkrije razliku između tog stanja i trenutnog. Objekat koji kontekst pamti nazivamo **praćenim objektom** (engl. *tracked entity*).

Svaki praćeni objekat ima jedno od četiri stanja. Objekat vraćen iz upita je *nepromenjen*. Objekat čije se svojstvo ili polje razlikuje od pročitanog je *izmenjen*. Objekat koji je predat kontekstu pozivom `Add` ili se pojavio u praćenoj kolekciji je *dodat*. Objekat koji je uklonjen iz praćene kolekcije, a bez ankete ne sme da postoji, je *obrisan*. Posmatrajmo šta kontekst radi kada komanda nad anketom u pripremi ukloni jedno pitanje, doda drugo, objavi anketu i sačuva izmene kroz repozitorijum:

```cs
var survey = await _surveyRepository.GetByIdAsync(surveyId)
  ?? throw new NotFoundException("Anketa ne postoji.");

survey.RemoveQuestion(questionId);
survey.AddQuestion("Da li biste preporučili kurs?");
survey.Publish();

await _surveyRepository.UpdateAsync(survey);
```

U datom kodu treba uočiti sledeće:

- Nakon učitavanja su anketa i sva njena pitanja praćeni objekti u stanju *nepromenjen*. Pozivi metoda korena menjaju objekte u memoriji, a kontekst još ništa ne zna o tome.
- Metoda `UpdateAsync` samo poziva `SaveChangesAsync`, jer kontekst već prati anketu i sva njena pitanja, pa ne treba da mu se kaže šta je izmenjeno. Poziv `SaveChangesAsync` poredi svaki praćeni objekat sa zapamćenim stanjem i za svaki izmenjen, dodat ili obrisan objekat sastavlja po jednu naredbu. Anketa je *izmenjena*, jer je `Publish` promenio status, i dobija `UPDATE`. Uklonjeno pitanje je *obrisano*, jer veza ka anketi ne dopušta pitanje bez ankete, i dobija `DELETE`. Novo pitanje je *dodato* i dobija `INSERT` sa identifikatorom koji mu je dodelio konstruktor. Preostala pitanja su *nepromenjena* i ne dobijaju ništa.
- Sve naredbe se izvršavaju u jednoj transakciji. Ako ijedna ne uspe, baza ostaje kakva je bila pre poziva. Nakon uspešnog čuvanja kontekst prestaje da prati obrisano pitanje, a svaki preostali praćeni objekat je ponovo u stanju *nepromenjen*.
- Nigde ne postoji poziv metode `Update` konteksta, koju smo videli u lekciji o objektno-relacionom mapiranju. Ona služi za objekat koji kontekst ne prati, a kontekst uvek prati agregat učitan kroz repozitorijum.

Praćenje ima cenu. Kontekst za svaki učitani objekat čuva kopiju pročitanog stanja i pri čuvanju sve praćene objekte poredi, pa bi upit koji vrati stotine anketa obavljao posao koji nikome ne treba.

## Repozitorijum za čitanje

Upiti aplikacionog sloja ne menjaju stanje, pa im praćenje ne treba, a ne treba im ni agregat, jer vraćaju DTO strukture. Repozitorijum za čitanje zato podatke iz baze projektuje pravo u DTO strukturu:

```cs
public sealed class SurveyReadRepository : ISurveyReadRepository
{
  private readonly SurveyDbContext _dbContext;

  public SurveyReadRepository(SurveyDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<List<SurveySummaryDto>> GetPublishedAsync()
  {
    return _dbContext.Surveys
      .AsNoTracking()
      .Where(survey => survey.Status == SurveyStatus.Published)
      .Select(survey => new SurveySummaryDto(survey.Id, survey.Title, survey.Questions.Count))
      .ToListAsync();
  }
}
```

U datom kodu treba uočiti sledeće:

- Poziv `Where` se prevodi u `WHERE` uslov, a `Select` u projekciju, pa baza vraća samo kolone koje DTO struktura sadrži. Objekat `Survey` se nikada ne pravi u memoriji, a `Questions.Count` se izvršava u bazi kao brojanje redova, bez učitavanja pitanja.
- Poziv `AsNoTracking` saopštava da rezultat upita ne treba pratiti. Kontekst DTO strukture ionako ne prati, jer nisu deo modela mapiranja, ali poziv stoji u svakom upitu za čitanje, da namera bude vidljiva i da važi i kada upit vrati domenski objekat. Upit tako ni pri grešci u kodu ne može da proizvede upis u bazu.

Repozitorijum agregata i repozitorijum za čitanje dele isti kontekst i iste tabele, a razlikuju se u tome šta od konteksta traže. Prvi traži praćen agregat koji će komanda menjati, a drugi nepraćene podatke koje će upit vratiti. Ostaju dva pitanja. Svaka metoda repozitorijuma koja upisuje ponavlja isti poziv `SaveChangesAsync`, a nije jasno ko taj poziv izvodi kada komanda menja agregate iz više repozitorijuma. Oba obrađuje [lekcija o jedinici posla](5-jedinica-posla.md).
