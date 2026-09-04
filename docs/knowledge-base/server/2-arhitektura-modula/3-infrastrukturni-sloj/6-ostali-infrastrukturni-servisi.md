Repozitorijum je jedna tehnička sposobnost koju slučaj korišćenja zahteva. Slučaj korišćenja može da zahteva i da se nekome van sistema nešto saopšti, ili da se proizvede nešto što modul sopstvenim kodom ne ume da proizvede. Objavljivanje ankete treba da obavesti ispitanike elektronskom poštom, a pregled rezultata treba da ponudi izveštaj u PDF obliku. Ovde razmatramo kako infrastrukturni sloj implementira sposobnost koja nije rad sa bazom i šta ostaje sa koje strane interfejsa.

## Konektorska klasa

**Konektorska klasa** je klasa infrastrukturnog sloja koja tehničku sposobnost implementira komunikacijom sa drugim sistemom. Ona poznaje protokol tog sistema, njegovu adresu, podatke za prijavu i format u kom sistem prima i vraća podatke. Nijedna od te četiri stvari ne pripada aplikacionom sloju, a svaka se menja nezavisno od slučaja korišćenja koji je koristi. Konektorske klase razlikujemo po tome šta je sa druge strane:

- HTTP klijent, koji poziva API druge aplikacije i tumači njen odgovor,
- pošiljalac poruka, koji šalje elektronsku poštu ili SMS poruke i
- razmena datoteka, koja podatke pakuje u datoteke i predaje ih udaljenom sistemu.

Posmatrajmo obaveštavanje ispitanika. Aplikacioni sloj deklariše interfejs koji opisuje šta slučaju korišćenja treba, a infrastrukturni sloj ga implementira slanjem elektronske pošte:

```cs
public interface IRespondentNotifier
{
  Task NotifySurveyPublishedAsync(Survey survey, List<string> emails);
}

public sealed class SmtpRespondentNotifier : IRespondentNotifier
{
  private readonly string _host;
  private readonly string _sender;

  public SmtpRespondentNotifier(IConfiguration configuration)
  {
    _host = configuration["Smtp:Host"];
    _sender = configuration["Smtp:Sender"];
  }

  public async Task NotifySurveyPublishedAsync(Survey survey, List<string> emails)
  {
    using var client = new SmtpClient(_host);
    foreach (var email in emails)
    {
      var message = new MailMessage(_sender, email,
        $"Nova anketa: {survey.Title}",
        $"Objavljena je anketa \"{survey.Title}\" sa {survey.Questions.Count} pitanja.");
      await client.SendMailAsync(message);
    }
  }
}
```

U datom kodu treba uočiti sledeće:

- Interfejs govori jezikom slučaja korišćenja. Prima anketu i adrese, a ne poruku, i u nazivu nosi šta se dešava, a ne kako se saopštava. Aplikacioni servis koji ga poziva ne zna da iza njega stoji elektronska pošta.
- Klasa prevodi domenski objekat u poruku. Naslov i tekst poruke nastaju iz svojstava ankete, a to prevođenje je jedino mesto na kom se sadržaj obaveštenja može promeniti.
- Adresa servera i pošiljalac se čitaju iz konfiguracije, kao i konekcioni string u [lekciji o kontekstu](2-efc-kontekst-i-model.md). Klasa nema nijednu vrednost upisanu u kod, pa se razvojno i produkciono okruženje razlikuju samo po konfiguraciji.
- Klasa `SmtpClient` iz osnovne biblioteke otvara konekciju ka serveru elektronske pošte i šalje poruku po SMTP protokolu. Kada server ne odgovori, poziv baca izuzetak, koji prolazi kroz komandu do middleware-a kao i svaki drugi neočekivani izuzetak.

Klasa za HTTP komunikaciju ima isti oblik. Umesto `SmtpClient` koristi `HttpClient`, adresu druge aplikacije čita iz konfiguracije, domenski objekat prevodi u JSON telo zahteva, a JSON odgovor u DTO strukturu koju interfejs obećava.

## Stručnjačka klasa

**Stručnjačka klasa** je klasa infrastrukturnog sloja koja tehničku sposobnost implementira lokalno, kroz biblioteku ili mogućnost radnog okvira, bez komunikacije sa drugim sistemom. Tipovi i način pozivanja biblioteke su aplikacionom sloju jednako strani kao i protokol drugog sistema, pa ostaju iza interfejsa. Stručnjačke klase razlikujemo po vrsti znanja koje nose:

