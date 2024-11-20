﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A SyntaxAnnotation is used to annotate syntax elements with additional information. 
    /// 
    /// Since syntax elements are immutable, annotating them requires creating new instances of them
    /// with the annotations attached.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    public sealed class SyntaxAnnotation : IObjectWritable, IEquatable<SyntaxAnnotation?>
    {
        static SyntaxAnnotation()
        {
            ObjectBinder.RegisterTypeReader(typeof(SyntaxAnnotation), r => new SyntaxAnnotation(r));
        }

        /// <summary>
        /// A predefined syntax annotation that indicates whether the syntax element has elastic trivia.
        /// </summary>
        public static SyntaxAnnotation ElasticAnnotation { get; } = new SyntaxAnnotation();

        // use a value identity instead of object identity so a deserialized instance matches the original instance.
        private readonly long _id;
        private static long s_nextId;

        // use a value identity instead of object identity so a deserialized instance matches the original instance.
        /// <summary>
        /// The kind of annotation.
        /// </summary>
        public string? Kind { get; }
        /// <summary>
        /// The annotation's data.
        /// </summary>
        public string? Data { get; }

        /// <summary>
        /// Creates a new empty annotation.
        /// </summary>
        public SyntaxAnnotation()
        {
            _id = Interlocked.Increment(ref s_nextId);
        }

        /// <summary>
        /// Creates a new annotation without a value.
        /// </summary>
        /// <param name="kind"></param>
        public SyntaxAnnotation(string? kind)
            : this()
        {
            Kind = kind;
        }

        /// <summary>
        /// Creates a new annotation.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="data"></param>
        public SyntaxAnnotation(string? kind, string? data)
            : this(kind)
        {
            Data = data;
        }

        private SyntaxAnnotation(ObjectReader reader)
        {
            _id = reader.ReadInt64();
            Kind = reader.ReadString();
            Data = reader.ReadString();
        }

        bool IObjectWritable.ShouldReuseInSerialization => true;

        void IObjectWritable.WriteTo(ObjectWriter writer)
        {
            writer.WriteInt64(_id);
            writer.WriteString(Kind);
            writer.WriteString(Data);
        }

        private string GetDebuggerDisplay() =>
            string.Format("Annotation: Kind='{0}' Data='{1}'", Kind ?? "", Data ?? "");

        /// <inheritdoc/>
        public bool Equals(SyntaxAnnotation? other) =>
            other is not null && _id == other._id;

        /// <summary>
        /// Checks whether two annotations are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(SyntaxAnnotation? left, SyntaxAnnotation? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Checks whether two annotations are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(SyntaxAnnotation? left, SyntaxAnnotation? right) =>
            !(left == right);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as SyntaxAnnotation);

        /// <inheritdoc/>
        public override int GetHashCode() => _id.GetHashCode();
    }
}
