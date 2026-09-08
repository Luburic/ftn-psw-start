TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the one architectural split on the client, and the folder layout that carries it.

Outline:
1. Anchor: a component that fetches, holds state and renders a table is hard to read and cannot be reused; the same smell as a fat controller on the server [text]
2. Definition of page (routed, injects services, owns handlers) and presentational component (inputs and outputs, injects nothing) [text + table]
3. Definition of use-case group: the screens serving one user goal, named as the backend's application-layer group; a module is a flat list of groups, a group is a flat list of component folders plus at most one service file; the Social module's tree read from the project [tree + text]
4. Where the split is visible without folders: the routes file lists every page, a page injects a service, a presentational component injects nothing [text]
5. Example: the blog detail page and the blog comments component, read side by side [code + analysis]
6. Deciding for a new component: three questions, the third being which group it belongs to; a component used only inside another group's screen lives with that screen, which is why the client has no commenting group while the server does [numbered list]
7. The Angular style guide's rule to organise by feature and not by type, and how the layout follows it [text]
8. Integration: split one monolithic component into a page and a child [code before and after]

Out of scope: nothing deferred; shared UI wrappers are deliberately absent.
