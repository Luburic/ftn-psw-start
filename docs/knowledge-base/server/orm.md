> **Tip: lekcija.** Primeri u dokumentu su pojednostavljeni radi učenja koncepta i ne prate konvencije projekta do kraja.

Do sada ste se upoznali sa ADO.NET bibliotekom, koju ste koristili za rad sa bazom podataka. Pisali ste repozitorijumske klase u kojima se nalazio sav kod koji je potreban za rad sa bazom podataka.

Podsetimo se primera jednog repozitorijuma koji koristi ADO.NET za upravljanje knjigama i obratite pažnju na poslove koje svaka metoda mora da reši.

<hr></hr>
<details>
<summary><b>Klikni da vidiš kod BookDbRepository klase</b></summary>

```csharp
public class BookDbRepository
{
  private readonly string connectionString;

  public BookDbRepository(IConfiguration configuration)
  {
    connectionString = configuration["ConnectionString:SQLiteConnection"];
  }

  public List<Book> GetAll()
  {
    List<Book> books = new List<Book>();
    try
    {
      using SqliteConnection connection = new SqliteConnection(connectionString);
      connection.Open();

      string query = "SELECT Id, Name, Author FROM Books";
      using SqliteCommand command = new SqliteCommand(query, connection);

      using SqliteDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        books.Add(new Book(
          Convert.ToInt32(reader["Id"]),
          Convert.ToString(reader["Name"]),
          Convert.ToString(reader["Author"])
        ));
      }
    }
    catch (SqliteException ex)
    {
      Console.WriteLine($"Greška pri povezivanju sa bazom ili izvršavanju SQL upita: {ex.Message}");
      throw;
    }
    catch (FormatException ex)
    {
      Console.WriteLine($"Greška u formatu podataka: {ex.Message}");
      throw;
    }
    catch (InvalidOperationException ex)
    {
      Console.WriteLine($"Greška jer konekcija nije ili je više puta otvorena: {ex.Message}");
      throw;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Neočekivana greška: {ex.Message}");
      throw;
    }
    return books;
  }

  public Book GetById(int id)
  {
    try
    {
      using SqliteConnection connection = new SqliteConnection(connectionString);
      connection.Open();

      string query = "SELECT Id, Name, Author FROM Books WHERE Id = @Id";
      using SqliteCommand command = new SqliteCommand(query, connection);
      command.Parameters.AddWithValue("@Id", id);

      using SqliteDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        return new Book(
          Convert.ToInt32(reader["Id"]),
          Convert.ToString(reader["Name"]),
          Convert.ToString(reader["Author"])
        );
      }
    }
    catch (SqliteException ex)
    {
      Console.WriteLine($"Greška pri povezivanju sa bazom ili izvršavanju SQL upita: {ex.Message}");
      throw;
    }
    catch (FormatException ex)
    {
      Console.WriteLine($"Greška u formatu podataka: {ex.Message}");
      throw;
    }
    catch (InvalidOperationException ex)
    {
      Console.WriteLine($"Greška jer konekcija nije ili je više puta otvorena: {ex.Message}");
      throw;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Neočekivana greška: {ex.Message}");
      throw;
    }
  }

  public Book Create(Book book)
  {
    try
    {
      using SqliteConnection connection = new SqliteConnection(connectionString);
      connection.Open();

      string sql = @"INSERT INTO Books (Name, Author) VALUES (@Name, @Author); SELECT LAST_INSERT_ROWID();";

      using SqliteCommand command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@Name", book.Name);
      command.Parameters.AddWithValue("@Author", book.Author);

      // Izvršavanje komande i dobijanje id-ja
      book.Id = Convert.ToInt32(command.ExecuteScalar());

      return book;
    }
    catch (SqliteException ex)
    {
      Console.WriteLine($"Greška pri povezivanju sa bazom ili izvršavanju SQL upita: {ex.Message}");
      throw;
    }
    catch (FormatException ex)
    {
      Console.WriteLine($"Greška u formatu podataka: {ex.Message}");
      throw;
    }
    catch (InvalidOperationException ex)
    {
      Console.WriteLine($"Greška jer konekcija nije ili je više puta otvorena: {ex.Message}");
      throw;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Neočekivana greška: {ex.Message}");
      throw;
    }
  }

  public Book Update(Book newBook)
  {
    try
    {
      using SqliteConnection connection = new SqliteConnection(connectionString);
      connection.Open();

      string sql = "UPDATE Books SET Name=@Name, Author=@Author WHERE Id=@Id";

      using SqliteCommand command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@Name", newBook.Name);
      command.Parameters.AddWithValue("@Author", newBook.Author);
      command.Parameters.AddWithValue("@Id", newBook.Id);

      int affectedRows = command.ExecuteNonQuery();
      return affectedRows > 0 ? newBook : null;
    }
    catch (SqliteException ex)
    {
      Console.WriteLine($"Greška pri povezivanju sa bazom ili izvršavanju SQL upita: {ex.Message}");
      throw;
    }
    catch (InvalidOperationException ex)
    {
      Console.WriteLine($"Greška jer konekcija nije ili je više puta otvorena: {ex.Message}");
      throw;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Neočekivana greška: {ex.Message}");
      throw;
    }
  }

  public bool Delete(int id)
  {
    try
    {
      using SqliteConnection connection = new SqliteConnection(connectionString);
      connection.Open();

      string sql = "DELETE FROM Books WHERE Id = @Id";

      using SqliteCommand command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@Id", id);

      int rowsAffected = command.ExecuteNonQuery();

      return rowsAffected > 0;
    }
    catch (SqliteException ex)
    {
      Console.WriteLine($"Greška pri povezivanju sa bazom ili izvršavanju SQL upita: {ex.Message}");
      throw;
    }
    catch (InvalidOperationException ex)
    {
      Console.WriteLine($"Greška jer konekcija nije ili je više puta otvorena: {ex.Message}");
      throw;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Neočekivana greška: {ex.Message}");
      throw;
    }
  }
}
```
 </details>
