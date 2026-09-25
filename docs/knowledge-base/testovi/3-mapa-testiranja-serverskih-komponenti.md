Sagledali smo opšte aspekte kvaliteta testova koji su relevantni za sve vrste automatskih testova. Sada ćemo razmotriti kakve testove pišemo za različite vrste koda serverske aplikacije.

U početnom projektu vidimo da modul `Exploration` ima agregat `Tour`, aplikacioni servis `TourAuthoringService`, upitnu klasu `TourBrowsingQueries`, repozitorijum `TourRepository`, API kontroler `TourAuthoringController` i strukturu `TourDto`. Kada bi svaka od tih klasa dobila svoj test, deo testova bi bio bezvredan, a doneo bi trošak održavanja. Bolje je ne napisati test nego napisati loš.

Da bismo razumeli šta je prikladno testirati i na koji način, izdelićemo kod po dve dimenzije:
- Složenost i domenski značaj
- Broj saradnika

## Dve dimenzije koda

**Složenost** koda dolazi iz kompleksnosti izvršavanja i izraza koje koristi. Pedeset linija koje ispisuju labelu na konzolu su trivijalne. Pedeset linija sa više ugnježdenih petlji ili dugačkim lancima LINQ izraza predstavljaju složen kod. **Domenski značaj** koda je mera koliko kod neposredno iskazuje pravilo domena. Agregati i domenski servisi sadrže većinu domenskog značaja aplikacije. Ove dve osobine čine prvu dimenziju i posmatraju se zajedno, jer je kod vredan testa ako ima bilo koju od njih. Izračunavanje cene bez ijednog grananja nije složeno, ali je domenski značajno i zaslužuje test. Složen kod je mesto gde greške nastaju, a domenski značajan kod je mesto gde greška najviše košta.

**Saradnik** je zavisnost koda koja ima promenljivo stanje ili živi van procesa, poput repozitorijuma ili baze podataka. Objekti koje klasa sadrži kao svoje stanje, poput vremena transporta u turi, nisu njeni saradnici. Vrednosni objekti i nepromenljivi ulazi takođe nisu saradnici. Broj saradnika je druga dimenzija i određuje trošak testa, jer takav test mora svakog saradnika da dovede u očekivano stanje pre akcije i da ga proveri posle nje. Što je veći broj saradnika, to je test teži za održavanje.

Sledeći kod uporedo prikazuje metodu agregata i metodu aplikacionog servisa iz projekta:

```cs
public void Publish()
{
    if (Status == TourStatus.Published)
    {
        throw new DomainException("The tour is already published.");
    }
    if (Description.Length < MinimumDescriptionLengthForPublishing)
    {
        throw new DomainException("...");
    }
    if (_transportTimes.Count == 0)
    {
        throw new DomainException("...");
    }

    Status = TourStatus.Published;
    PublishedAt = DateTime.UtcNow;
}

public async Task PublishAsync(Guid tourId, Guid authorId)
{
    var tour = await GetOwnedTourAsync(tourId, authorId);

    tour.Publish();
    await _unitOfWork.SaveChangesAsync();
}
```

U datom kodu treba uočiti sledeće:

- Metoda `Tour.Publish` nije složena, ali ima tri grananja i sva tri su pravila domena, pa ima visok domenski značaj. Ima samo jednog saradnika, a to je `DateTime.UtcNow`, čije stanje se menja kako vreme teče.
- Metoda `TourAuthoringService.PublishAsync` nema nijedno pravilo domena. Ima tri saradnika: repozitorijum kroz koji učitava turu, samu turu i jedinicu posla kroz koju čuva izmene.

Što je kod važniji, to manje saradnika treba da ima. Kod koji ima i pravila i saradnike je najskuplji za testiranje, jer test mora i da postavi svakog saradnika i da prođe kroz svako grananje.

## Četiri tipa koda

Dve dimenzije daju četiri tipa koda. Sledeća tabela ih prikazuje, zajedno sa komponentama čiste arhitekture koje sadrže takav kod i vrstom testa kojim se komponente testiraju:

