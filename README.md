<h1 align="center" style="position: relative;">
  <img width="200" style="border-radius: 50%;" src="logo.png" alt="Vivian Logo" /><br>
  Vivian
</h1>

<h3 align="center">A statically typed programming language powered by .NET</h3>

## How to Operate the REPL 

The Vivian REPL is a simple Console Text Editor with a few basic controls: 
```yaml
- PgUp: Cycle up through submission history
- PgDn: Cycle down through submission history
- End: Returns to the last character in the sequence
- Home: Returns to the first character in the sequence 
- Arrow Keys: Used for navigating around the text
- Tab: 4 space indent
- Enter: Without a scope open, it will automatically submit the input
- Ctrl Enter: Used to continue the expression on a new line (ex -> 12 + \n 4)
```
## Example Syntax 
Vivian as of currently is an implicitly typed language containing only bools, ints, and basic expression evaluation.

```c#
{
    var result = 20 // var is used to represent a mutable variable
    let result = 20 // let is used to represent a constant varable
    // variables cannot be redeclared
    
    
    // Basic for loop 
    for i = 0 to 100 
    {
        result = result + i
    }
    
    // Basic if/else/else-if
    if result <= 200
    {
        result = 0
    }
    else if result == 20
    {
        result = 0
    }
    else
    {
        result = 9000
    }
    
    // Basice while loop
    while result < 20
    {
        result = result - 1
        result = result + 1
    }
}

```
