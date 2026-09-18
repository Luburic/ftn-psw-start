Sve što smo do sada napisali iscrtava se unutar korenske komponente, na jednoj adresi. Projekat će imati više stranica, a svaka ima svoju adresu, pa korisnik može da otvori stranicu bloga ili da se dugmetom internet čitača vrati na prethodnu stranicu. Pri tome internet čitač ne učitava nov HTML dokument, već se menjaju samo adresa i sadržaj koji Angular iscrtava. U React-u se za to koristi React Router, biblioteka koju programer sam dodaje i podešava. Angular taj posao, četvrti od pet problema iz lekcije o Angularu, rešava ugrađenim delom radnog okvira koji zovemo **ruter** (engl. *router*). Ovde upoznajemo kako se adresi dodeljuje komponenta, gde se ta komponenta iscrtava i kako se iz adrese čita podatak.

## Tabela ruta

**Ruta** (engl. *route*) je par koji čine putanja i komponenta koju Angular iscrtava kada se adresa u internet čitaču poklopi sa tom putanjom. **Tabela ruta** (engl. *route table*) je niz ruta iz kog Angular bira prvu koja se poklapa sa trenutnom adresom. Tabela ruta aplikacije nalazi se u datoteci `src/app/app.routes.ts`, koju smo u lekciji o strukturi projekta privremeno zanemarili. Sledeći kod prikazuje tabelu sa tri rute:

```ts
import { Routes } from '@angular/router';
// import-i komponenti Home, Login i Register

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
];
```

Da bi Angular koristio tabelu, ona mora biti uključena u konfiguraciju aplikacije u datoteci `app.config.ts`:

```ts
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
  ],
};
```

U datom kodu treba uočiti sledeće:
- `Routes` je tip niza ruta iz paketa `@angular/router`, iz kog dolazi sve što se tiče rutera. Svaka ruta je objekat sa svojstvima `path` i `component`.
- Svojstvo `path` je deo adrese bez početne kose crte. Prazan tekst se poklapa sa adresom `localhost:4200`, a `login` sa adresom `localhost:4200/login`.
- Svojstvo `component` je klasa komponente. Klase `Home`, `Login` i `Register` su komponente početne stranice, prijave i registracije. Kada korisnik otvori adresu druge rute, Angular tu komponentu uništava i pravi komponentu nove rute.
- Poziv `provideRouter(routes)` je `ng new` već upisao u konfiguraciju, zajedno sa praznom tabelom ruta. Mi mu dodajemo drugi argument, `withComponentInputBinding()`, koji objašnjavamo uz rutu sa parametrom.

## Mesto iscrtavanja i veza

Tabela kaže koja se komponenta iscrtava, ali ne i gde. **Mesto iscrtavanja** (engl. *router outlet*) je element `router-outlet`, na čije mesto Angular iscrtava komponentu rute koja se poklapa sa trenutnom adresom. **Veza** (engl. *router link*) je zapis `routerLink="adresa"` na elementu `a`, koji pri kliku menja adresu u internet čitaču bez učitavanja novog dokumenta.

```ts
import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

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
- Klase `RouterLink` i `RouterOutlet` navedene su u podešavanju `imports`, kao i svaka komponenta koju šablon koristi. Bez njih prevodilac prijavljuje da element `router-outlet` nije poznat, a `routerLink` ostaje običan HTML atribut bez dejstva.
- Element `router-outlet` je prazan. Kada se adresa promeni, Angular na tom mestu zamenjuje komponentu stare rute komponentom nove. Zaglavlje iznad ostaje netaknuto.
- Adresa u vezi ovde počinje kosom crtom, jer je to cela adresa od korena. Deo adrese u tabeli ruta je bez nje, jer se nadovezuje na ono što je ispred njega. Veze mogu biti i relativne, u odnosu na trenutnu rutu, ali u projektu pišemo pune adrese.

## Ruta sa parametrom

Stranica koja prikazuje jedan blog treba da zna koji blog prikazuje. Za svaki blog ne može da postoji posebna ruta, pa deo adrese sadrži parametar. **Parametar rute** (engl. *route parameter*) je deo adrese oblika `:naziv`, koji se poklapa sa bilo kojom vrednošću na tom mestu i tu vrednost pod tim nazivom predaje komponenti.

Primere u ovom odeljku uzimamo iz modula Social. Modul u ovom kontekstu označava celinu projekta, a ne Angular `NgModule`, stari način organizovanja aplikacije koji ćete sretati u starijim primerima, a koji u projektu ne koristimo. Sledeći kod prikazuje tabelu ruta modula Social, stranicu koja prima parametar i vezu sa kartice bloga koja vodi na nju:

```ts
export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: ':id', component: BlogDetail },
];
```

```ts
import { Component, input } from '@angular/core';

