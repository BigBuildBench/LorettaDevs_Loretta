﻿namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    /// <summary>
    /// The naming strategy to use to convert a slot into a variable
    /// name.
    /// Uses the provided scope to check if the variable name is not being
    /// used already.
    /// </summary>
    /// <param name="slot">The slot to convert to a variable name.</param>
    /// <param name="scopes">The scopes the slot will be used in.</param>
    /// <returns></returns>
    public delegate string NamingStrategy(int slot, IEnumerable<IScope> scopes);
}
