using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiStudyApp2
{
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
}

public static class HtmlText
{
    public static readonly BindableProperty IsHtmlProperty =
        BindableProperty.CreateAttached("IsHtml", typeof(bool),
            typeof(HtmlText), false, propertyChanged: OnHtmlPropertyChanged);

    public static bool GetIsHtml(BindableObject view)
    {
        return (bool)view.GetValue(IsHtmlProperty);
    }

    public static void SetIsHtml(BindableObject view, bool value)
    {
        view.SetValue(IsHtmlProperty, value);
    }

    private static void OnHtmlPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Label label)
        {
        }
    }
}

public class FancyFrame : Frame
{
    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(
            nameof(CornerRadius),
            typeof(double),
            typeof(FancyFrame),
            0.0);

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}