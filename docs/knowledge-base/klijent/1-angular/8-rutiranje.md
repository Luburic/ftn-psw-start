Sve što smo do sada napisali iscrtava se unutar korenske komponente, na jednoj adresi. Projekat će imati više stranica, a svaka ima svoju adresu, tako korisnik može da otvori stranicu bloga ili da se dugmetom internet čitača vrati na prethodnu stranicu. Pri tome internet čitač ne učitava nov HTML dokument, već se menja samo adresa i sadržaj koji radni okvir iscrtava. U React-u je čitalac za to koristio React Router, biblioteku koju je sam dodao i podesio. Angular taj posao, četvrti od pet problema iz lekcije o Angular-u, rešava ugrađenim delom radnog okvira. Ovde upoznajemo kako se adresi dodeljuje komponenta, gde se ta komponenta iscrtava i kako iz adrese čita podatak.

## Tabela ruta

**Ruta** (engl. *route*) je par koji čine adresa (putanja) i komponenta koju radni okvir iscrtava kada se adresa unese u internet čitač. **Tabela ruta** (engl. *route table*) je niz ruta iz kog radni okvir bira prvu koja se poklapa sa trenutnom adresom. Tabela ruta aplikacije živi u datoteci `/app.routes.ts`. Sledeći kod prikazuje tabelu sa tri rute:

```ts
export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
];
```

Da bi radni okvir tabelu koristio, uključujemo je u konfiguraciju aplikacije:

```ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
  ],
};
```

U datom kodu treba uočiti sledeće:
- Anotacija `Routes` je tip niza ruta iz radnog okvira. Svaka ruta je objekat sa svojstvima `path` i `component`.
- Svojstvo `path` je deo adrese bez početne kose crte. Prazan tekst se poklapa sa adresom `localhost:4200`, a `login` sa adresom `localhost:4200/login`.
- Svojstvo `component` je klasa komponente. Klase `Home`, `Login` i `Register` su komponente početne stranice, prijave i registracije. Komponentu rute ne pravi šablon nekog roditelja, već radni okvir, kada se adresa poklopi sa rutom. Kada korisnik otvori adresu druge rute, radni okvir tu komponentu uništava i pravi komponentu nove rute.
- Poziv `provideRouter` je prvi deo radnog okvira koji smo dodali u konfiguraciju nakon `ng new`. Prima tabelu ruta, a drugi argument objašnjavamo uz rutu sa parametrom.

## Mesto iscrtavanja i veza

Tabela kaže koja se komponenta iscrtava, ali ne i gde. Zaglavlje sa navigacijom je isto na svakoj stranici, a menja se samo deo ispod njega. Zato korenska komponenta u svom šablonu označava mesto na kom radni okvir iscrtava komponentu rute. **Mesto iscrtavanja** (engl. *router outlet*) je element `router-outlet`, na čije mesto radni okvir iscrtava komponentu rute koja se poklapa sa trenutnom adresom. **Veza** (engl. *router link*) je zapis `routerLink="adresa"` na elementu `a`, koji pri kliku menja adresu u internet čitaču bez učitavanja novog dokumenta.

```ts
@Component({
  imports: [RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
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
- Klase `RouterLink` i `RouterOutlet` navedene su u podešavanju `imports`, kao i svaka komponenta koju šablon koristi. Bez njih prevodilac prijavljuje da element `router-outlet` nije poznat, a `routerLink` ostaje običan HTML atribut bez dejstva.
- Element `router-outlet` je prazan. Kada se adresa promeni, radni okvir na tom mestu zamenjuje komponentu stare rute komponentom nove. Zaglavlje iznad ostaje netaknuto.
- Adresa u vezi počinje kosom crtom, jer je to cela adresa od korena. Deo adrese u tabeli ruta je bez nje, jer se nadovezuje na ono što je ispred njega.

## Ruta sa parametrom

Stranica koja prikazuje jedan blog treba da zna koji blog prikazuje. Za svaki blog ne može da postoji posebna ruta, pa deo adrese sadrži parametar. **Parametar rute** (engl. *route parameter*) je deo adrese oblika `:naziv`, koji se poklapa sa bilo kojom vrednošću na tom mestu i tu vrednost pod tim nazivom predaje komponenti. Sledeći kod prikazuje, iz modula Social u projektu, tabelu ruta modula, stranicu koja prima parametar i vezu sa kartice bloga koja vodi na nju:

```ts
export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: ':id', component: BlogDetail },
];
```

```ts
export class BlogDetail {
  readonly id = input.required<string>();
}
```

```html
<a [routerLink]="['/social', blog().id]">Read more</a>
```

U datom kodu treba uočiti sledeće:
- Deo adrese `:id` se poklapa sa adresama `/social/1` i `/social/42`. Vrednost iza `/social` je parametar `id`.
- Parametar stiže u komponentu kao ulaz istog naziva. To omogućava `withComponentInputBinding` iz konfiguracije aplikacije. Nakon što izabere rutu, radni okvir svaki parametar rute upiše u ulaz komponente istog naziva, isto kao što roditelj upisuje vrednost u ulaz deteta zapisom `[id]="..."`. Bez tog podešavanja radni okvir parametar ne upisuje u ulaz.
- Prevodilac obavezan ulaz proverava u šablonu roditelja. Komponentu rute ne koristi nijedan šablon, pa proveru nema ko da prekrši, a ulaz popunjava radni okvir.
- Vrednost parametra je uvek tekst, jer dolazi iz adrese. Zato je ulaz tipa `string`, i kada identifikator na serveru nije tekst.
- Kada korisnik sa adrese `/social/1` otvori `/social/2`, ruta je ista i komponenta je ista, pa radni okvir komponentu ne uništava. Zadržava postojeću i upisuje novu vrednost u ulaz `id`. Ulaz je signal, pa se sve što ga čita ponovo računa, kao pri svakoj promeni signala.
- Veza sa parametrom se piše kao niz. Prvi element niza je deo adrese koji je isti za sve blogove, a drugi je vrednost parametra. Radni okvir od niza sastavlja adresu `/social/1`.
- Radni okvir bira prvu rutu u tabeli koja se poklapa sa adresom. Adresa `/social/create` se poklapa i sa rutom `create` i sa rutom `:id`. Ruta `create` je navedena pre, pa se otvara stranica za pravljenje bloga. Da je `:id` prva u tabeli, svaka adresa bi vodila na stranicu bloga. Zato su rute sa parametrom uvek na kraju tabele.

## Od adrese do stranice bloga

Povežimo pojmove. Korisnik je na spisku blogova i klikne na vezu „Read more“ prve kartice. Dešava se sledeće:
1. Veza presreće klik, od niza `['/social', '1']` sastavlja adresu `/social/1` i upisuje je u internet čitač.
2. Radni okvir u tabeli aplikacije nalazi stavku sa prefiksom `social` i prelazi na tabelu modula Social sa ostatkom adrese, `1`. Kako tabela modula ulazi u tabelu aplikacije opisuje segment o modularnom monolitu.
3. U tabeli modula redom proverava `''`, `mine` i `create`, koji se ne poklapaju sa `1`, i staje na `:id`, koji se poklapa sa vrednošću `1`.
4. Na mestu iscrtavanja uništava komponentu `BlogList` i pravi komponentu `BlogDetail`. Zaglavlje korenske komponente ostaje.
5. Upisuje tekst `'1'` u ulaz `id` komponente `BlogDetail`, jer ulaz nosi isti naziv kao parametar.
