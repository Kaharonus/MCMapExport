using System;
using System.Collections.Generic;
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

        private JobQueue<Region> _loader = new();


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

        public void Invalidate() {
            foreach (var region in _reader.Reader!.Regions) {
                var job = new Job<Region>(() => _reader.Reader.ReadRegion(region.x, region.y));
                job.Callback += CreateImage;
                _loader.Add(job);
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            _renderer = this.FindControl<OpenGLRenderer>("Renderer");
            _renderer.SetCamera(_cam);
        }


        private void CreateImage(object? sender, Region data) {
            var tex = new Texture(512, data.XOffset, data.YOffset);
            for (var x = 0; x < 32; x++) {
                for (var y = 0; y < 32; y++) {
                    var chunk = data[x, y];
                    WriteChunkIntoTexture(x, y, chunk, ref tex);
                }
            }
            _renderer.AddTexture(data.XOffset, data.YOffset, tex);
        }

        private static void WriteChunkIntoTexture(int xOffset, int yOffset, Chunk c, ref Texture tex) {
            if (c.IsEmpty) {
                return;
            }
            for (var x = 0; x < 16; x++) {
                for (var y = 0; y < 16; y++) {
                    var position = (x, y);
                    if (!c.TopLayer.ContainsKey(position)) {
                        continue;
                    }

                    tex[(yOffset * 16) + y, (xOffset * 16) + x] =
                        RgbaColor.FromColor(EnumHelpers.ColorFromBlockType(c.TopLayer[(x, y)].type));
                }
            }
        }


        private void OnPointerMoved(object? sender, PointerEventArgs e) {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed && _prevPoint is not null) {
                var (x, y) = point.Position - (Point)_prevPoint;
                var speed = 1 / _cam.Zoom;
                _cam.X += (float)(x / _renderer.Bounds.Width) * (speed);
                _cam.Y += (float)(y / _renderer.Bounds.Height) * (speed);
            }

            _prevPoint = point.Position;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
            if (e.KeyModifiers != KeyModifiers.Control) {
                return;
            }

            if (e.Delta.Y > 0) {
                _cam.Zoom *= _cam.ZoomFactor;
            } else if (e.Delta.Y < 0) {
                _cam.Zoom /= +_cam.ZoomFactor;
            }
        }
    }
}