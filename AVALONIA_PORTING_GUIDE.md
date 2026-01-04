# ReoGrid Avalonia Porting Guide - Quick Reference

## Overview

This is a companion guide to the detailed assessment document (`AVALONIA_COMPATIBILITY_ASSESSMENT.md`). It provides quick reference code snippets and implementation examples for porting ReoGrid to Avalonia UI.

---

## Quick Start

### 1. Project Setup

Create `ReoGrid/ReoGridAvalonia.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net6.0</TargetFrameworks>
    <OutputType>Library</OutputType>
    <RootNamespace>unvell.ReoGrid</RootNamespace>
    <AssemblyName>unvell.ReoGrid.Avalonia</AssemblyName>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <PropertyGroup>
    <DefineConstants>AVALONIA;FORMULA;LANG_JP;OUTLINE;DRAWING;COMMENT;PRINT;RICHTEXT</DefineConstants>
    <Authors>Jingwood</Authors>
    <Company>UNVELL Inc.</Company>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://reogrid.net</PackageProjectUrl>
    <RepositoryUrl>https://github.com/unvell/ReoGrid/</RepositoryUrl>
    <Description>ReoGrid spreadsheet control for Avalonia UI</Description>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.*" />
    <PackageReference Include="Avalonia.Skia" Version="11.0.*" />
    <PackageReference Include="System.IO.Compression" Version="4.3.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Exclude platform-specific files -->
    <Compile Remove="WinForm\**" />
    <Compile Remove="WPF\**" />
    <Compile Remove="Android\**" />
    <Compile Remove="iOS\**" />
    <Compile Remove="Test\**" />
    <EmbeddedResource Remove="WinForm\**" />
    <EmbeddedResource Remove="WPF\**" />
    <EmbeddedResource Remove="Android\**" />
    <EmbeddedResource Remove="iOS\**" />
    <EmbeddedResource Remove="Test\**" />
  </ItemGroup>
</Project>
```

### 2. Type Aliases

Add to relevant files (e.g., `IGraphics.cs`, `RenderingInterface.cs`):

```csharp
#if AVALONIA
using RGFloat = System.Double;
using RGPen = Avalonia.Media.Pen;
using RGBrush = Avalonia.Media.Brush;
using RGPath = Avalonia.Media.Geometry;
using RGImage = Avalonia.Media.Imaging.Bitmap;
using PlatformGraphics = Avalonia.Media.DrawingContext;
using RGTransform = Avalonia.Matrix;
using Cursor = Avalonia.Input.Cursor;
#endif
```

Add to `ControlShare.cs`:

```csharp
#if AVALONIA
using RGFloat = System.Double;
using RGPoint = Avalonia.Point;
using RGPointF = Avalonia.Point;
using IntOrDouble = System.Double;
#endif
```

---

## Implementation Examples

### 3. Graphics Implementation

Create `ReoGrid/Avalonia/AvaloniaGraphics.cs`:

