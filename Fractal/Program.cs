using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fractal
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            QuadTree q = new QuadTree();
            q.Compression("C:\\Users\\Валентин\\Pictures\\sample_1920×1280.bmp","Color");
        }

    }
}