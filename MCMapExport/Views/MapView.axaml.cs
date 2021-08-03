using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MCMapExport.Common;
using MCMapExport.Common.Models;
using MCMapExport.Reader;

namespace MCMapExport.Views {
    public class MapView : Canvas {
        private Camera _cam;
        private Point? _prevPoint = null;
        private WorldReader? _reader = null;

        public MapView() {
            InitializeComponent();
            _cam = new Camera();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public void SetWorldReader(WorldReader reader) {
            _reader = reader;
        }

        private void DrawChunk(DrawingContext context, Chunk chunk, Point center) {
            foreach (var (location, (y, block)) in chunk.TopLayer) {
                var point = new Point(((location.X + chunk.XMin) * _cam.Zoom) + center.X, ((location.Y + chunk.ZMin) * _cam.Zoom) + center.Y);
                var rect = new Rect(point.X, point.Y, _cam.Zoom, _cam.Zoom);
                var color = EnumHelpers.ColorFromBlockType(block);
                var brush = new SolidColorBrush(new Color(color.A, color.R, color.G, color.B));
                var pen = new Pen(brush);
                context.DrawRectangle(brush, pen, rect);
            }
        }

        public override void Render(DrawingContext context) {
            /*var background = this.Bounds;
            context.FillRectangle(new SolidColorBrush(new Color(255, 0, 0, 0)), background);
            if (_reader is null) {
                return;
            }

            var blockCount = GetViewPortSize() / _cam.Zoom;
            var viewPortCenter = GetViewPortCenter();
            var centerHalf = new Point(viewPortCenter.X / 2, viewPortCenter.Y / 2);
            for (var i = 0; i < blockCount.X; i += 16) {
                for (var j = 0; j < blockCount.Y; j += 16) {
                    var chunk = _reader.GetChunkAt(i, 0, j);
                    DrawChunk(context, chunk, viewPortCenter);
                }
            }*/

            base.Render(context);
        }

        private Point GetViewPortCenter() {
            var center = new Point(
                Math.Abs((Bounds.Left - Bounds.Right) / 2) + _cam.X,
                Math.Abs((Bounds.Top - Bounds.Bottom) / 2) + _cam.Y);
            return center;
        }

        private Point GetViewPortSize() {
            return new(Math.Abs(Bounds.Left - Bounds.Right), Math.Abs(Bounds.Top - Bounds.Bottom));
        }

        public void Invalidate() {
            InvalidateArrange();
        }


        private void OnPointerMoved(object? sender, PointerEventArgs e) {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed && _prevPoint is not null) {
                var tmp = point.Position - (Point) _prevPoint;
                _cam.Position += tmp;
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
            }
            else if (e.Delta.Y < 0) {
                _cam.ZoomOut();
            }

            Invalidate();
        }
    }
}