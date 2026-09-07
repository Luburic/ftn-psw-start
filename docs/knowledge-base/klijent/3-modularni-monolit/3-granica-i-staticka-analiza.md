TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the lint rule as the client's architecture test.

Outline:
1. Anchor: the architecture tests from the server segment; on the client the compiler does not know what a module is [text]
2. Definition of static analysis and the linter; the flat configuration file and the one block per module [code + text]
3. Reading a failing lint report [code output + text]
4. What the rule does not catch (a service exported from public-api) and why the boundary is weaker than the server's [text]
5. Running the linter locally and in CI [text]

Out of scope: other lint rules, formatting.
