using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Kaharonus.Avalonia.DependencyInjection;
using Kaharonus.Avalonia.DependencyInjection.Controls;
using MCMapExport.Common;
using MCMapExport.Common.Models;
using MCMapExport.MapRenderer;
using MCMapExport.MapRenderer.Utilities;
using MCMapExport.Reader;
using MCMapExport.Services;

namespace MCMapExport.Views {
    public class MapView : DIUserControl {
        private static readonly Random _random = new();

        [Inject] private WorldReaderService _reader;

        private Camera _cam = new();
        private Point? _prevPoint = null;


        private OpenGLRenderer _renderer;

        private double _resolutionScale = 1;
        private int _width = 0;
        private int _height = 0;
        private RgbaColor[] _colorData;

        public double ResolutionScale {
            get => _resolutionScale;
            set {
                _resolutionScale = value;
                _recalculateResolution = true;
            }
        }

        private bool _recalculateResolution = true;

        public MapView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            _renderer = this.FindControl<OpenGLRenderer>("Renderer");
            _renderer.SetCamera(_cam);
        }
        


        private bool CreateImage() {
            if (!_reader.IsInitialized) {
                return false;
            }

            if (_recalculateResolution) {
                var (width, height) = GetViewportSize();
                _width = (int) (width * ResolutionScale);
                _height = (int) (height * ResolutionScale);
                _colorData = new RgbaColor[_width * _height];
                _recalculateResolution = false;
            }

            var corner = GetCorner();
            var sameCount = (int) (1 / _resolutionScale);
            for (var col = 0; col < _height; col++) {
                for (var row = 0; row < _width; row++) {
                    try {
                        var index = _width * col + row;
                        var type = _reader.Reader!.GetBlockAtTop(row - corner.x, col - corner.y);
                        _colorData[index] = RgbaColor.FromColor(EnumHelpers.ColorFromBlockType(type));
                    }
                    catch (Exception e) { }
                }
            }
           

            return true;
        }


        private (int x, int y) GetCorner() {
            return ((int) (_cam.X - (_width / 2.0)), (int) (_cam.Y - (_height / 2.0)));
        }

        private (int x, int y) GetViewportCenter() {
            return ((int) (Math.Abs((Bounds.Left - Bounds.Right) / 2) + _cam.X),
                (int) (Math.Abs((Bounds.Top - Bounds.Bottom) / 2) + _cam.Y));
        }

        private (int width, int height) GetViewportSize() {
            return ((int) Math.Abs(Bounds.Left - Bounds.Right), (int) Math.Abs(Bounds.Top - Bounds.Bottom));
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
                _cam.Zoom -= +_cam.ZoomFactor;
            }

            //Invalidate();
        }
    }
}