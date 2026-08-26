> **Status: lekcija.** Primeri u ovom dokumentu su pojednostavljeni radi učenja koncepta i ne prate konvencije ovog projekta. Merodavan obrazac za pisanje koda su referentni modul i normativni dokumenti.

U JavaScriptu smo koristili callback, Promise i async/await mehanizme da podržimo asinhrone operacije. Asinhrone operacije su nam veoma interesantne prilikom slanja HTTP zahteva i prihvatanja HTTP odgovora. Između ova dva događaja se dešava sledeće:
1. HTTP poruka putuje kroz električne i optičke kablove od našeg računara do servera koji se nalazi u drugoj državi, pa možda i na drugom kontinentu. U gorem slučaju ovaj put može da traje pola sekunde.
2. Server obrađuje HTTP zahtev i izvršava određenu logiku da formira HTTP odgovor. Za proste operacije se odgovor formira u fragmentu sekunde. Složene operacije mogu da uzmu i više minuta.
3. HTTP poruka putuje od servera nazad do našeg internet čitača, što opet uzima do pola sekunde.

Kada je server geografski blizu ili kad je operacija trivijalna, ovo čekanje ne osetimo. Međutim, povremeno čekamo više sekundi da se podaci dobave i prikažu. Kada bismo morali sinhrono da čekamo da se operacija završi, internet čitač bi često bio zamrznut i ne bi mogao, na primer, da iscrtava elemente i reaguje na klikove i druge događaje. Ovaj problem bi posebno bio izražen kod složenih aplikacija, gde klijentski deo veb aplikacije pravi više desetina HTTP zahteva u određenim momentima.

Iako je potreba za asinhronim operacijama jasna u kontekstu klijent-server komunikacije, interesantno je da nam isti koncept znači i u kontekstu programiranja serverskog dela veb aplikacije. Naime, možemo da definišemo asinhrone metode i u C# programskom jeziku.

## Zašto nam treba asinhrono programiranje na serveru?
Potrebu za asinhronim operacijama na strani servera je teže razumeti jer zahteva dublje razumevanje rada .NET virtualne mašine koja leži u osnovi svih C# aplikacija. U redu je ako tekst u nastavku bude izazovan za razumevanje.

Kada ASP.NET aplikacija radi, operativni sistem joj dodeljuje resurse poput memorije i pristup procesoru u skladu sa potrebama aplikacije i raspoloživim kapacitetima sistema. Aplikacija takođe dobija ograničen broj **niti**. Pojednostavljeno, **nit** (engl. *Thread*) je radna jedinica koju operativni sistem koristi da izvršava operacije programa. Primeri operacija su sabiranje dva broja, ispis na konzolu ili provera vrednosti za uslovni blok naredbi.

ASP.NET aplikacija ima na raspolaganju određenu količinu niti. Svaki put kada se napravi HTTP zahtev i aktivira kontroler, angažuje se raspoloživa nit da prihvati niz operacija i izvrši logiku aplikacije. Do sada smo pravili metode koje su sinhrone, što znači da se logika mora izvršiti u okviru iste niti. Ako postoji čekanje (npr. dok ne stigne odgovor od baze podataka), nit će biti angažovana "čekajući".

Do sada nismo osetili problem sa ovim jer nismo imali ozbiljno čekanje niti veliku količinu korisnika. Međutim, ako istovremeno imamo mnogo korisnika, može da nestane slobodnih niti, pa aplikacija postaje spora.

Uz pomoć asinhronog programiranja možemo da naglasimo šta su operacije koje će zahtevati više vremena da se izvrše i da oslobodimo nit dok sačekamo njihov rezultat.

## Kako postižemo asinhrone operacije u C#?

Programski jezik C# ima ključne reči **async** i **await**, slično kao i JavaScript. Kada definišemo asinhronu metodu (koja će imati neko čekanje), treba tri stvari da istaknemo u njenom zaglavlju:
1. Dodajemo ključnu reč **async** nakon modifikatora pristupa,
2. Dodajemo sufiks *Async* u naziv metode (nije obavezno, ali je dobra praksa) i
3. Menjamo povratnu vrednost da vraća `Task` ako je metoda bila `void`, odnosno `Task<stari tip>` (npr. `Movie` postaje `Task<Movie>`).

