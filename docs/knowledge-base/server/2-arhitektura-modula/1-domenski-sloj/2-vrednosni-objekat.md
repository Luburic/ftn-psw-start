## Šta su karakteristike "Vrednosni objekat" obrasca?

### 1. Domenski značajna vrednost

**Vrednosni objekat** (engl. *value object*) modeluje domenski značajnu vrednost i realizuje se kroz jednu klasu sa jednim ili više svojstava.

Na primer, domen isporuke pošiljki definiše adresu kao domenski značajnu vrednost. Iako se sastoji od nekoliko pojedinačnih podataka (ulice, grada i poštanskog broja), ti podaci zajedno predstavljaju jedan smislen domenski koncept, a to je *mesto na koje se pošiljka isporučuje*. Adresa nije samo skup nezavisnih stringova, već konceptualna celina koja ima smisla jedino posmatrana kao jedinstvena vrednost.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš kod Address klase</b></summary>

U implementaciji, `Address` klasa sabira nekoliko svojstava:
```cs
public sealed class Address
{
  public string Street { get; }
  public string City { get; }
  public string PostalCode { get; }

  public Address(string street, string city, string postalCode)
  {
    Street = street;
    City = city;
    PostalCode = postalCode;
    Validate();
  }

  private void Validate()
  {
    if (string.IsNullOrWhiteSpace(Street))
      throw new DomainException("Ulica je obavezna.");
    if (string.IsNullOrWhiteSpace(City))
      throw new DomainException("Grad je obavezan.");
    if (string.IsNullOrWhiteSpace(PostalCode) || PostalCode.Length != 5)
      throw new DomainException("Poštanski broj mora imati 5 cifara.");
  }

  public override string ToString() => $"{Street}, {PostalCode} {City}";
}
```
Prekršeno pravilo metoda `Validate` prijavljuje izuzetkom **DomainException**, klasom koju definišemo u domenskom sloju kao naslednika klase `Exception` i koristimo za svako prekršeno domensko pravilo. Po tipu ovog izuzetka spoljašnji slojevi razlikuju neispravan zahtev od greške u programu.

Ovakvu klasu bismo onda mogli da iskoristimo kao svojstvo druge klase, u kodu poput sledećeg:
```cs
public class Shipment
{
  public Guid Id { get; private set; }
  public Address DeliveryAddress { get; private set; }
  // Ostala svojstva i metode
}
```
</details>
<hr></hr>

### 2. Identitet određen vrednostima svojstava
Za vrednosne objekte ne postoji poseban identifikator, već su dva vrednosna objekta istog tipa jednaka ako su im jednake vrednosti svih svojstava.

Na primer, domen prodavnice definiše novac kao domenski značajnu vrednost, gde će mušterija prilikom kupovine proizvoda da preda određenu količinu novčanica određene valute. Prodavcu i prodavnici nije bitno da li će mušterija da izvuče prvih 200 dinara iz novčanika ili drugih, dok god novčanica nije previše oštećena. Iako su u pitanju dva fizički različita objekta, prodavnica ih tretira identično.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš kod Money klase i 'record' C# konstrukt</b></summary>

U implementaciji klasa vrednosnog objekta implementira `GetHashCode` i `Equals` metode iz `object` klase, gde redom navodi svojstva koja određuju njen identitet.

```cs
public sealed class Money
{
  public decimal Amount { get; }
  public Currency Currency { get; }

  public Money(decimal amount, Currency currency)
  {
    Amount = amount;
    Currency = currency;
  }

  public override bool Equals(object? obj)
  {
    if (obj is not Money other) return false;
    if (ReferenceEquals(this, other)) return true;
    return Amount == other.Amount && Currency == other.Currency;
  }

  public override int GetHashCode() => HashCode.Combine(Amount, Currency);

  public static bool operator ==(Money? left, Money? right) => Equals(left, right);
  public static bool operator !=(Money? left, Money? right) => !Equals(left, right);
}
```
U datom kodu vidimo i da su operatori `==` i `!=` preklopljeni. Ovo nije obavezno, ali je korisno jer omogućava prirodno poređenje (money1 == money2) umesto obaveznog pozivanja `.Equals()`.

C# 9 je uveo **record** strukturni tip podatka, namenjen za modelovanje podataka čiji je identitet određen vrednostima svojstava. `record` je deklaracija tipa koja proširuje `class` sintetički generisanim članovima za strukturnu jednakost. Za dati `record` tip, kompajler automatski generiše `Equals`, `GetHashCode`, `==` i `!=`. 

Uz pomoć `record`, prethodni kod je zamenjen sa sledećim:
```cs
public sealed record Money
{
  public decimal Amount { get; }
  public Currency Currency { get; }

  public Money(decimal amount, Currency currency)
  {
    Amount = amount;
    Currency = currency;
  }
}
```

</details>
<hr></hr>

### 3. Nepromenljivost nakon kreiranja

Vrednosni objekat je **nepromenljiv** (engl. *immutable*), što znači da se vrednosti njegovih svojstava ne menjaju nakon konstrukcije. Nepromenljivost dodatno čini vrednosne objekte bezbednim za deljenje između više entiteta ili niti izvršavanja, jer ne postoji rizik da neko izmeni stanje objekta "ispod nogu" drugom delu koda koji drži referencu na isti objekat.

