TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: httpResource for reads, with loading and error states.

Outline:
1. Anchor: a page must show server data; fetch as the students know it, and what it lacks (loading, error, refresh, typed result); the React habit of fetching inside useEffect, which the resource replaces [text]
2. provideHttpClient added to app.config.ts; proxy.conf.json forwards every /api request to the server on port 5000, which is why client addresses start with /api and carry no server name; two sentences [code + text]
3. Definition of resource; httpResource<PageResult<TourDto>>(() => url) declared as a field of the tour list page, with the envelope PageResult<T> from shared/api shown next to it and the template reading .items; the envelope is used from the start because both real list pages read one [code + text]
4. The URL function as a reactive context, the term from the signals lesson: it re-runs when a signal it reads changes and the resource sends a new request; constant URL versus URL built from the route input, on the project's blog detail; the router keeping the component and setting the input again, from the routing lesson, is what makes a new id load a new blog [code + text]
5. The resource's three signals and the template that reads them: value() is undefined before the first load, ?? [] in templates, isLoading() and error(); the template pattern loading, error, list with @empty, using @if / @else if; reload() as one bullet, it repeats the request with the current URL and the commands lesson shows who calls it [code + text]
6. Integration: the my-tours list page over a resource with all states, read from the project with the command parts omitted [code + analysis]

Out of scope: HttpClient reads, Observables, RxJS, caching, the token interceptor (monolith segment), effect(), resources declared outside a page.
