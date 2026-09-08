TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: DTO types as the wire contract, generated per module; the one place a hand-written client type lives.

Outline:
1. Anchor: the api/ files in this repository are hand-written stand-ins today, so the drift between them and the server DTO is a live risk, not a hypothetical [text]
2. The server's Application DTO as the wire contract, recalled from the DTO lesson and the API layer lesson (ActionResult<T>) [text]
3. From the OpenAPI document to one types file per module, and where the shared envelope file comes from [diagram + text]
4. What a generated file looks like and how a module imports from api/ [code]
5. The rule: never hand-edit; regenerate when the server DTO changes; who runs the generator [text]
6. How server types arrive: enums as string unions, dates and GUIDs as strings [table]
7. A client-only type is exported from the component that emits it, with the comment edit payload as the example, since it exists only to carry an output; no models folder [code + text]
8. A type another module needs is re-exported through public-api [code]

Out of scope: the generator tool (TODO until chosen).
