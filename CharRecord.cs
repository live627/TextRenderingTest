namespace TextRenderingTest
{
    public struct CharRecord
    {
        public int ID { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float Xoffset { get; }
        public float Yoffset { get; }
        public float Xadvance { get; }

        public CharRecord(int iD, float x, float y, float width, float height, float xoffset, float yoffset, float xadvance)
        {
            ID = iD;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Xoffset = xoffset;
            Yoffset = yoffset;
            Xadvance = xadvance;
        }
    };
}
