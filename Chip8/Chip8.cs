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
        char counter;

        char[] registers; //Vx
        char addressPointer; //I

        public Chip8()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            memory = new char[4096];
            counter = (char) 0x200;

            registers = new char[16];
            addressPointer = (char) 0x0;

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

            //We need the opcode in the form of 0xXXXX
            //memory[counter] << 8 gets the first two parts of it, in the form of 0xXX, and shifts it to the left to form 0xXX00
            //The second part ORs the second half of the opcode on, leaving us with 0xXXXX
            char opcode = (char)(memory[counter] << 8 | memory[counter + 1]);

            Console.WriteLine(opcode & 0xFFFF);
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    {
                        break;
                    }
                case 0x1000:
                    {
                        break;
                    }
                case 0x2000:
                    {
                        break;
                    }
                case 0x3000:
                    {
                        break;
                    }
                case 0x4000:
                    {
                        break;
                    }
                case 0x5000:
                    {
                        break;
                    }
                case 0x6000:
                    {
                        break;
                    }
                case 0x7000:
                    {
                        break;
                    }
                case 0x8000:
                    {
                        break;
                    }
                case 0x9000:
                    {
                        break;
                    }
                case 0xA000:
                    {
                        break;
                    }
                case 0xB000:
                    {
                        break;
                    }
                case 0xC000:
                    {
                        break;
                    }
                case 0xD000:
                    {
                        break;
                    }
                case 0xE000:
                    {
                        break;
                    }
                case 0xF000:
                    {
                        break;
                    }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}