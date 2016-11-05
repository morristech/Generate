﻿using Generate.D2D;
using Generate.D3D;
using Generate.Input;
using Generate.Procedure;
using System;
using System.Threading.Tasks;

namespace Generate
{
    class Program
    {
        internal static Renderer Renderer;
        private static LoopWindow Window;
        private static Overlay Overlay;

        internal static bool Close = false;
        internal static int VSync = 1;
        private static uint Frames = 0;

        static void Main(string[] args)
        {
            Log("Seed? ");
            Worker.Master = new Master(Console.ReadLine().AsciiBytes());
            Constants.Load();
            
            using (Window = new LoopWindow())
            using (Renderer = new Renderer(Window))
            using (Overlay = new Overlay(Renderer.Device, Renderer.AntiAliasedBackBuffer))
            using (var Loop = Window.Loop())
            {
                KeyboardMouse.StartCapture();
                Watch = new System.Diagnostics.Stopwatch();
                Watch.Start();

                while (!Close && Loop.NextFrame())
                {
                    Frame();
                }
            }
        }

        static System.Diagnostics.Stopwatch Watch;
        static float FPS = 0f;

        static void Frame()
        {
            Task.WhenAll(Processor.Process());

            Content.Model ToLoad;
            if (Content.Model.ModelsToLoad.TryPop(out ToLoad))
            {
                ToLoad.Load();
            }

            Renderer.PrepareShadow();
            Content.Chunk.RenderVisible();
            Renderer.EndShadow();

            using (Renderer.PrepareCamera(Constants.BG))
            {
                Content.Chunk.RenderVisible();
            }

            Overlay?.Start();
            Overlay?.DrawCrosshair();
            Overlay?.Draw($"Coords ({Camera.Position.X}, {Camera.Position.Y}, {Camera.Position.Z})", 10, 10, 500, 20);
            Overlay?.Draw($"Rotation ({Camera.RotationX}, {Camera.RotationY})", 10, 30, 500, 20);
            Overlay?.Draw($"Frames ({FPS}, VSync {VSync})", 10, 50, 500, 20);
            Overlay?.Draw($"Moved Chunks ({Content.Chunk.MovedX}, {Content.Chunk.MovedZ})", 10, 70, 500, 20);
            Overlay?.End();

            Frames++;
            if (Watch.ElapsedMilliseconds >= 1000)
            {
                FPS = (float)Frames / Watch.ElapsedMilliseconds * 1000;
                Watch.Restart();
                Frames = 0;
            }

            Renderer.EndCamera(VSync);
        }

        internal static void LogLine(object In, string From = null)
            => Log(In + "\r\n", From);

        internal static void Log(object In, string From = null)
            => Console.Write($"[{DateTime.Now.ToLongTimeString()}] {From ?? "Main"} - {In}");
    }
}
