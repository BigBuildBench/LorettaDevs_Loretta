﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.InteropServices;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents a read-only list of <see cref="SyntaxToken"/>.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial struct SyntaxTokenList : IEquatable<SyntaxTokenList>, IReadOnlyList<SyntaxToken>
    {
        private readonly SyntaxNode? _parent;
        private readonly int _index;

        internal SyntaxTokenList(SyntaxNode? parent, GreenNode? tokenOrList, int position, int index)
        {
            LorettaDebug.Assert(tokenOrList != null || (position == 0 && index == 0 && parent == null));
            LorettaDebug.Assert(position >= 0);
            LorettaDebug.Assert(tokenOrList == null || tokenOrList.IsToken || tokenOrList.IsList);
            _parent = parent;
            Node = tokenOrList;
            Position = position;
            _index = index;
        }

        /// <summary>
        /// Creates a new token list with the provided token as its only element.
        /// </summary>
        /// <param name="token"></param>
        public SyntaxTokenList(SyntaxToken token)
        {
            _parent = token.Parent;
            Node = token.Node;
            Position = token.Position;
            _index = 0;
        }

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        /// <param name="tokens">An array of tokens.</param>
        public SyntaxTokenList(params SyntaxToken[] tokens)
            : this(null, CreateNode(tokens), 0, 0)
        {
        }

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        public SyntaxTokenList(IEnumerable<SyntaxToken> tokens)
            : this(null, CreateNode(tokens), 0, 0)
        {
        }

        private static GreenNode? CreateNode(SyntaxToken[] tokens)
        {
            if (tokens == null)
            {
                return null;
            }

            // TODO: we could remove the unnecessary builder allocations here and go directly
            // from the array to the List nodes.
            var builder = new SyntaxTokenListBuilder(tokens.Length);
            for (int i = 0; i < tokens.Length; i++)
            {
                var node = tokens[i].Node;
                LorettaDebug.Assert(node is not null);
                builder.Add(node);
            }

            return builder.ToList().Node;
        }

        private static GreenNode? CreateNode(IEnumerable<SyntaxToken> tokens)
        {
            if (tokens == null)
            {
                return null;
            }

            var builder = SyntaxTokenListBuilder.Create();
            foreach (var token in tokens)
            {
                LorettaDebug.Assert(token.Node is not null);
                builder.Add(token.Node);
            }

            return builder.ToList().Node;
        }

        internal GreenNode? Node { get; }

        internal int Position { get; }

        /// <summary>
        /// Returns the number of tokens in the list.
        /// </summary>
        public int Count => Node == null ? 0 : (Node.IsList ? Node.SlotCount : 1);

        /// <summary>
        /// Gets the token at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the token to get.</param>
        /// <returns>The token at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="Count" />. </exception>
        public SyntaxToken this[int index]
        {
            get
            {
                if (Node != null)
                {
                    if (Node.IsList)
                    {
                        if (unchecked((uint) index < (uint) Node.SlotCount))
                        {
                            return new SyntaxToken(_parent, Node.GetSlot(index), Position + Node.GetSlotOffset(index), _index + index);
                        }
                    }
                    else if (index == 0)
                    {
                        return new SyntaxToken(_parent, Node, Position, _index);
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// The absolute span of the list elements in characters, including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan FullSpan
        {
            get
            {
                if (Node == null)
                {
                    return default;
                }

                return new TextSpan(Position, Node.FullWidth);
            }
        }

        /// <summary>
        /// The absolute span of the list elements in characters, not including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan Span
        {
            get
            {
                if (Node == null)
                {
                    return default;
                }

                return TextSpan.FromBounds(Position + Node.GetLeadingTriviaWidth(),
                    Position + Node.FullWidth - Node.GetTrailingTriviaWidth());
            }
        }

        /// <summary>
        /// Returns the string representation of the tokens in this list, not including
        /// the first token's leading trivia and the last token's trailing trivia.
        /// </summary>
        /// <returns>
        /// The string representation of the tokens in this list, not including
        /// the first token's leading trivia and the last token's trailing trivia.
        /// </returns>
        public override string ToString() => Node != null ? Node.ToString() : string.Empty;

        /// <summary>
        /// Returns the full string representation of the tokens in this list including
        /// the first token's leading trivia and the last token's trailing trivia.
        /// </summary>
        /// <returns>
        /// The full string representation of the tokens in this list including
        /// the first token's leading trivia and the last token's trailing trivia.
        /// </returns>
        public string ToFullString() => Node != null ? Node.ToFullString() : string.Empty;

        /// <summary>
        /// Returns the first token in the list.
        /// </summary>
        /// <returns>The first token in the list.</returns>
        /// <exception cref="InvalidOperationException">The list is empty.</exception>
        public SyntaxToken First()
        {
            if (Any())
            {
                return this[0];
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns the last token in the list.
        /// </summary>
        /// <returns> The last token in the list.</returns>
        /// <exception cref="InvalidOperationException">The list is empty.</exception>
        public SyntaxToken Last()
        {
            if (Any())
            {
                return this[Count - 1];
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Tests whether the list is non-empty.
        /// </summary>
        /// <returns>True if the list contains any tokens.</returns>
        public bool Any() => Node != null;

        /// <summary>
        /// Returns a list which contains all elements of <see cref="SyntaxTokenList"/> in reversed order.
        /// </summary>
        /// <returns><see cref="Reversed"/> which contains all elements of <see cref="SyntaxTokenList"/> in reversed order</returns>
        public Reversed Reverse() => new(this);

        internal void CopyTo(int offset, GreenNode?[] array, int arrayOffset, int count)
        {
            LorettaDebug.Assert(Count >= offset + count);

            for (int i = 0; i < count; i++)
            {
                array[arrayOffset + i] = GetGreenNodeAt(offset + i);
            }
        }

        /// <summary>
        /// get the green node at the given slot
        /// </summary>
        private GreenNode? GetGreenNodeAt(int i)
        {
            LorettaDebug.Assert(Node is not null);
            return GetGreenNodeAt(Node, i);
        }

        /// <summary>
        /// get the green node at the given slot
        /// </summary>
        private static GreenNode? GetGreenNodeAt(GreenNode node, int i)
        {
            LorettaDebug.Assert(node.IsList || (i == 0 && !node.IsList));
            return node.IsList ? node.GetSlot(i) : node;
        }

        /// <summary>
        /// Retuns the index of the provided token in this list.
        /// </summary>
        /// <param name="tokenInList"></param>
        /// <returns>-1 if not found.</returns>
        public int IndexOf(SyntaxToken tokenInList)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                var token = this[i];
                if (token == tokenInList)
                {
                    return i;
                }
            }

            return -1;
        }

        internal int IndexOf(int rawKind)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                if (this[i].RawKind == rawKind)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the specified token added to the end.
        /// </summary>
        /// <param name="token">The token to add.</param>
        public SyntaxTokenList Add(SyntaxToken token) => Insert(Count, token);

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the specified tokens added to the end.
        /// </summary>
        /// <param name="tokens">The tokens to add.</param>
        public SyntaxTokenList AddRange(IEnumerable<SyntaxToken> tokens) => InsertRange(Count, tokens);

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the specified token insert at the index.
        /// </summary>
        /// <param name="index">The index to insert the new token.</param>
        /// <param name="token">The token to insert.</param>
        public SyntaxTokenList Insert(int index, SyntaxToken token)
        {
            if (token == default)
            {
                throw new ArgumentOutOfRangeException(nameof(token));
            }

            return InsertRange(index, new[] { token });
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the specified tokens insert at the index.
        /// </summary>
        /// <param name="index">The index to insert the new tokens.</param>
        /// <param name="tokens">The tokens to insert.</param>
        public SyntaxTokenList InsertRange(int index, IEnumerable<SyntaxToken> tokens)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (tokens is null) throw new ArgumentNullException(nameof(tokens));
            var items = tokens.ToList();
            if (items.Count == 0)
            {
                return this;
            }

            var list = this.ToList();
            list.InsertRange(index, tokens);

            if (list.Count == 0)
            {
                return this;
            }

            return new SyntaxTokenList(null, GreenNode.CreateList(list, static n => n.RequiredNode), 0, 0);
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the token at the specified index removed.
        /// </summary>
        /// <param name="index">The index of the token to remove.</param>
        public SyntaxTokenList RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var list = this.ToList();
            list.RemoveAt(index);
            return new SyntaxTokenList(null, GreenNode.CreateList(list, static n => n.RequiredNode), 0, 0);
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the specified token removed.
        /// </summary>
        /// <param name="tokenInList">The token to remove.</param>
        public SyntaxTokenList Remove(SyntaxToken tokenInList)
        {
            var index = IndexOf(tokenInList);
            if (index >= 0 && index <= Count)
            {
                return RemoveAt(index);
            }

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the specified token replaced with a new token.
        /// </summary>
        /// <param name="tokenInList">The token to replace.</param>
        /// <param name="newToken">The new token.</param>
        public SyntaxTokenList Replace(SyntaxToken tokenInList, SyntaxToken newToken)
        {
            if (newToken == default)
            {
                throw new ArgumentOutOfRangeException(nameof(newToken));
            }

            return ReplaceRange(tokenInList, new[] { newToken });
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with the specified token replaced with new tokens.
        /// </summary>
        /// <param name="tokenInList">The token to replace.</param>
        /// <param name="newTokens">The new tokens.</param>
        public SyntaxTokenList ReplaceRange(SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens)
        {
            var index = IndexOf(tokenInList);
            if (index >= 0 && index <= Count)
            {
                var list = this.ToList();
                list.RemoveAt(index);
                list.InsertRange(index, newTokens);
                return new SyntaxTokenList(null, GreenNode.CreateList(list, static n => n.RequiredNode), 0, 0);
            }

            throw new ArgumentOutOfRangeException(nameof(tokenInList));
        }

        // for debugging
        private SyntaxToken[] Nodes => this.ToArray();

        /// <summary>
        /// Returns an enumerator for the tokens in the <see cref="SyntaxTokenList"/>
        /// </summary>
        public Enumerator GetEnumerator() => new(in this);

        IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator()
        {
            if (Node == null)
            {
                return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
            }

            return new EnumeratorImpl(in this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Node == null)
            {
                return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
            }

            return new EnumeratorImpl(in this);
        }

        /// <summary>
        /// Compares <paramref name="left"/> and <paramref name="right"/> for equality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two <see cref="SyntaxTokenList"/>s are equal.</returns>
        public static bool operator ==(SyntaxTokenList left, SyntaxTokenList right) => left.Equals(right);

        /// <summary>
        /// Compares <paramref name="left"/> and <paramref name="right"/> for inequality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two <see cref="SyntaxTokenList"/>s are not equal.</returns>
        public static bool operator !=(SyntaxTokenList left, SyntaxTokenList right) => !left.Equals(right);

        /// <inheritdoc/>
        public bool Equals(SyntaxTokenList other) => Node == other.Node && _parent == other._parent && _index == other._index;

        /// <summary>
        /// Compares this <see cref=" SyntaxTokenList"/> with the <paramref name="obj"/> for equality.
        /// </summary>
        /// <returns>True if the two objects are equal.</returns>
        public override bool Equals(object? obj) => obj is SyntaxTokenList list && Equals(list);

        /// <summary>
        /// Serves as a hash function for the <see cref="SyntaxTokenList"/>
        /// </summary>
        public override int GetHashCode() =>
            // Not call GHC on parent as it's expensive
            Hash.Combine(Node, _index);

        /// <summary>
        /// Create a new Token List
        /// </summary>
        /// <param name="token">Element of the return Token List</param>
        public static SyntaxTokenList Create(SyntaxToken token) => new(token);
    }
}
