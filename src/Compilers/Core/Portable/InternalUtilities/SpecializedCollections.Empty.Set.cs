﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal partial class SpecializedCollections
    {
        private partial class Empty
        {
            internal class Set<T> : Collection<T>, ISet<T>, IReadOnlySet<T>
            {
                public static new readonly Set<T> Instance = new();

                protected Set()
                {
                }

                public new bool Add(T item) => throw new NotSupportedException();

                public void ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

                public void IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();

                public bool IsProperSubsetOf(IEnumerable<T> other) => !other.IsEmpty();

                public bool IsProperSupersetOf(IEnumerable<T> other) => false;

                public bool IsSubsetOf(IEnumerable<T> other) => true;

                public bool IsSupersetOf(IEnumerable<T> other) => other.IsEmpty();

                public bool Overlaps(IEnumerable<T> other) => false;

                public bool SetEquals(IEnumerable<T> other) => other.IsEmpty();

                public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

                public void UnionWith(IEnumerable<T> other) => throw new NotSupportedException();
            }
        }
    }
}
