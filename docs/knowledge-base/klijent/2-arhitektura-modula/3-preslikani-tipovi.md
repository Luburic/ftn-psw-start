Svaki resurs i svaka komanda iz prethodnih lekcija ima tip podatka koji šalje ili prima, poput `TourDto` i `CreateTourDto`. Ti tipovi opisuju podatak koji ne nastaje na klijentu, nego na serveru, a klijent ih ipak drži zapisane kod sebe.

Posmatrajmo šta se dešava kada tim koji radi na serveru svojstvo `Description` strukture `TourDto` preimenuje u `Summary`.

Prvo imamo prevođenje servera. Ono prolazi, jer je tim preimenovao svojstvo i na entitetu i u DTO strukturi, a maper ih spaja po imenu, pa se i dalje poklapaju. Testovi prođu, endpoint vraća JSON u kom sada stoji `summary`. Ništa u tom prevođenju ne zna da klijent postoji, pa nema ko da se pobuni.

Potom imamo prevođenje klijenta. I ono prolazi, jer klijentski interfejs `TourDto` i dalje ima svojstvo `description`, a nijedan alat ga ne poredi sa serverom. Zapis `httpResource<TourDto>(...)` je obećanje dato prevodiocu, ne provera koja se izvršava.

Najzad, korisnik otvori stranicu ture. Odgovor nema ključ `description`, pa je `tour.description` vrednost `undefined`, a šablon `undefined` iscrtava kao prazan tekst. Nema poruke o grešci ni traga u konzoli, samo polje koje je juče imalo tekst. Greška je iste vrste kao kada se na serveru preimenuje svojstvo, a profil mapera ostane nedopunjen, i jedna i druga prođu prevođenje i sačekaju prvog korisnika, s tim što maper bar baci izuzetak, dok klijent ćuti.

Lekcija o DTO strukturama je pokazala da je izlazna DTO struktura aplikacionog sloja oblik podatka koji klijent prikazuje, a lekcija o API sloju da je akcija vraća bez prevođenja. DTO struktura servera je zato ugovor između dve strane, a klijentski tip sme da bude samo njena slika. Ovde upoznajemo preslikani tip, pravila po kojima se tipovi servera prevode u tipove klijenta i pravilo o tome kada se takav tip menja.

## Preslikani tip

**Preslikani tip** (engl. *mirrored type*) je tip koji ima ista svojstva kao DTO struktura servera, sa tipovima prevedenim po utvrđenim pravilima. Preslikani tipovi jednog modula stoje u jednoj datoteci, u direktorijumu `api` tog modula. Strukture koje server vraća svim modulima, poput `PageResult` za spisak sa stranama, stoje van modula, u direktorijumu `shared/api` koji dele svi moduli. Svaki modul uvozi tipove iz sopstvenog direktorijuma `api`, pa granica između modula važi i za tipove.

Sledeći kod prikazuje DTO strukturu `TourDto` sa servera i njen preslikani tip iz datoteke `exploration-api-types.ts`:

```cs
public sealed record TourDto(
    Guid Id,
    Guid AuthorId,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    TourStatus Status,
    DateTime? PublishedAt,
    List<TransportTimeDto> TransportTimes);
```

```ts
export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  transportTimes: TransportTimeDto[];
}
```

U datom kodu treba uočiti sledeće:

- Svako svojstvo servera ima istoimeno svojstvo na klijentu, zapisano malim početnim slovom, jer radni okvir servera tako imenuje svojstva u JSON zapisu.
- Enumeraciju server u JSON zapis upisuje kao tekst sa imenom vrednosti, jer je u `Program.cs` registrovan `JsonStringEnumConverter`; bez njega bi u zapisu stajao redni broj vrednosti.
- `Guid` i `DateTime` u JSON zapisu ne postoje, već stižu kao tekst, pa su na klijentu `string`. Klijent identifikator nikada ne tumači, a datum prevodi u prikaz tek u šablonu.
- Svojstvo koje server sme da ostavi prazno, `DateTime?`, na klijentu ima uniju sa `null`, pa prevodilac traži proveru pre upotrebe.
- Ugnježdena DTO struktura je ugnježden interfejs, a lista je niz.

Sledeća tabela sažima pravila preslikavanja:

| Tip na serveru | Tip na klijentu | Primer |
|---|---|---|
| `string`, `int`, `bool` | `string`, `number`, `boolean` | `name: string` |
| Enumeracija | Unija tekstualnih literala | `status: TourStatus` |
| `Guid`, `DateTime` | `string` | `id: string` |
| Tip sa `?` | Unija sa `null` | `publishedAt: string \| null` |
| DTO struktura | Interfejs | `TransportTimeDto` |
| `List<T>` | `T[]` | `tags: string[]` |

## Pravilo o izmeni

Preslikani tip se menja samo kada se promeni DTO struktura servera, i to u istoj izmeni koda. Tim koji na serveru preimenuje, doda ili ukloni svojstvo, u istoj izmeni preslikava tu promenu u datoteku `api` svog modula. Ništa drugo tu datoteku ne menja. U nju ne ulazi svojstvo koje postoji samo na klijentu, tip koji server ne vraća, ni drugačije ime od serverskog.

Time preimenovanje sa početka lekcije menja tok. Preslikani tip gubi svojstvo `description` u istoj izmeni u kojoj ga gubi server, pa prevodilac prijavljuje svaki šablon i svaku klasu na klijentu koji to svojstvo čitaju. Greška se otkriva pri prevođenju, sa spiskom mesta koja treba dopuniti, umesto u internet čitaču kao prazno polje.

## Tip koji postoji samo na klijentu

Postoje tipovi koje server ne poznaje. Izlaz `editComment` iz prethodne lekcije nosi identifikator komentara i nov tekst, pa mu treba tip sa dva svojstva. Takav tip nije DTO struktura, jer ne prelazi granicu sa serverom, i ne ulazi u direktorijum `api`. Izvozi se iz datoteke komponente koja ga prijavljuje:

```ts
export interface CommentEdit {
  commentId: string;
  text: string;
}
```

U datom kodu treba uočiti sledeće:

- Tip živi u datoteci `blog-comments.ts`, pored izlaza koji ga koristi. Stranica koja na izlaz vezuje metodu uvozi tip iz iste datoteke iz koje uvozi komponentu.
- Ne postoji direktorijum `models` sa tipovima modula. Tip koji koristi jedna komponenta stoji u toj komponenti, a tip koji dolazi sa servera stoji u direktorijumu `api`. Trećeg mesta nema.
