Videli smo **vrednosni objekat** koji modeluje domenski značajne vrednosti i **entitet**, koji modeluje domenski koncept sa životnim ciklusom. U oba slučaja smo videli kako domenska pravila, odnosno invarijante, implementiramo da garantujemo validnost ovih objekata pri konstrukciji, kao i tokom čitavog životnog ciklusa u slučaju entiteta.

Međutim, domenska pravila su retko ograničena samo na jedan objekat i često povezuju više njih. Ovo nam stvara potrebu za obrascem koji grupu povezanih objekata tretira kao jednu celinu i taj obrazac se zove **agregat** (engl. *aggregate*).


## Šta su karakteristike "Agregat" obrasca?

### 1. Granica transakcione konzistentnosti

Agregat modeluje grupu domenskih koncepata čije se zajedničko stanje mora održavati validnim unutar jedne **granice transakcione konzistentnosti** (engl. *consistency boundary*). Tu grupu na okupu drži invarijanta, ali dok je kod entiteta invarijanta ograničavala stanje jednog objekta, ovde jedno pravilo obuhvata više njih odjednom. Da bi agregat mogao da se celokupno drži konzistentnim, mora biti u celosti učitan u memoriju.

Na primer, u domenu istraživanja javnog mnjenja, anketa i njena pitanja dele invarijantu: *objavljena anketa mora imati bar jedno aktivno pitanje*. Ako bi se status ankete i spisak pitanja menjali nezavisno, sistem bi završio u stanju koje krši pravilo, a da nijedna od dve izmene pojedinačno nije pogrešila. Anketa i pitanja zato pripadaju istoj granici konzistentnosti. Sa druge strane, odgovori ispitanika pristižu bez potrebe da se menja stanje ankete, te ne dele nijednu invarijantu sa anketom. Zbog toga odgovori ostaju izvan granice agregata ankete.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš granice agregata u domenu anketa</b></summary>

Sledeći kod prikazuje agregat ankete, koji se sastoji od jednog glavnog entiteta `Survey` i više entiteta tipa `Question`:

```cs
public sealed class Survey
{
  public Guid Id { get; }
  public string Title { get; private set; }
  public SurveyStatus Status { get; private set; }

  private readonly List<Question> _questions = new();
  public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

  public Survey(string title)
  {
    if (string.IsNullOrWhiteSpace(title))
      throw new DomainException("Naslov ankete je obavezan.");

    Title = title;
    Status = SurveyStatus.Draft;
  }

  public void AddQuestion(string text)
  {
    if (Status != SurveyStatus.Draft)
      throw new DomainException("Pitanja se mogu dodavati samo dok je anketa u pripremi.");

    _questions.Add(new Question(text));
  }

  public void RemoveQuestion(Guid questionId)
  {
    if (Status != SurveyStatus.Draft)
      throw new DomainException("Pitanja se mogu uklanjati samo dok je anketa u pripremi.");

    _questions.RemoveAll(q => q.Id == questionId);
  }

  public void Publish()
  {
    if (Status != SurveyStatus.Draft)
      throw new DomainException("Objaviti je moguće samo anketu u pripremi.");
    if (ActiveQuestionCount() == 0)
      throw new DomainException("Anketa mora imati bar jedno aktivno pitanje.");

    Status = SurveyStatus.Published;
  }

  public void ArchiveQuestion(Guid questionId)
  {
    var question = _questions.SingleOrDefault(q => q.Id == questionId)
      ?? throw new DomainException("Pitanje ne postoji u anketi.");

    if (Status == SurveyStatus.Published && !question.IsArchived && ActiveQuestionCount() == 1)
      throw new DomainException("Objavljena anketa mora zadržati bar jedno aktivno pitanje.");

    question.Archive();
  }

  public void Close()
  {
    if (Status != SurveyStatus.Published)
      throw new DomainException("Zatvoriti je moguće samo objavljenu anketu.");

    Status = SurveyStatus.Closed;
  }

  private int ActiveQuestionCount() => _questions.Count(q => !q.IsArchived);
}

public enum SurveyStatus
{
  Draft,
  Published,
  Closed
}

public sealed class Question
{
  public Guid Id { get; }
  public string Text { get; private set; }
  public bool IsArchived { get; private set; }

  private readonly List<Option> _options;
  public IReadOnlyList<Option> Options => _options.AsReadOnly();

  // Konstruktor i ostale metode entiteta
}
```

