using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the SVG Editor Dialog.
/// Provides real-time SVG generation, image pasting, and color sampling via pipette.
/// </summary>
public class SvgEditorViewModel : ViewModelBase
{
    private string? _svgContent;
    private string _title = "SVG Editor";
    private bool _isPipetteActive;
    private string _backgroundColor = "#FFFFFF";
    private bool _isCutterActive;
    private BitmapSource? _ghostImage;
    private Point _ghostPosition;
    private bool _isGhostingActive;
    private string _statClass = string.Empty;
    private string _statFirepower = "0";
    private string _statRange = "0";
    private string _statMorale = "0";
    private bool _hasAssaultFire;
    private bool _hasSprayingFire;
    private bool _hasELR;
    private string _statSmoke = string.Empty;
    private bool _isInfantry;

    /// <summary>
    /// Gets or sets the title of the editor.
    /// </summary>
    public string Title { get => _title; set => SetProperty(ref _title, value); }

    /// <summary>
    /// Gets or sets the SVG content being edited.
    /// </summary>
    public string? SvgContent 
    { 
        get => _svgContent; 
        set => SetProperty(ref _svgContent, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pipette tool is active.
    /// </summary>
    public bool IsPipetteActive 
    { 
        get => _isPipetteActive; 
        set 
        { 
            if (SetProperty(ref _isPipetteActive, value))
            {
                if (value) 
                {
                    IsCutterActive = false;
                    GlobalEditorService.ActiveSvgEditor = this;
                }
                else if (GlobalEditorService.ActiveSvgEditor == this) 
                {
                    GlobalEditorService.ActiveSvgEditor = null;
                }
            }
        } 
    }

    /// <summary>
    /// Gets or sets the background color hex string.
    /// </summary>
    public string BackgroundColor 
    { 
        get => _backgroundColor; 
        set 
        { 
            if (SetProperty(ref _backgroundColor, value))
            {
                UpdateSvgFromColor();
            }
        } 
    }

    /// <summary>
    /// Gets or sets the image currently ghosting for placement.
    /// </summary>
    public BitmapSource? GhostImage { get => _ghostImage; set => SetProperty(ref _ghostImage, value); }

    /// <summary>
    /// Gets or sets the position of the ghost image.
    /// </summary>
    public Point GhostPosition { get => _ghostPosition; set => SetProperty(ref _ghostPosition, value); }

    /// <summary>
    /// Gets or sets a value indicating whether ghosting is currently active.
    /// </summary>
    public bool IsGhostingActive { get => _isGhostingActive; set => SetProperty(ref _isGhostingActive, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the polygon cutter tool is active (synced from parent).
    /// </summary>
    public bool IsCutterActive 
    { 
        get => _isCutterActive; 
        set 
        { 
            if (SetProperty(ref _isCutterActive, value) && value)
            {
                IsPipetteActive = false;
            }
        } 
    }

    /// <summary>Gets or sets the letter for the unit class in the top right.</summary>
    public string StatClass { get => _statClass; set { if (SetProperty(ref _statClass, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets the firepower value.</summary>
    public string StatFirepower { get => _statFirepower; set { if (SetProperty(ref _statFirepower, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets the range value.</summary>
    public string StatRange { get => _statRange; set { if (SetProperty(ref _statRange, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets the morale value.</summary>
    public string StatMorale { get => _statMorale; set { if (SetProperty(ref _statMorale, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets a value indicating whether to underline firepower.</summary>
    public bool HasAssaultFire { get => _hasAssaultFire; set { if (SetProperty(ref _hasAssaultFire, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets a value indicating whether to underline range.</summary>
    public bool HasSprayingFire { get => _hasSprayingFire; set { if (SetProperty(ref _hasSprayingFire, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets a value indicating whether to underline morale.</summary>
    public bool HasELR { get => _hasELR; set { if (SetProperty(ref _hasELR, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets the smoke placement exponent superscript.</summary>
    public string StatSmoke { get => _statSmoke; set { if (SetProperty(ref _statSmoke, value)) RegenerateSvg(); } }
    /// <summary>Gets or sets whether to render infantry overlays.</summary>
    public bool IsInfantry { get => _isInfantry; set { if (SetProperty(ref _isInfantry, value)) RegenerateSvg(); } }

    /// <summary>
    /// Gets or sets the command to toggle the cutter tool on the unit images.
    /// </summary>
    public ICommand? ToggleCutterCommand { get; set; }

    /// <summary>
    /// Command to place the current ghost image into the SVG.
    /// </summary>
    public RelayCommand PlaceGhostImageCommand { get; }

    /// <summary>
    /// Command to accept changes and close the dialog.
    /// </summary>
    public RelayCommand OkCommand { get; }

    /// <summary>
    /// Command to cancel changes and close the dialog.
    /// </summary>
    public RelayCommand CancelCommand { get; }

    /// <summary>
    /// Action to close the dialog.
    /// </summary>
    public Action<bool>? CloseAction { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SvgEditorViewModel"/> class.
    /// </summary>
    public SvgEditorViewModel()
    {
        OkCommand = new RelayCommand(_ => CloseAction?.Invoke(true));
        CancelCommand = new RelayCommand(_ => CloseAction?.Invoke(false));
        PlaceGhostImageCommand = new RelayCommand(_ => ExecutePlaceGhostImage());

        // Default initialization
        UpdateSvgFromColor();

        GlobalEditorService.ActiveSvgEditor = this;
        GlobalEditorService.ClipBoardImageChanged += OnClipBoardImageChanged;

        // Load image from clipboard if available
        if (GlobalEditorService.ClipBoardImage != null)
        {
            GhostImage = GlobalEditorService.ClipBoardImage;
            IsGhostingActive = true;
        }
    }

    private void OnClipBoardImageChanged(BitmapSource? image)
    {
        if (image != null)
        {
            GhostImage = image;
            IsGhostingActive = true;
        }
    }

    private void ExecutePlaceGhostImage()
    {
        if (GhostImage == null) return;

        if (string.IsNullOrEmpty(SvgContent))
        {
            UpdateSvgFromColor();
        }

        if (string.IsNullOrEmpty(SvgContent)) return;

        string base64 = BitmapToBase64(GhostImage);
        // We use GhostPosition.X/Y which are relative to the 120x120 editor area.
        // We offset by half width/height to center the paste at the cursor.
        double x = GhostPosition.X - (GhostImage.Width / 2);
        double y = GhostPosition.Y - (GhostImage.Height / 2);

        string imgTag = $"\n  <image x=\"{x:F1}\" y=\"{y:F1}\" " +
                        $"width=\"{GhostImage.Width:F1}\" height=\"{GhostImage.Height:F1}\" " +
                        $"href=\"data:image/png;base64,{base64}\"/>";
        
        int index = SvgContent.LastIndexOf("</svg>");
        if (index != -1)
        {
            SvgContent = SvgContent.Insert(index, imgTag);
        }
    }

    private string BitmapToBase64(BitmapSource bitmap)
    {
        // Optimization: for counter design, we don't need high-res photos.
        // Downscaling keeps the SVG code manageable and prevents UI crashes.
        const int MaxDimension = 256;
        if (bitmap.PixelWidth > MaxDimension || bitmap.PixelHeight > MaxDimension)
        {
            double scale = (double)MaxDimension / Math.Max(bitmap.PixelWidth, bitmap.PixelHeight);
            var resized = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));
            bitmap = resized;
        }

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return Convert.ToBase64String(ms.ToArray());
    }

    private void RegenerateSvg()
    {
        // Dimensions matches the editor preview (120x120)
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<svg width=\"120\" height=\"120\" xmlns=\"http://www.w3.org/2000/svg\">");
        
        // 1. Background
        sb.AppendLine($"  <rect width=\"120\" height=\"120\" fill=\"{_backgroundColor}\" />");

        // 2. Infanry Stats Overlay
        if (IsInfantry)
        {
            const string font = "font-family=\"Arial, sans-serif\" font-weight=\"bold\"";
            
            // Class (Top Right)
            if (!string.IsNullOrEmpty(StatClass))
            {
                sb.AppendLine($"  <text x=\"110\" y=\"28\" text-anchor=\"end\" font-size=\"24\" {font} fill=\"black\">{StatClass}</text>");
            }

            // Stats row (Bottom Center: F-R-M)
            double centerY = 105;
            double dashWidth = 8;
            double digitWidth = 18;
            double totalWidth = (digitWidth * 3) + (dashWidth * 2);
            double startX = (120 - totalWidth) / 2;

            // Firepower
            double fpX = startX + (digitWidth / 2);
            sb.AppendLine($"  <text x=\"{fpX:F1}\" y=\"{centerY:F1}\" text-anchor=\"middle\" font-size=\"24\" {font} fill=\"black\">{StatFirepower}</text>");
            if (HasAssaultFire) 
                sb.AppendLine($"  <line x1=\"{fpX - 8:F1}\" y1=\"{centerY + 4:F1}\" x2=\"{fpX + 8:F1}\" y2=\"{centerY + 4:F1}\" stroke=\"black\" stroke-width=\"2\" />");

            // Smoke Exponent (Superscript next to Firepower)
            if (!string.IsNullOrEmpty(StatSmoke))
            {
                sb.AppendLine($"  <text x=\"{fpX + 9:F1}\" y=\"{centerY - 12:F1}\" text-anchor=\"start\" font-size=\"14\" {font} fill=\"black\">{StatSmoke}</text>");
            }

            // Dash 1
            double d1X = startX + digitWidth + (dashWidth / 2);
            sb.AppendLine($"  <text x=\"{d1X:F1}\" y=\"{centerY:F1}\" text-anchor=\"middle\" font-size=\"24\" {font} fill=\"black\">-</text>");

            // Range
            double rX = startX + digitWidth + dashWidth + (digitWidth / 2);
            sb.AppendLine($"  <text x=\"{rX:F1}\" y=\"{centerY:F1}\" text-anchor=\"middle\" font-size=\"24\" {font} fill=\"black\">{StatRange}</text>");
            if (HasSprayingFire)
                sb.AppendLine($"  <line x1=\"{rX - 8:F1}\" y1=\"{centerY + 4:F1}\" x2=\"{rX + 8:F1}\" y2=\"{centerY + 4:F1}\" stroke=\"black\" stroke-width=\"2\" />");

            // Dash 2
            double d2X = startX + (digitWidth * 2) + dashWidth + (dashWidth / 2);
            sb.AppendLine($"  <text x=\"{d2X:F1}\" y=\"{centerY:F1}\" text-anchor=\"middle\" font-size=\"24\" {font} fill=\"black\">-</text>");

            // Morale
            double mX = startX + (digitWidth * 2) + (dashWidth * 2) + (digitWidth / 2);
            sb.AppendLine($"  <text x=\"{mX:F1}\" y=\"{centerY:F1}\" text-anchor=\"middle\" font-size=\"24\" {font} fill=\"black\">{StatMorale}</text>");
            if (HasELR)
                sb.AppendLine($"  <line x1=\"{mX - 8:F1}\" y1=\"{centerY + 4:F1}\" x2=\"{mX + 8:F1}\" y2=\"{centerY + 4:F1}\" stroke=\"black\" stroke-width=\"2\" />");
        }

        sb.AppendLine("</svg>");
        SvgContent = sb.ToString();
    }

    private void UpdateSvgFromColor() => RegenerateSvg();

    /// <summary>
    /// Cleans up service registration and event subscriptions.
    /// </summary>
    public void Unload()
    {
        GlobalEditorService.ClipBoardImageChanged -= OnClipBoardImageChanged;
        if (GlobalEditorService.ActiveSvgEditor == this)
        {
            GlobalEditorService.ActiveSvgEditor = null;
        }
    }
}
