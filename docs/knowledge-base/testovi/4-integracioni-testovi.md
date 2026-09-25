**Integracioni test** (engl. *integration test*) je automatski test koji proverava jedinicu ponašanja, ali ne ispunjava drugi ili treći uslov jediničnog testa, jer se obraća bazi podataka ili drugom sistemu ili zavisi od drugih testova.

Kod projektovanja integracionih testova želimo da obuhvatimo što je moguće veći skup komponenti u jednom izvršavanju, kako bismo postigli značajnu zaštitu od regresija. Sledeći kod prikazuje test `Tour_is_published_when_all_rules_are_met` iz klase `TourAuthoringCommandTests`, koji proverava isto ponašanje kao istoimeni jedinični test iz prve lekcije:

```cs
[Fact]
public async Task Tour_is_published_when_all_rules_are_met()
{
    // Arrange
    var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

    // Act
    var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

    // Assert - Response
    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    // Assert - Database
    using var assertContext = Factory.CreateContext<ExplorationDbContext>();
    var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
    stored.Status.Should().Be(TourStatus.Published);
    stored.PublishedAt.Should().NotBeNull();
}
```

U datom kodu treba uočiti sledeće:

- Jedinični test je direktno gradio turu (kroz konstruktor i metodu `AddTransportTime`). Integracioni test turu ne gradi već je zatiče u bazi podataka. Kako se testna baza podataka popunjava opisuje naredna lekcija. Priprema koju vidimo se svodi na jedan red, u kom se pravi HTTP klijent koji će sa HTTP zahtevom poslati JWT za korisnika `WellKnownUsers.Explorer` sa ulogom `explorer`.
- Akcija podrazumeva slanje HTTP POST zahteva na navedeni URL.
- Provera prvo sagledava da li HTTP odgovor ima statusni kod 204, što dokazuje da je zahtev prošao proveru identiteta, kontroler i ostatak funkcije bez izuzetka.
- Provera kroz `Factory.CreateContext` otvara kontekst modula i čita turu iz baze. Tek red u bazi dokazuje da je jedinica posla sačuvala izmenu i da je postignut primarni željeni ishod.
- Polje `Factory` dolazi iz roditeljske klase koju test klasa nasleđuje, a `TourSeed` je statička klasa sa početnim podacima modula.

Pre nego što se izvrši kod test metode, serverska aplikacija je već pokrenuta, a testna baza podataka vraćena u početno stanje. U prethodnom primeru se to ne vidi, a kako se postiže opisuje naredna lekcija. Kod integracionog testa zatim tipično ima sledeću strukturu:
1. Arrange
   1. Priprema HTTP klijent, koji uz zahtev šalje JWT korisnika kada krajnja tačka to traži
   2. Priprema HTTP zahtev koji se šalje serverskoj aplikaciji (kada je zahtev složeniji)
   3. Kod složenih ishoda, priprema očekivane vrednosti koje se koriste u proveri (kada su vrednosti složenije)
2. Act
   1. Šalje HTTP zahtev serverskoj aplikaciji
3. Assert
   1. Proverava sadržaj HTTP odgovora serverske aplikacije
   2. Kod uspešnih komandnih operacija, proverava stanje baze podataka nakon obrade HTTP zahteva

## Upravljane i neupravljane zavisnosti

**Zavisnost van procesa** (engl. *out-of-process dependency*) je sistem sa kojim aplikacija komunicira, a koji ne živi u njenom procesu. Drugi uslov jediničnog testa zabranjuje obraćanje takvoj zavisnosti, pa joj se obraćaju jedino integracioni testovi. Pitanje je da li test radi sa pravom zavisnošću ili je zamenjuje. Zavisnosti van procesa delimo na upravljane zavisnosti i neupravljane zavisnosti.

**Upravljana zavisnost** (engl. *managed dependency*) je zavisnost van procesa čije promene vide samo testovi. Primer je testna baza podataka modula, čija jedina svrha je da skladišti podatke koje aplikacija generiše tokom testiranja. Drugi primer je testni API eksternog servisa, koji servisi nude baš za svrhu provere rada integracije. Tako PayPal nudi pravljenje "lažnog" računa nad kojim softver može da izvršava transakcije bez da je ugrožen stvarni PayPal nalog sa stvarnim novcem.

