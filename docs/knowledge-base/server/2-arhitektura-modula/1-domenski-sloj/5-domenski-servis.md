**Domenski servis** je stručnjačka funkcionalna klasa koja se uvodi kada domensko pravilo ne pripada prirodno jednom entitetu ili vrednosnom objektu, već zahteva uvid u više agregata. Posmatrajmo softver u kojem ispitanik popunjava anketu, gde `Survey` agregat modeluje anketu i njena pitanja, a `SurveyResponse` agregat modeluje odgovore jednog ispitanika. Možemo zamisliti slučaj korišćenja gde je potrebno sračunati rezultate ankete, odnosno raspodelu odgovora po svakom pitanju. Obračun se oslanja na domenska pravila: broje se samo predati odgovori i u obzir se uzimaju samo odgovori na pitanja koja nisu arhivirana.

Ovaj obračun ne pripada nijednom agregatu. `Survey` poznaje pitanja i njihove opcije, ali po pravilu o referenciranju preko identifikatora ne vidi nijedan odgovor. Pojedinačan `SurveyResponse` vidi svoje odgovore, ali ne i strukturu ankete niti odgovore ostalih ispitanika. Pošto nijedan koren ne vidi sve što je potrebno, pravilo se izdvaja u domenski servis:

```cs
public sealed class SurveyResultsCalculator
{
  public SurveyResults Calculate(
    Survey survey, List<SurveyResponse> responses)
  {
    // Domenska pravila za obračun raspodele odgovora
  }
}
```

Domenski servis radi isključivo sa domenskim objektima. On ne obrađuje HTTP zahtev, ne učitava podatke i ne bira tehničku biblioteku. Aplikacioni servis ga poziva, a pre toga učita potrebne agregate (kroz repozitorijume) i prosleđuje ih domenskom servisu. Domenski servis najčešće ili izvodi informaciju na osnovu više agregata (kao u prethodnom primeru) ili koordinira njihove domenske promene.