Interesantno je da ništa u telu metode ne moramo da menjamo. Drugim rečima, logika asinhrone metode može da ima identičan izgled kao sinhrona metoda. Međutim, za očekivati je da ćemo negde u asinhronoj metodi sačekati rezultat operacije pre nego što se kod metode izvrši do kraja. Ovo čekanje postižemo uz pomoć ključne reči **await**. U nastavku je primer jednostavne repozitorijumske metode koja je asinhrona:

```cs
public async Task<List<Movie>> GetAllAsync()
{
  Task<List<Movie>> dbTask = _context.Movies.ToListAsync();
  List<Movie> result = await dbTask;
  return result;
}
```
Osim izmene zaglavlja, interesantne su linije 3 i 4:
- U liniji 3 metoda poziva `ToListAsync` metodu nad `DbSet` svojstvom. Ova metoda generiše SQL SELECT naredbu i šalje je bazi podataka. Odgovor od baze podataka se ne čeka, a povratna vrednost `ToListAsync` je zadatak (Task) koji kada se završi će kao rezultat dati `List<Movie>` objekat. Zadatak je sličan Promise objektu.
- U liniji 4 je iskorišćena ključna reč **await**. Ovde kažemo da želimo da sačekamo da se zadatak završi, a kada se to desi ćemo njegov rezultat smestiti u promenljivu `result`.

Interesantno je da linija 4 neće aktivno čekati, odnosno neće uzurpirati nit već će metoda pauzirati sa izvršavanjem i osloboditi nit da se koristi za obradu nekog drugog zahteva (npr. od drugog korisnika). Tek kada odgovor od baze podataka stigne će .NET ispod haube da istakne da je vreme da se metoda nastavi i tada će se pronaći nova slobodna nit da izvrši ostatak operacija do kraja.

Prethodan kod možemo pojednostaviti u jednu liniju, što sledeći kod prikazuje:
```cs
public async Task<List<Movie>> GetAllAsync()
{
  return await _context.Movies.ToListAsync();
}
```

## Izvršavanje više asinhronih operacija odjednom
Ponekad ćemo pisati metode koje treba da izvrše više zahtevnih operacija pre nego što formiraju konačan rezultat. Za primer ovoga, analizirajte sledeći dijagram sekvenci:

