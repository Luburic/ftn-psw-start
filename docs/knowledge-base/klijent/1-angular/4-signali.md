TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: why signals are mandatory here, set and update, computed. Effect as awareness only.

Outline:
1. Anchor: the closing failure of the previous lesson, repeated in two lines [code + text]
2. Why: zoneless change detection re-renders only templates whose signals changed; definition of signal as a value the template subscribes to; OnPush named as the default that makes this strict [text + diagram]
3. signal(0), reading with a call, set and update; the counter now refreshes; useState as the known counterpart [code + text]
4. computed() as a derived value that recomputes when the signals it read change; a doubled counter as the example [code + text]
5. Dependency tracking happens only inside a reactive context; reading a signal in an ordinary method tracks nothing [text]
6. Changing arrays and objects held in a signal: update with a new array, not push on the old one; stated as a trap for the students' own modules, since every list in this project arrives from the server [code + text]
7. effect() in two sentences: exists, runs when its signals change, and any use inside the component tree is a smell; the project has no use of it [text]
8. React counterparts: useState / signal, useMemo / computed; an explicit non-equivalence row saying that useEffect's data-loading job belongs to a resource here, not to effect [table]
9. Integration: the counter with a computed label, both driven by one signal [code + analysis]

Out of scope: linkedSignal, resource (reading-data lesson), untracked, equality functions.
