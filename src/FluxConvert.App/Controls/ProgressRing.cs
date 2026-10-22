using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FluxConvert.App.Controls;

/// <summary>
/// A circular progress indicator drawn with the theme colours.
/// </summary>
/// <remarks>
/// <para>
/// The control draws a dimmed track and an indicator arc. It reports honest progress: the
/// determinate mode takes a value between 0 and 1, and the indeterminate mode sweeps a fixed
/// arc around the circle to show that work is happening without inventing a percentage.
/// </para>
/// <para>
/// The colours default to the theme resources but can be overridden per instance.
/// </para>
/// </remarks>
public class ProgressRing : FrameworkElement
{
    /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(ProgressRing),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="IsIndeterminate"/> dependency property.</summary>
    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
        nameof(IsIndeterminate),
        typeof(bool),
        typeof(ProgressRing),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Thickness"/> dependency property.</summary>
    public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(
        nameof(Thickness),
        typeof(double),
        typeof(ProgressRing),
        new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="TrackBrush"/> dependency property.</summary>
    public static readonly DependencyProperty TrackBrushProperty = DependencyProperty.Register(
        nameof(TrackBrush),
        typeof(Brush),
        typeof(ProgressRing),
        new FrameworkPropertyMetadata(null));

    /// <summary>Identifies the <see cref="IndicatorBrush"/> dependency property.</summary>
    public static readonly DependencyProperty IndicatorBrushProperty = DependencyProperty.Register(
        nameof(IndicatorBrush),
        typeof(Brush),
        typeof(ProgressRing),
        new FrameworkPropertyMetadata(null));

    private const double IndeterminateSweep = 90.0;
    private double _indeterminateAngle;

    /// <summary>Initialises a new instance of the <see cref="ProgressRing"/> class.</summary>
    public ProgressRing()
    {
        Width = 132;
        Height = 132;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>How far the conversion has progressed, between 0 and 1.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, 0.0, 1.0));
    }

    /// <summary>Whether the progress is unknown.</summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>The width of the ring in pixels.</summary>
    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    /// <summary>The colour of the dimmed track behind the indicator.</summary>
    public Brush? TrackBrush
    {
        get => (Brush?)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>The colour of the indicator arc.</summary>
    public Brush? IndicatorBrush
    {
        get => (Brush?)GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);

        var thickness = Math.Max(1.0, Thickness);
        var radius = (Math.Min(ActualWidth, ActualHeight) - thickness) / 2.0;
        if (radius <= 0)
        {
            return;
        }

        var centre = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
        var track = TrackBrush ?? FindBrush("Surface.Ring") ?? Brushes.Transparent;
        var indicator = IndicatorBrush ?? FindBrush("Accent.Blue") ?? Brushes.DodgerBlue;

        drawingContext.DrawEllipse(track, null, centre, radius, radius);

        var startAngle = -90.0;
        var sweep = Value * 360.0;

        if (IsIndeterminate)
        {
            startAngle = _indeterminateAngle;
            sweep = IndeterminateSweep;
        }

        if (sweep <= 0)
        {
            return;
        }

        var geometry = new PathGeometry();
        geometry.Figures.Add(new PathFigure
        {
            IsClosed = false,
            IsFilled = false,
            StartPoint = PointOnCircle(centre, radius, startAngle),
            Segments = new PathSegmentCollection
            {
                new ArcSegment
                {
                    Point = PointOnCircle(centre, radius, startAngle + sweep),
                    Size = new Size(radius, radius),
                    RotationAngle = 0,
                    IsLargeArc = sweep > 180.0,
                    SweepDirection = SweepDirection.Clockwise,
                    IsStroked = true,
                },
            },
        });

        var pen = new Pen(indicator, thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
        };

        drawingContext.DrawGeometry(null, pen, geometry);
    }

    private Brush? FindBrush(string key)
    {
        var brush = TryFindResource(key) as Brush;
        if (brush is not null)
        {
            return brush;
        }

        return Application.Current?.TryFindResource(key) as Brush;
    }

    private static Point PointOnCircle(Point centre, double radius, double angleDegrees)
    {
        var radians = angleDegrees * Math.PI / 180.0;
        return new Point(
            centre.X + (radius * Math.Cos(radians)),
            centre.Y + (radius * Math.Sin(radians)));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (IsIndeterminate)
        {
            CompositionTarget.Rendering += OnRendering;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CompositionTarget.Rendering -= OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        _indeterminateAngle = (_indeterminateAngle + 4.0) % 360.0;
        InvalidateVisual();
    }
}
