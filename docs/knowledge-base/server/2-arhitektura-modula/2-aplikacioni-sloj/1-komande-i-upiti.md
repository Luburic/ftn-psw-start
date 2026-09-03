Pri projektovanju aplikacionih servisa u složenom softveru, često u praksi pratimo princip razdvajanja komandi od upita, koji diktira šta jedna metoda sme da radi. Praćenje ovog principa čini kod lakšim za održavanje.

## Princip razdvajanja

**Razdvajanje komandi i upita** (engl. *command-query separation*) je pravilo da svaka javna metoda aplikacionog sloja kao konačni cilj ima ili promenu stanja sistema (npr. podataka u bazi) ili dobavljanje podataka. **Komanda** (engl. *command*) je metoda koja menja stanje sistema i ne vraća podatke ili vraća najmanju potvrdu izmene, poput identifikatora novog agregata. **Upit** (engl. *query*) je metoda koja vraća podatke i ne menja stanje sistema.

Pravilo garantuje tri stvari:
1. Metoda tipa *upit* se sme pozvati bilo kada, bilo koliko puta i sa bilo kog mesta, a stanje sistema se ne menja. Zato osvežavanje stranice, prikaz spiska ili generisanje izveštaja mogu da čitaju podatke bez neželjenih posledica.
2. Metoda tipa *komanda* ima samo dva moguća ishoda: ili je promenila stanje, ili je bacila izuzetak. Pozivalac mora da zna da li je promena upisana.
3. Kod za čitanje (query) i kod za pisanje (command) su razdvojeni, pa svaki može koristi model podataka koji mu odgovara: pisanje radi nad agregatom, a čitanje nad podacima spremnim za prikaz.

Posmatrajmo metodu koju bismo bez ovog pravila prirodno napisali. Ispitanik otvara anketu i treba mu njegov odgovor na tu anketu, sa dosadašnjim odgovorima na pitanja. Ako odgovor još ne postoji, jer ispitanik prvi put otvara anketu, metoda ga pravi:

```cs
public async Task<SurveyResponseDto> GetResponseAsync(long surveyId, long userId)
{
  var surveyResponse = await _surveyResponseRepository.GetBySurveyAndUserAsync(surveyId, userId);
  if (surveyResponse is null)
  {
    surveyResponse = new SurveyResponse(surveyId, userId);
    _surveyResponseRepository.Add(surveyResponse);
  }
  return MapToDto(surveyResponse);
}
```

U datom kodu treba uočiti sledeće:

- Metoda se zove i izgleda kao upit, a u jednoj grani pravi nov agregat. Svako ko je pozove da bi nešto pogledao, može da promeni stanje sistema.
- Autor koji želi da pogleda anketu koju je upravo napravio iz perspektive ispitanika, generiše odgovor u svoje ime bez da je nameravao da odgovori. Stranica sa spiskom anketa, koja za svaku anketu prikazuje napredak korisnika, pravi po jedan odgovor za svaku anketu koju je korisnik samo video. Izveštaj o broju započetih anketa broji i te odgovore.
- Test koji proverava prikaz odgovora mora prvo da pripremi odgovor u skladištu ili će ga metoda napraviti sama i test će proći iz pogrešnog razloga.
- Klasa koja sadrži ovu metodu mora da prima repozitorijum koji ume da upisuje, pa iz njenog konstruktora niko ne može da zaključi da klasa samo čita.

Pravilo razdvajanja nalaže da se metoda podeli na dve:

```cs
public Task<long> StartResponseAsync(long surveyId, long userId);

public Task<SurveyResponseDto?> GetResponseAsync(long surveyId, long userId);
```

Komanda `StartResponseAsync` pravi odgovor i vraća samo njegov identifikator. Upit `GetResponseAsync` vraća odgovor ako postoji, a `null` ako ne postoji, i ne pravi ništa. Odluka da se odgovor započne sada pripada klijentu, koji komandu poziva kada ispitanik izabere da započne anketu, a upit svaki put kada nešto prikazuje.

Isto pravilo važi i u drugom smeru. Komanda koja evidentira odgovor na pitanje i pozivaocu vraća prikaz celog odgovora, sa tekstom svakog pitanja, ima dva posla. Kada evidentiranje uspe, a sastavljanje prikaza baci izuzetak, pozivalac dobija grešku iako je stanje promenjeno i ne može da zna da li je odgovor sačuvan. Kada prikaz zatraži još jedan podatak, menja se metoda koja evidentira odgovore. Komanda zato vraća identifikator, a prikaz se dobija upitom.

