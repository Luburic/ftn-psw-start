Kartica iz prethodne lekcije ispisuje broj sviđanja unutar teksta dugmeta. Kada isti tekst treba i naslovu kartice, a kada uz broj treba i pravilo o tome kako se reč menja, isti izraz se ponavlja na dva mesta u šablonu. Vrednost koja se računa iz drugih signala treba da živi na jednom mestu u klasi. Ovde upoznajemo signal čija se vrednost računa iz drugih signala i pravilo o signalu koji čuva niz ili objekat.

## Izvedeni signal

**Izvedeni signal** (engl. *computed signal*) je signal čija se vrednost računa funkcijom iz drugih signala i koji se sam ponovo računa kada se neki od njih promeni. Sledeći kod prikazuje izvedeni signal za tekst dugmeta:

```ts
protected readonly likes = signal(0);
protected readonly likesLabel = computed(() => `Sviđa mi se (${this.likes()})`);
```

```html
<button type="button" (click)="like()">{{ likesLabel() }}</button>
```

U datom kodu treba uočiti sledeće:
- Poziv `computed` prima funkciju koja čita druge signale i vraća vrednost. Funkcija se izvršava u reaktivnom kontekstu, pa radni okvir beleži koje signale je pročitala.
- Izvedeni signal se čita pozivom kao i običan, ali nema `set` ni `update`. Njegova vrednost se menja samo kada se promeni signal od kog zavisi.
- Vrednost se računa tek pri prvom čitanju i zatim pamti. Dok se `likes` ne promeni, svako čitanje `likesLabel` vraća zapamćenu vrednost bez ponovnog računanja.

Ulogu izvedenog signala je u React-u imao `useMemo`, sa ručno navedenom listom zavisnosti. Ovde listu ne navodimo, jer je radni okvir sam beleži pri čitanju.

## Nizovi i objekti u signalu

Signal obaveštava pretplatnike kada metoda `set` ili `update` dodeli novu vrednost. Kada signal čuva niz ili objekat, izmena unutar te vrednosti ne prolazi kroz signal i pretplatnici ne dobijaju obaveštenje. Sledeći kod prikazuje pogrešan i ispravan način dodavanja oznake:

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
- Metoda `addTag` pravi nov niz sa dodatom oznakom i predaje ga signalu. Signal vidi novu vrednost i obaveštava pretplatnike.

Izmena postojećeg niza je najčešći uzrok prikaza koji se ne osvežava.

## Kartica sa oznakama

Povežimo pojmove u karticu koja prikazuje oznake ture i dozvoljava dodavanje nove:

```ts
@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
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
2. Metoda predaje signalu `tags` nov niz, pa signal obaveštava pretplatnike.
3. Izvedeni signal `tagsLabel` je pretplatnik signala `tags`, pa se označava za ponovno računanje.
4. Šablon je pretplatnik signala `tagsLabel`, pa radni okvir ponovo iscrtava komponentu. Pri iscrtavanju šablon čita `tagsLabel`, koji tek tada računa novu vrednost.
