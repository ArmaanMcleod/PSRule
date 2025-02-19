﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Runtime
{
    public abstract class ScopedItem
    {
        private readonly RunspaceContext _Context;

        internal ScopedItem()
        {

        }

        internal ScopedItem(RunspaceContext context)
        {
            _Context = context;
        }

        #region Helper methods

        internal void RequireScope(RunspaceScope scope)
        {
            if (GetContext().IsScope(scope))
                return;

            throw new RuntimeScopeException();
        }

        internal RunspaceContext GetContext()
        {
            return _Context ?? RunspaceContext.CurrentThread;
        }

        #endregion Helper methods
    }
}