</details>
<hr></hr>

### 2. Graf objekata sa korenskim entitetom

Agregat se realizuje kao graf objekata na čijem vrhu se nalazi tačno jedan entitet koji zovemo **koren agregata** (engl. *aggregate root*), dok unutrašnju strukturu grafa čine drugi entiteti i vrednosni objekti.

Na prethodnom primeru ankete, anketa je korenski entitet koji ujedno predstavlja i ceo agregat. Pitanje je unutrašnji entitet, gde znamo da postoji bar dve faze njegovog životnog ciklusa - aktivna i arhivirana. Ponuđena opcija odgovora je vrednosni objekat, jer su dve opcije sa istim tekstom međusobno zamenljive i nemaju sopstveni životni ciklus.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš strukturu Survey agregata</b></summary>

U implementaciji, agregat je skup klasa u kojem koren drži reference na unutrašnje objekte:

```cs
public sealed class Survey
{
  public Guid Id { get; }
  public string Title { get; private set; }
  public SurveyStatus Status { get; private set; }

  private readonly List<Question> _questions = new();
  public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

  // Konstruktor i ostale metode
}

public sealed class Question
{
  public Guid Id { get; }
  public string Text { get; private set; }
  public bool IsArchived { get; private set; }

  private readonly List<Option> _options = new();
  public IReadOnlyList<Option> Options => _options.AsReadOnly();

  // Konstruktor i ostale metode
}

public sealed record Option
{
  public string Value { get; }

  public Option(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      throw new DomainException("Tekst opcije je obavezan.");

    Value = value;
  }
}
```

</details>
<hr></hr>

### 3. Koren kao jedina tačka izmene stanja

Koren agregata predstavlja jedinu tačku kroz koju spoljašnji kod može da menja stanje agregata. Unutrašnji objekti smeju biti izloženi za čitanje, ali svaka izmena korenskih svojstava ili unutrašnjih objekata ide kroz javnu metodu korena. Time koren dobija priliku da pre svake izmene proveri invarijante, jer bi direktna izmena unutrašnjeg objekta te provere zaobišla.

Na primer, arhiviranje pitanja se ne izvodi tako što spoljašnji kod (npr. servis) dohvati `Question` objekat i izmeni ga, već pozivom metode nad anketom. Razlog je što je arhiviranje pitanja dozvoljeno u objavljenoj anketi dok god ostane makar jedno aktivno pitanje u anketi, a to pravilo može da proveri koren agregata ankete.

### 4. Referencira elemente drugih agregata preko identifikatora

Kada element jednog agregata treba da uputi na element drugog agregata, veza se čuva isključivo kao identifikator, a ne kao direktna referenca na objekat. Direktna referenca bi omogućila navigaciju do tuđe unutrašnjosti i njenu izmenu mimo korena, a učitavanje i čuvanje jednog agregata bi povlačilo i drugi, čime bi se dve granice konzistentnosti stopile u jednu transakciju.

U prethodnom primeru anketa, odgovor ispitanika mora da zna na koju anketu se odnosi i na koja pitanja odgovara. `SurveyResponse` zato pamti identifikator ankete, a svaki pojedinačni odgovor identifikator pitanja.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš agregat SurveyResponse</b></summary>

U implementaciji, veze ka tuđem agregatu su obična svojstva čiji je tip jednak tipu identifikatora:

