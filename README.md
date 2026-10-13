# Explorer

Explorer je nastavni projekat: platforma za istraživanje turističkih sadržaja koju jedan tim studenata razvija kao modularni monolit (ASP.NET Core, Angular, PostgreSQL). Pre prvog rada pratite protokol [Podešavanje radne stanice](docs/protocols/podesavanje-radne-stanice.md). Ostali protokoli su u direktorijumu [docs/protocols](docs/protocols/README.md), a gradivo u [bazi znanja](docs/knowledge-base/INDEX.md).

## Veštine za rad sa agentom

Projekat razvijate uz agenta Claude Code, kroz veštine koje pozivate po imenu. Veštine namerno usporavaju rad: pre nego što agent ponudi rešenje, traži vaše obrazloženje, jer je cilj da naučite gradivo, a funkcionalnost je put do toga. Nesavršen odgovor je dovoljan da se razgovor nastavi.

Jedan student vodi jednu funkcionalnost od zahteva do spojenog pull request-a, ovim redom:

| Veština | Šta joj dajete | Šta ostavlja |
|---|---|---|
| `/psw-zahtevi` | Tekst zahteva | Po jednu specifikaciju za svaku funkcionalnost, sa namerom, ponašanjem i pravilima |
| `/psw-dizajn` | Jednu specifikaciju | Istu specifikaciju dopunjenu dizajnom: tok korišćenja, domenski model, operacije i testovi |
| `/psw-implementacija` | Specifikaciju sa odobrenim dizajnom | Kod na novoj grani od `development`-a, lokalni commit i pregled za opis pull request-a |

Tokom implementacije agent piše kod u pozadini, a vi predviđate kako će taj kod izgledati i zatim predviđanje poredite sa napisanim kodom. Slanje grane i otvaranje pull request-a ostaju vaš posao, po protokolu [Rad sa granama i pull request-ovima](docs/protocols/rad-sa-granama.md).

Ostale veštine pozivate po potrebi:

| Veština | Kada je pozivate |
|---|---|
| `/psw-pitaj` | Kada vam deo koda nije jasan. Agent objašnjava šta kod radi i zašto je tako napisan, pojmovima iz baze znanja. |
| `/psw-ucenje` | Kada se spremate za usmeni ispit. Agent postavlja pet pitanja o kodu koji ste sami napisali. Uz poziv možete navesti deo gradiva, na primer `/psw-ucenje klijent`. |
| `/psw-izvoz-razgovora` | Ako učestvujete u istraživanju. Veština pakuje vaše razgovore sa agentom u jednu arhivu. |

Veštine čuvaju svoje radne datoteke u direktorijumu `.workspace/`, koji se ne commit-uje. Specifikaciju koju preuzima drugi član tima pošaljite mu kao datoteku.

Objašnjenje agenta koje izlazi iz okvira baze znanja počinje oznakom `Van baze znanja:`.
