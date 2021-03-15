using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Vivian.CodeAnalysis.Binding;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;

        private Lowerer() { }

        private BoundLabel GenerateLabel()
        {
            var name = $"Label{++_labelCount}";
            return new BoundLabel(name);
        }

        public static BoundBlockStatement Lower(Symbol symbol, BoundStatement statement)
        {
            if (!(symbol is FunctionSymbol || symbol is ClassSymbol))
            {
                throw new Exception($"Symbol of type {symbol.Kind} not expected in Lowerer.");
            }

            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement(statement);

            return RemoveDeadCode(Flatten(symbol, result));
        }

        private static BoundBlockStatement Flatten(Symbol function, BoundStatement statement)
        {
            // TODO: Take into account nested scopes when Flattening.  The compiler allows a naming collision
            // to occur if separate scope blocks contain identically named symbols.

            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                    {
                        stack.Push(s);
                    }
                }
                else
                {
                    builder.Add(current);
                }
            }

            if (function is FunctionSymbol f && f.ReturnType == TypeSymbol.Void)
            {
                if (builder.Count == 0 || CanFallThrough(builder.Last()))
                {
                    builder.Add(new BoundReturnStatement(statement.Syntax, null));
                }
            }

            return new BoundBlockStatement(statement.Syntax, builder.ToImmutable());
        }

        private static bool CanFallThrough(BoundStatement boundStatement)
        {
            return boundStatement.Kind != BoundNodeKind.ReturnStatement &&
                   boundStatement.Kind != BoundNodeKind.GotoStatement;
        }

        private static BoundBlockStatement RemoveDeadCode(BoundBlockStatement node)
        {
            var controlFlow = ControlFlowGraph.Create(node);
            var reachableStatements = new HashSet<BoundStatement>(controlFlow.Blocks.SelectMany(b => b.Statements));
            var builder = node.Statements.ToBuilder();

            for (var i = builder.Count - 1; i >= 0; i--)
            {
                if (!reachableStatements.Contains(builder[i]))
                {
                    builder.RemoveAt(i);
                }
            }

            return new BoundBlockStatement(node.Syntax, builder.ToImmutable());
        }


        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                // if <condition>
                //      <then>
                //
                // ---->
                //
                // gotoFalse <condition> end
                // <then>
                // end:

                var endLabel = GenerateLabel();
                var result = BoundNodeFactory.Block(node.Syntax,
                                                    BoundNodeFactory.GotoFalse(node.Syntax, endLabel, node.Condition),
                                                    node.ThenStatement,
                                                    BoundNodeFactory.Label(node.Syntax, endLabel)
                );

                return RewriteStatement(result);
            }
            else
            {
                // if <condition>
                //      <then>
                // else
                //      <else>
                //
                // ---->
                //
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:

                var elseLabel = GenerateLabel();
                var endLabel = GenerateLabel();
                var result = BoundNodeFactory.Block(node.Syntax,
                                                    BoundNodeFactory.GotoFalse(node.Syntax, elseLabel, node.Condition),
                                                    node.ThenStatement,
                                                    BoundNodeFactory.Goto(node.Syntax, endLabel),
                                                    BoundNodeFactory.Label(node.Syntax, elseLabel),
                                                    node.ElseStatement,
                                                    BoundNodeFactory.Label(node.Syntax, endLabel)
                );

                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            // while <condition>
            //      <body>
            //
            // ----->
            //
            // goto continue
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = GenerateLabel();
            var result = BoundNodeFactory.Block(node.Syntax,
                                                BoundNodeFactory.Goto(node.Syntax, node.ContinueLabel),
                                                BoundNodeFactory.Label(node.Syntax, bodyLabel),
                                                node.Body,
                                                BoundNodeFactory.Label(node.Syntax, node.ContinueLabel),
                                                BoundNodeFactory.GotoTrue(node.Syntax, bodyLabel, node.Condition),
                                                BoundNodeFactory.Label(node.Syntax, node.BreakLabel)
            );

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            // do
            //      <body>
            // while <condition>
            //
            // ----->
            //
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = GenerateLabel();
            var result = BoundNodeFactory.Block(node.Syntax,
                                                BoundNodeFactory.Label(node.Syntax, bodyLabel),
                                                node.Body,
                                                BoundNodeFactory.Label(node.Syntax, node.ContinueLabel),
                                                BoundNodeFactory.GotoTrue(node.Syntax, bodyLabel, node.Condition),
                                                BoundNodeFactory.Label(node.Syntax, node.BreakLabel)
            );

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            // for <var> = <lower> to <upper>
            //      <body>
            //
            // ---->
            //
            // {
            //      var <var> = <lower>
            //      let upperBound = <upper>
            //      while (<var> <= upperBound)
            //      {
            //          <body>
            //          continue:
            //          <var> = <var> + 1
            //      }
            // }

            var lowerBound = BoundNodeFactory.VariableDeclaration(node.Syntax, node.Variable, node.LowerBound);
            var upperBound = BoundNodeFactory.ConstantDeclaration(node.Syntax, "upperBound", node.UpperBound);
            var result = BoundNodeFactory.Block(node.Syntax,
                                                lowerBound,
                                                upperBound,
                                                BoundNodeFactory.While(node.Syntax,
                                                    BoundNodeFactory.LessOrEqual(
                                                        node.Syntax,
                                                        BoundNodeFactory.Variable(node.Syntax, lowerBound),
                                                        BoundNodeFactory.Variable(node.Syntax, upperBound)
                                                    ),
                                                    BoundNodeFactory.Block(
                                                        node.Syntax,
                                                        node.Body,
                                                        BoundNodeFactory.Label(node.Syntax, node.ContinueLabel),
                                                        BoundNodeFactory.Increment(
                                                            node.Syntax,
                                                            BoundNodeFactory.Variable(node.Syntax, lowerBound)
                                                    )
                                                ),
                                                node.BreakLabel,
                                                continueLabel: GenerateLabel()));
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            if (node.Condition.ConstantValue != null)
            {
                var condition = (bool)node.Condition.ConstantValue.Value!;
                condition = node.JumpIfTrue ? condition : !condition;
                
                if (condition)
                {
                    return RewriteStatement(BoundNodeFactory.Goto(node.Syntax, node.Label));
                }
                else
                {
                    return RewriteStatement(BoundNodeFactory.Nop(node.Syntax));
                }
            }

            return base.RewriteConditionalGotoStatement(node);
        }

        protected override BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
        {
            var rewrittenNode = base.RewriteVariableDeclaration(node);
            return new BoundSequencePointStatement(rewrittenNode.Syntax, rewrittenNode, rewrittenNode.Syntax.Location);
        }

        protected override BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var rewrittenNode = base.RewriteExpressionStatement(node);
            return new BoundSequencePointStatement(rewrittenNode.Syntax, rewrittenNode, rewrittenNode.Syntax.Location);
        }

        protected override BoundExpression RewriteCompoundAssignmentExpression(BoundCompoundAssignmentExpression node)
        {
            var newNode = (BoundCompoundAssignmentExpression) base.RewriteCompoundAssignmentExpression(node);

            // a <op>= b
            //
            // ---->
            //
            // a = (a <op> b)

            return BoundNodeFactory.Assignment(newNode.Syntax,
                                               newNode.Variable,
                                               BoundNodeFactory.Binary(newNode.Syntax,
                                                   BoundNodeFactory.Variable(newNode.Syntax, 
                                                                             newNode.Variable),
                                                                             newNode.Op,
                                                                             newNode.Expression));
        }

        protected override BoundExpression RewriteCompoundFieldAssignmentExpression(BoundCompoundFieldAssignmentExpression node)
        {
            var newNode = (BoundCompoundFieldAssignmentExpression) base.RewriteCompoundFieldAssignmentExpression(node);

            // a.f <op>= b
            //
            // ---->
            //
            // a.f = (a.f <op> b)
            return BoundNodeFactory.Assignment(newNode.Syntax,
                                               newNode.StructInstance,
                                               newNode.StructMember,
                                               BoundNodeFactory.Binary(newNode.Syntax,
                                                   BoundNodeFactory.Field(newNode.Syntax, 
                                                       newNode.StructInstance, newNode.StructMember),
                                                           newNode.Op,
                                                           newNode.Expression)
            );
        }
    }
}