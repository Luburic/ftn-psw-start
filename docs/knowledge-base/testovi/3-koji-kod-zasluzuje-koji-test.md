# Koji kod zaslužuje koji test

Prethodna lekcija je pokazala da test trivijalnog koda ima vrednost nula, a test koji proverava korake umesto ishoda takođe. Ostaje pitanje koje klase modula sadrže kod vredan testiranja i kojom vrstom testa. Modul Exploration ima agregat `Tour`, aplikacioni servis `TourAuthoringService`, upitnu klasu `TourBrowsingQueries`, repozitorijum `TourRepository`, API kontroler `TourAuthoringController` i strukturu `TourDto`. Kada bi svaka od tih klasa dobila svoj test, veći deo testova bi bio bezvredan, a njihovo održavanje bi koštalo koliko i održavanje vrednih. Bolje je ne napisati test nego napisati loš, pa je odluka šta se ne testira jednako važna kao odluka šta se testira.

Ova lekcija daje postupak kojim se za svaku klasu odlučuje da li dobija jedinični test, integracioni test ili nijedan. Postupak polazi od dve dimenzije koda.

## Dve dimenzije koda

**Složenost** (engl. *complexity*) koda je broj tačaka grananja u njemu. **Domenski značaj** (engl. *domain significance*) koda je mera koliko kod neposredno iskazuje pravilo domena. Ove dve osobine čine prvu dimenziju i posmatraju se zajedno, jer je kod vredan testa ako ima bilo koju od njih. Složen kod je mesto gde greške nastaju, a domenski značajan kod je mesto gde greška najviše košta. Izračunavanje cene bez ijednog grananja je domenski značajno i zaslužuje test.

**Saradnik** (engl. *collaborator*) je zavisnost koda koja ima promenljivo stanje ili živi van procesa, poput repozitorijuma ili baze podataka. Vrednosni objekti i nepromenljivi ulazi nisu saradnici. Broj saradnika je druga dimenzija i određuje trošak testa, jer test svakog saradnika mora da dovede u očekivano stanje pre akcije i da ga proveri posle nje. Test koda sa mnogo saradnika je dug, a dug test je teško održavati.

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

- Metoda `Tour.Publish` ima tri grananja i sva tri su pravila domena. Osim poziva `DateTime.UtcNow`, radi samo nad sopstvenim stanjem.
- Metoda `TourAuthoringService.PublishAsync` nema nijedno pravilo domena. Ima dva saradnika, repozitorijum kroz koji učitava turu i jedinicu posla kroz koju čuva izmene.
- Saradnik se računa i kada nije prosleđen kao parametar. Poziv `DateTime.UtcNow` je takav saradnik, jer test ne može da predvidi tačno vreme, pa test iz prve lekcije proverava samo da `PublishedAt` nije prazno.

Što je kod važniji, to manje saradnika treba da ima. Kod koji ima i pravila i saradnike je najskuplji za testiranje i o njemu govori jedno od polja u nastavku.

## Četiri tipa koda

Dve dimenzije daju četiri tipa koda. Sledeća tabela ih prikazuje, zajedno sa slojem modula u kome se svaki tip nalazi i vrstom testa koju dobija:

| | Malo saradnika | Mnogo saradnika |
|---|---|---|
| **Složen ili domenski značajan** | Domenski model: agregati, entiteti, vrednosni objekti, domenski servisi. Jedinični testovi. | Prekomplikovan kod. Deli se pre testiranja. |
| **Jednostavan i bez domenskog značaja** | Trivijalan kod: konstruktori, svojstva, DTO strukture. Bez testa. | Kontroleri: aplikacioni servis, repozitorijum, API kontroler. Integracioni testovi. |

Naredni odeljci obrađuju polja tabele jedno po jedno.

### Domenski model

Domenski model ima visok značaj i nema saradnike, pa je njegov test kratak, brz i najviše štiti. Klasa `TourTests` iz prve lekcije je test domenskog modela. Test gradi agregat konstruktorom, poziva njegove metode i proverava stanje. Ne koristi bazu, ne pokreće aplikaciju i ne zamenjuje nijednu zavisnost, jer zavisnosti nema. Direktorijum `Unit/` test projekta sadrži samo ovakve testove.

### Kontroleri

U ovoj tabeli **kontroler** je svaki kod koji koordinira rad drugih klasa, a sam ne sadrži pravila. U modulu su to zajedno aplikacioni servis, repozitorijum i API kontroler. Put komande `Publish` prolazi kroz sve tri klase. `TourAuthoringController.Publish` čita identifikator korisnika iz zahteva i poziva `TourAuthoringService.PublishAsync`. Servis kroz `TourRepository` učitava turu, poziva `tour.Publish()` i čuva izmene kroz jedinicu posla.

