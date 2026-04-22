using ASL.Infrastructure;
using ASL.Models;
using ASL.Models.Board;
using ASL.Models.Components;
using ASL.Models.Equipment;
using ASL.Models.Modules;
using ASL.Models.Scenarios;
using ASL.Models.Units;
using ASL.Services;
using ASLInputTool.ViewModels;
using Xunit;
using System.Linq;

namespace ASLInputTool.Tests;
using ASLInputTool.Infrastructure;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class MockScenarioRepository : IScenarioRepository
{
    public ObservableCollection<Scenario> AllScenarios { get; } = new();
    public ObservableCollection<Insignia> AllInsignias { get; } = new();
    public void Initialize(IEnumerable<Scenario> scenarios) { }
    public void ProcessData(string path) { }
    public void Add(Scenario s) => AllScenarios.Add(s);
    public void Remove(Scenario s) => AllScenarios.Remove(s);
    public void Clear() => AllScenarios.Clear();
    public Task SaveToDiskAsync(Scenario s, string? n) => Task.CompletedTask;
    public Task<IEnumerable<Scenario>> ScanAndLoadAsync() => Task.FromResult<IEnumerable<Scenario>>(AllScenarios);
    public Task DeleteFromDiskAsync(Scenario s) => Task.CompletedTask;
    public Task LoadInsigniasAsync() => Task.CompletedTask;
    public Task SaveInsigniaAsync(Insignia i) => Task.CompletedTask;
}

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
        var repository = new ASLInputTool.Infrastructure.UnitRepository();
        var moduleRepo = new ASLInputTool.Infrastructure.ModuleRepository();
        var vm = new LeadersViewModel(repository, moduleRepo);
        Assert.Equal("Leaders", vm.DisplayName);
        Assert.False(vm.IsAdding);
        Assert.Empty(vm.Items);
    }

    [Fact]
    public void LeadersViewModel_AddAndCancelCommands_ToggleIsAdding()
    {
        var repository = new ASLInputTool.Infrastructure.UnitRepository();
        var moduleRepo = new ASLInputTool.Infrastructure.ModuleRepository();
        var vm = new LeadersViewModel(repository, moduleRepo);
        
        vm.AddCommand.Execute(null);
        Assert.True(vm.IsAdding);

        vm.CancelCommand.Execute(null);
        Assert.False(vm.IsAdding);
    }

    [Fact]
    public void LeadersViewModel_Save_AddsToCollectionAndResetsView()
    {
        var repository = new ASLInputTool.Infrastructure.UnitRepository();
        var moduleRepo = new ASLInputTool.Infrastructure.ModuleRepository();
        var vm = new LeadersViewModel(repository, moduleRepo);
        vm.IsAdding = true;
        vm.Name = "Test Leader";
        vm.Morale = "9";
        vm.BrokenMorale = "10";
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
        var repository = new ASLInputTool.Infrastructure.UnitRepository();
        var moduleRepo = new ASLInputTool.Infrastructure.ModuleRepository();
        var vm = new SquadsViewModel(repository, moduleRepo);
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
        var repository = new ASLInputTool.Infrastructure.UnitRepository();
        var moduleRepo = new ASLInputTool.Infrastructure.ModuleRepository();
        var vm = new HeroesViewModel(repository, moduleRepo);
        vm.Name = "Test Hero";
        vm.Firepower = "1";
        vm.Range = "4";
        vm.Morale = "9";
        vm.BrokenMorale = "10";
        vm.WoundedRange = "4";
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
        var repository = new ASLInputTool.Infrastructure.UnitRepository();
        var moduleRepo = new ASLInputTool.Infrastructure.ModuleRepository();
        var vm = new HeroesViewModel(repository, moduleRepo);
        vm.Name = "Japanese Hero";
        vm.Firepower = "1";
        vm.Range = "4";
        vm.Morale = "10";
        vm.WoundedRange = "4";
        vm.SelectedNationality = Nationality.Japanese;

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        var hero = vm.Items[0].Item;
        Assert.Equal(0, hero.Infantry?.BrokenMorale);
    }

    [Fact]
    public void ScenariosViewModel_SaveScenario_AddsToCollectionAndResetsView()
    {
        var repository = new MockScenarioRepository();
        var vm = new ScenariosViewModel(repository);
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
        var repository = new ASLInputTool.Infrastructure.UnitRepository();
        var moduleRepo = new ASLInputTool.Infrastructure.ModuleRepository();
        var vm = new LeadersViewModel(repository, moduleRepo);
        string? changedPropertyName = null;
        vm.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

        vm.IsAdding = true;

        Assert.Equal(nameof(LeadersViewModel.IsAdding), changedPropertyName);
    }
}
