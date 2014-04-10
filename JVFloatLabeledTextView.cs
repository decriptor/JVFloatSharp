//  The MIT License (MIT)
//
//  Copyright (c) 2014 Stephen Shaw
//  Original implementation by Jared Verdi
//	https://github.com/jverdi/JVFloatLabeledTextField
//  Original Concept by Matt D. Smith
//  http://dribbble.com/shots/1254439--GIF-Mobile-Form-Interaction?list=users
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
//  the Software, and to permit persons to whom the Software is furnished to do so,
//  subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//  FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//  COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//  IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//  CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace JVFloatSharp
{
	public class JVFloatLabeledTextView : UITextView
	{
		private const float kFloatingLabelShowAnimationDuration = 0.3f;
		private const float kFloatingLabelHideAnimationDuration = 0.3f;


		private string _placeholder;
		private UILabel _placeholderLabel;


		private float _startingTextContainerInsetTop;

		private float _floatingLabelYPadding = 0.0f;

		private UILabel _floatingLabel;
		private UIFont _floatingLabelFont;
		private UIColor _floatingLabelTextColor;

		private bool _animateEvenIfNotFirstResponder;
		 

		public JVFloatLabeledTextView (RectangleF frame) : base (frame)
		{
			CommonInit ();
		}

		public JVFloatLabeledTextView (IntPtr handle) : base (handle)
		{
			CommonInit ();
		}

		void CommonInit ()
		{
			_startingTextContainerInsetTop = TextContainerInset.Top;

			_placeholderLabel = new UILabel ();
			_placeholderLabel.Font = Font;
			_placeholderLabel.Text = _placeholder;
			_placeholderLabel.Lines = 0;
			_placeholderLabel.LineBreakMode = UILineBreakMode.WordWrap;
			_placeholderLabel.BackgroundColor = UIColor.Clear;
			_placeholderLabel.TextColor = JVFloatLabeledTextView.DefaultiOSPlaceholderColor ();
			InsertSubview (_placeholderLabel, 0);

			_floatingLabel = new UILabel ();
			_floatingLabel.Alpha = 0.0f;
			AddSubview (_floatingLabel);

			// some basic default fonts/colors
			_floatingLabel.Font = UIFont.BoldSystemFontOfSize (12.0f);
			_floatingLabelTextColor = UIColor.Gray;
			_animateEvenIfNotFirstResponder = false;

			NSNotificationCenter.DefaultCenter.AddObserver (UITextView.TextDidChangeNotification, TextDidChange, this);
			NSNotificationCenter.DefaultCenter.AddObserver (UITextView.TextDidBeginEditingNotification, LayoutSubviews, this);
			NSNotificationCenter.DefaultCenter.AddObserver (UITextView.TextDidEndEditingNotification, LayoutSubviews, this);
		}

		protected override void Dispose (bool disposing)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver (this, UITextView.TextDidChangeNotification, this);
			NSNotificationCenter.DefaultCenter.RemoveObserver (this, UITextView.TextDidBeginEditingNotification, this);
			NSNotificationCenter.DefaultCenter.RemoveObserver (this, UITextView.TextDidEndEditingNotification, this);

			base.Dispose (disposing);
		}

		void setPlaceholder (string placeholder)
		{
			_placeholder = placeholder;
			_placeholderLabel.Text = placeholder;
			_placeholderLabel.SizeToFit ();

			_floatingLabel.Text = placeholder;
			_floatingLabel.SizeToFit ();
		}

		void setPlaceholder (string placeholder, string floatingTitle)
		{
			_placeholder = placeholder;
			_placeholderLabel.Text = placeholder;
			_placeholderLabel.SizeToFit ();

			_floatingLabel.Text = floatingTitle;
			_placeholderLabel.SizeToFit ();
		}

		void LayoutSubviews (NSNotification notification)
		{
			LayoutSubviews ();
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			AdjustTextContainerInsetTop ();

			RectangleF textRect = TextRect ();

			_placeholderLabel.Frame = new RectangleF (
				textRect.X,
				textRect.Y,
				_placeholderLabel.Frame.Size.Width,
				_placeholderLabel.Frame.Size.Height);

			SetLabelOriginForTextAlignment ();

			if (_floatingLabelFont != null)
			{
				_floatingLabel.Font = _floatingLabelFont;
			}

			bool firstResponder = IsFirstResponder;
			_floatingLabel.TextColor = (firstResponder && Text != null && Text.Length > 0) ? getLabelActiveColor () : _floatingLabelTextColor;

			if (!string.IsNullOrEmpty (Text))
			{
				HideFloatingLabel (firstResponder);
			}
			else
			{
				ShowFloatingLabel (firstResponder);
			}
		}

		UIColor getLabelActiveColor ()
		{
			return TintColor ?? UIColor.Blue;
		}

		void ShowFloatingLabel (bool animated)
		{
			Action showBlock = () => {
				_floatingLabel.Alpha = 1.0f;
				_floatingLabel.Frame = new RectangleF (
					_floatingLabel.Frame.X,
					2.0f,
					_floatingLabel.Frame.Size.Width,
					_floatingLabel.Frame.Size.Height);
			};

			if (animated || _animateEvenIfNotFirstResponder)
			{
				UIView.Animate (0.3f, 0.0f,
					UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseOut,
					new NSAction (showBlock),
					null);
			}
			else
			{
				showBlock ();
			}
		}

		void HideFloatingLabel (bool animated)
		{
			Action hideBlock = () => {
				_floatingLabel.Alpha = 0.0f;
				_floatingLabel.Frame = new RectangleF (
					_floatingLabel.Frame.X,
					_floatingLabel.Font.LineHeight + _floatingLabelYPadding,
					_floatingLabel.Frame.Size.Width,
					_floatingLabel.Frame.Size.Height);
			};

			if (animated || _animateEvenIfNotFirstResponder)
			{
				UIView.Animate (0.3f, 0.0f,
					UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn,
					new NSAction (hideBlock),
					null);
			}
			else
			{
				hideBlock ();
			}
		}

		void AdjustTextContainerInsetTop ()
		{
			TextContainerInset = new UIEdgeInsets (_startingTextContainerInsetTop + _floatingLabel.Font.LineHeight + _floatingLabelYPadding,
				TextContainerInset.Left, TextContainerInset.Bottom, TextContainerInset.Right);
		}

		void SetLabelOriginForTextAlignment ()
		{
			float floatingLabelOriginX = TextRect ().X;
			float placeholderLabelOriginX = floatingLabelOriginX;

			if (TextAlignment == UITextAlignment.Center)
			{
				floatingLabelOriginX = (Frame.Size.Width / 2) - (_floatingLabel.Frame.Size.Width / 2);
				placeholderLabelOriginX = (Frame.Size.Width / 2) - (_placeholderLabel.Frame.Size.Width / 2);
			}
			else if (TextAlignment == UITextAlignment.Right)
			{
				floatingLabelOriginX = (Frame.Size.Width - _floatingLabel.Frame.Size.Width);
				placeholderLabelOriginX = (Frame.Size.Width - _placeholderLabel.Frame.Size.Width - TextContainerInset.Right);
			}

			_floatingLabel.Frame = new RectangleF (
				floatingLabelOriginX,
				_floatingLabel.Frame.Y,
				_floatingLabel.Frame.Size.Width,
				_floatingLabel.Frame.Size.Height);

			_placeholderLabel.Frame = new RectangleF (placeholderLabelOriginX, _placeholderLabel.Frame.Y,
				_placeholderLabel.Frame.Size.Width, _placeholderLabel.Frame.Size.Height);
		}

		RectangleF TextRect ()
		{
			var rect = ContentInset.InsetRect (Bounds);

			if (TextContainer != null) {
				rect.X += TextContainer.LineFragmentPadding;
				rect.Y += TextContainerInset.Top;
			}

			return rect;
		}

		void setFloatingLabelFont (UIFont floatingLabelFont)
		{
			_floatingLabelFont = floatingLabelFont;
			_floatingLabel.Font = _floatingLabelFont ?? UIFont.BoldSystemFontOfSize (12.0f);
			_placeholder = _placeholder; // Force the label to lay itself out with the new font.
		}

		public static UIColor DefaultiOSPlaceholderColor ()
		{
			return UIColor.FromWhiteAlpha (0.702f, 1.0f);
		}

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;

				_placeholderLabel.Alpha = (Text.Length > 0) ? 0.0f : 1.0f;
				LayoutSubviews ();
			}
		}

		public override UITextAlignment TextAlignment
		{
			get
			{
				return base.TextAlignment;
			}
			set
			{
				base.TextAlignment = value;
				SetNeedsLayout ();
			}
		}

		public override UIFont Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
				_placeholderLabel.Font = Font;
				LayoutSubviews ();
			}
		}

		void TextDidChange (NSNotification notification)
		{
			_placeholderLabel.Alpha = (Text.Length > 0) ? 0.0f : 1.0f;
			LayoutSubviews ();
		}

		public override string AccessibilityLabel
		{
			get
			{
				return (!string.IsNullOrEmpty (Text)) ? string.Format ("{0} {1}", _floatingLabel.AccessibilityLabel, Text) : _floatingLabel.AccessibilityLabel;
			}
			set
			{
				base.AccessibilityLabel = value;
			}
		}
	}
}
