using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SimplePlanning.Shared.Models;

[EntityTypeConfiguration(typeof(TaskUserModelConfiguration))]
public class TaskUserModel
{
    public Guid TaskId { get; set; }

    [JsonIgnore]
    public TaskModel? Task { get; set; }

    public Guid UserId { get; set; }

    [JsonIgnore]
    public UserModel? User { get; set; }
}

public class TaskUserModelConfiguration : IEntityTypeConfiguration<TaskUserModel>
{
    public void Configure(EntityTypeBuilder<TaskUserModel> builder) => builder.HasKey(x => new
    {
        x.TaskId, x.UserId
    });
}
