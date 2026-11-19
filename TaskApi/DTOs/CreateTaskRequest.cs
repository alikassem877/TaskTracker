using System.ComponentModel.DataAnnotations;
using TaskTracker.Models;

namespace TaskApi.DTOs;

public record CreateTaskRequest(
    [Required]
    [StringLength(100)]
    string Title,

    Priority Priority
);