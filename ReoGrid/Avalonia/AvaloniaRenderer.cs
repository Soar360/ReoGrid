/*****************************************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * https://reogrid.net/
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Author: Jingwood <jingwood at unvell.com>
 *
 * Copyright (c) 2012-2025 Jingwood <jingwood at unvell.com>
 * Copyright (c) 2012-2025 UNVELL Inc. All rights reserved.
 * 
 ****************************************************************************/

#if AVALONIA

using System;
using Avalonia.Media;
using unvell.ReoGrid.Graphics;
using unvell.ReoGrid.Drawing.Text;
using Point = unvell.ReoGrid.Graphics.Point;
using Rectangle = unvell.ReoGrid.Graphics.Rectangle;
using Size = unvell.ReoGrid.Graphics.Size;

namespace unvell.ReoGrid.Rendering
{
	/// <summary>
	/// Avalonia-specific renderer implementing IRenderer interface
	/// </summary>
	internal class AvaloniaRenderer : AvaloniaGraphics, IRenderer
	{
		#region Running Focus Rectangle

		public void DrawRunningFocusRect(double x, double y, double w, double h, 
			SolidColor color, int runningOffset)
		{
			var pen = new Pen(resourceManager.GetBrush(color), 1.0)
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
			cappedLinePen = new Pen(resourceManager.GetBrush(color), width);
			
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
			batchLinePen = new Pen(resourceManager.GetBrush(color), width);
		}

		public void DrawLine(double x1, double y1, double x2, double y2)
		{
			if (batchLinePen != null)
			{
				PlatformGraphics.DrawLine(batchLinePen,
					new global::Avalonia.Point(x1, y1),
					new global::Avalonia.Point(x2, y2));
			}
			else
			{
				// Fallback to base implementation
				base.DrawLine(x1, y1, x2, y2, SolidColor.Black);
			}
		}

		public void EndDrawLine()
		{
			batchLinePen = null;
		}

		#endregion

		#region Cell Text Rendering

		public void DrawCellText(Cell cell, SolidColor textColor, DrawMode drawMode, double scale)
		{
			if (cell == null || string.IsNullOrEmpty(cell.DisplayText))
				return;

			try
			{
				var style = cell.InnerStyle;
				var rect = new Rectangle(cell.Left, cell.Top, cell.Width, cell.Height);

				// Get font
				string fontName = style.FontName ?? "Arial";
				double fontSize = (style.FontSize > 0 ? style.FontSize : 11) * scale;

				// Create formatted text
				var typeface = new Typeface(fontName);
				var brush = resourceManager.GetBrush(textColor);

				var formattedText = new FormattedText(
					cell.DisplayText,
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					typeface,
					fontSize,
					brush);

				// Apply text styling
				if ((style.Flag & PlainStyleFlag.FontStyleBold) != 0)
				{
					typeface = new Typeface(typeface.FontFamily, FontStyle.Normal, FontWeight.Bold);
				}
				if ((style.Flag & PlainStyleFlag.FontStyleItalic) != 0)
				{
					typeface = new Typeface(typeface.FontFamily, FontStyle.Italic, typeface.Weight);
				}

				// Calculate position based on alignment
				double x = rect.X;
				double y = rect.Y;

				var halign = style.HAlign;
				var valign = style.VAlign;

				switch (halign)
				{
					case ReoGridHorAlign.Center:
						x = rect.X + (rect.Width - formattedText.Width) / 2;
						break;
					case ReoGridHorAlign.Right:
						x = rect.X + rect.Width - formattedText.Width;
						break;
					case ReoGridHorAlign.General:
					case ReoGridHorAlign.Left:
					default:
						x = rect.X + 2; // Small padding
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
					case ReoGridVerAlign.General:
					case ReoGridVerAlign.Top:
					default:
						y = rect.Y + 1; // Small padding
						break;
				}

				// Draw text
				PlatformGraphics.DrawText(formattedText, new global::Avalonia.Point(x, y));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error drawing cell text: {ex.Message}");
			}
		}

		public void UpdateCellRenderFont(Cell cell, Core.UpdateFontReason reason)
		{
			// In Avalonia, fonts are created on-demand during rendering
			// This method is for compatibility but doesn't need implementation
		}

		public Size MeasureCellText(Cell cell, DrawMode drawMode, double scale)
		{
			if (cell == null || string.IsNullOrEmpty(cell.DisplayText))
				return new Size(0, 0);

			try
			{
				var style = cell.InnerStyle;
				string fontName = style.FontName ?? "Arial";
				double fontSize = (style.FontSize > 0 ? style.FontSize : 11) * scale;

				var typeface = new Typeface(fontName);
				var formattedText = new FormattedText(
					cell.DisplayText,
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					typeface,
					fontSize,
					Brushes.Black);

				return new Size(formattedText.Width, formattedText.Height);
			}
			catch
			{
				return new Size(0, 0);
			}
		}

		#endregion

		#region Header Text

		private double headerTextScale = 1.0;

		public void BeginDrawHeaderText(double scale)
		{
			headerTextScale = scale;
		}

		public void DrawHeaderText(string text, Brush brush, Rectangle rect)
		{
			if (string.IsNullOrEmpty(text)) return;

			var typeface = new Typeface("Arial");
			double fontSize = 11 * headerTextScale;

			var formattedText = new FormattedText(
				text,
				System.Globalization.CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				typeface,
				fontSize,
				brush)
			{
				TextAlignment = TextAlignment.Center
			};

			// Center the text
			double x = rect.X + (rect.Width - formattedText.Width) / 2;
			double y = rect.Y + (rect.Height - formattedText.Height) / 2;

			PlatformGraphics.DrawText(formattedText, new global::Avalonia.Point(x, y));
		}

		public void DrawLeadHeadArrow(Rectangle bounds, SolidColor startColor, SolidColor endColor)
		{
			// Draw the corner selector arrow (top-left corner of grid)
			var brush = new LinearGradientBrush
			{
				StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
				EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
				GradientStops = new GradientStops
				{
					new GradientStop(Color.FromArgb(startColor.A, startColor.R, startColor.G, startColor.B), 0),
					new GradientStop(Color.FromArgb(endColor.A, endColor.R, endColor.G, endColor.B), 1)
				}
			};

			var rect = new global::Avalonia.Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			PlatformGraphics.DrawRectangle(brush, null, rect);
		}

		#endregion

		#region Resource Management

		public Pen GetPen(SolidColor color)
		{
			return resourceManager.GetPen(color);
		}

		public void ReleasePen(Pen pen)
		{
			// Resource pooling - in Avalonia, GC handles this
		}

		public Brush GetBrush(SolidColor color)
		{
			return resourceManager.GetBrush(color);
		}

		public void ReleaseBrush(Brush brush)
		{
			// Resource pooling - in Avalonia, GC handles this
		}

		#endregion
	}
}

#endif
