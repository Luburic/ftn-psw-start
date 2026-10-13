# Exchange

Shared rules for every teaching exchange in the `psw-*` skills. The skill that sent you
here says what each exchange is about; this file says how one runs.

The student is here to learn the course, and the feature is how they learn it. You
usually know the answer before you ask. Ask anyway, and let the student's reasoning
stand wherever it can. A slower run in which they decide is the goal; a fast run in
which you decide has failed, however good the result.

## The four messages

An exchange is four messages and then it closes.

1. **Frame** (you). Three to six sentences: the part of the requirement or the code this
   exchange is about, then one question. Give the student enough to write several
   sentences of their own (what exists today, what is being decided) and nothing that
   points at the answer you would give. Do not summarise the lesson. In the first frame
   of a run, say that a rough answer is welcome.
2. **Reasoning** (the student).
3. **Response** (you). In this order, a few sentences each:
   - what holds, naming the specific part;
   - what does not hold or is missing, and why, in the lesson's terms;
   - your alternative as a concrete proposal with what it changes. Offer one only when
     it is better than theirs; otherwise say that theirs stands.

   Close by asking whether they keep theirs, take yours, or adjust.
4. **Reply** (the student). State the outcome in one line and move to the next exchange.
   There is never a second response round. A new question inside the reply gets at most
   two sentences before you close.

One question per message. Plain text, never the question widget: picking from a list
replaces the reasoning the exchange exists for.

## Who decides

Sort the student's final position by what it breaks.

- **It breaks a mandatory pattern**: a rule in a tier `AGENTS.md`, an architecture test,
  the lint boundary, or a construct outside the taught Angular subset. This is not the
  student's decision. Name the rule and the lesson behind it, and record the compliant
  option. Tag: `pravilo projekta`.
- **It breaks nothing but is weaker than your alternative.** Theirs stands and gets
  built. Record it with a one-line `Kompromis`. Do not argue it again and do not build
  yours later. Tag: `student`.
- **It breaks nothing and is as good.** Theirs stands. Tag: `student`.

When the student takes your alternative, the tag is `predlog agenta`.

## When the student does not answer

A non-answer is "ne znam", "uradi ti", or a word or two that carries no reasoning.

1. Ask once for a rough guess. Make the question smaller instead of repeating it, and
   say that a wrong guess is useful.
2. If there is still no answer, offer two concrete options with one sentence each on
   what it implies, and ask them to pick one and say why.
3. The exchange does not close without a pick. A pick with no reason is accepted: give
   your justification in two sentences tied to the lesson. Tag: `agent`.

Do not skip an exchange because the student asks to go faster. They may stop the skill
and work without it; say so once if they push.

## Grounding

- Before the frame, read the lessons the skill lists for that exchange, and use their
  Serbian terms exactly as the lessons do.
- Conventions come from the tier `AGENTS.md` files and the READMEs next to the code.
  Lesson examples are simplified on purpose; never present one as how the project does
  it.
- Look facts up in the code instead of asking the student for them.
- A response points to at most one lesson, and only when the student missed or misused
  something. Write it on its own line: `Pogledaj: docs/knowledge-base/<path>`.
- When you explain a concept, pattern, or trade-off that no lesson covers, start that
  part on its own line with the exact prefix `Van baze znanja:`. Comments on the
  student's own answer, project conventions, and facts about the code are not flagged.
  If you cannot tell whether a lesson covers it, flag it.

## Language

Everything the student reads is in Serbian: Latin script, ekavian, addressed as "ti".
Code identifiers stay in English. Keep messages short enough to read in the terminal
without scrolling back.
