<h1 align="center" style="position: relative;">
  <img width="200" style="border-radius: 50%;" src="logo.png" alt="Vivian Logo" /><br>
  Vivian
</h1>

<h3 align="center">A strongly typed programming language powered by .NET</h3>

## How to Operate the REPL 
The Vivian REPL is a Console Text Editor with some basic controls: 
```yaml
- PgUp: Cycle up through submission history
- PgDn: Cycle down through submission history
- End: Moves the cursor to the last character in the sequence
- Home: Moves the cursor to the first character in the sequence 
- Arrow Keys: Used for navigating around the text
- Tab: 4 space indent
- Enter: Submits current input
  - Input will only be submitted if the cursor is not within a scope (between {} curly brackets)
- Ctrl Enter: Used to continue the expression on a new line (ex -> 12 + \n 4)
```

## Example Syntax 
Vivian is currently an implicitly typed language.
It contains:
- `bool`
- `int`
- `string`
- basic expression evaluation, and
- an early implementation of functions

```c#
{
    // built-in conversions
    int(string input) // converts a string to an int, if a valid integer is given
    string(int input) // converts an int to a string
    bool(string/int input) // converts either string or int to bool

    // variable declaration types
    imply result = 20 // imply represents a mutable variable
    let result = 20 // let represents a constant varable
    // Remember, variables cannot be redeclared.
    
    // for loop 
    for i = 0 to 100 
    {
        result = result + i
    }
    
    // if/else if/else statements
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
    
    // while loop
    while result < 20
    {
        result = result - 1
        result = result + 1
    }

    // do while loop
    do
    {
        result = result + 1
    } while result < 25

    // built-in functions
    print(string output)
    input(string input)
    rnd(int max)

    // custom function declaration
    function name(param: string, other: int) 
    {
         // your code goes here!
    }

    // custom function example
    function myPrint(text: string, number: int) {
        print(text)
        print(string(number))
    }

    // calling custom functions
    myPrint("Hello, myPrint!", 100)
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

## Custom Function Fun
```c#
function randomFunctionOne()
{
    print("I am random function number one.")
    print("Congrats on picking me!")
}

function randomFunctionTwo()
{
    print("I am random function number two.")
    print("Pleased to make your acquaintance.")
}

function randomFunctionThree()
{
    print("Howdy, I'm rando function numba three!")
    print("YEEEEEHAW")
}

{
    let randomNumber = rnd(2) + 1
    if randomNumber == 1
        randomFunctionOne()
    else if randomNumber == 2
        randomFunctionTwo()
    else if randomNumber == 3
        randomFunctionThree()
}
```
