using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TextRenderingTest
{
    public class TextRenderer
    {
        private readonly Dictionary<int, CharRecord> chars;
        private readonly Texture2D texture;
        private readonly Effect effect;
        private readonly GraphicsDevice device;
        private VertexPositionColorTexture[] vertices;
        private int[] indices;

        /// <summary>
        /// Any character with an index not found in <cref>Colors</cref> will get this color.
        /// </summary>
        public Color DefaultColor { get; set; } = Color.White;

        /// <summary>
        /// Set the color of each individual character.
        /// </summary>
        public Color[] Colors { get; set; } = new[] { Color.White };

        public TextRenderer(GraphicsDevice device, CharRecord[] crs, Texture2D texture, Effect effect)
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(device.Viewport.Bounds, 0.1f, 1);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["Texture"].SetValue(texture);
            this.device = device;
            this.effect = effect;
            this.texture = texture;

            chars = new Dictionary<int, CharRecord>(crs.Length);
            foreach (var cr in crs)
                chars.Add(cr.ID, new CharRecord(cr.ID, cr.X, cr.Y, cr.Width, cr.Height, cr.Xoffset, cr.Yoffset, cr.Xadvance));
        }
        
        public void BuildVertexArray(string s, Vector2 location)
        {
            vertices = new VertexPositionColorTexture[s.Length * 4];
            indices = new int[s.Length * 6];
            for (int r = 0, v = 0, iii = 0, iiii = 0; r < s.Length; r++, v += 6, iii += 4, iiii += 4)
            {
                CharRecord cr = chars[s[r]];
                Color vertexColor = Colors.Length < r + 1 ? DefaultColor : Colors[r];
                Vector4 uvs = new Vector4(cr.X / texture.Width, cr.Y / texture.Height, cr.Width / texture.Width, cr.Height / texture.Height);
                Vector4 box = new Vector4(location.X + cr.Xoffset, location.Y - cr.Yoffset, cr.Width, cr.Height);

                indices[v] = iii;
                indices[v + 1] = iii + 1;
                indices[v + 2] = iii + 2;

                indices[v + 3] = iii + 1;
                indices[v + 4] = iii + 3;
                indices[v + 5] = iii + 2;
                
                // top left
                vertices[iiii] = new VertexPositionColorTexture(
                    new Vector3(box.X, box.Y, -0.5f), vertexColor, new Vector2(uvs.X, uvs.Y));

                // top right
                vertices[iiii + 1] = new VertexPositionColorTexture(
                    new Vector3(box.X + box.Z, box.Y, -0.5f), vertexColor, new Vector2(uvs.X + uvs.Z, uvs.Y));

                // bottom left
                vertices[iiii + 2] = new VertexPositionColorTexture(
                    new Vector3(box.X, box.Y + box.W, -0.5f), vertexColor, new Vector2(uvs.X, uvs.Y + uvs.W));

                // bottom right
                vertices[iiii + 3] = new VertexPositionColorTexture(
                    new Vector3(box.X + box.Z, box.Y + box.W, -0.5f), vertexColor, new Vector2(uvs.X + uvs.Z, uvs.Y + uvs.W));

                //
                location.X += cr.Xadvance;
            }
        }

        public void Draw()
        {
            if (vertices == null)
                return;

            device.BlendState = BlendState.NonPremultiplied;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    indices,
                    0,
                    indices.Length / 3);
            }
        }
    }
}