```cs
public sealed class SurveyResponse
{
  public Guid Id { get; }
  public Guid SurveyId { get; }

  private readonly List<Answer> _answers = new();
  public IReadOnlyList<Answer> Answers => _answers.AsReadOnly();

  // Konstruktor

  public void Record(Answer answer) { ... }
}

public sealed class Survey
{
  // ... prethodno definisana svojstva i metode

  public bool CanAccept(Answer answer) { ... }
}

public sealed record Answer
{
  public Guid QuestionId { get; }
  public string Value { get; }

  public Answer(Guid questionId, string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      throw new DomainException("Odgovor mora sadržati izabranu opciju.");

    QuestionId = questionId;
    Value = value;
  }
}
```

Primetimo šta `SurveyResponse` agregat *nema*. Koren agregata ne poseduje svojstvo tipa `Survey`, niti unutrašnji `Answer` vrednosni objekat poseduje direktnu referencu na `Question` objekat.

Kada nekoj operaciji zatrebaju podaci iz obe granice, svaki agregat se učitava i menja kroz sopstvenu granicu. Tako `CanAccept` proverava samo ono što anketa vidi, da odgovor upućuje na njeno aktivno pitanje, a `Record` samo ono što odgovor ispitanika vidi, na primer da odgovor još nije predat. Ko poziva jednu pa drugu metodu obrađuje aplikacioni sloj.

</details>
<hr></hr>

Prateći ovo pravilo, možemo zamisliti domenski sloj aplikacije da se sastoji od više agregata čiji elementi imaju slabe međusobne reference, što sledeća slika ilustruje. U datom prikazu, elementi sa ID poljem predstavljaju entitete (tamniji je koren agregata), a preostali elementi su vrednosni objekti. Sa crnim isprekidanim linijama je ilustrovana slaba asocijacija ka elementu drugog agregata.

