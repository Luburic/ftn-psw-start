Kartica iz prethodne lekcije ispisuje broj sviđanja unutar teksta dugmeta. Kada isti natpis treba i naslovu kartice, a uz broj treba i pravilo o tome kako se reč menja (jedno „sviđanje", više „sviđanja"), isti izraz se ponavlja na dva mesta u šablonu. Vrednost koja se računa iz drugih signala treba da živi na jednom mestu u klasi. Ovde upoznajemo signal čija se vrednost računa iz drugih signala i pravilo o signalu koji čuva niz ili objekat.

## Izvedeni signal

**Izvedeni signal** (engl. *computed signal*) je signal čija se vrednost računa funkcijom iz drugih signala i koji se sam ponovo računa kada se neki od njih promeni. Sledeći kod prikazuje izvedeni signal za tekst dugmeta:

```ts
protected readonly likes = signal(0);
protected readonly likesLabel = computed(() => {
  const n = this.likes();
  return `${n} ${n === 1 ? 'sviđanje' : 'sviđanja'}`;
});
```

```html
<button type="button" (click)="like()">{{ likesLabel() }}</button>
```

U datom kodu treba uočiti sledeće:
- Poziv `computed` prima funkciju koja čita druge signale i vraća vrednost.
- Izvedeni signal se čita pozivom kao i običan, ali nema `set` ni `update`. Njegova vrednost se menja samo kada se promeni signal od kog zavisi.
- Vrednost se računa tek pri prvom čitanju i zatim pamti. Dok se `likes` ne promeni, svako čitanje `likesLabel` vraća zapamćenu vrednost bez ponovnog računanja.

Pravilo o jednini i množini sada živi na jednom mestu. Gde god natpis treba, na dugmetu, u naslovu kartice, šablon čita `likesLabel()`, umesto da isti izraz sa uslovom ponavlja uz svaku pojavu.

## Nizovi i objekti u signalu

Signal obaveštava čitaoce kada metoda `set` ili `update` dodeli novu vrednost. Kada signal čuva niz ili objekat, izmena unutar te vrednosti ne prolazi kroz signal i čitaoci ne dobijaju obaveštenje. Sledeći kod prikazuje pogrešan i ispravan način dodavanja oznake:

```ts
protected readonly tags = signal<string[]>([]);

protected addTagWrong(tag: string): void {
  this.tags().push(tag);
}

protected addTag(tag: string): void {
  this.tags.update((tags) => [...tags, tag]);
}
```

U datom kodu treba uočiti sledeće:
- Metoda `addTagWrong` menja niz koji signal već drži. Signal ne zna za tu izmenu, jer `set` ni `update` nisu pozvani, pa spisak na ekranu ostaje star.
- Metoda `addTag` pravi nov niz sa dodatom oznakom i predaje ga signalu. Signal vidi novu vrednost i obaveštava čitaoce.

Izmena postojećeg niza je najčešći uzrok prikaza koji se ne osvežava.

## Kartica sa oznakama

Povežimo pojmove u karticu koja prikazuje oznake ture i dozvoljava dodavanje nove:

```ts
@Component({
  selector: 'app-tour-card',
  templateUrl: './tour-card.html',
  styleUrl: './tour-card.scss',
})
export class TourCard {
  protected readonly name = 'Stari grad';
  protected readonly tags = signal<string[]>([]);
  protected readonly tagsLabel = computed(() => this.tags().join(', '));

  protected addTag(tag: string): void {
    this.tags.update((tags) => [...tags, tag]);
  }
}
```

```html
<article>
  <h3>{{ name }}</h3>
  <p>{{ tagsLabel() }}</p>
  <button type="button" (click)="addTag('istorija')">Dodaj oznaku</button>
</article>
```

Kada korisnik klikne na dugme, dešava se sledeće:
1. Vezivanje događaja poziva metodu `addTag`.
2. Metoda predaje signalu `tags` nov niz, pa signal obaveštava čitaoce.
3. Izvedeni signal `tagsLabel` je čitalac signala `tags`, pa se označava za ponovno računanje.
4. Šablon je čitalac signala `tagsLabel`, pa radni okvir ponovo iscrtava komponentu. Pri iscrtavanju šablon čita `tagsLabel`, koji tek tada računa novu vrednost.

Signal drži izvor stanja, izvedeni signal drži vrednost izračunatu iz njega. Kad god vrednost u šablonu možeš da izvedeš iz drugih signala: natpis, filtriran spisak, zbir uradi to izvedenim signalom. Tako stanje ima jedan izvor istine, a vrednosti koje iz njega slede same se održavaju u skladu sa njegovom promenom.
