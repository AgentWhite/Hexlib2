using ASLInputTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// A global service to track active editors for cross-window communication.
/// </summary>
public static class GlobalEditorService
{
    private static SvgEditorViewModel? _activeSvgEditor;
    private static BitmapSource? _clipBoardImage;

    /// <summary>
    /// Event fired when the global cutout 'clipboard' image changes.
    /// </summary>
    public static event Action<BitmapSource?>? ClipBoardImageChanged;

    /// <summary>
    /// Gets or sets the top-level Window currently hosting an SVG Editor.
    /// </summary>
    public static Window? ActiveSvgEditorWindow { get; set; }

    /// <summary>
    /// Gets or sets the image currently in the 'clipboard' for pasting into the SVG editor.
    /// </summary>
    public static BitmapSource? ClipBoardImage
    {
        get => _clipBoardImage;
        set 
        {
            _clipBoardImage = value;
            ClipBoardImageChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Gets or sets the currently active SVG editor ViewModel.
    /// </summary>
    public static SvgEditorViewModel? ActiveSvgEditor
    {
        get => _activeSvgEditor;
        set => _activeSvgEditor = value;
    }
}