```csharp
#if AVALONIA

using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using unvell.ReoGrid.Graphics;
using unvell.ReoGrid.Drawing.Text;

namespace unvell.ReoGrid.Avalonia
{
    /// <summary>
    /// Avalonia implementation of IGraphics interface
    /// </summary>
    internal class AvaloniaGraphics : IGraphics
    {
        private DrawingContext dc;

        public PlatformGraphics PlatformGraphics
        {
            get => dc;
            set => dc = (DrawingContext)value;
        }

        #region Line Drawing

        public void DrawLine(double x1, double y1, double x2, double y2, SolidColor color)
        {
            var pen = GetPen(color);
            dc.DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
        }

        public void DrawLine(double x1, double y1, double x2, double y2, 
            SolidColor color, double width, LineStyles style)
        {
            var pen = new Pen(GetBrush(color), width);
            
            // Set line style
            switch (style)
            {
                case LineStyles.Solid:
                    // Default is solid
                    break;
                case LineStyles.Dash:
                    pen.DashStyle = DashStyle.Dash;
                    break;
                case LineStyles.Dot:
                    pen.DashStyle = DashStyle.Dot;
                    break;
                case LineStyles.DashDot:
                    pen.DashStyle = DashStyle.DashDot;
                    break;
                case LineStyles.DashDotDot:
                    pen.DashStyle = DashStyle.DashDotDot;
                    break;
            }

            dc.DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
        }

        public void DrawLine(Point startPoint, Point endPoint, SolidColor color)
        {
            DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, color);
        }

        public void DrawLine(Point startPoint, Point endPoint, 
            SolidColor color, double width, LineStyles style)
        {
            DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, 
                color, width, style);
        }

        public void DrawLine(Pen p, double x1, double y1, double x2, double y2)
        {
            dc.DrawLine(p, new Point(x1, y1), new Point(x2, y2));
        }

        public void DrawLine(Pen p, Point startPoint, Point endPoint)
        {
            dc.DrawLine(p, new global::Avalonia.Point(startPoint.X, startPoint.Y),
                new global::Avalonia.Point(endPoint.X, endPoint.Y));
        }

        public void DrawLines(Point[] points, int start, int length, 
            SolidColor color, double width, LineStyles style)
        {
            if (points == null || points.Length == 0) return;

            var pen = new Pen(GetBrush(color), width);
            
            for (int i = start; i < start + length - 1; i++)
            {
                var p1 = new global::Avalonia.Point(points[i].X, points[i].Y);
                var p2 = new global::Avalonia.Point(points[i + 1].X, points[i + 1].Y);
                dc.DrawLine(pen, p1, p2);
            }
        }

        #endregion

        #region Rectangle Drawing

        public void DrawRectangle(Rectangle rect, SolidColor color)
        {
            var pen = GetPen(color);
            var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            dc.DrawRectangle(null, pen, avRect);
        }

        public void DrawRectangle(Rectangle rect, SolidColor color, 
            double width, LineStyles lineStyle)
        {
            var pen = new Pen(GetBrush(color), width);
            var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            dc.DrawRectangle(null, pen, avRect);
        }

        public void DrawRectangle(double x, double y, double width, 
            double height, SolidColor color)
        {
            var pen = GetPen(color);
            var rect = new Rect(x, y, width, height);
            dc.DrawRectangle(null, pen, rect);
        }

        public void DrawRectangle(Pen p, Rectangle rect)
        {
            var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            dc.DrawRectangle(null, p, avRect);
        }

        public void DrawRectangle(Pen p, double x, double y, 
            double width, double height)
        {
            var rect = new Rect(x, y, width, height);
            dc.DrawRectangle(null, p, rect);
        }

        #endregion

        #region Rectangle Filling

        public void FillRectangle(Rectangle rect, IColor color)
        {
            var brush = GetBrush(color);
            var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            dc.DrawRectangle(brush, null, avRect);
        }

        public void FillRectangle(double x, double y, double width, 
            double height, IColor color)
        {
            var brush = GetBrush(color);
            var rect = new Rect(x, y, width, height);
            dc.DrawRectangle(brush, null, rect);
        }

        public void FillRectangle(Brush b, double x, double y, 
            double width, double height)
        {
            var rect = new Rect(x, y, width, height);
            dc.DrawRectangle(b, null, rect);
        }

        public void FillRectangleLinear(SolidColor startColor, SolidColor endColor, 
            double angle, Rectangle rect)
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(ToAvaloniaColor(startColor), 0),
                    new GradientStop(ToAvaloniaColor(endColor), 1)
                }
            };

            var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            dc.DrawRectangle(brush, null, avRect);
        }

        public void DrawAndFillRectangle(Rectangle rect, SolidColor lineColor, 
            IColor fillColor)
        {
            var pen = GetPen(lineColor);
            var brush = GetBrush(fillColor);
            var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            dc.DrawRectangle(brush, pen, avRect);
        }

        #endregion

        #region Ellipse

        public void DrawEllipse(Rectangle rect, SolidColor color)
        {
            var pen = GetPen(color);
            var center = new global::Avalonia.Point(
                rect.X + rect.Width / 2, 
                rect.Y + rect.Height / 2);
            var radiusX = rect.Width / 2;
            var radiusY = rect.Height / 2;
            
            var geometry = new EllipseGeometry(new Rect(rect.X, rect.Y, 
                rect.Width, rect.Height));
            dc.DrawGeometry(null, pen, geometry);
        }

        public void FillEllipse(Rectangle rect, IColor color)
        {
            var brush = GetBrush(color);
            var geometry = new EllipseGeometry(new Rect(rect.X, rect.Y, 
                rect.Width, rect.Height));
            dc.DrawGeometry(brush, null, geometry);
        }

        #endregion

        #region Text

        public void DrawText(string text, string fontName, double fontSize, 
            SolidColor color, Rectangle rect, 
            ReoGridHorAlign halign = ReoGridHorAlign.Left,
            ReoGridVerAlign valign = ReoGridVerAlign.Top)
        {
            if (string.IsNullOrEmpty(text)) return;

            var typeface = new Typeface(fontName);
            var foreground = GetBrush(color);

            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                foreground);

            // Calculate position based on alignment
            double x = rect.X;
            double y = rect.Y;

            switch (halign)
            {
                case ReoGridHorAlign.Center:
                    x = rect.X + (rect.Width - formattedText.Width) / 2;
                    break;
                case ReoGridHorAlign.Right:
                    x = rect.X + rect.Width - formattedText.Width;
                    break;
            }

            switch (valign)
            {
                case ReoGridVerAlign.Middle:
                    y = rect.Y + (rect.Height - formattedText.Height) / 2;
                    break;
                case ReoGridVerAlign.Bottom:
                    y = rect.Y + rect.Height - formattedText.Height;
                    break;
            }

            dc.DrawText(formattedText, new global::Avalonia.Point(x, y));
        }

        public Size MeasureText(string text, string fontName, double fontSize)
        {
            if (string.IsNullOrEmpty(text)) 
                return new Size(0, 0);

            var typeface = new Typeface(fontName);
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        #endregion

        #region Helper Methods

        private Pen GetPen(SolidColor color, double width = 1.0)
        {
            return new Pen(GetBrush(color), width);
        }

        public Pen GetPen(SolidColor color)
        {
            return GetPen(color, 1.0);
        }

        private Brush GetBrush(IColor color)
        {
            if (color is SolidColor solidColor)
            {
                return new SolidColorBrush(ToAvaloniaColor(solidColor));
            }
            return Brushes.Black;
        }

        private Color ToAvaloniaColor(SolidColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        #endregion

        // Implement remaining IGraphics interface methods...
    }
}

#endif
```