**Neupravljana zavisnost** (engl. *unmanaged dependency*) je zavisnost van procesa čije promene vide i drugi sistemi. Primer je produkciona baza podataka, čije podatke kroz našu aplikaciju vide stvarni korisnici. Drugi primer je servis za slanje elektronske pošte, koji bi opteretio korisnike spam porukama ako bi ga automatski testovi pozivali svako malo. Poslate poruke, naplaćen iznos ili izmena u produkcionoj bazi su posledice koje drugi sistem vidi (bilo da je taj sistem čovek ili softver), pa test ne sme da ih izazove. Takva zavisnost se u testu zamenjuje. Ovo se može postići tako što:
1. Uvedemo upravljanu zavisnost koja zamenjuje eksterni sistem (npr. testni API ili posebnu bazu podataka za testiranje).
2. Podmetnemo lažnu implementaciju (engl. *test double*) naše klase koja interaguje sa eksternim sistemom, tako da osiguramo da ga ne opterećuje. Ovo radimo kada opcija 1 nije moguća i razmatraćemo kasnije.

U našem projektu smo osposobili serversku aplikaciju da podigne testnu bazu podataka kada god se testovi pokrenu. Testovi rade sa pravim PostgreSQL serverom, ali ne prljaju podatke produkcione baze.

## Scenariji koje integracioni test pokriva

Integracioni testovi jedne komande pokrivaju tri vrste scenarija.

Prva vrsta je **uspešan scenario** (engl. *happy path*), tok u kome zahtev prolazi kroz sve slojeve i završava upisom u bazu. Primer je test `Explorer_creates_a_draft_tour`, koji šalje ispravan zahtev, proverava vraćenu DTO strukturu i čita upisanu turu iz baze.

Drugu vrstu čine **odbijanja van domena**, koja nastaju u obradi zahteva pre nego što se agregat uopšte pozove. Sledeći kod prikazuje tri takva testa iz klase `TourAuthoringCommandTests`:

```cs
[Fact]
public async Task Anonymous_user_cannot_create_a_tour()
{
    var client = Factory.CreateClient();
    var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

    var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}

[Fact]
public async Task Administrator_cannot_create_a_tour()
{
    var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
    var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

    var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}

[Fact]
public async Task Explorer_cannot_publish_another_authors_tour()
{
    var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

    var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    using var assertContext = Factory.CreateContext<ExplorationDbContext>();
    var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
    stored.Status.Should().Be(TourStatus.Draft);
}
```

U datom kodu treba uočiti sledeće:

- Prvi test koristi klijenta bez prijavljenog korisnika, koji šalje HTTP POST bez tokena, i očekuje odgovor sa statusnim kodom 401. Middleware za proveru identiteta odbija zahtev pre nego što on stigne do kontrolera.
- Drugi test koristi klijenta prijavljenog sa ulogom `administrator` i očekuje statusni kod 403. Atribut `[Authorize(Roles = "explorer")]` na kontroleru odbija zahtev čija uloga iz tokena nije `explorer`, pre nego što pozove akciju kontrolera.
- Treći test koristi klijenta prijavljenog kao nasumičan korisnik i očekuje statusni kod 404, jer servis tretira turu drugog autora kao nepostojeću. Provera zatim čita bazu i potvrđuje da je tura ostala u statusu nacrta, jer odbijena komanda ne sme da ostavi trag.

Treća vrsta je **odbijanje domena**, test koji proverava da li izuzetak iz agregata postaje odgovor sa statusnim kodom 400 i da li baza ostaje nepromenjena. Test `Explorer_cannot_create_a_tour_without_a_name` šalje zahtev sa praznim imenom ture, a zatim proverava statusni kod i da li je broj tura u bazi ostao isti.

Testovi upita su slični, uz ključnu razliku da upit ne menja stanje baze podataka. Test upita ne čita bazu posle akcije, već samo proverava HTTP odgovor krajnje tačke, uz veći akcenat na ispitivanje tela odgovora.
