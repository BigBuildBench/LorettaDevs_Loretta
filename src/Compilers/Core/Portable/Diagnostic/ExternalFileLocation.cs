﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A program location in source code.
    /// </summary>
    internal sealed class ExternalFileLocation : Location, IEquatable<ExternalFileLocation?>
    {
        private readonly TextSpan _sourceSpan;
        private readonly FileLinePositionSpan _lineSpan;

        internal ExternalFileLocation(string filePath, TextSpan sourceSpan, LinePositionSpan lineSpan)
        {
            _sourceSpan = sourceSpan;
            _lineSpan = new FileLinePositionSpan(filePath, lineSpan);
        }

        public override TextSpan SourceSpan => _sourceSpan;

        public string FilePath => _lineSpan.Path;

        public override FileLinePositionSpan GetLineSpan() => _lineSpan;

        public override LocationKind Kind => LocationKind.ExternalFile;

        public override bool Equals(object? obj) => Equals(obj as ExternalFileLocation);

        public bool Equals(ExternalFileLocation? obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            return obj != null
                && _sourceSpan == obj._sourceSpan
                && _lineSpan.Equals(obj._lineSpan);
        }

        public override int GetHashCode() => Hash.Combine(_lineSpan.GetHashCode(), _sourceSpan.GetHashCode());
    }
}
