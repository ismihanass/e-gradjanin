using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace eGradjaninFBiH.Models;

public class ApplicationUser : IdentityUser { [MaxLength(120)] public string FullName { get; set; } = ""; }
public class AdministrativeService
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(80)] public string Category { get; set; } = "Opće";
    [MaxLength(1200)] public string Description { get; set; } = "";
    [MaxLength(1200)] public string RequiredDocuments { get; set; } = "";
    public decimal Fee { get; set; }
    public int EstimatedDays { get; set; } = 3;
    public bool IsActive { get; set; } = true;
}
public enum RequestStatus { Submitted, UnderReview, AdditionalInformationRequired, Approved, Rejected, Completed }
public class ServiceRequest
{
    public int Id { get; set; }
    [Required, MaxLength(24)] public string Reference { get; set; } = "";
    [Required] public string CitizenId { get; set; } = "";
    public ApplicationUser? Citizen { get; set; }
    public int AdministrativeServiceId { get; set; }
    public AdministrativeService? AdministrativeService { get; set; }
    [Required, MaxLength(1200)] public string ApplicantNote { get; set; } = "";
    public RequestStatus Status { get; set; } = RequestStatus.Submitted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(1000)] public string? RegistrarNote { get; set; }
    public string? RegistrarId { get; set; }
    public List<RequestAttachment> Attachments { get; set; } = [];
    public Payment? Payment { get; set; }
    public IssuedDocument? IssuedDocument { get; set; }
}
public class RequestAttachment { public int Id { get; set; } public int ServiceRequestId { get; set; } public ServiceRequest? ServiceRequest { get; set; } [MaxLength(260)] public string OriginalName { get; set; } = ""; [MaxLength(500)] public string StoredName { get; set; } = ""; public DateTime UploadedAt { get; set; } = DateTime.UtcNow; }
public class Payment { public int Id { get; set; } public int ServiceRequestId { get; set; } public ServiceRequest? ServiceRequest { get; set; } public decimal Amount { get; set; } public string Status { get; set; } = "Pending"; public string Method { get; set; } = "Demo kartica"; public DateTime? PaidAt { get; set; } public string TransactionReference { get; set; } = ""; }
public class IssuedDocument { public int Id { get; set; } public int ServiceRequestId { get; set; } public ServiceRequest? ServiceRequest { get; set; } [MaxLength(32)] public string DocumentReference { get; set; } = ""; [MaxLength(64)] public string VerificationToken { get; set; } = ""; public DateTime IssuedAt { get; set; } = DateTime.UtcNow; public bool IsDemo { get; set; } = true; }
public class UserNotification { public int Id { get; set; } public string UserId { get; set; } = ""; [MaxLength(240)] public string Message { get; set; } = ""; public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public bool IsRead { get; set; } }
public class AuditLog { public int Id { get; set; } public string? ActorId { get; set; } [MaxLength(80)] public string Action { get; set; } = ""; [MaxLength(1200)] public string Details { get; set; } = ""; public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
