using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MCMapExport.MapRenderer.Utilities;

namespace MCMapExport.MapRenderer.Develop {
    public class MainWindow : Window {
        private OpenGLRenderer _renderer;

        private Camera _cam;
        private Point? _prevPoint = null;
        
        
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _cam = new Camera();
            _renderer.SetCamera(_cam);
            var r = new Random();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            _renderer = this.FindControl<OpenGLRenderer>("GlRenderer");
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e) {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed && _prevPoint is not null) {
                var (x, y) = point.Position - (Point) _prevPoint;
                _cam.X += (float) (x / _renderer.Bounds.Width) * _cam.MovementSpeed;
                _cam.Y += (float) (y / _renderer.Bounds.Height) * _cam.MovementSpeed;
            }

            _prevPoint = point.Position;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
            if (e.KeyModifiers != KeyModifiers.Control) {
                return;
            }

            if (e.Delta.Y > 0) {
                _cam.Zoom += _cam.ZoomFactor;
            }
            else if (e.Delta.Y < 0) {
                _cam.Zoom -= _cam.ZoomFactor;
            }
        }
    }
}