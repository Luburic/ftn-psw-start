TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the template constructs that branch, loop and read elements.

Outline:
1. Anchor: a list of tours must render one card per tour and nothing when empty; the array map the students used in JSX [code + text]
2. @if / @else if / @else [code + text]
3. @for with track, $index and @empty; why track is mandatory [code + text]
4. The alias @if (x(); as y) for a value that may be absent [code + text]
5. Template reference variable #input and reading its value inside an event binding, as the project's search box does [code + text]
6. Integration: a search box filtering a constant list through one signal and one computed, rendered with @for and @empty [code + analysis]

Out of scope: @switch (allowed but unused), @defer, pipes.
