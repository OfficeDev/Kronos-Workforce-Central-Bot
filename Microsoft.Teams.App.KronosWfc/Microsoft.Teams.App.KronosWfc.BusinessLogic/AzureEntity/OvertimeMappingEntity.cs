//-----------------------------------------------------------------------
// <copyright file="OvertimeMappingEntity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Overtime mapping entity class
    /// </summary>
    public class OvertimeMappingEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OvertimeMappingEntity"/> class.
        /// Your entity type must expose a parameter-less constructor
        /// </summary>
        public OvertimeMappingEntity()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OvertimeMappingEntity"/> class.
        /// Defines the Primary key and Row key.
        /// </summary>
        /// <param name="channelId">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        public OvertimeMappingEntity(string channelId, string rowKey)
        {
            PartitionKey = channelId;
            RowKey = rowKey;
        }

        /// <summary>
        /// Gets or sets PayCodeName
        /// </summary>
        public string PayCodeName { get; set; }

        /// <summary>
        /// Gets or sets PayCodeType
        /// </summary>
        public string PayCodeType { get; set; }
    }
}
