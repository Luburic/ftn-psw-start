TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: public-api.ts as the client-side contract.

Outline:
1. Anchor: the two public-api files that already exist and already export a DTO type; the contract lesson from the server segment recalled [code + text]
2. Definition of public-api.ts; what it exports (routes, components, types with export type) and what it never exports (services) and why: a shared service is shared state [code + text]
3. Using another module, in order of preference: navigate to its route; embed a component it exports, IDs in and outputs out [text + code]
4. Data composition stays on the server through contracts: why two client calls stitched together is wrong [text]
5. Extending a public-api is a cross-team negotiation [text]
6. Integration: Payment embedding a tour summary component, as a worked hypothetical [code]

Out of scope: nothing deferred.
