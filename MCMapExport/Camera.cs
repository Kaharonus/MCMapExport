using Avalonia;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MCMapExport {
    public class Camera {
        
        const double ZoomFactor = 0.25;

        public double Zoom { get; set; }

        public Point Position { get; set; }


        public double X {
            get => Position.X;
            set => Position = Position.WithX(value);
        }

        public double Y {
            get => Position.Y;
            set => Position = Position.WithY(value);
        }

        public Camera() {
            Zoom = 10;
            Position = new Point(0, 0);
        }

        public void ZoomIn() {
            if (Zoom < 100) {
                Zoom += Zoom * ZoomFactor;
            }
        }

        public void ZoomOut() {
            if (Zoom > 1) {
                Zoom -= Zoom * ZoomFactor;
            }
        }
    }
}