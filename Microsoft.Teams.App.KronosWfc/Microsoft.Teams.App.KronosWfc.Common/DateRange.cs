//-----------------------------------------------------------------------
// <copyright file="DateRange.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Daterange class
    /// </summary>
    public class DateRange
    {
        [Required(ErrorMessage = "Start date value must be a valid date")]
        [Date()]
        public string StartDate { get; set; }
        [Required(ErrorMessage = "End date value must be a valid date")]
        public string EndDate { get; set; }

        public static DateRange Parse(dynamic dateRange)
        {
            return new DateRange
            {
                StartDate = Convert.ToString(dateRange.StartDate),
                EndDate = Convert.ToString(dateRange.EndDate)
            };
        }
    }

    /// <summary>
    /// Date Attributes
    /// </summary>
    public class DateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance as DateRange;
            var startDate = Convert.ToDateTime(model.StartDate);
            var endDate = Convert.ToDateTime(model.EndDate);
            var dateDiff = (endDate - startDate).Days;

            if (model != null)
            {
                if (Convert.ToDateTime(model.StartDate) > Convert.ToDateTime(model.EndDate))
                {
                    return new ValidationResult(Constants.StartDateGreaterThanEndDateError);
                }
                else if (dateDiff > 14)
                {
                    return new ValidationResult(Constants.DateDifferenceError);
                }
            }

            return ValidationResult.Success;
        }
    }
}
