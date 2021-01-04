using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using wsc.CodeAnalysis.Syntax;

namespace wsc.CodeAnalysis.Binding
{
    public abstract class SeparatedSyntaxList
    {
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators();

    }
    
    public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T> where T : SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> _nodesAndSeparators;

        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
        {
            _nodesAndSeparators = nodesAndSeparators;
        }

        public int Count => (_nodesAndSeparators.Length + 1) / 2;

        public T this[int index] => (T) _nodesAndSeparators[index * 2];

        public SyntaxToken GetSeparator(int index) => (SyntaxToken) _nodesAndSeparators[index * 2 + 1];
        
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        public override ImmutableArray<SyntaxNode> GetWithSeparators() => _nodesAndSeparators;
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}