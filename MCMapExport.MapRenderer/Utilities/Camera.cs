namespace MCMapExport.MapRenderer.Utilities {
    public class Camera {
        private float _zoom = 1;

        public float ZoomFactor { get; set; } = 1.1f;
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;

        public float Zoom {
            get => _zoom;
            set {
                _zoom = value;
                if (_zoom <= 0.01f) {
                    _zoom = 0.01f;
                }
            }
        }
    }
}