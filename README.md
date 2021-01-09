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
    // built in functions
    print(string output)
    input(string input)
    rnd(int max)
        
    // built in conversions 
    int(string input) // converts a string to an int, if a valid integer is given
    string(int input) // converts an int to a string
    bool(string/int input) // converts either string or int to bool

    imply result = 20 // imply is used to represent a mutable variable
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

## Fizz Buzz Example
```c#
{   
    imply message = " " 
    for i = 0 to 100 
    {
        if i % 3 == 0
            message = "fizz"
        else if i % 5 == 0 
            message = "buzz"
        else if i % 3 == 0 && i % 5 == 0
            message = "fizzbuzz"
        else
            message = string(i)
        print(message)
}
```

## Number Guessing Game
```c#
{   
    imply factor = true
    while factor == true
    {   
        print("I'm thinking of a number between 0 and 10")
        imply answer = input()
        imply num = rnd(10)
        if int(answer) == num
        {
            print("Congrats!, the number I was thinking of was " + num)
            factor = false
        }
        else
            print("not quite!")
    }
}
```

