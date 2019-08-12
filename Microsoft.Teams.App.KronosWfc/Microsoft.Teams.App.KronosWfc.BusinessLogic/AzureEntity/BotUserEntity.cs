//-----------------------------------------------------------------------
// <copyright file="BotUserEntity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Bot User Entity Class
    /// </summary>
    public class BotUserEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotUserEntity"/> class.
        /// Your entity type must expose a parameter-less constructor
        /// </summary>
        public BotUserEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotUserEntity"/> class.
        /// Defines the PK and RK.
        /// </summary>
        /// <param name="channelId">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        public BotUserEntity(string channelId, string rowKey)
        {
            this.PartitionKey = channelId;
            this.RowKey = rowKey;
        }

        /// <summary>
        /// Gets or sets EndpointUrl
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets Teams User ID
        /// </summary>
        public string TeamsUserId { get; set; }

        /// <summary>
        /// Gets or sets Person Number
        /// </summary>
        public string PersonNumber
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets Conversation ID
        /// </summary>
        public string ConversationId { get; set; }
    }
}
