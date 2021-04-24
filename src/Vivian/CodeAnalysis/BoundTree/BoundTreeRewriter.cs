using System;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public virtual BoundStatement RewriteStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    return RewriteBlockStatement((BoundBlockStatement) node);
                case BoundNodeKind.ConditionalGotoStatement:
                    return RewriteConditionalGotoStatement((BoundConditionalGotoStatement) node);
                case BoundNodeKind.DoWhileStatement:
                    return RewriteDoWhileStatement((BoundDoWhileStatement) node);
                case BoundNodeKind.ExpressionStatement:
                    return RewriteExpressionStatement((BoundExpressionStatement) node);
                case BoundNodeKind.ForStatement:
                    return RewriteForStatement((BoundForStatement) node);
                case BoundNodeKind.GotoStatement:
                    return RewriteGotoStatement((BoundGotoStatement) node);
                case BoundNodeKind.IfStatement:
                    return RewriteIfStatement((BoundIfStatement) node);
                case BoundNodeKind.LabelStatement:
                    return RewriteLabelStatement((BoundLabelStatement) node);
                case BoundNodeKind.MemberBlockStatement:
                    return RewriteBlockStatement((BoundMemberBlockStatement) node);
                case BoundNodeKind.NopStatement:
                    return RewriteNopStatement((BoundNopStatement) node);
                case BoundNodeKind.ReturnStatement:
                    return RewriteReturnStatement((BoundReturnStatement) node);
                case BoundNodeKind.SequencePointStatement:
                    return RewriteSequencePointStatement((BoundSequencePointStatement) node);
                case BoundNodeKind.VariableDeclaration:
                    return RewriteVariableDeclaration((BoundVariableDeclaration) node);
                case BoundNodeKind.WhileStatement:
                    return RewriteWhileStatement((BoundWhileStatement) node);
                default:
                    throw new InternalCompilerException($"Unexpected node: {node.Kind}");
            }
        }

        protected virtual BoundStatement RewriteBlockStatement(BoundStatement node)
        {
            ImmutableArray<BoundStatement>.Builder? builder = null;

            var boundStatements = node switch
            {
                BoundBlockStatement bbs => bbs.Statements,
                BoundMemberBlockStatement bmbs => bmbs.Statements,
                _ => throw new Exception($"Unexpected block statement type '{node.Kind}'."),
            };

            for (var i = 0; i < boundStatements.Length; i++)
            {
                var oldStatement = boundStatements[i];
                var newStatement = RewriteStatement(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(boundStatements.Length);

                        for (var j = 0; j < i; j++)
                        {
                            builder.Add(boundStatements[j]);
                        }
                    }
                }

                builder?.Add(newStatement);
            }

            if (builder == null)
            {
                return node;
            }

            return new BoundBlockStatement(node.Syntax, builder.MoveToImmutable());
        }

        protected virtual BoundStatement RewriteNopStatement(BoundNopStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
        {
            var initializer = RewriteExpression(node.Initializer);
            if (initializer == node.Initializer)
            {
                return node;
            }

            return new BoundVariableDeclaration(node.Syntax, node.Variable, initializer);
        }

        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var thenStatement = RewriteStatement(node.ThenStatement);
            var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);
            
            if (condition == node.Condition && 
                thenStatement == node.ThenStatement &&
                elseStatement == node.ElseStatement)
            {
                return node;
            }

            return new BoundIfStatement(node.Syntax, condition, thenStatement, elseStatement);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
            {
                return node;
            }

            return new BoundWhileStatement(node.Syntax, condition, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            var body = RewriteStatement(node.Body);
            var condition = RewriteExpression(node.Condition);
            if (body == node.Body && condition == node.Condition)
            {
                return node;
            }

            return new BoundDoWhileStatement(node.Syntax, body, condition, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var lowerBound = RewriteExpression(node.LowerBound);
            var upperBound = RewriteExpression(node.UpperBound);
            var body = RewriteStatement(node.Body);

            if (lowerBound == node.LowerBound && upperBound == node.UpperBound && body == node.Body)
            {
                return node;
            }

            return new BoundForStatement(node.Syntax, node.Variable, lowerBound, upperBound, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            var condition = RewriteExpression(node.Condition);

            if (condition == node.Condition)
            {
                return node;
            }

            return new BoundConditionalGotoStatement(node.Syntax, node.Label, condition, node.JumpIfTrue);
        }

        protected virtual BoundStatement RewriteReturnStatement(BoundReturnStatement node)
        {
            var expression = node.Expression == null ? null : RewriteExpression(node.Expression);

            if (expression == node.Expression)
            {
                return node;
            }

            return new BoundReturnStatement(node.Syntax, expression);
        }

        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
            {
                return node;
            }

            return new BoundExpressionStatement(node.Syntax, expression);
        }

        private BoundStatement RewriteSequencePointStatement(BoundSequencePointStatement node)
        {
            var statement = RewriteStatement(node.Statement);

            if (statement == node.Statement)
            {
                return node;
            }

            return new BoundSequencePointStatement(node.Syntax, statement, node.Location);
        }

        public virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.AssignmentExpression:
                    return RewriteAssignmentExpression((BoundAssignmentExpression) node);
                case BoundNodeKind.BinaryExpression:
                    return RewriteBinaryExpression((BoundBinaryExpression) node);
                case BoundNodeKind.CallExpression:
                    return RewriteCallExpression((BoundCallExpression) node);
                case BoundNodeKind.CompoundAssignmentExpression:
                    return RewriteCompoundAssignmentExpression((BoundCompoundAssignmentExpression) node);
                case BoundNodeKind.CompoundFieldAssignmentExpression:
                    return RewriteCompoundFieldAssignmentExpression((BoundCompoundFieldAssignmentExpression) node);
                case BoundNodeKind.ConversionExpression:
                    return RewriteConversionExpression((BoundConversionExpression) node);
                case BoundNodeKind.ErrorExpression:
                    return RewriteErrorExpression((BoundErrorExpression) node);
                case BoundNodeKind.FieldAccessExpression:
                    return RewriteFieldAccessExpression((BoundFieldAccessExpression) node);
                case BoundNodeKind.FieldAssignmentExpression:
                    return RewriteFieldAssignmentExpression((BoundFieldAssignmentExpression) node);
                case BoundNodeKind.LiteralExpression:
                    return RewriteLiteralExpression((BoundLiteralExpression) node);
                case BoundNodeKind.ThisExpression:
                    return RewriteThisExpression((BoundThisExpression) node);
                case BoundNodeKind.UnaryExpression:
                    return RewriteUnaryExpression((BoundUnaryExpression) node);
                case BoundNodeKind.VariableExpression:
                    return RewriteVariableExpression((BoundVariableExpression) node);
                default:
                    throw new InternalCompilerException($"Unexpected node: {node.Kind}");
            }
        }

        private BoundExpression RewriteThisExpression(BoundThisExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteCompoundFieldAssignmentExpression(BoundCompoundFieldAssignmentExpression node)
        {
            var classInstanceExpression = RewriteExpression(node.ClassInstance);
            var valueExpr = RewriteExpression(node.Expression);

            if (classInstanceExpression == node.ClassInstance && valueExpr == node.Expression)
            {
                return node;
            }

            return new BoundCompoundFieldAssignmentExpression(node.Syntax, classInstanceExpression, node.ClassMember, node.Op, valueExpr);
        }

        private BoundExpression RewriteFieldAssignmentExpression(BoundFieldAssignmentExpression node)
        {
            var classInstanceExpression = RewriteExpression(node.ClassInstance);
            var valueExpr = RewriteExpression(node.Expression);

            if (classInstanceExpression == node.ClassInstance && valueExpr == node.Expression)
            {
                return node;
            }

            return new BoundFieldAssignmentExpression(node.ClassInstance.Syntax, classInstanceExpression, node.ClassMember, valueExpr);
        }

        private BoundExpression RewriteFieldAccessExpression(BoundFieldAccessExpression node)
        {
            var expression = RewriteExpression(node.ClassInstance);

            if (expression == node.ClassInstance)
            {
                return node;
            }

            return new BoundFieldAccessExpression(expression.Syntax, expression, node.ClassMember);
        }

        protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
            {
                return node;
            }

            return new BoundAssignmentExpression(node.Syntax, node.Variable, expression);
        }

        protected virtual BoundExpression RewriteCompoundAssignmentExpression(BoundCompoundAssignmentExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
            {
                return node;
            }

            return new BoundCompoundAssignmentExpression(node.Syntax, node.Variable, node.Op, expression);
        }

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            var operand = RewriteExpression(node.Operand);

            if (operand == node.Operand)
            {
                return node;
            }

            return new BoundUnaryExpression(node.Syntax, node.Op, operand);
        }

        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);

            if (left == node.Left && right == node.Right)
            {
                return node;
            }

            return new BoundBinaryExpression(node.Syntax, left, node.Op, right);
        }

        protected virtual BoundExpression RewriteCallExpression(BoundCallExpression node)
        {
            ImmutableArray<BoundExpression>.Builder? builder = null;

            for (var i = 0; i < node.Arguments.Length; i++)
            {
                var oldArgument = node.Arguments[i];
                var newArgument = RewriteExpression(oldArgument);

                if (newArgument != oldArgument)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);

                        for (var j = 0; j < i; j++)
                        {
                            builder.Add(node.Arguments[j]);
                        }
                    }
                }

                builder?.Add(newArgument);
            }

            if (builder == null)
            {
                return node;
            }

            return new BoundCallExpression(node.Syntax, node.Function, builder.MoveToImmutable());
        }

        protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
            {
                return node;
            }

            return new BoundConversionExpression(node.Syntax, node.Type, expression);
        }
    }
}