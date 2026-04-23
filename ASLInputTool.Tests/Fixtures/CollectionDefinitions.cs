namespace ASLInputTool.Tests.Fixtures;

/// <summary>
/// Prevents parallel execution for all tests that read or write SettingsManager singleton state.
/// </summary>
[CollectionDefinition("SettingsManager")]
public class SettingsManagerCollection { }
