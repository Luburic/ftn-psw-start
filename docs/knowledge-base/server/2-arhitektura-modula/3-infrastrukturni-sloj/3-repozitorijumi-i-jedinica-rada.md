gde klasa `SurveyReadRepository` implementira interfejs `ISurveyReadRepository` i sastavlja upit nad bazom ([Objektno-relaciono mapiranje](../3-infrastrukturni-sloj/1-orm.md)):

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

U datom kodu treba uočiti sledeće.

- Upitna klasa nema nijednu granu odlučivanja. Njen posao je da izloži upit kontroleru pod nazivom slučaja korišćenja i da sakrije kroz koji interfejs se podaci dobijaju.
- Poziv `AsNoTracking` isključuje praćenje promena nad učitanim podacima. Ni pri grešci u kodu upit ne može da proizvede upis u skladište.
- Metoda `Select` se prevodi u SQL projekciju, pa baza vraća samo kolone koje DTO struktura sadrži. Agregat `Survey` se nikada ne pravi u memoriji, a broj pitanja se računa u bazi.