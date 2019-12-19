using Microsoft.Teams.App.KronosWfc.Configurator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Providers
{
    public class PaycodeMappingProvider : IPaycodeMappingProvider
    {
        private readonly string tableName = "KronosOvertimeMapping";

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

        /// <summary>
        /// Ensure table storage connection is initialized.
        /// </summary>
        /// <returns>A task.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }

        public PaycodeMappingProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(this.configuration["StorageConnectionString"]));
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

        public async Task<List<PaycodeMappingEntities>> RetrieveAllAsync()
        {
            await this.EnsureInitializedAsync();
            TableQuery<PaycodeMappingEntities> query = new TableQuery<PaycodeMappingEntities>();
            TableContinuationToken contToke = null;
            List<PaycodeMappingEntities> paycodes = new List<PaycodeMappingEntities>();
            do
            {
                var queryResult = await cloudTable.ExecuteQuerySegmentedAsync(query, contToke);
                paycodes.AddRange(queryResult.Results);

                 contToke = queryResult.ContinuationToken;
            }
            while (contToke != null);

            return paycodes;
        }

        /// <summary>
        /// Removes all paycode mappings for tenant.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task<bool> DeleteAllAsync()
        {
            await this.EnsureInitializedAsync();
            TableQuery<PaycodeMappingEntities> query = new TableQuery<PaycodeMappingEntities>();
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

        /// <summary>
        /// Adds paycode mapping for tenant in batch.
        /// </summary>
        /// <param name="rooms">List of favorite rooms.</param>
        /// <returns>A task.</returns>
        public async Task<bool> AddBatchAsync(List<PaycodeMappingEntities> paycodes)
        {
            await this.EnsureInitializedAsync();
            TableBatchOperation insertBatchOperation = new TableBatchOperation();
            TableOperation insertOperation;
            foreach (var paycode in paycodes)
            {
                insertOperation = TableOperation.InsertOrReplace(paycode);
                insertBatchOperation.Add(insertOperation);
            }

            await cloudTable.ExecuteBatchAsync(insertBatchOperation);
            return true;

        }
    }
}
