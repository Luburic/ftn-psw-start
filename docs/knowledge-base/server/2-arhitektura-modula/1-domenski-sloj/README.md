**Domenski sloj modeluje deo domena problema koji je relevantan za rad softvera.**

Posmatrajmo softver u kojem ispitanik popunjava anketu i predaje svoje odgovore ([Čista arhitektura](../čista-arhitektura.md)). Domen istraživanja javnog mnjenja obuhvata mnogo više pojmova nego što je potrebno jednoj aplikaciji. Posmatrani softver modeluje ankete, pitanja, ponuđene odgovore, odgovore ispitanika i pravila koja određuju kada je dozvoljeno evidentirati odgovor. Ovi pojmovi i pravila čine domenski model relevantan za rad softvera.

Domenski sloj ne opisuje HTTP zahteve, tabele u bazi ili način serijalizacije podataka. Njegove klase koriste jezik domena i implementiraju domenske koncepte i pravila koja važe bez obzira na tehničko okruženje u kojem se softver izvršava.

### 1. Entiteti i vrednosni objekti grupisani u agregate

**Domenski sloj sadrži entitete i vrednosne objekte grupisane u agregate.**

Ovi obrasci su detaljno opisani u lekcijama o [taktičkim DDD obrascima](../ddd/1-takticki-obrasci.md). U posmatranom primeru `Survey` i `SurveyResponse` predstavljaju korene dva agregata. `Survey` agregat sadrži korenski entitet `Survey`, unutrašnje entitete `Question` i vrednosne objekte `Option`. `SurveyResponse` agregat sadrži korenski entitet `SurveyResponse` i vrednosne objekte `Answer`.

Svaki agregat štiti pravila koja zahtevaju uvid u njegovo stanje kao celinu. `Survey` proverava da li se na anketu trenutno može odgovarati i da li postoji odgovarajuće aktivno pitanje. `SurveyResponse` kontroliše promenu sopstvene kolekcije odgovora.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš kod navedenih agregata</b></summary>

Metoda `CanAccept` izvodi domenski značajnu informaciju iz stanja `Survey` agregata:

```cs
public sealed class Survey
{
  public long Id { get; }
  public SurveyStatus Status { get; private set; }

  private readonly List<Question> _questions = new();
  public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

  public bool CanAccept(Answer answer)
  {
    if (!IsPublished())
      return false;
    if (!HasActiveQuestion(answer.QuestionId))
      return false;
    return true;
  }

  private bool IsPublished() => Status == SurveyStatus.Published;

  private bool HasActiveQuestion(long questionId) =>
    _questions.Any(question => question.Id == questionId && !question.IsArchived);
}
```

Koren agregata koristi sopstveni status, spisak pitanja i ponašanje odgovarajućeg pitanja. Spoljašnji kod dobija odgovor na domensko pitanje pozivom jedne metode. Ne mora samostalno da proverava status ankete, pronalazi pitanje i analizira njegove opcije.

`SurveyResponse` agregat nudi metodu za kontrolisanu promenu stanja:

```cs
public sealed class SurveyResponse
{
  public long Id { get; }
  public long UserId { get; }
  public long SurveyId { get; }
  public ResponseStatus Status { get; private set; }

  private readonly List<Answer> _answers = new();
  public IReadOnlyList<Answer> Answers => _answers.AsReadOnly();

  public void Record(Answer answer)
  {
    if (!IsIncomplete())
      throw new InvalidOperationException("Odgovori se mogu menjati samo pre predaje ankete.");

    _answers.RemoveAll(
      existing => existing.QuestionId == answer.QuestionId);

    _answers.Add(answer);
  }

  public bool IsIncomplete() =>
    Status == ResponseStatus.Incomplete;
}

public enum ResponseStatus
{
  Incomplete,
  Submitted
}
```

Metoda `Record` štiti životni ciklus odgovora. Ispitanik može da doda ili promeni odgovor dok je celokupan odgovor na anketu nepotpun. Nakon predaje ankete, ista operacija više nije dozvoljena. Prekršeno domensko pravilo prijavljujemo kroz izuzetak koji spoljašnji slojevi prepoznaju i prevode u odgovarajući odgovor.

`Answer` je vrednosni objekat koji garantuje ispravnost pojedinačnog odgovora pri kreiranju:

```cs
public sealed record Answer
{
  public long QuestionId { get; }
  public string Value { get; }

  public Answer(long questionId, string value)
  {
    if (questionId <= 0)
      throw new ArgumentException("Identifikator pitanja mora biti ispravan.");

    if (string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("Odgovor ne sme biti prazan.");

    QuestionId = questionId;
    Value = value;
  }
}
```

</details>
<hr></hr>

### 2. Domenski servisi

**Domenski sloj sadrži domenske servise.**

[Domenski servis](../ddd/5-domenski-servis.md) je stručnjačka funkcionalna klasa koja se uvodi kada domensko pravilo ne pripada prirodno jednom entitetu ili vrednosnom objektu, već zahteva uvid u više agregata. Aplikacioni servis je odgovoran da agregate učita, prosledi domenskom servisu i sačuva njihove izmene, a domenski servis radi isključivo sa domenskim objektima.
