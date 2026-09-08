TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: httpResource for reads, with loading and error states.

Outline:
1. Anchor: a page must show server data; fetch as the students know it, and what it lacks (loading, error, refresh, typed result); the React habit of fetching inside useEffect, and that Angular's effect exists but the project never uses it for this [text]
2. provideHttpClient added to app.config.ts; proxy.conf.json forwards every /api request to the server on port 5000, which is why client addresses start with /api and carry no server name [code + text]
3. Definition of resource; httpResource<T>(() => url) declared as a field of the page component [code + text]
4. The URL function: it re-runs when a signal it reads changes; constant URL versus URL built from a signal; the project's blog detail, whose URL reads the route input, as the example [code + text]
5. value() is undefined before the first load; ?? [] in templates; isLoading() and error() [code + text]
6. The template pattern: loading, error, empty, list with @if / @else if [code + text]
7. reload() and when it is called (after a command, next lesson) [text]
8. One sentence: every request carries the user's token, added centrally in core; the monolith segment shows where [text]
9. The generic envelope PageResult<T> from shared/api [code]
10. Integration: a list page over a resource with all four states [code + analysis]

Out of scope: HttpClient reads, Observables, RxJS, caching, the interceptor itself, resources declared outside a page.