@Component({
  selector: 'app-blog-detail',
  templateUrl: './blog-detail.html',
  styleUrl: './blog-detail.scss',
})
export class BlogDetail {
  readonly id = input.required<string>();
}
```

```html
<a [routerLink]="['/social', blog().id]">Read more</a>
```

U datom kodu treba uočiti sledeće:
- Deo adrese `:id` se poklapa sa adresama `/social/1` i `/social/42`. Vrednost iza `/social` je parametar `id`.
- Parametar stiže u komponentu kao ulaz istog naziva. To omogućava `withComponentInputBinding` iz konfiguracije aplikacije. Nakon što izabere rutu, Angular svaki parametar rute upiše u ulaz komponente istog naziva, isto kao što roditelj upisuje vrednost u ulaz deteta zapisom `[id]="..."`. Bez tog podešavanja Angular parametar ne upisuje u ulaz.
- Vrednost parametra je uvek tekst, jer dolazi iz adrese. Zato je ulaz tipa `string`.
- Kada korisnik sa adrese `/social/1` otvori `/social/2`, ruta je ista i komponenta je ista, pa je Angular ne uništava. Zadržava postojeću komponentu i upisuje novu vrednost u ulaz `id`. Ulaz je signal, pa se sve što ga čita ponovo računa, kao pri svakoj promeni signala. Kod u konstruktoru, međutim, neće se ponovo izvršiti. Zato sve što zavisi od `id` računamo iz signala.
- Veza sa parametrom se piše kao niz, uz vezivanje svojstva `[routerLink]`. Prvi element niza je deo adrese koji je isti za sve blogove, a drugi je vrednost parametra. Angular od niza sastavlja adresu `/social/1`. Komponenta kartice bloga mora u podešavanju `imports` da navede `RouterLink`, isto kao korenska komponenta.
- Angular bira prvu rutu u tabeli koja se poklapa sa adresom. Adresa `/social/create` se poklapa i sa rutom `create` i sa rutom `:id`. Ruta `create` je navedena pre, pa se otvara stranica za pravljenje bloga. Da je `:id` prva u tabeli, svaka adresa bi vodila na stranicu bloga. Zato su rute sa parametrom uvek na kraju tabele.

Iz istog razloga poslednja u tabeli aplikacije ide ruta `{ path: '**', component: NotFound }`. Putanja `**` se poklapa sa svakom adresom, pa se prikazuje samo kada nijedna ruta pre nje ne odgovara, npr. za stranicu „Stranica ne postoji“.

## Stranica bloga

Ulaz `id` nosi samo tekst `'1'`. Stranica treba da prikaže naslov i sadržaj bloga sa tim identifikatorom, pa iz identifikatora mora da dođe do podataka. Pravi podaci stižu sa servera, što je tema lekcije o komunikaciji sa serverom. Ovde blogove, kao i ture u lekciji o kontroli toka, privremeno upisujemo u klasu, da bismo videli samu vezu između adrese i podataka.

Blog opisuje sledeći interfejs:

```ts
export interface BlogDto {
  id: string;
  title: string;
  content: string;
}
```

Sledeći kod prikazuje stranicu bloga koja iz ulaza `id` pronalazi blog:

```ts
import { Component, computed, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BlogDto } from '../blog-dto';

@Component({
  selector: 'app-blog-detail',
  imports: [RouterLink],
  templateUrl: './blog-detail.html',
  styleUrl: './blog-detail.scss',
})
export class BlogDetail {
  readonly id = input.required<string>();

