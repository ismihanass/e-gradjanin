using System.ComponentModel.DataAnnotations;
namespace eGradjaninFBiH.ViewModels;
public class RequestCreateViewModel
{
    [Required] public int ServiceId { get; set; }
    [Required, StringLength(1200, MinimumLength=5)] public string ApplicantNote { get; set; } = "";
    public IFormFile? Attachment { get; set; }
}
public class StatusUpdateViewModel { public int Id { get; set; } public string Status { get; set; } = ""; public string? Note { get; set; } }
public class AiQuestionViewModel { [Required, StringLength(1000,MinimumLength=2)] public string Question { get; set; } = ""; }
