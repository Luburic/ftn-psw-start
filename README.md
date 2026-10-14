# Explorer

Explorer je nastavni projekat: platforma za istraživanje turističkih sadržaja koju jedan tim studenata razvija kao modularni monolit (ASP.NET Core, Angular, PostgreSQL). Pre prvog rada pratite protokol [Podešavanje radne stanice](docs/protocols/podesavanje-radne-stanice.md). Gradivo koje pokriva predmet i koje razvijamo kroz rad na projektu je u [bazi znanja](docs/knowledge-base/INDEX.md).

## Veštine za rad sa agentom

Projekat razvijate uz agenta Claude Code i uz pomoć veština koje smo definisali. Veštine u isto vreme razvijaju funkcionalnosti i vas podučavaju, gde je naš glavni cilj da razvijemo vaše sposobnosti.

Jedan student vodi jednu funkcionalnost od zahteva do pull request-a, ovim redom:

| Veština | Šta joj dajete | Šta je rezultat |
|---|---|---|
| `/psw-zahtevi` | Tekst zahteva | Po jednu specifikaciju za svaku funkcionalnost, sa namerom, ponašanjem i pravilima |
| `/psw-dizajn` | Jednu specifikaciju | Istu specifikaciju dopunjenu dizajnom: korisničko iskustvo, domenski model, tok poruka i dizajn testova |
| `/psw-implementacija` | Specifikaciju sa odobrenim dizajnom | Kod na novoj grani, lokalni commit i siže koji će vam pomoći da formirate opis pull request-a |

Tokom implementacije agent piše kod u pozadini, a vi predviđate kako će taj kod izgledati i zatim predviđanje poredite sa napisanim kodom. Slanje grane i otvaranje pull request-a ostaju vaš posao, po protokolu [Rad sa granama i pull request-ovima](docs/protocols/rad-sa-granama.md).

Ostale veštine pozivate po potrebi:

| Veština | Kada je pozivate |
|---|---|
| `/psw-pitaj` | Kada vam deo koda nije jasan. Agent objašnjava šta kod radi i zašto je tako napisan, pojmovima iz baze znanja. |
| `/psw-ucenje` | Kada se spremate za proveru znanja na vežbama i usmeni ispit na kraju. Agent postavlja pitanja o kodu koji ste sami napisali. Uz poziv možete navesti deo gradiva, na primer `/psw-ucenje klijent` koje želite posebno da obradite. |
| `/psw-izvoz-razgovora` | Ako učestvujete u istraživanju (pratite obaveštenje na Canvasu). Veština pakuje vaše razgovore sa agentom u jednu arhivu. |

Objašnjenje agenta koje izlazi iz okvira baze znanja počinje oznakom `Van baze znanja:`.
