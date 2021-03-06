﻿using Generate.Content;
using Generate.D2D;
using Generate.D3D;
using Generate.Input;
using Generate.Procedure;
using System;
using System.Collections.Generic;

namespace Generate
{
    class Program
    {
        internal static Renderer Renderer;
        internal static ChunkLoader Chunks;
        private static LoopWindow Window;
        internal static Overlay Overlay;

        internal static bool Close = false;
        internal static int VSync = 1;
        internal static bool DebugMode = false;
        private static uint Frames = 0;

        static void Main(string[] args)
        {
            Console.Title = "Generate CLI";

            Log("Seed? ");

            var Seed = string.Empty;
            while (true)
            {
                var Key = Console.ReadKey();
                if (Key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (Key.Key == ConsoleKey.Backspace)
                {
                    Console.Write(" ");
                    if (Seed.Length != 0)
                    {
                        Seed = Seed.Substring(0, Seed.Length - 1);
                        Console.Write("\b");
                    }

                    continue;
                }

                Seed += Key.KeyChar;
            }

            Worker.Master = new Master(Seed.ASCIIBytes());

            Constants.Load();
            
            using (Window = new LoopWindow())
            using (Renderer = new Renderer(Window))
            using (Overlay = new Overlay(Renderer.Device, Renderer.AntiAliasedBackBuffer))
            using (Chunks = new ChunkLoader())
            using (Sun.Main = new Sun(Constants.SunSeed))
            using (Skybox.Main = new Skybox(Constants.SkySeed))
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
        public static float FPS = 1f;

        static void Frame()
        {
            Processor.Process();
            
            Model ToLoad;
            for (int i = 0; i < 2 - VSync && Model.ModelsToLoad.TryPop(out ToLoad); i++)
            {
                ToLoad.Load();
            }

            Sun.Main.Tick();

            Renderer.PrepareShadow();
            Chunks.RenderVisible();
            Renderer.EndShadow();

            using (Renderer.PrepareCamera(Constants.Background))
            {
                Chunks.RenderVisible();

                ((CameraShader)Renderer.ActiveShader).DisableLighting();

                Skybox.Main.MoveWorld = Camera.Position;
                Skybox.Main.Render();
                Sun.Main.Render();
            }
            
            Overlay?.Start();
            Overlay?.DrawCrosshair();
            Overlay?.Draw($"Coords ({Camera.Position.X}, {Camera.Position.Y}, {Camera.Position.Z})", 10, 10, 600, 20);
            Overlay?.Draw($"Rotation ({Camera.RotationX}, {Camera.RotationY})", 10, 30, 600, 20);
            Overlay?.Draw($"Frames ({FPS}, VSync {VSync})", 10, 50, 600, 20);
            Overlay?.Draw($"Moved Chunks ({ChunkLoader.MovedX}, {ChunkLoader.MovedZ}) - Size {ChunkLoader.ChunkCountSide}", 10, 70, 600, 20);
            Overlay?.Draw($"F1/2/4 Anti Aliasing Count", 10, 90, 600, 20);
            Overlay?.Draw($"F9/10 Chunks, F11 Fullscreen, ESC Exit", 10, 110, 600, 20);
            Overlay?.Draw($"F8 WASD Space/Shift Movement", 10, 130, 600, 20);
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
            => System.Threading.Tasks.Task.Run(() => Console.Write($"[{DateTime.Now.ToLongTimeString()}] {From ?? "Main"} - {In}"));
    }
}
