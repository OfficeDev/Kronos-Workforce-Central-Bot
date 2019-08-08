//-----------------------------------------------------------------------
// <copyright file="CarouselVacationBalance.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.CarouselCards
{
    /// <summary>
    /// Enum used in the vacation balance card.
    /// </summary>
    public partial class CarouselVacationBalance
    {
        /// <summary>
        /// AccrualType enum.
        /// </summary>
        private enum AccrualType
        {
            /// <summary>
            /// Hours enum
            /// </summary>
            Hours = 1,

            /// <summary>
            /// Days enum
            /// </summary>
            Days = 2,

            /// <summary>
            /// Currency enum
            /// </summary>
            Currency = 3,
        }
    }
}