| | Malo saradnika | Mnogo saradnika |
|---|---|---|
| **Složen ili domenski značajan** | Agregati, domenski servisi i složeni lokalni tehnički stručnjaci. Jedinični testovi. | Hibridne (koordinatorsko-stručnjačke) klase. Refaktorišu se pre testiranja. |
| **Jednostavan i bez domenskog značaja** | Trivijalan kod. Ne testira se direktno. | Aplikacioni servisi. Integracioni testovi. |

### Složene ili domenski značajne klase sa malo saradnika

U ovu kategoriju spada nekoliko komponenti:
- Agregati
- Domenski servisi
- Složeni lokalni tehnički stručnjaci

Agregati imaju visok domenski značaj, a ponekad i visoku složenost. Ove komponente testiramo sa jediničnim testovima, gde priprema instancira objekte koji čine agregat i dovodi ga u željeno početno stanje, akcija poziva metodu nad korenom agregata koja predstavlja ponašanje koje se testira, a provera poredi stvarnu povratnu vrednost ili stvarno stanje agregata sa očekivanim.

Domenski servis nema stanje, te je njegova priprema trivijalna. Ostatak testnog koda liči na test agregata.

Složen lokalni tehnički stručnjak je stručnjačka klasa infrastrukturnog sloja koja implementira složen tehnički algoritam. Primer takve klase je servis koji parsira Excel dokument i iz njega izvlači podatke koje pretvara u određen objektni model. Klasa `JwtTokenFactory` modula `Identity` nije dobar primer jer ceo posao prepušta biblioteci, pa bi njen test proveravao biblioteku.

### Jednostavne klase sa mnogo saradnika

Odgovornost aplikacionog servisa je da poziva druge klase u dobrom redosledu, kako bi objekti kolektivno ispunili određen zahtev. Njegovi saradnici su repozitorijum i jedinica posla, koji rade sa bazom podataka, pa je test servisa po definiciji integracioni. Ovakve klase testiramo kroz integracione testove, kako bismo osigurali da je logika koordinacije ispravna, kao i da ceo sistem zajedno radi. Ovakva postavka pruža snažnu zaštitu od regresija, jer se testovi direktno naslanjaju na zahteve koje softver treba da ispuni.

Pošto aplikacione servise pozivaju kontroleri, integracioni testovi se zapravo sprovode nad kontrolerima, kako bismo osigurali da je čitava putanja pokrivena, uključujući middleware. Repozitorijum i jedinica posla se izvršavaju kroz servis koji ih poziva, pa zaseban test ne dobijaju.

### Trivijalan kod

Trivijalan kod nema ni grananja ni pravila. U modulu su to konstruktori koji samo dodeljuju vrednosti, svojstva, DTO strukture i mapiranje agregata na DTO. Ovaj kod ne testiramo direktno, ali se često pokrije kroz druge vrste testova. Na primer, greška u mapiranju na DTO se hvata usput, kada integracioni test pročita odgovor krajnje tačke.

### Hibridne klase

Hibridna klasa je klasa koja i koordiniše saradnike i sama sadrži pravila. U modulu bi to bio aplikacioni servis koji sam proverava pravilo domena ili agregat koji poziva repozitorijum. Sledeći kod prikazuje takav servis:

```cs
public async Task PublishAsync(Guid tourId, Guid authorId)
{
    var tour = await GetOwnedTourAsync(tourId, authorId);
    if (tour.Description.Length < 100)
    {
        throw new DomainException("A tour can be published only with a longer description.");
    }

    tour.Publish();
    await _unitOfWork.SaveChangesAsync();
}
```

U datom kodu treba uočiti sledeće:

- Pravilo o dužini opisa sada živi u servisu, pored saradnika, a metoda `Tour.Publish` ga ne sadrži. Pravilo se može proveriti samo kroz servis, pa test mora da poseje turu sa kratkim opisom u bazu. Taj test je skuplji od jediničnog testa `Tour_with_a_short_description_cannot_be_published` iz klase `TourTests`.
- Ispravka nije bolji test, nego refaktorisanje: pravilo se premešta u metodu `Tour.Publish`, gde ono i jeste u projektu. Servis ostaje bez pravila i vraća se među koordinatorske klase.
- Kod može da bude dubok, sa mnogo pravila, ili širok, sa mnogo saradnika, ali ne oboje. Podela na domenski i aplikacioni sloj iz čiste arhitekture je upravo ta podela, pa u modulu koji je poštuje ovo polje ostaje prazno.