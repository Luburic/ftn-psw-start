Svi feature moduli prate istu arhitekturu, pa se ista tehnička potreba javlja u svakom od njih. Svakom entitetu treba identifikator. Svaki modul prijavljuje prekršeno domensko pravilo izuzetkom. Svaki modul vraća stranicu rezultata za spiskove sa mnogo stavki. Kada bi svaki tim ovo pisao za sebe, dobili bismo više različitih rešenja istog problema, a čitalac bi morao da nauči rešenje svakog tima. Ovde razmatramo gde kod koji je zajednički za sve module živi.

## Zajedničko jezgro

**Zajedničko jezgro** (engl. *shared kernel*) je deo koda koji koriste svi moduli. U našem projektu ga čine projekti `Shared.Domain`, `Shared.Api`, `Shared.Infrastructure` i `Shared.Tests`, po jedan za svaki sloj modula koji ima zajednički kod. Svaki sloj modula referencira samo projekat jezgra svog sloja, pa domenski sloj vidi `Shared.Domain`, a ne vidi `Shared.Infrastructure`.

Zajedničko jezgro se drži namerno malim, jer svaka njegova promena pogađa sve module. Kada bi zajednički kod pripadao jednom modulu, svi moduli bi zavisili od jednog tima, a svaka promena tog modula bi mogla da lomi ostale.

## Gradivni elementi

**Gradivni elementi** (engl. *building blocks*) su klase zajedničkog jezgra koje rešavaju tehničku potrebu zajedničku svim modulima, tako da je svi moduli rešavaju na isti način.

Za domenski sloj su gradivni elementi bazne klase domenskih objekata i zajednički izuzeci:

```cs
public abstract class Entity
{
  public Guid Id { get; protected set; }

  protected Entity() { }

  protected Entity(Guid id)
  {
    Id = id;
  }

  public override bool Equals(object? obj)
  {
    if (obj is not Entity other || GetType() != other.GetType())
    {
      return false;
    }

    return ReferenceEquals(this, other) || Id == other.Id;
  }

  public override int GetHashCode() => Id.GetHashCode();
}

public abstract class AggregateRoot : Entity
{
  protected AggregateRoot() { }

  protected AggregateRoot(Guid id) : base(id) { }
}

public class DomainException : Exception
{
  public DomainException(string message) : base(message) { }
}
```

U datom kodu treba uočiti sledeće:

- Klasa `Entity` daje identifikator svakom entitetu. Pristupnik `set` je zaštićen, pa identifikator dodeljuje samo konstruktor naslednika, a konstruktor bez parametara postoji radi rehidracije.
- Metode `Equals` i `GetHashCode` određuju jednakost isključivo po identifikatoru, uz uslov da su oba objekta istog tipa, jer pitanje i anketa sa istim identifikatorom nisu isti entitet. Nijedan entitet u modulima ne piše sopstvenu jednakost.
- Klasa `AggregateRoot` ne dodaje nijedan član, već postoji da bi koren agregata bio prepoznatljiv po tipu. Modul Ankete piše `public sealed class Survey : AggregateRoot` za koren, a `public sealed class Question : Entity` za unutrašnji entitet.
- Izuzetak `DomainException` prijavljuje prekršeno domensko pravilo, a `NotFoundException`, istog oblika, agregat koji ne postoji. Middleware glavne aplikacije ih prevodi u odgovore sa statusnim kodovima 400 i 404 za sve module odjednom, što je moguće jer su oba tipa u zajedničkom jezgru.

Ostali slojevi imaju manje zajedničkog koda, ali istog oblika:

- `Shared.Domain` uz navedene klase sadrži `PageResult<T>`, strukturu koja nosi jednu stranicu rezultata i ukupan broj redova.
- `Shared.Infrastructure` sadrži metodu proširenja `AddModuleDbContext`, kojom svaki modul registruje svoj kontekst sa konekcionim stringom i sopstvenom šemom baze.
- `Shared.Api` sadrži metodu proširenja `GetUserId`, kojom akcija iz identiteta korisnika izdvaja njegov identifikator.
- `Shared.Tests` sadrži pomoćni kod kojim integracioni testovi svakog modula pokreću aplikaciju nad testnom bazom.

## Platformski radni okvir

U praksi zajedničko jezgro vremenom prerasta u **platformski radni okvir** (engl. *platform framework*), nadskup radnog okvira i biblioteka koje aplikacija koristi, proširen klasama specifičnim za tu aplikaciju. Programeri koji rade nad feature modulima koriste platformski radni okvir isto kao što koriste ASP.NET, čime dobijaju moćne funkcionalnosti koje je platforma sakupila kroz godine.

Gradivni element se ne piše unapred, već nastaje **promocijom**. Kada se isto rešenje zatraži u drugom modulu, platformski tim ga izdvaja iz modula u kom je nastalo i uvodi u zajedničko jezgro. Dodavanje u zajedničko jezgro time ostaje odluka koja se donosi za ceo sistem, a ne pogodnost jednog modula.
