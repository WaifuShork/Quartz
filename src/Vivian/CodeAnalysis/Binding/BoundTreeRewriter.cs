using System;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public virtual BoundStatement RewriteStatement(BoundStatement node)
        {
            return node.Kind switch
            {
                BoundNodeKind.BlockStatement => RewriteBlockStatement((BoundBlockStatement)node),
                BoundNodeKind.ConditionalGotoStatement => RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node),
                BoundNodeKind.DoWhileStatement => RewriteDoWhileStatement((BoundDoWhileStatement)node),
                BoundNodeKind.ExpressionStatement => RewriteExpressionStatement((BoundExpressionStatement)node),
                BoundNodeKind.ForStatement => RewriteForStatement((BoundForStatement)node),
                BoundNodeKind.GotoStatement => RewriteGotoStatement((BoundGotoStatement)node),
                BoundNodeKind.IfStatement => RewriteIfStatement((BoundIfStatement)node),
                BoundNodeKind.LabelStatement => RewriteLabelStatement((BoundLabelStatement)node),
                BoundNodeKind.MemberBlockStatement => RewriteBlockStatement((BoundMemberBlockStatement)node),
                BoundNodeKind.NopStatement => RewriteNopStatement((BoundNopStatement)node),
                BoundNodeKind.ReturnStatement => RewriteReturnStatement((BoundReturnStatement)node),
                BoundNodeKind.SequencePointStatement => RewriteSequencePointStatement((BoundSequencePointStatement)node),
                BoundNodeKind.VariableDeclaration => RewriteVariableDeclaration((BoundVariableDeclaration)node),
                BoundNodeKind.WhileStatement => RewriteWhileStatement((BoundWhileStatement)node),
                _ => throw new Exception($"Unexpected node: {node.Kind}"),
            };
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
                            builder.Add(boundStatements[j]);
                    }
                }

                builder?.Add(newStatement);
            }

            if (builder == null)
                return node;

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
                return node;

            return new BoundVariableDeclaration(node.Syntax, node.Variable, initializer);
        }

        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var thenStatement = RewriteStatement(node.ThenStatement);
            var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);
            if (condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement)
                return node;

            return new BoundIfStatement(node.Syntax, condition, thenStatement, elseStatement);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
                return node;

            return new BoundWhileStatement(node.Syntax, condition, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            var body = RewriteStatement(node.Body);
            var condition = RewriteExpression(node.Condition);
            if (body == node.Body && condition == node.Condition)
                return node;

            return new BoundDoWhileStatement(node.Syntax, body, condition, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var lowerBound = RewriteExpression(node.LowerBound);
            var upperBound = RewriteExpression(node.UpperBound);
            var body = RewriteStatement(node.Body);

            if (lowerBound == node.LowerBound && upperBound == node.UpperBound && body == node.Body)
                return node;

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
                return node;

            return new BoundConditionalGotoStatement(node.Syntax, node.Label, condition, node.JumpIfTrue);
        }

        protected virtual BoundStatement RewriteReturnStatement(BoundReturnStatement node)
        {
            var expression = node.Expression == null ? null : RewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundReturnStatement(node.Syntax, expression);
        }

        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundExpressionStatement(node.Syntax, expression);
        }

        private BoundStatement RewriteSequencePointStatement(BoundSequencePointStatement node)
        {
            var statement = RewriteStatement(node.Statement);

            if (statement == node.Statement)
                return node;

            return new BoundSequencePointStatement(node.Syntax, statement, node.Location);
        }

        public virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            return node.Kind switch
            {
                BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression((BoundAssignmentExpression)node),
                BoundNodeKind.BinaryExpression => RewriteBinaryExpression((BoundBinaryExpression)node),
                BoundNodeKind.CallExpression => RewriteCallExpression((BoundCallExpression)node),
                BoundNodeKind.CompoundAssignmentExpression => RewriteCompoundAssignmentExpression((BoundCompoundAssignmentExpression)node),
                BoundNodeKind.CompoundFieldAssignmentExpression => RewriteCompoundFieldAssignmentExpression((BoundCompoundFieldAssignmentExpression)node),
                BoundNodeKind.ConversionExpression => RewriteConversionExpression((BoundConversionExpression)node),
                BoundNodeKind.ErrorExpression => RewriteErrorExpression((BoundErrorExpression)node),
                BoundNodeKind.FieldAccessExpression => RewriteFieldAccessExpression((BoundFieldAccessExpression)node),
                BoundNodeKind.FieldAssignmentExpression => RewriteFieldAssignmentExpression((BoundFieldAssignmentExpression)node),
                BoundNodeKind.LiteralExpression => RewriteLiteralExpression((BoundLiteralExpression)node),
                BoundNodeKind.ThisExpression => RewriteThisExpression((BoundThisExpression)node),
                BoundNodeKind.UnaryExpression => RewriteUnaryExpression((BoundUnaryExpression)node),
                BoundNodeKind.VariableExpression => RewriteVariableExpression((BoundVariableExpression)node),
                _ => throw new Exception($"Unexpected node: {node.Kind}"),
            };
        }

        private BoundExpression RewriteThisExpression(BoundThisExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteCompoundFieldAssignmentExpression(BoundCompoundFieldAssignmentExpression node)
        {
            var structInstanceExpr = RewriteExpression(node.StructInstance);
            var valueExpr = RewriteExpression(node.Expression);

            if (structInstanceExpr == node.StructInstance && valueExpr == node.Expression)
                return node;

            return new BoundCompoundFieldAssignmentExpression(node.Syntax, structInstanceExpr, node.StructMember, node.Op, valueExpr);
        }

        private BoundExpression RewriteFieldAssignmentExpression(BoundFieldAssignmentExpression node)
        {
            var structInstanceExpr = RewriteExpression(node.StructInstance);
            var valueExpr = RewriteExpression(node.Expression);

            if (structInstanceExpr == node.StructInstance && valueExpr == node.Expression)
                return node;

            return new BoundFieldAssignmentExpression(node.StructInstance.Syntax, structInstanceExpr, node.StructMember, valueExpr);
        }

        private BoundExpression RewriteFieldAccessExpression(BoundFieldAccessExpression node)
        {
            var expression = RewriteExpression(node.StructInstance);

            if (expression == node.StructInstance)
                return node;

            return new BoundFieldAccessExpression(expression.Syntax, expression, node.StructMember);
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
                return node;

            return new BoundAssignmentExpression(node.Syntax, node.Variable, expression);
        }

        protected virtual BoundExpression RewriteCompoundAssignmentExpression(BoundCompoundAssignmentExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundCompoundAssignmentExpression(node.Syntax, node.Variable, node.Op, expression);
        }

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            var operand = RewriteExpression(node.Operand);

            if (operand == node.Operand)
                return node;

            return new BoundUnaryExpression(node.Syntax, node.Op, operand);
        }

        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);

            if (left == node.Left && right == node.Right)
                return node;

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
                            builder.Add(node.Arguments[j]);
                    }
                }

                builder?.Add(newArgument);
            }

            if (builder == null)
                return node;

            return new BoundCallExpression(node.Syntax, node.Function, builder.MoveToImmutable());
        }

        protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundConversionExpression(node.Syntax, node.Type, expression);
        }
    }
}