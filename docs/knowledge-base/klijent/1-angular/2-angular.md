TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the smallest ng new project, read file by file, using trimmed versions of the two files this project has already grown. No component authoring.

Outline:
1. Anchor: the five technical problems every client application solves (render, refresh on change, react to the user, pick a screen by address, talk to the server) and how React the students know covers only the first two while Angular covers all five [text]
2. Definition of framework versus library recalled from the server segment, applied to the client [text]
3. The workspace tree after ng new: which files a student reads, which are tooling and ignored [tree + text]
4. index.html and main.ts: the single HTML page and the bootstrap call that mounts the root component with a configuration [code + text]
5. app.config.ts as the list of capabilities registered once: provideRouter with withComponentInputBinding, provideHttpClient, each named as an entry whose lesson comes later; provideBrowserGlobalErrorListeners named as an ng new default to ignore [code + text]
6. app.routes.ts as the route table, shown trimmed to its three eager entries ('', login, register): one entry maps an address to a component; the lazy entries are cut and named as the routing lesson's subject [code + text]
7. The root component, shown trimmed to imports: [RouterOutlet] and a nav of plain routerLinks: decorator with selector, templateUrl and styleUrl; class; template with router-outlet. Definition of component. The cut lines (inject, @if, the signal call) named with the lesson that restores each [code + text]
8. Standalone as the default, and the sentence that NgModule exists in older code and is not used here [text]
9. From address to pixels: bootstrap, root component, outlet, routed component [diagram or numbered list]
10. Running it: npm start, the proxy to the backend on port 5000, where errors appear (terminal versus browser console) [text]
11. Integration: trace 'the user opens localhost:4200' through main.ts, app.config.ts, app.routes.ts and the root component [numbered list]

Out of scope: zoneless and OnPush (named as defaults only, explained in the signals lesson), server-side rendering, ng generate.
