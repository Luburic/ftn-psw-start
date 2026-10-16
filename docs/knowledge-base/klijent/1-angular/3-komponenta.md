Klijentske veb aplikacije danas se najčešće grade od **komponenti** tj. samostalnih delova stranice od kojih se, kao od cigli, sklapa ceo prikaz. Komponenta sabira HTML prikaz, CSS stilove i JS ponašanje za takvu jednu ciglu u jednom direktorijumu.

U veb aplikacijama pisanim u čistom JS-u, sadržaj stranice menjamo direktnom izmenom DOM stabla. Sledeći kod prikazuje karticu ture koja ispisuje naziv ture i ima dugme za objavu ture koje se nakon klika onemogućava:

```html
<h3 id="name"></h3>
<button id="publish" type="button">Objavi</button>
```

```js
let published = false;

const name = document.getElementById('name');
const button = document.getElementById('publish');

name.textContent = 'Stari grad';
button.disabled = published;
button.addEventListener('click', () => {
  published = true;
  button.disabled = published;
});
```

U datom kodu treba uočiti sledeće:
- HTML opisuje samo strukturu. Sadržaj i stanje elemenata postavlja JavaScript, pa se iz HTML-a ne vidi šta će biti prikazano. Ovo nam omogućuje da isti šablon primenimo za bilo kakvu turu.
- Svaki element koji menjamo prvo moramo da pronađemo, za šta korisitmo `document.getElementById` i modifikujemo njegov:
   - sadržaj (`textContent`) u slučaju postavke naziva ture ili
   - atribut (`disabled`) kada želimo da onemogućimo dugme.
- Funkcija `addEventListener` nam omogućuje da reagujemo na događaje (u primeru, za "klik") i izvršimo željenu reakciju.

Prethodni kod je relativno jednostavan, ali samo zato što radi sa malo podatkaa. U složenim stranicama bismo morali da implementiramo stotine linija JS koda da postignemo željenu dinamiku. Angular značajno pojednostavljuje ovakvu vrstu posla:
- U šablonu komponente uz element zapisujemo koji član TS klase određuje njegov sadržaj, koristeći *interpolaciju*.
- U šablonu za atribut navodimo koji član TS klase određuje njegovu vrednost, koristeći *vezivanje svojstva*.
- U šablonu za događaj navodimo koja funkcija TS klase određuje logiku koja će se pozvati, koristeći *vezivanje događaja*.

Angular zatim sam ažurira HTML i poziva odgovarajuće funkcije kako korisnik interaguje sa stranicom.

## Interpolacija

**Interpolacija** (engl. *interpolation*) je zapis `{{ izraz }}` u HTML šablonu, koji na tom mestu ispisuje vrednost izraza kao tekst. Izraz najčešće čita polje ili poziva metodu klase. Interpolacija zamenjuje dodelu `name.textContent` iz uvoda. Sledeći kod prikazuje komponentu kartice ture koja ispisuje naziv i opis:

- Klasa (`tour-card.ts`):
```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
}
```

- Šablon (`tour-card.html`):
```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
</article>
```

- Stilovi (`tour-card.scss`):
```scss
article {
  padding: 10px;
  border: 1px solid black;
  border-radius: 8px;
}

h3 {
  color: red;
}
```

U datom kodu treba uočiti sledeće:
- Šablon vidi članove klase po nazivu. Polje `name` u klasi i `name` u šablonu su isto polje.
- Šablon vidi članove označene sa `public` i `protected`, a ne vidi one označene sa `private`. Angular prevodilac proverava i šablon i prijavljuje grešku ako šablon pristupa privatnom članu ili članu koji ne postoji.
- Stilovi iz `tour-card.scss` važe samo za ovu komponentu. Angular ih ograničava na njen šablon, pa selektori `article` i `h3` ovde ne utiču na iste elemente u drugim komponentama.

Za dati primer, DOM stablo bi prikazalo:
```html
<article>
  <h3>Stari grad</h3>
  <p>Šetnja kroz tvrđavu.</p>
</article>
```

U prikazu bi "Stari grad" bio crvene boje, a čitav sadržaj bi bio uokviren sa crnom bordurom.

## Vezivanje svojstva

Element u DOM stablu je objekat čija svojstva određuju njegovo stanje, poput `disabled` kod dugmeta ili `value` kod polja za unos. U uvodu smo svojstvo `disabled` postavljali iz JavaScript-a, jer atribut u HTML-u nosi samo fiksan tekst koji ne prati podatak. **Vezivanje svojstva** (engl. *property binding*) se ostvaruje kroz zapis `[svojstvo]="izraz"` na elementu, koji svojstvu elementa dodeljuje vrednost izraza. Sledeći kod prikazuje dugme čija dostupnost zavisi od polja klase:

```ts
export class TourCard {
  protected readonly published = true;
}
```

```html
<button type="button" [disabled]="published">Objavi</button>
```

U datom kodu treba uočiti sledeće:
- Uglaste zagrade oko `disabled` znače da se desna strana tumači kao izraz, a ne kao tekst. Bez zagrada bi `disabled="published"` bio običan HTML atribut sa tekstom `published`.
- Kada je `published` tačno, dugme je onemogućeno.

