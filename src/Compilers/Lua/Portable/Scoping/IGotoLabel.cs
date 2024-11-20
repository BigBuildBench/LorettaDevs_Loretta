﻿using System.Runtime.CompilerServices;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The interface for a goto label.
    /// </summary>
    [InternalImplementationOnly]
    public interface IGotoLabel
    {
        /// <summary>
        /// The label's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The label's location.
        /// </summary>
        GotoLabelStatementSyntax? LabelSyntax { get; }

        /// <summary>
        /// The nodes that jump to this label.
        /// </summary>
        IEnumerable<GotoStatementSyntax> JumpSyntaxes { get; }
    }

    internal interface IGotoLabelInternal : IGotoLabel
    {
        void AddJump(GotoStatementSyntax jump);
    }

    internal class GotoLabel : IGotoLabelInternal
    {
        private readonly List<GotoStatementSyntax> _jumps = new();

        public GotoLabel(string name, GotoLabelStatementSyntax? label)
        {
            LorettaDebug.Assert(!string.IsNullOrEmpty(name));

            Name = name;
            LabelSyntax = label;
            JumpSyntaxes = SpecializedCollections.ReadOnlyEnumerable(_jumps);
        }

        public string Name { get; }

        public GotoLabelStatementSyntax? LabelSyntax { get; }

        public IEnumerable<GotoStatementSyntax> JumpSyntaxes { get; }

        public void AddJump(GotoStatementSyntax jump)
        {
            LorettaDebug.AssertNotNull(jump);
            _jumps.Add(jump);
        }
    }
}
