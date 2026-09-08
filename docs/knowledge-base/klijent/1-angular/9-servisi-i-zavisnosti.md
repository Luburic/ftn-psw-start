TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: inject() and root services, as mechanism only. The rule about where state lives belongs to the module architecture segment.

Outline:
1. Anchor: several pages need the same logged-in user, and each page cannot hold its own copy; the dependency container recalled from the server segment, which is the strong callback here [text]
2. Definition of service: a class marked @Injectable({ providedIn: 'root' }), of which the framework creates one instance for the whole application on first injection; the real Auth class trimmed to its private token signal, the computed user and isLoggedIn fields from the derived-signals lesson, and logout; Auth lives in core because every module reads it [code + text]
3. Definition of injection: inject(Auth) as a field initialiser obtains that one instance without new; where inject may be called (field initialiser, constructor); a page holds it as protected readonly auth and the template reads auth.isLoggedIn(); a service injects too, shown by private readonly http = inject(HttpClient) inside Auth, with HttpClient named and nothing more [code + text]
4. Integration: the root component from the project, showing the user's email and the logout button through @if (auth.user(); as user) or the login link otherwise; traced from click on logout to the header re-rendering [code + analysis]

Out of scope: HTTP (next lesson), the state-placement rule (module architecture segment), provider scopes other than root, injection tokens, the lifetime comparison with server scopes.