### 4. Main Control Implementation

Create `ReoGrid/Avalonia/AvaloniaControl.cs`:

```csharp
#if AVALONIA

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using unvell.ReoGrid.Rendering;
using unvell.ReoGrid.Views;

namespace unvell.ReoGrid.Avalonia
{
    /// <summary>
    /// ReoGrid control for Avalonia UI
    /// </summary>
    public partial class ReoGridControl : Control
    {
        private AvaloniaRenderer renderer;
        private Worksheet currentWorksheet;

        static ReoGridControl()
        {
            // Register properties that affect rendering
            AffectsRender<ReoGridControl>(BoundsProperty);
            AffectsMeasure<ReoGridControl>(BoundsProperty);
        }

        public ReoGridControl()
        {
            InitializeComponent();
            
            renderer = new AvaloniaRenderer();
            
            // Enable keyboard and pointer events
            Focusable = true;
            ClipToBounds = true;
        }

        private void InitializeComponent()
        {
            // Initialize workbook and worksheets
            // Similar to WPF implementation
        }

        #region Rendering

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (currentWorksheet == null) return;

            try
            {
                renderer.PlatformGraphics = context;
                
                // Render the worksheet
                // Call into shared rendering logic
                RenderWorksheet(renderer);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Render error: {ex}");
            }
        }

        private void RenderWorksheet(IRenderer renderer)
        {
            // Implement worksheet rendering logic
            // This calls into the shared ReoGrid core rendering code
        }

        #endregion

        #region Mouse/Pointer Events

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            
            Focus();
            
            var point = e.GetPosition(this);
            var button = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
            
            // Convert to ReoGrid event
            HandleMouseDown(point.X, point.Y, button);
            
            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            
            var point = e.GetPosition(this);
            HandleMouseMove(point.X, point.Y);
            
            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            
            var point = e.GetPosition(this);
            HandleMouseUp(point.X, point.Y);
            
            e.Handled = true;
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            
            var delta = e.Delta.Y;
            HandleMouseWheel(delta);
            
            e.Handled = true;
        }

        #endregion

        #region Keyboard Events

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            // Convert Avalonia key to ReoGrid KeyCode
            var keyCode = ConvertKey(e.Key);
            HandleKeyDown(keyCode);
            
            e.Handled = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            
            var keyCode = ConvertKey(e.Key);
            HandleKeyUp(keyCode);
            
            e.Handled = true;
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            
            if (!string.IsNullOrEmpty(e.Text))
            {
                HandleTextInput(e.Text);
            }
            
            e.Handled = true;
        }

        private Interaction.KeyCode ConvertKey(Key key)
        {
            // Map Avalonia keys to ReoGrid KeyCode enum
            // Implementation needed
            return Interaction.KeyCode.None;
        }

        #endregion

        #region Event Handlers (call into shared logic)

        private void HandleMouseDown(double x, double y, 
            PointerUpdateKind button)
        {
            // Call into shared control logic in ControlShare.cs
        }

        private void HandleMouseMove(double x, double y)
        {
            // Call into shared control logic
        }

        private void HandleMouseUp(double x, double y)
        {
            // Call into shared control logic
        }

        private void HandleMouseWheel(double delta)
        {
            // Handle scroll
            InvalidateVisual();
        }

        private void HandleKeyDown(Interaction.KeyCode keyCode)
        {
            // Call into shared control logic
        }

        private void HandleKeyUp(Interaction.KeyCode keyCode)
        {
            // Call into shared control logic
        }

        private void HandleTextInput(string text)
        {
            // Handle text input for cell editing
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets or sets the current worksheet
        /// </summary>
        public Worksheet CurrentWorksheet
        {
            get => currentWorksheet;
            set
            {
                currentWorksheet = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Refresh the control
        /// </summary>
        public void Refresh()
        {
            Dispatcher.UIThread.Post(() => InvalidateVisual(), 
                DispatcherPriority.Render);
        }

        #endregion
    }
}

#endif
```

