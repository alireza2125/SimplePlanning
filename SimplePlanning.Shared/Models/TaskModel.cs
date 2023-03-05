using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SimplePlanning.Shared.Models;

public class TaskModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public TaskTypeStatus Status { get; set; }

    public required Guid ProjectId { get; set; }

    [JsonIgnore]
    public ProjectModel? Project { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }

    [JsonIgnore]
    public List<TaskUserModel> TaskUsers { get; } = new();
}
