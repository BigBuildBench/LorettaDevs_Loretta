﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Loretta.CodeAnalysis
{
    public readonly partial struct ChildSyntaxList
    {
        /// <summary>
        /// An enumerator that enumerates the list in reverse order.
        /// </summary>
        public readonly partial struct Reversed : IEnumerable<SyntaxNodeOrToken>, IEquatable<Reversed>
        {
            private readonly SyntaxNode? _node;
            private readonly int _count;

            internal Reversed(SyntaxNode node, int count)
            {
                _node = node;
                _count = count;
            }

            /// <summary>
            /// Returns the enumerator for this list.
            /// </summary>
            /// <returns></returns>
            public Enumerator GetEnumerator()
            {
                LorettaDebug.Assert(_node is not null);
                return new Enumerator(_node, _count);
            }

            IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator()
            {
                if (_node == null)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
                }

                return new EnumeratorImpl(_node, _count);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                if (_node == null)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
                }

                return new EnumeratorImpl(_node, _count);
            }

            /// <inheritdoc/>
            public override int GetHashCode() =>
                _node != null ? Hash.Combine(_node.GetHashCode(), _count) : 0;

            /// <inheritdoc/>
            public override bool Equals(object? obj) =>
                (obj is Reversed r) && Equals(r);

            /// <inheritdoc/>
            public bool Equals(Reversed other)
            {
                return _node == other._node
                    && _count == other._count;
            }

            /// <summary>
            /// Checks whether two reversed lists are equal.
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            public static bool operator ==(Reversed left, Reversed right) => left.Equals(right);

            /// <summary>
            /// Checks whether two reversed lists are not equal.
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            public static bool operator !=(Reversed left, Reversed right) => !(left == right);

            /// <summary>
            /// Enumerates the nodes in this reversed children list.
            /// </summary>
            public struct Enumerator
            {
                private readonly SyntaxNode? _node;
                private readonly int _count;
                private int _childIndex;

                internal Enumerator(SyntaxNode node, int count)
                {
                    _node = node;
                    _count = count;
                    _childIndex = count;
                }

                /// <summary>
                /// Moves to the next element.
                /// </summary>
                /// <returns>Whether there was another element to move to.</returns>
                [MemberNotNullWhen(true, nameof(_node))]
                public bool MoveNext() => --_childIndex >= 0;

                /// <summary>
                /// The element the enumerator is at.
                /// </summary>
                public SyntaxNodeOrToken Current
                {
                    get
                    {
                        LorettaDebug.Assert(_node is not null);
                        return ItemInternal(_node, _childIndex);
                    }
                }

                /// <summary>
                /// Resets the enumerator to the last element.
                /// </summary>
                public void Reset() => _childIndex = _count;
            }

            private class EnumeratorImpl : IEnumerator<SyntaxNodeOrToken>
            {
                private Enumerator _enumerator;

                internal EnumeratorImpl(SyntaxNode node, int count)
                {
                    _enumerator = new Enumerator(node, count);
                }

                /// <summary>
                /// Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>
                /// The element in the collection at the current position of the enumerator.
                ///   </returns>
                public SyntaxNodeOrToken Current => _enumerator.Current;

                /// <summary>
                /// Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>
                /// The element in the collection at the current position of the enumerator.
                ///   </returns>
                object IEnumerator.Current => _enumerator.Current;

                /// <summary>
                /// Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
                /// </returns>
                /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created. </exception>
                public bool MoveNext() => _enumerator.MoveNext();

                /// <summary>
                /// Sets the enumerator to its initial position, which is before the first element in the collection.
                /// </summary>
                /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created. </exception>
                public void Reset() => _enumerator.Reset();

                /// <summary>
                /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
                /// </summary>
                public void Dispose()
                { }
            }
        }
    }
}