### 5. Renderer Implementation

Create `ReoGrid/Avalonia/AvaloniaRenderer.cs`:

```csharp
#if AVALONIA

using System;
using Avalonia.Media;
using unvell.ReoGrid.Graphics;
using unvell.ReoGrid.Rendering;
using unvell.ReoGrid.Drawing.Text;

namespace unvell.ReoGrid.Avalonia
{
    /// <summary>
    /// Avalonia-specific renderer implementing IRenderer
    /// </summary>
    internal class AvaloniaRenderer : AvaloniaGraphics, IRenderer
    {
        #region Running Focus Rectangle

        public void DrawRunningFocusRect(double x, double y, double w, double h, 
            SolidColor color, int runningOffset)
        {
            var pen = new Pen(GetBrush(color), 1.0)
            {
                DashStyle = new DashStyle(new[] { 2.0, 2.0 }, runningOffset)
            };

            var rect = new global::Avalonia.Rect(x, y, w, h);
            PlatformGraphics.DrawRectangle(null, pen, rect);
        }

        #endregion

        #region Capped Lines

        private Pen cappedLinePen;

        public void BeginCappedLine(LineCapStyles startCap, Size startSize,
            LineCapStyles endCap, Size endSize, SolidColor color, double width)
        {
            cappedLinePen = new Pen(GetBrush(color), width);
            
            // Set line caps
            cappedLinePen.StartLineCap = ConvertLineCap(startCap);
            cappedLinePen.EndLineCap = ConvertLineCap(endCap);
        }

        public void DrawCappedLine(double x1, double y1, double x2, double y2)
        {
            if (cappedLinePen != null)
            {
                PlatformGraphics.DrawLine(cappedLinePen, 
                    new global::Avalonia.Point(x1, y1),
                    new global::Avalonia.Point(x2, y2));
            }
        }

        public void EndCappedLine()
        {
            cappedLinePen = null;
        }

        private PenLineCap ConvertLineCap(LineCapStyles style)
        {
            switch (style)
            {
                case LineCapStyles.Round:
                    return PenLineCap.Round;
                case LineCapStyles.Square:
                    return PenLineCap.Square;
                default:
                    return PenLineCap.Flat;
            }
        }

        #endregion

        #region Batch Line Drawing

        private Pen batchLinePen;

        public void BeginDrawLine(double width, SolidColor color)
        {
            batchLinePen = new Pen(GetBrush(color), width);
        }

        public void DrawLine(double x1, double y1, double x2, double y2)
        {
            if (batchLinePen != null)
            {
                PlatformGraphics.DrawLine(batchLinePen,
                    new global::Avalonia.Point(x1, y1),
                    new global::Avalonia.Point(x2, y2));
            }
        }

        public void EndDrawLine()
        {
            batchLinePen = null;
        }

        #endregion

        #region Cell Text Rendering

        public void DrawCellText(Cell cell, SolidColor textColor, 
            DrawMode drawMode, double scale)
        {
            // Implement cell text rendering
            // This is called from the core rendering logic
        }

        public void UpdateCellRenderFont(Cell cell, 
            Core.UpdateFontReason reason)
        {
            // Update the cached font for the cell
        }

        public Size MeasureCellText(Cell cell, DrawMode drawMode, double scale)
        {
            // Measure cell text for layout calculations
            return new Size(0, 0);
        }

        #endregion

        #region Header Text

        public void BeginDrawHeaderText(double scale)
        {
            // Prepare for header text rendering
        }

        public void DrawHeaderText(string text, Brush brush, 
            Rectangle rect)
        {
            // Draw header text (row/column headers)
        }

        public void DrawLeadHeadArrow(Rectangle bounds, 
            SolidColor startColor, SolidColor endColor)
        {
            // Draw the corner arrow button
        }

        #endregion
    }
}

#endif
```

