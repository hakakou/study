using Microsoft.Maui.Layouts;

namespace MauiStudyApp.CustomLayout;

public class CircularLayout : Layout
{
    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public static readonly BindableProperty RadiusProperty =
        BindableProperty.Create(nameof(Radius), typeof(double),
                                typeof(CircularLayout), 80d);

    protected override ILayoutManager CreateLayoutManager()
        => new CircularLayoutManager(this);
}

public sealed class CircularLayoutManager : ILayoutManager
{
    readonly CircularLayout parent;
    public CircularLayoutManager(CircularLayout p) => parent = p;

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        // Measure all visible children to populate DesiredSize
        for (int i = 0; i < parent.Count; i++)
        {
            var child = parent[i];
            if (child.Visibility != Visibility.Collapsed)
                child.Measure(double.PositiveInfinity, double.PositiveInfinity);
        }

        // Desired panel size: diameter (simple approach)
        double d = parent.Radius * 2;
        return new Size(
            double.IsFinite(parent.WidthRequest) ? parent.WidthRequest : d,
            double.IsFinite(parent.HeightRequest) ? parent.HeightRequest : d
        );
    }

    public Size ArrangeChildren(Rect bounds)
    {
        int count = parent.Count;
        if (count == 0) return bounds.Size;

        double r = parent.Radius;
        double cx = bounds.X + r; // center X
        double cy = bounds.Y + r; // center Y
        double step = Math.PI * 2 / count;

        for (int i = 0; i < count; i++)
        {
            var child = parent[i];
            if (child.Visibility == Visibility.Collapsed) continue;

            // Top-left so the child’s top-left sits on the circle
            double x = cx + r * Math.Cos(step * i);
            double y = cy + r * Math.Sin(step * i);

            child.Arrange(new Rect(
                x, y,
                child.DesiredSize.Width,
                child.DesiredSize.Height));
        }
        return bounds.Size;
    }
}
