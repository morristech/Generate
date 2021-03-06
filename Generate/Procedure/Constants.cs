﻿using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Linq;

namespace Generate.Procedure
{
    static class Constants
    {
        internal static float StartLight;

        internal static RawColor4 Color;
        internal static RawColor4 Background;
        internal static Vector4 Light;

        internal static int TextureDensityPower;
        internal static int TextureDensity;
        internal static float HeightIntensity;

        internal static Vector3 BaseLightDirection;
        private static double SinCos1;
        private static double SinCos2;
        private static double SinCos3;

        internal static float StripeStart;
        internal static float StripeMultiplyFactor;

        internal static float DirtSmoothness;
        internal static bool RandomColor;

        internal static int BuildingDensity = 0;
        internal static int BuildingHeight;
        internal static float BuildingChance;

        internal static float FadeIntensity;
        internal static float[] FadeColor;

        internal static int SunSeed;
        internal static int SkySeed;
        internal static int StarDensity = 0;
        internal static float SunRotateTime;

        internal static float FogIntensity;
        internal static float FogBias;
        internal static float FogFactor;
        internal static float FogBackgroundFactor;

        internal static float CharSize;
        
        internal static void Load()
        {
            var Rand = new Worker("constants");

            var Hue = Rand.NextFloat();
            var Saturation = Rand.NextFloat();
            var Brightness = Rand.NextFloat();

            StartLight = Brightness * 0.3f;

            var Float = new[] { Hue, Saturation, Brightness }.ToRGB();
            Color = new RawColor4(Float[0], Float[1], Float[2], 1);

            Float = new[] { Hue, Saturation, Brightness * 0.5f + 0.5f }.ToRGB();
            Background = new RawColor4(Float[0], Float[1], Float[2], 1);

            Float = new[] { Hue, Saturation * 0.5f, Brightness * 0.25f + 0.75f }.ToRGB();
            Light = new Vector4(Float[0], Float[1], Float[2], 1);

            TextureDensityPower = Rand.Next(3, 8);
            TextureDensity = (int)Math.Pow(2, TextureDensityPower);
            HeightIntensity = (float)Math.Pow(Rand.NextDouble(), 4) * 128f;

            BaseLightDirection = new Vector3(
                Rand.NextFloat(-1f, 1f),
                Rand.NextFloat(-1f, -0.5f),
                Rand.NextFloat(-1f, 1f)
            );

            BaseLightDirection.Normalize();

            var Scale = (float)Math.Pow(2, Rand.Next(8, 17));
            SinCos1 = Rand.NextDouble() * Scale;
            SinCos2 = Rand.NextDouble() * Scale;
            SinCos3 = Math.Sqrt(SinCos1 * SinCos2);

            Texture.Handlers = Texture.Handlers.Where(x => Rand.Next(0, 5) < 3).ToArray();

            StripeStart = Rand.NextFloat(0.4f, 1f);
            StripeMultiplyFactor = 1f - StripeStart;

            DirtSmoothness = Rand.NextFloat(0.5f, 1f);
            RandomColor = Rand.Next(0, 2) == 1;

            if (HeightIntensity < 15f)
            {
                BuildingDensity = (int)Math.Pow(2, Rand.Next(1, 5));
                BuildingHeight = (int)Math.Pow(2, Rand.Next(1, 4));
                BuildingChance = Rand.NextFloat();
            }

            FadeIntensity = Rand.NextFloat();
            FadeColor = new[] { Rand.NextFloat(), Rand.NextFloat(), Rand.NextFloat() };

            SunSeed = Rand.Next();
            SkySeed = Rand.Next();

            if (Brightness < 0.5f)
            {
                StarDensity = (int)Math.Ceiling((0.5f - Brightness) * 26);
            }

            SunRotateTime = 1000f * Rand.Next(1, 4);

            FogIntensity = 0.2f * (float)Math.Pow(Brightness * Rand.NextFloat(), 0.5);
            FogBias = Rand.NextFloat(FogIntensity * 0.75f);
            FogFactor = (float)Math.Pow(Rand.NextFloat(0.075f, End: 0.14f), 1.25);
            FogBackgroundFactor = Rand.NextFloat(0.667f);

            CharSize = Rand.NextFloat(30f, 90f);
        }

        internal static float[,] GetHeights(long X, long Z)
        {
            var Places = new float[2, 2];

            for (int dX = 0; dX < 2; dX++)
            {
                for (int dZ = 0; dZ < 2; dZ++)
                {
                    Places[dX, dZ] = GetHeight(X + dX, Z + dZ);
                }
            }

            return Places;
        }

        private static float GetHeight(long X, long Z)
        {
            ulong uX = (ulong)X;
            ulong uZ = (ulong)Z;

            uint x1 = (uint)(uX & uint.MaxValue);
            uint x2 = (uint)(uX >> 32);

            uint z1 = (uint)(uZ & uint.MaxValue);
            uint z2 = (uint)(uZ >> 32);

            double Xin = x1 * SinCos1 + ~x2 * SinCos2 + Math.Pow(uX % 1234, 2) * SinCos3;
            double Zin = ~z1 * SinCos2 + z2 * SinCos1 + Math.Pow(uZ % 2345, 3) * SinCos3;

            return (float)(Math.Sin(Xin) * Math.Cos(Zin) * HeightIntensity) - HeightIntensity;
        }
    }
}
