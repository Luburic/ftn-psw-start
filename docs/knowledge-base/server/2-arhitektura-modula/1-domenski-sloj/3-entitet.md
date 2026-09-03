## Šta su karakteristike "Entitet" obrasca?

Entitet je reč koju smo do sada koristili liberalno, pre svega pozivajući se na značenje koje nam definišu ER (*entity-relationship*) dijagrami. DDD Entitet ima određen broj karakteristika koje ga razlikuju od entiteta u ER kontekstu i deo karakteristika koje ga čine sličnim vrednosnom objektu.

### 1. Domenski koncept sa životnim ciklusom
 
**Entitet** (engl. *entity*) modeluje domenski koncept koji, za razliku od vrednosnog objekta, poseduje životni ciklus - nastaje, prolazi kroz niz stanja tokom vremena i u nekom trenutku prestaje da bude aktivan.
 
Na primer, u domenu zauzeća prostorija, rezervacija se kreira, potom potvrđuje ili otkazuje, a nakon proteka termina postaje istorijski zapis. Kroz sve te faze i dalje govorimo o istoj rezervaciji, iako su joj se svojstva u međuvremenu promenila.
 
<hr></hr>
<details>
<summary><b>Klikni da analiziraš osnovni oblik Reservation klase</b></summary>

```cs
public sealed class Reservation
{
  public long Id { get; }
  public DateRange Period { get; }
  public ReservationStatus Status { get; private set; }
 
  // Konstruktor i metode za promenu stanja
}
 
public enum ReservationStatus
{
  Pending,
  Confirmed,
  Cancelled
}
```
Rezervacija nastaje u stanju `Pending`, može preći u `Confirmed` ili `Cancelled` i ostaje isti entitet kroz ceo taj životni ciklus.
 
</details>
<hr></hr>

### 2. Nepromenljiv identifikator
 
Entitet poseduje nepromenljiv identifikator, dodeljen pri kreiranju, koji se nikada ne menja. Dva entiteta istog tipa su jednaka ako imaju jednak identifikator, bez obzira na to da li im se trenutno stanje razlikuje. Ovo je suštinska razlika u odnosu na vrednosni objekat, gde je jednakost zasnovana na vrednostima svih svojstava.
 
Na primer, rezervacija sa `Id` 482 ostaje ista rezervacija bez obzira na to da li joj se status promeni iz `Pending` u `Confirmed`.
 
<hr></hr>
<details>
<summary><b>Klikni da analiziraš jednakost Reservation klase po identifikatoru</b></summary>

U implementaciji klasa entiteta implementira `GetHashCode` i `Equals` metode iz `object` klase, gde se jednakost određuje isključivo spram identifikatora:

```cs
public sealed class Reservation
{
  public long Id { get; }
  // ... ostala svojstva
 
  public override bool Equals(object? obj)
  {
    if (obj is not Reservation other) return false;
    if (ReferenceEquals(this, other)) return true;
    return Id == other.Id;
  }
 
  public override int GetHashCode() => Id.GetHashCode();
}
```
 
Ovde namerno ne koristimo `record`, jer bi generisao jednakost po svim svojstvima, isto kao kod vrednosnog objekta, a nama treba jednakost isključivo po `Id`. U praksi ćemo često definisati `Entity` apstraktnu klasu koja ima `Id` i implementaciju `GetHashCode` i `Equals` metode.
 
</details>
<hr></hr>

### 3. Promenljiva svojstva
 
Klasa koja modeluje entitet sadrži promenljiva svojstva, koja predstavljaju njegovo stanje i menjaju se tokom životnog ciklusa entiteta.
 
Na primer, rezervacija ima `Status` koji se menja iz `Pending` u `Confirmed` ili `Cancelled`, dok joj period i identifikator ostaju fiksni od trenutka kreiranja.

### 4. Metode za kontrolisanu promenu stanja
 
Klasa koja modeluje entitet sadrži metode za kontrolisanu promenu stanja, uz očuvanje domenskih pravila koja diktiraju kakve izmene su u kom momentu dozvoljene. Ovo sprečava da entitet ikada dospe u nevalidno ili nedosledno stanje kroz nekontrolisanu direktnu izmenu svojstava.

Na primeru rezervacija možemo sagledati sledeće kontrolisane provere stanja:
1. Broj mesta na rezervaciji se može naknadno korigovati, ali nikada ispod broja već prijavljenih učesnika.
2. Rezervacija sme da bude potvrđena samo dok je na čekanju, a potvrda pored statusa beleži i datum kada je izvršena.
3. Učesnik može da se prijavi na rezervaciju ako postoji slobodno mesto i ista osoba nije već prijavljena.
 
<hr></hr>
<details>
<summary><b>Klikni da analiziraš kontrolisanu promenu stanja Reservation klase</b></summary>

U implementaciji klase entiteta, glavni mehanizam za kontrolu promene stanja predstavlja dobra enkapsulacija. Ovde treba da budemo svesni tri slučaja, koji odgovaraju zahtevima iznad:
1. Kada operacija podrazumeva izmenu jednog svojstva, provera domenskog pravila se može ugraditi u sam setter.
2. Kada je potrebno promeniti više svojstava u sklopu jedne operacije, rešenje je da se sakriju `set` pristupnici (`private`) i da se promena dešava kroz metodu koja proverava domenska pravila.
3. U slučaju kada operacija podrazumeva promenu svojstva tipa kolekcije, potrebno je sakriti i `get` i `set` pristupnik jer ugrađene kolekcije nude metode za modifikaciju svog stanja čak i kada je `private set` za njihovu referencu.

