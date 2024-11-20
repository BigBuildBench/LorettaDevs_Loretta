﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxFactsGenerator
{
    internal static class KindUtils
    {
        public static KindList? ExtractKindList(SourceProductionContext context, WantedSymbols symbols)
        {
            if (symbols.SyntaxKindType is null)
                return null;

            var fields = symbols.SyntaxKindType.GetMembers()
                                               .OfType<IFieldSymbol>()
                                               .ToImmutableArray();

            return new KindList(ExtractKindInfo(context, symbols, fields));
        }

        private static ImmutableArray<KindInfo> ExtractKindInfo(
            SourceProductionContext context,
            WantedSymbols symbols,
            ImmutableArray<IFieldSymbol> fields)
        {
            var infos = ImmutableArray.CreateBuilder<KindInfo>(fields.Length);
            foreach (var field in fields)
            {
                var isTrivia = IsTrivia(symbols.TriviaAttributeType, field);
                var tokenInfo =
                    GetTokenInfo(symbols.TokenAttributeType, symbols.KeywordAttributeType, field);
                var unaryOperatorInfo =
                    GetOperatorInfo(symbols.UnaryOperatorAttributeType, field);
                var binaryOperatorInfo =
                    GetOperatorInfo(symbols.BinaryOperatorAttributeType, field);
                var extraCategories =
                    GetExtraCategories(symbols.ExtraCategoriesAttributeType, field, context);
                var properties =
                    GetProperties(symbols.PropertyAttributeType, field, context);

                var hasErrors = false;
                var location = field.Locations.Single();
                if (isTrivia && tokenInfo is not null)
                {
                    hasErrors = true;
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TriviaKindIsAlsoAToken, location));
                }

                if (tokenInfo is { IsKeyword: true, Text: null })
                {
                    hasErrors = true;
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.KeywordKindWithoutText, location));
                }

                if ((unaryOperatorInfo is not null || binaryOperatorInfo is not null) && string.IsNullOrWhiteSpace(tokenInfo?.Text))
                {
                    hasErrors = true;
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.OperatorKindWithoutText, location));
                }

                if (hasErrors)
                    continue;

                infos.Add(new KindInfo(
                    field,
                    isTrivia,
                    tokenInfo,
                    unaryOperatorInfo,
                    binaryOperatorInfo,
                    extraCategories,
                    properties));
            }

            return infos.ToImmutable();
        }

        private static bool IsTrivia(INamedTypeSymbol triviaAttributeType, IFieldSymbol field) =>
            Utilities.GetAttribute(field, triviaAttributeType) is not null;

        private static TokenInfo? GetTokenInfo(
            INamedTypeSymbol tokenAttributeType,
            INamedTypeSymbol keywordAttributeType,
            IFieldSymbol field)
        {
            if (Utilities.GetAttribute(field, keywordAttributeType) is AttributeData keywordAttributeData)
            {
                var text = keywordAttributeData.ConstructorArguments.Single().Value as string;
                if (string.IsNullOrWhiteSpace(text)) text = null;
                return new TokenInfo(text, true);
            }
            else if (Utilities.GetAttribute(field, tokenAttributeType) is AttributeData tokenAttributeData)
            {
                var text = tokenAttributeData.NamedArguments.SingleOrDefault(kv => kv.Key == "Text").Value.Value as string;
                if (string.IsNullOrWhiteSpace(text)) text = null;
                return new TokenInfo(text, false);
            }
            else
            {
                return null;
            }
        }

        private static OperatorInfo? GetOperatorInfo(
            INamedTypeSymbol operatorAttributeType,
            IFieldSymbol field)
        {
            var attr = Utilities.GetAttribute(field, operatorAttributeType);
            if (attr is null)
                return null;

            var precedence = (int) attr.ConstructorArguments[0].Value!;
            var expression = attr.ConstructorArguments[1];
            return new OperatorInfo(precedence, expression);
        }

        private static ImmutableArray<string> GetExtraCategories(
            INamedTypeSymbol extraCategoriesAttributeType,
            IFieldSymbol field,
            SourceProductionContext context)
        {
            var attr = Utilities.GetAttribute(field, extraCategoriesAttributeType);
            if (attr is null)
                return ImmutableArray<string>.Empty;

            if (attr.ApplicationSyntaxReference is not null)
            {
                var attrSyntax = (AttributeSyntax) attr.ApplicationSyntaxReference!.GetSyntax(context.CancellationToken);
                if (attrSyntax.ArgumentList is not null)
                {
                    foreach (var arg in attrSyntax.ArgumentList!.Arguments)
                    {
                        if (arg.Expression is not MemberAccessExpressionSyntax member
                            || member.Expression is not SimpleNameSyntax baseName
                            || !baseName.Identifier.Text.Equals("SyntaxKindCategory", StringComparison.Ordinal))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.CategoryNotInConstantClass, arg.GetLocation()));
                        }
                    }
                }
            }

            var categories = attr.ConstructorArguments.Single().Values.Select(arg => (string) arg.Value!).ToImmutableArray();
            return categories;
        }

        private static ImmutableDictionary<string, TypedConstant> GetProperties(INamedTypeSymbol propertyAttributeType, IFieldSymbol field, SourceProductionContext context)
        {
            var attributes = Utilities.GetAttributes(field, propertyAttributeType);
            if (attributes.IsEmpty)
                return ImmutableDictionary<string, TypedConstant>.Empty;

            foreach (var attribute in attributes)
            {
                if (attribute.ApplicationSyntaxReference is null)
                    continue;

                var attrSyntax = (AttributeSyntax) attribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);
                if (attrSyntax.ArgumentList is null)
                    continue;

                var arg = attrSyntax.ArgumentList.Arguments.First();
                if (arg.Expression is not MemberAccessExpressionSyntax member
                        || member.Expression is not SimpleNameSyntax baseName
                        || !baseName.Identifier.Text.Equals("SyntaxKindProperty", StringComparison.Ordinal))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.PropertyNotInConstantClass, arg.GetLocation()));
                }
            }

            var properties = attributes.Select(attr => new KeyValuePair<string, TypedConstant>((string) attr.ConstructorArguments[0].Value!, attr.ConstructorArguments[1]));
            return ImmutableDictionary.CreateRange(properties);
        }

        public static readonly SourceText SyntaxKindAttributesText = SourceText.From(SyntaxKindAttributes, Encoding.UTF8);

        public const string SyntaxKindAttributes = @"
