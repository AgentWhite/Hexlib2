using System.Windows;
using ASLInputTool.ViewModels;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for InsigniaSelectionDialog.xaml
/// </summary>
public partial class InsigniaSelectionDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsigniaSelectionDialog"/> class.
    /// </summary>
    /// <param name="viewModel">The ViewModel to bind to.</param>
    public InsigniaSelectionDialog(InsigniaSelectionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += (s, result) => { DialogResult = result; Close(); };
    }
}
