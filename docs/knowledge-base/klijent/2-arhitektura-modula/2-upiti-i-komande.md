TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: where a read resource lives, where a command lives, and the rule about where state lives.

Outline:
1. Anchor: a page shows a list and publishes an item from it; if the list's resource lived in a shared service, every command in the module would have to know which lists it makes stale, and the cost of that bookkeeping in the earlier version of the project [text]
2. Definition of query in the page: httpResource declared in the page component, created on every visit and destroyed with it, so navigating back always shows fresh data [code + text]
3. Definition of the group service: one per use-case group, holding the group's commands and nothing else; a group with no commands has no service [code + text]
4. The page pattern: await the command, then reload the page's own resource; the mirror of the server's command and query separation [text + diagram]
5. Loading and error states rendered the same way on every page [code]
6. The rule: state that belongs to one page lives in the page (its resource, a filter, a pending flag); state that outlives a page lives in a root service, with the current user in core as the only instance [text + table]
7. Integration: the my-tours page and the tour authoring service read top to bottom [code + analysis]

Out of scope: caching, optimistic updates, sharing state across modules (forbidden, monolith segment).
