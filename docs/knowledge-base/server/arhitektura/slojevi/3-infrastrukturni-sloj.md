**Infrastrukturni sloj implementira tehničke sposobnosti softvera.**

Infrastrukturni sloj sadrži konkretan kod koji radi sa bazom podataka, udaljenim API-jima, datotekama, bibliotekama i radnim okvirima. Njegove klase realizuju tehnički deo operacija koje je aplikacioni sloj opisao kroz interfejse.

U posmatranom primeru ([Čista arhitektura](../čista-arhitektura.md)) infrastrukturne klase učitavaju `Survey` i `SurveyResponse` agregate i čuvaju promenjeni `SurveyResponse`.

### 1. Repozitorijumske klase

**Infrastrukturni sloj sadrži repozitorijumske klase koje učitavaju i čuvaju agregate koristeći konkretan sistem za skladištenje podataka.**

Repozitorijumska klasa poznaje konkretan sistem za skladištenje podataka. Ona zna kako se podaci čitaju, kako se od njih rekonstruiše agregat i kako se stanje agregata čuva. Repozitorijum učitava agregat kao celinu potrebnu za izvršavanje njegovih pravila. `SurveyRepository` zato učitava pitanja i opcije potrebne metodi `CanAccept`. `SurveyResponseRepository` učitava postojeće odgovore i status potreban metodi `Record`.

Pored repozitorijuma koji učitavaju i čuvaju agregate, infrastrukturni sloj sadrži i repozitorijume za čitanje. Repozitorijum za čitanje implementira interfejs poput `ISurveyReadRepository` tako što podatke projektuje pravo u DTO strukture, bez rekonstrukcije agregata. Ovi repozitorijumi služe upitima aplikacionog sloja ([Komande i upiti](../komande-i-upiti.md)).

### 2. Konektorske klase

**Infrastrukturni sloj sadrži konektorske klase koje interaguju sa API-jem drugih aplikacija.**

Konektorska klasa implementira tehničku sposobnost kroz komunikaciju sa udaljenim softverom. Ona poznaje protokol, adresu, autentifikaciju i format podataka drugog sistema. Tako možemo pronaći konektorske klase za:
- HTTP komunikaciju, gde klasa šalje HTTP zahteve eksternom API-ju i prihvata HTTP odgovor.
- SMTP komunikaciju, gde klasa šalje email.
- FTP komunikaciju, gde klasa pakuje podatke u datoteke i šalje ih na udaljeni sistem.

Konektorske klase implementiraju interfejse aplikacionog sloja koje definišu ovu tehničku sposobnost.

### 3. Klasa lokalne tehničke sposobnosti

**Infrastrukturni sloj sadrži stručnjačke klase koje nude lokalnu tehničku sposobnost kroz rad sa bibliotekom ili radnim okvirom.**

Stručnjačka klasa koristi tehničko znanje biblioteke, radnog okvira ili mogućnosti lokalnog sistema. Ona ne mora da komunicira sa drugom aplikacijom. Na primer, `ISurveyReporter` može da definiše tehničku sposobnost za generisanje izveštaja, što može da bude realizovan klasom koja koristi biblioteku za formiranje PDF dokumenta:

```cs
public sealed class PdfSurveyReporter : ISurveyReporter
{
  public string Generate(Survey survey, List<SurveyResponse> response)
  {
    // Obradi podatke koji stižu kao argumenti poziva metode.
    // Upotrebi biblioteku za generisanje PDF datoteke.
    // Sačuvaj PDF i vrati putanju do njega.
  }
}
```

Dodatni primeri stručnjačkih klasa su:

- klasa koja šifruje osetljive odgovore,
- klasa koja kompresuje izvezene datoteke,
- klasa koja čita podatke iz Excel dokumenta i
- klasa koja generiše slučajne identifikatore.

Aplikacioni sloj definiše interfejs prema potrebama slučaja korišćenja. Klasa koja implementira interfejs bira i koristi konkretan tehnički mehanizam.
