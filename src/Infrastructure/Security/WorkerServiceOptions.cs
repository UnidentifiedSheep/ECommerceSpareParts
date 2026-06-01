using System.ComponentModel.DataAnnotations;

namespace Security;

public class WorkerServiceOptions
{
    public const string SectionName = "WorkerService";
    [Required]
    public Guid SystemId { get; set; }
}