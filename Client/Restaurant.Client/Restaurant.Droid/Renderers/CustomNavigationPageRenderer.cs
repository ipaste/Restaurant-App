﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Com.Mikepenz.Actionitembadge.Library;
using Com.Mikepenz.Actionitembadge.Library.Utils;
using Restaurant.Droid.Providers;
using Restaurant.Droid.Renderers;
using Restaurant.Mobile.UI.Controls;
using Restaurant.Mobile.UI.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using AToolbar = Android.Support.V7.Widget.Toolbar;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomNavigationPageRenderer))]

namespace Restaurant.Droid.Renderers
{
    public class CustomNavigationPageRenderer : NavigationPageRenderer
    {
        private readonly ColorProvider _colorProvider;
        private readonly DrawableProvider _drawableProvider;
        private bool _disposed;

        private int _statusBarHeight = -1;
        private AToolbar _toolbar;
        private ToolbarTracker _toolbarTracker;

        public CustomNavigationPageRenderer()
        {
            _colorProvider = new ColorProvider();
            _drawableProvider = new DrawableProvider();
        }

        private IPageController PageController => Element;

        protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
        {
            base.OnElementChanged(e);

            var memberInfo = typeof(CustomNavigationPageRenderer).BaseType;
            if (memberInfo != null)
            {
                var field = memberInfo.GetField(nameof(_toolbar), BindingFlags.Instance | BindingFlags.NonPublic);
                var toolBarTrackerField =
                    memberInfo.GetField(nameof(_toolbarTracker), BindingFlags.Instance | BindingFlags.NonPublic);

                _toolbar = field?.GetValue(this) as AToolbar;
                _toolbarTracker = toolBarTrackerField?.GetValue(this) as ToolbarTracker;
                if (_toolbarTracker != null)
                    _toolbarTracker.CollectionChanged += ToolbarTracker_CollectionChanged;
            }

            UpdateMenu(true);

            if ((int) Build.VERSION.SdkInt >= 21)
            {
                var navPage = Element;
                SetThemeColors(navPage.CurrentPage);
            }
        }

        private void ToolbarTracker_CollectionChanged(object sender, EventArgs e)
        {
            UpdateMenu(true);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
                if ((int) Build.VERSION.SdkInt >= 21)
                {
                    var navPage = Element;
                    SetThemeColors(navPage.CurrentPage);
                }
        }

        private void SetThemeColors(Page page)
        {
            if (Context is Activity context)
                if (page is ITransparentActionBarPage transparentPage && transparentPage.IsTransparentActionBar)
                {
                    context.Window.DecorView.SystemUiVisibility =
                        (StatusBarVisibility) (SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable);
                    context.Window.SetStatusBarColor(Color.Transparent.ToAndroid());
                    _toolbar.SetBackgroundColor(Color.Transparent.ToAndroid());
                }
                else
                {
                    context.Window.DecorView.SystemUiVisibility = StatusBarVisibility.Visible;
                    context.Window.SetStatusBarColor(_colorProvider.GetPrimaryDarkColor(context));
                    _toolbar.SetBackgroundColor(_colorProvider.GetPimaryColor(context));
                }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (!(Element.CurrentPage is ITransparentActionBarPage page))
                return;

            if (page.IsTransparentActionBar && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                LayoutBehindTheToolbar(l, t, r, b);
        }

        private void LayoutBehindTheToolbar(int l, int t, int r, int b)
        {
            var bar = _toolbar;
            var barHeight = ActionBarHeight();
            var statusBarHeight = GetStatusBarHeight();
            var containerHeight = b - t;

            PageController.ContainerArea =
                new Rectangle(0, 0, Context.FromPixels(r - l), Context.FromPixels(containerHeight));
            Element.ForceLayout();

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (child is AToolbar)
                    bar.Layout(0, statusBarHeight, r - l, barHeight + statusBarHeight);

                var pageContainer = child.GetType().GetProperty("Child")?.GetValue(child) as IVisualElementRenderer;

                if (!(pageContainer?.Element is Page childPage))
                    return;

                var childHasNavBar = NavigationPage.GetHasNavigationBar(childPage);

                if (childHasNavBar)
                    child.Layout(0, 0, r, b);
            }
        }

        private void UpdateMenu(bool init = false, string propertyName = null)
        {
            if (_disposed)
                return;


            var bar = _toolbar;
            var menu = bar.Menu;

            if (init)
                UpdateToolbarItems(menu);

            UpdateBadgeToolbarItem(menu, propertyName);
        }

