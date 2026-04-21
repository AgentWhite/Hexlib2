using System.Windows;
using ASLInputTool.ViewModels;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for NameInputDialog.xaml
/// </summary>
public partial class NameInputDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NameInputDialog"/> class.
    /// </summary>
    /// <param name="viewModel">The ViewModel to bind to.</param>
    public NameInputDialog(NameInputDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += (s, result) => { DialogResult = result; Close(); };
    }
}
