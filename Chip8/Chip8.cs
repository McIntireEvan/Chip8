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
        Texture2D pixel;

        ushort[] memory;
        ushort counter;

        ushort[] registers; //Vx
        ushort addressPointer; //I

        ushort[] stack;
        ushort stackPointer;

        byte[,] display;
        bool redraw;

        public Chip8()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 512;
            Content.RootDirectory = "Content";
            memory = new ushort[4096];
            counter = 0x200;

            registers = new ushort[16];
            addressPointer = (char) 0x0;

            stack = new ushort[12];
            stackPointer = 0;

            display = new byte[64,32];
            redraw = true; 
        }

        private void LoadFile(string file) 
        {
            using (Stream reader = File.OpenRead(file))
            {
                int offset = 0;
                while(reader.Position != reader.Length)
                {
                    memory[0x200 + offset] = (ushort)(reader.ReadByte() & 0xFF);
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
            pixel = Content.Load<Texture2D>("pixel");
            LoadFile("pong.c8");
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
            ushort opcode = (ushort)(memory[counter] << 8 | memory[counter + 1]);

            Console.WriteLine((opcode & 0xFFFF).ToString("x"));
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
                case 0x2000: //Call subroutine at 0x2NNN
                    {
                        ushort address = (ushort)(opcode & 0x0FFF);
                        Console.WriteLine(address.ToString("x"));
                        stack[stackPointer] = counter;
                        stackPointer++;
                        counter = address;
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
                        ushort x = (ushort)((opcode & 0x0F00) >> 8);
                        ushort NN = (ushort)(opcode & 0x00FF);
                        registers[x] = NN;
                        counter += 2;
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
                        Console.WriteLine("9");
                        break;
                    }
                case 0xA000:
                    {
                        addressPointer = (ushort)(opcode & 0x0FFF);
                        counter += 2;
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
                        //Draw at DXYN
                        counter += 2;
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
                default:
                    {
                        Console.WriteLine("Error");
                        break;
                    }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    Color color = display[i,j] == 1 ? Color.White : Color.Black;
                    spriteBatch.Draw(pixel, new Rectangle(i * 64 * 16, i * 32 * 16, 64 * 16, 32 * 16), color);
                }
            }
            spriteBatch.End();
        }
    }
}