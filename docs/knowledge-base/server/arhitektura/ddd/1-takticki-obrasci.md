Svaki softver koji rešava stvaran poslovni problem sadrži **domensku logiku**, odnosno pravila koja određuju koje operacije je u tom domenu dozvoljeno izvršiti, šta je ispravno stanje određenog skupa podataka i kako se određeni podaci dobijaju. Klase koje te podatke i pravila predstavljaju u kodu zajedno zovemo **domenski model** (engl. *domain model*).

Prilikom dizajna domenskog modela postavljamo dva velika pitanja:
1. Gde živi domenska logika?
2. Kako su objekti domenskog modela međusobno povezani?

Spram odgovora na prvo pitanje dobijamo dva pristupa dizajniranja domenskog modela:
- **Anemičan domenski model**, gde domenski model predstavlja model podataka bez ikakvih metoda (klase podataka), a servisne klase implementiraju domensku logiku.
- **Bogati domenski model**, gde domenski model sadrži podatke i implementira poslovna pravila koja rade sa tim podacima (stručnjačke klase).

Spram odgovora na drugo pitanje dobijamo dva pristupa povezivanju modela:
- **Potpuno povezan objektni graf**, gde objekti drže direktne reference jedni na druge, pa se od svakog objekta navigacijom stiže do svakog drugog.
- **Graf isečen na omeđene celine**, gde direktne reference postoje samo unutar male celine, dok celine jedna na drugu upućuju isključivo identifikatorima.

Sve četiri kombinacije odgovora postoje u praksi. Nisu sve kombinacije jednako dobre, niti je ijedna najbolja u svakom kontekstu. U nastavku analiziramo karakteristike svakog modela, kontekste u kojima su dobar izbor i njihova ograničenja.

Kroz ceo tekst koristimo isti primer: domen biblioteke, u kojem članovi pozajmljuju knjige. Pozajmica ima rok vraćanja i može se produžiti, uz pravila da se samo aktivna pozajmica može produžiti, da se pozajmica u kašnjenju ne može produžiti i da je broj produženja ograničen na dva. Isti domen ćemo modelovati na tri načina i posmatrati šta se sa ovim pravilima dešava.

## Anemičan domenski model

Anemičan domenski model bira klase podataka kao odgovor na prvo pitanje. Domenske klase su čiste strukture podataka, sa javnim `get` i `set` pristupnicima i bez ijedne metode, dok celokupna domenska logika živi u zasebnim servisnim klasama koje te strukture čitaju i menjaju spolja. Model je "anemičan" jer klase nose podatke, ali ne i krv domena — pravila.
 
<hr></hr>
<details>
<summary><b>Klikni da analiziraš anemičan model pozajmice</b></summary>

U implementaciji, pozajmica je struktura podataka, a pravila produženja žive u servisnoj klasi:
 
```cs
public class Loan
{
  public long Id { get; set; }
  public long BookId { get; set; }
  public long MemberId { get; set; }
  public DateOnly DueDate { get; set; }
  public int RenewalCount { get; set; }
  public LoanStatus Status { get; set; }
}
 
public enum LoanStatus
{
  Active,
  Returned
}
 
public class LoanService
{
  public void Renew(Loan loan, DateOnly today)
  {
    if (loan.Status != LoanStatus.Active)
      throw new InvalidOperationException("Samo aktivna pozajmica može biti produžena.");
    if (loan.DueDate < today)
      throw new InvalidOperationException("Pozajmica u kašnjenju ne može biti produžena.");
    if (loan.RenewalCount == 2)
      throw new InvalidOperationException("Pozajmica se može produžiti najviše dva puta.");
 
    loan.DueDate = loan.DueDate.AddDays(14);
    loan.RenewalCount = loan.RenewalCount + 1;
  }
}
```
 
Primetimo da su podaci i pravila fizički razdvojeni. `Loan` ne zna ni za jedno pravilo o produženju, a `LoanService` ne poseduje nijedan podatak. Sva pravila koja `Renew` metoda proverava važe samo ako svaki deo sistema disciplinovano prolazi kroz nju. Model to ničim ne garantuje, jer je `loan.RenewalCount = 0` legalan potez iz ugla kompajlera.
 
