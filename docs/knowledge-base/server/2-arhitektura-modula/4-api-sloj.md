Aplikacioni servis nudi metode koje ispunjavaju slučajeve korišćenja, ali te metode niko spolja ne može da pozove. Klijentska aplikacija šalje HTTP zahtev, a aplikacioni servis prima identifikator i DTO strukturu. Neko mora iz zahteva da izdvoji podatke koje metoda očekuje, da je pozove i da njen rezultat vrati u obliku koji klijent razume. Taj posao pripada **API sloju**, koji poruke protokola prevodi u pozive aplikacionog sloja, a rezultat tih poziva u odgovore protokola.

U našem projektu je protokol HTTP, pa API sloj čine kontroleri iz [lekcije o kontrolerima](../1-aspnet/2-kontroleri.md). Ta lekcija je pokazala kako radni okvir bira akciju, popunjava njene parametre i pretvara povratnu vrednost u odgovor. Ovde razmatramo šta sloj znači za oblik kontrolera, odnosno kako se kontroleri dele, šta jedna akcija radi i šta ne radi.

## Kontroler po grupi slučajeva korišćenja

Kontroler postoji za svaku grupu slučajeva korišćenja aplikacionog sloja i nosi njeno ime. Kroz konstruktor prima komandnu i upitnu klasu te grupe, obe kao konkretne klase. Sledeći kod prikazuje kostur kontrolera ispitaničke grupe, gde su tela akcija izostavljena:

```cs
[ApiController]
[Route("api/surveys")]
[Authorize]
public sealed class SurveyRespondingController : ControllerBase
{
  private readonly SurveyRespondingService _respondingService;
  private readonly SurveyRespondingQueries _respondingQueries;

  public SurveyRespondingController(SurveyRespondingService respondingService,
    SurveyRespondingQueries respondingQueries)
  {
    _respondingService = respondingService;
    _respondingQueries = respondingQueries;
  }

  [HttpPost("{surveyId:guid}/response")]
  public async Task<ActionResult<Guid>> StartResponse(Guid surveyId) { ... }

  [HttpPost("responses/{responseId:guid}/answers")]
  public async Task<ActionResult> SubmitAnswer(Guid responseId, [FromBody] AnswerDto answerDto) { ... }

  [HttpGet("{surveyId:guid}/response")]
  public async Task<ActionResult<SurveyResponseDto>> GetResponse(Guid surveyId) { ... }
}
```

U datom kodu treba uočiti sledeće:

- Kontroler vidi samo dve klase aplikacionog sloja. Ne prima repozitorijum, jedinicu posla ni agregat, jer API sloj referencira isključivo aplikacioni sloj. U našem projektu je to i fizički onemogućeno, jer projekat API sloja ne referencira projekat domenskog niti infrastrukturnog sloja, a ta pravila proverava [arhitektonski test](../3-modularni-monolit/4-arhitektonski-testovi.md).
- Komandna i upitna klasa se primaju bez interfejsa. Kontroler i servis su u istom modulu, a kontroler zavisi od servisa u smeru koji čista arhitektura već propisuje, pa interfejs ne bi obrnuo nijednu zavisnost ([Čista arhitektura](5-čista-arhitektura.md)).
- Atribut `[Route]` daje prefiks adrese po agregatu oko kog se grupa okuplja. Kontroleri drugih grupa istog agregata, na primer autorske i pregledne grupe, dele isti prefiks `api/surveys`, a razlikuju se po ostatku adrese i HTTP metodi svojih akcija. Dva kontrolera nikada ne deklarišu istu HTTP metodu nad istom adresom.
- Atribut `[Authorize]` traži da zahtev nosi važeći identitet korisnika. Zahtev bez identiteta radni okvir odbija odgovorom sa statusnim kodom 401, pre poziva bilo koje akcije. Šta akcija radi sa identitetom obrađuje naredna sekcija.

## Tri koraka akcije

Svaka akcija obavlja tačno tri koraka:
1. vezuje podatke iz zahteva za svoje parametre,
2. poziva jednu metodu komandne ili upitne klase i
3. rezultat poziva prevodi u HTTP odgovor.

Sledeći kod prikazuje tela tri akcije iz prethodnog kostura:

