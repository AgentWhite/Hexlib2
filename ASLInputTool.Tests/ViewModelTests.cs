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
        Assert.False(vm.IsAdding);
        Assert.Empty(vm.Items);
    }

    [Fact]
    public void CountersViewModel_AddAndCancelCommands_ToggleIsAdding()
    {
        var vm = new CountersViewModel();
        
        vm.AddCommand.Execute(null);
        Assert.True(vm.IsAdding);

        vm.CancelCommand.Execute(null);
        Assert.False(vm.IsAdding);
    }

    [Fact]
    public void CountersViewModel_SaveCounter_AddsToCollectionAndResetsView()
    {
        var vm = new CountersViewModel();
        vm.IsAdding = true;
        vm.Name = "Test Leader";
        vm.IsLeader = true;
        vm.Leadership = "-1";

        vm.SaveCommand.Execute("SMC");

        Assert.Single(vm.Items);
        Assert.False(vm.IsAdding);
        var leader = Assert.IsType<ASL.Counters.Leader>(vm.Items[0]);
        Assert.Equal("Test Leader", leader.Name);
        Assert.Equal(-1, leader.Leadership);
    }

    [Fact]
    public void ScenariosViewModel_SaveScenario_AddsToCollectionAndResetsView()
    {
        var vm = new ScenariosViewModel();
        vm.IsAdding = true;
        vm.Name = "Test Scenario";
        vm.Reference = "REF-1";
        vm.Place = "Test Place";

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.False(vm.IsAdding);
        var scenario = vm.Items[0];
        Assert.Equal("Test Scenario", scenario.Name);
        Assert.Equal("REF-1", scenario.Reference);
        Assert.Equal("Test Place", scenario.Description.Place);
    }

    [Fact]
    public void ScenariosViewModel_SaveDuplicateScenario_DoesNotAddToCollection()
    {
        var vm = new ScenariosViewModel();
        vm.IsAdding = true;
        vm.Name = "Test Scenario";
        vm.Reference = "REF-1";
        vm.SaveCommand.Execute(null);
        Assert.Single(vm.Items);

        // Attempting to add with same name
        vm.IsAdding = true;
        vm.Name = "test scenario"; // different casing
        vm.Reference = "REF-2";
        vm.SaveCommand.Execute(null);
        Assert.Single(vm.Items);
        Assert.True(vm.IsAdding); // Still in adding view

        // Attempting to add with same reference
        vm.Name = "Other Name";
        vm.Reference = "ref-1"; // different casing
        vm.SaveCommand.Execute(null);
        Assert.Single(vm.Items);
        Assert.True(vm.IsAdding); // Still in adding view
    }

    [Fact]
    public void ViewModelBase_OnPropertyChanged_IsRaised()
    {
        var vm = new CountersViewModel();
        string? changedPropertyName = null;
        vm.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

        vm.IsAdding = true;

        Assert.Equal(nameof(CountersViewModel.IsAdding), changedPropertyName);
    }
}
