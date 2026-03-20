using System.Windows.Controls;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for HeroesView.xaml
/// </summary>
public partial class HeroesView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesView"/> class.
    /// </summary>
    public HeroesView()
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
