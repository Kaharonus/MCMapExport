using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MCMapExport.Views {
    public class MapView : Canvas {

        private Camera _cam;
        private Point? _prevPoint = null;


        public MapView() {
            InitializeComponent();
            _cam = new Camera();
        }
        
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void Render(DrawingContext context) {
            var background = this.Bounds;
            context.FillRectangle(new SolidColorBrush(new Color(255,0,0,0)), background);
            
            var center = GetViewPortCenter();
            
            var r = new Rect( center.X / 2, center.Y / 2, _cam.Zoom, _cam.Zoom);
            var brush = new SolidColorBrush(new Color(255, 0, 0, 255));
            var p = new Pen(brush);
            context.DrawRectangle(brush,p,r);

            base.Render(context);
        }

        private Point GetViewPortCenter() {
            var center = new Point(
                Math.Abs((Bounds.Left - Bounds.Right) / 2) + _cam.X,
                Math.Abs((Bounds.Top - Bounds.Bottom) / 2) + _cam.Y);
            return center;
        }

        public void Invalidate(){
            InvalidateArrange();
        }


        private void OnPointerMoved(object? sender, PointerEventArgs e) {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed && _prevPoint is not null) {
                var tmp = point.Position - (Point)_prevPoint;
                _cam.Position += tmp * 2;
                Invalidate();
            }
            _prevPoint = point.Position;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
            if (e.KeyModifiers != KeyModifiers.Control) {
                return;
            }
            if (e.Delta.Y > 0) {
                _cam.ZoomIn();
            }else if (e.Delta.Y < 0) {
                _cam.ZoomOut();
            }
            Invalidate();
        }
    }
}