# Spec template

One file per feature: `.workspace/specs/<modul>-<kratak-opis>.md`, lowercase and without
diacritics, for example `exploration-ocena-ture.md`. The branch later takes the same
words: `exploration/ocena-ture`.

The file is written in Serbian, with code identifiers in English. `/psw-zahtevi` writes
the header and `## Zahtevi`. `/psw-dizajn` adds `## Dizajn`. Headings, labels and tags
are fixed; write them exactly as below.

```
# <Naziv>

**Status:** <Zahtevi odobreni | Dizajn odobren | Implementirano>
**Modul:** <Exploration | Games | Social | Payment>

## Zahtevi

### Ko i zašto

<Jedna ili dve rečenice: ko ima korist i šta time postiže.>

### Ponašanje

1. <radnja korisnika> → <šta korisnik vidi>

### Pravila

- P1. <tvrdnja koja može da se proveri>

### Pretpostavke i pitanja

- Pretpostavka: <šta je uzeto kao tačno>
- Pitanje za zainteresovanu stranu: <pitanje> Do odgovora važi: <pretpostavka>
- Zavisnost: modul <Ime> treba da ponudi <šta> kroz svoj kontrakt.

## Dizajn

### Tok korišćenja

Polazna stranica: <adresa i šta korisnik na njoj vidi>

1. <radnja korisnika> → <šta se prikazuje i koje je vrste>

Kraj: <gde korisnik ostaje i šta vidi>

**Odluke**
- <odluka u jednoj rečenici> (<oznaka>)

### Domenski model

<Agregat>
- <postojeći član koji je bitan za funkcionalnost>
- + <nov član>: <značenje>; čuva P1

**Odluke**
- <odluka u jednoj rečenici> (<oznaka>)

### Operacije

| Operacija | Vrsta | Grupa | Opis |
|---|---|---|---|
| <ime> | <komanda | upit> | <grupa slučajeva korišćenja> | <šta prima i šta menja ili vraća> |

**Odluke**
- <odluka u jednoj rečenici> (<oznaka>)

### Testovi

- <rečenica o domenu> (<jedinični | integracioni>; čuva <P1 | uspešan scenario>)

Greške i klijent:
- <greška svojstvena ovoj funkcionalnosti> → <šta korisnik vidi i gde>

**Odluke**
- <odluka u jednoj rečenici> (<oznaka>)
```

## Field notes

- **Naziv:** a short noun phrase naming the capability, not the implementation.
- **Status** moves forward only: `/psw-zahtevi` sets `Zahtevi odobreni`, `/psw-dizajn`
  sets `Dizajn odobren`, `/psw-implementacija` sets `Implementirano` and adds the line
  `**Grana:** <modul>/<kratak-opis>` under it.
- **Ko i zašto:** the role and the benefit they are after. The benefit is what changes
  for that person, never a restatement of the action.
- **Ponašanje:** the main flow as the user sees it. No pages, components, classes or
  endpoints; those belong to the design.
- **Pravila:** one checkable statement per line. The numbers are stable once written,
  because the design and the tests refer to them.
- **Pretpostavke i pitanja:** write `Nema.` when there are none. Use `Pretpostavka` when
  a wrong guess changes a detail, `Pitanje za zainteresovanu stranu` when it changes what
  gets built, and `Zavisnost` when the feature needs something another module does not
  yet offer.
- **Domenski model:** only the aggregates the feature touches. When nothing in the
  domain changes, write `Bez izmena domenskog modela.` and omit the rest of the section.
- **Odluke:** one line per decision the exchange settled. The tag is one of `student`,
  `predlog agenta`, `agent`, `pravilo projekta`, as `.claude/psw/exchange.md` defines
  them. A weaker design the student kept gets a second line:
  `- Kompromis: <šta je slabije i zašto je prihvaćeno>`.
