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
- `object`
- basic expression evaluation, and
- an early implementation of functions

```c#
{
  
    // data types
    int: whole numbers
    bool: true/false
    string: "sentences encased in double quotes"
    object: unboxed object that can represent any type
    
    // built-in conversions
    int(string input) // converts a string to an int, if a valid integer is given.
    string(int input) // converts an int to a string.
    bool(string/int input) // converts either string or int to bool.
    object(object input) // converts anything to an unboxed object implicitly.

    // Remember, variables cannot be redeclared.
    
    // for loop 
    for (i = 0 to 100) 
    {
        result = result + i
    }
    
    // if/else if/else statements
    if (result <= 200)
    {
        result = 0
    }
    else if (result == 20)
    {
        result = 0
    }
    else
    {
        result = 9000
    }
    
    // while loop
    while (result < 20)
    {
        result = result - 1
        result = result + 1
    }

    // do while loop
    do
    {
        result = result + 1
    } while (result < 25)

    // built-in functions
    print(string output)
    input(string input)
    rnd(int max)

    // custom function declaration
    void name(string param, int other) 
    {
         // your code goes here!
    }

    // custom function example
    void myPrint(string text, int number)
    {
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
    string message = " " 
    for (i = 0 to 100) 
    {
        if (i % 3 == 0)
            message = "fizz"
        else if (i % 5 == 0) 
            message = "buzz"
        else if (i % 3 == 0 && i % 5 == 0)
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
    bool factor = true
    while (factor == true)
    {   
        print("I'm thinking of a number between 0 and 10")
        int answer = input()
        int num = rnd(10)
        if (int(answer) == num)
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
void randomFunctionOne()
{
    print("I am random function number one.")
    print("Congrats on picking me!")
}

void randomFunctionTwo()
{
    print("I am random function number two.")
    print("Pleased to make your acquaintance.")
}

void randomFunctionThree()
{
    print("Howdy, I'm rando function numba three!")
    print("YEEEEEHAW")
}

{
    int randomNumber = rnd(2) + 1
    if (randomNumber == 1)
        randomFunctionOne()
    else if (randomNumber == 2)
        randomFunctionTwo()
    else if (randomNumber == 3)
        randomFunctionThree()
}
```
