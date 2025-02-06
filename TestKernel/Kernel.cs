using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
using StarDustCosmos;
using Sys = Cosmos.System;

namespace CosmosKernel1
{
    public class Kernel : Sys.Kernel
    {
        [ManifestResourceStream(ResourceName = "CosmosKernel1.Logo.bmp")]
        static byte[] file;

        Canvas canv;
        Terminal term;
        protected override void BeforeRun()
        {
            canv = FullScreenCanvas.GetFullScreenCanvas(new Mode(1280,720,ColorDepth.ColorDepth32));
            term = new(Sys.Graphics.Fonts.PCScreenFont.Default,canv,1280,720);
            term.ClearColor = Color.Black;
            term.Clear();

            term.WriteImage(new Bitmap(file));
            term.Cursor.X = 14;
            term.Cursor.Y = 2;
            term.TextColor = Color.LightGreen;
            Console.Write("TestOS - a StarDust terminal example");
            term.TextColor = Color.LightGray;
            term.Cursor.X = 14;
            term.Cursor.Y = 3;
            Console.Write("CPU: "+CPU.GetCPUBrandString());
            term.Cursor.X = 14;
            term.Cursor.Y = 4;
            Console.Write("RAM: " + GCImplementation.GetUsedRAM() + " / " + GCImplementation.GetAvailableRAM());

            term.Cursor.X = 0;
            term.Cursor.Y = 8;
            Console.Write(term.InputPrefix);

            term.Commands.Add("echo", (string[] args) =>
            {
                string buffer = "";
                foreach (var item in args)
                {
                    buffer += item + " ";
                }
                buffer = buffer.Remove(0, args[0].Length + 1);
                Console.WriteLine(buffer);
            });
            term.Commands.Add("hw", (string[] args) => 
            {
                Console.WriteLine("hello world");
            });
            term.Commands.Add("host", (string[] args) =>
            {
                Console.WriteLine("hello world");
            });
            term.Commands.Add("helloWorld", (string[] args) =>
            {
                Console.WriteLine("hello world");
            });
            term.Commands.Add("label", (string[] args) =>
            {
                term.WriteImage(new Bitmap(file));
            });
            term.Commands.Add("clear", (string[] args) =>
            {
                term.Clear();
            });
        }

        protected override void Run()
        {
            term.UpdateInput();
            term.Draw(0,0);
            canv.Display();
            Heap.Collect();
        }
    }
}