Sledeći kod proširuje `Reservation` klasu tako da ilustruje sva tri slučaja:
 
```cs
public sealed class Reservation
{
  public long Id { get; }
  public DateRange Period { get; }
  public ReservationStatus Status { get; private set; }
  public DateOnly? ConfirmedOn { get; private set; }

  private int _numberOfSeats;
  public int NumberOfSeats
  {
    get => _numberOfSeats;
    set
    {
      // Slučaj 1: operacija menja jedno svojstvo,
      // pa je domensko pravilo ugrađeno u 'set' pristupnik
      if (value < 1)
        throw new DomainException("Rezervacija mora imati bar jedno mesto.");
      if (value < _attendees.Count)
        throw new DomainException("Broj mesta ne može biti manji od broja prijavljenih učesnika.");
 
      _numberOfSeats = value;
    }
  }
 
  // Slučaj 2: jedna operacija menja više svojstava, pa su njihovi
  // 'set' pristupnici privatni, a promena se dešava kroz metodu
  public void Confirm()
  {
    if (Status != ReservationStatus.Pending)
      throw new DomainException("Potvrditi je moguće samo rezervaciju koja je na čekanju.");
 
    Status = ReservationStatus.Confirmed;
    ConfirmedOn = DateOnly.FromDateTime(DateTime.Now);
  }
 
  // Slučaj 3: kolekcija je skrivena iza read-only pogleda,
  // a izmene se dešavaju isključivo kroz metode
  private readonly List<string> _attendees = new();
  public IReadOnlyList<string> Attendees => _attendees.AsReadOnly();
 
  public void AddAttendee(string attendee)
  {
    if (Status == ReservationStatus.Cancelled)
      throw new DomainException("Nije moguće prijaviti učesnika na otkazanu rezervaciju.");
    if (_attendees.Count == NumberOfSeats)
      throw new DomainException("Sva mesta su popunjena.");
    if (_attendees.Contains(attendee))
      throw new DomainException("Učesnik je već prijavljen.");
 
    _attendees.Add(attendee);
  }
}
```
 
Primetimo da spolja ne postoji nijedan način da se `Status`, `ConfirmedOn` ili sadržaj `_attendees` kolekcije izmene direktno. Svaka promena stanja prolazi kroz tačku na kojoj se proveravaju domenska pravila, pa entitet ne može da dospe u nevalidno stanje.
 
</details>
<hr></hr>

Domenska pravila koja definišu validna stanja objekta zovemo **invarijante** (engl. *invariant*). Vrednosni objekat svoje invarijante proverava prilikom konstrukcije, a nepromenljivost garantuje da nakon toga ne mogu biti narušene. Entitet, čija se svojstva menjaju kroz životni ciklus, mora da brani invarijante pri svakoj promeni stanja. U prethodnom primeru, invarijanta rezervacije je da broj prijavljenih učesnika nikada ne prelazi broj mesta. Pravilo mora da preživi i prijavu novog učesnika i naknadno smanjenje broja mesta, pa ga proverava svaka metoda koja dira bilo koje od ta dva svojstva.

U svim dosadašnjim primerima prekršeno domensko pravilo prijavljujemo izuzetkom `DomainException`. Domenski objekat ne zna ko ga poziva niti kako se greška saopštava korisniku, pa samo odbija nedozvoljenu izmenu i opisuje razlog. Spoljašnji slojevi prepoznaju taj izuzetak po tipu i prevode ga u odgovor koji pozivalac razume, kao što middleware iz [lekcije o kontrolerima](../../1-aspnet/2-kontroleri.md) formira HTTP odgovor sa statusnim kodom 400.

### 5. Metode za izvođenje domenski značajne informacije
 
Kao i kod vrednosnog objekta, klasa koja modeluje entitet može sadržati metode koje vraćaju domenski značajne informacije izvedene iz njegovog trenutnog stanja, ne menjajući pri tome to stanje.
 
Na primer, da bi sprečio duplo zauzimanje prostorije, sistem mora da zna da li su dve rezervacije u konfliktu. Dve rezervacije su u konfliktu ako nijedna od njih nije otkazana i ako im se periodi preklapaju. Ova provera zavisi isključivo od stanja dve rezervacije, pa prirodno pripada `Reservation` klasi, koja deo posla delegira `DateRange` vrednosnom objektu.
 
<hr></hr>
<details>
<summary><b>Klikni da analiziraš izvođenje informacija u Reservation klasi</b></summary>

```cs
public sealed class Reservation
{
  // ... prethodno definisana svojstva i metode
 
  public bool ConflictsWith(Reservation other) =>
    Status != ReservationStatus.Cancelled
      && other.Status != ReservationStatus.Cancelled
      && Period.OverlapsWith(other.Period);
 
  public bool IsExpired(DateOnly today) =>
    Period.End < today;
}
```
 
Primetimo da `ConflictsWith` ne duplira logiku preklapanja perioda, već je delegira `OverlapsWith` metodi `DateRange` vrednosnog objekta. Entitet kombinuje sopstveno promenljivo stanje (status) sa informacijama koje izvode njegovi vrednosni objekti, pa svaka logika živi na mestu kome prirodno pripada.
 
</details>
<hr></hr>