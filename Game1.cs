using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace TextRenderingTest
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Effect SimpleFontEffect;
        private Effect SimpleFontEffect2;
        private TextRenderer textRenderer;
        private Texture2D texture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            SimpleFontEffect = Content.Load<Effect>("File");
            SimpleFontEffect2 = Content.Load<Effect>("File2");
            string chars = string.Concat(Enumerable.Range(32, 96).Select(x => (char)x));
            var FontTool = new FontService(this);
            FontTool.SetFont(@"C:\Windows\Fonts\Arial.ttf");
            FontTool.SetSize(12);
            this.texture = FontTool.RenderString("Testing 123...");
            var texture = FontTool.RenderString(chars);
            var crs = FontTool.crs;
            textRenderer = new TextRenderer(GraphicsDevice, crs, texture, SimpleFontEffect);
            textRenderer.BuildVertexArray("Testing 123...", new Vector2(200, 100));
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Immediate);
            SimpleFontEffect2.Techniques[0].Passes[0].Apply();
            spriteBatch.Draw(texture, new Vector2(200, 130), Color.White);
            spriteBatch.End();
            textRenderer.Draw();
            base.Draw(gameTime);
        }
    }
}
