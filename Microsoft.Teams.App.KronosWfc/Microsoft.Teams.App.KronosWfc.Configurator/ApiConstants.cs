//-----------------------------------------------------------------------
// <copyright file="ApiConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Configurator
{
    /// <summary>
    /// API Constants class
    /// </summary>
    public static class ApiConstants
    {
        public const string SoapEnvOpen = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:hs=""http://localhost/wfc/XMLAPISchema"" ><soapenv:Body><hs:KronosWFC><Kronos_WFC version = ""1.0"">";

        public const string SoapEnvClose = @"</Kronos_WFC></hs:KronosWFC></soapenv:Body></soapenv:Envelope>";

        public const string SoapAction = "http://localhost/wfc/XMLAPISchema";

        public const string System = "System";

        public const string LogonAction = "Logon";

        public const string Response = "Response";

        public const string Success = "Success";

        public const string Failure = "Failure";

        public const string LoadAction = "Load";
        public const string DateFormat = "MM/d/yyyy";

        public const string AddOnlyAction = "AddOnly";
        public const string WorkRuleAction = "LoadAllWorkRules";

        public const string AddRequests = "AddRequests";
        public const string ApproveRequests = "ApproveRequests";
        public const string RefuseRequests = "RefuseRequests";
        public const string SubmitRequests = "SubmitRequests";
        public const string RetrieveRequests = "RetrieveRequests";

        public const string LoadDailyTotals = "LoadDailyTotals";

        public const string RunQueryAction = "RunQuery";
        public const string AllHomeHyperFindQuery = "All Home";
        public const string PubilcVisibilityCode = "Public";
        public const string ReportsToHyperFindQuery = "Report";
        public const string PersonalVisibilityCode = "Private";
        public const string OvertimeHyperFindQuery = "Employees with Overtime";

        public const string UserNotLoggedInError = "1307";
        public const string UserUnauthorizedError = "1305";

        public const string LoadJobs = "LoadJobs";
        public const string RetrieveAllForUpdate = "RetrieveAllForUpdate";
        public const string RetrieveWithDetails = "RetrieveWithDetails";

        public const string LoadEligibleEmployees = "LoadEligibleEmployees";
        public const string UpdateStatus = "UpdateStatus";

        public const string SwapShiftAutoApproval = "SWAP_SHIFT_AUTOMATIC_APPROVAL";

        public const string SwapShift = "SWAP_SHIFT";
        public const string LoadActiveComments = "LoadActiveComments";
        public const string REQUESTS = "REQUESTS";
        public const string LoadAllPayCodes = "LoadAllPayCodes";
    }
}