</details>
<hr></hr>

### Kada je dobar izbor
 
Anemičan model je prikladan u domenima u kojima pravila gotovo da nema. Tipični primeri su:
- CRUD aplikacije i administrativni ekrani koji su u suštini "forme nad podacima", 
- Izveštaji i integracije koje podatke samo premeštaju iz jednog oblika u drugi i
- Prototipovi kojima je brzina izrade važnija od dugovečnosti.

Strukture bez ponašanja se trivijalno serijalizuju, mapiraju na bazu i prenose kroz slojeve aplikacije, a razume ih svaki programer na prvi pogled.
 
### Ograničenja
 
Problemi počinju kada pravila počnu da se množe i uključuju:
1. Model po konstrukciji dozvoljava nevalidno stanje: bilo koji deo koda može da upiše bilo šta u bilo koje svojstvo, pa ispravnost podataka postaje stvar konvencije, a ne garancija.
2. Ista pravila se vremenom dupliraju. Za dati primer, produženje se osim iz servisa poziva i iz zadatka koji se izvršava noću, pa iz administratorskog ekrana i svaki od tih puteva nosi svoju kopiju provera koje se neprimetno raziđu.
3. Podaci i pravila koja su konceptualno jedna celina žive na dva mesta, pa svaka izmena domena znači usaglašenu izmenu strukture na jednom i logike na drugom mestu, uz potragu po svim servisima koji tu strukturu diraju.

## Potpuno povezan objektni graf

Potpuno povezan objektni graf podrazumeva da objekti drže direktne reference jedni na druge, pa se od svakog objekta navigacijom stiže do svakog drugog. Ovo je objektno-orijentisano programiranje onako kako se uči, gde pri modelovanju stvarnosti vidimo da su objekti direktno povezani.
 
<hr></hr>
<details>
<summary><b>Klikni da analiziraš potpuno povezan graf biblioteke</b></summary>

U implementaciji, član, pozajmica i knjiga upućuju jedni na druge direktnim referencama, a svaka klasa nosi svoje ponašanje:
 
```cs
public class Member
{
  public long Id { get; set; }
  public string Name { get; set; }
  public List<Loan> Loans { get; } = new();
 
  public bool CanBorrow() =>
    Loans.Count(loan => loan.Status == LoanStatus.Active) < 5;
}
 
public class Loan
{
  public long Id { get; set; }
  public Member Member { get; set; }
  public Book Book { get; set; }
  public DateOnly DueDate { get; set; }
  public int RenewalCount { get; set; }
  public LoanStatus Status { get; set; }
 
  public void Renew(DateOnly today)
  {
    if (Status != LoanStatus.Active)
      throw new InvalidOperationException("Samo aktivna pozajmica može biti produžena.");
    if (DueDate < today)
      throw new InvalidOperationException("Pozajmica u kašnjenju ne može biti produžena.");
    if (RenewalCount == 2)
      throw new InvalidOperationException("Pozajmica se može produžiti najviše dva puta.");
 
    DueDate = DueDate.AddDays(14);
    RenewalCount = RenewalCount + 1;
  }
}
 
public class Book
{
  public long Id { get; set; }
  public string Title { get; set; }
  public List<Loan> Loans { get; } = new();
 
  public bool IsAvailable() =>
    Loans.All(loan => loan.Status == LoanStatus.Returned);
}
```
 
Primetimo da su pravila sada pored podataka koje štite: `Renew` živi u pozajmici, dostupnost u knjizi, ograničenje broja pozajmica u članu. Primetimo i reference: član zna svoje pozajmice, pozajmica zna člana i knjigu, knjiga zna svoje pozajmice.
 
</details>
<hr></hr>

### Kada je dobar izbor

Potpuno povezan objektni graf omogućuje prirodno kretanje kroz strukturu domena. Ovakav pristup je intuitivan jer struktura softverskog modela neposredno odražava odnose između pojmova iz stvarnog sveta. Posebno je pogodan za manje aplikacije, sisteme zasnovane na jednostavnim operacijama unosa, čitanja, izmene i brisanja podataka, kao i za scenarije u kojima je potrebno prikazati veći broj povezanih informacija. Potpuni objektni grafovi mogu pojednostaviti implementaciju poslovnih operacija koje obuhvataju više povezanih objekata, a dobro se uklapaju i u način rada objektno-relacionih mapera, koji podržavaju navigaciju kroz veze, automatsko praćenje promena i kaskadno čuvanje podataka.
 