![](https://luburic.github.io/ftn-tutor-images/images/ddd/relacije-agregati.png)

## Šta su karakteristike klase koja modeluje koren agregata?

### 1. Identifikator, sopstvena svojstva i unutrašnje reference

Klasa korena agregata sadrži identifikator agregata, sopstvena svojstva i reference na unutrašnje objekte agregata. Identifikator korena je ujedno identifikator celog agregata. Po njemu spoljašnji svet pronalazi agregat i po njemu se, kao i kod svakog entiteta, određuje jednakost.

Na primer, u domenu prodaje, faktura je agregat čiji koren objedinjuje identifikator fakture, sopstvena svojstva poput statusa i datuma izdavanja i reference na unutrašnjost koju čine stavke fakture i poreska stopa. Kupac kome se faktura izdaje je, sa druge strane, samostalan koncept sa sopstvenim životnim ciklusom, pa ostaje izvan fakture.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš osnovni oblik Invoice agregata</b></summary>

U implementaciji, klasa korena objedinjuje identifikator, sopstvena svojstva i reference na unutrašnje objekte:

```cs
public sealed class Invoice
{
  // Identifikator agregata
  public Guid Id { get; }

  // Sopstvena svojstva korena
  public Guid CustomerId { get; }
  public InvoiceStatus Status { get; private set; }
  public DateOnly? IssuedOn { get; private set; }

  // Reference na unutrašnje objekte
  public TaxRate Tax { get; private set; }
  private readonly List<InvoiceLine> _lines = new();
  public IReadOnlyList<InvoiceLine> Lines => _lines.AsReadOnly();
}

public sealed class InvoiceLine
{
  public Guid Id { get; }
  public string Description { get; private set; }
  public int Quantity { get; private set; }
  public decimal UnitPrice { get; private set; }
}

public sealed record TaxRate
{
  public decimal Percent { get; }

  public TaxRate(decimal percent)
  {
    if (percent < 0 || percent > 100)
      throw new DomainException("Poreska stopa mora biti između 0 i 100.");

    Percent = percent;
  }
}

public enum InvoiceStatus
{
  Draft,
  Issued,
  Paid
}
```

Primetimo da su reference na unutrašnjost raznovrsne: `Tax` je vrednosni objekat, a `_lines` kolekcija unutrašnjih entiteta, skrivena iza `IReadOnlyList` pogleda. `CustomerId` je referenca na objekat koji ne pripada ovom agregatu.

</details>
<hr></hr>

### 2. Metode za kontrolisanu promenu stanja agregata

Kao i kod entiteta, klasa korena agregata nudi metode za kontrolisanu promenu stanja, uz očuvanje domenskih pravila koja diktiraju kakve izmene su u kom momentu dozvoljene. Razlika je u dometu: pravila koja koren proverava sada mogu da obuhvate i unutrašnje objekte, pa i izmena unutrašnjeg entiteta počinje metodom korena.

Na primer, stavke fakture se dodaju, menjaju i uklanjaju samo dok je faktura u pripremi. Izdavanje je dozvoljeno samo nad fakturom koja ima bar jednu stavku i tom prilikom se, pored statusa, beleži i datum izdavanja.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš kontrolisanu promenu stanja Invoice agregata</b></summary>

Mehanizme enkapsulacije (skrivanje `set` pristupnika i izlaganje kolekcija kroz `IReadOnlyList`) smo upoznali kod entiteta, a sledeći kod pokazuje kako se isti mehanizmi šire na ceo agregat, sa korenom kao jedinom ulaznom tačkom:

```cs
public sealed class Invoice
{
  // ... prethodno definisana svojstva

  public void AddLine(string description, int quantity, decimal unitPrice)
  {
    if (Status != InvoiceStatus.Draft)
      throw new DomainException("Stavke se mogu dodavati samo dok je faktura u pripremi.");

    _lines.Add(new InvoiceLine(description, quantity, unitPrice));
  }

  public void ChangeLineQuantity(Guid lineId, int quantity)
  {
    if (Status != InvoiceStatus.Draft)
      throw new DomainException("Stavke se mogu menjati samo dok je faktura u pripremi.");

    var line = _lines.SingleOrDefault(l => l.Id == lineId)
      ?? throw new DomainException("Stavka ne postoji na fakturi.");

    line.ChangeQuantity(quantity);
  }

  public void Issue(DateOnly today)
  {
    if (Status != InvoiceStatus.Draft)
      throw new DomainException("Izdati je moguće samo fakturu u pripremi.");
    if (_lines.Count == 0)
      throw new DomainException("Faktura mora imati bar jednu stavku.");

    Status = InvoiceStatus.Issued;
    IssuedOn = today;
  }
}

public sealed class InvoiceLine
{
  // ... prethodno definisana svojstva

  internal void ChangeQuantity(int quantity)
  {
    if (quantity < 1)
      throw new DomainException("Količina mora biti bar 1.");

    Quantity = quantity;
  }
}
```

Primetimo tok kroz `ChangeLineQuantity` metodu: spoljašnji kod ne dohvata `InvoiceLine` da bi ga izmenio, već korenu prosleđuje identifikator stavke. Koren prvo proverava pravilo koje zavisi od njegovog stanja (status fakture), pa tek onda prosleđuje izmenu unutrašnjem entitetu. Vredi istaći i da `Issue` menja dva svojstva u sklopu jedne operacije, što je isti slučaj enkapsulacije koji smo videli kod entiteta, samo na nivou korena agregata.

</details>
<hr></hr>

### 3. Metode za izvođenje domenski značajne informacije

Kao i kod vrednosnog objekta i entiteta, klasa korena agregata može sadržati metode koje vraćaju domenski značajne informacije izvedene iz stanja agregata, ne menjajući ga pri tome. Specifičnost agregata je u tome što se informacija često izvodi kombinovanjem sopstvenih svojstava korena sa stanjem unutrašnjih objekata.

Na primer, ukupan iznos fakture zavisi od svih njenih stavki i od poreske stope. Nijedna stavka pojedinačno ne zna ukupan iznos, a poreska stopa ne zna ni za jednu stavku. Jedini objekat koji vidi sve potrebne podatke je koren, pa izračunavanje prirodno pripada njemu.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš izvođenje informacija u Invoice agregatu</b></summary>

U implementaciji, koren kombinuje informacije koje izvode unutrašnji objekti:

```cs
public sealed class InvoiceLine
{
  // ... prethodno definisana svojstva i metode

  public decimal Amount() => Quantity * UnitPrice;
}

public sealed class Invoice
{
  // ... prethodno definisana svojstva i metode

  public decimal Total()
  {
    var subtotal = _lines.Sum(line => line.Amount());
    return subtotal * (1 + Tax.Percent / 100);
  }
}
```

Primetimo da se ukupan iznos ne čuva kao svojstvo, već se svaki put iznova izvodi iz stavki. Sačuvan zbir bi pri svakoj izmeni stavke mogao da "ispadne iz sinhronizacije" sa njima, dok izveden zbir po definiciji ne može. Vredi istaći i podelu posla: iznos jedne stavke izvodi sama stavka, a koren samo sabira iznose i primenjuje porez.

</details>
<hr></hr>

### 4. Pravila koja zahtevaju uvid u agregat kao celinu

Metode korena agregata implementiraju samo ona domenska pravila koja zahtevaju uvid u stanje agregata kao celine. Pravila koja su lokalna za pojedinačan unutrašnji objekat ostaju u tom objektu, kroz mehanizme koje smo već upoznali. Ovo sprečava da koren naraste u klasu koja zna sve, dok bi unutrašnji objekti postali puke strukture podataka bez ponašanja.

Na primer, u domenu anketa razlikujemo tri nivoa pravila:
1. Tekst ponuđene opcije ne sme biti prazan — pravilo je vidljivo iz same opcije, pa pripada `Option` vrednosnom objektu.
2. Pitanje sa ponuđenim odgovorima mora imati bar dve opcije — pravilo zahteva uvid u jedno pitanje i njegove opcije, pa pripada `Question` entitetu.
3. Objavljena anketa mora imati bar jedno pitanje — pravilo zahteva uvid u status ankete i spisak pitanja zajedno, pa jedino ono pripada korenu.

Isti raspored smo, ne imenujući ga, primenili i kod fakture: koren proverava status fakture, dok `InvoiceLine` sam štiti ispravnost svoje količine.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš raspodelu pravila u Survey agregatu</b></summary>

Prvi nivo, validaciju u konstruktoru `Option` klase, smo već videli u sekciji 2, a sledeći kod prikazuje preostala dva nivoa:

```cs
public sealed class Question
{
  // ... prethodno definisana svojstva

  // Pravilo lokalno za pitanje: bar dve opcije
  internal void RemoveOption(Option option)
  {
    if (_options.Count <= 2)
      throw new DomainException("Pitanje mora imati bar dve opcije.");

    _options.Remove(option);
  }
}

public sealed class Survey
{
  // ... prethodno definisana svojstva i metode

  public void Publish()
  {
    if (Status != SurveyStatus.Draft)
      throw new DomainException("Objaviti je moguće samo anketu u pripremi.");
    if (ActiveQuestionCount() == 0)
      throw new DomainException("Anketa mora imati bar jedno aktivno pitanje.");

    Status = SurveyStatus.Published;
  }
}
```

Primetimo šta `Publish` *ne* proverava: ni prazan tekst opcija, ni broj opcija po pitanju. Ta pravila su već zagarantovana na nižim nivoima. Ne postoji put kroz kod kojim opcija bez teksta može da nastane, niti kojim pitanje može da ostane sa manje od dve opcije. U skladu sa trećom karakteristikom, koren će pozivati `RemoveOption` nakon što proveri da je anketa u pripremi, ali samo pravilo o dve opcije živi u pitanju. Koren se tako bavi isključivo onim što jedino on može da vidi, a na ostatak grafa se oslanja da sam čuva svoju ispravnost.

</details>
<hr></hr>