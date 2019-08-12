//-----------------------------------------------------------------------
// <copyright file="TenantMapEntity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Tenant map entity class
    /// </summary>
    public class TenantMapEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantMapEntity"/> class.
        /// Your entity type must expose a parameter-less constructor
        /// </summary>
        public TenantMapEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantMapEntity"/> class.
        /// Defines the PK and RK.
        /// </summary>
        /// <param name="channelId">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        public TenantMapEntity(string channelId, string rowKey)
        {
            this.PartitionKey = channelId;
            this.RowKey = rowKey;
        }

        /// <summary>
        /// Gets or sets EndpointUrl
        /// </summary>
        public string EndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets SuperUserName
        /// </summary>
        public string SuperUserName { get; set; }
        /// <summary>
        /// Gets or sets SuperUserPwd
        /// </summary>
        public string SuperUserPwd { get; set; }

        /// <summary>
        /// Gets or sets SuperUserPersonNumber
        /// </summary>
        public string SuperUserPersonNumber { get; set; }
    }
}
