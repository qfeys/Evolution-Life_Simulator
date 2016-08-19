using System;
using System.Collections.Generic;

namespace Maps
{
    /// <summary>
    /// The Light map has a value at each pointmeasured in Klux, where full sunlight can go up to 100 Klux
    /// </summary>
    static class Light
    {
        static float[,] map = new float[Simulation.mapsize, Simulation.mapsize];
        static double max = 60;

        public static void Initialise()
        {
            int radius = Simulation.mapsize / 2;
            for (int i = 0; i < Simulation.mapsize; i++)
            {
                for (int j = 0; j < Simulation.mapsize; j++)
                {
                    // sqrt( radius^2 - (i-radius)^2 - (j-radius)^2)* max / radius
                    map[i, j] = (float)(Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(i - radius, 2) - Math.Pow(j - radius, 2)) * max / radius);
                    //IOHandler.DebugWriteToFile("" + i + ", " + j + ", " + map[i, j]);
                    if (float.IsNaN(map[i, j])) map[i, j] = 0;
                }
            }
        }

        /// <summary>
        /// Returns the biliniear interploation of the nearest gridpoints
        /// </summary>
        public static float GetValue(UnityEngine.Vector2 v)
        {
            return GetValue(v.x, v.y);
        }

        /// <summary>
        /// Returns the biliniear interploation of the nearest gridpoints
        /// </summary>
        public static float GetValue(float x, float y)
        {
            if (x < 0 | x >= Simulation.mapsize - 1 | y < 0 | y >= Simulation.mapsize - 1) return 0;
            int x0 = (int)x; int y0 = (int)y;
            float xf = x - x0; float yf = y - y0;
            return map[x0, y0] * (1 - xf) * (1 - yf) + map[x0 + 1, y0] * xf * (1 - yf) +
                map[x0, y0 + 1] * (1 - xf) * yf + map[x0 + 1, y0 + 1] * xf * yf;
        }

        static public UnityEngine.Texture2D GetImage(float xmin, float xmax, float ymin, float ymax, int ppu)
        {
            // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
            var texture = new UnityEngine.Texture2D((int)((xmax-xmin)*ppu), (int)((ymax-ymin)*ppu), UnityEngine.TextureFormat.ARGB32, false);
            UnityEngine.Debug.Log("xmin: " + xmin + ", ymin: " + ymin + ", ppu: " + ppu);
            UnityEngine.Debug.Log("xmax: " + xmax + ", ymax: " + ymax + ", range: " + (xmax - xmin) * ppu);
            // set the pixel values
            for (int i = 0; i < (xmax - xmin) * ppu; i++)
            {
                float x = xmin + (float)i / ppu;
                for (int j = 0; j < (ymax - ymin) * ppu; j++)
                {
                    float y = ymin + (float)j / ppu;
                    float v = (float)(GetValue(x, y) / max);
                    texture.SetPixel(i, j, new UnityEngine.Color(0.5f + v / 2, v, 0.5f + v / 2));
                }
            }
            texture.SetPixel(0, 0, new UnityEngine.Color(1.0f, 1.0f, 1.0f, 0.5f));
            texture.SetPixel(1, 0, UnityEngine.Color.clear);
            texture.SetPixel(0, 1, UnityEngine.Color.white);
            texture.SetPixel(1, 1, UnityEngine.Color.black);

            // Apply all SetPixel calls
            texture.Apply();
            return texture;

        }
    }
}