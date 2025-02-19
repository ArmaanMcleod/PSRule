// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using PSRule.Definitions.Baselines;

namespace PSRule.Configuration
{
    public class BaselineOption
    {
        internal sealed class BaselineRef : BaselineOption
        {
            public readonly string Name;

            public BaselineRef(string name)
            {
                Name = name;
            }
        }

        internal sealed class BaselineInline : BaselineOption, IBaselineSpec
        {
            public BaselineInline()
            {
                Binding = new BindingOption();
                Configuration = new ConfigurationOption();
                Rule = new RuleOption();
            }

            public BindingOption Binding { get; set; }

            public ConfigurationOption Configuration { get; set; }

            public ConventionOption Convention { get; set; }

            public RuleOption Rule { get; set; }
        }

        public static implicit operator BaselineOption(Hashtable hashtable)
        {
            return FromHashtable(hashtable);
        }

        public static implicit operator BaselineOption(string value)
        {
            return FromString(value);
        }

        public static BaselineOption FromHashtable(Hashtable hashtable)
        {
            var option = new BaselineInline();
            if (hashtable != null)
            {
                // Build index to allow mapping
                var index = PSRuleOption.BuildIndex(hashtable);
                Load(option, index);
            }
            return option;
        }

        public static BaselineOption FromString(string value)
        {
            return new BaselineRef(value);
        }

        internal static void Load(IBaselineSpec option, EnvironmentHelper env)
        {
            // Binding.Field - currently not supported

            if (env.TryBool("PSRULE_BINDING_IGNORECASE", out var ignoreCase))
                option.Binding.IgnoreCase = ignoreCase;

            if (env.TryString("PSRULE_BINDING_NAMESEPARATOR", out var nameSeparator))
                option.Binding.NameSeparator = nameSeparator;

            if (env.TryBool("PSRULE_BINDING_PREFERTARGETINFO", out var preferTargetInfo))
                option.Binding.PreferTargetInfo = preferTargetInfo;

            if (env.TryStringArray("PSRULE_BINDING_TARGETNAME", out var targetName))
                option.Binding.TargetName = targetName;

            if (env.TryStringArray("PSRULE_BINDING_TARGETTYPE", out var targetType))
                option.Binding.TargetType = targetType;

            if (env.TryBool("PSRULE_BINDING_USEQUALIFIEDNAME", out var useQualifiedName))
                option.Binding.UseQualifiedName = useQualifiedName;

            if (env.TryString("PSRULE_RULE_BASELINE", out var baseline))
                option.Rule.Baseline = baseline;

            if (env.TryStringArray("PSRULE_RULE_EXCLUDE", out var exclude))
                option.Rule.Exclude = exclude;

            if (env.TryBool("PSRULE_RULE_INCLUDELOCAL", out var includeLocal))
                option.Rule.IncludeLocal = includeLocal;

            if (env.TryStringArray("PSRULE_RULE_INCLUDE", out var include))
                option.Rule.Include = include;

            // Rule.Tag - currently not supported

            // Process configuration values
            option.Configuration.Load(env);
        }

        /// <summary>
        /// Load matching values
        /// </summary>
        /// <param name="option">A baseline options object to load.</param>
        /// <param name="properties">One or more indexed properties.</param>
        internal static void Load(IBaselineSpec option, Dictionary<string, object> properties)
        {
            if (properties.TryPopValue("Binding.Field", out Hashtable map))
                option.Binding.Field = new FieldMap(map);

            if (properties.TryPopBool("Binding.IgnoreCase", out var ignoreCase))
                option.Binding.IgnoreCase = ignoreCase;

            if (properties.TryPopString("Binding.NameSeparator", out var nameSeparator))
                option.Binding.NameSeparator = nameSeparator;

            if (properties.TryPopBool("Binding.PreferTargetInfo", out var preferTargetInfo))
                option.Binding.PreferTargetInfo = preferTargetInfo;

            if (properties.TryPopStringArray("Binding.TargetName", out var targetName))
                option.Binding.TargetName = targetName;

            if (properties.TryPopStringArray("Binding.TargetType", out var targetType))
                option.Binding.TargetType = targetType;

            if (properties.TryPopValue("Binding.UseQualifiedName", out bool useQualifiedName))
                option.Binding.UseQualifiedName = useQualifiedName;

            if (properties.TryPopString("Rule.Baseline", out var baseline))
                option.Rule.Baseline = baseline;

            if (properties.TryPopStringArray("Rule.Exclude", out var exclude))
                option.Rule.Exclude = exclude;

            if (properties.TryPopBool("Rule.IncludeLocal", out var includeLocal))
                option.Rule.IncludeLocal = includeLocal;

            if (properties.TryPopStringArray("Rule.Include", out var include))
                option.Rule.Include = include;

            if (properties.TryPopValue("Rule.Tag", out Hashtable tag))
                option.Rule.Tag = tag;

            // Process configuration values
            option.Configuration.Load(properties);
        }
    }
}
