Na serveru je svaki sloj modula zaseban projekat, a kompajler odbija build kada projekat koristi tip iz projekta koji ne referencira. Modul tako ne može da posegne u unutrašnjost drugog modula, jer referenca ka njoj ne postoji. Na klijentu je cela aplikacija jedan projekat, pa naredba `import` sa bilo kojom putanjom prolazi prevođenje. Uvoz kartice ture iz unutrašnjosti modula Exploration, sa početka lekcije o javnoj površini, prevodilac prihvata bez primedbe. Pravilo o javnoj površini zato mora da proverava nešto drugo, kao što na serveru arhitektonski test proverava pravila koja kompajler ne čuva.

## Statička analiza

**Statička analiza** (engl. *static analysis*) je provera izvornog koda bez njegovog izvršavanja. **Linter** (engl. *linter*) je alat za statičku analizu koji prijavljuje svako mesto u kodu koje krši neko od zadatih pravila. U projektu je to ESLint, a pravila stoje u datoteci `eslint.config.js` u korenu klijentske aplikacije. Sledeći kod prikazuje deo te datoteke koji čuva granicu modula Exploration:

```js
{
  files: ['src/app/modules/exploration/**/*.ts'],
  rules: {
    'no-restricted-imports': [
      'error',
      {
        patterns: [
          {
            regex: '^(\.\./)+(games|social|payment)/(?!public-api$)',
            message: 'Import another module only through its public-api.',
          },
        ],
      },
    ],
  },
},
```

U datom kodu treba uočiti sledeće:

- Svojstvo `files` bira datoteke na koje blok važi, ovde sve TS datoteke modula Exploration. Ostala tri modula imaju isti blok, u kom je iz zagrade izostavljen naziv tog modula, a navedena ostala tri.
- Pravilo `no-restricted-imports` zabranjuje uvoz čija putanja odgovara regularnom izrazu. Izraz se čita ovako: putanja koja jednim ili više `../` izlazi iz direktorijuma, ulazi u direktorijum `games`, `social` ili `payment` i posle toga se ne završava na `public-api`. Uvoz `../../social/public-api` prolazi, a `../../social/blog-reading/blog-card/blog-card` ne.
- Svojstvo `message` je tekst koji linter ispisuje uz prijavu.
- Nivo `error` znači da prijava obara proveru, a ne da samo upozorava.
- Pravilo proverava putanju uvoza, a ne šta se uvozi. Servis koji bi neko izvezao kroz javnu površinu pravilo propušta. Granica klijenta je ovo pravilo i dogovor iz lekcije o javnoj površini, i ništa strukturno, za razliku od servera na kom referenca između projekata ne postoji.

## Čitanje prijave

Sledeći ispis prikazuje šta komanda `npm run lint` prijavljuje kada stranica modula Payment uveze karticu iz unutrašnjosti modula Exploration:

```
src/app/modules/payment/payment-home/payment-home.ts
  2:1  error  '../../exploration/tour-browsing/tour-card/tour-card' import is restricted from being used by a pattern. Import another module only through its public-api  no-restricted-imports

✖ 1 problem (1 error, 0 warnings)
```

U datom ispisu treba uočiti sledeće:

- Prvi red je datoteka u kojoj je prekršaj, a `2:1` je red i kolona naredbe `import`.
- Iza nivoa `error` stoji poruka. Prvi deo poruke piše ESLint, sa putanjom koja je odgovarala regularnom izrazu, a drugi deo je tekst iz svojstva `message` u konfiguraciji.
- Na kraju reda je naziv pravila koje je prekršeno.
- Ispravka je jedan od dva načina iz lekcije o javnoj površini. Stranica ili navigira na adresu modula Exploration, ili ugrađuje komponentu koju tim modula Exploration izveze kroz javnu površinu. Konfiguracija se ne menja.

## Kada se pravilo proverava

Pravilo se proverava na dva mesta. Lokalno ga proverava komanda `npm run lint`, koju pokrećemo pre nego što izmene pošaljemo na repozitorijum. Na serveru za kontinuiranu integraciju se na svaki push i svaki pull request pokreće ista komanda, pa zatim `npm run build`, uz prevođenje i testove serverske aplikacije. Prekršaj pravila tako obara build za ceo tim, kao i arhitektonski test na serveru.

Kada build padne, u zapisu izvršavanja treba naći posao `frontend` i u njemu korak `Lint`. Njegov ispis je isti kao ispis komande `npm run lint` iz prethodnog odeljka i čita se na isti način.
