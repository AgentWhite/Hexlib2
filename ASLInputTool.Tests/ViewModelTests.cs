using ASLInputTool.ViewModels;
using Xunit;

namespace ASLInputTool.Tests;

public class ViewModelTests
{
    [Fact]
    public void MainViewModel_InitialState_SetsCurrentView()
    {
        var vm = new MainViewModel();
        Assert.NotNull(vm.CurrentView);
        Assert.NotEmpty(vm.NavigationItems);
        Assert.Equal(vm.NavigationItems[0], vm.CurrentView);
    }

    [Fact]
    public void CountersViewModel_InitialState_IsCorrect()
    {
        var vm = new CountersViewModel();
        Assert.Equal("Counters", vm.DisplayName);
        Assert.False(vm.IsAddingCounter);
        Assert.Empty(vm.Counters);
    }

    [Fact]
    public void CountersViewModel_AddAndCancelCommands_ToggleIsAddingCounter()
    {
        var vm = new CountersViewModel();
        
        vm.AddCounterCommand.Execute(null);
        Assert.True(vm.IsAddingCounter);

        vm.CancelAddCommand.Execute(null);
        Assert.False(vm.IsAddingCounter);
    }

    [Fact]
    public void ViewModelBase_OnPropertyChanged_IsRaised()
    {
        var vm = new CountersViewModel();
        string? changedPropertyName = null;
        vm.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

        vm.IsAddingCounter = true;

        Assert.Equal(nameof(CountersViewModel.IsAddingCounter), changedPropertyName);
    }
}
