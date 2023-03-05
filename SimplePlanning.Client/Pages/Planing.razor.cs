using MudBlazor;

using SimplePlanning.Shared.Models;

namespace SimplePlanning.Client.Pages;

public partial class Planing : IDisposable
{
    public void Dispose()
    {
        ViewModel.OnChange -= ViewModel_OnChange;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.OnChange += ViewModel_OnChange;
    }

    protected override async Task OnInitializedAsync() => await ViewModel.LoadAsync().ConfigureAwait(false);

    private void ViewModel_OnChange(object? sender, EventArgs e) => InvokeAsync(StateHasChanged);

    private async Task DropItemAsync(MudItemDropInfo<TaskModel> dropItem)
    {
        await ViewModel.UpdateTask(dropItem.Item,
                options =>
                    options.Status = Enum.Parse<TaskTypeStatus>(dropItem.DropzoneIdentifier))
            .ConfigureAwait(false);
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
}
