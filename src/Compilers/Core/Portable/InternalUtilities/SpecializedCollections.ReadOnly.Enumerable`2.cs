﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal partial class SpecializedCollections
    {
        private partial class ReadOnly
        {
            internal class Enumerable<TUnderlying, T> : Enumerable<TUnderlying>, IEnumerable<T>
                where TUnderlying : IEnumerable<T>
            {
                public Enumerable(TUnderlying underlying)
                    : base(underlying)
                {
                }

                public new IEnumerator<T> GetEnumerator() => Underlying.GetEnumerator();
            }
        }
    }
}
