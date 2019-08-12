//-----------------------------------------------------------------------
// <copyright file="AzureTableStorageHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Azure table storage helper class.
    /// </summary>
    [Serializable]
    public class AzureTableStorageHelper : IAzureTableStorageHelper
    {
        AppSettings appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageHelper" /> class
        /// </summary>
        public AzureTableStorageHelper()
        {
            this.appSettings = AppSettings.Instance;
        }

        /// <summary>
        /// Gets cloud table client.
        /// </summary>
        public CloudTableClient TableClient { get => this.StorageAccount.CreateCloudTableClient(); }

        /// <summary>
        /// field to access storage account.
        /// </summary>
        public CloudStorageAccount StorageAccount { get => CloudStorageAccount.Parse(this.appSettings.AzureTableStorageConnectionString); }

        /// <summary>
        /// Execute azure storage table operation.
        /// </summary>
        /// <param name="tableName">name of table.</param>
        /// <param name="tableOperation">Operation tobe performed on table.</param>
        /// <returns>returns the cloud TableResult.</returns>
        public async Task<TableResult> Execute(string storageTableName, TableOperation tableOperation)
        {
            TableResult result = new TableResult();
            try
            {
                CloudTable table = this.TableClient.GetTableReference(storageTableName);
                if (!table.Exists())
                {
                    await table.CreateIfNotExistsAsync();
                }

                result = await table.ExecuteAsync(tableOperation);
            }
            catch (Exception ex)
            {
                AppInsightsLogger.Error(ex);
            }

            return result;
        }

        /// <summary>
        /// Execute asynchronously the most efficient storage query - the point query - where both partition key and row key are specified.
        /// </summary>
        /// <param name="partitionKey">partition key of the table.</param>
        /// <param name="rowKey">row key of the table.</param>
        /// <returns>A Task of boolean indicating whether the entity was found or not.</returns>
        public async Task<T> ExecuteQueryUsingPointQueryAsync<T>(string partitionKey, string rowKey, string tableName = null) where T : TableEntity
        {
            var entity = default(T);
            CloudTable table;
            try
            {
                if (tableName == null)
                {
                    table = this.TableClient.GetTableReference(this.appSettings.TableName);
                }
                else
                {
                    table = this.TableClient.GetTableReference(tableName);
                }

                TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                entity = (T)result?.Result;
            }
            catch (StorageException)
            {

            }

            return entity;
        }

        public async Task<string> Retrive<T>(string personNumber, string tenantId, string channelId, string tableName = "KronosUserDetails")
        {
            try
            {
                CloudTable table = this.TableClient.GetTableReference(tableName);
                string partitionKeyQ = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, channelId);
                string kronosPersonNumberQ = TableQuery.GenerateFilterCondition("PersonNumber", QueryComparisons.Equal, personNumber);
                string tenantIDQ = TableQuery.GenerateFilterCondition("TenantId", QueryComparisons.Equal, tenantId);
                string combinedFilter = TableQuery.CombineFilters(partitionKeyQ, TableOperators.And, TableQuery.CombineFilters(tenantIDQ, TableOperators.And, kronosPersonNumberQ));

                string convId = null;
                TableQuery query = new TableQuery().Where(combinedFilter);

                TableContinuationToken token = null;

                var result = await table.ExecuteQuerySegmentedAsync(query, token);

                List<T> userList = new List<T>();

                if (result.Results.Count > 0)
                {
                    convId = result.Results.OrderByDescending(x => x.Timestamp).FirstOrDefault().Properties["ConversationId"].StringValue;
                }

                return convId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> RetriveAndDelete<T>(string personNumber, string tenantId, string channelId, string teamsUserId, string tableName = "KronosUserDetails")
        {
            try
            {
                CloudTable table = this.TableClient.GetTableReference(tableName);
                string partitionKeyQ = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, channelId);
                string kronosPersonNumberQ = TableQuery.GenerateFilterCondition("PersonNumber", QueryComparisons.Equal, personNumber);
                string tenantIDQ = TableQuery.GenerateFilterCondition("TenantId", QueryComparisons.Equal, tenantId);
                string teamsUserIdQ = TableQuery.GenerateFilterCondition("TeamsUserId", QueryComparisons.Equal, teamsUserId);
                string combinedFilter = TableQuery.CombineFilters(partitionKeyQ, TableOperators.And, TableQuery.CombineFilters(tenantIDQ, TableOperators.And, TableQuery.CombineFilters(kronosPersonNumberQ, TableOperators.And, teamsUserIdQ)));
                TableQuery query = new TableQuery().Where(combinedFilter);

                TableContinuationToken token = null;

                var result = await table.ExecuteQuerySegmentedAsync(query, token);
                if (result?.Results.Count() > 0)
                {
                    TableOperation deleteOperation = TableOperation.Delete(result.Results[0]);
                    table.Execute(deleteOperation);

                }
                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Insert or merge the activity id.
        /// </summary>
        /// <param name="entity">table entity.</param>
        /// <returns>table entity inserted or updated.</returns>
        public async Task<T> InsertOrMergeTableEntityAsync<T>(T entity, string activityTableName) where T : TableEntity
        {
            var activityEntity = default(T);

            if (entity == null)
            {
                throw new ArgumentNullException("null table entity");
            }

            try
            {
                CloudTable table = this.TableClient.GetTableReference(activityTableName);
                if (!table.Exists())
                    await table.CreateIfNotExistsAsync();

                TableOperation insertOrMergeOperation = TableOperation.InsertOrReplace(entity);

                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                activityEntity = result.Result as T;
            }
            catch (StorageException ex)
            {
                AppInsightsLogger.Error(ex);
            }

            return activityEntity;
        }

        /// <summary>
        /// Execute point query with filter option.
        /// </summary>
        /// <param name="partitionKey">Partition key of the table.</param>
        /// <param name="rowKey">Row key of the table.</param>
        /// /// <param name="tableName">Table name.</param>
        /// <returns>Ienumerable genric type.</returns>
        public async Task<IEnumerable<T>> ExecuteQueryUsingPointQueryAsyncFiltered<T>(string partitionKey, string rowKey, string tableName = null) where T : TableEntity, new()
        {
            CloudTable table;
            try
            {
                if (tableName == null)
                {
                    table = this.TableClient.GetTableReference(this.appSettings.TableName);
                }
                else
                {
                    table = this.TableClient.GetTableReference(tableName);
                }

                partitionKey = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
                rowKey = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKey);
                string combinedFilter = TableQuery.CombineFilters(partitionKey, TableOperators.And, rowKey);

                TableQuery<T> query = new TableQuery<T>().Where(combinedFilter);

                TableContinuationToken token = null;

                var segment = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                return segment?.Results;
            }
            catch (StorageException ex)
            {
                AppInsightsLogger.Error(ex.ToString());
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Get list of paycodes for time off request cards.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <returns>List of dynamic table entities.</returns>
        public async Task<List<DynamicTableEntity>> GetTimeOffPaycodes(string tableName)
        {
            try
            {
                CloudTable table = this.TableClient.GetTableReference(tableName);
                string sickQ = TableQuery.GenerateFilterCondition("PayCodeType", QueryComparisons.Equal, Constants.SickAZTS);
                string vacationQ = TableQuery.GenerateFilterCondition("PayCodeType", QueryComparisons.Equal, Constants.VacationAZTS);
                string personalQ = TableQuery.GenerateFilterCondition("PayCodeType", QueryComparisons.Equal, Constants.PersonalAZTS);
                string combinedFilter = TableQuery.CombineFilters(sickQ, TableOperators.Or, TableQuery.CombineFilters(vacationQ, TableOperators.Or, personalQ));
                TableQuery query = new TableQuery().Where(combinedFilter);
                TableContinuationToken token = null;
                var result = await table.ExecuteQuerySegmentedAsync(query, token);
                return result.Results;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
