using System.Windows.Controls;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for LeadersView.xaml
/// </summary>
public partial class LeadersView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LeadersView"/> class.
    /// </summary>
    public LeadersView()
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
