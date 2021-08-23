namespace MCMapExport.MapRenderer.Utilities {
    public static class VertexData {
        public static float[] Vertices { get; } = {
            1.0f,  1.0f, 0.0f,  // top right
            1.0f, -1.0f, 0.0f,  // bottom right
            -1.0f, -1.0f, 0.0f,  // bottom left
            -1.0f,  1.0f, 0.0f   // top left 
        };

        public static int[] Indices { get; } = {
            0, 1, 3,
            1, 2, 3
        };
    }
}