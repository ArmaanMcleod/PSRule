﻿using PSRule.Commands;
using PSRule.Pipeline;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSRule.Host
{
    /// <summary>
    /// A dynamic variable $Rule used during Rule execution.
    /// </summary>
    internal sealed class RuleVariable : PSVariable
    {
        private const string VARIABLE_NAME = "Rule";

        private readonly Runtime.Rule _Value;

        public RuleVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new Runtime.Rule();
        }

        public override object Value
        {
            get
            {
                return _Value;
            }
        }
    }

    /// <summary>
    /// A dynamic variable $LocalizedData used during Rule execution.
    /// </summary>
    internal sealed class LocalizedDataVariable : PSVariable
    {
        private const string VARIABLE_NAME = "LocalizedData";

        private readonly Runtime.LocalizedData _Value;

        public LocalizedDataVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new Runtime.LocalizedData();
        }

        public override object Value
        {
            get
            {
                return _Value;
            }
        }
    }

    /// <summary>
    /// An assertion helper variable $Assert used during Rule execution.
    /// </summary>
    internal sealed class AssertVariable : PSVariable
    {
        private const string VARIABLE_NAME = "Assert";

        private readonly Runtime.Assert _Value;

        public AssertVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new Runtime.Assert();
        }

        public override object Value
        {
            get
            {
                return _Value;
            }
        }
    }

    /// <summary>
    /// A dynamic variable used during Rule execution.
    /// </summary>
    internal sealed class TargetObjectVariable : PSVariable
    {
        private const string VARIABLE_NAME = "TargetObject";

        public TargetObjectVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {

        }

        public override object Value
        {
            get
            {
                return PipelineContext.CurrentThread.TargetObject;
            }
        }
    }

    internal sealed class ConfigurationVariable : PSVariable
    {
        private const string VARIABLE_NAME = "Configuration";

        private readonly RuntimeRuleConfigurationView _Value;

        public ConfigurationVariable()
            : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
        {
            _Value = new RuntimeRuleConfigurationView(); 
        }

        public override object Value
        {
            get
            {
                return _Value;
            }
        }
    }

    internal static class HostState
    {
        /// <summary>
        /// Define language commands.
        /// </summary>
        private readonly static SessionStateCmdletEntry[] BuiltInCmdlets = new SessionStateCmdletEntry[]
        {
            new SessionStateCmdletEntry("New-RuleDefinition", typeof(NewRuleDefinitionCommand), null),
            new SessionStateCmdletEntry("Write-Recommendation", typeof(WriteRecommendationCommand), null),
            new SessionStateCmdletEntry("Write-Reason", typeof(WriteReasonCommand), null),
            new SessionStateCmdletEntry("Assert-Exists", typeof(AssertExistsCommand), null),
            new SessionStateCmdletEntry("Assert-Within", typeof(AssertWithinCommand), null),
            new SessionStateCmdletEntry("Assert-Match", typeof(AssertMatchCommand), null),
            new SessionStateCmdletEntry("Assert-TypeOf", typeof(AssertTypeOfCommand), null),
            new SessionStateCmdletEntry("Assert-AllOf", typeof(AssertAllOfCommand), null),
            new SessionStateCmdletEntry("Assert-AnyOf", typeof(AssertAnyOfCommand), null),
        };

        /// <summary>
        /// Define language aliases.
        /// </summary>
        private readonly static SessionStateAliasEntry[] BuiltInAliases = new SessionStateAliasEntry[]
        {
            new SessionStateAliasEntry("rule", "New-RuleDefinition", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("recommend", "Write-Recommendation", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("hint", "Write-Recommendation", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("reason", "Write-Reason", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("exists", "Assert-Exists", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("within", "Assert-Within", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("match", "Assert-Match", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("typeof", "Assert-TypeOf", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("allof", "Assert-AllOf", string.Empty, ScopedItemOptions.ReadOnly),
            new SessionStateAliasEntry("anyof", "Assert-AnyOf", string.Empty, ScopedItemOptions.ReadOnly),
        };

        /// <summary>
        /// Create a default session state.
        /// </summary>
        /// <returns></returns>
        public static InitialSessionState CreateSessionState()
        {
            var state = InitialSessionState.CreateDefault();

            // Add in language elements
            state.Commands.Add(BuiltInCmdlets);
            state.Commands.Add(BuiltInAliases);

            // Set execution policy
            SetExecutionPolicy(state: state, executionPolicy: Microsoft.PowerShell.ExecutionPolicy.RemoteSigned);

            return state;
        }

        private static void SetExecutionPolicy(InitialSessionState state, Microsoft.PowerShell.ExecutionPolicy executionPolicy)
        {
            // Only set execution policy on Windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                state.ExecutionPolicy = executionPolicy;
            }
        }
    }
}
