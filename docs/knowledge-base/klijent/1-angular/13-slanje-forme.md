TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: sending a form: submit, pending, navigation, reset.

Outline:
1. Anchor: the create form from the previous lesson must reach the server and land on the list [text]
2. The submit handler: the (submit) event on the form element and preventDefault, since there is no framework submit event; the command pattern from the commands lesson reused unchanged; the button disabled on !form().valid() || pending() [code + text]
3. Navigating after success with inject(Router).navigate, the second injection the students meet, reused without re-explaining [code + text]
4. reset with the initial model for a form that stays on screen, on the project's comment form [code]
5. Integration: the create tour page in full, completing the forms lesson's example [code + analysis]

Out of scope: the submit() helper, cross-field validation, Reactive Forms.