  private readonly blogs = signal<BlogDto[]>([
    { id: '1', title: 'Zimski uspon na Rtanj', content: 'Opis uspona po snegu.' },
    { id: '2', title: 'Vikend na Tari', content: 'Vidikovci i staze uz Drinu.' },
  ]);

  protected readonly currentBlog = computed(() =>
    this.blogs().find((blog) => blog.id === this.id()),
  );
}
```

```html
@if (currentBlog(); as blog) {
  <h2>{{ blog.title }}</h2>
  <p>{{ blog.content }}</p>
} @else {
  <p>Blog ne postoji.</p>
}

<a routerLink="/social/2">Pročitajte i: Vikend na Tari</a>
```

U datom kodu treba uočiti sledeće:
- Izvedeni signal `currentBlog` čita ulaz `id` i signal `blogs`, pa je čitalac oba. Ovo je mesto na kom se adresa pretvara u podatke: identifikator iz adrese određuje koji se blog prikazuje.
- Metoda `find` vraća `undefined` kada blog sa tim identifikatorom ne postoji, npr. za adresu `/social/99`. Zato je tip izvedenog signala `BlogDto | undefined`, a šablon koristi `@if` sa aliasom, kao u lekciji o kontroli toka. Grana `@else` pokriva nepostojeći blog.
- Blog se ne traži u konstruktoru, već u izvedenom signalu. U konstruktoru obavezan ulaz još nema vrednost, a i kada bi je imao, konstruktor se ne bi ponovo izvršio pri prelasku na drugi blog. Izvedeni signal se preračunava svaki put kada se `id` promeni.
- Spisak blogova u klasi je privremeno rešenje. U lekciji o komunikaciji sa serverom zamenjujemo ga zahtevom ka serveru, ali princip ostaje isti: podaci zavise od signala `id`, pa se ponovo učitavaju kada se `id` promeni.

## Od adrese do stranice bloga

Povežimo pojmove. Korisnik je na spisku blogova i klikne na vezu „Read more“ prve kartice. Najpre ruter bira komponentu:
1. Veza presreće klik, od niza `['/social', '1']` sastavlja adresu `/social/1` i upisuje je u internet čitač, bez učitavanja novog dokumenta.
2. Angular u tabeli aplikacije nalazi stavku sa putanjom `social` i prelazi na tabelu modula Social sa ostatkom adrese, `1`. Kako tabela modula ulazi u tabelu aplikacije, opisuje segment o modularnom monolitu.
3. U tabeli modula redom proverava `''`, `mine` i `create`, koje se ne poklapaju sa `1`, i staje na `:id`, koja se poklapa sa vrednošću `1`.
4. Na mestu iscrtavanja uništava komponentu `BlogList` i pravi komponentu `BlogDetail`. Zaglavlje korenske komponente ostaje.

Zatim komponenta od identifikatora dolazi do podataka:
1. Angular upisuje tekst `'1'` u ulaz `id` komponente `BlogDetail`, jer ulaz nosi isti naziv kao parametar.
2. Pri prvoj proveri šablona `@if` čita izvedeni signal `currentBlog`, koji tada prvi put računa vrednost: čita `id()`, dobija `'1'` i u spisku pronalazi blog „Zimski uspon na Rtanj“.
3. Pošto je blog pronađen, `@if` prikazuje naslov i sadržaj.

Na kraju korisnik klikne na vezu „Pročitajte i: Vikend na Tari“:
1. Veza upisuje adresu `/social/2`. Ruta je ista, `:id`, pa Angular ne uništava komponentu `BlogDetail`, već zadržava postojeću.
2. Angular upisuje tekst `'2'` u ulaz `id`. Ulaz je signal, pa obaveštava svoje čitaoce.
3. Izvedeni signal `currentBlog` je čitalac ulaza `id`, pa se označava za ponovno računanje.
4. Angular ponovo proverava šablon. Izvedeni signal `currentBlog` pronalazi blog „Vikend na Tari“, a Angular menja samo naslov i sadržaj na stranici.
