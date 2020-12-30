using System;
using System.Collections;
using System.Collections.Generic;
using wsc.CodeAnalysis.Syntax;
using wsc.CodeAnalysis.Text;

namespace wsc.CodeAnalysis
{
    /// <summary>
    /// Represents a container for all Diagnostic error messages.
    /// </summary>
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();
        
        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        /// <summary>
        /// Adds a Diagnostic report to the last position of the DiagnosticBag.
        /// </summary>
        /// <param name="diagnostics"></param>
        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        /// <summary>
        /// Reports an error with the given TextSpan and a message containing the error message. 
        /// </summary>
        /// <param name="span"></param>
        /// <param name="message"></param>
        private void Report(TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        /// <summary>
        /// Error message for an invalid number.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            var message = $"The number <{text}> isn't valid <{type}>.";
            Report(span, message);
        }

        /// <summary>
        /// Error message for a bad token/character in the input.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="character"></param>
        public void ReportBadCharacter(int position, char character)
        {
            var span = new TextSpan(position, 1);
            var message = $"Bad character input: <{character}>.";
            Report(span, message);
        }
        
        /// <summary>
        /// Error message for an unexpected token.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="actualKind"></param>
        /// <param name="expectedKind"></param>
        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token: <{actualKind}>, expected: <{expectedKind}>.";
            Report(span, message);
        }

        /// <summary>
        /// Error message for an undefined unary operator.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="operatorText"></param>
        /// <param name="operandType"></param>
        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, Type operandType)
        {
            var message = $"Unary operator <{operatorText}> is not defined for type <{operandType}>.";
            Report(span, message);
        }

        /// <summary>
        /// Error message for an undefined binary operator.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="operatorText"></param>
        /// <param name="leftType"></param>
        /// <param name="rightType"></param>
        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, Type leftType, Type rightType)
        {
            var message = $"Binary operator <{operatorText}> is not defined for type <{leftType}> and <{rightType}>.";
            Report(span, message);
        }

        /// <summary>
        /// Error message for an undefined variable name.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="name"></param>
        public void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Variable <{name}> does not exist in the current context.";
            Report(span, message);
        }
        

        /// <summary>
        /// Error message for invalid conversion operations.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        public void ReportCannotConvert(TextSpan span, Type fromType, Type toType)
        {
            var message = $"Cannot convert type <{fromType}> to {toType}.";
            Report(span, message);
        }

        /// <summary>
        /// Error message for a variable that's already been declared.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="name"></param>
        public void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"Variable <{name}> is already declared.";
            Report(span, message);
        }

        /// <summary>
        /// Error message for an unassignable variable.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="name"></param>
        public void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"Variable <{name}> is read-only and cannot be assigned to.";
            Report(span, message);
        }
    }
}