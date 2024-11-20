﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Loretta.CodeAnalysis
{
    internal static class CollectionsExtensions
    {
        internal static bool IsNullOrEmpty<T>([NotNullWhen(returnValue: false)] this ICollection<T>? collection) =>
            collection == null || collection.Count == 0;

        internal static bool IsNullOrEmpty<T>([NotNullWhen(returnValue: false)] this IReadOnlyCollection<T>? collection) =>
            collection == null || collection.Count == 0;

        internal static bool IsNullOrEmpty<T>([NotNullWhen(returnValue: false)] this ImmutableHashSet<T>? hashSet) =>
            hashSet == null || hashSet.Count == 0;
    }
}