![](https://luburic.github.io/ftn-tutor-images/images/tech/sequence%20-%20sync.png)

U dijagramu vidimo da serverska aplikacija:
1. Pravi HTTP zahtev ka drugoj serverskoj aplikaciji (eksterni servis).
   - *Primer*: Pri online kupovini, prodavnica nas može preusmeriti na sajt banke kako bismo uneli podatke o plaćanju. Ono što ne vidimo je da serverska aplikacija online prodavnice šalje HTTP zahtev aplikaciji banke kako bi se pripremila ta stranica za plaćanje.
2. Kada stigne HTTP odgovor, pravi upit ka bazi podataka.
   - *Primer*: U praksi nije retko da baza podataka stoji na odvojenom serveru koji je možda i geografski udaljen. U tom slučaju EF ispod haube pravi HTTP zahtev ka udaljenoj bazi podataka, što opet podrazumeva čekanje.
3. Kada stigne rezultat od baze podataka, aplikacija otvara datoteke sa fajl sistema.
   - *Primer*: Slično kao i baza podataka, datoteke mogu biti na udaljenom serveru. Kada nisu, ispod haube aplikacija traži od operativnog sistema da pročita sadržaj datoteke i smesti ga sa hard diska u radnu memoriju. Ovo je uglavnom brza operacija, ali kod krupnih datoteka može da bude zahtevna.
4. Kada stignu podaci iz datoteke formira se HTTP odgovor.

Radi jednostavnosti, zamislimo da svaka operacija traje 3 sekunde. Pošto imamo čekanje između svake operacije, kompletan rezultat se formira za 9 sekundi. Ako bismo primenili asinhrono programiranje, mogli bismo da izmenimo redosled izvršavanja naredbi. Sledeći dijagram sekvenci ilustruje promenu:

![](https://luburic.github.io/ftn-tutor-images/images/tech/sequence%20-%20async.png)

Iako je redosled poziva isti, ključna razlika je da se sve operacije pokrenu, bez da se čeka da stigne rezultat jedne da bi se pokrenula sledeća. Šalju se odgovarajući zahtevi ka eksternim aplikacijama, bazama podataka i sistemu datoteka. Zatim se čekaju svi odgovori, koji mogu pristići u bilo kom redosledu. Kada su svi stigli, formira se HTTP odgovor. Ako svaka operacija traje 3 sekunde i pokreću se bez međusobnog čekanja, kompletna operacija će trajati 3 sekunde (umesto 9). Ključno je da će u tih 3 sekunde samo delić sekunde nit biti zauzeta (na početku da pokrene operacije i na kraju da sabere rezultate).

## Kako izgleda kod za izvršavanje više asinhronih operacija odjednom?
Za poslednji dijagram ćemo definisati prost kod da naglasimo redosled poziva asinhronih operacija i upotrebe **await** ključne reči. Analizirajte sledeći kod:

```cs
public async Task<Result> PerformComplexOperation()
{
  Task<List<ExternalData>> externalTask = _externalConnection.GetAllAsync();
  Task<List<DbData>> dbTask = _repository.GetAllAsync();
  Task<List<FileData>> fileSystemTask = _fileSystem.GetAllAsync();

  List<ExternalData> externalResult = await externalTask;
  List<DbData> dbResult = await dbTask;
  List<FileData> fsResult = await fileSystemTask;

  // Logika za obradu tri izvora podataka koja definiše konačan 'result'
  return result;
}
```
Vidimo da:
- Linije 3, 4 i 5 pokreću različite asinhrone operacije. Ova pokretanja se izvršavaju sinhrono, gde možemo zamisliti da se prave tri zahteva (ka eksternom servisu, bazi podataka i sistemu datoteka) bez da se čeka odgovor na zahteve.
- Linija 7 predstavlja prvo čekanje. Kada program stigne do ove linije, proveriće da li je zadatak završen. Ako jeste, rezultat se smešta u promenljivu `externalResult`. Ako nije, metoda se pauzira i nit se oslobađa. Kada odgovor stigne, .NET će zauzeti novu nit da nastavi izvršavanje ove metode. Slično se zatim dešava za liniju 8 i 9.

Interesantno je da se zadaci `dbTask` i `fileSystemTask` mogu završiti pre `externalServiceTask`. Ovo nećemo suštinski osetiti. U tom slučaju će kod da, nakon što stignu rezultati u liniji 7, pređe na liniju 8, vidi da su rezultati stigli, smesti ih u promenljivu (bez čekanja i oslobađanja niti) i slično uradi za liniju 9.

Za dati kod imamo dve bitne napomene, sa kojim ćemo zaokružiti ovaj segment lekcije. 

Prvo, trenutni kod će pokrenuti sve tri operacije pre nego što dobije njihov rezultat. Ovo je poželjno ako želimo rezultate iz sva tri izvora pre nego što donesemo neku odluku. Međutim, šta se dešava ako treba spram podataka dobijenih od eksternog servisa da otkažemo dalju logiku? U tom slučaju smo bespotrebno napravili zahtev ka bazi podataka i sistemu datoteka. Tipičan primer ovog slučaja je ako eksterni servis treba da potvrdi da korisnik ima pravo da pročita neke podatke iz baze podataka.

Drugo, veoma je bitan raspored naredbi u kodu, odnosno redosled poziva asinhronih operacija i njihovo čekanje. Analizirajte sledeći kod:
```cs
public async Task<Result> PerformComplexOperation()
{
  List<ExternalData> externalResult = await _externalConnection.GetAllAsync();
  List<DbData> dbResult = await _repository.GetAllAsync();
  List<FileData> fsResult = await _fileSystem.GetAllAsync();

  // Logika za obradu tri izvora podataka koja definiše konačan 'result'
  return result;
}
```
Dati kod je naizgled unapređen u odnosu na prethodni, slično kao što smo uradili sa repozitorijumom. Međutim, ključna razlika je što kod zahteva da se jedna operacija sačeka pre nego što se pokrene sledeća. Ovakav kod će i dalje oslobađati nit, ali neće u paraleli izvršavati zadatke, te će konačan rezultat stići za 9 sekundi (ako su 3 sekunde po operaciji).

Prethodan kod nam je interesantan ako želimo da dobijemo, na primer, prvo rezultate od eksternog servisa pre nego što odlučimo da naredne operacije aktiviramo.