TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the route table, outlet, links, directives, parameters, lazy loading. First appearance of routing anywhere in the segment; the Angular lesson showed a root component without it. No injection needed.

Outline:
1. Anchor: a single page application changes the address without reloading; what the students saw with React Router; the fourth of the five problems from the Angular lesson [text]
2. Definition of route and route table; app.routes.ts with the three literal entries; provideRouter(routes) added to app.config.ts as the first framework part enabled after ng new [code + text]
3. The real root component: router-outlet as the place a routed component renders and routerLink as the navigation link; definition of directive as a class that goes into imports and attaches behaviour to an element without being a component; imports recalled from the composition lesson [code + text]
4. A route with a parameter :id, arriving in the component as input.required<string>(); withComponentInputBinding added to provideRouter as the option that makes this happen; the input is a signal like any other [code + text]
5. The array form [routerLink]="['/social', id]" for links with parameters, shown on the same example as item 4 [code + text]
6. Route order: literal paths before :id, read from the project's social.routes.ts [code + text]
7. A child route table per module and loadChildren with dynamic import(), read from the project's app.routes.ts, with one sentence that public-api re-exports the module's routes and a TODO for the monolith segment; why lazy (one bundle per module, loaded on first visit) [code + text]
8. Integration: trace /social/123 from the address bar to the id input of the detail page, stopping at the input [numbered list]

Out of scope: programmatic navigation (submitting-a-form lesson), guards, resolvers, query parameters.