### Ograničenja
 
Iako omogućavaju jednostavnu navigaciju i intuitivno modelovanje odnosa, potpuni objektni grafovi mogu imati sledeća ograničenja:

- Nepredvidive performanse, naročito kada pristup svojstvu automatski pokreće dodatni upit prema bazi podataka. Ovde dolazi do pojave N+1 problema, kada se za svaki objekat iz početnog rezultata izvršava poseban upit za učitavanje njegovih povezanih podataka.
- Složeno upravljanje transakcijama, pošto izmena jednog dela grafa može uticati na veći broj objekata i zapisa u bazi podataka.
- Jaka sprega između delova modela, zbog čega promena strukture jednog objekta može zahtevati izmene u većem broju povezanih komponenti.
- Problemi pri serijalizaciji, kao što su ciklične reference, preduboka struktura podataka, veliki mrežni odgovori i nenamerno izlaganje podataka.

Poslednje tri tačke utiču na otežano održavanje celokupnog sistema, gde u umereno složenom softveru dobijamo graf klasa i asocijacija koji je relativno teško ispratiti.

![](https://luburic.github.io/ftn-tutor-images/images/ddd/classes.jpg)

## Taktički obrasci DDD metodologije

**Dizajn vođen domenom** (engl. _Domain-driven design_; DDD) je metodologija projektovanja softvera koja je korisna u projektima koji rešavaju probleme u složenom poslovnom domenu. Čine je dva dela:
- **Strateški obrasci** koji se bave podelom velikog sistema na poslovne oblasti i
- **Taktički obrasci** koji se bave dizajnom domenskog modela unutar jedne oblasti.

DDD taktički obrasci formiraju *bogat domenski model* koji drži strukturu *grafa isečenog na omeđene celine* koje nazivamo *agregatima*. Ubrzo ćemo se upoznati sa agregatima i prostijim taktičkim obrascima koje nazivamo *vrednosni objekat* i *entitet*.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš pozajmicu modelovanu DDD taktičkim obrascima</b></summary>

Ista pozajmica, oblikovana taktičkim obrascima, spaja ponašanje jednog i granice drugog pola:

```cs
public sealed class Loan
{
  public long Id { get; }
  public long BookId { get; }
  public long MemberId { get; }
  public DateOnly DueDate { get; private set; }
  public int RenewalCount { get; private set; }
  public LoanStatus Status { get; private set; }

  public void Renew(DateOnly today)
  {
    if (Status != LoanStatus.Active)
      throw new InvalidOperationException("Samo aktivna pozajmica može biti produžena.");
    if (DueDate < today)
      throw new InvalidOperationException("Pozajmica u kašnjenju ne može biti produžena.");
    if (RenewalCount == 2)
      throw new InvalidOperationException("Pozajmica se može produžiti najviše dva puta.");

    DueDate = DueDate.AddDays(14);
    RenewalCount = RenewalCount + 1;
  }
}
```

Primetimo tri izmene u ovoj klasi, gde svaka odgovara jednom ograničenju sa prethodnih primera:
- `set` pristupnici su skriveni i stanje se menja isključivo kroz `Renew` metodu, pa pravila više nisu konvencija nego garancija.
- Pravila stoje u istoj klasi kao i podaci, pa nema ni dupliranja ni rasipanja po servisima.
- Knjiga i član su prisutni samo kao identifikatori, pa je graf presečen i lakše je ispratiti šta se sve može promeniti kroz objekat pozajmice.

</details>
<hr></hr>

Za kraj, vredi naglasiti da DDD nije univerzalno rešenje. Domenski model koji prati DDD traži više klasa, više discipline i više razmišljanja unapred, pa je njihova cena opravdana tamo gde su pravila brojna, promenljiva i skupa kada se prekrše. Za formu nad podacima, anemičan model ostaje dobar izbor.