using System.Windows.Controls;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for SquadsView.xaml
/// </summary>
public partial class SquadsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SquadsView"/> class.
    /// </summary>
    public SquadsView()
    {
        InitializeComponent();
    }

    private void OnTextBoxLostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            var binding = tb.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
        }
    }
}
