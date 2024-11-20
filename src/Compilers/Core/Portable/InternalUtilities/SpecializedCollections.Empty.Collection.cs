﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal static partial class SpecializedCollections
    {
        private static partial class Empty
        {
            internal class Collection<T> : Enumerable<T>, ICollection<T>
            {
                public static readonly ICollection<T> Instance = new Collection<T>();

                protected Collection()
                {
                }

                public void Add(T item) => throw new NotSupportedException();

                public void Clear() => throw new NotSupportedException();

                public bool Contains(T item) => false;

                public void CopyTo(T[] array, int arrayIndex)
                {
                }

                public int Count => 0;

                public bool IsReadOnly => true;

                public bool Remove(T item) => throw new NotSupportedException();
            }
        }
    }
}
