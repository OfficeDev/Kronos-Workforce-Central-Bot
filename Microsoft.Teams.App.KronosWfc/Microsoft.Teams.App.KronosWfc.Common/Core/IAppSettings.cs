//-----------------------------------------------------------------------
// <copyright file="IAppSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common.Core
{
    public interface IAppSettings
    {
        string AzureTableStorageConnectionString { get; }

        string TableName { get; }

        string AppInsightKey { get; }

        string LogInsightsFlag { get; }

    }
}
