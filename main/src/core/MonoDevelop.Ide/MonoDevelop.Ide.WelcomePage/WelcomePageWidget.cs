// 
// WelcomePageFallbackWidget.cs
// 
// Author:
//   Scott Ellington
//   Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
// Copyright (c) 2005 Scott Ellington
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Xml;
using System.Linq;
using Gdk;
using Gtk;
using Mono.Addins;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Desktop;
using System.Reflection;
using System.Xml.Linq;
using MonoDevelop.Components;

namespace MonoDevelop.Ide.WelcomePage
{
	public class WelcomePageWidget : Gtk.EventBox
	{
		public Gdk.Pixbuf LogoImage { get; set; }
		public int LogoHeight { get; set; }
		public Gdk.Pixbuf TopBorderImage { get; set; }
		public Gdk.Pixbuf BackgroundImage { get; set; }
		public string BackgroundColor { get; set; }

		public bool ShowScrollbars { get; set; }

		public WelcomePageWidget ()
		{
			ShowScrollbars = true;
			VisibleWindow = false;

			BackgroundColor = "white";
			LogoHeight = 90;

			var background = new WelcomePageWidgetBackground ();
			background.Owner = this;
			var mainAlignment = new Gtk.Alignment (0f, 0f, 1f, 1f);
			background.Add (mainAlignment);

			BuildContent (mainAlignment);

			if (ShowScrollbars) {
				var scroller = new ScrolledWindow ();
				scroller.AddWithViewport (background);
				((Gtk.Viewport)scroller.Child).ShadowType = ShadowType.None;
				scroller.ShadowType = ShadowType.None;
				scroller.FocusChain = new Widget[] { background };
				scroller.Show ();
				Add (scroller);
			} else
				this.Add (background);

			if (LogoImage != null) {
				var logoHeight = LogoHeight;
				mainAlignment.SetPadding ((uint)(logoHeight + Styles.WelcomeScreen.Spacing), 0, (uint)Styles.WelcomeScreen.Spacing, 0);
			}

			ShowAll ();

			IdeApp.Workbench.GuiLocked += OnLock;
			IdeApp.Workbench.GuiUnlocked += OnUnlock;
		}

		protected override void OnRealized ()
		{
			Gdk.Color color = Gdk.Color.Zero;
			if (!Gdk.Color.Parse (BackgroundColor, ref color))
				color = Style.White;
			ModifyBg (StateType.Normal, color);

			base.OnRealized ();
		}

		void OnLock (object s, EventArgs a)
		{
			Sensitive = false;
		}
		
		void OnUnlock (object s, EventArgs a)
		{
			Sensitive = true;
		}
		
		protected virtual void BuildContent (Container parent)
		{
		}

		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			IdeApp.Workbench.GuiLocked -= OnLock;
			IdeApp.Workbench.GuiUnlocked -= OnUnlock;
		}

		class WelcomePageWidgetBackground : Gtk.EventBox
		{
			public WelcomePageWidget Owner { get; set; }

			protected override bool OnExposeEvent (EventExpose evnt)
			{
				//draw the background

				if (Owner.BackgroundImage != null) {
					var gc = Style.BackgroundGC (State);
					var height = Owner.BackgroundImage.Height;
					var width = Owner.BackgroundImage.Width;
					for (int y = Allocation.Y; y < Allocation.Bottom; y += height) {
						if (evnt.Region.RectIn (new Gdk.Rectangle (Allocation.X, y, Allocation.Width, height)) == OverlapType.Out)
							continue;
						for (int x = Allocation.X; x < Allocation.Right && x < evnt.Area.Right; x += width) {
							if (x + width < evnt.Area.X)
								continue;
							evnt.Window.DrawPixbuf (gc, Owner.BackgroundImage, 0, 0, x, y, width, height, RgbDither.None, 0, 0);
						}
					}
				}

				if (Owner.LogoImage != null) {
					var gc = Style.BackgroundGC (State);
					var lRect = new Rectangle (Allocation.X, Allocation.Y, Owner.LogoImage.Width, Owner.LogoImage.Height);
					if (evnt.Region.RectIn (lRect) != OverlapType.Out)
						evnt.Window.DrawPixbuf (gc, Owner.LogoImage, 0, 0, lRect.X, lRect.Y, lRect.Width, lRect.Height, RgbDither.None, 0, 0);
					
					var bgRect = new Rectangle (Allocation.X + Owner.LogoImage.Width, Allocation.Y, Allocation.Width - Owner.LogoImage.Width, Owner.TopBorderImage.Height);
					if (evnt.Region.RectIn (bgRect) != OverlapType.Out)
						for (int x = bgRect.X; x < bgRect.Right; x += Owner.TopBorderImage.Width)
							evnt.Window.DrawPixbuf (gc, Owner.TopBorderImage, 0, 0, x, bgRect.Y, Owner.TopBorderImage.Width, bgRect.Height, RgbDither.None, 0, 0);
				}
				
				foreach (Widget widget in Children)
					PropagateExpose (widget, evnt);
				
				return true;
			}
		}
	}
}