- generisanje dokumenata, poput PDF, Excel ili CSV datoteka,
- kriptografija, poput heširanja lozinki i izdavanja tokena i
- obrada datoteka i medija, poput promene veličine slike.

Posmatrajmo izveštaj o rezultatima ankete. Domenski servis `SurveyResultsCalculator` iz [lekcije o domenskom servisu](../1-domenski-sloj/5-domenski-servis.md) izračunava rezultate, a stručnjačka klasa ih pretvara u PDF dokument:

```cs
public interface ISurveyReportGenerator
{
  byte[] Generate(SurveyResults results);
}

public sealed class PdfSurveyReportGenerator : ISurveyReportGenerator
{
  public byte[] Generate(SurveyResults results)
  {
    var document = new PdfDocument();
    document.AddTitle($"Rezultati ankete {results.SurveyId}");
    foreach (var question in results.Questions)
    {
      document.AddParagraph($"{question.Text}: {question.AnswerCount} odgovora");
    }
    return document.ToBytes();
  }
}
```

U datom kodu treba uočiti sledeće:

- Interfejs prima domenski objekat i vraća niz bajtova. Nijedan tip PDF biblioteke ne prelazi granicu interfejsa, pa promena biblioteke menja samo ovu klasu.
- Klasa ne zna odakle su rezultati došli niti šta će sa dokumentom biti. Upit aplikacionog sloja učita agregate, pozove domenski servis, prosledi rezultate ovoj klasi i dobijene bajtove vrati kontroleru.
- Pozivi `AddTitle`, `AddParagraph` i `ToBytes` predstavljaju biblioteku za rad sa PDF dokumentima. Konkretna biblioteka bira se pri implementaciji, a nazivi njenih metoda se razlikuju od biblioteke do biblioteke.

U našem projektu je stručnjačka klasa `JwtTokenFactory` modula za identitet, koja od podataka o korisniku, kroz biblioteku za rad sa JWT tokenima, pravi potpisan token koji klijent šalje uz svaki zahtev.

## Interfejs tehničke sposobnosti

Obe vrste klasa implementiraju interfejs koji je aplikacioni sloj deklarisao, kao što to rade i repozitorijumi. Interfejs imenuje sposobnost koja slučaju korišćenja treba, a ne tehnologiju kojom se ostvaruje. Zato njegove metode primaju i vraćaju isključivo domenske objekte, DTO strukture i proste tipove, a nikada tip biblioteke ili protokola. Isti interfejs može danas da zadovolji konektorska klasa, a sutra stručnjačka. Obaveštenje ispitanicima koje se danas šalje kroz SMTP server može sutra da ide kroz HTTP API spoljašnjeg servisa za slanje pošte, a aplikacioni servis to ne primećuje.

Implementacija se registruje u metodi proširenja modula, uz repozitorijume i jedinicu posla:

```cs
services.AddScoped<IRespondentNotifier, SmtpRespondentNotifier>();
services.AddScoped<ISurveyReportGenerator, PdfSurveyReportGenerator>();
```

## Put jedne komande

Povežimo pojmove praćenjem komande koja objavljuje anketu i obaveštava ispitanike.

1. Kontroler prima `POST /api/surveys/{id}/publish` i poziva metodu `PublishSurveyAsync` klase `SurveyAuthoringService`, koja kroz konstruktor prima repozitorijum ankete, jedinicu posla i `IRespondentNotifier`.
2. Komanda kroz repozitorijum učitava anketu i poziva metodu `Publish`, koja proverava pravila i menja status.
3. Komanda poziva `SaveChangesAsync` na jedinici posla. Anketa je objavljena u bazi.
4. Tek tada komanda poziva `NotifySurveyPublishedAsync`. Konektorska klasa prevodi anketu u poruke i šalje ih. Obaveštenje se šalje nakon čuvanja, jer bi obaveštenje o anketi čije čuvanje nije uspelo bilo gore od izostanka obaveštenja.
5. Kada slanje ne uspe, anketa ostaje objavljena, a pozivalac dobija grešku. Kako se takav neuspeh naknadno ispravlja pitanje je koje ova lekcija ne obrađuje.
