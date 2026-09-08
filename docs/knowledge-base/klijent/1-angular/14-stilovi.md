TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: component scoping and the global files, as mechanics. Ownership belongs to the monolith segment.

Outline:
1. Anchor: a student writes .card { padding: 0 } in a component stylesheet and nothing changes elsewhere, or styles .error locally and finds it already red; the global classes every template uses unqualified [code + text]
2. Component scoping: a component's styles apply only to its template, with the mechanism in one sentence, the compiler adds an attribute to every element of the template and rewrites each selector to require it; the same class name in two components does not collide; a component cannot reach into a child's template [code + text]
3. The global file: styles.scss is four @use lines and nothing else, one per partial, with what each partial holds; this is the whole SCSS subset used [tree + text]
4. Tokens as CSS custom properties on :root and var(--name) in a component stylesheet [code + text]
5. The decision procedure for a new style: is there a token, is there a global class, else the component file [numbered list]
6. Integration: styling the tour card from the component lesson with one token and one global class [code]

Out of scope: who owns the global files and the promotion rule (monolith segment), ::ng-deep, Sass variables and mixins, responsive layout, animations.
