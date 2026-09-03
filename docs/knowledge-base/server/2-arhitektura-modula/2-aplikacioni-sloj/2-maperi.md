Aplikacioni sloj na svojoj granici prevodi podatke između domenskih objekata i DTO struktura ([Aplikacioni sloj](arhitektura/slojevi/2-aplikacioni-sloj.md)). To prevođenje je mehanički posao: metoda čita svojstvo jednog objekta i upisuje ga u istoimeno svojstvo drugog. Za svaki DTO se piše ovakva metoda:

```cs
private static SurveyResponseDto MapToDto(SurveyResponse surveyResponse) =>
  new(surveyResponse.Id, surveyResponse.SurveyId, surveyResponse.Status);
```

Sa svakim novim DTO i svakim novim svojstvom ovih metoda je sve više, a njihov sadržaj ne nosi nikakvu odluku. Uz to, kada se domenskoj klasi i DTO strukturi doda novo svojstvo, a metoda za prevođenje se ne dopuni, kompajler grešku ne vidi, pa klijent dobija prazno polje.

**Maper** (engl. *mapper*) je biblioteka koja prevođenje između parova tipova izvodi umesto programera, uparujući svojstva po imenu. U ovim lekcijama koristimo biblioteku AutoMapper.

**Profil mapera** (engl. *mapper profile*) je klasa u kojoj se deklarišu parovi tipova koje maper prevodi:

```cs
public sealed class SurveyMapperProfile : Profile
{
  public SurveyMapperProfile()
  {
    CreateMap<SurveyResponse, SurveyResponseDto>();
  }
}
```

U datom kodu treba uočiti sledeće.

- Poziv `CreateMap` deklariše da maper ume da prevede `SurveyResponse` u `SurveyResponseDto`. Nijedno svojstvo se ne navodi, jer maper svojstvo odredišta popunjava iz istoimenog svojstva izvora.
- Profil nasleđuje klasu `Profile` iz biblioteke. Pri pokretanju aplikacije biblioteka pronalazi sve profile i od njihovih deklaracija gradi konfiguraciju, koja se registruje u kontejner zavisnosti ([Registracija zavisnosti](registracija-zavisnosti.md)).

Klasa koja prevodi podatke tada kroz konstruktor prima interfejs `IMapper` i poziva njegovu metodu `Map`:

```cs
public SurveyResponseDto GetResponse(long responseId)
{
  var surveyResponse = _surveyResponseRepository.Get(responseId)
    ?? throw new NotFoundException("Odgovor na anketu ne postoji.");

  return _mapper.Map<SurveyResponseDto>(surveyResponse);
}
```

Metoda `Map` pronalazi deklarisani par za prosleđeni objekat i traženi tip, pravi novu instancu DTO strukture i popunjava njena svojstva. Metoda `MapToDto` sa početka lekcije više ne postoji, a novo istoimeno svojstvo se prevodi bez ikakve izmene koda.

Kada se imena svojstava razlikuju ili se vrednost izvodi, uparivanje se deklariše u profilu:

```cs
CreateMap<SurveyResponse, SurveyResponseDto>()
  .ForMember(dto => dto.AnswerCount,
    options => options.MapFrom(response => response.Answers.Count));
```

Poziv `ForMember` dopunjava deklaraciju para: svojstvo `AnswerCount` se popunjava brojem odgovora, a preostala svojstva se i dalje uparuju po imenu. Profil time ostaje jedino mesto na kom je opisano kako se par tipova prevodi. Profil živi u aplikacionom sloju, jer su tamo vidljivi i domenski objekti i DTO strukture koje prevodi.
