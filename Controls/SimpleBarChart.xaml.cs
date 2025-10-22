using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AuroraInvoice.Controls;

public partial class SimpleBarChart : UserControl
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(Dictionary<string, decimal>), typeof(SimpleBarChart),
            new PropertyMetadata(null, OnDataChanged));

    public Dictionary<string, decimal>? Data
    {
        get => (Dictionary<string, decimal>?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public SimpleBarChart()
    {
        InitializeComponent();
        Loaded += (s, e) => DrawChart();
        SizeChanged += (s, e) => DrawChart();
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SimpleBarChart chart)
        {
            chart.DrawChart();
        }
    }

    private void DrawChart()
    {
        ChartCanvas.Children.Clear();

        if (Data == null || Data.Count == 0 || ActualWidth == 0 || ActualHeight == 0)
            return;

        var maxValue = Data.Values.Max();
        if (maxValue == 0) maxValue = 1;

        var barWidth = (ActualWidth - 40) / Data.Count;
        var chartHeight = ActualHeight - 40;
        var x = 20.0;

        var colors = new[] { "#3b82f6", "#8b5cf6", "#10b981", "#f59e0b", "#ef4444", "#06b6d4" };
        if (colors.Length == 0) return;

        var colorIndex = 0;

        foreach (var item in Data)
        {
            var barHeight = (double)(item.Value / maxValue) * chartHeight;
            var y = chartHeight - barHeight + 10;

            // Bar
            var bar = new Rectangle
            {
                Width = barWidth - 10,
                Height = barHeight,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[colorIndex % colors.Length])),
                RadiusX = 4,
                RadiusY = 4
            };

            Canvas.SetLeft(bar, x);
            Canvas.SetTop(bar, y);
            ChartCanvas.Children.Add(bar);

            // Label
            var label = new TextBlock
            {
                Text = item.Key,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Width = barWidth - 10,
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, chartHeight + 15);
            ChartCanvas.Children.Add(label);

            x += barWidth;
            colorIndex++;
        }
    }
}
