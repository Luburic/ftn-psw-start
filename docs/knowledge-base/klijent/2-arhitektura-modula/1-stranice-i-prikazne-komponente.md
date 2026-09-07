TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the one architectural split on the client.

Outline:
1. Anchor: a component that fetches, holds state and renders a table is hard to read and cannot be reused; the same smell as a fat controller on the server [text]
2. Definition of page (routed, injects services, owns handlers) and presentational component (inputs and outputs, injects nothing) [text + table]
3. The folders pages/ and components/ as the physical split [tree]
4. Example: the blog detail page and the blog comments component, read side by side [code + analysis]
5. Deciding for a new component: three questions [numbered list]
6. The divergence from the Angular style guide's feature folders, and why the project keeps the split visible [text]
7. Integration: split one monolithic component into a page and a child [code before and after]

Out of scope: nothing deferred; shared UI wrappers are deliberately absent.