## Vezivanje događaja

**Vezivanje događaja** (engl. *event binding*) se ostvaruje kroz zapis `(događaj)="izraz"` na elementu, koji izvršava izraz kada se na elementu desi navedeni događaj. Izraz najčešće poziva metodu klase. Zapis zamenjuje poziv `addEventListener` iz uvoda. Sledeći kod prikazuje dugme koje poziva metodu:

```ts
export class TourCard {
  protected logClick(event: Event): void {
    console.log(event.target);
  }
}
```

```html
<button type="button" (click)="logClick($event)">Objavi</button>
```

U datom kodu treba uočiti sledeće:
- Zagrade oko `click` znače da je desna strana izraz koji se izvršava pri kliku.
- Reč `$event` je objekat događaja koji pravi internet čitač. Prosleđujemo ga metodi kada joj treba. Kada metodi ne treba, poziv je `(click)="logClick()"`.

## Kartica ture

Povežimo pojmove u jednu komponentu. Kartica prikazuje naziv i opis ture i ima dugme koje je onemogućeno kada je tura već objavljena:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
  protected published = false;

  protected publish(): void {
    this.published = true;
  }
}
```

```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
  <button type="button" [disabled]="published" (click)="publish()">Objavi</button>
</article>
```

U datom kodu treba uočiti sledeće:
- Interpolacija ispisuje dva polja, vezivanje svojstva veže dostupnost dugmeta za treće polje, a vezivanje događaja poziva metodu koja to polje menja.
- Polje `published` nije `readonly`, jer ga metoda menja.

## Upotreba komponente

Napravljena komponenta se ne prikazuje sama od sebe. Da bi se kartica pojavila na stranici, treba da je upotrebimo u šablonu druge komponente koja se prikazuje. Sledeći kod prikazuje korensku komponentu koja koristi karticu:

```ts
import { Component } from '@angular/core';
import { TourCard } from './tour-card/tour-card';

@Component({
  selector: 'app-root',
  imports: [TourCard],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
```

```html
<main>
  <app-tour-card />
</main>
```

U datom kodu treba uočiti sledeće:
- `@Component` je proširen sa poljem `imports`, gde navodimo komponente koje HTML šablon `App` komponente sme da koristi. Klasu `TourCard` najpre uvozimo naredbom `import` (drugi red TS-a), a zatim je navodimo u nizu ispod. Ako to izostavimo, prevodilac prijavljuje grešku da ne poznaje element `app-tour-card`.
- Naziv elementa `app-tour-card` odgovara vrednosti `selector` iz dekoratora kartice. Na tom mestu Angular iscrtava šablon kartice, isto kao što šablon korenske komponente iscrtava unutar elementa `app-root`.

Pošto `index.html` učitava `App` komponentu, sve naše komponente će imati posrednu ili neposrednu vezu sa korenskom komponentom.

Kada pokrenemo aplikaciju i kliknemo na dugme, metoda `publish` se izvršava, polje `published` dobija vrednost `true`, a dugme postaje onemogućeno. Prikaz se automatski osvežio jer je promenu izazvao događaj iz šablona (klik).

## Prikaz ne prati svaku promenu

Logično je pomisliti da Angular stalno posmatra polje `published` i osvežava prikaz čim se ono promeni. Međutim, Angular osvežava prikaz komponente samo kada zna da se nešto promenilo, a najčešći povod za to je upravo događaj iz šablona, na primer klik, unos teksta i slično. Pošto je Angular sam postavio osluškivač za taj događaj, on zna da posle njega treba osvežiti prikaz. Pri tome Angular ne iscrtava komponentu iznova, već menja samo one delove stranice čija se vrednost promenila.

Problem nastaje kada se polje promeni bez takvog događaja. Zamislimo da se tura objavi van šablona, tj. da je taj podatak stigao sa servera. To ovde simuliramo tajmerom koji posle jedne sekunde postavi `published` na `true`, a dugme ovoga puta nema klik:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly description = 'Šetnja kroz tvrđavu.';
  protected published = false;

  constructor() {
    setTimeout(() => {
      this.published = true;
      console.log('published:', this.published);
    }, 1000);
  }
}
```

```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ description }}</p>
  <button type="button" [disabled]="published">Objavi</button>
</article>
```

U datom kodu treba uočiti sledeće:
- Posle jedne sekunde polje `published` zaista dobija vrednost `true`. U to se uveravamo ispisom u konzoli unutar tajmera.
- Ipak, dugme ostaje omogućeno. Prikaz je zaostao za podatkom, jer promenu nije izazvao nijedan događaj iz šablona, pa Angular ne zna da treba da osveži karticu.

Prikaz prati promene samo kada uz promenu ide i događaj iz šablona. Nama treba polje čiju svaku promenu Angular primeti, bez obzira odakle promena dolazi. Navedeni zahtev ispunjavaju **signali** i njima se bavimo u [narednoj lekciji o signalima](4-signali.md).