## Oblik komandne i upitne klase

Pravilo razdvajanja ne govori gde metode žive. Komande i upiti mogu da stoje u istoj klasi, u dve odvojene klase, ili svaka metoda u sopstvenoj klasi. U našem projektu se držimo pristupa da aplikacioni servis sadrži samo komande za grupu slučajeva korišćenja (ove klase imaju sufiks `Service`) ili samo upite (sufiks `Queries`). Sledeći kod prikazuje kostur dve klase ispitaničke grupe, gde su tela metoda izostavljena:

```cs
public sealed class SurveyRespondingService
{
  private readonly ISurveyRepository _surveyRepository;
  private readonly ISurveyResponseRepository _surveyResponseRepository;
  private readonly IUnitOfWork _unitOfWork;

  public SurveyRespondingService(ISurveyRepository surveyRepository,
    ISurveyResponseRepository surveyResponseRepository, IUnitOfWork unitOfWork) { ... }

  public Task<long> StartResponseAsync(long surveyId, long userId) { ... }

  public Task SubmitAnswerAsync(long responseId, AnswerDto answerDto) { ... }

  public Task ConcludeResponseAsync(long responseId) { ... }
}

public sealed class SurveyRespondingQueries
{
  private readonly ISurveyResponseReadRepository _surveyResponseReadRepository;

  public SurveyRespondingQueries(ISurveyResponseReadRepository surveyResponseReadRepository) { ... }

  public Task<SurveyResponseDto?> GetResponseAsync(long surveyId, long userId) { ... }
}
```

U datom kodu treba uočiti sledeće:

- Komandna klasa kroz konstruktor prima:
  - **Repozitorijume agregata**, koji vraćaju cele agregate, jer spoljni kod može samo kroz koren agregata da menja stanje agregata.
  - **Jedinicu posla** (engl. *unit of work*), što je interfejs čija metoda `SaveChangesAsync` upisuje u skladište sve izmene agregata učitanih tokom obrade jednog zahteva, u jednoj transakciji. Komanda je poziva tačno jednom, na kraju. Problem koji jedinica posla rešava ćemo videti u infrastrukturnom sloju.
- Upitna klasa kroz konstruktor prima:
   - **Repozitorijum za čitanje** (engl. *read repository*), što je interfejs čije metode vraćaju DTO strukture spremne za prikaz, bez učitavanja agregata.
   - Ređe repozitorijume agregata, kada upit učitava agregat da bi pozvao metodu koja izvodi domenski značajnu informaciju iz njegovog stanja. Kada se koji oblik upita piše obrađuje [lekcija o aplikacionom servisu](2-aplikacioni-servis.md).
- Upitna klasa nikada ne prima jedinicu posla, što je sprečava da čuva izmene stanja.

Sva četiri interfejsa su deklarisana u aplikacionom sloju, a implementirana u infrastrukturnom, kao i svaki drugi interfejs tehničke sposobnosti ([Infrastrukturni sloj](../3-infrastrukturni-sloj/README.md)).

## Put dva zahteva

Povežimo pojmove praćenjem jedne komande i jednog upita, u redosledu u kom ih klijent poziva.

1. Ispitanik otvara anketu i klijent šalje `GET /api/surveys/5/response`.
2. Kontroler poziva `GetResponseAsync` na klasi `SurveyRespondingQueries`.
3. Upit prosleđuje poziv repozitorijumu za čitanje, koji vraća `null`, jer odgovor ne postoji. Nijedan agregat nije učitan i ništa nije sačuvano. Klijent prikazuje dugme za započinjanje ankete.
4. Ispitanik započinje anketu i klijent šalje `POST /api/surveys/5/response`.
5. Kontroler poziva `StartResponseAsync` na klasi `SurveyRespondingService`.
6. Komanda kroz repozitorijum učitava `Survey` agregat i pita ga da li se na anketu trenutno može odgovarati. Zatim pravi `SurveyResponse` agregat i dodaje ga u repozitorijum.
7. Komanda jednom poziva `SaveChangesAsync` na jedinici posla i vraća identifikator novog odgovora.
8. Klijent ponovo šalje `GET /api/surveys/5/response` i ovaj put upit vraća započet odgovor.
