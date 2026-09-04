Ovde razmatramo strukturu aplikacionog sloja i izgled metoda komandnih i upitnih klasa u našem projektu.

## Struktura sloja

Aplikacioni sloj modula je organizovan po grupama slučajeva korišćenja. Svaka grupa ima sopstveni direktorijum sa komandnom klasom, upitnom klasom i DTO strukturama koje samo ta grupa koristi. Interfejsi repozitorijuma i DTO strukture koje direktno prate oblik agregata stoje u direktorijumu nazvanom po agregatu. Sledeći prikaz daje strukturu aplikacionog sloja modula za ankete:

```
Application/
  SurveyAuthoring/
    SurveyAuthoringService.cs
    CreateSurveyDto.cs
  SurveyResponding/
    SurveyRespondingService.cs
    SurveyRespondingQueries.cs
  SurveyBrowsing/
    SurveyBrowsingQueries.cs
    SurveySummaryDto.cs
    SurveyResultsDto.cs
  Surveys/
    ISurveyRepository.cs
    ISurveyReadRepository.cs
  SurveyResponses/
    AnswerDto.cs
    SurveyResponseDto.cs
    ISurveyResponseRepository.cs
    ISurveyResponseReadRepository.cs
  IUnitOfWork.cs
```

U datoj strukturi vidimo četiri klase aplikacionog servisa, gde svaka ima jasnu odgovornost:

- `SurveyAuthoringService` je komandna klasa autorske grupe. Njene komande prave anketu, dodaju i arhiviraju pitanja, objavljuju i zatvaraju anketu.
- `SurveyRespondingService` je komandna klasa ispitaničke grupe. Njene komande započinju odgovor, evidentiraju odgovor na pitanje i predaju ceo odgovor.
- `SurveyRespondingQueries` je upitna klasa ispitaničke grupe. Njen upit vraća odgovor ispitanika na anketu, sa dosadašnjim odgovorima na pitanja.
- `SurveyBrowsingQueries` je upitna klasa pregledne grupe. Njeni upiti vraćaju spisak objavljenih anketa i rezultate jedne ankete.

Metode ovih klasa se javljaju u tri oblika. Komanda je jedan oblik, a upiti se javljaju u dva, u zavisnosti od toga da li učitavaju agregat. U nastavku za svaki oblik posmatramo celu implementaciju, a zatim postupak kojim za nov zahtev biramo oblik.

## Slučaj 1: komanda

Telo komande ima tri koraka:
1. Učitavanje agregata koje slučaj korišćenja zahteva putem repozitorijuma,
2. Poziv metoda korena agregata da se izvrši kontrolisana izmena stanja,
3. Čuvanje izmene pozivom jedinice posla, pri čemu se nov agregat prethodno dodaje u repozitorijum.

Sledeći kod prikazuje komandu koja evidentira odgovor ispitanika, gde pravilo o prihvatanju odgovora povezuje dva agregata:

```cs
public sealed class SurveyRespondingService
{
  private readonly ISurveyRepository _surveyRepository;
  private readonly ISurveyResponseRepository _surveyResponseRepository;
  private readonly IUnitOfWork _unitOfWork;

  public SurveyRespondingService(ISurveyRepository surveyRepository,
    ISurveyResponseRepository surveyResponseRepository, IUnitOfWork unitOfWork)
  {
    _surveyRepository = surveyRepository;
    _surveyResponseRepository = surveyResponseRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task SubmitAnswerAsync(Guid responseId, AnswerDto answerDto)
  {
    var surveyResponse = await _surveyResponseRepository.GetByIdAsync(responseId)
      ?? throw new NotFoundException("Odgovor na anketu ne postoji.");
    var survey = await _surveyRepository.GetByIdAsync(surveyResponse.SurveyId)
      ?? throw new NotFoundException("Anketa ne postoji.");

    var answer = new Answer(answerDto.QuestionId, answerDto.Value);
    if (!survey.CanAccept(answer))
      throw new DomainException("Anketa ne prihvata prosleđeni odgovor.");

    surveyResponse.Record(answer);
    await _unitOfWork.SaveChangesAsync();
  }
}
```

U datom kodu treba uočiti sledeće:

- Repozitorijum vraća ceo agregat ili `null` kada agregat ne postoji. Komanda nepostojeći agregat prijavljuje izuzetkom `NotFoundException`, koji middleware prevodi u odgovor sa statusnim kodom 404.
- Ulazna DTO struktura se prevodi u vrednosni objekat `Answer` pre nego što bilo koji agregat vidi podatke. Konstruktor vrednosnog objekta proverava svoja pravila, pa agregati rade sa već ispravnim odgovorom.
- Pravilo da li anketa prihvata odgovor živi u metodi `Survey.CanAccept`, a pravilo kada se odgovor sme evidentirati u metodi `SurveyResponse.Record`. Komanda samo prosleđuje odgovor jednog agregata drugom. Kada `Record` odbije promenu, izuzetak iz agregata prolazi kroz komandu nepromenjen.
- Poziv `SaveChangesAsync` je jedini poziv koji upisuje u skladište. Izmenjen `SurveyResponse` agregat se upisuje u toj transakciji, a `Survey` agregat, koji nije menjan, ne proizvodi nijedan upis.

## Slučaj 2: čist upit

**Čist upit** je upit koji podatke dobija projekcijom iz skladišta pravo u DTO strukturu, kroz repozitorijum za čitanje. Ovo je podrazumevani oblik upita i jedini ispravan izbor kada je rezultat spisak ili prikaz sačuvanih podataka. U aplikacionom sloju upitna klasa prosleđuje poziv repozitorijumu za čitanje:

```cs
public sealed class SurveyBrowsingQueries
{
  private readonly ISurveyReadRepository _surveyReadRepository;

  public SurveyBrowsingQueries(ISurveyReadRepository surveyReadRepository)
  {
    _surveyReadRepository = surveyReadRepository;
  }

  public Task<List<SurveySummaryDto>> GetPublishedAsync()
  {
    return _surveyReadRepository.GetPublishedAsync();
  }
}
```

Implementaciju repozitorijuma za čitanje obrađuje [lekcija o repozitorijumima](../3-infrastrukturni-sloj/4-repozitorijumi.md).

## Slučaj 3: upit koji koristi agregat

Upit koji koristi agregat je upit koji učita agregat kroz njegov repozitorijum, pozove metodu za izvođenje domenski značajne informacije i rezultat upakuje u DTO strukturu. Logika izvođenja ostaje u agregatu, ili u domenskom servisu kada obuhvata više agregata, i ne piše se ponovo kao projekcija u skladištu, jer bi tada postojala na dva mesta. Ovaj oblik je izuzetak, a ne podrazumevani izbor. Sledeći kod proširuje upitnu klasu `SurveyBrowsingQueries` grupe upitom koji koristi domenski servis `SurveyResultsCalculator`:

```cs
public sealed class SurveyBrowsingQueries
{
  private readonly ISurveyReadRepository _surveyReadRepository;
  private readonly ISurveyRepository _surveyRepository;
  private readonly ISurveyResponseRepository _surveyResponseRepository;
  private readonly SurveyResultsCalculator _resultsCalculator;

  // Konstruktor i metoda GetPublishedAsync

  public async Task<SurveyResultsDto> GetResultsAsync(Guid surveyId)
  {
    var survey = await _surveyRepository.GetByIdAsync(surveyId)
      ?? throw new NotFoundException("Anketa ne postoji.");
    var responses = await _surveyResponseRepository.GetBySurveyAsync(surveyId);

    var results = _resultsCalculator.Calculate(survey, responses);
    return MapToDto(results);
  }
}
```

U datom kodu treba uočiti sledeće:

- Upitna klasa za ovaj oblik kroz konstruktor prima i repozitorijume agregata, ali i dalje ne prima jedinicu posla. Domenski servis je obična klasa koju kontejner zavisnosti instancira kao i svaku drugu.
- Pravila obračuna žive u domenskom servisu. Upit učita anketu i sve njene odgovore, prosledi ih servisu i rezultat prevede u DTO strukturu.
- Učitani agregati se odbacuju na kraju obrade zahteva. Ništa što je domenski servis izračunao ne ostaje u skladištu, pa ponovljen upit ponovo obračunava rezultate.

## Kako odlučiti

Za svaki nov zahtev postavljamo tri pitanja, redom:

1. Da li zahtev menja stanje? Ako da, piše se komanda.
2. Da li je rezultat spisak ili prikaz sačuvanih podataka? Ako da, piše se čist upit.
3. Da li je rezultat informacija koju agregat ili domenski servis izvode iz stanja agregata? Ako da, piše se upit koji koristi agregat.

Razmotrimo redom zahteve nad anketama. Evidentiranje odgovora menja stanje, pa je komanda i staje na prvom pitanju. Spisak objavljenih anketa ne menja stanje i jeste prikaz sačuvanih podataka, pa je čist upit i staje na drugom pitanju. Rezultati ankete ne menjaju stanje, nisu prikaz sačuvanih podataka, već se izvode pravilima domenskog servisa iz ankete i odgovora, pa su upit koji koristi agregat.
