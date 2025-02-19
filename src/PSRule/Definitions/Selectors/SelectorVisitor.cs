// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Definitions.Selectors
{
    internal interface ISelector : ILanguageBlock
    {

    }

    [DebuggerDisplay("Id: {Id}")]
    internal sealed class SelectorVisitor : ISelector
    {
        private readonly LanguageExpressionOuterFn _Fn;

        public SelectorVisitor(ResourceId id, SourceFile source, LanguageIf expression)
        {
            Id = id;
            Source = source;
            InstanceId = Guid.NewGuid();
            var builder = new LanguageExpressionBuilder();
            _Fn = builder.Build(expression);
        }

        public Guid InstanceId { get; }

        public ResourceId Id { get; }

        public SourceFile Source { get; }

        [Obsolete("Use Source property instead.")]
        string ILanguageBlock.SourcePath => Source.Path;

        [Obsolete("Use Source property instead.")]
        string ILanguageBlock.Module => Source.Module;

        public bool Match(object o)
        {
            var context = new ExpressionContext(Source);
            context.Debug(PSRuleResources.SelectorMatchTrace, Id);
            return _Fn(context, o).GetValueOrDefault(false);
        }
    }
}
