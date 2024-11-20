﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal partial class SyntaxList
    {
        internal class WithThreeChildren : SyntaxList
        {
            static WithThreeChildren()
            {
                ObjectBinder.RegisterTypeReader(typeof(WithThreeChildren), r => new WithThreeChildren(r));
            }

            private readonly GreenNode _child0;
            private readonly GreenNode _child1;
            private readonly GreenNode _child2;

            internal WithThreeChildren(GreenNode child0, GreenNode child1, GreenNode child2)
            {
                SlotCount = 3;
                AdjustFlagsAndWidth(child0);
                _child0 = child0;
                AdjustFlagsAndWidth(child1);
                _child1 = child1;
                AdjustFlagsAndWidth(child2);
                _child2 = child2;
            }

            internal WithThreeChildren(DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, GreenNode child0, GreenNode child1, GreenNode child2)
                : base(diagnostics, annotations)
            {
                SlotCount = 3;
                AdjustFlagsAndWidth(child0);
                _child0 = child0;
                AdjustFlagsAndWidth(child1);
                _child1 = child1;
                AdjustFlagsAndWidth(child2);
                _child2 = child2;
            }

            internal WithThreeChildren(ObjectReader reader)
                : base(reader)
            {
                SlotCount = 3;
                _child0 = (GreenNode) reader.ReadValue();
                AdjustFlagsAndWidth(_child0);
                _child1 = (GreenNode) reader.ReadValue();
                AdjustFlagsAndWidth(_child1);
                _child2 = (GreenNode) reader.ReadValue();
                AdjustFlagsAndWidth(_child2);
            }

            internal override void WriteTo(ObjectWriter writer)
            {
                base.WriteTo(writer);
                writer.WriteValue(_child0);
                writer.WriteValue(_child1);
                writer.WriteValue(_child2);
            }

            internal override GreenNode? GetSlot(int index)
            {
                return index switch
                {
                    0 => _child0,
                    1 => _child1,
                    2 => _child2,
                    _ => null,
                };
            }

            internal override void CopyTo(ArrayElement<GreenNode>[] array, int offset)
            {
                array[offset].Value = _child0;
                array[offset + 1].Value = _child1;
                array[offset + 2].Value = _child2;
            }

            internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) =>
                new Syntax.SyntaxList.WithThreeChildren(this, parent, position);

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? errors) =>
                new WithThreeChildren(errors, GetAnnotations(), _child0, _child1, _child2);

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations) =>
                new WithThreeChildren(GetDiagnostics(), annotations, _child0, _child1, _child2);
        }
    }
}
