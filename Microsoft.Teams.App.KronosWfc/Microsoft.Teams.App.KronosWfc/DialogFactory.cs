//-----------------------------------------------------------------------
// <copyright file="DialogFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc
{
    using Autofac;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Teams.App.KronosWfc.Models;

    /// <summary>
    /// dialog factory class.
    /// </summary>
    public class DialogFactory : IDialogFactory
    {
        private readonly IComponentContext scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogFactory"/> class.
        /// </summary>
        /// <param name="scope">Component context.</param>
        public DialogFactory(IComponentContext scope)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
        }

        public T CreateLogonResponseDialog<T>(LoginResponse response)
        {
            return this.scope.Resolve<T>(TypedParameter.From(response));
        }
    }
}