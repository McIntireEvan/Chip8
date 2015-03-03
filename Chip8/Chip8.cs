using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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
            addressPointer = (char)0x0;

            stack = new ushort[12];
            stackPointer = 0;

            display = new byte[64, 32];

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
                while (reader.Position != reader.Length)
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
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

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
                        counter = (ushort)(opcode & 0x0FFF);
                        break;
                    }
                case 0x2000: //Call subroutine at 0x2NNN
                    {
                        ushort address = (ushort)(opcode & 0x0FFF);
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
                        int x = (opcode & 0x0F00) >> 8;
                        int nn = (opcode & 0x00FF);
                        if (registers[x] != nn)
                        {
                            counter += 4;
                        }
                        else
                        {
                            counter += 2;
                        }
                        break;
                    }
                case 0x5000:
                    {
                        int x = opcode & 0x0F00 >> 8;
                        int y = opcode & 0x00F0 >> 4;
                        if (registers[x] == registers[y])
                        {
                            counter += 4;
                        }
                        else
                        {
                            counter += 2;
                        }
                        break;
                    }
                case 0x6000:
                    {
                        ushort x = (ushort)((opcode & 0x0F00) >> 8);
                        registers[x] = (ushort)(opcode & 0x00FF);
                        counter += 2;
                        break;
                    }
                case 0x7000:
                    {
                        int x = (opcode & 0x0F00) >> 8;
                        registers[x] = (ushort)((registers[x] + (opcode & 0x00FF)) & 0xFF);
                        counter += 2;
                        break;
                    }
                case 0x8000:
                    {
                        int x = opcode & 0x0F00 >> 8;
                        int y = opcode & 0x00F0 >> 4;
                        switch (opcode & 0x000F)
                        {
                            case 0x0000:
                                {
                                    registers[x] = registers[y];
                                    counter += 2;
                                    break;
                                }
                            case 0x0001:
                                {
                                    registers[x] |= registers[y];
                                    counter += 2;
                                    break;
                                }
                            case 0x0002:
                                {
                                    registers[x] &= registers[y];
                                    counter += 2;
                                    break;
                                }
                            case 0x0003:
                                {
                                    registers[x] ^= registers[y];
                                    counter += 2;
                                    break;
                                }
                            case 0x0004:
                                {
                                    registers[0xF] = (registers[y] > (255 - registers[x])) ? (ushort)1 : (ushort)0;
                                    registers[x] = (ushort)((registers[y] + registers[x]) & 0xFF);
                                    counter += 2;
                                    break;
                                }
                            case 0x0005:
                                {
                                    registers[0xF] = (registers[x] > registers[y]) ? (ushort)1 : (ushort)0;
                                    registers[x] = (ushort)((registers[x] - registers[y]) & 0xFF);
                                    counter += 2;
                                    break;
                                }
                            case 0x0006:
                                {
                                    ushort bit = (ushort)(registers[x] & 0x1);
                                    registers[x] = (ushort)(registers[x] >> 1);
                                    registers[0xF] = bit;
                                    counter += 2;
                                    break;
                                }
                            case 0x0007:
                                {
                                    registers[0xF] = (registers[x] > registers[y]) ? (ushort)0 : (ushort)1;
                                    registers[x] = (ushort)((registers[y] - registers[x]) & 0xFF);
                                    counter += 2;
                                    break;
                                }
                            case 0x000E:
                                {
                                    registers[0xF] = (ushort)(registers[x] & 0x80);
                                    registers[x] = (ushort)(registers[x] << 1);
                                    counter += 2;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x9000:
                    {
                        int x = registers[(opcode & 0x0F00) >> 8];
                        int y = registers[(opcode & 0x00F0) >> 4];
                        if (x != y)
                        {
                            counter += 4;
                        }
                        else
                        {
                            counter += 2;
                        }
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
                        addressPointer = (ushort)((opcode & 0x0FFF) + (registers[0] & 0xFF));
                        break;
                    }
                case 0xC000:
                    {
                        int x = (opcode & 0x0F00) >> 8;
                        int nn = (opcode & 0x00FF);

                        registers[x] = (ushort)(new Random().Next(256) & nn);
                        counter += 2;
                        break;
                    }
                case 0xD000:
                    {
                        //Draw at DXYN
                        int x = registers[(opcode & 0x0F00) >> 8];
                        int y = registers[(opcode & 0x00F0) >> 4];
                        int height = opcode & 0x000F;
                        registers[0xF] = 0;

                        for (int _y = 0; _y < height; _y++)
                        {
                            ushort pixel = memory[addressPointer + _y];
                            for (int _x = 0; _x < 8; _x++)
                            {
                                if ((pixel & (0x80 >> _x)) != 0)
                                {
                                    int totalX = (x + _x) % 64;
                                    int totalY = (y + _y) % 32;
                                    if (display[totalX, totalY] == 1)
                                        registers[0xF] = 1;
                                    display[totalX, totalY] ^= 1;
                                }
                            }
                        }

                        counter += 2;
                        break;
                    }
                case 0xE000:
                    {
                        switch (opcode & 0x00FF)
                        {
                            case 0x009E:
                                {
                                    //TODO: Check if key down
                                    counter += 2;
                                    break;
                                }
                            case 0x00A1:
                                {
                                    //TODO: Check if key up
                                    counter+=4;
                                    break;
                                }
                        }
                        break;
                    }
                case 0xF000:
                    {
                        switch (opcode & 0x00FF)
                        {
                            case 0x0007:
                                {
                                    int x = (opcode & 0x0F00) >> 8;
                                    registers[x] = delay;
                                    counter += 2;
                                    break;
                                }
                            case 0x000A:
                                {
                                    //TODO: Read User input
                                    break;
                                }
                            case 0x0015:
                                {
                                    int x = (opcode & 0x0F00) >> 8;
                                    delay = registers[x];
                                    counter += 2;
                                    break;
                                }
                            case 0x0018:
                                {
                                    int x = (opcode & 0x0F00) >> 8;
                                    soundDelay = registers[x];
                                    counter += 2;
                                    break;
                                }
                            case 0x001E:
                                {
                                    int x = (opcode & 0x0F00) >> 8;
                                    addressPointer += registers[x];
                                    counter += 2;
                                    break;
                                }
                            case 0x0029:
                                {
                                    int x = (opcode & 0x0F00) >> 8;
                                    int character = registers[x];
                                    addressPointer = (ushort)(0x050 + (character * 5));
                                    counter += 2;
                                    break;
                                }
                            case 0x0033:
                                {
                                    int vx = registers[(opcode & 0x0F00) >> 8], hundreds, tens;
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
                            case 0x0055:
                                {
                                    int x = opcode & 0x0F00 >> 8;
                                    for (int i = 0; i < x; i++)
                                    {
                                        memory[addressPointer + i] = registers[i];
                                    }
                                    addressPointer += (ushort)(x + 1);
                                    counter += 2;
                                    break;
                                }
                            case 0x0065:
                                {
                                    int x = opcode & 0x0F00 >> 8;
                                    for (int i = 0; i < x; i++)
                                    {
                                        registers[i] = memory[addressPointer + i];
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
            if (delay > 0)
            {
                delay--;
            }
            if (soundDelay > 0)
            {
                soundDelay--;
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
                    Color color = display[i, j] == 1 ? Color.White : Color.Black;
                    spriteBatch.Draw(pixel, new Rectangle(i * 16, j * 16, 16, 16), color);
                }
            }
            spriteBatch.End();
        }
    }
}