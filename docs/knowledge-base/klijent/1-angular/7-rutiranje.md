TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the route table, outlet, links, parameters, lazy loading. No injection needed.

Outline:
1. Anchor: a single page application changes the address without reloading; what the students saw with React Router [text]
2. Definition of route and route table; provideRouter recalled from the project walkthrough [code + text]
3. router-outlet as the place a routed component renders and routerLink as the navigation link; definition of directive as a class that goes into imports and attaches behaviour to an element without being a component [code + text]
4. A route with a parameter :id, arriving in the component as input.required<string>() because of withComponentInputBinding [code + text]
5. The array form [routerLink]="['/social', id]" for links with parameters [code + text]
6. A child route table per module and loadChildren with dynamic import(), taught against ./social.routes; then the project's real line importing from public-api, with one sentence that a module exposes its routes through a public file the monolith segment explains; why lazy (one bundle per module, loaded on first visit) [code + text]
7. Route order: literal paths before :id [text]
8. Integration: trace /social/123 from the address bar to the id input of the detail page, stopping at the input [numbered list]

Out of scope: programmatic navigation (submitting-a-form lesson), guards, resolvers, query parameters.
