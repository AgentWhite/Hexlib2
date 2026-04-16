using System.Windows;
using System.Windows.Controls;

namespace ASLInputTool.Controls;

/// <summary>
/// A reusable header control for unit categories with an optional "Add" button.
/// </summary>
public partial class UnitHeaderControl : UserControl
{
    /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(UnitHeaderControl), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="AddButtonText"/> dependency property.</summary>
    public static readonly DependencyProperty AddButtonTextProperty =
        DependencyProperty.Register(nameof(AddButtonText), typeof(string), typeof(UnitHeaderControl), new PropertyMetadata(string.Empty));

    /// <summary>Gets or sets the title text shown in the header.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the text for the "Add" button.</summary>
    public string AddButtonText
    {
        get => (string)GetValue(AddButtonTextProperty);
        set => SetValue(AddButtonTextProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitHeaderControl"/> class.
    /// </summary>
    public UnitHeaderControl()
    {
        InitializeComponent();
    }
}
