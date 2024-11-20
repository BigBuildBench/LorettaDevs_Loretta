﻿using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class GotoWalker : BaseWalker
        {
            private readonly IDictionary<SyntaxNode, IGotoLabel> _labels;

            public GotoWalker(
                IDictionary<SyntaxNode, IScope> scopes,
                IDictionary<SyntaxNode, IGotoLabel> labels)
                : base(scopes)
            {
                LorettaDebug.AssertNotNull(labels);

                _labels = labels;
            }

            public override void VisitGotoStatement(GotoStatementSyntax node)
            {
                var scope = FindScope(node) ?? throw new System.Exception("Scope not found for node.");
                if (string.IsNullOrWhiteSpace(node.LabelName.Text))
                    return;
                var label = scope.GetOrCreateLabel(node.LabelName.Text);
                label.AddJump(node);
                _labels[node] = label;
            }
        }
    }
}
