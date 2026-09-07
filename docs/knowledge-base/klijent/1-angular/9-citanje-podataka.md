TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: httpResource for reads, with loading and error states.

Outline:
1. Anchor: a page must show server data; fetch as the students know it, and what it lacks (loading, error, refresh, typed result) [text]
2. Definition of resource; httpResource<T>(() => url) declared in a service [code + text]
3. The URL function: it re-runs when a signal it reads changes; constant URL versus URL built from a signal; returning undefined keeps the resource idle; the project's blog detail as the example, including the one effect() that feeds the route input into the service [code + text]
4. value() is undefined before the first load; ?? [] in templates; isLoading() and error() [code + text]
5. The template pattern: loading, error, empty, list with @if / @else if [code + text]
6. reload() and when it is called (after a command, next lesson) [text]
7. One sentence: every request carries the user's token, added centrally in core; the monolith segment shows where [text]
8. The generic envelope PageResult<T> from shared/api [code]
9. Integration: a list page over a resource with all four states [code + analysis]

Out of scope: HttpClient reads, Observables, RxJS, caching, the interceptor itself.
