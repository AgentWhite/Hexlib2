using System.Windows.Controls;

namespace ASLInputTool.Views;

/// <summary>
/// Interaction logic for EquipmentView.xaml
/// </summary>
public partial class EquipmentView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentView"/> class.
    /// </summary>
    public EquipmentView()
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
