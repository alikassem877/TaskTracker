using System.ComponentModel.DataAnnotations;
using TaskTracker.Models;

namespace TaskApi.DTOs;

public record CreateTaskRequest(
    [Required]
    [StringLength(100)]
    string Title,

    Priority Priority
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!Enum.IsDefined(typeof(Priority), Priority))
        {
            yield return new ValidationResult(
                "Invalid priority value.",
                [nameof(Priority)]
            );
        }
    }
}