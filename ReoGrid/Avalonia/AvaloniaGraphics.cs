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
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using unvell.ReoGrid.Graphics;
using unvell.ReoGrid.Drawing.Text;
using Point = unvell.ReoGrid.Graphics.Point;
using Rectangle = unvell.ReoGrid.Graphics.Rectangle;
using Size = unvell.ReoGrid.Graphics.Size;
using AvaloniaPoint = Avalonia.Point;
using AvaloniaDrawingContext = Avalonia.Media.DrawingContext;

namespace unvell.ReoGrid.Rendering
{
	/// <summary>
	/// Avalonia implementation of IGraphics interface using Skia rendering
	/// </summary>
	internal class AvaloniaGraphics : IGraphics
	{
		protected ResourcePoolManager resourceManager = new ResourcePoolManager();

		public ResourcePoolManager ResourcePoolManager
		{
			get { return this.resourceManager; }
		}

		private AvaloniaDrawingContext dc = null;
		private Stack<Matrix> transformStack = new Stack<Matrix>();
		private Stack<Rect> clipStack = new Stack<Rect>();

		public AvaloniaGraphics()
		{
		}

		public AvaloniaDrawingContext PlatformGraphics 
		{ 
			get { return dc; } 
			set { this.dc = value; } 
		}

		#region Line Drawing

		public void DrawLine(double x1, double y1, double x2, double y2, SolidColor color)
		{
			var pen = resourceManager.GetPen(color);
			dc.DrawLine(pen, new AvaloniaPoint(x1, y1), new AvaloniaPoint(x2, y2));
		}

		public void DrawLine(double x1, double y1, double x2, double y2, SolidColor color, double width, LineStyles style)
		{
			var pen = resourceManager.GetPen(color, width, ToAvaloniaDashStyle(style));
			dc.DrawLine(pen, new AvaloniaPoint(x1, y1), new AvaloniaPoint(x2, y2));
		}

		public void DrawLine(Point startPoint, Point endPoint, SolidColor color)
		{
			DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, color);
		}

