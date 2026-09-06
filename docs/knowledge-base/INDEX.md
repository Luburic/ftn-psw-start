# Indeks baze znanja

Jedna linija po dokumentu: putanja, opis i preduslovi (dokumenti koje čitalac treba prethodno da poznaje). Direktorijumi su segmenti, a redni brojevi u nazivima daju redosled čitanja. Svaki direktorijum ima `README.md` sa uvodom u segment i mapom svojih lekcija. Dokumenti su lekcije sa namerno pojednostavljenim primerima, osim onih označenih kao normativni.

## ASP.NET Core

- `server/1-aspnet/README.md` — Šta radni okvir preuzima od serverske aplikacije i kako se razlikuje od biblioteke. Uvod u segment. Preduslovi: nema.
- `server/1-aspnet/1-aspnet.md` — Šta ASP.NET Core radi pri obradi zahteva i kako izgleda najmanja aplikacija dovoljna za rad. Preduslovi: nema.
- `server/1-aspnet/2-kontroleri.md` — Kontroleri i akcije: rutiranje, vezivanje modela, povratna vrednost akcije i middleware. Preduslovi: `server/1-aspnet/1-aspnet.md`.
- `server/1-aspnet/3-registracija-zavisnosti.md` — Kontejner zavisnosti: kako pravi objekte, životni vekovi, pozadinski servisi i metoda proširenja kojom modul grupiše svoje registracije. Preduslovi: `server/1-aspnet/1-aspnet.md`.
- `server/1-aspnet/4-asinhrono-programiranje.md` — Zašto server ostaje bez niti pri sinhronom čekanju, `async` i `await` i redosled poziva. Preduslovi: nema.

## Arhitektura modula

- `server/2-arhitektura-modula/README.md` — Čista arhitektura: četiri sloja, njihove odgovornosti i smer zavisnosti. Uvod u segment. Preduslovi: `server/1-aspnet/README.md`.

### Domenski sloj

- `server/2-arhitektura-modula/1-domenski-sloj/README.md` — Uvod u domenski sloj i mapa taktičkih obrazaca. Preduslovi: `server/2-arhitektura-modula/README.md`.
- `server/2-arhitektura-modula/1-domenski-sloj/1-takticki-obrasci.md` — Gde živi domenska logika i kako su objekti povezani: anemičan i bogat model, povezan graf i graf isečen na celine. Preduslovi: nema.
- `server/2-arhitektura-modula/1-domenski-sloj/2-vrednosni-objekat.md` — Vrednosni objekat: identitet po vrednostima, nepromenljivost, validnost pri kreiranju. Preduslovi: `1-takticki-obrasci.md`.
- `server/2-arhitektura-modula/1-domenski-sloj/3-entitet.md` — Entitet: životni ciklus, nepromenljiv identifikator, invarijante i njihovo prijavljivanje izuzetkom. Preduslovi: `2-vrednosni-objekat.md`.
- `server/2-arhitektura-modula/1-domenski-sloj/4-agregat.md` — Agregat: granica konzistentnosti, koren kao jedina tačka izmene, veza ka drugim agregatima identifikatorom. Preduslovi: `3-entitet.md`.
- `server/2-arhitektura-modula/1-domenski-sloj/5-domenski-servis.md` — Domenski servis: pravilo koje zahteva uvid u više agregata. Preduslovi: `4-agregat.md`.

### Aplikacioni sloj

- `server/2-arhitektura-modula/2-aplikacioni-sloj/README.md` — Uvod u aplikacioni sloj i mapa lekcija. Preduslovi: `server/2-arhitektura-modula/1-domenski-sloj/README.md`.
- `server/2-arhitektura-modula/2-aplikacioni-sloj/1-komande-i-upiti.md` — Princip razdvajanja komandi od upita, oblik komandne i upitne klase, komanda nad više agregata. Preduslovi: `server/2-arhitektura-modula/1-domenski-sloj/4-agregat.md`.
- `server/2-arhitektura-modula/2-aplikacioni-sloj/2-aplikacioni-servis.md` — Struktura aplikacionog sloja u projektu i tri oblika metoda servisa, sa postupkom izbora oblika za nov zahtev. Preduslovi: `1-komande-i-upiti.md`.
- `server/2-arhitektura-modula/2-aplikacioni-sloj/3-dto-i-mapiranje.md` — DTO strukture na granici sloja, prevođenje ulazne strukture u domenski objekat, popunjavanje izlazne projekcijom ili maperom. Preduslovi: `2-aplikacioni-servis.md`.

