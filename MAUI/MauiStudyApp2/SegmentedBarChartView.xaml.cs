
namespace MauiStudyApp2;

public partial class SegmentedBarChartView : ContentView
{
    public float Value
    {
        get => (float)GetValue(ValueProperty);
        set { SetValue(ValueProperty, value); }
    }

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create("Value",
            typeof(float),
            typeof(SegmentedBarChartView),
            defaultValue: 0f,
            propertyChanged: (b, o, n) => ((SegmentedBarChartView)b).OnValueChanged());

    void OnValueChanged()
    {
        ((BarChartDrawable)graphicsView.Drawable).Value = Value;
        graphicsView.Invalidate();
    }

    public SegmentedBarChartView() => InitializeComponent();
}

public class BarChartDrawable : IDrawable
{
    Color[] Palette = [Colors.LightGreen, Colors.Gold, Colors.Coral];
    float spacing = 5;
    float cornerRadius = 4;
    public float Value { get; set; } = 1;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float rectSize = dirtyRect.Height;
        int maxStep = (int)(dirtyRect.Width / (rectSize + spacing));
        int valueBasedSteps = (int)(maxStep * Value);

        for (int step = 0; step < valueBasedSteps; step++)
        {
            canvas.FillColor = Palette[Palette.Length * step / maxStep];
            canvas.FillRoundedRectangle(
                x: (rectSize + spacing) * step,
                y: 0,
                width: rectSize,
                height: rectSize,
                cornerRadius: cornerRadius);
        }
    }
}