### 6. Platform Utilities

Create `ReoGrid/Avalonia/Platform.cs`:

```csharp
#if AVALONIA

using System;
using Avalonia;
using Avalonia.Input;
using unvell.ReoGrid.Interaction;

namespace unvell.ReoGrid.Rendering
{
    partial class PlatformUtility
    {
        internal static bool IsKeyDown(KeyCode key)
        {
            // Get keyboard state from Avalonia
            // Note: Avalonia doesn't have direct keyboard state query
            // May need to track state manually
            return false;
        }

        public static double GetDPI()
        {
            // Get DPI from visual
            // Default to 96 if cannot determine
            try
            {
                var scaling = TopLevel.GetTopLevel(
                    Application.Current?.ApplicationLifetime as 
                    global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?
                    .MainWindow?.RenderScaling ?? 1.0;
                return 96.0 * scaling;
            }
            catch
            {
                return 96.0;
            }
        }
    }
}

#endif
```

---

## Demo Application

### 7. Create Demo App

Create `DemoAvalonia/Program.cs`:

```csharp
using System;
using Avalonia;

namespace ReoGrid.DemoAvalonia
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
```

Create `DemoAvalonia/App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="ReoGrid.DemoAvalonia.App">
    <Application.Styles>
        <FluentTheme />
    </Application.Styles>
</Application>
```

Create `DemoAvalonia/MainWindow.axaml`:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:rg="clr-namespace:unvell.ReoGrid.Avalonia;assembly=unvell.ReoGrid.Avalonia"
        x:Class="ReoGrid.DemoAvalonia.MainWindow"
        Title="ReoGrid Avalonia Demo"
        Width="1024"
        Height="768">
    <Grid>
        <rg:ReoGridControl x:Name="grid" />
    </Grid>
</Window>
```

Create `DemoAvalonia/MainWindow.axaml.cs`:

```csharp
using Avalonia.Controls;
using unvell.ReoGrid.Avalonia;

namespace ReoGrid.DemoAvalonia
{
    public partial class MainWindow : Window
    {
        private ReoGridControl grid;

        public MainWindow()
        {
            InitializeComponent();
            grid = this.FindControl<ReoGridControl>("grid");
            
            // Initialize demo content
            InitializeDemoContent();
        }

        private void InitializeDemoContent()
        {
            // Add demo data, formulas, etc.
            var worksheet = grid.CurrentWorksheet;
            
            if (worksheet != null)
            {
                // Example: Set cell values
                worksheet["A1"] = "Hello";
                worksheet["B1"] = "Avalonia!";
                worksheet["A2"] = 123;
                worksheet["B2"] = 456;
            }
        }
    }
}
```

---

## Testing Checklist

### Basic Functionality
- [ ] Control renders correctly
- [ ] Mouse click selects cells
- [ ] Keyboard navigation works
- [ ] Cell editing functions
- [ ] Scrolling is smooth
- [ ] Resizing works

### Advanced Features
- [ ] Excel file import/export
- [ ] Formulas calculate correctly
- [ ] Charts render properly
- [ ] Printing works
- [ ] Custom cell types function
- [ ] Performance is acceptable

### Cross-Platform
- [ ] Windows tested
- [ ] macOS tested
- [ ] Linux tested
- [ ] High DPI support verified

---

## Performance Optimization Tips

1. **Use RenderTargetBitmap for caching**
   - Cache rendered cells that don't change
   - Invalidate cache only when needed

2. **Implement virtual scrolling**
   - Only render visible cells
   - Recycle cell renderers

3. **Batch rendering operations**
   - Group DrawLine calls
   - Use DrawingContext efficiently

4. **Profile with Avalonia DevTools**
   - Monitor render time
   - Check for unnecessary redraws

---

## Common Issues and Solutions

### Issue: Text rendering is blurry
**Solution**: Ensure pixel-aligned rendering, check DPI scaling

### Issue: Performance is slow
**Solution**: Implement caching, reduce invalidations, use Skia directly

### Issue: Mouse events not working
**Solution**: Ensure `Focusable = true`, check event handlers

### Issue: Fonts don't match WPF
**Solution**: Use same font families, adjust sizing calculations

---

## Next Steps

1. Complete the basic control implementation
2. Test on all target platforms
3. Implement missing features incrementally
4. Gather community feedback
5. Iterate and improve

For detailed workload assessment, see `AVALONIA_COMPATIBILITY_ASSESSMENT.md`.