        private void UpdateToolbarItems(IMenu menu)
        {
            foreach (var item in _toolbarTracker.ToolbarItems)
                item.PropertyChanged -= ToolbarItemPropertyChanged;

            menu.Clear();

            foreach (var item in _toolbarTracker.ToolbarItems)
            {
                IMenuItemController controller = item;
                item.PropertyChanged += ToolbarItemPropertyChanged;
                if (item.Order == ToolbarItemOrder.Secondary)
                {
                    var menuItem = menu.Add(item.Text);
                    menuItem.SetEnabled(controller.IsEnabled);
                    menuItem.SetOnMenuItemClickListener(new MenuClickListener(controller.Activate));
                }
                else
                {
                    IMenuItem menuItem;
                    if (item is BadgeToolbarItem badgeMenuItem)
                        menuItem = menu.Add(0, badgeMenuItem.UniqId, 0, badgeMenuItem.Text);
                    else
                        menuItem = menu.Add(item.Text);

                    var icon = item.Icon;
                    if (!string.IsNullOrEmpty(icon))
                    {
                        var iconDrawable = _drawableProvider.GetFormsDrawable(Context.Resources, icon);
                        if (iconDrawable != null)
                            menuItem.SetIcon(iconDrawable);
                    }
                    menuItem.SetEnabled(controller.IsEnabled);
                    menuItem.SetShowAsAction(ShowAsAction.Always);
                    menuItem.SetOnMenuItemClickListener(new MenuClickListener(controller.Activate));
                }
            }
        }

        private void UpdateBadgeToolbarItem(IMenu menu, string propertyName)
        {
            foreach (var item in _toolbarTracker.ToolbarItems.OfType<BadgeToolbarItem>())
            {
                IMenuItemController controller = item;
                item.PropertyChanged -= ToolbarItemPropertyChanged;
                item.PropertyChanged += ToolbarItemPropertyChanged;

                if (item.HasInitialized && propertyName == BadgeToolbarItem.BadgeTextProperty.PropertyName)
                {
                    var menuItem1 = menu.FindItem(item.UniqId);
                    ActionItemBadge.Update(menuItem1, item.BadgeText);
                }
                else
                {
                    menu.RemoveItem(item.UniqId);
                    var menuItem = menu.Add(0, item.UniqId, 0, item.Text);
                    var icon = item.Icon;
                    if (!string.IsNullOrEmpty(icon))
                    {
                        var iconDrawable = _drawableProvider.GetFormsDrawable(Context.Resources, icon);
                        if (iconDrawable != null)
                            menuItem.SetIcon(iconDrawable);

                        var color = item.BadgeColor.ToAndroid();
                        var colorPressed = item.BadgePressedColor.ToAndroid();
                        var textColor = item.BadgeTextColor.ToAndroid();

                        var badgeStyle = new BadgeStyle(BadgeStyle.Style.Default,
                            Resource.Layout.menu_action_item_badge,
                            color,
                            colorPressed,
                            textColor);

                        var activity = (FormsAppCompatActivity) Context;
                        ActionItemBadge.Update(activity, menuItem, iconDrawable, badgeStyle, item.BadgeText);
                        menuItem.ActionView.Click += (_, __) => controller.Activate();
                        menuItem.SetEnabled(controller.IsEnabled);
                        menuItem.SetShowAsAction(ShowAsAction.Always);
                        item.HasInitialized = true;
                    }
                }
            }
        }

        private void ToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BadgeToolbarItem.BadgeTextProperty.PropertyName
                || e.PropertyName == BadgeToolbarItem.BadgeTextColorProperty.PropertyName
                || e.PropertyName == BadgeToolbarItem.BadgeColorProperty.PropertyName)
                UpdateMenu(false, e.PropertyName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
                _disposed = true;
            base.Dispose(disposing);
        }

        private int ActionBarHeight()
        {
            var attr = Resource.Attribute.actionBarSize;

            int actionBarHeight;
            using (var tv = new TypedValue())
            {
                actionBarHeight = 0;
                if (Context.Theme.ResolveAttribute(attr, tv, true))
                    actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, Resources.DisplayMetrics);
            }

            //if (actionBarHeight <= 0)
            //	return Device.Info.CurrentOrientation.IsPortrait() ? (int)Context.ToPixels(56) : (int)Context.ToPixels(48);

            return actionBarHeight;
        }

        private int GetStatusBarHeight()
        {
            if (_statusBarHeight >= 0)
                return _statusBarHeight;

            var result = 0;
            var resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
                result = Resources.GetDimensionPixelSize(resourceId);
            return _statusBarHeight = result;
        }
    }
}