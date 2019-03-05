﻿using System;
using System.ComponentModel;
using Android.Graphics;
using Android.Runtime;
using Android.Webkit;
using WView = Android.Webkit.WebView;

namespace Xamarin.Forms.Platform.Android
{
	public class FormsWebViewClient : WebViewClient
	{
		WebNavigationResult _navigationResult = WebNavigationResult.Success;
		WebViewRenderer _renderer;

		public FormsWebViewClient(WebViewRenderer renderer)
		{
			_renderer = renderer ?? throw new ArgumentNullException("renderer");
		}

		protected FormsWebViewClient(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnPageStarted(WView view, string url, Bitmap favicon)
		{
			if (_renderer?.Element == null)
				return;

			if (!_renderer.Loading && !_renderer.SendNavigating(url))
				view.StopLoading();
			else
				base.OnPageStarted(view, url, favicon);

			_renderer.Loading = false;
		}

		public override void OnPageFinished(WView view, string url)
		{
			if (_renderer?.Element == null || url == WebViewRenderer.AssetBaseUrl)
				return;

			var source = new UrlWebViewSource { Url = url };
			_renderer.IgnoreSourceChanges = true;
			_renderer.ElementController.SetValueFromRenderer(WebView.SourceProperty, source);
			_renderer.IgnoreSourceChanges = false;

			var args = new WebNavigatedEventArgs(WebNavigationEvent.NewPage, source, url, _navigationResult);

			_renderer.ElementController.SendNavigated(args);

			_renderer.UpdateCanGoBackForward();

			base.OnPageFinished(view, url);
		}

		[Obsolete("OnReceivedError is obsolete as of version 2.3.0. This method was deprecated in API level 23.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void OnReceivedError(WView view, ClientError errorCode, string description, string failingUrl)
		{
			_navigationResult = WebNavigationResult.Failure;
			if (errorCode == ClientError.Timeout)
				_navigationResult = WebNavigationResult.Timeout;
#pragma warning disable 618
			base.OnReceivedError(view, errorCode, description, failingUrl);
#pragma warning restore 618
		}

		public override void OnReceivedError(WView view, IWebResourceRequest request, WebResourceError error)
		{
			_navigationResult = WebNavigationResult.Failure;
			if (error.ErrorCode == ClientError.Timeout)
				_navigationResult = WebNavigationResult.Timeout;
			base.OnReceivedError(view, request, error);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				_renderer = null;
		}
	}
}