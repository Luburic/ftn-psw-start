U [modularnom monolitu](modularni-monolit.md) aplikacija je podeljena na feature module koji poseduju sopstveni kod i sopstvene podatke. Postavlja se pitanje kako jedan modul dolazi do podataka drugog modula.

Posmatrajmo softver za istraživanje javnog mnjenja sa modulom Ankete, koji poseduje `Survey` i `SurveyResponse` agregate, i modulom Nagrade, koji ispitanicima dodeljuje poene za popunjene ankete. Modul Nagrade bi mogao direktno da učita `SurveyResponse` agregat ili da čita tabele modula Ankete. Takav potez bi spojio unutrašnjosti dva modula. Svaka promena domenskog modela ili šeme baze u modulu Ankete lomila bi kod modula Nagrade, a pravilo poput "računaju se samo predati odgovori" moralo bi da se ponovi van modula koji ga poseduje.

**Kontrakt** (engl. *contract*) je javna površina modula namenjena drugim modulima, koju čine interfejs sa operacijama koje modul nudi i DTO strukture koje te operacije prihvataju i vraćaju.

Modul Ankete definiše kontrakt kroz koji drugi moduli saznaju koje je ankete ispitanik popunio:

```cs
public interface ISurveyApi
{
  List<CompletedSurveyDto> GetCompletedSurveys(long userId);
}

public sealed record CompletedSurveyDto(
  long SurveyId, string Title, DateTime CompletedAt);
```

U datom kodu treba uočiti sledeće.

- Kontrakt ne referencira nijedan drugi deo modula. Njegove DTO strukture sadrže primitivne tipove i identifikatore, pa modul Nagrade ne vidi agregate, repozitorijume ni bazu modula Ankete.
- `CompletedSurveyDto` je odvojen od DTO struktura koje modul Ankete koristi za svoje kontrolere. Kontrakt se dogovara između timova dva modula i namerno je minimalan, jer sadrži samo podatke koje je modul Nagrade zatražio.
- Interfejs implementira aplikacioni servis modula Ankete, jer je odgovaranje na zahtev drugog modula slučaj korišćenja kao i svaki drugi ([Aplikacioni sloj](slojevi/2-aplikacioni-sloj.md)).
- Aplikacioni servis modula Nagrade prima `ISurveyApi` kroz konstruktor, kao i svaki drugi interfejs tehničke sposobnosti. Pozivalac ne zna niti ga zanima kako modul Ankete dolazi do odgovora.

Kontrakt je time druga javna površina modula, pored kontrolera. Kontroleri služe klijentskoj aplikaciji, a kontrakt služi drugim modulima. Kada se kontrakt doda na slojeve čiste arhitekture, elementi jednog modula i njihove zavisnosti izgledaju ovako:

![](architecture.png)
