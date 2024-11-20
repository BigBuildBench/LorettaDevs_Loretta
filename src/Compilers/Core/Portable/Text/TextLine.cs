﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Information about the character boundaries of a single line of text.
    /// </summary>
    public readonly struct TextLine : IEquatable<TextLine>
    {
        private readonly SourceText? _text;
        private readonly int _start;
        private readonly int _endIncludingBreaks;

        private TextLine(SourceText text, int start, int endIncludingBreaks)
        {
            _text = text;
            _start = start;
            _endIncludingBreaks = endIncludingBreaks;
        }

        /// <summary>
        /// Creates a <see cref="TextLine"/> instance.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="span">The span of the line.</param>
        /// <returns>An instance of <see cref="TextLine"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The span does not represent a text line.</exception>
        public static TextLine FromSpan(SourceText text, TextSpan span)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));
            if (span.Start > text.Length || span.Start < 0 || span.End > text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(span));
            }

            if (text.Length > 0)
            {
                // check span is start of line
                if (span.Start > 0 && !TextUtilities.IsAnyLineBreakCharacter(text[span.Start - 1]))
                {
                    throw new ArgumentOutOfRangeException(nameof(span), CodeAnalysisResources.SpanDoesNotIncludeStartOfLine);
                }

                bool endIncludesLineBreak = false;
                if (span.End > span.Start)
                {
                    endIncludesLineBreak = TextUtilities.IsAnyLineBreakCharacter(text[span.End - 1]);
                }

                if (!endIncludesLineBreak && span.End < text.Length)
                {
                    var lineBreakLen = TextUtilities.GetLengthOfLineBreak(text, span.End);
                    if (lineBreakLen > 0)
                    {
                        // adjust span to include line breaks
                        endIncludesLineBreak = true;
                        span = new TextSpan(span.Start, span.Length + lineBreakLen);
                    }
                }

                // check end of span is at end of line
                if (span.End < text.Length && !endIncludesLineBreak)
                {
                    throw new ArgumentOutOfRangeException(nameof(span), CodeAnalysisResources.SpanDoesNotIncludeEndOfLine);
                }

                return new TextLine(text, span.Start, span.End);
            }
            else
            {
                return new TextLine(text, 0, 0);
            }
        }

        /// <summary>
        /// Gets the source text.
        /// </summary>
        public SourceText? Text => _text;

        /// <summary>
        /// Gets the zero-based line number.
        /// </summary>
        public int LineNumber => _text?.Lines.IndexOf(_start) ?? 0;

        /// <summary>
        /// Gets the start position of the line.
        /// </summary>
        public int Start => _start;

        /// <summary>
        /// Gets the end position of the line not including the line break.
        /// </summary>
        public int End => _endIncludingBreaks - LineBreakLength;

        private int LineBreakLength
        {
            get
            {
                if (_text == null || _text.Length == 0 || _endIncludingBreaks == _start)
                {
                    return 0;
                }

                TextUtilities.GetStartAndLengthOfLineBreakEndingAt(_text, _endIncludingBreaks - 1, out _, out var lineBreakLength);
                return lineBreakLength;
            }
        }

        /// <summary>
        /// Gets the end position of the line including the line break.
        /// </summary>
        public int EndIncludingLineBreak => _endIncludingBreaks;

        /// <summary>
        /// Gets the line span not including the line break.
        /// </summary>
        public TextSpan Span => TextSpan.FromBounds(Start, End);

        /// <summary>
        /// Gets the line span including the line break.
        /// </summary>
        public TextSpan SpanIncludingLineBreak => TextSpan.FromBounds(Start, EndIncludingLineBreak);

        /// <summary>
        /// Returns the text for this line.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_text == null || _text.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                return _text.ToString(Span);
            }
        }

        /// <summary>
        /// Checks whether two text lines are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(TextLine left, TextLine right) => left.Equals(right);

        /// <summary>
        /// Checks whether two text lines are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(TextLine left, TextLine right) => !left.Equals(right);

        /// <inheritdoc/>
        public bool Equals(TextLine other)
        {
            return other._text == _text
                && other._start == _start
                && other._endIncludingBreaks == _endIncludingBreaks;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is TextLine line)
            {
                return Equals(line);
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode() =>
            Hash.Combine(_text, Hash.Combine(_start, _endIncludingBreaks));
    }
}
