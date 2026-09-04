**API sloj adaptira konkretan protokol u pozive aplikacionog sloja i obrnuto.**

Korisnik ne poziva aplikacioni servis direktno. Klijentska aplikacija šalje HTTP zahtev ili aktivira aplikaciju putem nekog drugog protokola. API sloj iz tog zahteva izdvaja podatke potrebne aplikacionom servisu i njegov rezultat prevodi u odgovor odgovarajućeg protokola.

### 1. Kontrolerske klase

**API sloj sadrži kontrolerske klase koje obrađuju zahtev određenog protokola, aktiviraju odgovarajući slučaj korišćenja i pakuju rezultat u očekivani format.**

U posmatranom primeru ([Čista arhitektura](../čista-arhitektura.md)), kontroler za evidenciju odgovora na pitanje obavlja sledeće korake:

1. čita identifikator odgovora iz putanje,
2. deserijalizuje telo HTTP zahteva u DTO,
3. poziva aplikacioni servis,
4. pakuje rezultat u odgovor sa statusom `200 OK` i
5. domensku grešku prevodi u odgovor sa statusom `422 Unprocessable Content`.


<hr></hr>
<details>
<summary><b>Klikni da analiziraš kod SurveyResponseController klase</b></summary>

Kontroler za evidentiranje odgovora može da izgleda ovako:

```cs
[ApiController]
[Route("api/survey-responses")]
public sealed class SurveyResponseController : ControllerBase
{
  private readonly SurveyResponseService _surveyResponseService;

  public SurveyResponseController(SurveyResponseService surveyResponseService)
  {
    _surveyResponseService = surveyResponseService;
  }

  [HttpPost("{responseId:guid}")]
  public ActionResult<SubmitAnswerResultDto> SubmitAnswer(
    Guid responseId, [FromBody] AnswerDto answerDto)
  {
    try
    {
      var result = _surveyResponseService.SubmitAnswer(responseId, answerDto);
      return Ok(result);
    }
    catch (InvalidOperationException exception)
    {
      return UnprocessableEntity(new { Message = exception.Message });
    }
  }
}
```

</details>
<hr></hr>


Obrada grešaka može da bude izdvojena u zajedničku komponentu API sloja (middleware). U tom slučaju kontroler sadrži samo poziv servisa, dok komponenta za obradu grešaka prevodi izuzetak u odgovarajući HTTP status.
