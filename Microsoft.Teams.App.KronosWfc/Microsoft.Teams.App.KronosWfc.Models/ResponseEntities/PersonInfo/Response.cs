using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PersonInfo
{

    using System.Xml.Serialization;

    [XmlRoot]
    public class Response
    {
        [XmlElement("PersonInformation")]
        public PersonInformation PersonInformation { get; set; }

        [XmlAttribute]
        public string Status { get; set; }

        [XmlAttribute]
        public string Action { get; set; }

        [XmlIgnore]
        public string Jsession { get; set; }

        public Error Error { get; set; }
    }



    [Serializable]

    public class PersonInformation
    {
        [XmlElement("PersonAccessAssignments")]
        public PersonAccessAssignments PersonAccessAssign { get; set; }

        [XmlElement("AccessAssignmentData")]

        public AccessAssignmentData AccessAssignmentDat { get; set; }

        [XmlElement("HomeAccounts")]
        public HomeAccounts HomeAcc { get; set; }

        [XmlElement("PersonLicenseTypes")]

        public PersonLicenseTypes PersonLicenseTypes { get; set; }

        [XmlElement("Identity")]
        public Identity Identity { get; set; }

        [XmlElement("PersonAuthenticationTypes")]
        public PersonAuthenticationTypes PersonAuthenticationTyp { get; set; }

        [XmlElement("PersonData")]
        public PersonData PersonDat { get; set; }


        [XmlElement("UserAccountStatusList")]

        public UserAccountStatusList UserAccountStatusLis { get; set; }

        [XmlElement("CurrencyAssignment")]

        public CurrencyAssignment CurrencyAssignm { get; set; }

        [XmlElement("KPassAccountAssignment")]

        public KPassAccountAssignment KPassAccountAssignm { get; set; }

        [XmlElement("EmploymentStatusList")]

        public EmploymentStatusList EmploymentStatusLis { get; set; }

        [XmlElement("SupervisorDataSupervisor")]
        public SupervisorDataSupervisor SupervisorDataSupervisor { get; set; }
    }

    [Serializable]
    public class Identity
    {
        [XmlElement("PersonIdentity")]
        public PersonIdentity PersonID { get; set; }
    }
    [Serializable]
    public class PersonIdentity
    {
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
    [Serializable]
    public class AccessAssignmentData
    {
        [XmlElement("AccessAssignment")]
        public AccessAssignment AccessAssignments { get; set; }
        public EffectiveDatedTimeEntryMethods effectiveDatedTimeEntryMethods { get; set; }
    }
    [Serializable]
    public class EffectiveDatedTimeEntryMethods
    {
    }
    [Serializable]
    public class AccessAssignment
    {
        [XmlAttribute]
        public string AccessProfileName { get; set; }

        [XmlAttribute]
        public string GroupScheduleName { get; set; }

        [XmlAttribute]
        public string ManagerAccessSetName { get; set; }

        [XmlAttribute]
        public string ManagerPayCodeName { get; set; }

        [XmlAttribute]
        public string ManagerViewPayCodeName { get; set; }

        [XmlAttribute]
        public string ManagerTransferSetName { get; set; }

        [XmlAttribute]
        public string ManagerApprovalSetName { get; set; }

        [XmlAttribute]
        public string ManagerWorkRuleName { get; set; }

        [XmlAttribute]
        public string PreferenceProfileName { get; set; }

        [XmlAttribute]
        public string ProfessionalPayCodeName { get; set; }

        [XmlAttribute]
        public string ProfessionalTransferSetName { get; set; }

        [XmlAttribute]
        public string ProfessionalWorkRuleName { get; set; }

        [XmlAttribute]
        public string ReportName { get; set; }

        [XmlAttribute]
        public string SchedulePatternName { get; set; }

        [XmlAttribute]
        public string ShiftCodeName { get; set; }

        [XmlAttribute]
        public string AvailabilityPatternName { get; set; }

        [XmlAttribute]
        public string TransferEmployeeFlag { get; set; }

        [XmlAttribute]
        public string DelegateProfileName { get; set; }

        [XmlAttribute]
        public string ApproveOvertimeFlag { get; set; }

        [XmlAttribute]
        public string HyperFindScheduleVisibilityName { get; set; }

        [XmlAttribute]

        public string KnownPlaceProfileName { get; set; }

        [XmlAttribute]
        public string AccessMethodProfileName { get; set; }

        [XmlAttribute]
        public string NotificationProfileName { get; set; }
    }
    [Serializable]
    public class PersonAccessAssignments
    {
        [XmlElement("PersonAccessAssignment")]
        public PersonAccessAssignment[] PersonAccesses { get; set; }

    }
    [Serializable]
    public class PersonAccessAssignment
    {
        [XmlAttribute]
        public string EffectiveDate { get; set; }
        [XmlAttribute]
        public string ExpirationDate { get; set; }
        [XmlAttribute]
        public string ManagerAccessOrganizationSetName { get; set; }
        [XmlAttribute]
        public string ManagerTransferOrganizationSetName { get; set; }
        [XmlAttribute]
        public string ProfessionalTransferOrganizationSetName { get; set; }



    }
    [Serializable]
    public class EmploymentStatusList
    {
        [XmlElement("EmploymentStatus")]
        public EmploymentStatus[] EmploymentStatuses { get; set; }
    }
    [Serializable]
    public class EmploymentStatus
    {
        [XmlAttribute]
        public string EffectiveDate { get; set; }
        [XmlAttribute]
        public string ExpirationDate { get; set; }
        [XmlAttribute]
        public string EmploymentStatusName { get; set; }
    }
    [Serializable]
    public class HomeAccounts
    {
        [XmlElement("HomeAccount")]
        public HomeAccount[] HomeAccountList { get; set; }
    }
    [Serializable]
    public class HomeAccount
    {
        [XmlAttribute]
        public string EffectiveDate { get; set; }
        [XmlAttribute]
        public string ExpirationDate { get; set; }
        [XmlAttribute]
        public string LaborAccountName { get; set; }
    }

    [Serializable]
    public class PersonLicenseTypes
    {
        [XmlElement("PersonLicenseType")]
        public List<PersonLicenseType> PersonLicenseType { get; set; }
    }
    
    [Serializable]
    public class PersonLicenseType
    {
        [XmlAttribute]
        public string ActiveFlag { get; set; }


        [XmlAttribute]
        public string LicenseTypeName { get; set; }
    }
    [Serializable]
    public class PersonAuthenticationTypes
    {
        [XmlElement("PersonAuthenticationType")]
        public PersonAuthenticationType[] PersonAuthenticationTypeList { get; set; }
    }
    [Serializable]
    public class PersonAuthenticationType
    {
        [XmlAttribute]
        public string ActiveFlag { get; set; }
        [XmlAttribute]
        public string AuthenticationTypeName { get; set; }
    }
    [Serializable]
    public class PersonData
    {
        [XmlElement("Person")]
        public Person PersonDetail { get; set; }
    }
    [Serializable]
    public class Person
    {
        [XmlAttribute]
        public string FullName { get; set; }

        [XmlAttribute]
        public string HireDate { get; set; }

        [XmlAttribute]
        public string LastName { get; set; }

        [XmlAttribute]
        public string PersonNumber { get; set; }

        [XmlAttribute]
        public string FullTimePercentage { get; set; }

        [XmlAttribute]
        public string ManagerSignoffThruDateTime { get; set; }

        [XmlAttribute]
        public string PayrollLockoutThruDateTime { get; set; }

        [XmlAttribute]
        public string FingerRequiredFlag { get; set; }

        [XmlAttribute]
        public string BaseWageHourly { get; set; }

        [XmlAttribute]
        public string HasKmailNotificationDelivery { get; set; }

        [XmlElement("FullTimeEquivalencies")]
        public FullTimeEquivalencies FullTimeEquivalenciesList { get; set; }

    }

    [Serializable]
    public class FullTimeEquivalencies
    {
        [XmlElement("FullTimeEquivalency")]
        public FullTimeEquivalency[] FullTimeEquivalenc { get; set; }
    }
    [Serializable]
    public class FullTimeEquivalency
    {
        [XmlAttribute]
        public string FullTimePercentage { get; set; }
        [XmlAttribute]
        public string EffectiveDate { get; set; }
    }
    [Serializable]
    public class UserAccountStatusList
    {
        [XmlElement("UserAccountStatus")]
        public UserAccountStatus[] UserAccountStatuses { get; set; }
    }
    [Serializable]
    public class UserAccountStatus
    {
        [XmlAttribute]
        public string EffectiveDate { get; set; }
        [XmlAttribute]
        public string ExpirationDate { get; set; }
        [XmlAttribute]
        public string UserAccountStatusName { get; set; }

    }
    [Serializable]
    public class CurrencyAssignment
    {
        [XmlElement("EmployeeCurrencyAssignment")]
        public EmployeeCurrencyAssignment EmployeeCurrencyAssign { get; set; }
    }
    [Serializable]
    public class EmployeeCurrencyAssignment
    {
        [XmlAttribute]
        public string CurrencyCode { get; set; }

        [XmlAttribute]
        public string CurrencyLocale { get; set; }
    }
    [Serializable]
    public class KPassAccountAssignment
    {
        [XmlElement("KPassAccount")]
        public KPassAccount KPassAcc { get; set; }

    }
    [Serializable]
    public class KPassAccount
    {
        [XmlAttribute]
        public string LearningPath { get; set; }

        [XmlAttribute]
        public string isRestrictedUser { get; set; }
    }

    public class SupervisorDataSupervisor
    {
       public Supervisor Supervisor { get; set; }  
    }

    public class Supervisor {
        [XmlAttribute]
        public string FullName { get; set; }

        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
}
