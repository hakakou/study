using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading;

namespace MauiStudyApp2;

public partial class Page3 : ContentPage
{
    readonly Kernel kernel;

    public Page3(Kernel kernel)
    {
        this.kernel = kernel;
        InitializeComponent();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        KernelArguments arguments = new(settings);

        var r = await kernel.InvokePromptAsync("Draw a happy face within 500,500 size. Use up to 30 shapes with various colors, strokes, sizes. Colors are in the #hex format", arguments);
        result.Text = r.ToString();
        graphicsView.Invalidate();
    }
}

public class CanvasRequest
{
    public static DrawRectangle[] Commands { get; set; } = [];
}


public class AIDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        foreach (var command in CanvasRequest.Commands)
        {
            if (command is DrawRectangle rect)
            {
                canvas.FillColor = Color.FromArgb(rect.FillColor);
                canvas.StrokeColor = Color.FromArgb(rect.StrokeColor);
                canvas.StrokeSize = rect.StrokeSize;
                canvas.FillRectangle(rect.X, rect.Y, rect.Width, rect.Height);
                canvas.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }
        }
    }
}

public class Drawer(CanvasRequest canvasRequest)
{
    [KernelFunction]
    public void Draw(DrawRectangle[] commands)
    {
        CanvasRequest.Commands = commands;
    }
}

public class Draw
{
}

public class DrawRectangle : Draw
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string FillColor { get; set; }
    public string StrokeColor { get; set; }
    public float StrokeSize { get; set; }
}


public class DemoDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        // Background
        canvas.FillColor = Color.FromArgb("#F5F5F5");
        canvas.FillRectangle(dirtyRect);

        // 1. Gradient Circle
        var gradientPaint = new RadialGradientPaint
        {
            StartColor = Colors.Purple,
            EndColor = Colors.Pink
        };
        canvas.SetFillPaint(gradientPaint, new RectF(20, 20, 80, 80));
        canvas.FillCircle(60, 60, 40);

        // 2. Rounded Rectangle with Shadow
        canvas.SaveState();
        canvas.SetShadow(new SizeF(2, 2), 5, Colors.Gray.WithAlpha(0.5f));
        canvas.FillColor = Colors.Orange;
        canvas.FillRoundedRectangle(120, 20, 80, 60, 10);
        canvas.RestoreState();

        // 3. Star Path
        canvas.StrokeColor = Colors.Gold;
        canvas.FillColor = Colors.Yellow;
        canvas.StrokeSize = 2;
        var starPath = new PathF();
        DrawStar(starPath, 250, 50, 35);
        canvas.FillPath(starPath);
        canvas.DrawPath(starPath);

        // 4. Linear Gradient Rectangle
        var linearGradient = new LinearGradientPaint
        {
            StartPoint = new Point(20, 120),
            EndPoint = new Point(100, 120),
            GradientStops = new[]
            {
                new PaintGradientStop(0, Colors.Blue),
                new PaintGradientStop(0.5f, Colors.Cyan),
                new PaintGradientStop(1, Colors.Green)
            }
        };
        canvas.SetFillPaint(linearGradient, new RectF(20, 120, 80, 60));
        canvas.FillRectangle(20, 120, 80, 60);

        // 5. Dashed Line Pattern
        canvas.StrokeColor = Colors.DarkBlue;
        canvas.StrokeSize = 3;
        canvas.StrokeDashPattern = new float[] { 10, 5 };
        canvas.DrawLine(120, 150, 200, 120);
        canvas.StrokeDashPattern = null;

        // 6. Bezier Curve
        var curvePath = new PathF();
        curvePath.MoveTo(220, 120);
        curvePath.CurveTo(240, 100, 260, 180, 280, 160);
        canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = 2;
        canvas.DrawPath(curvePath);

        // 7. Text with different styles
        canvas.FontColor = Colors.DarkSlateGray;
        canvas.FontSize = 14;
        canvas.DrawString("ICanvas Demo", 20, 210, HorizontalAlignment.Left);

        canvas.FontColor = Colors.Purple;
        canvas.FontSize = 18;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.DrawString("Bold Text", 20, 235, HorizontalAlignment.Left);

        // 8. Clipping Region (Circle)
        canvas.SaveState();
        var clipPath = new PathF();
        clipPath.AddArc(180, 220, 230, 270, 0, 360, true);
        canvas.ClipPath(clipPath);
        canvas.FillColor = Colors.Coral;
        canvas.FillRectangle(155, 195, 100, 100);
        canvas.RestoreState();

        // 9. Triangle with Stroke
        var trianglePath = new PathF();
        trianglePath.MoveTo(270, 200);
        trianglePath.LineTo(300, 250);
        trianglePath.LineTo(240, 250);
        trianglePath.Close();
        canvas.FillColor = Colors.Teal;
        canvas.StrokeColor = Colors.DarkSlateGray;
        canvas.StrokeSize = 2;
        canvas.FillPath(trianglePath);
        canvas.DrawPath(trianglePath);

        // 10. Pattern with multiple shapes
        for (int i = 0; i < 5; i++)
        {
            float x = 20 + i * 20;
            float y = 280;
            canvas.FillColor = Color.FromHsla(i * 0.2, 0.8, 0.6);
            canvas.FillEllipse(x, y, 15, 25);
        }

        // 11. Concentric circles with alpha
        for (int i = 3; i > 0; i--)
        {
            canvas.FillColor = Colors.Blue.WithAlpha(0.2f * i);
            canvas.FillCircle(180, 300, i * 15);
        }

        // 12. Image pattern effect with rectangles
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                float hue = (i * 6 + j) / 18f;
                canvas.FillColor = Color.FromHsla(hue, 0.7, 0.5);
                canvas.FillRectangle(220 + i * 12, 280 + j * 12, 10, 10);
            }
        }
    }

    private void DrawStar(PathF path, float centerX, float centerY, float radius)
    {
        float innerRadius = radius * 0.4f;
        path.MoveTo(centerX, centerY - radius);

        for (int i = 1; i <= 10; i++)
        {
            float angle = (float)(i * Math.PI / 5 - Math.PI / 2);
            float r = i % 2 == 0 ? radius : innerRadius;
            float x = centerX + r * (float)Math.Cos(angle);
            float y = centerY + r * (float)Math.Sin(angle);
            path.LineTo(x, y);
        }

        path.Close();
    }
}

