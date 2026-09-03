U klijent-server arhitekturi, često koristimo HTTP za komunikaciju između ove dve strane. Tako klijent šalje HTTP zahtev, a server obrađuje zahtev, pokreće određenu logiku spram njega i formira HTTP odgovor koji klijent može da obradi.

Razmotrimo posao koji stoji iza jednog zahteva poput `GET /api/books/5`. Serverska aplikacija mora da:
1. Sluša na mrežnom portu i prihvati dolaznu konekciju.
2. Pročita sirov tekst HTTP zahteva i iz njega izdvoji HTTP metodu, adresu, zaglavlja i telo.
3. Odluči koji deo koda obrađuje baš tu adresu i tu metodu.
4. Pretvori podatke iz zahteva u objekte programskog jezika, na primer telo zahteva iz JSON zapisa u C# objekat.
5. Izvrši logiku aplikacije.
6. Pretvori rezultat u JSON zapis, doda statusni kod i zaglavlja i pošalje ispravno formiran HTTP odgovor.

Logika aplikacije je samo jedan korak od šest. Kada bismo sve ostale korake pisali ručno, taj kod bi višestruko premašio poslovnu logiku, a bio bi isti u svakoj serverskoj aplikaciji na svetu. Zato taj posao preuzima radni okvir.

## ASP.NET Core

**ASP.NET Core** je radni okvir (engl. *framework*) za izgradnju serverskih veb aplikacija na .NET platformi, sličan Spring-u u Java svetu. Radni okvir preuzima tehničke korake obrade HTTP zahteva, a programeru ostavlja da napiše logiku aplikacije i da je označi tako da je radni okvir pronađe i pozove. Za nas se automatski rešavaju koraci 1, 2, 3, 4 i 6 navedeni iznad, gde je na nama samo da iskoristimo par atributa i metoda radnog okvira. Videćemo kako ASP.NET Core ovo realizuje kroz svoje kontrolere. Radni okvir nudi i elegantan mehanizam za ubrizgavanje zavisnosti, koji ćemo kasnije sagledati.

Svaka ASP.NET Core aplikacija ima ulaznu tačku u datoteci `Program.cs`, koja se izvršava kada pokrenemo aplikaciju. Sledeći kod prikazuje najmanji oblik te datoteke koji nam je dovoljan za rad:

```cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<BookService>();

var app = builder.Build();
app.MapControllers();

app.Run();
```

U datom kodu treba uočiti sledeće:
- Prvi region koda, oko promenljive `builder`, opisuje od čega se aplikacija sastoji. Ovde prijavljujemo kontrolere i servisnu klasu od koje kontroler zavisi. Ove linije ćemo kasnije detaljnije objasniti.
- Poziv `Build()` pravi aplikaciju na osnovu tog opisa, a `MapControllers()` uključuje rutiranje HTTP zahteva ka kontrolerima.
- Poziv `Run()` pokreće aplikaciju. Od tog trenutka aplikacija sluša na portu i obrađuje zahteve sve dok je ne zaustavimo.

## Kontroleri

**Kontroler** je klasa čije javne metode obrađuju HTTP zahteve. Metode kontrolera zovemo **akcije**. U kod treba svaku akciju deklarativno označiti kako bi radni okvir znao da je aktivira kada HTTP zahtev stigne na određenu adresu. Povezivanje adrese i akcije zovemo **rutiranje** (engl. *routing*). Rutiranje se postiže atributima. Atribut `[Route]` na klasi definiše zajednički početak adrese za sve akcije kontrolera. Atributi poput `[HttpGet]` i `[HttpPost]` na metodi definišu HTTP metodu i ostatak adrese. Parametre akcije radni okvir popunjava iz zahteva automatski, na primer iz adrese ili iz tela zahteva.

Akcija vraća vrednost tipa `ActionResult<T>`, gde je `T` tip podatka koji šaljemo klijentu. Ovaj tip omogućava akciji da vrati podatak ili statusni kod. Kada akcija vrati objekat, radni okvir ga pretvara u JSON i šalje odgovor sa statusnim kodom 200. Kada akcija vrati poziv poput `Problem(statusCode: StatusCodes.Status404NotFound)`, radni okvir šalje odgovor sa statusnim kodom 404.

Sledeći kod prikazuje kontroler koji upravlja knjigama:

```cs
[ApiController]
[Route("api/books")]
public class BookController : ControllerBase
{
  private readonly BookService _bookService;

  public BookController(BookService bookService)
  {
    _bookService = bookService;
  }

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

  [HttpPost]
  public ActionResult<Book> Create(Book book)
  {
    Book created = _bookService.Create(book);
    return Ok(created);
  }
}
```

