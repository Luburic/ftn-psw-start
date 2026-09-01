# Indeks baze znanja

Jedna linija po dokumentu: putanja, opis i preduslovi (dokumenti koje čitalac treba prethodno da poznaje). Dokumenti su lekcije sa namerno pojednostavljenim primerima.

## Server

- `server/aspnet.md` — Šta ASP.NET Core radni okvir radi za serversku aplikaciju pri obradi HTTP zahteva. Preduslovi: nema.
- `server/kontroleri.md` — Kontroleri i akcije: rutiranje, vezivanje parametara, formiranje odgovora i middleware. Preduslovi: `server/aspnet.md`.
- `server/registracija-zavisnosti.md` — Kontejner zavisnosti: kako pravi objekte, životni vekovi i oblici registracije. Preduslovi: `server/aspnet.md`.
- `server/asinhrono-programiranje.md` — Asinhrone operacije i async/await na klijentu i serveru. Preduslovi: nema.
- `server/orm.md` — Objektno-relaciono mapiranje i Entity Framework naspram ručnog ADO.NET koda. Preduslovi: nema.
- `server/maperi.md` — Maper i profil mapera (AutoMapper): prevođenje između domenskih objekata i DTO struktura. Preduslovi: `server/arhitektura/slojevi/2-aplikacioni-sloj.md`.

## Arhitektura

- `server/arhitektura/modularni-monolit.md` — Modularni monolit i feature moduli: granice, zajedničko jezgro, sastavljanje aplikacije. Preduslovi: nema.
- `server/arhitektura/čista-arhitektura.md` — Čista arhitektura: primer kroz sve slojeve, četiri odgovornosti i smer zavisnosti između slojeva. Preduslovi: `server/arhitektura/ddd/4-agregat.md`.
- `server/arhitektura/slojevi/1-domenski-sloj.md` — Domenski sloj: agregati i domenski servisi kao klase sloja. Preduslovi: `server/arhitektura/čista-arhitektura.md`, `server/arhitektura/ddd/4-agregat.md`.
- `server/arhitektura/slojevi/2-aplikacioni-sloj.md` — Aplikacioni sloj: aplikacioni servisi, DTO strukture, interfejsi tehničkih sposobnosti, komande i upiti. Preduslovi: `server/arhitektura/čista-arhitektura.md`, `server/arhitektura/slojevi/1-domenski-sloj.md`.
- `server/arhitektura/slojevi/3-infrastrukturni-sloj.md` — Infrastrukturni sloj: repozitorijumske, konektorske i stručnjačke klase. Preduslovi: `server/arhitektura/slojevi/2-aplikacioni-sloj.md`.
- `server/arhitektura/slojevi/4-api-sloj.md` — API sloj: kontrolerske klase kao adapteri protokola. Preduslovi: `server/arhitektura/slojevi/2-aplikacioni-sloj.md`, `server/kontroleri.md`.
- `server/arhitektura/komande-i-upiti.md` — Oblikovanje komandi i upita: tri slučaja i postupak odlučivanja za nov zahtev. Preduslovi: `server/arhitektura/slojevi/2-aplikacioni-sloj.md`, `server/arhitektura/slojevi/3-infrastrukturni-sloj.md`.
- `server/arhitektura/kontrakti.md` — Kontrakt kao javna površina modula prema drugim modulima: interfejs i minimalne DTO strukture. Preduslovi: `server/arhitektura/modularni-monolit.md`, `server/arhitektura/slojevi/2-aplikacioni-sloj.md`.
- `server/arhitektura/gradivni-elementi.md` — Gradivni elementi zajedničkog jezgra i platformski radni okvir. Preduslovi: `server/arhitektura/modularni-monolit.md`.
- `server/arhitektura/arhitektonski-testovi.md` — Arhitektonski testovi: pravila o zavisnostima kao automatski testovi i njihove vrste. Preduslovi: `server/arhitektura/čista-arhitektura.md`, `server/arhitektura/modularni-monolit.md`.

## DDD taktički obrasci

- `server/arhitektura/ddd/1-takticki-obrasci.md` — Domenski model, anemičan i bogat pristup, pregled taktičkih obrazaca. Preduslovi: nema.
- `server/arhitektura/ddd/2-vrednosni-objekat.md` — Vrednosni objekat: domenski značajna vrednost, nepromenljivost, validnost pri kreiranju. Preduslovi: `server/arhitektura/ddd/1-takticki-obrasci.md`.
- `server/arhitektura/ddd/3-entitet.md` — Entitet: domenski koncept sa identitetom i životnim ciklusom. Preduslovi: `server/arhitektura/ddd/2-vrednosni-objekat.md`.
- `server/arhitektura/ddd/4-agregat.md` — Agregat: granica konzistentnosti, koren agregata, referenciranje preko identifikatora. Preduslovi: `server/arhitektura/ddd/3-entitet.md`.
- `server/arhitektura/ddd/5-domenski-servis.md` — Domenski servis: pravilo koje zahteva uvid u više agregata. Preduslovi: `server/arhitektura/ddd/4-agregat.md`.

## Testovi

- `testovi/xunit.md` — Automatsko testiranje sa xUnit i pomoćni kod za integracione testove. Preduslovi: nema.
