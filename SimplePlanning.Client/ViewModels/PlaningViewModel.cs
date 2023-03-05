using System.Collections.ObjectModel;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;

using MudBlazor;

using SimplePlanning.Client.Data;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Client.ViewModels;

public class PlaningViewModel
{
    private readonly DataContext _dataContext;
    private readonly HttpClient _httpClient;
    private readonly ISnackbar _snackbar;

    public event EventHandler? OnChange;

    public IEnumerable<TaskTypeStatus> TaskTypeStatuses { get; } = Enum.GetValues<TaskTypeStatus>();

    public ObservableCollection<ProjectModel> Projects { get; }
    public ObservableCollection<TaskModel> Tasks { get; }
    public ObservableCollection<TaskUserModel> TaskUsers { get; }
    public ObservableCollection<UserModel> Users { get; }

    public PlaningViewModel(DataContext dataContext, HttpClient httpClient, ISnackbar snackbar)
    {
        _dataContext = dataContext;
        _httpClient = httpClient;
        _snackbar = snackbar;
        Projects = _dataContext.Projects.Local.ToObservableCollection();
        Tasks = _dataContext.Tasks.Local.ToObservableCollection();
        TaskUsers = _dataContext.TaskUsers.Local.ToObservableCollection();
        Users = _dataContext.Users.Local.ToObservableCollection();

        Projects.CollectionChanged += Projects_CollectionChanged;
        Tasks.CollectionChanged += Tasks_CollectionChanged;
        TaskUsers.CollectionChanged += TaskUsers_CollectionChanged;
        Users.CollectionChanged += Users_CollectionChanged;
    }

    ~PlaningViewModel()
    {
        Projects.CollectionChanged -= Projects_CollectionChanged;
        Tasks.CollectionChanged -= Tasks_CollectionChanged;
        TaskUsers.CollectionChanged -= TaskUsers_CollectionChanged;
        Users.CollectionChanged -= Users_CollectionChanged;
    }

    private void Projects_CollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => OnChange?.Invoke(this, new());

    private void Tasks_CollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => OnChange?.Invoke(this, new());

    private void TaskUsers_CollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => OnChange?.Invoke(this, new());

    private void Users_CollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => OnChange?.Invoke(this, new());

    // ReSharper disable once CognitiveComplexity
    public async ValueTask LoadAsync()
    {
        var projects =
            await _httpClient.GetFromJsonAsync<IEnumerable<ProjectModel>>("/api/projects").ConfigureAwait(false);
        if (projects is null)
        {
            return;
        }

        _dataContext.RemoveRange(_dataContext.TaskUsers);
        _dataContext.RemoveRange(_dataContext.Tasks);
        _dataContext.RemoveRange(_dataContext.Projects);

        foreach (var project in projects)
        {
            await _dataContext.AddAsync(project).ConfigureAwait(false);

            var tasks = await _httpClient
                .GetFromJsonAsync<IEnumerable<TaskModel>>($"/api/tasks?projectId={project.Id}")
                .ConfigureAwait(false);
            if (tasks is null)
            {
                return;
            }

            foreach (var task in tasks)
            {
                project.Tasks.Add(task);

                var taskUsers = await _httpClient
                    .GetFromJsonAsync<IEnumerable<TaskUserModel>>($"/api/taskUsers/{task.Id}")
                    .ConfigureAwait(false);
                if (taskUsers is null)
                {
                    return;
                }

                foreach (var taskUser in taskUsers)
                {
                    task.TaskUsers.Add(taskUser);

                    if (await _dataContext.Users.AnyAsync(x => x.Id == taskUser.UserId).ConfigureAwait(false))
                    {
                        continue;
                    }

                    var user = await _httpClient.GetFromJsonAsync<UserModel>($"/api/Users/{taskUser.UserId}")
                        .ConfigureAwait(false);
                    if (user is null)
                    {
                        return;
                    }

                    await _dataContext.AddAsync(user).ConfigureAwait(false);
                }
            }
        }

        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async ValueTask AddProject(ProjectModel projectModel)
    {
        using var response =
            await _httpClient.PostAsJsonAsync("/api/projects", projectModel).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to add project", Severity.Error);
            return;
        }

        var id = await response.Content.ReadFromJsonAsync<Guid?>().ConfigureAwait(false);
        if (!id.HasValue)
        {
            _snackbar.Add("Failed to add project", Severity.Error);
            return;
        }

        projectModel.Id = id.Value;
        await _dataContext.AddAsync(projectModel).ConfigureAwait(false);
        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Add project successful", Severity.Success);
    }

