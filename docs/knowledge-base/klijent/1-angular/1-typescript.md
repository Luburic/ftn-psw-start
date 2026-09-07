TODO: lesson not written. Below is the planned outline: one line per instructional item, stated as its goal, with the expected format in brackets.
Prior knowledge assumed everywhere in klijent/: HTML, CSS, JavaScript (ES2015+, async/await, fetch), React basics (function components, props, useState), the server segments of this knowledge base.

Scope: the minimum TypeScript needed to read this project, nothing more.

Outline:
1. Anchor: open a page component from the project and mark every token a JavaScript reader would not recognise; motivate TypeScript as JavaScript plus type annotations the compiler checks before the browser ever runs the code [code + text]
2. Type annotation on variables, parameters and return values; the primitive types; the compiler error when a wrong type is passed [code + text]
3. Interface as the named shape of an object, using a DTO from api/ as the example [code + text]
4. Union of string literals as the way an enum arrives from the server ('Draft' | 'Published') and why a plain string is rejected [code + text]
5. Generic type parameter read as 'of': TourDto[], Promise<TourDto>, signal<string | null> [code + text]
6. Missing values: null and undefined as part of a type, and the ?., ?? operators the compiler makes necessary [code + text]
7. Class members: field initialisers without a constructor, readonly, private, protected [code + text]
8. Typed async functions: async returns Promise<T>, await unwraps, try/catch/finally around it [code + text]
9. The `as` cast: what it does and the project's two uses, an initial model value narrowed to a literal union ('Easy' as Difficulty) [code + text]
10. Integration: translate a 15-line JavaScript snippet into TypeScript step by step, each step named after an item above [code]

Out of scope: decorators (presented as Angular syntax in the next lesson), inheritance, enums, unknown and type narrowing, tsconfig.
