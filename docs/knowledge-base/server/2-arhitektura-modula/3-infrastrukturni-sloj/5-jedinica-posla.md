U [lekciji o komandama i upitima](../2-aplikacioni-sloj/1-komande-i-upiti.md) smo videli komandu `CloseSurveyAsync`, koja zatvara anketu i označava svaki započet odgovor kao istekao, i zaključili da jedino komanda zna gde se celina izmena završava. Zato je jedinica posla deklarisana kao interfejs sa jednom metodom `SaveChangesAsync`, koju komanda poziva jednom, na kraju. Ovde razmatramo šta bi se desilo kada bi svaki repozitorijum upisivao sam, kako jedan poziv obuhvata agregate učitane kroz dva repozitorijuma i šta se u repozitorijumima zbog toga menja.

## Repozitorijum koji čuva

Posmatrajmo prvo šta se dešava kada repozitorijumi zadrže oblik iz [lekcije o repozitorijumima](4-repozitorijumi.md), u kom svaka metoda koja upisuje sama poziva `SaveChangesAsync`. Komanda tada od svakog repozitorijuma traži da sačuva svoj agregat:

```cs
public async Task CloseSurveyAsync(Guid surveyId)
{
  var survey = await _surveyRepository.GetByIdAsync(surveyId)
    ?? throw new NotFoundException("Anketa ne postoji.");
  var startedResponses = await _surveyResponseRepository.GetStartedBySurveyAsync(surveyId);

  survey.Close();
  await _surveyRepository.UpdateAsync(survey);

  foreach (var response in startedResponses)
  {
    response.Expire();
    await _surveyResponseRepository.UpdateAsync(response);
  }
}
```

U datom kodu treba uočiti sledeće:

- Svaki poziv `UpdateAsync` je poziv metode `SaveChangesAsync` konteksta, a svaki takav poziv otvara i potvrđuje sopstvenu transakciju. Komanda sa pet započetih odgovora otvara i potvrđuje šest transakcija.
- Kada `Expire` nad trećim odgovorom baci izuzetak, anketa je zatvorena, a dva odgovora istekla, i te tri transakcije su već potvrđene. Komanda ne može da ih poništi, jer se potvrđena transakcija ne može vratiti. Pozivalac dobija grešku, a stanje je ipak promenjeno, što krši drugu garanciju principa razdvajanja komandi i upita.
- Isti problem postoji i bez izuzetka. Između dve transakcije baza sadrži zatvorenu anketu sa započetim odgovorima, pa upit koji stigne u tom trenutku vidi stanje koje domenska pravila ne dopuštaju.

Repozitorijum je pogrešno mesto za odluku o čuvanju, jer vidi jedan agregat, a celina obuhvata više njih.

## Kontekst kao jedinica posla

Sve što je jedinici posla potrebno kontekst već radi. Oba repozitorijuma su tokom obrade jednog zahteva dobila istu instancu konteksta, jer je kontekst registrovan sa životnim vekom zahteva. Kontekst prati svaki agregat koji je bilo koji od njih učitao, kako smo videli u lekciji o repozitorijumima, a njegova metoda `SaveChangesAsync` sve praćene izmene upisuje u jednoj transakciji. Kontekst je zato jedinica posla, a to saopštavamo tako što kontekst implementira interfejs aplikacionog sloja:

```cs
public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class SurveyDbContext : DbContext, IUnitOfWork
{
  // ... prethodno definisani konstruktor, svojstva i konfiguracija
}
```

Klasa ne dobija nijednu novu liniju, jer `DbContext` već ima metodu `SaveChangesAsync` sa potpisom koji interfejs traži. Ostaje da kontejner zavisnosti za interfejs `IUnitOfWork` vrati istu instancu koju dobijaju repozitorijumi:

```cs
services.AddDbContext<SurveyDbContext>(options => ...);
services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SurveyDbContext>());
```

U datom kodu treba uočiti sledeće:

