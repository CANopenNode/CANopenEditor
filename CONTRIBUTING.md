# CANopenEditor Contributing Guidelines

Thank you for your interest in contributing to this project!<br>
Being a well referenced EDS/DCF editor and the only one supporting CANopenNode, CANopenEditor let everyone manage CANopen files, completely free of charge!<br>
CANopenEditor is maintained by a small team of volunteers from around the world. As such, those rules are meant for a better communication between everyone but, any help being very welcome, reviewers might approve PRs not respecting all of these rules as long as you tried. Project owners are above those rules as they might have specific needs.<br>
Please note that by contributing to this project, you agree to have read this file and to the [Developer's Certificate of Origin 1.1](https://developercertificate.org/).

## Discussions
Discussions shall be done in respect to everyone and everyone's work.
Any major change shall be first discussed in an issue. Do not worry: maintainers are quite active and will answer.

## Code convention
This project doesn't really follow any real code convention and doesn't have any linter for now BUT following [common C# code conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) is very welcome and might be asked of you if some parts of your code are too hard to read.
If linting is ever needed, `dotnet format` will be our solution of choice.
Even tho there is no hard-defined conventions, non-explicit names or lack of spacing might result in non-approval of a PR.

## What might or might not be a PR
A simple refactor of code without any real change won't ever be approved. This project relies quite a bit on git blame and as such you are only allowed to refactor code you change. Moreover, trying to enforce a code convention without approval of the main maintainers first might be seen as disrespectful.

## Testing
Any code change shall be tested. You can:
- Add unit tests in the right directories
- Describe any tests you did with the files you might have used

Code that you didn't even try to compile won't be accepted.

## Code comments
- Complex algorithms MUST be explained with code comments.
- Hardcoded values (e.g. specific numbers in the middle of nowhere) MUST be justified with code comments.
- Non explicit data structures (e.g. an array of set size with every index having a meaning) MUST be explained with code comments.
- Commented lines of code MUST be preceded by a comment line starting with either `//// DEBUG code:` or `//// TODO:`.
- Comments SHALL have a space between the comment string (e.g. `//`) and the text content.
- Functions and methods MIGHT have comments describing what they are doing, what are their inputs and/or outputs (if it's not explicit enough).
- Loops and switches MIGHT have comments justifying why every case is taken into account (if it's not explicit enough).
- Any more useful comment is welcome.

## Names and descriptions
### PR names
They shall be composed of three sections:
1) Which project is affected (MIX if multiple)
2) What types of changes are made (e.g. FEAT, FIX, DOCUMENTATION/DOC, CORRECTION/CORR, SUGGESTION/SUGG, BUILD, ...)
3) An explicit but less than 100 chars long summary of the changes

Sections shall be clearly separated (e.g. with colons, parenthesis, ...).<br>
Examples of valid and explicit PR names: `[MIX][DOC] Explaining explicit PR title rules`, `gui2: fix: Fixed bug where 1 = 2`.<br>
If you have multiple unrelated changes (e.g. adding a feature and fixing another) to do, please do multiple PRs.

### PR descriptions
They shall:
- Describe any change made
- Explain why such changes are made
- Explain any decision taken
- Link any related issue / any related other PR
- Describe any test made
- The sentence "I have read the contributing guidelines, I agree to following them and I agree to the Developer's Certificate of Origin 1.1"

### Commits
Names shall be composed of:
1) What types of changes are made (e.g. PR names sections 2)
2) An explicit but less than 100 chars long summary of the changes

Descriptions are entirely up to you.
