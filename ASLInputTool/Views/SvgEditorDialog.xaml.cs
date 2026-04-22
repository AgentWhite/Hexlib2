using ASLInputTool.ViewModels;
using ASLInputTool.Infrastructure;
using System;
using System.Windows;
using System.Windows.Input;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for SvgEditorDialog.xaml
/// </summary>
public partial class SvgEditorDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SvgEditorDialog"/> class.
    /// </summary>
    public SvgEditorDialog()
    {
        InitializeComponent();
        GlobalEditorService.ActiveSvgEditorWindow = this;
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        (DataContext as SvgEditorViewModel)?.Unload();
        base.OnClosed(e);
        GlobalEditorService.ActiveSvgEditorWindow = null;
    }

    private void PreviewArea_MouseMove(object sender, MouseEventArgs e)
    {
        if (DataContext is SvgEditorViewModel vm && vm.IsGhostingActive)
        {
            var pos = e.GetPosition((IInputElement)sender);
            vm.GhostPosition = pos;

            if (vm.GhostImage != null)
            {
                // We want to center the ghost image at the cursor.
                // Since GhostPosition is what we bind to for insertion, 
                // we'll use it to update the UI transform here.
                GhostTransform.X = pos.X - (vm.GhostImage.Width / 2);
                GhostTransform.Y = pos.Y - (vm.GhostImage.Height / 2);
            }
        }
    }

    private void PreviewArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SvgEditorViewModel vm && vm.IsGhostingActive)
        {
            vm.PlaceGhostImageCommand.Execute(null);
        }
    }
}
