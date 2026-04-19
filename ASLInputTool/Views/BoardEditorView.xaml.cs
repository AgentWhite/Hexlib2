using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ASLInputTool.ViewModels;

namespace ASLInputTool.Views
{
    /// <summary>
    /// Interaction logic for BoardEditorView.xaml
    /// </summary>
    public partial class BoardEditorView : UserControl
    {
        private Cursor? _zoomInCursor;
        private Cursor? _zoomOutCursor;

        /// <summary>
    /// Initializes a new instance of the <see cref="BoardEditorView"/> class.
    /// </summary>
    public BoardEditorView()
        {
            InitializeComponent();
            LoadCursors();
            DataContextChanged += OnDataContextChanged;
            PreviewMouseMove += OnBoardMouseMove;
        }

        private void LoadCursors()
        {
            try
            {
                var zoomInStream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Cursors/zoom-in.cur"));
                if (zoomInStream != null) _zoomInCursor = new Cursor(zoomInStream.Stream);

                var zoomOutStream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Cursors/zoom-out.cur"));
                if (zoomOutStream != null) _zoomOutCursor = new Cursor(zoomOutStream.Stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cursors: {ex.Message}");
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is BoardEditorViewModel oldVm)
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            
            if (e.NewValue is BoardEditorViewModel newVm)
            {
                newVm.PropertyChanged += OnViewModelPropertyChanged;
                UpdateCursor();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BoardEditorViewModel.CurrentTool))
            {
                UpdateCursor();
            }
        }

        private void UpdateCursor()
        {
            if (ViewModel == null) return;

            Cursor newCursor = Cursors.Arrow;
            if (ViewModel.CurrentTool == ToolMode.ZoomIn && _zoomInCursor != null)
                newCursor = _zoomInCursor;
            else if (ViewModel.CurrentTool == ToolMode.ZoomOut && _zoomOutCursor != null)
                newCursor = _zoomOutCursor;
            
            // Apply to the entire ScrollViewer for consistent tool feel
            BoardScrollViewer.Cursor = newCursor;
        }

        private BoardEditorViewModel? ViewModel => DataContext as BoardEditorViewModel;

        private void OnBoardMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel == null) return;

            // Optional: Only zoom with Ctrl held down to allow normal scroll
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            e.Handled = true;
            double delta = e.Delta > 0 ? 0.1 : -0.1;
            ApplyZoom(delta, e.GetPosition(ZoomContainer));
        }

        private void OnBoardMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            // Determine which coordinate space to use (Editor or LOS tab)
            bool isLosTab = false;
            bool hitContent = false;
            DependencyObject? current = e.OriginalSource as DependencyObject;
            while (current != null)
            {
                if (current == LosZoomContainer) { isLosTab = true; hitContent = true; break; }
                if (current == ZoomContainer) { hitContent = true; break; }
                current = VisualTreeHelper.GetParent(current);
            }
            
            if (!hitContent) return;
            UIElement container = isLosTab ? LosZoomContainer : ZoomContainer;
            Point position = e.GetPosition(container);

            if (ViewModel.CurrentTool == ToolMode.ZoomIn || ViewModel.CurrentTool == ToolMode.ZoomOut)
            {
                e.Handled = true;
                double delta = ViewModel.CurrentTool == ToolMode.ZoomIn ? 0.2 : -0.2;
                ApplyZoom(delta, position);
            }
            else if (ViewModel.CurrentTool == ToolMode.PenRect)
            {
                e.Handled = true;
                ViewModel.PenRectClickCommand.Execute(position);
            }
            else if (ViewModel.CurrentTool == ToolMode.PenPolygon || ViewModel.CurrentTool == ToolMode.PenSubtract)
            {
                e.Handled = true;
                ViewModel.PenPolygonClickCommand.Execute(position);
            }
        }

        private void OnBoardMouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel == null) return;
            if (ViewModel.CurrentTool == ToolMode.PenRect || ViewModel.CurrentTool == ToolMode.PenPolygon || ViewModel.CurrentTool == ToolMode.PenSubtract)
            {
                // Determine container
                bool isLosTab = false;
                DependencyObject? current = e.OriginalSource as DependencyObject;
                while (current != null)
                {
                    if (current == LosZoomContainer) { isLosTab = true; break; }
                    current = VisualTreeHelper.GetParent(current);
                }
                
                UIElement container = isLosTab ? LosZoomContainer : ZoomContainer;
                Point position = e.GetPosition(container);

                if (ViewModel.CurrentTool == ToolMode.PenRect)
                {
                    ViewModel.PenRectHoverCommand.Execute(position);
                }
                else if (ViewModel.CurrentTool == ToolMode.PenPolygon || ViewModel.CurrentTool == ToolMode.PenSubtract)
                {
                    ViewModel.PenPolygonHoverCommand.Execute(position);
                }
            }
        }

        private void ApplyZoom(double delta, Point mousePosInContent)
        {
            if (ViewModel == null) return;

            double oldScale = ViewModel.ZoomLevel;
            double newScale = Math.Max(0.1, Math.Min(10.0, oldScale + delta));
            
            if (Math.Abs(oldScale - newScale) < 0.001) return;

            // Zoom at mouse position logic:
            // 1. Get offsets relative to the viewport
            double relativeX = mousePosInContent.X / ZoomContainer.ActualWidth;
            double relativeY = mousePosInContent.Y / ZoomContainer.ActualHeight;

            // 2. Update the scale
            ViewModel.ZoomLevel = newScale;

            // 3. Forced Layout update so we can get new container sizes
            ZoomContainer.UpdateLayout();

            // 4. Adjust ScrollViewer to keep the mouse point stationary (or centered)
            // If it was a click-to-zoom (MouseDown), we want to center the point.
            // If it was a wheel zoom, we want to keep it under the cursor.
            
            // For simplicity and "Advanced" UX, we'll try to keep the point stationary during wheel
            // and center it during tool-click.
            
            bool isToolClick = Math.Abs(delta) > 0.15; // Rough heuristic for 0.2 delta
            
            if (isToolClick)
            {
                // Center the clicked point
                BoardScrollViewer.ScrollToHorizontalOffset(mousePosInContent.X * newScale - BoardScrollViewer.ViewportWidth / 2);
                BoardScrollViewer.ScrollToVerticalOffset(mousePosInContent.Y * newScale - BoardScrollViewer.ViewportHeight / 2);
            }
            else
            {
                // Scroll to keep mouse point stationary
                BoardScrollViewer.ScrollToHorizontalOffset(mousePosInContent.X * newScale - relativeX * BoardScrollViewer.ViewportWidth);
                BoardScrollViewer.ScrollToVerticalOffset(mousePosInContent.Y * newScale - relativeY * BoardScrollViewer.ViewportHeight);
            }
        }
    }
}
