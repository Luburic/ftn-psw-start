TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: Signal Forms: model, schema, validators, field state. No submit yet.

Outline:
1. Anchor: a create page needs validation before sending; the HTML form the students know [text]
2. A model signal and form(model, schema); definition of form and schema, on the create-tour model [code + text]
3. Validators required and min, each with a message, the two the module code uses; the other validators follow the same shape and are not named [code + text]
4. The one rule for the three call shapes: form.x is the field to bind, form.x() is that field's state, form() is the whole form's state; the four reads used: value, touched, errors, valid [text + table]
5. [formField] on input and select, a directive like routerLink; a number input yields a number in the model [code + text]
6. Showing errors only after touched, with @for over errors() and track $index [code + text]
7. Integration: the create-tour page's class and template up to validation, with the submit handler omitted; the sending-a-form lesson completes the same page [code + analysis]

Out of scope: submit, preventDefault, navigation, reset, the submit() helper, cross-field validation, email and minLength, Reactive and template-driven forms.