U datom kodu treba uočiti sledeće:
- Atribut `[Route("api/books")]` na klasi i atribut `[HttpGet("{id}")]` na metodi zajedno znače da zahtev `GET /api/books/5` poziva akciju `GetById`. Deo adrese `{id}` je promenljiv i radni okvir njegovu vrednost smešta u parametar `id`.
- Zahtev `POST /api/books` poziva akciju `Create`. Parametar `book` radni okvir popunjava iz tela zahteva, gde JSON zapis automatski pretvara u objekat klase `Book`.
- Akcija ne sadrži nijednu liniju parsiranja niti pretvaranja u JSON. Njen posao je da pozove logiku aplikacije i da odluči kakav odgovor vraća.
- Atribut `[ApiController]` označava klasu kao kontroler i uključuje podrazumevana ponašanja koja su nam potrebna, poput automatskog popunjavanja parametara.

Više detalja o kontrolerima i obradi HTTP zahteva se nalazi u [lekciji o kontrolerima](2-kontroleri.md).

## Ugrađeni kontejner zavisnosti

Kada stigne HTTP zahtev na ASP.NET aplikaciju, ona će pronaći odgovarajuću akciju koju treba izvršiti kroz rutiranje. Tada će ispod haube instancirati kontroler. Pitanje je kako ASP.NET zna koje objekte treba da pošalje konstruktoru kontrolera.

Obrazac ubrizgavanja zavisnosti nam je poznat. Klasa ne pravi svoje zavisnosti sama, već ih prima kroz konstruktor. U dosadašnjim aplikacijama smo zavisnosti povezivali ručno, tako što smo na jednom mestu pisali kod poput `new BookService(new BookDbRepository())` (u tzv. *Injector* klasi). U serverskoj aplikaciji ručno povezivanje ne funkcioniše. Kontroler se instancira iznova za svaki HTTP zahtev, te ne postoji linija koda `new BookController(...)`. Neko drugi mora da napravi kontroler i sve njegove zavisnosti, za svaki zahtev. Taj posao radi kontejner zavisnosti.

**Kontejner zavisnosti** (engl. *Dependency Injection container*) je komponenta radnog okvira koja pravi objekte i popunjava njihove zavisnosti na osnovu prijavljenih klasa. Klase prijavljujemo u datoteci `Program.cs`, pre pokretanja aplikacije. Kada zatreba objekat neke klase, kontejner čita njen konstruktor, pravi svaku zavisnost koju konstruktor traži i prosleđuje ih konstruktoru.

Sledeći kod prikazuje prijavu klasa i lanac zavisnosti koji kontejner povezuje.

```csharp
// U datoteci Program.cs
builder.Services.AddControllers();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<IBookRepository, BookDbRepository>();
```

```csharp
public class BookService
{
    private readonly IBookRepository _repository;

    public BookService(IBookRepository repository)
    {
        _repository = repository;
    }

    public Book GetById(int id)
    {
        return _repository.GetById(id);
    }
}
```

U datom kodu treba uočiti sledeće:
- Nigde nismo napisali `new`. Kada zahtev stigne u `BookController`, kontejner čita konstruktor kontrolera, vidi da mu treba `BookService`, zatim čita konstruktor te klase i vidi da joj treba `IBookRepository`. Kontejner zna da instancira odgovarajuće objekte i poveže ceo lanac zavisnosti.
- `builder.Services.AddScoped<BookService>();` označava da je `BookService` klasa koju kontejner treba da instancira ako je pronađe u listi parametara konstruktora nekog objekta u lancu koji počinje sa kontrolerom.
- `builder.Services.AddScoped<IBookRepository, BookDbRepository>();` označava da `BookDbRepository` treba instancirati kada se u listi parametara konstruktora nekog objekta u lancu koji počinje sa kontrolerom pojavi `IBookRepository`.

Različiti načini prijavljivanja klasa, kao i pitanje koliko dugo napravljeni objekti žive, se obrađuju u [lekciji o registraciji zavisnosti](3-registracija-zavisnosti.md).

## Put jednog zahteva

Na kraju, povežimo sve pojmove iz lekcije praćenjem jednog zahteva:
1. Klijent šalje `GET /api/books/5`.
2. Radni okvir prihvata konekciju i parsira zahtev.
3. Rutiranje na osnovu adrese i HTTP metode bira akciju `GetById` u klasi `BookController`.
4. Kontejner zavisnosti pravi kontroler i ceo lanac njegovih zavisnosti.
5. Radni okvir popunjava parametar `id` vrednošću 5 i poziva akciju.
6. Akcija poziva logiku aplikacije i vraća knjigu.
7. Radni okvir pretvara knjigu u JSON, dodaje statusni kod 200 i šalje HTTP odgovor klijentu.

Svaki korak smo mi opisali sa nekoliko atributa i linija u `Program.cs`, dok je radni okvir zadužen za gotovo sve detalje. To je suština rada sa radnim okvirom. Mi pišemo logiku aplikacije i deklarativno je označavamo, a tehničke korake obrade zahteva prepuštamo alatu.