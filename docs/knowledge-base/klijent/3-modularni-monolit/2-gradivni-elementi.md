TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: core, shared, global styles, the token interceptor, and the current user.

Outline:
1. Anchor: the shared kernel and the host recalled from the server segment [text]
2. core/ as the host: bootstrap, route table, auth, layout; platform-owned [tree + text]
3. The interceptor: definition, the Authorization header it adds to every request, and why it lives in core [code + text]
4. shared/: util and the generated envelopes; the promotion rule [text]
5. Global styles as shared code: who owns the four partials and how a class is promoted [text]
6. The current user as the one shared state: how a module reads it (auth.user(), auth.isLoggedIn()) and who writes it [code + text]
7. Ownership: who edits what [table]

Out of scope: nothing deferred.
