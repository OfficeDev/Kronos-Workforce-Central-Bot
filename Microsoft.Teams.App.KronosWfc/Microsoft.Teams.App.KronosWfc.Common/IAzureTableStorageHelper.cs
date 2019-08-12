//-----------------------------------------------------------------------
// <copyright file="IAzureTableStorageHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public interface IAzureTableStorageHelper
    {
        CloudStorageAccount StorageAccount { get; }
        CloudTableClient TableClient { get; }

        Task<TableResult> Execute(string storageTableName, TableOperation tableOperation);
        Task<T> ExecuteQueryUsingPointQueryAsync<T>(string partitionKey, string rowKey, string tableName = null) where T : TableEntity;
        Task<List<DynamicTableEntity>> GetTimeOffPaycodes(string tableName);
        Task<T> InsertOrMergeTableEntityAsync<T>(T entity, string activityTableName) where T : TableEntity;
        Task<string> Retrive<T>(string personNumber, string tenantId, string channelId, string tableName = "KronosUserDetails");
        Task<int> RetriveAndDelete<T>(string personNumber, string tenantId, string channelId, string teamsUserId, string tableName = "KronosUserDetails");
        Task<IEnumerable<T>> ExecuteQueryUsingPointQueryAsyncFiltered<T>(string partitionKey, string rowKey, string tableName = null) where T : TableEntity, new();
    }
}