<hr></hr>

Vidimo da je za implementaciju osnovnih CRUD operacija za jednostavnu klasu (sa svega 3 atributa) neophodna velika količina koda (200 linija!).

Ne zaboravimo da entiteti mogu imati desetine atributa i više veza sa drugim tabelama. Pošto je često potrebno preuzeti podatke iz više tabela na osnovu tih veza, možemo pretpostaviti koliko će komplikovan biti repozitorijum koji to postiže. U stvarnim aplikacijama bi repozitorijumi lako prešli 1000 linija koda, te bi održavanje tog koda i praćenje izmena bilo veoma izazovno. Na sreću, postoje alati koji pojednostavljuju interakciju sa bazom podataka i čine kod mnogo jednostavnijim.

## Objektno-relaciono mapiranje

**Objektno-relaciono mapiranje** (engl. *Object-Relational Mapping*, ORM) je proces prevođenja podataka između objektno-orijentisanih programskih jezika i relacionih baza podataka. Biblioteke koje implementiraju logiku objektno-relacionog mapiranja se zovu **objektno-relacioni maperi**.

Objektno-relacioni maperi automatizuju i sakrivaju značajan deo posla rada sa bazom podataka. Takođe omogućuju programerima da interaguju sa bazom kroz funkcije programskog jezika umesto da pišu SQL. Uz pomoć njih možemo kroz par linija koda i komandi da postignemo sledeće:
- Spram definicije klase se generiše i izvršava SQL CREATE naredba, gde se svojstva klase preslikavaju na kolone u tabelama,
- Za konkretan objekat se generiše i izvršava INSERT ili UPDATE naredba,
- Pozivom funkcije za čitanje sadržaja tabele se generiše i izvršava SELECT naredba, gde se povratni string automatski parsira i pretvara u jedan ili više objekata.

U našim projektima ćemo koristiti objektno-relacioni maper koji je najpoznatiji u C# svetu i zove se **Entity Framework Core** (EF). U nastavku je `BookDbRepository` klasa koja koristi EF umesto ADO.NET za rad sa podacima. Kod je dosta jednostavniji i kraći nego u prethodnom primeru.

```csharp
public class BookDbRepository
{
    private BookDbContext _context;

    public BookDbRepository(BookDbContext context)
    {
        _context = context;
    }

    public List<Book> GetAll()
    {
        return _context.Books.ToList(); // ToList ispod haube generiše SELECT * FROM Books
                                        // i automatski parsira dobijen string u List<Book>
    }

    public Book GetById(int id)
    {
        return _context.Books.Find(id); // Ispod haube generiše SELECT upit
                                        // kojim traži knjigu sa zadatim id-em
                                        // i pretvara u instancu klase Book
    }

    public Book Create(Book book)
    {
        _context.Books.Add(book);
        _context.SaveChanges(); // SaveChanges ispod haube proverava koje su se sve
                                // izmene desile nad DbSet objektom i generiše
                                // optimalan SQL (u ovom slučaju INSERT naredbu)
                                // zbog prethodno pozvane metode Add
        return book;
    }

    public Book Update(Book book)
    {
        _context.Books.Update(book);
        _context.SaveChanges(); // Kao iznad, samo sada generiše UPDATE
                                // zbog prethodno pozvane metode Update
        return book;
    }

    public void Delete(int id)
    {
        Book book = _context.Books.Find(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            _context.SaveChanges(); // Kao iznad, samo sada generiše DELETE
                                    // zbog prethodno pozvane metode Remove
        }
    }
}
```
U datom kodu vidimo više jednostavnih, a veoma moćnih linija koda. Pozivom metoda kao što su `ToList()`, `Find()`, `Add()`, `Update()`, `Remove()`, i `SaveChanges()`, EF sam formira odgovarajuće SQL naredbe i izvršava ih. Programer samo koristi metode kao da radi sa običnim C# objektima, bez ručnog upravljanja konekcijom ka bazi, pretvaranja objekta u SQL naredbu i pretvaranja rezultata SQL naredbe u objekat.

U datom primeru vidimo da smo sa 200 linija koda se spustili na 40. Ovo nije skroz fer poređenje jer je prethodna implementacija repozitorijuma takođe upravljala izuzecima. Ako bismo slično uveli ovde, `BookDbRepository` bi porastao na blizu 100 linija koda. Međutim, prava prednost ORM tehnologije dolazi do izražaja kod složenijih repozitorijuma (npr. entiteti sa više svojstava i veza). U tom slučaju bismo videli kako 500 linija koda spada na 100.