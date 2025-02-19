// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSRule.Configuration
{
    /// <summary>
    /// Options that configure the execution sandbox.
    /// </summary>
    public sealed class ExecutionOption : IEquatable<ExecutionOption>
    {
        private const LanguageMode DEFAULT_LANGUAGEMODE = Configuration.LanguageMode.FullLanguage;
        private const bool DEFAULT_INCONCLUSIVEWARNING = true;
        private const bool DEFAULT_NOTPROCESSEDWARNING = true;
        private const bool DEFAULT_SUPPRESSEDRULEWARNING = true;
        private const bool DEFAULT_ALIASREFERENCEWARNING = true;
        private const bool DEFAULT_INVARIANTCULTUREWARNING = true;

        internal static readonly ExecutionOption Default = new ExecutionOption
        {
            AliasReferenceWarning = DEFAULT_ALIASREFERENCEWARNING,
            LanguageMode = DEFAULT_LANGUAGEMODE,
            InconclusiveWarning = DEFAULT_INCONCLUSIVEWARNING,
            NotProcessedWarning = DEFAULT_NOTPROCESSEDWARNING,
            SuppressedRuleWarning = DEFAULT_SUPPRESSEDRULEWARNING,
            InvariantCultureWarning = DEFAULT_INVARIANTCULTUREWARNING
        };

        public ExecutionOption()
        {
            AliasReferenceWarning = null;
            LanguageMode = null;
            InconclusiveWarning = null;
            NotProcessedWarning = null;
            SuppressedRuleWarning = null;
            InvariantCultureWarning = null;
        }

        public ExecutionOption(ExecutionOption option)
        {
            if (option == null)
                return;

            AliasReferenceWarning = option.AliasReferenceWarning;
            LanguageMode = option.LanguageMode;
            InconclusiveWarning = option.InconclusiveWarning;
            NotProcessedWarning = option.NotProcessedWarning;
            SuppressedRuleWarning = option.SuppressedRuleWarning;
            InvariantCultureWarning = option.InvariantCultureWarning;
        }

        public override bool Equals(object obj)
        {
            return obj is ExecutionOption option && Equals(option);
        }

        public bool Equals(ExecutionOption other)
        {
            return other != null &&
                AliasReferenceWarning == other.AliasReferenceWarning &&
                LanguageMode == other.LanguageMode &&
                InconclusiveWarning == other.InconclusiveWarning &&
                NotProcessedWarning == other.NotProcessedWarning &&
                SuppressedRuleWarning == other.NotProcessedWarning &&
                InvariantCultureWarning == other.InvariantCultureWarning;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (AliasReferenceWarning.HasValue ? AliasReferenceWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (LanguageMode.HasValue ? LanguageMode.Value.GetHashCode() : 0);
                hash = hash * 23 + (InconclusiveWarning.HasValue ? InconclusiveWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (NotProcessedWarning.HasValue ? NotProcessedWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (SuppressedRuleWarning.HasValue ? SuppressedRuleWarning.Value.GetHashCode() : 0);
                hash = hash * 23 + (InvariantCultureWarning.HasValue ? InvariantCultureWarning.Value.GetHashCode() : 0);
                return hash;
            }
        }

        internal static ExecutionOption Combine(ExecutionOption o1, ExecutionOption o2)
        {
            var result = new ExecutionOption(o1)
            {
                AliasReferenceWarning = o1.AliasReferenceWarning ?? o2.AliasReferenceWarning,
                LanguageMode = o1.LanguageMode ?? o2.LanguageMode,
                InconclusiveWarning = o1.InconclusiveWarning ?? o2.InconclusiveWarning,
                NotProcessedWarning = o1.NotProcessedWarning ?? o2.NotProcessedWarning,
                SuppressedRuleWarning = o1.SuppressedRuleWarning ?? o2.SuppressedRuleWarning,
                InvariantCultureWarning = o1.InvariantCultureWarning ?? o2.InvariantCultureWarning
            };
            return result;
        }

        /// <summary>
        /// Determines if a warning is raised when an alias to a resource is used.
        /// </summary>
        [DefaultValue(null)]
        public bool? AliasReferenceWarning { get; set; }

        /// <summary>
        /// The langauge mode to execute PowerShell code with.
        /// </summary>
        [DefaultValue(null)]
        public LanguageMode? LanguageMode { get; set; }

        /// <summary>
        /// Determines if a warning is raised when a rule does not return pass or fail.
        /// </summary>
        [DefaultValue(null)]
        public bool? InconclusiveWarning { get; set; }

        /// <summary>
        /// Determines if a warning is raised when an object is not processed by any rule.
        /// </summary>
        [DefaultValue(null)]
        public bool? NotProcessedWarning { get; set; }

        /// <summary>
        /// Determines if a warning is raised when a rule is suppressed.
        /// </summary>
        [DefaultValue(null)]
        public bool? SuppressedRuleWarning { get; set; }

        /// <summary>
        /// Determines if warning is raised when invariant culture is used.
        /// </summary>
        [DefaultValue(null)]
        public bool? InvariantCultureWarning { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryBool("PSRULE_EXECUTION_ALIASREFERENCEWARNING", out var bvalue))
                AliasReferenceWarning = bvalue;

            if (env.TryEnum("PSRULE_EXECUTION_LANGUAGEMODE", out LanguageMode languageMode))
                LanguageMode = languageMode;

            if (env.TryBool("PSRULE_EXECUTION_INCONCLUSIVEWARNING", out bvalue))
                InconclusiveWarning = bvalue;

            if (env.TryBool("PSRULE_EXECUTION_NOTPROCESSEDWARNING", out bvalue))
                NotProcessedWarning = bvalue;

            if (env.TryBool("PSRULE_EXECUTION_SUPPRESSEDRULEWARNING", out bvalue))
                SuppressedRuleWarning = bvalue;

            if (env.TryBool("PSRULE_EXECUTION_INVARIANTCULTUREWARNING", out bvalue))
                InvariantCultureWarning = bvalue;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopBool("Execution.AliasReferenceWarning", out var bvalue))
                AliasReferenceWarning = bvalue;

            if (index.TryPopEnum("Execution.LanguageMode", out LanguageMode languageMode))
                LanguageMode = languageMode;

            if (index.TryPopBool("Execution.InconclusiveWarning", out bvalue))
                InconclusiveWarning = bvalue;

            if (index.TryPopBool("Execution.NotProcessedWarning", out bvalue))
                NotProcessedWarning = bvalue;

            if (index.TryPopBool("Execution.SuppressedRuleWarning", out bvalue))
                SuppressedRuleWarning = bvalue;

            if (index.TryPopBool("Execution.InvariantCultureWarning", out bvalue))
                InvariantCultureWarning = bvalue;
        }
    }
}
