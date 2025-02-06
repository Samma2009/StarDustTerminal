using System.Drawing;
using Cosmos.Core;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;

namespace StarDustCosmos
{
    public class Terminal
    {
        #region Properties
        public TerminalWriter Writer { get; internal set; }
        public PCScreenFont Font { get; internal set; }
        public Point Cursor = new Point(0, 0);
        public Color TextColor = Color.LightGray;
        public Color ClearColor = Color.Black;
        public Color BackColor = Color.Black;
        public string InputPrefix = "> ";

        List<string> CommandsHistory = new();
        public Dictionary<string, Action<string[]>> Commands = new();

        private Canvas Canvas;
        private Bitmap Buffer;
        private string InputBuffer = "";
        private int CommandIDX = 0;
        private int WriteIndex = 0;
        #endregion

        #region Constructors
        public Terminal(PCScreenFont font, Canvas canvas, uint W, uint H)
        {
            Font = font;
            Writer = new TerminalWriter(Write);
            System.Console.SetOut(Writer);
            Canvas = canvas;
            Buffer = new Bitmap(W, H, canvas.Mode.ColorDepth);
            Clear();
        }
        #endregion

        #region Methods
        #region Drawing
        public void Clear()
        {
            MemoryOperations.Fill(Buffer.RawData, ClearColor.ToArgb());
            Cursor = new Point(0, 0);
            BackColor = ClearColor;
        }

        public void DrawString(string str, int x, int y)
        {
            int length = str.Length;
            byte width = Font.Width;
            for (int i = 0; i < length; i++)
            {
                DrawRect(x, y, Font.Width, Font.Height, BackColor);
                DrawChar(str[i], Font, TextColor, x, y);
                x += width;
            }
        }

        public virtual void DrawChar(char c, Font font, Color color, int x, int y)
        {
            byte height = font.Height;
            byte width = font.Width;
            byte[] data = font.Data;
            int num = height * (byte)c;
            for (int i = 0; i < height; i++)
            {
                for (byte b = 0; b < width; b++)
                {
                    if (font.ConvertByteToBitAddress(data[num + i], b + 1))
                    {
                        Buffer.RawData[Buffer.Width * (y + i) + (x + b)] = color.ToArgb();
                    }
                }
            }
        }

        static Color AlphaBlend(Color to, Color from, byte alpha)
        {
            byte R = (byte)(((to.R * alpha) + (from.R * (255 - alpha))) >> 8);
            byte G = (byte)(((to.G * alpha) + (from.G * (255 - alpha))) >> 8);
            byte B = (byte)(((to.B * alpha) + (from.B * (255 - alpha))) >> 8);
            return Color.FromArgb(R, G, B);
        }

