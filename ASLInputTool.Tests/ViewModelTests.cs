using ASL;
using ASL.Models;
using ASL.Models.Components;
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
        vm.BrokenMorale = "10";
        vm.BPV = "20";
        vm.Leadership = "-1";

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.False(vm.IsAdding);
        var unit = vm.Items[0].Item;
        Assert.Equal("Test Leader", unit.Name);
        Assert.True(unit.IsLeader);
        Assert.Equal(-1, unit.Leadership?.Leadership);
    }

    [Fact]
    public void SquadsViewModel_Save_AddsToCollection()
    {
        var vm = new SquadsViewModel();
        vm.Name = "Test Squad";
        vm.Firepower = "4";
        vm.Range = "6";
        vm.Morale = "7";
        vm.BrokenMorale = "8";
        vm.BPV = "15";
        vm.SelectedScale = InfantryScale.Squad;

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        var unit = vm.Items[0].Item;
        Assert.True(unit.IsSquad);
    }

    [Fact]
    public void HeroesViewModel_Save_AddsToCollection()
    {
        var vm = new HeroesViewModel();
        vm.Name = "Test Hero";
        vm.Firepower = "1";
        vm.Range = "4";
        vm.Morale = "9";
        vm.BrokenMorale = "10";
        vm.SelectedNationality = Nationality.German;

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        var hero = vm.Items[0].Item;
        Assert.Equal(10, hero.Infantry?.BrokenMorale);
        Assert.True(hero.IsHero);
    }

    [Fact]
    public void HeroesViewModel_JapaneseHero_BrokenMoraleIsZero()
    {
        var vm = new HeroesViewModel();
        vm.Name = "Japanese Hero";
        vm.Firepower = "1";
        vm.Range = "4";
        vm.Morale = "10";
        vm.SelectedNationality = Nationality.Japanese;

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        var hero = vm.Items[0].Item;
        Assert.Equal(0, hero.Infantry?.BrokenMorale);
    }

    [Fact]
    public void ScenariosViewModel_SaveScenario_AddsToCollectionAndResetsView()
    {
        var vm = new ScenariosViewModel();
        vm.IsAdding = true;
        vm.Name = "Test Scenario";
        vm.Reference = "REF-1";
        vm.Place = "Test Place";
        vm.SideAName = "Finns";
        vm.SideBName = "Soviets";

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.False(vm.IsAdding);
        var scenario = vm.Items[0].Item;
        Assert.Equal("Test Scenario", scenario.Name);
        Assert.Equal("REF-1", scenario.Reference);
        Assert.Equal("Test Place", scenario.Description.Place);
        Assert.Equal(2, scenario.ScenarioSides.Count);
        Assert.Equal("Finns", scenario.ScenarioSides[0].Name);
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
