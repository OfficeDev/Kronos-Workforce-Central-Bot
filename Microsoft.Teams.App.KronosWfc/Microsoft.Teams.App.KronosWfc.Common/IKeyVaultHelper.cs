//-----------------------------------------------------------------------
// <copyright file="KeyVaultHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common
{
    /// <summary>
    /// key vault helper interface
    /// </summary>
    public interface IKeyVaultHelper
    {
        /// <summary>
        /// get secret by uri
        /// </summary>
        /// <param name="resourceUri">key url</param>
        /// <returns></returns>
        string GetSecretByUri(string resourceUri);
    }
}