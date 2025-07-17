using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MauiStudyApp2;

public static class EntrySelection
{
    public static readonly BindableProperty SelectAllOnFocusProperty =
        BindableProperty.CreateAttached(
            "SelectAllOnFocus",
            typeof(bool),
            typeof(EntrySelection),
            false,
            propertyChanged: OnSelectAllOnFocusChanged);

    public static bool GetSelectAllOnFocus(BindableObject view) =>
        (bool)view.GetValue(SelectAllOnFocusProperty);

    public static void SetSelectAllOnFocus(BindableObject view, bool value) =>
        view.SetValue(SelectAllOnFocusProperty, value);

    public static void OnSelectAllOnFocusChanged(
        BindableObject obj, object oldValue, object newValue)
    {
        if (obj is Entry entry)
        {
            if ((bool)newValue)
            {
                entry.Focused += OnEntryFocused;
            }
            else
            {
                entry.Focused -= OnEntryFocused;
            }
        }
    }

    private static async void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry entry && e.IsFocused)
        {
            // Delay to ensure the focus event is fully processed
            await Task.Delay(100);
            if (entry.IsFocused && !string.IsNullOrEmpty(entry.Text))
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text.Length;
            }
        }
    }
}

public class DoubleTapToZoomBehavior : Behavior<Image>
{
    public static readonly BindableProperty ScaleFactorProperty =
            BindableProperty.Create(
                nameof(ScaleFactor),
                typeof(double),
                typeof(DoubleTapToZoomBehavior),
                2.0);

    // Making the property bindable will allow you not only to set it when using
    // DoubleTapToZoomBehavior, but also to bind it to a view model or another control.

    public double ScaleFactor
    {
        get => (double)GetValue(ScaleFactorProperty);
        set => SetValue(ScaleFactorProperty, value);
    }
    protected override void OnAttachedTo(Image bindable)
    {
        base.OnAttachedTo(bindable);
        image = bindable;
        tapGestureRecognizer = new TapGestureRecognizer()
        {
            NumberOfTapsRequired = 2
        };

        tapGestureRecognizer.Tapped += OnImageDoubleTap;
        image.GestureRecognizers.Add(tapGestureRecognizer);
    }

    protected override void OnDetachingFrom(Image bindable)
    {
        base.OnDetachingFrom(bindable);
        if (tapGestureRecognizer != null)
        {
            tapGestureRecognizer.Tapped -= OnImageDoubleTap;
            image.GestureRecognizers.Remove(tapGestureRecognizer);
        }
        image = null;
        // We also set the image field to null in order to not
        // hold a reference to the parent Image object.
    }

    private void OnImageDoubleTap(object sender, TappedEventArgs e)
    {
        Point? tappedPoint = e.GetPosition(image);
        if (isZoomed)
        {
            image.ScaleTo(1);
            image.TranslateTo(0, 0);
            isZoomed = false;
        }
        else
        {
            double translateFactor = ScaleFactor - 1;
            double translateX = (image.Width / 2 - tappedPoint.Value.X) * translateFactor;
            double translateY = (image.Height / 2 - tappedPoint.Value.Y) * translateFactor;
            image.TranslateTo(translateX, translateY);
            image.ScaleTo(ScaleFactor);
            isZoomed = true;
        }
    }

    private bool isZoomed;
    private Image image;
    private TapGestureRecognizer tapGestureRecognizer;

    // Attached Property to easily add/remove the behavior in XAML  

    public static readonly BindableProperty AttachBehaviorProperty =
    BindableProperty.CreateAttached("AttachBehavior", typeof(bool),
        typeof(DoubleTapToZoomBehavior), false,
        propertyChanged: OnAttachBehaviorChanged);

    public static bool GetAttachBehavior(BindableObject view) =>
        (bool)view.GetValue(AttachBehaviorProperty);

    public static void SetAttachBehavior(BindableObject view, bool value) =>
        view.SetValue(AttachBehaviorProperty, value);

    static void OnAttachBehaviorChanged(BindableObject view, object oldValue, object newValue)
    {
        var img = view as Image;
        if (img == null)
            return;

        bool shouldAttach = (bool)newValue;

        if (shouldAttach)
        {
            img.Behaviors.Add(new DoubleTapToZoomBehavior());
        }
        else
        {
            var toRemove = img.Behaviors
                .FirstOrDefault(b => b is DoubleTapToZoomBehavior);

            if (toRemove != null)
                img.Behaviors.Remove(toRemove);
        }
    }
}