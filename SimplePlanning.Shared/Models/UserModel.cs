using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SimplePlanning.Shared.Models;

public class UserModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    [JsonIgnore]
    public List<TaskUserModel> TaskUsers { get; } = new();
}
