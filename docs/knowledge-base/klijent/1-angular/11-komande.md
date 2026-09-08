TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: state-changing calls and the page pattern around them.

Outline:
1. Anchor: create and publish change server state; command versus query recalled from the server segment [text]
2. HttpClient.post returns an Observable; one sentence on what that is, then firstValueFrom to turn it into the Promise the students know, and await [code + text]
3. The mechanics of one command method in a service: await the post and return the result; the service touches no resource [code + text]
4. The page pattern: pending signal, error signal, try / catch / finally, and reload() of the page's own resource after the await [code + text]
5. Server errors: the Problem response and the exception middleware recalled from the server controllers lesson; the body shape with its title field defined here; the serverMessage helper presented as a black box, signature and fallback only [code + text]
6. Button disabled while pending; the error shown in the template [code + text]
7. Integration: the Publish button from click to reloaded list [code + analysis]

Out of scope: forms, optimistic updates, RxJS operators, the body of serverMessage, which service a command belongs to (module architecture segment).
