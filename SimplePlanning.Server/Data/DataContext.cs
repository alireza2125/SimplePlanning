using Microsoft.EntityFrameworkCore;

using SimplePlanning.Shared.Models;

namespace SimplePlanning.Server.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<ProjectModel> Projects => Set<ProjectModel>();
    public DbSet<TaskModel> Tasks => Set<TaskModel>();
    public DbSet<TaskUserModel> TaskUsers => Set<TaskUserModel>();
}
