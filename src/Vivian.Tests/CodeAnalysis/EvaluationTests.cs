using System;
using System.Collections.Generic;
using wsc.CodeAnalysis;
using wsc.CodeAnalysis.Symbols;
using wsc.CodeAnalysis.Syntax;
using Xunit;

namespace WSC.Tests.CodeAnalysis
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
        
        [InlineData("{ var a = 0 (a = 10) * a }", 100)]
        
        [InlineData("{ var a = 0 if a == 0 a = 10 a }", 10)]
        
        [InlineData("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1 } result }", 55)]
        
        [InlineData("{ var result = 0 for i = 1 to 10 { result = result + i } result }", 55)]
        
        public void Evaluator_Computes_CorrectValues(string text, object expectedValue)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = new Compilation(syntaxTree);
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
                    var x = 10
                    var y = 100
                    {
                        var x = 100
                    }            
                    var [x] = 5
                }
            ";
            var diagnostics = @"
                Variable <x> is already declared.
            ";
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_Name_Reports_Undefined()
        {
            var text = @"[x] + 10";
            
            var diagnostics = @"
                Variable <x> does not exist in the current context.
            ";
            AssertDiagnostics(text, diagnostics);
        }

        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
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
                var actualSpan = result.Diagnostics[i].Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}