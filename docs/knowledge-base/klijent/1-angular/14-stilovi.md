Komponentu čine tri datoteke, a treću, sa stilovima, do sada nismo razmatrali. Šabloni u projektu na elemente stavljaju klase koje nijedna komponenta ne definiše, na primer `card`, `stack` i `error`, a elementi ipak dobijaju izgled. Sa druge strane, kada u datoteci stilova jedne komponente napišemo `.card { padding: 0; }`, kartice u ostalim komponentama se ne menjaju. Čitalac zna CSS, u kom pravilo iz jedne datoteke važi za ceo dokument, pa mu ni jedno ni drugo nije očekivano. Ovde upoznajemo gde stilovi u projektu žive i kako se za nov stil bira mesto.

Datoteke stilova imaju nastavak `.scss`. SCSS je proširenje CSS-a koje alat za prevođenje aplikacije pretvara u običan CSS. Projekat od tog proširenja koristi jednu naredbu, koju upoznajemo uz globalne stilove, a sve ostalo u tim datotekama je običan CSS.

## Stilovi komponente

Stilovi iz datoteke na koju dekorator upućuje podešavanjem `styleUrl` važe samo za šablon te komponente. Pri prevođenju aplikacije radni okvir svakom elementu šablona dodaje atribut jedinstven za tu komponentu, a svaki selektor iz njene datoteke stilova prepisuje tako da zahteva taj atribut. Sledeći kod prikazuje, iz projekta, jedno pravilo iz datoteke stilova komponente koja prikazuje spisak komentara i deo njenog šablona:

```scss
.comments > li + li {
  border-top: 1px solid var(--color-border);
  padding-top: var(--space-3);
}
```

```html
<ul class="comments">
  <li>Prvi komentar</li>
  <li>Drugi komentar</li>
</ul>
```

U datom kodu treba uočiti sledeće:
- Radni okvir selektor prepisuje u oblik `.comments[_ngcontent-x] > li[_ngcontent-x] + li[_ngcontent-x]`, gde je `_ngcontent-x` atribut koji imaju samo elementi ovog šablona. Druga komponenta sme da ima klasu `comments` sa drugim pravilima, bez sudara.
- Elementi šablona druge komponente, i one koja se koristi unutar ovog šablona, taj atribut nemaju, pa pravilo na njih ne deluje.
- Prepisani selektor je određeniji od selektora bez atributa. Kada komponenta napiše `.card { padding: 0; }`, pravilo važi samo za kartice u njenom šablonu, a nad njima ima prednost nad pravilom koje važi za ceo dokument.
- Datoteka stilova komponente drži samo pravila koja nisu potrebna nigde drugde. Boja linije i razmak dolaze iz vrednosti zajedničkih za ceo projekat, koje upoznajemo niže.

## Globalni stilovi

Stilovi koji važe za ceo dokument žive u datoteci `styles.scss` iz stabla radnog prostora, koju alat za prevođenje uključuje u dokument kao običnu CSS datoteku, bez ikakve oznake u komponentama. Ona iz projekta ima četiri reda:

```scss
@use 'styles/tokens';
@use 'styles/base';
@use 'styles/layout';
@use 'styles/components';
```

Naredba `@use` je jedina SCSS naredba koju projekat koristi. Učitava drugu datoteku iz direktorijuma `styles` pored ove datoteke i njen sadržaj ugrađuje na ovo mesto. Datoteka čiji naziv počinje donjom crtom postoji samo da bi je druga datoteka učitala, a u naredbi se navodi bez donje crte i bez nastavka. Četiri učitane datoteke drže redom:
1. `_tokens.scss`, vrednosti zajedničke za ceo projekat.
2. `_base.scss`, pravila za osnovne HTML elemente, poput `body` i naslova.
3. `_layout.scss`, klase za raspored, `container`, `stack` i `grid`.
4. `_components.scss`, klase za elemente koji se ponavljaju, `card`, `button`, `field`, `table`, `error` i `meta`.

Ova pravila radni okvir ne prepisuje, pa važe za svaki element dokumenta, uključujući elemente unutar šablona komponenti. Zato klasa `card` iz šablona deluje bez ijednog reda u datoteci komponente. Klasu iz ovih datoteka zovemo globalna klasa.

## Tokeni

**Token** (engl. *design token*) je vrednost boje, razmaka, poluprečnika ili veličine fonta imenovana kao CSS promenljiva na korenskom elementu dokumenta, koju sva ostala pravila čitaju umesto da vrednost ponavljaju. Sledeći kod prikazuje, iz projekta, deo datoteke `_tokens.scss` i pravilo iz datoteke `_components.scss` koje tokene čita:

```scss
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
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: var(--space-4);
}
```

U datom kodu treba uočiti sledeće:
- Selektor `:root` je element `html`, pa je promenljiva dostupna svakom elementu ispod njega, i unutar šablona komponenti. Datoteka stilova komponente je čita na isti način, kao `var(--color-border)` u pravilu za komentare.
- Kada se `--color-accent` promeni u ovoj datoteci, menja se svaki element koji je čita.

## Izbor mesta za nov stil

Kada elementu treba izgled koji još nema, postupamo redom:
1. Kada neki element u projektu već ima taj izgled, na element se stavlja postojeća globalna klasa.
2. U suprotnom se pravilo piše u datoteku stilova komponente.
3. U svakom pravilu se boja, razmak i veličina čitaju iz postojećeg tokena, a ne upisuju kao nova vrednost.

## Kartica ture

Povežimo pojmove u karticu ture iz lekcije o komponenti, sada sa stilovima u datoteci `tour-card.scss`. Kartica dobija okvir kao svaka kartica u projektu, a naslov u boji naglaska, što nijedna druga komponenta nema:

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

Kada internet čitač primeni stilove na karticu, dešava se sledeće:
1. Element `article` nosi globalnu klasu `card`, pa dobija pozadinu, okvir, poluprečnik i unutrašnji razmak iz datoteke `_components.scss`, sa vrednostima iz tokena.
2. Na element `h3` deluje globalno pravilo za naslove iz datoteke `_base.scss`, koje mu daje margine i visinu reda.
3. Isti element nosi atribut komponente, pa na njega deluje i prepisani selektor `h3[_ngcontent-x]` iz datoteke `tour-card.scss`, koji boju čita iz tokena `--color-accent`. Element `h3` u drugoj komponenti taj atribut nema i zadržava boju teksta dokumenta.
4. Dugme nosi globalnu klasu `button`, pa izgleda kao svako dugme u projektu, bez ijednog pravila u datoteci komponente.
