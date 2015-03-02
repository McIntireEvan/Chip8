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

        ushort delay;
        ushort soundDelay;

        ushort[] font;

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

            delay = 0;
            soundDelay = 0;

            font = new ushort[]{ 
                0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                0x20, 0x60, 0x20, 0x20, 0x70, // 1
                0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };

            for (int i = 0; i < font.Length; i++)
            {
                memory[0x50 + i] = font[i];
            }
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

            Console.WriteLine(counter + "::" + (opcode & 0xFFFF).ToString("x"));
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    {
                        switch (opcode & 0x00FF)
                        {
                            case 0x00E0:
                                {
                                    display = new byte[64, 32];
                                    break;
                                }
                            case 0x00EE:
                                {
                                    stackPointer--;
                                    counter = (ushort)(stack[stackPointer] + 2);
                                    break;
                                }
                            default:
                                {
                                    Exit();
                                    break;
                                }
                        }
                        break;
                    }
                case 0x1000:
                    {
                        int address = opcode & 0x0FFF;
                        counter = (ushort)address;
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
                        int x = (opcode & 0x0F00) >> 8;
                        int nn = (opcode & 0x00FF);
                        if (registers[x] == nn)
                        {
                            counter += 4;
                        }
                        else
                        {
                            counter += 2;
                        }
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
                        int x = (opcode & 0x0F00) >> 8;
                        ushort nn = (ushort)(opcode & 0x00FF);
                        registers[x] += nn;
                        counter += 2;
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
                        int x = registers[(opcode & 0x0F00) >> 8];
                        int y = registers[(opcode & 0x00F0) >> 4];
                        int height = opcode & 0x000F;

                        registers[0xF] = 0;

                        for(int _y = 0; _y < height; _y++)
                        {
                            ushort pixel = memory[addressPointer + _y];
                            for (int _x = 0; _x < 8; _x++)
                            {
                                if((pixel & (0x80 >> _x)) != 0)
                                {
                                    if (display[x + _x, y + _y] == 1)
                                        registers[0xF] = 1;
                                    display[x + _x, y + _y] ^= 1;
                                }
                            }
                        }

                        counter += 2;
                        break;
                    }
                case 0xE000:
                    {
                        break;
                    }
                case 0xF000:
                    {
                        switch(opcode & 0x00FF) 
                        {
                            case 0x0007:
                                {
                                    int x = opcode & 0x0F00 >> 8;
                                    registers[x] = delay;
                                    counter += 2;
                                    break;
                                }
                            case 0x0015:
                                {
                                    int x = opcode & 0x0F00 >> 8;
                                    delay = registers[x];
                                    counter += 2;
                                    break;
                                }
                            case 0x0018:
                                {
                                    int x = opcode & 0x0F00 >> 8;
                                    soundDelay = registers[x];
                                    counter += 2;
                                    break;
                                }
                            case 0x0029:
                                {
                                    int x = opcode & 0x0F00 >> 8;
                                    int character = registers[x];
                                    //Console.WriteLine(character);
                                    //Console.WriteLine((ushort)(0x050 + (character * 5)));
                                    addressPointer = (ushort)(0x050 + (character * 5));
                                    counter += 2;
                                    break;
                                }
                            case 0x0033:
                                {
                                    int vx = registers[opcode & 0x0F00 >> 8], hundreds, tens;
                                    hundreds = ((vx - (vx % 100)) / 100);
                                    vx -= hundreds * 100;
                                    tens = ((vx - (vx % 10)) / 10);
                                    vx -= tens * 10;

                                    memory[addressPointer] = (ushort)hundreds;
                                    memory[addressPointer + 1] = (ushort)tens;
                                    memory[addressPointer + 2] = (ushort)vx;
                                    counter += 2;
                                    break;
                                }
                            case 0x0065:
                                {
                                    int x = opcode & 0x0F00 >> 8;
                                    for (int i = 0; i < x; i++)
                                    {
                                        registers[i] = memory[addressPointer + 1];
                                    }
                                    addressPointer += (ushort)(x + 1);
                                    counter += 2;
                                    break;
                                }
                        }
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
                    spriteBatch.Draw(pixel, new Rectangle(i * 16, j * 16, 16, 16), color);
                }
            }
            spriteBatch.End();
        }
    }
}