		public void DrawLine(Point startPoint, Point endPoint, SolidColor color, double width, LineStyles style)
		{
			DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, color, width, style);
		}

		public void DrawLine(Pen p, double x1, double y1, double x2, double y2)
		{
			dc.DrawLine(p, new AvaloniaPoint(x1, y1), new AvaloniaPoint(x2, y2));
		}

		public void DrawLine(Pen p, Point startPoint, Point endPoint)
		{
			dc.DrawLine(p, new AvaloniaPoint(startPoint.X, startPoint.Y), 
				new AvaloniaPoint(endPoint.X, endPoint.Y));
		}

		public void DrawLines(Point[] points, int start, int length, SolidColor color, double width, LineStyles style)
		{
			if (points == null || points.Length == 0) return;

			var pen = resourceManager.GetPen(color, width, ToAvaloniaDashStyle(style));
			
			for (int i = start; i < start + length - 1; i++)
			{
				var p1 = new AvaloniaPoint(points[i].X, points[i].Y);
				var p2 = new AvaloniaPoint(points[i + 1].X, points[i + 1].Y);
				dc.DrawLine(pen, p1, p2);
			}
		}

		#endregion

		#region Rectangle Drawing

		public void DrawRectangle(Rectangle rect, SolidColor color)
		{
			var pen = resourceManager.GetPen(color);
			var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
			dc.DrawRectangle(null, pen, avRect);
		}

		public void DrawRectangle(Rectangle rect, SolidColor color, double width, LineStyles lineStyle)
		{
			var pen = resourceManager.GetPen(color, width, ToAvaloniaDashStyle(lineStyle));
			var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
			dc.DrawRectangle(null, pen, avRect);
		}

		public void DrawRectangle(double x, double y, double width, double height, SolidColor color)
		{
			var pen = resourceManager.GetPen(color);
			var rect = new Rect(x, y, width, height);
			dc.DrawRectangle(null, pen, rect);
		}

		public void DrawRectangle(Pen p, Rectangle rect)
		{
			var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
			dc.DrawRectangle(null, p, avRect);
		}

		public void DrawRectangle(Pen p, double x, double y, double width, double height)
		{
			var rect = new Rect(x, y, width, height);
			dc.DrawRectangle(null, p, rect);
		}

		#endregion

		#region Rectangle Filling

		public void FillRectangle(HatchStyles style, SolidColor hatchColor, SolidColor bgColor, Rectangle rect)
		{
			FillRectangle(style, hatchColor, bgColor, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillRectangle(HatchStyles style, SolidColor hatchColor, SolidColor bgColor, 
			double x, double y, double width, double height)
		{
			// For now, use solid color. Hatch patterns could be implemented with drawing patterns
			var brush = resourceManager.GetBrush(bgColor);
			var rect = new Rect(x, y, width, height);
			dc.DrawRectangle(brush, null, rect);
		}

		public void FillRectangle(Rectangle rect, IColor color)
		{
			var brush = GetBrush(color);
			var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
			dc.DrawRectangle(brush, null, avRect);
		}

		public void FillRectangle(double x, double y, double width, double height, IColor color)
		{
			var brush = GetBrush(color);
			var rect = new Rect(x, y, width, height);
			dc.DrawRectangle(brush, null, rect);
		}

		public void FillRectangle(Brush b, double x, double y, double width, double height)
		{
			var rect = new Rect(x, y, width, height);
			dc.DrawRectangle(b, null, rect);
		}

		public void FillRectangleLinear(SolidColor startColor, SolidColor endColor, double angle, Rectangle rect)
		{
			// Calculate gradient direction based on angle
			double radians = angle * Math.PI / 180.0;
			double dx = Math.Cos(radians);
			double dy = Math.Sin(radians);

			var brush = new LinearGradientBrush
			{
				StartPoint = new RelativePoint(0.5 - dx / 2, 0.5 - dy / 2, RelativeUnit.Relative),
				EndPoint = new RelativePoint(0.5 + dx / 2, 0.5 + dy / 2, RelativeUnit.Relative),
				GradientStops = new GradientStops
				{
					new GradientStop(ToAvaloniaColor(startColor), 0),
					new GradientStop(ToAvaloniaColor(endColor), 1)
				}
			};

			var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
			dc.DrawRectangle(brush, null, avRect);
		}

		public void DrawAndFillRectangle(Rectangle rect, SolidColor lineColor, IColor fillColor)
		{
			DrawAndFillRectangle(rect, lineColor, fillColor, 1, LineStyles.Solid);
		}

		public void DrawAndFillRectangle(Rectangle rect, SolidColor lineColor, IColor fillColor, 
			double weight, LineStyles lineStyle)
		{
			var pen = resourceManager.GetPen(lineColor, weight, ToAvaloniaDashStyle(lineStyle));
			var brush = GetBrush(fillColor);
			var avRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
			dc.DrawRectangle(brush, pen, avRect);
		}

		#endregion

		#region Ellipse

		public void DrawEllipse(SolidColor color, Rectangle rectangle)
		{
			DrawEllipse(color, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}

		public void DrawEllipse(SolidColor color, double x, double y, double width, double height)
		{
			var pen = resourceManager.GetPen(color);
			var geometry = new EllipseGeometry(new Rect(x, y, width, height));
			dc.DrawGeometry(null, pen, geometry);
		}

		public void DrawEllipse(Pen pen, Rectangle rectangle)
		{
			var geometry = new EllipseGeometry(new Rect(rectangle.X, rectangle.Y, 
				rectangle.Width, rectangle.Height));
			dc.DrawGeometry(null, pen, geometry);
		}

		public void FillEllipse(IColor fillColor, Rectangle rectangle)
		{
			var brush = GetBrush(fillColor);
			var geometry = new EllipseGeometry(new Rect(rectangle.X, rectangle.Y, 
				rectangle.Width, rectangle.Height));
			dc.DrawGeometry(brush, null, geometry);
		}

		public void FillEllipse(Brush b, Rectangle rectangle)
		{
			var geometry = new EllipseGeometry(new Rect(rectangle.X, rectangle.Y, 
				rectangle.Width, rectangle.Height));
			dc.DrawGeometry(b, null, geometry);
		}

		public void FillEllipse(Brush b, double x, double y, double width, double height)
		{
			var geometry = new EllipseGeometry(new Rect(x, y, width, height));
			dc.DrawGeometry(b, null, geometry);
		}

		#endregion

		#region Polygon

		public void DrawPolygon(SolidColor color, double lineWidth, LineStyles lineStyle, params Point[] points)
		{
			if (points == null || points.Length < 3) return;

			var pen = resourceManager.GetPen(color, lineWidth, ToAvaloniaDashStyle(lineStyle));
			var geometry = CreatePolygonGeometry(points);
			dc.DrawGeometry(null, pen, geometry);
		}

		public void FillPolygon(IColor color, params Point[] points)
		{
			if (points == null || points.Length < 3) return;

			var brush = GetBrush(color);
			var geometry = CreatePolygonGeometry(points);
			dc.DrawGeometry(brush, null, geometry);
		}

		private PolylineGeometry CreatePolygonGeometry(Point[] points)
		{
			var avPoints = new List<AvaloniaPoint>();
			foreach (var pt in points)
			{
				avPoints.Add(new AvaloniaPoint(pt.X, pt.Y));
			}
			// Close the polygon
			avPoints.Add(avPoints[0]);
			
			return new PolylineGeometry(avPoints, true);
		}

		#endregion

		#region Path

		public void FillPath(IColor color, Geometry graphicsPath)
		{
			var brush = GetBrush(color);
			dc.DrawGeometry(brush, null, graphicsPath);
		}

		public void DrawPath(SolidColor color, Geometry graphicsPath)
		{
			var pen = resourceManager.GetPen(color);
			dc.DrawGeometry(null, pen, graphicsPath);
		}

		#endregion

		#region Image

		public void DrawImage(Bitmap image, double x, double y, double width, double height)
		{
			if (image != null)
			{
				var rect = new Rect(x, y, width, height);
				dc.DrawImage(image, rect);
			}
		}

		public void DrawImage(Bitmap image, Rectangle rect)
		{
			DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
		}

		#endregion

		#region Text

		public void DrawText(string text, string fontName, double size, SolidColor color, Rectangle rect,
			ReoGridHorAlign halign = ReoGridHorAlign.Center, ReoGridVerAlign valign = ReoGridVerAlign.Middle)
		{
			if (string.IsNullOrEmpty(text)) return;

			var typeface = new Typeface(fontName);
			var foreground = resourceManager.GetBrush(color);

			var formattedText = new FormattedText(
				text,
				System.Globalization.CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				typeface,
				size,
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

			dc.DrawText(formattedText, new AvaloniaPoint(x, y));
		}

		#endregion

		#region Transform

		public void ScaleTransform(double sx, double sy)
		{
			var matrix = Matrix.CreateScale(sx, sy);
			dc.PushTransform(matrix);
		}

		public void TranslateTransform(double x, double y)
		{
			var matrix = Matrix.CreateTranslation(x, y);
			dc.PushTransform(matrix);
		}

		public void RotateTransform(double angle)
		{
			var matrix = Matrix.CreateRotation(angle * Math.PI / 180.0);
			dc.PushTransform(matrix);
		}

		public void ResetTransform()
		{
			// Pop all transforms
			while (transformStack.Count > 0)
			{
				dc.Pop();
				transformStack.Pop();
			}
		}

		public void PushTransform()
		{
			transformStack.Push(Matrix.Identity);
		}

		public void PushTransform(Matrix t)
		{
			dc.PushTransform(t);
			transformStack.Push(t);
		}

		public Matrix PopTransform()
		{
			if (transformStack.Count > 0)
			{
				dc.Pop();
				return transformStack.Pop();
			}
			return Matrix.Identity;
		}

		#endregion

		#region Clipping

		public void PushClip(Rectangle clip)
		{
			var rect = new Rect(clip.X, clip.Y, clip.Width, clip.Height);
			dc.PushClip(rect);
			clipStack.Push(rect);
		}

		public void PopClip()
		{
			if (clipStack.Count > 0)
			{
				dc.Pop();
				clipStack.Pop();
			}
		}

		#endregion

		#region Properties

		public bool IsAntialias { get; set; } = true;

		#endregion

		#region Reset

		public void Reset()
		{
			// Clean up any pending state
			while (transformStack.Count > 0)
			{
				dc.Pop();
				transformStack.Pop();
			}
			while (clipStack.Count > 0)
			{
				dc.Pop();
				clipStack.Pop();
			}
		}

		#endregion

		#region Helper Methods

		private Brush GetBrush(IColor color)
		{
			if (color is SolidColor solidColor)
			{
				return resourceManager.GetBrush(solidColor);
			}
			return Brushes.Black;
		}

		private Color ToAvaloniaColor(SolidColor color)
		{
			return Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		private DashStyle ToAvaloniaDashStyle(LineStyles style)
		{
			switch (style)
			{
				case LineStyles.Solid:
					return DashStyle.Solid;
				case LineStyles.Dash:
					return DashStyle.Dash;
				case LineStyles.Dot:
					return DashStyle.Dot;
				case LineStyles.DashDot:
					return DashStyle.DashDot;
				case LineStyles.DashDotDot:
					return DashStyle.DashDotDot;
				default:
					return DashStyle.Solid;
			}
		}

		#endregion
	}
}

#endif
