Dobra arhitektura definiše skup pravila o tome ko sme da zavisi od koga. Na primer:

- Domenski model ne sme da zna za bazu podataka.
- Kontroler ne sme direktno da pristupi bazi podataka.
- Jedan modul ne sme da poseže za unutrašnjošću drugog modula.

Deo ovih pravila čuva kompajler. Projekat koji nema referencu na biblioteku za rad sa bazom ne može da je koristi, a projekat koji nema referencu na drugi projekat ne može da koristi njegove tipove. Međutim, referenca između projekata se dodaje jednom linijom u `.csproj` datoteci i takvu izmenu kompajler ne brani.

Prečica kroz tuđu unutrašnjost u trenutku deluje razumno. Rok je blizu, podatak koji treba nalazi se u tabeli drugog modula, a put kroz njegov kontrakt zahteva dogovor sa drugim timom. Prečica se napravi i funkcionalnost proradi, a cena stiže kasnije, kada drugi tim izmeni svoju šemu baze i kod prestane da radi. Nakon nekoliko takvih prečica više nije moguće ispratiti gde se jedan modul završava, a drugi počinje. Ovaj problem jednako pogađa ljude i programerske agente, jer i jedni i drugi programiraju tako što čitaju kod koji već postoji, uključujući i prečice.

## Arhitektonski test

**Arhitektonski test** (engl. *architecture test*) je automatski test koji pravilo o zavisnostima pretvara u tvrdnju nad kodom, tako da se test crveni kada je pravilo prekršeno.

Kada kompajler prevede C# kod, uz svaki tip u prevedenom projektu čuva metapodatke o njegovim zavisnostima, kao što su klasa koju nasleđuje, tipovi polja i parametara i metode koje poziva. Biblioteka za arhitektonsko testiranje, u našem projektu ArchUnitNET, učitava prevedene projekte, čita te metapodatke i od njih gradi graf u kom su čvorovi svi tipovi u sistemu, a grana postoji od tipa ka svakom tipu od kog on zavisi. Arhitektonski test proverava tvrdnju nad tim grafom:

```cs
[Fact]
public void Domenski_sloj_ne_zavisi_od_infrastrukturnog()
{
  Types().That().ResideInAssembly("Ankete.Domain")
    .Should().NotDependOnAny(Types().That().ResideInAssembly("Ankete.Infrastructure"))
    .Check(Architecture);
}
```

U datom kodu treba uočiti sledeće:

- Polje `Architecture` je graf koji test klasa gradi jednom, učitavanjem prevedenih projekata svih modula i zajedničkog jezgra, i deli ga sa svim testovima.
- Metode `Types`, `That` i `ResideInAssembly` biraju skup čvorova grafa, u ovom slučaju sve tipove prevedenog projekta `Ankete.Domain`.
- Metode `Should` i `NotDependOnAny` iskazuju tvrdnju nad granama, da ni od jednog izabranog tipa ne postoji grana ka tipu iz projekta `Ankete.Infrastructure`.
- Poziv `Check` obilazi graf i pada ako pronađe granu koja krši tvrdnju, navodeći tip koji je pravilo prekršio. Test se pokreće kao i svaki drugi automatski test, pa prekršaj pravila obara build umesto da ostane primedba na pregledu koda.

Kompajler i arhitektonski testovi dele posao. Reference između projekata čine da se većina nedozvoljenih zavisnosti ne može ni prevesti, a arhitektonski testovi hvataju slučaj u kom neko doda referencu koju pravila zabranjuju.

## Vrste arhitektonskih testova

Svaki arhitektonski test čuva jednu vrstu granice, pa se testovi grupišu po poreklu pravila koje čuvaju:

1. **Pravila slojeva** potiču iz čiste arhitekture i čuvaju raspodelu odgovornosti unutar jednog modula. Primer je prethodni test, kao i pravila da API sloj ne koristi tipove domenskog sloja, iako ih kroz referencu na aplikacioni sloj vidi, i da domenski i aplikacioni sloj ne koriste EFC ni ASP.NET biblioteke. U našem projektu ih čuva klasa `ModuleLayerTests`.
2. **Granice modula** potiču iz modularnog monolita i čuvaju odnose između modula, zajedničkog jezgra i glavne aplikacije. Primeri su pravila da modul sme da zavisi samo od projekta `Contracts` drugog modula, da nijedan projekat zajedničkog jezgra ne zavisi ni od jednog modula i da glavna aplikacija modul dodiruje samo kroz njegove metode proširenja. U našem projektu ih čuvaju klase `ModuleIsolationTests`, `SharedKernelTests` i `HostCompositionTests`.
3. **Konvencije na nivou klasa** potiču iz pojedinačnih obrazaca i čuvaju pravila koja ne zavise od referenci između projekata, već od naziva i članova klasa. U našem projektu ih čuva klasa `ApplicationConventionTests`.

Testovi treće vrste biraju skup čvorova po nazivu klase umesto po projektu. Sledeći test iskazuje pravilo da upitna klasa ne sme da zavisi od jedinice posla, jer upit nikada ne čuva izmene:

```cs
[Fact]
public void Upitna_klasa_ne_zavisi_od_jedinice_posla()
{
  Classes().That().HaveNameEndingWith("Queries")
    .Should().NotDependOnAnyTypesThat().HaveName("IUnitOfWork")
    .Check(Architecture);
}
```

U datom kodu treba uočiti sledeće:

- Metoda `HaveNameEndingWith` bira sve klase čiji se naziv završava na `Queries`, iz svih projekata u grafu. Konvencija imenovanja upitnih klasa je time postala uslov koji test ume da proveri.
- Metode `NotDependOnAnyTypesThat` i `HaveName` zabranjuju granu ka bilo kom tipu naziva `IUnitOfWork`, bez obzira na to u kom projektu taj tip živi, pa jedan test važi za sve module.

Prve dve vrste iskazuju tvrdnje nad projektima, pa se menjaju retko, uglavnom kada se uvede nov modul. Treća vrsta iskazuje tvrdnje nad nazivima i članovima klasa, pa raste sa svakom konvencijom koju tim odluči da pretvori iz primedbe na pregledu koda u automatsku proveru.
