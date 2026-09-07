TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: component scoping and the four global files, as mechanics. Ownership belongs to the monolith segment.

Outline:
1. Anchor: a student writes .card { padding: 0 } in a component stylesheet and nothing changes elsewhere, or styles .error locally and finds it already red; the global classes every template uses unqualified [code + text]
2. Emulated encapsulation: a component's styles apply only to its template; the same class name in two components does not collide [code + text]
3. styles.scss and the four partials; what each holds [tree + text]
4. Tokens as CSS custom properties on :root; theming by editing one file [code + text]
5. The SCSS subset used: partials and @use, nothing else [code + text]
6. The decision procedure for a new style: token, global class, component [numbered list]
7. ::ng-deep named and forbidden [text]
8. Integration: styling a new card page with the procedure [code]

Out of scope: who owns the global files and the promotion rule (monolith segment), Sass variables and mixins, responsive layout, animations.
