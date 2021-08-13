namespace MCMapExport.MapRenderer {
    public class Camera {

        public int MovementSpeed { get; set; } = 2;
        public float ZoomFactor { get; set; } = 0.05f;
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Zoom { get; set; } = 1;
    }
}