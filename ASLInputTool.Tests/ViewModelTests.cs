using ASLInputTool.ViewModels;
using Xunit;
using System.Linq;

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
    public void LeadersViewModel_InitialState_IsCorrect()
    {
        var vm = new LeadersViewModel();
        Assert.Equal("Leaders", vm.DisplayName);
        Assert.False(vm.IsAdding);
        Assert.Empty(vm.Items);
    }

    [Fact]
    public void LeadersViewModel_AddAndCancelCommands_ToggleIsAdding()
    {
        var vm = new LeadersViewModel();
        
        vm.AddCommand.Execute(null);
        Assert.True(vm.IsAdding);

        vm.CancelCommand.Execute(null);
        Assert.False(vm.IsAdding);
    }

    [Fact]
    public void LeadersViewModel_Save_AddsToCollectionAndResetsView()
    {
        var vm = new LeadersViewModel();
        vm.IsAdding = true;
        vm.Name = "Test Leader";
        vm.Morale = "9";
        vm.Leadership = "-1";

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.False(vm.IsAdding);
        var leader = vm.Items[0];
        Assert.Equal("Test Leader", leader.Name);
        Assert.Equal(-1, leader.Leadership);
    }

    [Fact]
    public void SquadsViewModel_Save_AddsToCollection()
    {
        var vm = new SquadsViewModel();
        vm.Name = "Test Squad";
        vm.Firepower = "4";
        vm.Range = "6";
        vm.Morale = "7";
        vm.IsHalfSquad = false;

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.IsType<ASL.Counters.Squad>(vm.Items[0]);
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
    public void ViewModelBase_OnPropertyChanged_IsRaised()
    {
        var vm = new LeadersViewModel();
        string? changedPropertyName = null;
        vm.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

        vm.IsAdding = true;

        Assert.Equal(nameof(LeadersViewModel.IsAdding), changedPropertyName);
    }
}
