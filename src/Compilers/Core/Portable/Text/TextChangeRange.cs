﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Represents the change to a span of text.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public readonly struct TextChangeRange : IEquatable<TextChangeRange>
    {
        /// <summary>
        /// The span of text before the edit which is being changed
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// Width of the span after the edit.  A 0 here would represent a delete
        /// </summary>
        public int NewLength { get; }

        internal int NewEnd => Span.Start + NewLength;

        /// <summary>
        /// Initializes a new instance of <see cref="TextChangeRange"/>.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="newLength"></param>
        public TextChangeRange(TextSpan span, int newLength)
            : this()
        {
            if (newLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newLength));
            }

            Span = span;
            NewLength = newLength;
        }

        /// <summary>
        /// Compares current instance of <see cref="TextChangeRange"/> to another.
        /// </summary>
        public bool Equals(TextChangeRange other)
        {
            return
                other.Span == Span &&
                other.NewLength == NewLength;
        }

        /// <summary>
        /// Compares current instance of <see cref="TextChangeRange"/> to another.
        /// </summary>
        public override bool Equals(object? obj) =>
            obj is TextChangeRange range && Equals(range);

        /// <summary>
        /// Provides hash code for current instance of <see cref="TextChangeRange"/>.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() =>
            Hash.Combine(NewLength, Span.GetHashCode());

        /// <summary>
        /// Determines if two instances of <see cref="TextChangeRange"/> are same.
        /// </summary>
        public static bool operator ==(TextChangeRange left, TextChangeRange right) =>
            left.Equals(right);

        /// <summary>
        /// Determines if two instances of <see cref="TextChangeRange"/> are different.
        /// </summary>
        public static bool operator !=(TextChangeRange left, TextChangeRange right) =>
            !(left == right);

        /// <summary>
        /// An empty set of changes.
        /// </summary>
        public static IReadOnlyList<TextChangeRange> NoChanges => SpecializedCollections.EmptyReadOnlyList<TextChangeRange>();

        /// <summary>
        /// Collapse a set of <see cref="TextChangeRange"/>s into a single encompassing range.  If
        /// the set of ranges provided is empty, an empty range is returned.
        /// </summary>
        public static TextChangeRange Collapse(IEnumerable<TextChangeRange> changes)
        {
            var diff = 0;
            var start = int.MaxValue;
            var end = 0;

            foreach (var change in changes)
            {
                diff += change.NewLength - change.Span.Length;

                if (change.Span.Start < start)
                {
                    start = change.Span.Start;
                }

                if (change.Span.End > end)
                {
                    end = change.Span.End;
                }
            }

            if (start > end)
            {
                // there were no changes.
                return default;
            }

            var combined = TextSpan.FromBounds(start, end);
            var newLen = combined.Length + diff;

            return new TextChangeRange(combined, newLen);
        }

        private string GetDebuggerDisplay() =>
            $"new TextChangeRange(new TextSpan({Span.Start}, {Span.Length}), {NewLength})";

        /// <summary>
        /// Converts the text change range to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"TextChangeRange(Span={Span}, NewLength={NewLength})";
    }
}
