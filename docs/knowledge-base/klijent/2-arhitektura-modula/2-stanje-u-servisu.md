TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the module's state service and the rule about where state lives.

Outline:
1. Anchor: two pages show the same list, and a command on one must refresh the other; publish reloading both lists in the project [text]
2. Definition of the module state service: one per module, all server talk goes through it, resources for reads and methods for commands [code + text]
3. Reads as resources, writes as commands, reload as the link between them; the mirror of the server's queries and service classes [text + diagram]
4. Loading and error states rendered the same way on every page [code]
5. The rule: local UI state stays in the page (a filter, a pending flag); module state lives in the service because it survives navigation and is shared by pages [text + table]
6. Integration: the Tours service read top to bottom [code + analysis]

Out of scope: caching, optimistic updates, sharing state across modules (forbidden, monolith segment).
