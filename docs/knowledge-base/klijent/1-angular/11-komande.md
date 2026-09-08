TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: state-changing calls and the page pattern around them.

Outline:
1. Anchor: create and publish change server state; command versus query recalled from the server segment [text]
2. HttpClient.post returns an Observable, defined in one sentence as a value that arrives later and may arrive more than once; the project always wants exactly one value, so every call is wrapped in firstValueFrom, which turns it into the Promise the students know, and awaited [code + text]
3. The group service: the real BlogAuthoring read with its base URL constant, the injected HttpClient from the services lesson, create returning a DTO and publish returning void; the service touches no resource [code + text]
4. The page pattern with its template: pending signal, error signal, try / catch / finally, reload() of the page's own resource after the await; the button disabled while pending and the error shown in the template [code + text]
5. Server errors: the Problem response and the exception middleware recalled from the server controllers lesson; the body shape reduced to its title field; the serverMessage helper presented by signature only, with the fallback argument shown by the project's real fallback text [code + text]
6. Integration: the real MyTours.publish method and its button, from click to reloaded list [code + analysis]

Out of scope: forms, optimistic updates, RxJS operators, the body of serverMessage, which service a command belongs to and where the resource lives (module architecture segment).
