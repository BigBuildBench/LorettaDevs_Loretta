﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal partial class SyntaxList
    {
        internal abstract class WithManyChildrenBase : SyntaxList
        {
            internal readonly ArrayElement<GreenNode>[] children;

            internal WithManyChildrenBase(ArrayElement<GreenNode>[] children)
            {
                this.children = children;
                InitializeChildren();
            }

            internal WithManyChildrenBase(DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, ArrayElement<GreenNode>[] children)
                : base(diagnostics, annotations)
            {
                this.children = children;
                InitializeChildren();
            }

            private void InitializeChildren()
            {
                int n = children.Length;
                if (n < byte.MaxValue)
                {
                    SlotCount = (byte) n;
                }
                else
                {
                    SlotCount = byte.MaxValue;
                }

                for (int i = 0; i < children.Length; i++)
                {
                    AdjustFlagsAndWidth(children[i]);
                }
            }

            internal WithManyChildrenBase(ObjectReader reader)
                : base(reader)
            {
                var length = reader.ReadInt32();

                children = new ArrayElement<GreenNode>[length];
                for (var i = 0; i < length; i++)
                {
                    children[i].Value = (GreenNode) reader.ReadValue();
                }

                InitializeChildren();
            }

            internal override void WriteTo(ObjectWriter writer)
            {
                base.WriteTo(writer);

                // PERF: Write the array out manually.Profiling shows that this is cheaper than converting to 
                // an array in order to use writer.WriteValue.
                writer.WriteInt32(children.Length);

                for (var i = 0; i < children.Length; i++)
                {
                    writer.WriteValue(children[i].Value);
                }
            }

            protected override int GetSlotCount() => children.Length;

            internal override GreenNode GetSlot(int index) => children[index];

            internal override void CopyTo(ArrayElement<GreenNode>[] array, int offset) =>
                Array.Copy(children, 0, array, offset, children.Length);

            internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
            {
                var separated = SlotCount > 1 && HasNodeTokenPattern();
                if (parent != null && parent.ShouldCreateWeakList())
                {
                    return separated
                        ? new Syntax.SyntaxList.SeparatedWithManyWeakChildren(this, parent, position)
                        : new Syntax.SyntaxList.WithManyWeakChildren(this, parent, position);
                }
                else
                {
                    return separated
                        ? new Syntax.SyntaxList.SeparatedWithManyChildren(this, parent, position)
                        : new Syntax.SyntaxList.WithManyChildren(this, parent, position);
                }
            }

            private bool HasNodeTokenPattern()
            {
                for (int i = 0; i < SlotCount; i++)
                {
                    // even slots must not be tokens, odds slots must be tokens
                    if (GetSlot(i).IsToken == ((i & 1) == 0))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal sealed class WithManyChildren : WithManyChildrenBase
        {
            static WithManyChildren()
            {
                ObjectBinder.RegisterTypeReader(typeof(WithManyChildren), r => new WithManyChildren(r));
            }

            internal WithManyChildren(ArrayElement<GreenNode>[] children)
                : base(children)
            {
            }

            internal WithManyChildren(DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, ArrayElement<GreenNode>[] children)
                : base(diagnostics, annotations, children)
            {
            }

            internal WithManyChildren(ObjectReader reader)
                : base(reader)
            {
            }

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? errors) =>
                new WithManyChildren(errors, GetAnnotations(), children);

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations) =>
                new WithManyChildren(GetDiagnostics(), annotations, children);
        }
    }
}
