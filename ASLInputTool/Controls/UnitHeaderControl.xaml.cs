using System.Windows;
using System.Windows.Controls;

namespace ASLInputTool.Controls;

public partial class UnitHeaderControl : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(UnitHeaderControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AddButtonTextProperty =
        DependencyProperty.Register(nameof(AddButtonText), typeof(string), typeof(UnitHeaderControl), new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string AddButtonText
    {
        get => (string)GetValue(AddButtonTextProperty);
        set => SetValue(AddButtonTextProperty, value);
    }

    public UnitHeaderControl()
    {
        InitializeComponent();
    }
}
