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
    public ICollection<SchoolEmailSetting> EmailSettings { get; set; } = new List<SchoolEmailSetting>();
}

public class SchoolEmailSetting : BaseEntity
{
    [Key]
    public long EmailSettingId { get; set; }

    // FK -> SchoolMaster
    public int SchoolId { get; set; }

    [ForeignKey(nameof(SchoolId))]
    public SchoolMaster? School { get; set; }

    [Required, MaxLength(50)]
    public string ProviderName { get; set; } = string.Empty;   // Zoho / Gmail ...

    // Lower number = used first (1 = primary, 2 = fallback ...)
    public int Priority { get; set; } = 1;

    [Required, MaxLength(200)]
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    [Required, MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    // Stored ENCRYPTED (see controller below). Never render it back to the page.
    [Required]
    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    [Required, MaxLength(200)]
    public string FromEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FromName { get; set; } = string.Empty;

    // Named IsEmailActive so it does not clash with BaseEntity.IsActive
    public bool IsEmailActive { get; set; } = true;
}
public class AdminCredentialVm
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Password { get; set; }
    public bool Exists { get; set; }
}