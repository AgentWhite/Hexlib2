using System.Windows.Controls;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for ScenariosView.xaml
/// </summary>
public partial class ScenariosView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosView"/> class.
    /// </summary>
    public ScenariosView()
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
