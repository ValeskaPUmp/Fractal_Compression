using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fractal
{
    public unsafe class BlockArray
    {
        private static double u = 0.75d;
        public static int blockcheck=0;
        public static List<Coefficients> listcoef;
        
        public struct BlockArrayFull
        {
            public Block[,] Blocks;
            public int num_width;
            public int num_height;
            public int BlockSize;
        }
        public struct Block
        {
            public int X;
            public int Y;
            
            public int SumR;
            public int SumG;
            public int SumB;
            
            public int BlockSize;
           
            public int SumR2;
            public int SumG2;
            public int SumB2;
            public double Px;
            public double Py;
            public Coefficients Coeff;
            public bool Active;
            public int Depth;
        }
        public struct Coefficients
        {
            public int X;
            public int Y;
            public int rotate;
            public int shift;
            public int shiftR;
            public int shiftG;
            public int shiftB;
            public int Depth;
        }

        public static Block CreateRange(int x, int y, Color[,] cov, int rangeblock, double[,] brightness)
        {
            Block block = new Block()
            {
                X = x,
                Y = y,
                SumR = 0,
                SumG = 0,
                SumB = 0,
                Px = 0,
                Py = 0,
                BlockSize = rangeblock,
                Active = false
            };
            double doublebrightness = 0;
            for (int i = 0; i < rangeblock; ++i)
            {
                for (int j = 0; j < rangeblock; ++j)
                {
                    block.SumR += cov[block.X + i, block.Y + j].R;
                    block.SumG += cov[block.X + i, block.Y + j].G;
                    block.SumB += cov[block.X + i, block.Y + j].B;

                    block.SumR2 += (block.SumR * block.SumR);
                    block.SumG2 += (block.SumG * block.SumG);
                    block.SumB2 += (block.SumB * block.SumB);

                    doublebrightness += brightness[block.X + i, block.Y + j];
                    block.Px += i * brightness[block.X + i, block.Y + j];
                    block.Py += j * brightness[block.X + i, block.Y + j];
                    
                }
                
            }
            block.Px = (block.Px / doublebrightness) - ((rangeblock + 1) / 2);
            block.Py = (block.Py / doublebrightness) - ((rangeblock + 1) / 2);
            return block;
        }

        public static Block CreateDomain(int x, int y, Color[,] cov, int rangeblock, double[,] brightness)
        {
            Block block = new Block()
            {
                X = x,
                Y = y,
                SumR = 0,
                SumG = 0,
                SumB = 0,
                Px = 0,
                Py = 0,
                BlockSize = rangeblock,
                Active = false
            };
            double doublebrightness = 0;
            for (int i = 0; i < rangeblock; ++i)
            {
                for (int j = 0; j < rangeblock; ++j)
                {
                    block.SumR += cov[block.X + i * 2, block.Y + j * 2].R;
                    block.SumG += cov[block.X + i * 2, block.Y + j * 2].G;
                    block.SumB += cov[block.X + i * 2, block.Y + j * 2].B;

                    block.SumR2 += (block.SumR * block.SumR);
                    block.SumG2 += (block.SumG * block.SumG);
                    block.SumB2 += (block.SumB * block.SumB);

                    doublebrightness += brightness[block.X + i * 2, block.Y + j * 2];
                    block.Px += i * brightness[block.X + i * 2, block.Y + j * 2];
                    block.Py += j * brightness[block.X + i * 2, block.Y + j * 2];

                }

            }

            block.Px = (block.Px / doublebrightness) - ((rangeblock + 1) / 2);
            block.Py = (block.Py / doublebrightness) - ((rangeblock + 1) / 2);
            return block;
        }

        public double Angle(Block RangeBlock, Block DomainBlock)
        {
            double angle = 0;
            double v1x = RangeBlock.Px;
            double v2x = DomainBlock.Px;
            double v1y = RangeBlock.Py;
            double v2y = RangeBlock.Py;
            double vec1abs = QuickSqrt(v1x * v1x + v1y * v1y);
            double vec2abs = QuickSqrt(v2x * v2x + v2y * v2y);
            double scalar = v1x * v2x + v1y * v2y;
            return Math.Sin(scalar / (vec1abs * vec2abs));
        }
        private float QuickSqrt(double sqrt)
        {
            int ki = BitConverter.ToInt32(BitConverter.GetBytes(sqrt),0);
            int i = *(int*) &ki;
            int x = (1 << 29) + (i >> 1) - (1 << 22);
            return BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
        }
        public static double Shift(BlockArray.Block rangeBlock, BlockArray.Block domainBlock, int range_block_size, int flag)
        {
            double shift = 0;
            if (flag == 0)
                shift = ((rangeBlock.SumR) - (u * domainBlock.SumR)) / (range_block_size * range_block_size);
            if (flag == 1)
                shift = ((rangeBlock.SumG) - (u * domainBlock.SumG)) / (range_block_size * range_block_size);
            if (flag == 2)
                shift = ((rangeBlock.SumB) - (u * domainBlock.SumB)) / (range_block_size * range_block_size);
            return shift;
        }
        static public double DistanceQuad(Color[,] classImageColor, BlockArray.Block rangeBlock, Block domainBlock, int range_block_size, double shift)
        {
            double distance = 0;
            double rangeValue = 0;
            double domainValue = 0;
            for (int i = 0; i < range_block_size; ++i)
            {
                for (int j = 0; j < range_block_size; ++j)
                {
                    rangeValue = classImageColor[rangeBlock.X + i, rangeBlock.Y + j].R;
                    domainValue = classImageColor[domainBlock.X + (i * 2), domainBlock.Y + (j * 2)].R;
                    distance += Two(rangeValue, domainValue, shift);
                }
            }
            return distance;
        }
        static public double One(double rangeValue, double domainValue, double shift)
        {
            return (Math.Pow((rangeValue * shift - u * domainValue), 2));
        }

        static public double Two(double rangeValue, double domainValue, double shift)
        {
            return (Math.Pow((rangeValue + shift - u * domainValue), 2));
        }
    }
}