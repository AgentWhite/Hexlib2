using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ASLInputTool.Infrastructure;
using ASLInputTool.ViewModels;

namespace ASLInputTool.Controls;

/// <summary>
/// A control that allows picking front and back images for a unit.
/// </summary>
public partial class UnitImagePickerControl : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnitImagePickerControl"/> class.
    /// </summary>
    public UnitImagePickerControl()
    {
        InitializeComponent();
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        if (DataContext is UnitViewModelBase vm && vm.IsCutterActive && vm.ActivePolygonPoints.Count > 0)
        {
            // Only ghost on the active side
            if (sender is FrameworkElement fe && fe.Tag as string != vm.ActiveCutterSide) return;

            var pos = e.GetPosition((IInputElement)sender);
            UpdateCutterGhost(vm, pos);
        }
    }

    private void UpdateCutterGhost(UnitViewModelBase vm, Point p)
    {
        if (vm.ActivePolygonPoints.Count == 0)
        {
            vm.CutterGhostGeometry = null;
            return;
        }

        Point startPoint = vm.ActivePolygonPoints[0];
        Point lastPoint = vm.ActivePolygonPoints[^1];

        // Snapping logic
        double distToStart = (p - startPoint).Length;
        const double SnapThreshold = 15;
        bool isSnapped = distToStart < SnapThreshold && vm.ActivePolygonPoints.Count >= 2;

        if (isSnapped) p = startPoint;

        // Building preview geometry
        var group = new GeometryGroup();

        // 1. Solid segments
        var solid = new StreamGeometry();
        using (var ctx = solid.Open())
        {
            ctx.BeginFigure(startPoint, false, false);
            if (vm.ActivePolygonPoints.Count > 1)
            {
                ctx.PolyLineTo(System.Linq.Enumerable.ToList(System.Linq.Enumerable.Skip(vm.ActivePolygonPoints, 1)), true, true);
            }
        }
        group.Children.Add(solid);

        // 2. Ghost segment to current mouse
        group.Children.Add(new LineGeometry(lastPoint, p));

        if (group.CanFreeze) group.Freeze();
        vm.CutterGhostGeometry = group;
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not UnitViewModelBase vm) return;

        if (vm.IsCutterActive)
        {
            if (sender is not FrameworkElement fe) return;
            string side = fe.Tag as string ?? "";

            // If we haven't started drawing, set the side
            if (vm.ActivePolygonPoints.Count == 0)
            {
                vm.ActiveCutterSide = side;
            }
            // If we are drawing, only allow same side
            else if (vm.ActiveCutterSide != side)
            {
                return;
            }

            Point pos = e.GetPosition((IInputElement)sender);

            // Check for snap to close
            if (vm.ActivePolygonPoints.Count >= 3)
            {
                Point startPoint = vm.ActivePolygonPoints[0];
                if ((pos - startPoint).Length < 15)
                {
                    FinishCutting(vm, (Image)sender);
                    e.Handled = true;
                    return;
                }
            }

            vm.ActivePolygonPoints.Add(pos);
            UpdateCutterGhost(vm, pos);
            e.Handled = true;
            return;
        }

        var activeEditor = GlobalEditorService.ActiveSvgEditor;
        if (activeEditor != null && activeEditor.IsPipetteActive)
        {
            if (sender is Image image && image.Source is BitmapSource source)
            {
                Point pos = e.GetPosition(image);
                
                // Map control coordinates to pixel coordinates for UniformToFill
                double controlAspect = image.ActualWidth / image.ActualHeight;
                double imageAspect = (double)source.PixelWidth / source.PixelHeight;

                double scale;
                double offsetX = 0, offsetY = 0;

                if (imageAspect > controlAspect)
                {
                    scale = image.ActualHeight / source.PixelHeight;
                    offsetX = (source.PixelWidth * scale - image.ActualWidth) / 2;
                }
                else
                {
                    scale = image.ActualWidth / source.PixelWidth;
                    offsetY = (source.PixelHeight * scale - image.ActualHeight) / 2;
                }

                int pixelX = (int)((pos.X + offsetX) / scale);
                int pixelY = (int)((pos.Y + offsetY) / scale);

                if (pixelX >= 0 && pixelX < source.PixelWidth && pixelY >= 0 && pixelY < source.PixelHeight)
                {
                    byte[] pixels = new byte[4];
                    source.CopyPixels(new Int32Rect(pixelX, pixelY, 1, 1), pixels, 4, 0);
                    string hexColor = $"#{pixels[2]:X2}{pixels[1]:X2}{pixels[0]:X2}";
                    activeEditor.BackgroundColor = hexColor;
                }
                
                e.Handled = true;
            }
        }
    }

    private void FinishCutting(UnitViewModelBase vm, Image image)
    {
        if (vm.ActivePolygonPoints.Count < 3) return;

        var pathGeometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = vm.ActivePolygonPoints[0], IsClosed = true, IsFilled = true };
        figure.Segments.Add(new PolyLineSegment(System.Linq.Enumerable.Skip(vm.ActivePolygonPoints, 1), true));
        pathGeometry.Figures.Add(figure);

        if (image.Source is BitmapSource source)
        {
            var clipped = ImageProcessingService.ClipImage(source, pathGeometry, image.ActualWidth, image.ActualHeight, image.Stretch);
            if (clipped != null)
            {
                if (clipped.CanFreeze) clipped.Freeze();
                GlobalEditorService.ClipBoardImage = clipped;
                vm.IsCutterActive = false;
                vm.ActivePolygonPoints.Clear();
                vm.CutterGhostGeometry = null;
            }
        }
    }
}
