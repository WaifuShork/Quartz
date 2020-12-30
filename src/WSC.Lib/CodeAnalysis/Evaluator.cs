using System;
using System.Collections.Generic;
using wsc.CodeAnalysis.Binding;

namespace wsc.CodeAnalysis
{
    /// <summary>
    /// Represents a basic text evaluator for the compiler.
    /// </summary>
    internal sealed class Evaluator
    {
        private readonly BoundStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue;
        
        /// <summary>
        /// Constructor of the evaluator.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="variables"></param>
        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }
        
        /// <summary>
        /// Helper method for EvaluateStatement.
        /// </summary>
        /// <returns>Returns the last value.</returns>
        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }

        /// <summary>
        /// Represents a statement evaluator, to determine of the current block is a Statement, Variable, or Expression.
        /// </summary>
        /// <param name="node"></param>
        /// <exception cref="Exception"></exception>
        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement((BoundBlockStatement) node);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration) node);
                    break;
                
                case BoundNodeKind.IfStatement:
                    EvaluateIfStatement((BoundIfStatement) node);
                    break;
                
                case BoundNodeKind.WhileStatement:
                    EvaluateWhileStatement((BoundWhileStatement) node);
                    break;
                
                case BoundNodeKind.ForStatement:
                    EvaluateForStatement((BoundForStatement) node);
                    break;
                
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement) node);
                    break;

                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        /// <summary>
        /// Evaluates the for loop statement.
        /// </summary>
        /// <param name="node"></param>
        private void EvaluateForStatement(BoundForStatement node)
        {
            var lowerBound = (int)EvaluateExpression(node.LowerBound);
            var upperBound = (int)EvaluateExpression(node.UpperBound);

            for (var i = lowerBound; i <= upperBound; i++)
            {
                _variables[node.Variable] = i;
                EvaluateStatement(node.Body);
            }
        }

        /// <summary>
        /// Evaluates the while loop statement.
        /// </summary>
        /// <param name="node"></param>
        private void EvaluateWhileStatement(BoundWhileStatement node)
        {
            while ((bool) EvaluateExpression(node.Condition))
                EvaluateStatement(node.Body);
        }
        
        /// <summary>
        /// Evaluates the if statement.
        /// </summary>
        /// <param name="node"></param>
        private void EvaluateIfStatement(BoundIfStatement node)
        {
            var condition = (bool) EvaluateExpression(node.Condition);
            if (condition)
                EvaluateStatement(node.ThenStatement);
            else if (node.ElseStatement != null)
                EvaluateStatement(node.ElseStatement);
        }

        /// <summary>
        /// Evaluates a variable declaration.
        /// </summary>
        /// <param name="node"></param>
        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
        }

        /// <summary>
        /// Evaluates a given block statement => { }.
        /// </summary>
        /// <param name="node"></param>
        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (var statement in node.Statements)
            {
                EvaluateStatement(statement);
            }
        }

        /// <summary>
        /// Evaluates basic "expressions" seen as by the compiler.
        /// </summary>
        /// <param name="node"></param>
        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }
        
        /// <summary>
        /// Evaluates whether an expression is literal, variable, assignment, unary, or binary.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private object EvaluateExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression((BoundLiteralExpression) node);
                
                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression((BoundVariableExpression) node);
                
                case BoundNodeKind.AssignmentExpression:
                    return EvaluateAssignmentExpression((BoundAssignmentExpression) node);
                
                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression((BoundUnaryExpression) node);
                
                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression((BoundBinaryExpression) node);
                
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }
        
        /// <summary>
        /// Evaluates a literal expression.
        /// </summary>
        /// <param name="n"></param>
        /// <returns>Returns a BoundLiteralExpression value</returns>
        private static object EvaluateLiteralExpression(BoundLiteralExpression n)
        {
            return n.Value;
        }
        
        /// <summary>
        /// Evaluates a variable expression.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>Returns a BoundVariableExpression.</returns>
        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            return _variables[v.Variable];
        }
        
        /// <summary>
        /// Evaluates an assignment expression.
        /// </summary>
        /// <param name="a"></param>
        /// <returns>Returns a BoundAssignmentExpression.</returns>
        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            _variables[a.Variable] = value;
            return value;
        }

        /// <summary>
        /// Evaluates a unary expression based on the operator given.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>Returns BoundUnaryExpression.</returns>
        /// <exception cref="Exception"></exception>
        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);

            switch (u.Op.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return (int) operand;
                case BoundUnaryOperatorKind.Negation:
                    return -(int) operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool) operand;
                default:
                    throw new Exception($"Unexpected unary operator {u.Op}");
            }
        }
        
        /// <summary>
        /// Evaluates a binary expression.
        /// </summary>
        /// <param name="b"></param>
        /// <returns>Returns a BoundBinaryExpression.</returns>
        /// <exception cref="Exception"></exception>
        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return (int) left + (int) right;
                case BoundBinaryOperatorKind.Subtraction:
                    return (int) left - (int) right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int) left * (int) right;
                case BoundBinaryOperatorKind.Division:
                    return (int) left / (int) right;
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool) left && (bool) right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool) left || (bool) right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.Less:
                    return (int) left < (int) right;
                case BoundBinaryOperatorKind.Greater:
                    return (int) left > (int) right;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    return (int) left >= (int) right;
                case BoundBinaryOperatorKind.LessOrEquals:
                    return (int) left <= (int) right;

                default:
                    throw new Exception($"Unexpected binary operator {b.Op}");
            }
        }
    }
}