using System;
using System.Collections.Generic;
using Vivian.CodeAnalysis;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;
using Xunit;

namespace Vivian.Tests.CodeAnalysis
{
    public class EvaluationTests
    {
        [Fact]
        public void Evaluator_VariableDeclaration_Reports_Redeclaration()
        {
            const string? text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
            ";

            const string? diagnostics = @"
                VE0020: 'x' is already declared.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_BinaryExpression_DivisionByZero()
        {
            const string? text = @"
                var x: int32 = [1 / 0]
            ";

            const string? diagnostics = @"
                VE0002: Division by zero.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_BlockStatement_NoInfiniteLoop()
        {
            const string? text = @"
                {
                [)][]
            ";

            const string? diagnostics = @"
                VE0008: Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
                VE0008: Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_Missing()
        {
            const string? text = @"
                WriteLine([)]
            ";

            const string? diagnostics = @"
                VE0026: Function 'WriteLine' requires 1 arguments but was given 0.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_Exceeding()
        {
            const string? text = @"
                WriteLine(""Hello""[, "" "", "" world!""])
            ";

            const string? diagnostics = @"
                VE0026: Function 'WriteLine' requires 1 arguments but was given 3.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop()
        {
            const string? text = @"
                WriteLine(""Hi""=[)]
            ";

            const string? diagnostics = @"
                VE0008: Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_FunctionReturn_Missing()
        {
            const string? text = @"
                procedure [add](a: int32, b: int32) => int32 
                {
                }
            ";

            const string? diagnostics = @"
                VE0029: Not all code paths return a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_IfStatement_Reports_CannotConvert()
        {
            const string? text = @"
                {
                    var x = 0
                    if([10])
                        x = 10
                }
            ";

            const string? diagnostics = @"
                VE0018: Cannot convert type <int32> to <bool>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_WhileStatement_Reports_CannotConvert()
        {
            const string? text = @"
                {
                    var x = 0
                    while([10])
                        x = 10
                }
            ";

            const string? diagnostics = @"
                VE0018: Cannot convert type <int32> to <bool>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_DoWhileStatement_Reports_CannotConvert()
        {
            const string? text = @"
                {
                    var x = 0
                    do
                        x = 10
                    while([10])
                }
            ";

            const string? diagnostics = @"
                VE0018: Cannot convert type <int32> to <bool>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert_LowerBound()
        {
            const string? text = @"
                {
                    var result = 0
                    for(i = [false] to 10)
                        result = result + i
                }
            ";

            const string? diagnostics = @"
                VE0018: Cannot convert type <bool> to <int32>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert_UpperBound()
        {
            const string? text = @"
                {
                    var result = 0
                    for(i = 1 to [true])
                        result = result + i
                }
            ";

            const string? diagnostics = @"
                VE0018: Cannot convert type <bool> to <int32>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_Undefined()
        {
            const string? text = @"[x] * 10";

            const string? diagnostics = @"
                VE0014: Variable 'x' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_NoErrorForInsertedToken()
        {
            const string? text = @"1 + []";

            const string? diagnostics = @"
                VE0008: Unexpected token <EndOfFileToken>, expected <IdentifierToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_UnaryExpression_Reports_Undefined()
        {
            const string? text = @"[+]true";

            const string? diagnostics = @"
                VE0010: Unary operator '+' is not defined for type <bool>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_BinaryExpression_Reports_Undefined()
        {
            const string? text = @"10 [*] false";

            const string? diagnostics = @"
                VE0011: Binary operator '*' is not defined for types <int32> and <bool>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_CompoundExpression_Reports_Undefined()
        {
            const string? text = @"var x = 10
                         x [+=] false";

            const string? diagnostics = @"
                VE0011: Binary operator '+=' is not defined for types <int32> and <bool>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_Undefined()
        {
            const string? text = @"[x] = 10";

            const string? diagnostics = @"
                VE0014: Variable 'x' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

         [Fact]
        public void Evaluator_CompoundExpression_Assignment_NonDefinedVariable_Reports_Undefined()
        {
            const string? text = @"[x] += 10";

            const string? diagnostics = @"
                VE0014: Variable 'x' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_NotAVariable()
        {
            const string? text = @"[WriteLine] = 42";

            const string? diagnostics = @"
                VE0015: 'WriteLine' is not a variable.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_CannotAssign()
        {
            const string? text = @"
                {
                    const x = 10
                    x [=] 0
                }
            ";

            const string? diagnostics = @"
                VE0021: Variable 'x' is read-only and cannot be assigned to.
            ";

            AssertDiagnostics(text, diagnostics);
        }

         [Fact]
        public void Evaluator_CompoundDeclarationExpression_Reports_CannotAssign()
        {
            const string? text = @"
                {
                    const x = 10
                    x [+=] 1
                }
            ";

            const string? diagnostics = @"
                VE0021: Variable 'x' is read-only and cannot be assigned to.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_CannotConvert()
        {
            const string? text = @"
                {
                    var x = 10
                    x = [true]
                }
            ";

            const string? diagnostics = @"
                VE0018: Cannot convert type <bool> to <int32>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_Undefined()
        {
            const string? text = @"[foo](42)";

            const string? diagnostics = @"
                VE0022: Function 'foo' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_NotAFunction()
        {
            const string? text = @"
                {
                    const foo = 42
                    [foo](42)
                }
            ";

            const string? diagnostics = @"
                VE0023: 'foo' is not a function.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Variables_Can_Shadow_Functions()
        {
            const string? text = @"
                {
                    const print = 42
                    [print](""test"")
                }
            ";

            const string? diagnostics = @"
                VE0023: 'print' is not a function.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Void_Function_Should_Not_Return_Value()
        {
            const string? text = @"
                procedure test() => void
                {
                    return [1]
                }
            ";

            const string? diagnostics = @"
                VE0030: Since the function 'test' does not return a value the 'return' keyword cannot be followed by an expression.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Function_With_ReturnValue_Should_Not_Return_Void()
        {
            const string? text = @"
                procedure test() => int32
                {
                    [return]
                }
            ";

            const string? diagnostics = @"
                VE0032: An expression of type <int32> is expected.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Not_All_Code_Paths_Return_Value()
        {
            const string? text = @"
                procedure [test](n: int32) => bool
                {
                    if (n > 10)
                       return true
                }
            ";

            const string? diagnostics = @"
                VE0029: Not all code paths return a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Expression_Must_Have_Value()
        {
            const string? text = @"
                procedure test(n: int32) => void
                {
                    return
                }

                const value = [test(100)]
            ";

            const string? diagnostics = @"
                VE0027: Expression must have a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_IfStatement_Reports_NotReachableCode_Warning()
        {
            const string? text = @"
                procedure test() => void
                {
                    const x = 4 * 3
                    if(x > 12)
                    {
                        [WriteLine](""x"")
                    }
                    else
                    {
                        WriteLine(""x"")
                    }
                }
            ";

            const string? diagnostics = @"
                VE0045: Unreachable code detected.
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ElseStatement_Reports_NotReachableCode_Warning()
        {
            const string? text = @"
                procedure test() => int32
                {
                    if(true)
                    {
                        return 1
                    }
                    else
                    {
                        [return] 0
                    }
                }
            ";

            const string? diagnostics = @"
                VE0045: Unreachable code detected.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_WhileStatement_Reports_NotReachableCode_Warning()
        {
            const string? text = @"
                procedure test() => void 
                {
                    while(false)
                    {
                        [continue]
                    }
                }
            ";

            const string? diagnostics = @"
                VE0045: Unreachable code detected.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Theory]
        [InlineData("[break]", "break")]
        [InlineData("[continue]", "continue")]
        public void Evaluator_Invalid_Break_Or_Continue(string text, string keyword)
        {
            var diagnostics = $@"
                VE0028: The keyword '{keyword}' can only be used inside of loops.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Parameter_Already_Declared()
        {
            const string? text = @"
                procedure sum(a: int32, b: int32, [a: int32]) => int32
                {
                    return a + b + c
                }
            ";

            const string? diagnostics = @"
                VE0012: A parameter with the name 'a' already exists.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Function_Must_Have_Name()
        {
            const string? text = @"
                procedure [(]b: int32, b: int32) => int32
                {
                    return a + b
                }
            ";

            const string? diagnostics = @"
                VE0008: Unexpected token <OpenParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Wrong_Argument_Type()
        {
            const string? text = @"
                procedure test(n: int32) => bool
                {
                    return n > 10
                }
                const testValue = ""string""
                test([testValue])
            ";

            const string? diagnostics = @"
                VE0019: Cannot convert type <string> to <int32>. An explicit conversion exists (are you missing a cast?)
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Bad_Type()
        {
            const string? text = @"
                procedure test(n: [invalidtype]) => void
                {
                }
            ";

            const string? diagnostics = @"
               VE0017: Type 'invalidtype' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Cannot_Access_Member()
        {
            const string? text = @"
                var p = 0

                WriteLine([p].[[length]])
            ";

            const string? diagnostics = @"
                VE0016: 'p' is not a valid class.
                VE0023: 'length' is not a function.
                VE0039: Cannot access members of 'p'. Only members of classes can be accessed using the '.' operator.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = Compilation.Create(syntaxTree);
            var diagnostics = compilation.Validate();

            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
            {
                throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics");
            }

            Assert.Equal(expectedDiagnostics.Length, diagnostics.Length);

            for (var i = 0; i < expectedDiagnostics.Length; i++)
            {
                var expectedMessage = expectedDiagnostics[i];
                var actualMessage = diagnostics[i].Message;
                Assert.Equal(expectedMessage, actualMessage);

                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = diagnostics[i].Location.Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}