```cs
[HttpPost("{surveyId:guid}/response")]
public async Task<ActionResult<Guid>> StartResponse(Guid surveyId)
{
  return await _respondingService.StartResponseAsync(surveyId, User.GetUserId());
}

[HttpPost("responses/{responseId:guid}/answers")]
public async Task<ActionResult> SubmitAnswer(Guid responseId, [FromBody] AnswerDto answerDto)
{
  await _respondingService.SubmitAnswerAsync(responseId, answerDto);
  return NoContent();
}

[HttpGet("{surveyId:guid}/response")]
public async Task<ActionResult<SurveyResponseDto>> GetResponse(Guid surveyId)
{
  return await _respondingQueries.GetResponseAsync(surveyId, User.GetUserId());
}
```

U datom kodu treba uočiti sledeće:

- Prvi korak obavlja radni okvir vezivanjem modela. Identifikator stiže iz adrese, a `AnswerDto` iz tela zahteva. Akcija vezuje ulaznu DTO strukturu aplikacionog sloja direktno, bez posebne klase za telo zahteva, pa je DTO struktura ujedno i oblik poruke koju klijent šalje.
- Drugi korak je jedan poziv. Komanda `StartResponseAsync` iz [lekcije o komandama i upitima](2-aplikacioni-sloj/1-komande-i-upiti.md) prima identifikator ankete i identifikator korisnika. Prvi je stigao iz adrese, a drugi akcija čita iz svojstva `User`, koje je middleware za proveru identiteta popunio iz tokena koji je klijent poslao. Metoda `GetUserId` iz tog svojstva izdvaja identifikator i akcija ga prosleđuje kao običnu vrednost, pa aplikacioni sloj ne zna odakle identifikator potiče. Kako token nastaje i kako se proverava obrađuje TODO.
- Treći korak zavisi od toga šta je metoda vratila. Komanda `StartResponseAsync` vraća identifikator novog odgovora, pa ga akcija vraća direktno, a radni okvir šalje odgovor sa statusnim kodom 200. Komanda `SubmitAnswerAsync` ne vraća ništa, pa akcija poziva `NoContent`, čime šalje odgovor sa statusnim kodom 204 i praznim telom. Upit `GetResponseAsync` vraća izlaznu DTO strukturu, koju akcija vraća direktno.
- Povratni tip je uvek `ActionResult<T>` sa tipom podatka koji akcija šalje, ili `ActionResult` kada telo odgovora ne postoji. Iz tih tipova radni okvir zna oblik svakog odgovora, pa svaka akcija mora da ga navede.
- Sve tri akcije su asinhrone, jer metode aplikacionog sloja vraćaju `Task`. Akcija poziv sačeka sa `await` i rezultat vrati, kako smo videli u [lekciji o asinhronom programiranju](../1-aspnet/4-asinhrono-programiranje.md).

## Šta akcija ne radi

Tri koraka iscrpljuju posao akcije. Kod koji bi se prirodno našao u akciji, a ne pripada joj, ima svoje mesto u nekom drugom sloju:

- Akcija ne hvata izuzetke. Kada `SubmitAnswerAsync` baci `NotFoundException` jer odgovor ne postoji, ili `DomainException` jer anketa ne prihvata odgovor, izuzetak prolazi kroz akciju nepromenjen. Middleware iz lekcije o kontrolerima ga prevodi u odgovor sa statusnim kodom 404, odnosno 400. Blok try/catch u akciji bi ponovio posao koji middleware obavlja za sve akcije.
- Akcija ne proverava podatke. Vezivanje modela proverava samo da li JSON zapis odgovara obliku DTO strukture. Da li je vrednost odgovora prazna proverava konstruktor vrednosnog objekta `Answer`, a da li anketa prihvata odgovor proverava agregat. Provera u akciji bi isto pravilo napisala na dva mesta, a domenski sloj bi i dalje morao da proveri svoje.
- Akcija ne odlučuje. Uslovna naredba o stanju podataka u akciji je znak da je domensko pravilo pobeglo iz agregata. Akcija ne zna ni da li odgovor postoji, ni da li je anketa objavljena, jer te podatke ne vidi.

Kontroler koji prati ova pravila je klasa bez logike, koju je moguće u celosti pročitati kao spisak adresa i metoda aplikacionog sloja koje te adrese pozivaju. Svaka promena pravila domena, skladišta ili redosleda koraka slučaja korišćenja ga zaobilazi.
