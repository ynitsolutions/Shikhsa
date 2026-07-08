using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class SchoolMaster:BaseEntity
{
    [Key]
    public int SchoolId { get; set; }

    public string SchoolName { get; set; }
    public string SchoolShortName { get; set; }
    public string? TagLine { get; set; } = string.Empty;
    public string? SchoolMotto { get; set; } = string.Empty;

    public string SchoolAddress { get; set; }
    public string State { get; set; }
    public string District { get; set; }
    public string? City { get; set; }=string.Empty;
    public string PinCode { get; set; }

    public string MobileContactNo { get; set; }
    public string LandlineNo { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }

    public string Board { get; set; }
    public string AffiliationNo { get; set; }
    public string RegistrationCode { get; set; }

    public int InitialClassId { get; set; }
    public int AcademicSessionStartMonth { get; set; }

    public string PrincipalName { get; set; }
    public string PrincipalMobileNo { get; set; }
    public string PrincipalEmail { get; set; }

    public string? ManagerName { get; set; }=string.Empty;
    public string ManagerMobileNo { get; set; } = string.Empty;

    public string UDISECode { get; set; } = string.Empty;
    public string PANNo { get; set; } = string.Empty;
    public string GSTNo { get; set; } = string.Empty;

    public int? EstablishedYear { get; set; }

    public string RecognitionText { get; set; } = string.Empty;
    public string ReportCardFooterText { get; set; } = string.Empty;
    public string TCFooterText { get; set; } = string.Empty;

    // public bool IsActive { get; set; } = true;

    public string LogoPath { get; set; }

    [NotMapped]
    public IFormFile SchoolLogo { get; set; }
}