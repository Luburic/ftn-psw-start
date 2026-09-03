U svakom dosadašnjem primeru je granicu aplikacionog sloja prešao neki podatak. Komanda je primila `AnswerDto`, a upiti su vratili `SurveySummaryDto` i `SurveyResultsDto`. Ovde razmatramo zašto ti tipovi postoje, kako izgledaju i kako se popunjavaju.

## DTO struktura

Posmatrajmo šta bi se desilo bez njih. Kontroler bi komandi prosledio `Answer` vrednosni objekat, a upit bi kontroleru vratio `Survey` agregat. U prvom smeru radni okvir mora da napravi `Answer` iz JSON zapisa, a konstruktor tog objekta baca izuzetak za svaki neispravan podatak, pre nego što je zahtev stigao do aplikacionog sloja. U drugom smeru kontroler dobija ceo agregat, pa sam bira koja svojstva šalje klijentu, a radni okvir pri prevođenju u JSON obilazi sve unutrašnje objekte i njihove reference.

**DTO struktura** (engl. *data transfer object*) je klasa bez ponašanja koja nosi podatke preko granice aplikacionog sloja. **Ulazna DTO struktura** je parametar komande ili upita i nosi podatke koje je klijent poslao. **Izlazna DTO struktura** je povratna vrednost upita i nosi tačno one podatke koje klijent prikazuje. Sledeći kod prikazuje po jednu:

```cs
public sealed record AnswerDto(long QuestionId, string Value);

public sealed record SurveySummaryDto(long Id, string Title, int QuestionCount);
```

U datom kodu treba uočiti sledeće.

- Obe strukture su deklarisane kao `record` sa pozicionim parametrima. Kompajler iz jedne linije generiše svojstva, konstruktor i jednakost po vrednostima, što je isti konstrukt koji smo koristili za vrednosne objekte.
- Ulazna struktura nema validaciju. Ona samo prenosi ono što je klijent poslao, a pravila proverava konstruktor domenskog objekta u koji je komanda prevodi. Klijent koji pošalje prazan odgovor dobija izuzetak iz konstruktora vrednosnog objekta `Answer`, preveden u odgovor sa statusnim kodom 400.
- Izlazna struktura nosi broj pitanja, a ne spisak pitanja. Klijent koji prikazuje spisak anketa ne dobija ništa što ne prikazuje, a agregat i njegova unutrašnjost ostaju nevidljivi van aplikacionog sloja.

## Popunjavanje DTO strukture

Ulazna struktura se u domenski objekat prevodi isključivo pozivom konstruktora, kao u komandi `SubmitAnswerAsync`, jer konstruktor jedini garantuje ispravnost. Izlazna struktura se popunjava na jedan od dva načina, u zavisnosti od toga da li domenski objekat postoji u memoriji:

1. Kada upit ne učitava agregat, struktura se popunjava projekcijom pravo iz skladišta. Ovo je čist upit, čiju smo mehaniku videli u repozitorijumu za čitanje.
2. Kada je domenski objekat već u memoriji, struktura se popunjava prevođenjem tog objekta. Ovo je slučaj upita koji koristi agregat, gde je domenski servis vratio objekat `SurveyResults`.

Za drugi način je potreban kod koji čita svojstva domenskog objekta i upisuje ih u istoimena svojstva DTO strukture:

```cs
private static SurveyResultsDto MapToDto(SurveyResults results) =>
  new(results.SurveyId, results.Questions.Select(MapToDto).ToList());

private static QuestionResultDto MapToDto(QuestionResult result) =>
  new(result.QuestionId, result.Text, result.AnswerCount);
```

Sa svakim novim parom tipova i svakim novim svojstvom ovakvih metoda je sve više, a nijedna ne nosi odluku. Kada se domenskom objektu i DTO strukturi doda novo svojstvo, a metoda se ne dopuni, kompajler grešku ne vidi, pa klijent dobija prazno polje.

## Maper

**Maper** (engl. *mapper*) je biblioteka koja prevođenje između parova tipova izvodi umesto programera, uparujući svojstva po imenu. Koristimo biblioteku AutoMapper. **Profil mapera** (engl. *mapper profile*) je klasa u kojoj se deklarišu parovi tipova koje maper prevodi:

```cs
public sealed class SurveyMapperProfile : Profile
{
  public SurveyMapperProfile()
  {
    CreateMap<SurveyResults, SurveyResultsDto>();
    CreateMap<QuestionResult, QuestionResultDto>();
  }
}
```

Upit tada kroz konstruktor prima interfejs `IMapper` i poziva njegovu metodu `Map`:

```cs
public async Task<SurveyResultsDto> GetResultsAsync(long surveyId)
{
  var survey = await _surveyRepository.GetByIdAsync(surveyId)
    ?? throw new NotFoundException("Anketa ne postoji.");
  var responses = await _surveyResponseRepository.GetBySurveyAsync(surveyId);

  var results = _resultsCalculator.Calculate(survey, responses);
  return _mapper.Map<SurveyResultsDto>(results);
}
```

U datom kodu treba uočiti sledeće.

- Poziv `CreateMap` deklariše da maper ume da prevede `SurveyResults` u `SurveyResultsDto`. Nijedno svojstvo se ne navodi, jer maper svojstvo odredišta popunjava iz istoimenog svojstva izvora. Ugnježdeni objekti se prevode po sopstvenom paru, pa spisak `Questions` nastaje iz druge deklaracije.
- Profil nasleđuje klasu `Profile` iz biblioteke. Pri pokretanju aplikacije biblioteka pronalazi sve profile, od njihovih deklaracija gradi konfiguraciju i registruje je u kontejner zavisnosti ([Registracija zavisnosti](../../1-aspnet/3-registracija-zavisnosti.md)).
- Metoda `Map` pronalazi deklarisani par za prosleđeni objekat i traženi tip, pravi novu instancu DTO strukture i popunjava njena svojstva. Metode `MapToDto` više ne postoje, a novo istoimeno svojstvo se prevodi bez ikakve izmene koda.
- Maper se koristi samo u smeru od domenskog objekta ka DTO strukturi. Suprotan smer bi zaobišao konstruktor domenskog objekta, a sa njim i proveru pravila.

Kada se imena svojstava razlikuju ili se vrednost izvodi, uparivanje se deklariše u profilu:

```cs
CreateMap<SurveyResults, SurveyResultsDto>()
  .ForMember(dto => dto.QuestionCount,
    options => options.MapFrom(results => results.Questions.Count));
```

Poziv `ForMember` dopunjava deklaraciju para. Svojstvo `QuestionCount` se popunjava brojem pitanja, a preostala svojstva se i dalje uparuju po imenu. Profil živi u aplikacionom sloju, jer su tamo vidljivi i domenski objekti i DTO strukture koje prevodi, i jedan je za ceo modul.
