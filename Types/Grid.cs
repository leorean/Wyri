using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Types
{
    public struct Grid<T>
    {
        private readonly T[][] data;

        public int Width { get; }
        public int Height { get; }

        public int Count { get => Width * Height; }

        public Grid(int w, int h)
        {
            Width = w;
            Height = h;

            data = new T[w][];
            for (var i = 0; i < w; i++)
            {
                data[i] = new T[h];
            }
        }

        public List<T> ToList()
        {
            List<T> t = new List<T>();

            for (var i = 0; i < Width * Height; i++)
            {
                t.Add(this[i]);
            }
            return t;
        }

        public T this[int i]
        {
            get
            {
                var x = i % Width;
                var y = (int)Math.Floor((double)(i / Width));

                if (x < 0 || y < 0 || x >= Width || y >= Height)
                    return default;

                return data[x][y];
            }
            set
            {
                var x = i % Width;
                var y = (int)Math.Floor((double)(i / Width));

                data[x][y] = value;
            }
        }

        public T this[int x, int y]
        {
            get
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height)
                    return default;

                return data[x][y];
            }
            set
            {
                data[x][y] = value;
            }
        }
    }
}