    public async ValueTask UpdateProject(ProjectModel projectModel, Action<ProjectModel>? action = default)
    {
        action?.Invoke(projectModel);
        using var response =
            await _httpClient.PutAsJsonAsync("/api/projects", projectModel).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to update project", Severity.Error);
            return;
        }

        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Update project successful", Severity.Success);
    }

    public async ValueTask RemoveProject(ProjectModel projectModel)
    {
        using var response =
            await _httpClient.DeleteAsync($"/api/projects/{projectModel.Id}").ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to remove project", Severity.Error);
            return;
        }

        _dataContext.Remove(projectModel);
        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Remove project successful", Severity.Success);
    }

    public async ValueTask AddTask(TaskModel taskModel)
    {
        using var response =
            await _httpClient.PostAsJsonAsync("/api/tasks", taskModel).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to add task", Severity.Error);
            return;
        }

        var id = await response.Content.ReadFromJsonAsync<Guid?>().ConfigureAwait(false);
        if (!id.HasValue)
        {
            _snackbar.Add("Failed to add task", Severity.Error);
            return;
        }

        taskModel.Id = id.Value;
        await _dataContext.AddAsync(taskModel).ConfigureAwait(false);
        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Add task successful", Severity.Success);
    }

    public async ValueTask UpdateTask(TaskModel taskModel, Action<TaskModel>? action = default)
    {
        action?.Invoke(taskModel);
        using var response =
            await _httpClient.PutAsJsonAsync("/api/tasks", taskModel).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to update task", Severity.Error);
            return;
        }

        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Update v successful", Severity.Success);
    }

    public async ValueTask RemoveTask(TaskModel taskModel)
    {
        using var response =
            await _httpClient.DeleteAsync($"/api/tasks/{taskModel.Id}").ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to remove task", Severity.Error);
            return;
        }

        _dataContext.Remove(taskModel);
        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Remove task successful", Severity.Success);
    }

    public async ValueTask AddUserTask(TaskUserModel taskUserModel)
    {
        using var response =
            await _httpClient.PostAsJsonAsync("/api/taskUsers/add", taskUserModel).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to add user to task", Severity.Error);
            return;
        }

        await _dataContext.AddAsync(taskUserModel).ConfigureAwait(false);
        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Add user to task successful", Severity.Success);
    }

    public async ValueTask RemoveUserTask(TaskUserModel taskUserModel)
    {
        using var response =
            await _httpClient.PostAsJsonAsync("/api/taskUsers/delete", taskUserModel).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Failed to remove user from task", Severity.Error);
            return;
        }

        _dataContext.Remove(taskUserModel);
        await _dataContext.SaveChangesAsync().ConfigureAwait(false);
        _snackbar.Add("Remove user from task successful", Severity.Success);
    }

    public async ValueTask<UserModel?> GetUserAsync(string email) => await _httpClient
        .GetFromJsonAsync<UserModel>($"/api/Users/{email}")
        .ConfigureAwait(false);

    public async ValueTask<IEnumerable<UserModel>> FindUserAsync(string email,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetFromJsonAsync<IEnumerable<UserModel>>($"/api/Users/find?email={email}", cancellationToken)
            .ConfigureAwait(false);
        return response ?? Enumerable.Empty<UserModel>();
    }
}
