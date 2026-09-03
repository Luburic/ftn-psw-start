> **Tip: lekcija.** Primeri u dokumentu su pojednostavljeni radi učenja koncepta i ne prate konvencije projekta do kraja.

ASP.NET Core uvodi koncept kontrolera kao klasu čije javne metode, koje zovemo akcije, obrađuju HTTP zahteve. Radni okvir kroz rutiranje bira akciju, automatski popunjava njene parametre i pretvara njenu povratnu vrednost u HTTP odgovor. Ovde detaljnije razmatramo pravila po kojima se svaki od ovih koraka dešava. Na kraju uvodimo middleware, mehanizam u kom možemo da centralizujemo obradu koja je zajednička za sve akcije.

## Rutiranje

Adresa akcije nastaje spajanjem vrednosti atributa `[Route]` na klasi i vrednosti atributa HTTP metode na akciji. Kada stigne zahtev, radni okvir bira akciju kod koje se poklapaju i adresa i HTTP metoda. Deo adrese u vitičastim zagradama, na primer `{id}`, je promenljiv i poklapa se sa bilo kojom vrednošću na tom mestu.

Sledeći kod prikazuje kontroler sa više akcija, gde su tela akcija izostavljena:

```cs
[ApiController]
[Route("api/books")]
public class BookController : ControllerBase
{
  [HttpGet]
  public ActionResult<List<Book>> GetAll() { ... }

  [HttpGet("{id}")]
  public ActionResult<Book> GetById(int id) { ... }

  [HttpPost]
  public ActionResult<Book> Create(Book book) { ... }

  [HttpPut("{id}")]
  public ActionResult<Book> Update(int id, Book book) { ... }
}
```

U datom kodu treba uočiti sledeće:
- Zahtev `GET /api/books` poziva akciju `GetAll`, a zahtev `GET /api/books/5` poziva akciju `GetById`. Obe akcije imaju istu HTTP metodu, a razlikuju se po obliku adrese, jer prva nema dodatan deo adrese, a druga ima promenljiv deo `{id}`.
- Zahtev `PUT /api/books/5` poziva akciju `Update`. Akcije `GetById` i `Update` obrađuju istu adresu, a razlikuju se po HTTP metodi.
- Kada nijedna akcija ne odgovara zahtevu, radni okvir vraća odgovor sa statusnim kodom 404, bez poziva bilo koje akcije.

## Vezivanje modela

**Vezivanje modela** (engl. *model binding*) je postupak kojim radni okvir popunjava parametre akcije podacima iz HTTP zahteva. Podaci mogu doći iz tri izvora:
1. `[FromRoute]` - Iz parametra putanje, odnosno promenljivog dela adrese. Ovaj izvor tipično nosi identifikator podatka sa kojim se radi.
2. `[FromQuery]` - Iz parametara upita, odnosno dela adrese posle znaka `?`. Ovaj izvor tipično nosi opcione podatke, poput kriterijuma pretrage ili rednog broja stranice.
3. `[FromBody]` - Iz tela zahteva. Ovaj izvor nosi složene podatke u JSON zapisu, koje radni okvir pretvara u objekat zadate klase. Telo zahteva postoji kod HTTP metoda POST i PUT.

Sledeći kod prikazuje dve akcije koje koriste sva tri izvora:

```cs
[HttpGet("search")]
public ActionResult<List<Book>> Search([FromQuery] string author, [FromQuery] int page)
{
  return Ok(_bookService.Search(author, page));
}

[HttpPut("{id}")]
public ActionResult<Book> Update([FromRoute] int id, [FromBody] Book book)
{
  return Ok(_bookService.Update(id, book));
}
```

U datom kodu treba uočiti sledeće:
- Zahtev `GET /api/books/search?author=Andric&page=2` poziva akciju `Search`, gde parametar `author` dobija vrednost `Andric`, a parametar `page` vrednost `2`.
- Zahtev `PUT /api/books/5` poziva akciju `Update`, gde parametar `id` dobija vrednost `5` iz adrese, a parametar `book` nastaje pretvaranjem JSON zapisa iz tela zahteva u objekat klase `Book`.
- Imena parametara akcije se poklapaju sa imenom promenljivog dela adrese, odnosno sa imenima upitnih parametara. Po tom imenu radni okvir zna koju vrednost gde smešta.

Atributi se mogu i izostaviti. Tada radni okvir primenjuje podrazumevana pravila:
- Parametar prostog tipa se vezuje za parametar putanje istog imena ako postoji, a inače za parametar upita istog imena.
- Parametar složenog tipa, odnosno klase, se vezuje za telo zahteva.

Preporuka je da se koriste izričit atribut jer čine izvor vidljivim čitaocu koda.

## Povratna vrednost akcije

Akcija vraća vrednost tipa `ActionResult<T>` i radni okvir objekat pretvara u JSON i šalje odgovor sa statusnim kodom 200. Ovde dodajemo dva detalja:
1. Akcija može da vrati objekat direktno, naredbom `return book;`, ili kroz poziv `return Ok(book);`. Oba oblika daju isti odgovor, sa statusnim kodom 200 i objektom u JSON zapisu.
2. Kada akcija treba da vrati grešku, poziva metodu `Problem` i navodi statusni kod i poruku:

```cs
return Problem(statusCode: StatusCodes.Status404NotFound, title: "Book does not exist.");
```