Jedinični test ovog koda bi morao da zameni repozitorijum i jedinicu posla, a proverio bi tri reda koordinacije. Takav test ima malu zaštitu od regresija i veliki trošak. Umesto toga, kod iz polja kontrolera se testira malim brojem integracionih testova koji prolaze kroz sve tri klase odjednom, sa pravom bazom. Oblik tih testova razmatra naredna lekcija.

### Trivijalan kod

Trivijalan kod nema ni grananja ni pravila. U modulu su to konstruktori koji samo dodeljuju vrednosti, svojstva, DTO strukture i mapiranje agregata na DTO. Struktura `TourDto` je zapis sa devet svojstava i ničim drugim. Njen test bi izvršio kod u kome greške nema. Greška u mapiranju na DTO se hvata usput, kada integracioni test pročita odgovor krajnje tačke.

### Prekomplikovan kod

Prekomplikovan kod ima i pravila i saradnike. U modulu bi to bio aplikacioni servis koji sam proverava pravilo domena ili agregat koji poziva repozitorijum. Sledeći kod prikazuje takav servis:

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

- Pravilo o dužini opisa sada živi u servisu, pored dva saradnika. Jedinični test pravila mora da zameni repozitorijum, a integracioni test mora da poseje turu sa kratkim opisom u bazu. Oba testa su skuplja od testa `Publish_rejects_a_short_description` iz prve lekcije.
- Ispravka nije bolji test, nego premeštanje pravila u metodu `Tour.Publish`, gde ono i jeste u projektu. Servis ostaje bez pravila i vraća se u polje kontrolera.
- Kod može da bude dubok, sa mnogo pravila, ili širok, sa mnogo saradnika, ali ne oboje. Podela na domenski i aplikacioni sloj iz čiste arhitekture je upravo ta podela, pa u modulu koji je poštuje ovo polje ostaje prazno.

## Preduslovi

**Preduslov** (engl. *precondition*) je uslov koji ulaz metode mora da ispuni, a čije kršenje metoda odbija izuzetkom. Konstruktor klase `Tour` ima tri preduslova, a metoda `Publish` još tri. Nije svaki preduslov vredan testa.

Preduslov koji iskazuje pravilo domena je invarijanta agregata i dobija test. Uslov da tura ima bar jednu oznaku je pravilo domena, pa test `Constructor_rejects_empty_tags` postoji. Preduslov koji štiti od greške u kodu, na primer uslov da niz sa podacima za rehidraciju ima očekivan broj elemenata, nema domensko značenje. Takav preduslov postoji da bi greška u kodu bila otkrivena rano, i test za njega se ne piše.

## Repozitorijumi i upiti

Dve klase modula izgledaju kao da zaslužuju zaseban test, a ne zaslužuju.

Repozitorijum `TourRepository` ima dve metode bez grananja i jednog saradnika van procesa, bazu. Pripada polju kontrolera. Njegov zaseban test bi tražio bazu, pa bi koštao koliko integracioni test, a proverio bi samo ponašanje biblioteke EF Core. Greška u mapiranju agregata na tabelu se hvata kada integracioni test komande pročita red iz baze posle zahteva.

Upitna klasa `TourBrowsingQueries` ne prolazi kroz domenski sloj, jer čitanje ne menja stanje, pa nema šta da se jedinično testira. Greška u čitanju vraća pogrešan spisak, ali ne kvari podatke, pa je cena te greške manja od cene greške u komandi. Kada se upit testira, testira se integraciono, kroz projekciju koju krajnja tačka vraća. Ni repozitorijum ni upitna klasa nemaju direktorijum u `Unit/`.

## Piramida testova

**Piramida testova** (engl. *test pyramid*) je odnos u kome jedinični testovi čine većinu skupa testova, a integracioni manjinu. Integracioni test je sporiji i skuplji za održavanje, pa se piše samo tamo gde jedinični ne dopire, za polje kontrolera i za spoj sa bazom. Sva pravila domena ostaju u jediničnim testovima.

Oblik zavisi od modula. Modul sa bogatim agregatom, poput Exploration, ima mnogo jediničnih i malo integracionih testova. Modul koji uglavnom čuva i čita podatke, bez pravila, ima približno isti broj jednih i drugih. Takav oblik nije nedostatak modula, nego posledica toga što u njemu nema pravila koja bi jedinični testovi proveravali.
