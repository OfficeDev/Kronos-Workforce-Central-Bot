//-----------------------------------------------------------------------
// <copyright file="IDialogFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc
{
    using Microsoft.Teams.App.KronosWfc.Models;

    /// <summary>
    /// Interface to create dialogs.
    /// </summary>
    public interface IDialogFactory
    {
        /// <summary>
        /// Create dialogs with logon response.
        /// </summary>
        /// <typeparam name="T">Generic dialog.</typeparam>
        /// <param name="response">LoginResponse response.</param>
        /// <returns>Generic dialog type.</returns>
        T CreateLogonResponseDialog<T>(LoginResponse response);
    }
}