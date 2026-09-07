TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: parent and child, data down, events up.

Outline:
1. Anchor: a list page repeating the same card markup; why a child component (readability, reuse) and how props in React solved the same thing [text]
2. Using a child: adding it to imports and writing its selector in the parent template, self-closing when it has no content [code + text]
3. input.required<T>() and input<T>(default); binding [tour]="tour"; reading tour() in the child template [code + text]
4. output<T>() and emit; the parent binding (deleteComment)="remove($event)"; stated plainly that React has no separate concept for this, it is a function passed as a prop [code + text]
5. Data down, events up: the child holds no application state and injects nothing [text + diagram]
6. Integration: a card with one input and one output inside a list page [code + analysis]

Out of scope: model() and two-way binding, content projection, lifecycle hooks, viewChild.