        void DrawRect(int x, int y, int w, int h, Color col)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Buffer.RawData[Buffer.Width * (y + j) + (x + i)] = col.ToArgb();
                }
            }
        }

        public void Draw(int X, int Y)
        {
            Canvas.DrawImage(Buffer, X, Y);

            int w = 0;

            List<string> cmdinspect = new List<string>();
            foreach (var item in Commands)
            {
                if (item.Key.StartsWith(InputBuffer))
                {
                    cmdinspect.Add(item.Key);
                    if (item.Key.Length * Font.Width > w) w = item.Key.Length * Font.Width;
                }
            }

            w += 20;

            if (cmdinspect.Count > 0 && InputBuffer.Length > 0)
            {
                var h = Math.Min(cmdinspect.Count, 5) * Font.Height;
                var y = Cursor.Y * Font.Height + Y;
                if (Cursor.Y - 1 < h / Font.Height + 1)
                    y += Font.Height;
                else
                    y -= (h / Font.Height) * Font.Height;
                Canvas.DrawFilledRectangle(Color.DimGray, X + Cursor.X * Font.Width, y, w, h);
                Canvas.DrawRectangle(Color.White, X + Cursor.X * Font.Width, y, w, h);
                for (int i = 0; i < Math.Min(cmdinspect.Count, 5); i++)
                    Canvas.DrawString(cmdinspect[i], Font, TextColor, X + Cursor.X * Font.Width + 10, y + (i * Font.Height));
            }

            Canvas.DrawRectangle(TextColor, X + Cursor.X * Font.Width, Y + Cursor.Y * Font.Height, 1, Font.Height);
        }
        #endregion

        #region Writing
        public void WriteImage(Image buffer)
        {
            int X = Cursor.X * Font.Width;
            int Y = Cursor.Y * Font.Height;
            for (int i = 0; i < buffer.Width; i++)
            {
                for (int j = 0; j < buffer.Height; j++)
                {
                    long addr1 = Buffer.Width * (Y + j) + (X + i);
                    long addr2 = buffer.Width * j + i;

                    Color tocol = Color.FromArgb(buffer.RawData[addr2]);

                    Buffer.RawData[addr1] = AlphaBlend(tocol, Color.FromArgb(Buffer.RawData[addr1]), tocol.A).ToArgb();
                }
            }
        }

        internal void Write(string value)
        {
            string colbuffer = "";
            string prntbuff = "";
            bool iscoloring = false;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\n')
                {
                    DrawString(prntbuff, Cursor.X * Font.Width, Cursor.Y * Font.Height);
                    Return();
                    prntbuff = "";
                }
                else if (value[i] == '\x1b' && i + 1 <= value.Length && value[i + 1] == '[')
                {
                    if (prntbuff.Length > 0)
                    {
                        DrawString(prntbuff, Cursor.X * Font.Width, Cursor.Y * Font.Height);
                        Cursor.X += prntbuff.Length;
                    }
                    prntbuff = "";
                    iscoloring = true;
                    i++;
                }
                else if (value[i] == 'm' && iscoloring)
                {
                    if (colbuffer == "0")
                    {
                        BackColor = ClearColor;
                        TextColor = Color.LightGray;
                    }
                    else
                    {
                        var codes = colbuffer.Split(';');
                        foreach (var item in codes)
                        {
                            if (ANSIConstants.Foregrounds.ContainsKey(item)) TextColor = ANSIConstants.Foregrounds[item];
                            if (ANSIConstants.Backgrounds.ContainsKey(item)) BackColor = ANSIConstants.Backgrounds[item];
                            Kernel.PrintDebug(item);
                        }
                        if (BackColor == Color.Transparent) BackColor = ClearColor;
                    }

                    iscoloring = false;
                    colbuffer = "";
                }
                else
                {
                    if (iscoloring) colbuffer += value[i];
                    else prntbuff += value[i];
                }
            }
            if (prntbuff.Length > 0)
            {
                DrawString(prntbuff, Cursor.X * Font.Width, Cursor.Y * Font.Height);
                Cursor.X += prntbuff.Length;
            }
        }



        internal void Return()
        {
            Cursor.X = 0;
            if (Cursor.Y * Font.Height >= Buffer.Height - Font.Height)
            {
                for (int y = Font.Height; y < Font.Height * (Buffer.Height / Font.Height); y++)
                {
                    for (int x = 0; x < Buffer.Width; x++)
                    {
                        Buffer.RawData[Buffer.Width * (y - Font.Height) + x] = Buffer.RawData[Buffer.Width * y + x];
                    }
                }
                DrawRect(0, Cursor.Y * Font.Height, (int)Buffer.Width, (int)Buffer.Height - Cursor.Y * Font.Height, ClearColor);
            }
            else
                Cursor.Y++;
        }
        #endregion

        #region Inputs
        public void UpdateInput()
        {
            if (KeyboardManager.TryReadKey(out KeyEvent k))
            {
                switch (k.Key)
                {
                    default:
                        InputBuffer = InputBuffer.Insert(WriteIndex, k.KeyChar.ToString());
                        WriteIndex++;

                        string addr = InputBuffer.Remove(0, WriteIndex);

                        Write(k.KeyChar.ToString() + addr);
                        Cursor.X -= addr.Length;
                        break;
                    case ConsoleKeyEx.Backspace:
                        if (InputBuffer.Length > 0 && WriteIndex > 0)
                        {
                            InputBuffer = InputBuffer.Remove(WriteIndex - 1, 1);
                            Cursor.X--;
                            WriteIndex--;
                            DrawRect((Cursor.X + (InputBuffer.Length - WriteIndex)) * Font.Width, Cursor.Y * Font.Height, Font.Width, Font.Height, ClearColor);
                            string removr = InputBuffer.Remove(0, WriteIndex);

                            Write(removr);
                            Cursor.X -= removr.Length;
                        }
                        break;
                    case ConsoleKeyEx.Enter:
                        Return();
                        var args = InputBuffer.Split(' ').ToList();
                        if (Commands.ContainsKey(args[0])) Commands[args[0]].Invoke(args.ToArray());
                        if (InputBuffer.Length > 0) CommandsHistory.Add(InputBuffer);
                        InputBuffer = "";
                        CommandIDX = 0;
                        WriteIndex = 0;
                        Write(InputPrefix);
                        break;
                    case ConsoleKeyEx.UpArrow:
                        if (CommandsHistory.Count - (CommandIDX + 1) >= 0)
                        {
                            CommandIDX++;
                            Cursor.X -= InputBuffer.Length;
                            DrawRect(Cursor.X * Font.Width, Cursor.Y * Font.Height, InputBuffer.Length * Font.Width, Font.Height, ClearColor);
                            InputBuffer = CommandsHistory[CommandsHistory.Count - CommandIDX];
                            Write(InputBuffer);
                        }
                        break;
                    case ConsoleKeyEx.DownArrow:
                        if (CommandIDX - 1 >= 1)
                        {
                            CommandIDX--;
                            Cursor.X -= InputBuffer.Length;
                            DrawRect(Cursor.X * Font.Width, Cursor.Y * Font.Height, InputBuffer.Length * Font.Width, Font.Height, ClearColor);
                            InputBuffer = CommandsHistory[CommandsHistory.Count - CommandIDX];
                            Write(InputBuffer);
                        }
                        break;
                    case ConsoleKeyEx.LeftArrow:
                        if (WriteIndex > 0)
                        {
                            Cursor.X--;
                            WriteIndex--;
                        }
                        break;
                    case ConsoleKeyEx.RightArrow:
                        if (WriteIndex < InputBuffer.Length)
                        {
                            Cursor.X++;
                            WriteIndex++;
                        }
                        break;
                }
            }
        }
        #endregion
        #endregion
    }
}
