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
            var tex = new Texture(3, 0, 0);
            tex[1,1] = RgbaColor.FromRgb(0,255,0);
            tex[0,0] = tex[0,1] = tex[1,0] =tex[2,0] = tex[0,2] = tex[2,2] = tex[1,2] = tex[2,1]  = RgbaColor.FromRgb(255,0,0);
            _renderer.AddTexture(0,0, tex);
            
            var tex2 = new Texture(3, 0, 1);
            tex2[1,1] = RgbaColor.FromRgb(0,255,255);
            tex2[0,0] = tex2[0,1] = tex2[1,0] =tex2[2,0] = tex2[0,2] = tex2[2,2] = tex2[1,2] = tex2[2,1]  = RgbaColor.FromRgb(0,0,255);
            _renderer.AddTexture(0,1, tex2);
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e) {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed && _prevPoint is not null) {
                var (x, y) = point.Position - (Point) _prevPoint;
                var speed = 1 / _cam.Zoom;
                _cam.X += (float)(x / _renderer.Bounds.Width) * speed;
                _cam.Y += (float)(y / _renderer.Bounds.Height) * speed;
            }

            _prevPoint = point.Position;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
            if (e.KeyModifiers != KeyModifiers.Control) {
                return;
            }

            if (e.Delta.Y > 0) {
                _cam.Zoom *= _cam.ZoomFactor;
            }
            else if (e.Delta.Y < 0) {
                _cam.Zoom /= _cam.ZoomFactor;
            }
        }
    }
}