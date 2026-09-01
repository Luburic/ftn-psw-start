Dobra arhitektura definiše skup pravila o tome ko sme da zavisi od koga. Na primer:

- Domenski model ne sme da zna za bazu podataka.
- Kontroler ne sme direktno da pristupi bazi podataka.
- Jedan modul ne sme da poseže za unutrašnjošću drugog modula.

Deo ovih pravila čuva kompajler. Projekat koji nema referencu na biblioteku za rad sa bazom fizički ne može da je koristi. Projekat koji nema referencu na drugi projekat fizički ne može da koristi njegove tipove. Međutim, referenca između projekata se dodaje jednom linijom u `.csproj` datoteci i takvu izmenu kompajler ne brani.

Tako programer ili agent naprave male prečice koje u trenutku deluju razumno. Rok je blizu, a podatak koji vam treba se nalazi u tabeli drugog modula. "Ispravan" put kroz njegov javni interfejs deluje kao nepotrebna procedura. Prečica se napravi, funkcionalnost proradi i arhitektura se naruši. Cena stiže kasnije. Drugi tim izmeni svoju šemu baze i vaš kôd prestane da radi, a niko ne razume zašto. Sve postaje uvezano i teško je ispratiti gde se jedna funkcionalnost završava, a druga počinje. Ovaj problem jednako pogađa ljude i programerske agente, jer i jedni i drugi programiraju tako što čitaju kod koji već postoji.

**Arhitektonski test** (engl. *architecture test*) je automatski test koji pravilo o zavisnostima pretvara u tvrdnju nad kodom, tako da se test crveni kada je pravilo prekršeno.

Kada kompajler prevede C# kod, uz svaki tip se u prevedenom projektu čuvaju metapodaci o njegovim zavisnostima. Za svaku klasu se čuvaju informacije o klasi koju nasleđuje, kog tipa su polja i parametri i koje metode poziva. Biblioteka za arhitektonsko testiranje (na primer ArchUnitNET) učitava prevedene projekte, čita te metapodatke i od njih gradi graf: čvorovi su svi tipovi u sistemu, a grana postoji od tipa ka svakom tipu od kog on zavisi. Arhitektonski test proverava tvrdnje o tom grafu:

```cs
[Fact]
public void Domenski_sloj_ne_zavisi_od_infrastrukturnog()
{
  IArchRule rule = Types().That().ResideInAssembly("Ankete.Domain")
    .Should().NotDependOnAny(Types().That().ResideInAssembly("Ankete.Infrastructure"));

  rule.Check(Architecture);
}
```

U datom kodu treba uočiti sledeće.

- Metode `Types`, `That` i `ResideInAssembly` biraju skup čvorova grafa, u ovom slučaju sve tipove prevedenog projekta `Ankete.Domain`.
- Metode `Should` i `NotDependOnAny` iskazuju tvrdnju nad granama: ni od jednog izabranog tipa ne sme postojati grana ka tipu iz projekta `Ankete.Infrastructure`.
- Poziv `Check` obilazi graf i pada ako pronađe granu koja krši tvrdnju, navodeći tip koji je pravilo prekršio. Test se pokreće kao i svaki drugi automatski test, pa prekršaj pravila obara build umesto da ostane primedba na pregledu koda.

Kompajler i arhitektonski testovi dele posao. Reference između projekata čine da se većina nedozvoljenih zavisnosti ne može ni prevesti. Arhitektonski testovi hvataju slučaj kada neko izmeni `.csproj` datoteku i doda referencu koju pravila zabranjuju.

## Vrste arhitektonskih testova

Svaki arhitektonski test čuva jednu vrstu granice, pa se testovi grupišu po poreklu pravila koje čuvaju:

1. **Pravila slojeva** potiču iz [čiste arhitekture](čista-arhitektura.md) i čuvaju raspodelu odgovornosti unutar jednog modula. Primer je prethodni test, kao i pravilo da aplikacioni sloj ne vidi infrastrukturni.
2. **Granice modula** potiču iz [modularnog monolita](modularni-monolit.md) i čuvaju odnose između modula. Primer je pravilo da modul Nagrade sme da zavisi samo od kontrakta modula Ankete, kao i pravilo da zajedničko jezgro ne zavisi ni od jednog modula.
3. **Konvencije na nivou klasa** potiču iz pojedinačnih obrazaca i čuvaju pravila koja ne zavise od referenci između projekata. Primer je pravilo da upitna klasa ne sme da zavisi od jedinice posla, jer upit nikada ne čuva izmene ([Komande i upiti](komande-i-upiti.md)).

Prve dve vrste iskazuju tvrdnje nad projektima, pa se menjaju retko, uglavnom kada se uvede nov modul. Treća vrsta iskazuje tvrdnje nad imenima i članovima klasa, pa raste sa svakom konvencijom koju tim odluči da pretvori iz primedbe na pregledu koda u automatsku proveru.
