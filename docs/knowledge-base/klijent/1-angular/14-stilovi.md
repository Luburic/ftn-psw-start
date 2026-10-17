Komponentu čine tri datoteke, gde smo datoteku sa stilovima za sada samo pomenuli. Ovde ispitujemo gde se stilovi definišu, odnosno u koju datoteku treba da ih smestimo. Datoteke stilova imaju ekstenziju `.scss`. SCSS je proširenje CSS-a koje alat za prevođenje aplikacije pretvara u običan CSS.

## Stilovi komponente

Stilovi iz datoteke na koju dekorator upućuje podešavanjem `styleUrl` važe samo za šablon te komponente. Sledeći kod prikazuje jedno pravilo iz datoteke stilova komponente koja prikazuje spisak komentara:

```scss
.comments > li {
  border-top: 1px solid gray;
  padding-top: 12px;
}
```

```html
<ul class="comments">
  <li>Prvi komentar</li>
  <li>Drugi komentar</li>
</ul>
```

U datom kodu treba uočiti sledeće:

- Radni okvir selektor prepisuje u oblik `.comments[_ngcontent-x] > li[_ngcontent-x]`, gde je `_ngcontent-x` atribut koji imaju samo elementi ovog šablona. Druga komponenta sme da ima klasu `comments` sa drugim pravilima, bez preklapanja.

## Globalni stilovi

Stilovi koji važe za ceo dokument žive u datoteci `styles.scss`, koju alat za prevođenje uključuje u dokument kao običnu CSS datoteku. Sledeći kod prikazuje datoteku `styles.scss` koja globalne stilove deli na četiri dela:

```scss
@use 'styles/tokens';
@use 'styles/base';
@use 'styles/layout';
@use 'styles/components';
```

Naredba `@use` učitava drugu datoteku iz direktorijuma `styles` pored ove datoteke i njen sadržaj ugrađuje na ovo mesto. Četiri učitane datoteke drže redom:

1. `_tokens.scss`, vrednosti zajedničke za celu aplikaciju.
2. `_base.scss`, pravila za osnovne HTML elemente, poput `body` i naslova.
3. `_layout.scss`, klase za raspored, `container`, `stack` i `grid`.
4. `_components.scss`, klase za elemente koji se ponavljaju, `card`, `button`, `field`, `table`, `error` i `meta`.

## Tokeni

**Token** (engl. *design token*) je vrednost boje, razmaka, poluprečnika ili veličine fonta imenovana kao CSS promenljiva na korenskom elementu dokumenta, koju sva ostala pravila čitaju umesto da vrednost ponavljaju. Sa ovim globalnim promenljivama možemo na jednom mestu da izmenimo elemente stila cele aplikacije i osiguravamo konzistentnost (npr. da se svuda uvede margina od 12px, a ne negde 10px ili 14px). Sledeći kod prikazuje deo datoteke `_tokens.scss` i pravilo iz datoteke `_components.scss` koje tokene čita:

```scss
// _tokens.scss
:root {
  --color-surface: #ffffff;
  --color-border: #e2e2e6;
  --color-accent: #2f6df6;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --radius: 0.5rem;
}
```

```scss
// _components.scss
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: var(--space-4);
}
```

U datom kodu treba uočiti sledeće:

- Selektor `:root` je element `html`, pa je promenljiva dostupna svakom elementu ispod njega, i unutar šablona komponenti.
- Kada se `--color-accent` promeni u ovoj datoteci, menja se svaki element koji je čita.

## Izbor mesta za nov stil

Kada elementu treba određen izgled, postavljamo pitanje: **postoji li globalna klasa za taj izgled?**
- Ako postoji, na element se stavlja ta klasa.
- Ako ne postoji, a izgled treba samo toj komponenti, pravilo se piše u njenu datoteku stilova. Tamo ne može da dotakne nikog drugog, jer radni okvir selektor sužava na njen šablon.
- Ako ne postoji, a isti izgled treba na više mesta, definiše se nova globalna klasa. To više nije izmena jedne komponente, nego izmena koja menja izgled cele aplikacije, pa se donosi kao i svako proširenje zajedničkog koda, u dogovoru sa timom koji te datoteke održava. Isto važi i za nov token.

U sva tri slučaja vrednosti se čitaju iz postojećih tokena. Umesto `padding: 12px` piše `padding: var(--space-3)`, a umesto `color: #2f6df6` piše `color: var(--color-accent)`.

## Kartica ture

Povežimo pojmove u karticu ture iz lekcije o komponenti, sada i sa njenom datotekom stilova. Kartica ima okvir kao i svaka druga kartica u aplikaciji, a naslov joj je obojen pravilom koje postoji samo u njenoj datoteci:

```html
<article class="card">
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
  <button type="button" class="button" [disabled]="published" (click)="publish()">Objavi</button>
</article>
```

```scss
h3 {
  color: var(--color-accent);
}
```

Kada internet čitač iscrta karticu, svaki element dobija stilove iz više izvora:

1. Element `article` nosi globalnu klasu `card`, pa iz datoteke `_components.scss` dobija pozadinu, okvir, zaobljene uglove i unutrašnji razmak, sve sa vrednostima iz tokena.
2. Na naslov `h3` prvo deluje globalno pravilo za naslove iz datoteke `_base.scss`, koje mu daje margine i visinu reda.
3. Isti `h3` nosi i atribut ove komponente, pa na njega deluje i pravilo iz datoteke `tour-card.scss`, koje mu daje boju iz tokena `--color-accent`. Naslov `h3` u nekoj drugoj komponenti taj atribut nema, pa ostaje u podrazumevanoj boji teksta.
4. Dugme nosi globalnu klasu `button` i time izgleda kao svako dugme u aplikaciji, bez ijednog pravila u datoteci ove komponente.