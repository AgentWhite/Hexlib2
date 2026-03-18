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
    public void CountersViewModel_SaveCounter_AddsToCollectionAndResetsView()
    {
        var vm = new CountersViewModel();
        vm.IsAddingCounter = true;
        vm.Name = "Test Leader";
        vm.IsLeader = true;
        vm.Leadership = "-1";

        vm.SaveCounterCommand.Execute("SMC");

        Assert.Single(vm.Counters);
        Assert.False(vm.IsAddingCounter);
        var leader = Assert.IsType<ASL.Counters.Leader>(vm.Counters[0]);
        Assert.Equal("Test Leader", leader.Name);
        Assert.Equal(-1, leader.Leadership);
    }

    [Fact]
    public void ScenariosViewModel_SaveScenario_AddsToCollectionAndResetsView()
    {
        var vm = new ScenariosViewModel();
        vm.IsAddingScenario = true;
        vm.Name = "Test Scenario";
        vm.Reference = "REF-1";
        vm.Place = "Test Place";

        vm.SaveScenarioCommand.Execute(null);

        Assert.Single(vm.Scenarios);
        Assert.False(vm.IsAddingScenario);
        var scenario = vm.Scenarios[0];
        Assert.Equal("Test Scenario", scenario.Name);
        Assert.Equal("REF-1", scenario.Reference);
        Assert.Equal("Test Place", scenario.Description.Place);
    }

    [Fact]
    public void ScenariosViewModel_SaveDuplicateScenario_DoesNotAddToCollection()
    {
        var vm = new ScenariosViewModel();
        vm.IsAddingScenario = true;
        vm.Name = "Test Scenario";
        vm.Reference = "REF-1";
        vm.SaveScenarioCommand.Execute(null);
        Assert.Single(vm.Scenarios);

        // Attempting to add with same name
        vm.IsAddingScenario = true;
        vm.Name = "test scenario"; // different casing
        vm.Reference = "REF-2";
        vm.SaveScenarioCommand.Execute(null);
        Assert.Single(vm.Scenarios);
        Assert.True(vm.IsAddingScenario); // Still in adding view

        // Attempting to add with same reference
        vm.Name = "Other Name";
        vm.Reference = "ref-1"; // different casing
        vm.SaveScenarioCommand.Execute(null);
        Assert.Single(vm.Scenarios);
        Assert.True(vm.IsAddingScenario); // Still in adding view
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
