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
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+1", 1)]
        [InlineData("-1", -1)]
        
        [InlineData("14 + 12", 26)]
        [InlineData("12 - 3", 9)]
        [InlineData("4 * 2", 8)]
        [InlineData("9 / 3", 3)]
        [InlineData("(10)", 10)]
        [InlineData("12 == 3", false)]
        [InlineData("3 == 3", true)]
        [InlineData("12 != 3", true)]
        [InlineData("3 != 3", false)]
        
        [InlineData("3 < 5", true)]
        [InlineData("5 < 4", false)]
        [InlineData("4 >= 5", false)]
        [InlineData("4 <= 5", true)]
        
        [InlineData("~1", -2)]

        [InlineData("1 | 2", 3)]
        [InlineData("1 | 0", 1)]
        
        [InlineData("1 & 3", 1)]
        [InlineData("1 & 0", 0)]
        
        [InlineData("1 ^ 0", 1)]
        [InlineData("0 ^ 1", 1)]
        [InlineData("1 ^ 3", 2)]

        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("!false", true)]
        [InlineData("!true", false)]
        [InlineData("false == false", true)]
        [InlineData("true == false", false)]
        [InlineData("false != false", false)]
        [InlineData("true != false", true)]
        
        [InlineData("false | false", false)]
        [InlineData("false | true", true)]
        [InlineData("true | false", true)]
        [InlineData("true | true", true)]
        
        [InlineData("false & false", false)]
        [InlineData("false & true", false)]
        [InlineData("true & false", false)]
        [InlineData("true & true", true)]
        
        [InlineData("false ^ false", false)]
        [InlineData("true ^ false", true)]
        [InlineData("false ^ true", true)]
        [InlineData("true ^ true", false)]
        
        [InlineData("\"test\"", "test")]
        [InlineData("\"te\"\"st\"", "te\"st")]
        [InlineData("\"test\" == \"test\"", true)]
        [InlineData("\"test\" != \"test\"", false)]
        [InlineData("\"abc\" != \"abc\"", false)]
        [InlineData("\"abc\" == \"abc\"", true)]
        
        [InlineData("{ var a = 0; return (a = 10) * a; }", 100)]
        
        [InlineData("{ var a = 0; if (a == 0) a = 10; return a; }", 10)]
        
        [InlineData("{ var i = 10; var result = 0; while (i > 0) { result = result + i; i = i - 1; } return result; }", 55)]
        
        [InlineData("{ var result = 0; for (i = 1 to 10) { result = result + i; } return result; }", 55)]
        
        [InlineData("{ var a = 0; do a = a + 1; while (a < 10); return a; }", 10)]
        
        [InlineData("{ var a : object = 0 var b : object = \"b\" return a == b; }", false)]
        [InlineData("{ var a : object = 0 var b : object = \"b\" return a != b; }", true)]
        [InlineData("{ var a : object = 0 var b : object = 0 return a == b; }", true)]
        [InlineData("{ var a : object = 0 var b : object = 0 return a != b; }", false)]
        
        // [InlineData("{ var i = 0 while i < 5 { i = i + 1 if i == 5 continue } i }", 5)]
        // [InlineData("{ var i = 0 do { i = i + 1 if i == 5 continue } while i < 5 i }", 5)]
        
        public void Evaluator_Computes_CorrectValues(string text, object expectedValue)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = Compilation.CreateScript(null, syntaxTree);
            var variables = new Dictionary<VariableSymbol, object>();
            var result = compilation.Evaluate(variables);
            Assert.Empty(result.Diagnostics);
            
            Assert.Equal(expectedValue, result.Value);
        }

        [Fact]
        public void Evaluator_VariableDeclaration_Reports_Redeclaration()
        {
            var text = @"
                {
                    var x = 10;
                    var y = 100;
                    {
                        var x = 100;
                    }            
                    var [x] = 5;
                }
            ";
            var diagnostics = @"
                'x' is already declared.
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_FunctionParameters_NoInfiniteLoop()
        {
            var text = @"
                function printer(name: string[[[=]]][)]
                { 
                    print(""hi "" + name + ""!"");
                }[]
            ";

            var diagnostics = @"
                Unexpected token: 'EqualsToken', expected: 'CloseParenthesisToken'.
                Unexpected token: 'EqualsToken', expected: 'OpenBraceToken'.
                Unexpected token: 'EqualsToken', expected: 'IdentifierToken'.
                Unexpected token: 'CloseParenthesisToken', expected: 'IdentifierToken'.
                Unexpected token: 'EndOfFileToken', expected: 'CloseBraceToken'.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_AssignmentExpression_Reports_NotAVariable()
        {
            var text = @"[print] = 42;";

            var diagnostics = @"
                'print' is not a variable.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_CallExpression_Reports_Undefined()
        {
            var text = @"[foo](42);";

            var diagnostics = @"
                Function 'foo' does not exist in the current context.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_NotAFunction()
        {
            var text = @"
                {
                    const foo = 42;
                    [foo](42);
                }
            ";

            var diagnostics = @"
                'foo' is not a function.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_FunctionReturn_Missing()
        {
            var text = @"
                function [add](a: int, b: int): int
                {
                }
            ";

            var diagnostics = @"
                Not all code paths return a value.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_Variables_Can_Shadow_Functions()
        {
            var text = @"
                {
                    const print = 42;
                    [print](""test"");
                }
            ";

            var diagnostics = @"
                'print' is not a function.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Name_Reports_Undefined()
        {
            var text = @"[x] + 10";
            
            var diagnostics = @"
                Variable 'x' does not exist in the current context.
            ";
            AssertDiagnostics(text, diagnostics);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = Compilation.CreateScript(null, syntaxTree);
            var variables = new Dictionary<VariableSymbol, object>();
            var result = compilation.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedValue, result.Value);
        }
        
        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = Compilation.CreateScript(null, syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
                throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics");

            Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);
            
            for (var i = 0; i < expectedDiagnostics.Length; i++)
            {
                var expectedMessage = expectedDiagnostics[i];
                var actualMessage = result.Diagnostics[i].Message;
                Assert.Equal(expectedMessage, actualMessage);
                
                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = result.Diagnostics[i].Location.Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}