Telo ovakvog odgovora prati standardan JSON oblik za opis greške, koji sadrži statusni kod i navedenu poruku. Klijent tako iz svakog neuspešnog odgovora čita grešku na isti način.

## Middleware

Analizirajmo akciju `GetById`, gde metoda `BookService` baca izuzetak kada knjiga ne postoji. Akcija hvata izuzetak i prevodi ga u HTTP odgovor:

```cs
[HttpGet("{id}")]
public ActionResult<Book> GetById(int id)
{
  try
  {
    return _bookService.GetById(id);
  }
  catch (NotFoundException exception)
  {
    return Problem(statusCode: StatusCodes.Status404NotFound, title: exception.Message);
  }
}
```

Možemo zamisliti da će svaka akcija koja traži podatke od aplikacije imati identičan try/catch blok. Dalje, svaka nova vrsta izuzetka dodaje novi `catch` blok u svaku akciju, pa bi kontroler sa pet akcija imao više koda za obradu grešaka nego za svoj osnovni posao. Ponavljanje izbegavamo tako što obradu grešaka izdvojimo na jedno mesto kroz koje prolazi svaki zahtev, za šta nam treba middleware. **Middleware** je komponenta koja obrađuje svaki HTTP zahtev pre nego što stigne do akcije i svaki HTTP odgovor pre nego što se vrati klijentu.

Middleware komponente se registruju radnom okviru (u datoteci `Program.cs`) i nadovezuju se u lanac, redosledom registracije. Kada zahtev stigne na aplikaciju, dešava se sledeće:
1. Svaka middleware komponenta primi zahtev, izvrši svoj posao i prosledi zahtev sledećoj komponenti u lancu,
2. Na kraju lanca se izvršava akcija i logika naše aplikacije,
3. Kada akcija završi, odgovor se kroz isti lanac vraća nazad, pa se kod komponente napisan posle prosleđivanja izvršava nakon akcije.

Sledeći kod prikazuje middleware komponentu koju mi definišemo da izuzetke određenog tipa pretvorimo u odgovarajuće HTTP odgovore:

```cs
public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;

  public ExceptionHandlingMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (NotFoundException exception)
    {
      await Results.Problem(statusCode: StatusCodes.Status404NotFound, title: exception.Message)
        .ExecuteAsync(context);
    }
    catch (Exception)
    {
      await Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
        .ExecuteAsync(context);
    }
  }
}
```

U datom kodu treba uočiti sledeće:
- Radni okvir poziva metodu `InvokeAsync` za svaki HTTP zahtev. Parametar `context` sadrži informaciju o HTTP zahtevu i odgovoru u obradi.
- Polje `_next` predstavlja ostatak lanca, uključujući akciju. Poziv `_next(context)` prosleđuje zahtev dalje, a naredni red koda se izvršava tek kada je akcija završila.
- Blok `try` obuhvata poziv ostatka lanca, pa `catch` hvata izuzetak bačen iz bilo koje akcije. Jedan `catch` blok tako zamenjuje try/catch blokove u svim akcijama.
- Poziv `Results.Problem` upisuje u odgovor istu vrstu greške koju bi akcija napravila metodom `Problem`.
- Poslednji `catch` blok hvata svaki drugi izuzetak, na primer pad konekcije ka bazi, i vraća odgovor sa statusnim kodom 500. Takav odgovor ne nosi poruku jer korisniku ne pomaže; on je znak programerima da u aplikaciji postoji greška koju treba istražiti.

Da bi se prethodni kod izvršio kroz lanac middleware komponenti, neophodno je registrovati klasu u datoteci `Program.cs`, između pravljenja i pokretanja aplikacije:

```cs
var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
```

Kada middleware preuzme obradu grešaka, akcija se svodi na poziv logike aplikacije:

```cs
[HttpGet("{id}")]
public ActionResult<Book> GetById(int id)
{
    return _bookService.GetById(id);
}
```

Za kraj, vredi istaći da i sam radni okvir koristi middleware komponente. Rutiranje je ugrađeni middleware koji na osnovu adrese i HTTP metode bira akciju, a middleware obavlja i proveru identiteta korisnika. Sledeća slika prikazuje lanac middleware-a, gde su poslednja dva ugrađena:

```
          ┌─────────────┐   ┌─────────┐   ┌────────────────┐   ┌─────────┐
Zahtev ──>│ ErrorHandl. │──>│ Logging │──>│ Authentication │──>│ Routing │──> Akcija
          └─────────────┘   └─────────┘   └────────────────┘   └─────────┘
```

Redosled izvršavanja ovog lanca kada pristigne zahtev je:
1. `ErrorHandling`, koji u ovom smeru samo prosledi zahtev dalje,
2. `Logging`, gde se zahtev evidentira u log datoteku,
3. `Authentication`, koji proverava identitet korisnika (npr. kroz validaciju JWT-a)
4. `Routing`, koji vrši izbor akcije
5. Izvršavanje akcije
6. Vraćanje `Routing`, koji u ovom smeru ništa ne radi
7. Vraćanje `Authentication`, koji u ovom smeru ništa ne radi
8. Vraćanje `Logging`, koji u ovom smeru može da evidentira odgovor u log datoteku
9. Vraćanje `ErrorHandling` middleware-u, koji ništa ne radi ako je sve prošlo uredno. Ako je neki deo aplikacije bacio izuzetak, on se propagira do mesta njegove obrade, što je ovaj middleware. Tada se formira HTTP odgovor spram izuzetka.