### Infrastrukturni sloj

- `server/2-arhitektura-modula/3-infrastrukturni-sloj/README.md` — Uvod u infrastrukturni sloj: repozitorijumi, konektorske klase i lokalni tehnički stručnjaci. Preduslovi: `server/2-arhitektura-modula/2-aplikacioni-sloj/README.md`.
- `server/2-arhitektura-modula/3-infrastrukturni-sloj/1-orm.md` — Objektno-relaciono mapiranje i Entity Framework Core naspram ručnog ADO.NET koda. Preduslovi: nema.
- `server/2-arhitektura-modula/3-infrastrukturni-sloj/2-efc-kontekst-i-model.md` — Kontekstna klasa i model mapiranja: konvencije, konfiguracija po agregatu, rehidracija. Preduslovi: `1-orm.md`, `server/2-arhitektura-modula/1-domenski-sloj/4-agregat.md`.
- `server/2-arhitektura-modula/3-infrastrukturni-sloj/3-migracije.md` — Migracije: generisanje iz razlike prema snimku modela, primena pri pokretanju aplikacije, početni podaci. Preduslovi: `2-efc-kontekst-i-model.md`.
- `server/2-arhitektura-modula/3-infrastrukturni-sloj/4-repozitorijumi.md` — Repozitorijum agregata, učitavanje povezanih objekata, praćenje promena i repozitorijum za čitanje. Preduslovi: `2-efc-kontekst-i-model.md`, `server/2-arhitektura-modula/2-aplikacioni-sloj/1-komande-i-upiti.md`.
- `server/2-arhitektura-modula/3-infrastrukturni-sloj/5-jedinica-posla.md` — Jedinica posla: kontekst kao jedinica posla, repozitorijum bez čuvanja, put jedne komande. Preduslovi: `4-repozitorijumi.md`.
- `server/2-arhitektura-modula/3-infrastrukturni-sloj/6-ostali-infrastrukturni-servisi.md` — Konektorske klase i lokalni tehnički stručnjaci: sposobnost kroz drugi sistem ili kroz biblioteku, oblik interfejsa tehničke sposobnosti. Preduslovi: `5-jedinica-posla.md`, `server/2-arhitektura-modula/1-domenski-sloj/5-domenski-servis.md`.

### API sloj i zaokruženje

- `server/2-arhitektura-modula/4-api-sloj.md` — API sloj: kontroler po grupi slučajeva korišćenja, tri koraka akcije, identifikator korisnika iz zahteva. Preduslovi: `server/2-arhitektura-modula/2-aplikacioni-sloj/README.md`, `server/1-aspnet/2-kontroleri.md`.
- `server/2-arhitektura-modula/5-čista-arhitektura.md` — Sva četiri sloja zajedno: put jedne komande i jednog upita, odgovornosti po slojevima i pravilo o zavisnostima. Preduslovi: `server/2-arhitektura-modula/3-infrastrukturni-sloj/README.md`, `4-api-sloj.md`.

## Modularni monolit

- `server/3-modularni-monolit/README.md` — Modularni monolit i feature modul: granica modula, glavna aplikacija koja module sastavlja. Uvod u segment. Preduslovi: `server/2-arhitektura-modula/README.md`.
- `server/3-modularni-monolit/1-kontrakti.md` — Kontrakt kao javna površina modula prema drugim modulima: interfejs i minimalne DTO strukture, implementacija i poziv kroz kontejner zavisnosti. Preduslovi: `README.md`, `server/2-arhitektura-modula/2-aplikacioni-sloj/README.md`.
- `server/3-modularni-monolit/2-gradivni-elementi.md` — Zajedničko jezgro, gradivni elementi po slojevima, platformski radni okvir i promocija koda u jezgro. Preduslovi: `README.md`.
- `server/3-modularni-monolit/3-arhitektonski-testovi.md` — Arhitektonski testovi: pravila o zavisnostima kao automatski testovi i tri vrste pravila sa primerom za svaku. Preduslovi: `server/2-arhitektura-modula/5-čista-arhitektura.md`, `2-gradivni-elementi.md`.

## Testovi

- `testovi/xunit.md` — Normativan dokument. Automatsko testiranje sa xUnit i pomoćni kod za integracione testove. Preduslovi: nema.
