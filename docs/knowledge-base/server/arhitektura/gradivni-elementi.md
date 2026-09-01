U [modularnom monolitu](modularni-monolit.md) smo videli da mali deo koda koriste svi moduli i da taj kod čini zajedničko jezgro. Ovde razmatramo šta u zajedničkom jezgru živi i čemu ono vodi tokom života projekta.

Svi moduli jedne aplikacije prate istu arhitekturu, pa se ista tehnička potreba javlja u svakom od njih. Svakom entitetu treba identifikator i poređenje po njemu. Svaki modul prijavljuje prekršeno domensko pravilo izuzetkom. Svaki modul vraća stranicu rezultata za spiskove koji rastu bez granice. Kada bi svaki tim ovo pisao za sebe, dobili bismo više različitih rešenja istog problema. Čitalac bi morao da nauči rešenje svakog tima, a razlike među rešenjima ne nose nikakvu vrednost.

**Gradivni elementi** (engl. *building blocks*) su klase zajedničkog jezgra koje rešavaju tehničku potrebu zajedničku svim modulima, tako da je svi moduli rešavaju na isti način.

Za domenski sloj, gradivni elementi su bazne klase domenskih objekata i zajednički izuzeci:

```cs
public abstract class Entity
{
  public long Id { get; }
  // Poređenje dva entiteta po identifikatoru
}

public class DomainException : Exception
{
  public DomainException(string message) : base(message) { }
}
```

Modul Ankete tada piše `public sealed class Survey : Entity`, a prekršeno domensko pravilo prijavljuje izuzetkom `DomainException`. Modul Nagrade radi isto. Pravila poređenja entiteta i vrsta izuzetka su napisani jednom i važe za celu aplikaciju.

Druga grupa gradivnih elemenata rešava tehničke potrebe koje prožimaju sve module. Primeri su:

- pomoćni kod kroz koji svaki modul povezuje svoje klase sa bazom podataka,
- middleware koji domenske izuzetke svih modula prevodi u HTTP odgovore i
- pomoćni kod za pokretanje aplikacije u automatskim testovima.

Ovi elementi ne pripadaju nijednom modulu, jer bi tada svi moduli zavisili od jednog tima. Zato žive u zajedničkom jezgru, a svaki modul ih koristi.

Vremenom zajedničko jezgro prerasta u **platformski radni okvir** (engl. *platform framework*): nadskup opšteg radnog okvira i biblioteka koje aplikacija koristi, proširen klasama specifičnim za tu platformu. Tim modula tada programira nad platformskim radnim okvirom na isti način na koji programira nad ASP.NET-om, gde koristi njegove klase, a ne menja ih. Gradivni element se ne piše unapred, već nastaje promocijom: kada se isto rešenje zatraži u drugom modulu, tim koji poseduje zajedničko jezgro ga izdvaja iz modula i uvodi kao gradivni element. Dodavanje u zajedničko jezgro time ostaje odluka koja se donosi za ceo sistem.
