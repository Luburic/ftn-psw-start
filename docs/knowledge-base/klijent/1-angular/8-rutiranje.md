Postoje dva dominantna mehanizma za prikaz komponente na stranici. Prvi je direktno navođenje u roditeljskoj komponenti, a drugi prikaz kroz rutiranje, što podrazumeva učitavanje komponente na određenom mestu spram URL adrese koja je aktivna.

## Tabela ruta

**Ruta** (engl. *route*) je par koji čine putanja i komponenta koju Angular iscrtava kada se adresa u internet čitaču poklopi sa tom putanjom. **Tabela ruta** (engl. *route table*) je niz ruta iz kog Angular bira prvu koja se poklapa sa trenutnom adresom. Tabela ruta aplikacije nalazi se u datoteci `app.routes.ts`. Sledeći kod prikazuje njen početak, sa tri rute:

```ts
export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  // ...
];
```
U datom kodu treba uočiti sledeće:

- `Routes` je tip niza ruta iz paketa `@angular/router`, iz kog dolazi sve što se tiče rutera. Svaka ruta je objekat sa svojstvima `path` i `component`.
- Svojstvo `path` je deo adrese koji sledi posle adrese aplikacije. Prazan tekst se poklapa sa adresom `localhost:4200`, a `login` sa adresom `localhost:4200/login`.
- Svojstvo `component` je klasa komponente. Klase `Home`, `Login` i `Register` su komponente početne stranice, prijave i registracije. Kada korisnik otvori adresu druge rute, Angular tu komponentu uništava i pravi komponentu nove rute.

Da bi Angular koristio tabelu, ona mora biti uključena u konfiguraciju aplikacije u datoteci `app.config.ts`.

## Element `router-outlet` i veza

Tabela kaže koja se komponenta iscrtava, ali ne i gde. Angular uvodi poseban HTML element `router-outlet` na čije mesto iscrtava komponentu rute koja se poklapa sa trenutnom adresom. Adresa Angular aplikacije se ne menja kroz standardan `href` atribut, već kroz `routerLink` koji se koristi u formatu `routerLink="adresa"` na elementu `a`. Pri kliku menja adresu u internet čitaču bez učitavanja novog dokumenta.

```ts
@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
```

```html
<header>
  <nav>
    <a routerLink="/">Home</a>
    <a routerLink="/login">Login</a>
  </nav>
</header>

<main>
  <router-outlet />
</main>
```

U datom kodu treba uočiti sledeće:

- Klase `RouterLink` i `RouterOutlet` navedene su u podešavanju `imports`, kao i svaka komponenta koju šablon koristi. Bez njih prevodilac prijavljuje da element `router-outlet` nije poznat, a `routerLink` nema efekat.
- Element `router-outlet` je prazan. Kada se adresa promeni, Angular na tom mestu zamenjuje komponentu stare rute komponentom nove. Zaglavlje iznad ostaje netaknuto.

## Ruta sa parametrom

Posmatramo primer stranice koja prikazuje blog i koja treba da zna koji blog prikazuje. Za svaki blog ne može da postoji posebna ruta, pa deo adrese sadrži parametar. **Parametar rute** (engl. *route parameter*) je deo adrese oblika `:naziv`, koji se poklapa sa bilo kojom vrednošću na tom mestu i tu vrednost pod tim nazivom predaje komponenti.

Sledeći kod prikazuje tabelu ruta koja sadrži rute sa parametrima:

```ts
export const blogRoutes: Routes = [
  { path: 'blogs', component: BlogList },
  { path: 'blogs/create', component: CreateBlog },
  { path: 'blogs/:id', component: BlogDetail },
];
```

Zatim gledamo klasu komponente za prikaz jednog bloga:
```ts
@Component({
  selector: 'app-blog-detail',
  templateUrl: './blog-detail.html',
  styleUrl: './blog-detail.scss',
})
export class BlogDetail {
  readonly id = input.required<string>();
}
```
Najzad, u komponenti koja sadrži prikaz svih blogova, možemo zamisliti sledeći HTML element ispod svakog bloga:
```html
<a [routerLink]="['/blogs', blog().id]">Pročitaj više</a>
```

Kada korisnik klikne na vezu, dešava se sledeće:

1. Klikom na „Pročitaj više“ za blog čiji je identifikator 42 postavlja se adresa `/blogs/42`.
2. Ovo aktivira treću rutu (`{ path: 'blogs/:id', component: BlogDetail }`), koja na mesto `:id` postavlja 42 i na mesto `router-outlet` učitava `BlogDetail` komponentu.
3. Vrednost parametra stiže u komponentu kao ulaz istog naziva (`id`), koji u primeru dobija tekst `'42'`.
4. Kada korisnik sa adrese `/blogs/42` pređe na `/blogs/2`, ruta je ista i komponenta je ista, pa je Angular ne uništava. Zadržava postojeću komponentu i upisuje novu vrednost u ulaz `id`. Ulaz je signal, pa se sve što ga čita ponovo računa, kao pri svakoj promeni signala.

Angular bira prvu rutu u tabeli koja se poklapa sa adresom. Adresa `/blogs/create` se poklapa i sa rutom `blogs/create` i sa rutom `blogs/:id`. Ruta `blogs/create` je navedena pre, pa se otvara stranica za pravljenje bloga. Da je `blogs/:id` prva u tabeli, svaka adresa bi vodila na stranicu bloga. Zato su rute sa parametrom uvek na kraju tabele.