U primeru `Address` smo videli da nepromenljivost postižemo za `Street`, `City` i `PostalCode` tako što navodimo samo `get` pristupnik, dok `set` ne postoji. Vrednosti se dodeljuju isključivo kroz konstruktor, jednom i nakon toga se smatraju fiksnim.

### 4. Validacija pri kreiranju

Pri kreiranju vrednosnog objekta se primenjuju validaciona pravila koja osiguravaju da je objekat u ispravnom stanju nakon izvršavanja konstruktora. Time je zagarantovano da svaka referenca na vrednosni objekat koja postoji bilo gde u sistemu predstavlja ispravnu, konzistentnu vrednost.

Na primer, domen evidencije građana Srbije definiše JMBG kao domensku vrednost sa strogim pravilima: mora imati tačno 13 cifara, sadržati ispravan datum rođenja i proći proveru kontrolne cifre (checksum). Ako bilo koje od ovih pravila nije zadovoljeno, ne postoji ispravan način da `Jmbg` objekat uopšte bude kreiran.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš kod Jmbg klase</b></summary>

Primere validacije smo već videli za `Address` klasu, a sledeći kod daje dodatan primer gde se validacija poziva na kraju konstrukcije objekta:

```cs
public sealed record Jmbg
{
  public string Value { get; }

  public Jmbg(string value)
  {
    Value = value;
    Validate();
  }

  private void Validate()
  {
    if (string.IsNullOrWhiteSpace(Value))
      throw new DomainException("JMBG je obavezan.");
    if (Value.Length != 13 || !Value.All(char.IsDigit))
      throw new DomainException("JMBG mora imati 13 cifara.");
    if (!IsDateValid())
      throw new DomainException("JMBG ima neispravan datum.");
    if (!IsChecksumValid())
      throw new DomainException("Neispravna kontrolna cifra.");
  }

  private bool IsDateValid()
  {
    var d = int.Parse(Value[..2]);
    var m = int.Parse(Value[2..4]);
    var yyy = int.Parse(Value[4..7]);
    var y = yyy >= 900 ? 1000 + yyy : 2000 + yyy;
    return DateTime.TryParse($"{y:D4}-{m:D2}-{d:D2}", out _);
  }

  private bool IsChecksumValid()
  {
    // Sračunaj checksum
    // Vrati poređenje sračunatog sa poslednjim karakterom
  }

  public override string ToString() => Value;
}
```

Primetimo da su ulančana četiri nezavisna pravila (format, dužina, datum, kontrolna cifra) i sva moraju proći pre nego što `Jmbg` uopšte počne da postoji kao objekat. Bitno je da validacija bude deo konstruktora, a ne posebna metoda koja se poziva "posle" kreiranja objekta jer bismo tako dozvolili da neispravan objekat postoji u memoriji.

</details>
<hr></hr>

### 5. Metode za izvođenje domenski značajne informacije

Klasa koja modeluje vrednosni objekat može sadržati metode koje vraćaju domenski značajne informacije izvedene iz njegovog stanja.

Na primer, u domenu evidencije građana, JMBG u sebi kodira datum rođenja i pol osobe. Umesto da `Person` klasa ili bilo koji drugi deo koda sama parsira string JMBG-a kad god joj zatreba datum rođenja, ta logika prirodno pripada `Jmbg` klasi, jer zavisi isključivo od njenog internog stanja.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš prošireni kod Jmbg klase</b></summary>

```cs
public sealed record Jmbg
{
  // ... prethodno definisana svojstva i metode

  public DateOnly GetBirthDate()
  {
    var d = int.Parse(Value[..2]);
    var m = int.Parse(Value[2..4]);
    var yyy = int.Parse(Value[4..7]);
    var y = yyy >= 900 ? 1000 + yyy : 2000 + yyy;
    return new DateOnly(y, m, d);
  }

  public bool IsMale()
  {
    var bbb = int.Parse(Value[9..12]);
    return bbb < 500;
  }
}
```

</details>
<hr></hr>

Dodatan primer pronalazimo u domenu upravljanja zauzećem prostorija, gde je svaki termin predstavljen periodom od datuma početka do datuma završetka. Da bismo sprečili duplo zakazivanje, sistem mora da zna da li se dva perioda preklapaju (`OverlapsWith`), a da bismo proverili da li jedan slobodan termin u potpunosti pokriva traženi period, potrebno je da znamo da li jedan period sadrži drugi (`Includes`). Obe provere zavise isključivo od granica dva perioda, pa prirodno pripadaju `DateRange` klasi.

<hr></hr>
<details>
<summary><b>Klikni da analiziraš kod DateRange klase</b></summary>

`DateRange` je validan ako njegov početak dolazi pre njegovog završetka, što ćemo ugraditi kao validaciono pravilo u konstruktoru. Uz to vidimo primer dve *query* metode u nastavku:

```cs
public sealed record DateRange
{
  public DateOnly Start { get; }
  public DateOnly End { get; }

  public DateRange(DateOnly start, DateOnly end)
  {
    if (end < start)
      throw new DomainException("Datum završetka ne sme biti pre datuma početka.");

    Start = start;
    End = end;
  }

  public bool OverlapsWith(DateRange other) =>
    Start <= other.End && other.Start <= End;

  public bool Includes(DateRange other) =>
    Start <= other.Start && End >= other.End;
}
```

</details>
<hr></hr>