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
using MCMapExport.Reader;
using MCMapExport.Services;

namespace MCMapExport.Views {
    public class MapView : DIUserControl {
        private static readonly Random _random = new();

        [Inject]
        private WorldReaderService _reader;
        
        private Camera _cam;
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
            _cam = new Camera();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            _renderer = this.FindControl<OpenGLRenderer>("Renderer");
        }
        

        public void Invalidate() {
            //InvalidateArrange();
            if (!CreateImage()) {
                return;
            }

            _renderer.SetTexture(_colorData, _height, _width);
            _renderer.ScheduleRedraw();
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
            var sameCount = (int)(1 / _resolutionScale);
            for (var col = 0; col < _height; col++) {
                for (var row = 0; row < _width; row++) {
                    try {
                        var index = _width * col + row;
                        var type = _reader.Reader!.GetBlockAtTop(row - corner.x, col - corner.y);
                        _colorData[index] = RgbaColor.FromColor(EnumHelpers.ColorFromBlockType(type));
                    }
                    catch (Exception e) {
                        
                    }
                }
            }
            /*for (var i = 0; i < data.Length; i++) {
                data[i] = RgbaColor.FromRgb((byte) r.Next(0, 255), (byte) r.Next(0, 255), (byte) r.Next(0, 255));
            }*/

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
            const int minMove = 5;
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed && _prevPoint is not null) {
                var tmp = point.Position - (Point) _prevPoint;
                if (tmp.X + tmp.Y > minMove) {
                    _cam.Position += tmp;
                    Invalidate();
                }
            }

            _prevPoint = point.Position;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
            if (e.KeyModifiers != KeyModifiers.Control) {
                return;
            }

            if (e.Delta.Y > 0) {
                _cam.ZoomIn();
            } else if (e.Delta.Y < 0) {
                _cam.ZoomOut();
            }

            Invalidate();
        }
    }
}