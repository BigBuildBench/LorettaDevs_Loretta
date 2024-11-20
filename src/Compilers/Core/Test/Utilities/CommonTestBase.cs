﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Test.Utilities
{
    /// <summary>
    /// Base class for all language specific tests.
    /// </summary>
    public abstract partial class CommonTestBase : TestBase
    {
        public static string WithWindowsLineBreaks(string source)
            => source.Replace(Environment.NewLine, "\r\n");
    }
}
