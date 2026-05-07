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
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using unvell.ReoGrid.Interaction;

namespace unvell.ReoGrid.Rendering
{
	#region PlatformUtility
	partial class PlatformUtility
	{
		internal static bool IsKeyDown(KeyCode key)
		{
			// Avalonia doesn't provide direct keyboard state query
			// This would need to be tracked by the control
			return false;
		}

		private static double lastGetDPI = 0;

		public static double GetDPI()
		{
			if (lastGetDPI == 0)
			{
				try
				{
					// Try to get DPI from the main window
					if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
					{
						var mainWindow = desktop.MainWindow;
						if (mainWindow != null)
						{
							var scaling = mainWindow.RenderScaling;
							lastGetDPI = 96.0 * scaling;
						}
					}

					if (lastGetDPI == 0)
					{
						lastGetDPI = 96.0; // Default DPI
					}
				}
				catch
				{
					lastGetDPI = 96.0;
				}
			}

			return lastGetDPI;
		}
	}
	#endregion

	#region StaticResources
	partial class StaticResources
	{
		// Avalonia-specific static resources can be defined here
	}
	#endregion
}

namespace unvell.ReoGrid
{
	partial class Cell
	{
		/// <summary>
		/// Avalonia-specific formatted text cache
		/// </summary>
		[NonSerialized]
		internal Avalonia.Media.FormattedText formattedText;
	}
}

#endif