- Prva registracija je ona iz [lekcije o kontekstu](2-efc-kontekst-i-model.md) i daje kontekstu životni vek jednog zahteva.
- Druga registracija ne pravi nov objekat, već za `IUnitOfWork` traži `SurveyDbContext` iz istog opsega. Komanda, `SurveyRepository` i `SurveyResponseRepository` tako u jednom zahtevu drže tri reference na jedan kontekst, a u sledećem zahtevu na drugi.
- Komanda kroz `IUnitOfWork` vidi samo `SaveChangesAsync`. Ne vidi `DbSet` svojstva ni upite, pa ne može da zaobiđe repozitorijum, iako radi sa istim objektom.

## Repozitorijum bez čuvanja

Kada čuvanje pripada komandi, repozitorijum ga gubi. Od tri metode koje su upisivale ostaje jedna. Metoda `CreateAsync` iz lekcije o repozitorijumima postaje `Add`, a `UpdateAsync` i `DeleteAsync` nestaju:

```cs
public sealed class SurveyRepository : ISurveyRepository
{
  // ... prethodno definisani konstruktor i GetByIdAsync

  public void Add(Survey survey)
  {
    _dbContext.Surveys.Add(survey);
  }
}
```

U datom kodu treba uočiti sledeće:

- Metoda više nije asinhrona, jer se ne obraća bazi. Poziv `Add` samo beleži anketu kao praćen objekat u stanju *dodat*, a `INSERT` nastaje tek pri čuvanju.
- Metoda `UpdateAsync` je i ranije samo pozivala `SaveChangesAsync`, pa bez tog poziva nema šta da radi. Izmena je posledica poziva metode korena nad praćenim agregatom, a brisanje unutrašnjeg entiteta posledica njegovog uklanjanja iz kolekcije. Brisanje celog agregata u našem projektu još nije potrebno, pa ni `DeleteAsync` nema zamenu.
- Tri poziva `SaveChangesAsync` u tri metode repozitorijuma postala su jedan poziv u komandi. Jedinica posla tako rešava i ponavljanje iz lekcije o repozitorijumima, a ne samo pitanje transakcije.
- Ni repozitorijum ni upitna klasa ne mogu da proizvedu upis. Repozitorijum nema poziv `SaveChangesAsync`, a upitna klasa ne prima jedinicu posla. U našem projektu tu drugu zabranu proverava automatski test koji odbija svaku upitnu klasu koja zavisi od `IUnitOfWork`.

## Put jedne komande

Povežimo pojmove praćenjem komande `CloseSurveyAsync` u obliku iz lekcije o komandama i upitima, sa jedinicom posla.

1. Kontroler prima `POST /api/surveys/{id}/close` i poziva metodu `CloseSurveyAsync` klase `SurveyAuthoringService`. Kontejner zavisnosti je za ovaj zahtev napravio jedan `SurveyDbContext` i predao ga repozitorijumu ankete, repozitorijumu odgovora i, kao `IUnitOfWork`, komandi.
2. Repozitorijum ankete izvršava `SELECT` nad tabelama `Surveys` i `Questions`, jer je učitavanje pitanja konfigurisano kao obavezno. Kontekst rehidrira anketu i pitanja i prati ih u stanju *nepromenjen*.
3. Repozitorijum odgovora izvršava `SELECT` nad tabelom `SurveyResponses` sa uslovom da odgovor pripada anketi i da je započet. Kontekst prati i te agregate.
4. Metoda `Close` proverava da li je anketa objavljena i menja status. Metoda `Expire` nad svakim odgovorom proverava da li je odgovor započet i menja status. Kontekst još ništa ne zna o tim izmenama.
5. Komanda poziva `SaveChangesAsync`. Kontekst poredi praćene objekte sa zapamćenim stanjem, sastavlja jedan `UPDATE` za anketu i po jedan za svaki odgovor i sve izvršava u jednoj transakciji.
6. Kada transakcija uspe, baza sadrži zatvorenu anketu bez započetih odgovora, a kontroler vraća uspešan HTTP odgovor. Kada `Expire` baci `DomainException`, do koraka 5 se ne stiže, baza je nepromenjena, a middleware vraća HTTP odgovor sa statusnim kodom 400.
7. Zahtev je obrađen i kontejner uništava kontekst. Sledeći zahtev dobija nov kontekst koji ne prati nijedan objekat.
