using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace Chip8
{
    public class Chip8 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        char[] memory;

        public Chip8()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            memory = new char[4096];
            LoadFile("pong.c8");
        }

        private void LoadFile(string file) 
        {
            using (Stream reader = File.OpenRead(file))
            {
                int offset = 0;
                while(reader.Position != reader.Length)
                {
                    memory[0x200 + offset] = (char)(reader.ReadByte() & 0xFF);
                    offset++;
                }
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            
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

            base.Draw(gameTime);
        }
    }
}