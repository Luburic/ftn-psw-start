TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: inject() and root services, as mechanism only. The rule about where state lives belongs to the module architecture segment.

Outline:
1. Anchor: several pages need the same logged-in user; the dependency container recalled from the server segment, which is the strong callback here [text]
2. inject(Auth) as the first injection, already seen in the root component: an object the framework created once, obtained without new; where inject may be called (field initialiser, constructor); Auth lives in core by an exception the monolith segment explains [code + text]
3. Definition of service: @Injectable({ providedIn: 'root' }) gives one instance for the whole application; lifetime compared with the server's scopes [code + text]
4. A service holding a signal; a page reading it through protected readonly auth = inject(Auth) and auth.isLoggedIn() in the template [code + text]
5. Integration: the tour list page and the blog list page both injecting the Auth service, read from the project [code + analysis]

Out of scope: HTTP (next lesson), the state-placement rule (module architecture segment), provider scopes other than root, injection tokens.
