using Microsoft.Teams.App.KronosWfc.Configurator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Providers
{
    public class TenantMappingProvider : ITenantMappingProvider
    {
        private readonly string tableName = "TenantMappingTable";

        /// <summary>
        /// CloudTableClient object.
        /// </summary>
        private static CloudTableClient cloudTableClient;

        /// <summary>
        /// CloudTable object.
        /// </summary>
        private static CloudTable cloudTable;

        /// <summary>
        /// Task for initialization.
        /// </summary>
        private readonly Lazy<Task> initializeTask;

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        public TenantMappingProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(this.configuration["StorageConnectionString"]));
        }

        /// <summary>
        /// Ensure table storage connection is initialized.
        /// </summary>
        /// <returns>A task.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }

        /// <summary>
        /// Create tables if it doesnt exists.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            cloudTableClient = storageAccount.CreateCloudTableClient();
            cloudTable = cloudTableClient.GetTableReference(this.tableName);
            if (!await cloudTable.ExistsAsync())
            {
                await cloudTable.CreateIfNotExistsAsync();
            }
        }

        /// <summary>
        /// Get activity ids.
        /// </summary>
        /// <param name="aadObjectId">AadObjectId of user.</param>
        /// <param name="activityReferenceId">Unique GUID referencing to activity id.</param>
        /// <returns>A task.</returns>
        public async Task<List<TenantMappingEntities>> GetAsync()
        {
            await this.EnsureInitializedAsync();
            TableQuery<TenantMappingEntities> query = new TableQuery<TenantMappingEntities>();
            TableContinuationToken contToke = null;
            List<TenantMappingEntities> tenants = new List<TenantMappingEntities>();
            do
            {
                var queryResult = await cloudTable.ExecuteQuerySegmentedAsync(query, contToke);
                tenants.AddRange(queryResult.Results);
                contToke = queryResult.ContinuationToken;
            }
            while (contToke != null);

            return tenants;
        }

        public async Task<bool> SetAsync(TenantMappingEntities tenantMappingEntity)
        {
            await this.EnsureInitializedAsync();
            TableOperation insertOrMergeOperation = TableOperation.InsertOrReplace(tenantMappingEntity);
            TableResult result = await cloudTable.ExecuteAsync(insertOrMergeOperation);
            return true;
        }

        /// <summary>
        /// Removes tenant mapping from storage.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task<bool> DeleteAllAsync()
        {
            await this.EnsureInitializedAsync();
            TableQuery<TenantMappingEntities> query = new TableQuery<TenantMappingEntities>();
            var queryResult = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
            TableBatchOperation deleteBatch = new TableBatchOperation();
            foreach (var item in queryResult.Results)
            {
                TableOperation deleteOperation = TableOperation.Delete(item);
                deleteBatch.Add(deleteOperation);
            }

            if (deleteBatch.Count > 0)
            {
                await cloudTable.ExecuteBatchAsync(deleteBatch);
            }

            return true;
        }
    }
}
