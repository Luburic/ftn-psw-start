**Aplikacioni sloj koordiniše ispunjenje slučajeva korišćenja.**

Slučaj korišćenja opisuje cilj koji korisnik ostvaruje kroz softver. U posmatranom primeru ([Čista arhitektura](../čista-arhitektura.md)) ispitanik evidentira odgovor na jedno pitanje. Za ispunjenje tog zahteva potrebno je učitati dva agregata, primeniti njihovo ponašanje i sačuvati promenjeno stanje. Aplikacioni sloj određuje redosled ovih koraka. Domenski sloj određuje da li su pojedinačne operacije dozvoljene.

### 1. Aplikacioni servisi

**Aplikacioni sloj definiše aplikacione servise koji opisuju kako se koordinišu domenski objekti i tehničke sposobnosti da se ispuni korisnički zahtev.**

U primeru aplikacioni servis koordiniše sledeće korake:

1. prevodi ulaznu DTO strukturu u domenski objekat,
2. učitava `SurveyResponse`,
3. učitava `Survey`,
4. pita `Survey` da li prihvata odgovor,
5. traži od `SurveyResponse` da evidentira odgovor,
6. čuva promenjeni agregat i
7. formira rezultat.

Aplikacioni servis ne pristupa kolekciji pitanja da bi sam tražio pitanje. Ne menja direktno kolekciju odgovora. Ne zna kako repozitorijumi dolaze do podataka. Njegova odgovornost je koordinacija.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš kod aplikacionog servisa</b></summary>

Za evidentiranje odgovora definišemo `SurveyResponseService`:

```cs
public sealed class SurveyResponseService
{
  private readonly ISurveyRepository _surveyRepository;
  private readonly ISurveyResponseRepository _surveyResponseRepository;

  public SurveyResponseService(ISurveyRepository surveyRepository,
    ISurveyResponseRepository surveyResponseRepository)
  {
    _surveyRepository = surveyRepository;
    _surveyResponseRepository = surveyResponseRepository;
  }

  public SubmitAnswerResultDto SubmitAnswer(
    long responseId, AnswerDto answerDto)
  {
    var answer = MapToDomain(answerDto);

    var surveyResponse = _surveyResponseRepository.Get(responseId)
      ?? throw new NotFoundException("Odgovor na anketu ne postoji.");

    var survey = _surveyRepository.Get(surveyResponse.SurveyId)
      ?? throw new NotFoundException("Anketa ne postoji.");

    if (!survey.CanAccept(answer))
      throw new InvalidOperationException("Anketa ne prihvata prosleđeni odgovor.");

    surveyResponse.Record(answer);

    _surveyResponseRepository.Update(surveyResponse);

    return new SubmitAnswerResultDto(surveyResponse.Id, surveyResponse.Status);
  }

  private static Answer MapToDomain(AnswerDto answerDto) =>
    new(answerDto.QuestionId, answerDto.Value);
}
```

</details>
<hr></hr>

### 2. DTO strukture

**Aplikacioni sloj definiše ulazne i izlazne DTO strukture koje servisi razmenjuju sa spoljašnjim slojevima.**

DTO strukture definišu tipove povratne vrednosti i parametara metoda servisa, odnosno podatke koji prelaze granicu aplikacionog sloja. `AnswerDto` može da nastane iz HTTP zahteva ili zahteva drugog protokola. Aplikacioni servis ga prevodi u `Answer` vrednosni objekat. Konstruktor domenskog objekta tada proverava pravila koja važe za svaki odgovor, bez obzira na izvor podataka. Izlazni DTO sadrži samo podatke potrebne klijentu nakon izvršene operacije. Kontroler ne mora da dobije celokupan `SurveyResponse` agregat i da sam bira koja njegova svojstva treba izložiti.

### 3. Interfejsi tehničkih sposobnosti

**Aplikacioni sloj definiše interfejse za tehničke sposobnosti koje su potrebne slučajevima korišćenja.**

Posmatrani slučaj korišćenja od tehničkih sposobnosti koristi samo učitavanje i čuvanje agregata, što se realizuje kroz repozitorijumske klase. Aplikacioni sloj definiše interfejse tih repozitorijuma čime naglašava kakva mogućnost skladištenja mu je potrebna. Drugi slučajevi korišćenja mogu zahtevati dodatne tehničke sposobnosti. Na primer, nakon konačne predaje odgovora, sistem može da pošalje email obaveštenje korisniku. Aplikacioni servis ovo realizuje uz pomoć tehničke sposobnosti aplikacije, ali sa njom interaguje kroz interfejs poput sledećeg:

```cs
public interface ISurveyCompletionNotifier
{
  void Notify(Survey survey, long userId);
}
```

Aplikacioni sloj ovim interfejsima opisuje šta mu je potrebno. Ne određuje adresu udaljenog API-ja, format HTTP zahteva, biblioteku za generisanje datoteke ili način pristupa operativnom sistemu. Interfejs se oblikuje prema potrebama slučaja korišćenja. Metoda `Notify` govori jezikom aplikacije. Aplikacioni servis ne poziva opštu metodu poput `PostJson` i ne formira zahtev specifičan za udaljeni servis.

### 4. Komande i upiti

**Komanda** (engl. *command*) je metoda aplikacionog sloja koja menja stanje sistema i ne vraća podatke, ili vraća najmanju potvrdu izmene. **Upit** (engl. *query*) je metoda aplikacionog sloja koja vraća podatke i ne menja ništa. Svaka javna metoda aplikacionog sloja je ili komanda ili upit, nikada oboje.

Metoda aplikacionog sloja koja u istom pozivu menja stanje sistema i vraća složene podatke brzo postaje teška za razumevanje. Pozivalac takve metode ne zna da li sme da je pozove ponovo bez posledica, a sama metoda vremenom raste jer služi i izmeni i prikazu, čije se potrebe razlikuju.

Podela ima strukturnu posledicu. Komande žive u servisnim klasama poput `SurveyResponseService`, čija metoda `SubmitAnswer` menja stanje i vraća najmanju potvrdu izmene. Upiti žive u zasebnim upitnim klasama, koje rade sa sopstvenim interfejsom repozitorijuma: repozitorijum za pisanje vraća agregate, a repozitorijum za čitanje vraća DTO strukture spremne za prikaz.

```cs
public sealed class SurveyQueries
{
  private readonly ISurveyReadRepository _surveyReadRepository;

  public SurveyQueries(ISurveyReadRepository surveyReadRepository)
  {
    _surveyReadRepository = surveyReadRepository;
  }

  public List<SurveySummaryDto> GetPublished() =>
    _surveyReadRepository.GetPublished();
}
```

Upitna klasa ne učitava agregate da bi ih prikazala. Repozitorijum za čitanje projektuje podatke pravo u `SurveySummaryDto`, pa upit ni greškom ne može da promeni stanje. Detaljna pravila za oblikovanje komandi i upita opisuje dokument [Komande i upiti](../komande-i-upiti.md).