using System;

#nullable enable

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The extra categories attribute
    /// Can be checked by the Is{CategoryName} methods in SyntaxFacts.
    /// All members of a category can also be retrieved by the Get{CategoryName} methods in SyntaxFacts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class ExtraCategoriesAttribute : Attribute
    {
        public ExtraCategoriesAttribute(params string[] extraCategories)
        {
            Categories = extraCategories;
        }

        public string[] Categories { get; }
    }

    /// <summary>
    /// Properties associated with the enum value.
    /// Can be retrieved from the Get{Key} methods in SyntaxFacts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    internal sealed class PropertyAttribute : Attribute
    {
        public PropertyAttribute(string key, object? value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public object? Value { get; }
    }

    /// <summary>
    /// The trivia indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref=""SyntaxKind""/> is a trivia's.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class TriviaAttribute : Attribute
    {
    }

    /// <summary>
    /// The token indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref=""SyntaxKind""/> is a token's.
    /// May optionally indicate a fixed text for the token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class TokenAttribute : Attribute
    {
        /// <summary>
        /// The <see cref=""SyntaxToken""/>'s fixed text.
        /// </summary>
        public string? Text { get; set; }
    }

    /// <summary>
    /// The keyword indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref=""SyntaxKind""/> is a keywords's
    /// and the keyword fixed text.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class KeywordAttribute : Attribute
    {
        public KeywordAttribute(string text)
        {
            Text = text;
        }

        /// <summary>
        /// The keyword's text.
        /// </summary>
        public String Text { get; }
    }

    /// <summary>
    /// The unary operator indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this
    /// <see cref=""SyntaxKind""/> is an unary operator's with the
    /// provided precedence.
    /// THIS DOES NOT IMPLY THE <see cref=""TokenAttribute""/>
    /// ATTRIBUTE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class UnaryOperatorAttribute : Attribute
    {
        public UnaryOperatorAttribute(int precedence, SyntaxKind expressionKind)
        {
            Precedence = precedence;
        }

        /// <summary>
        /// The unary operator's precedence.
        /// </summary>
        public int Precedence { get; }
    }

    /// <summary>
    /// The binary operator indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that
    /// this <see cref=""SyntaxKind""/> is a binary operator's
    /// with the provided precedence.
    /// THIS DOES NOT IMPLY THE <see cref=""TokenAttribute""/>
    /// ATTRIBUTE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class BinaryOperatorAttribute : Attribute
    {
        public BinaryOperatorAttribute(int precedence, SyntaxKind expressionKind)
        {
            Precedence = precedence;
        }

        /// <summary>
        /// The binary operator's precedence.
        /// </summary>
        public int Precedence { get; }
    }
}";
    }
}
