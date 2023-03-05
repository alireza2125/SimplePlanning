using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SimplePlanning.Shared.Models;

public class ProjectModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Name { get; set; }
    public string? Description { get; set; }

    [JsonIgnore]
    public List<TaskModel> Tasks { get; } = new();
}
