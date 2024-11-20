﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal partial struct ChildSyntaxList
    {
        internal readonly partial struct Reversed
        {
            private readonly GreenNode? _node;

            internal Reversed(GreenNode? node)
            {
                _node = node;
            }

            public Enumerator GetEnumerator() => new(_node);

#if DEBUG
            [Obsolete("For debugging", error: true)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "for debugging")]
            private GreenNode[] Nodes
            {
                get
                {
                    var result = new List<GreenNode>();
                    foreach (var n in this)
                    {
                        result.Add(n);
                    }

                    return result.ToArray();
                }
            }

#endif
        }
    }
}
