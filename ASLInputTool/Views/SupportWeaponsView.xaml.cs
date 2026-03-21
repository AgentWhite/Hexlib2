using System.Windows.Controls;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for SupportWeaponsView.xaml
/// </summary>
public partial class SupportWeaponsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SupportWeaponsView"/> class.
    /// </summary>
    public SupportWeaponsView()
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
