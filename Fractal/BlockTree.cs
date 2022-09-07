using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.SymbolStore;
using System.Drawing;

namespace Fractal
{
    public class BlockTree
    {
        public BlockArray.Block main;
        private int mainx;
        private int mainy;
        private readonly int sumr;
        private readonly int sumg;
        private readonly int sumb;
        private int depth;
        private int blocksize;
        public BlockTree ul;
        public BlockTree ur;
        public BlockTree dl;
        public BlockTree dr;

        public BlockTree(BlockArray.Block block, int size)
        {
            main = block;
            mainx = block.X;
            mainy = block.Y;
            sumr = block.SumR;
            sumg = block.SumG;
            sumb = block.SumB;
            blocksize = block.BlockSize;
            depth = QuadTree.numlock;
            if (QuadTree.numlock == (-1))
            {
                QuadTree.numlock = 2;
            }
            else
            {
                ++QuadTree.numlock;
            }

            if (block.BlockSize > 2)
            {
                BlockArray.Block tmp = BlockArray.CreateRange(block.X, block.Y, QuadTree.classcolor, blocksize / 2,
                    QuadTree.brightness);
                ul = new BlockTree(tmp, tmp.BlockSize);
                tmp = BlockArray.CreateRange(block.X + blocksize / 2, block.Y, QuadTree.classcolor, blocksize / 2,QuadTree.brightness);
                ur = new BlockTree(tmp, tmp.BlockSize);
                tmp = BlockArray.CreateRange(block.X, block.Y + blocksize / 2, QuadTree.classcolor, blocksize / 2,
                    QuadTree.brightness);
                dl = new BlockTree(tmp, tmp.BlockSize);
                tmp = BlockArray.CreateRange(block.X + blocksize / 2, block.Y + blocksize / 2, QuadTree.classcolor,
                    blocksize / 2, QuadTree.brightness);
                dr = new BlockTree(tmp, tmp.BlockSize);

            }
        }
        public void RoundTree(BlockTree rangeTree, BlockArray.BlockArrayFull[] domainArray, Color[,] classImageColor, int range_block_size)
        {
            if (!(QuadTree.CheckMonotoneBlock(rangeTree.main)) && (rangeTree.main.BlockSize > 2))
            {
                RoundTree(rangeTree.ul, domainArray, classImageColor, rangeTree.main.BlockSize);
                RoundTree(rangeTree.ur, domainArray, classImageColor, rangeTree.main.BlockSize);
                RoundTree(rangeTree.dl, domainArray, classImageColor, rangeTree.main.BlockSize);
                RoundTree(rangeTree.dr, domainArray, classImageColor, rangeTree.main.BlockSize);
            }
            else
            {
                int current_x = 0;
                int current_y = 0;
                double current_distance = Double.MaxValue;
                double current_shiftR = 0;
                double current_shiftG = 0;
                double current_shiftB = 0;
                bool oneflag = false;
                int CurrentSize = 0;
                for (int i = 0; i < domainArray.Length; ++i)
                {
                    if ((domainArray[i].BlockSize) == (rangeTree.main.BlockSize * 2))
                    {
                        CurrentSize = i;
                    }
                }
                for (int k = 0; k < domainArray[CurrentSize].num_width; ++k)
                {
                    for (int l = 0; l < domainArray[CurrentSize].num_height; ++l)
                    {
                        double shiftR = BlockArray.Shift(rangeTree.main, domainArray[CurrentSize].Blocks[k, l], rangeTree.main.BlockSize, 0);
                        double shiftG = BlockArray.Shift(rangeTree.main, domainArray[CurrentSize].Blocks[k, l], rangeTree.main.BlockSize, 1);
                        double shiftB = BlockArray.Shift(rangeTree.main, domainArray[CurrentSize].Blocks[k, l], rangeTree.main.BlockSize, 2);
                        double distance = BlockArray.DistanceQuad(classImageColor, rangeTree.main, domainArray[CurrentSize].Blocks[k, l], rangeTree.main.BlockSize, shiftR);
                        if (distance < 1000000)
                        {
                            oneflag = true;
                            current_x = k;
                            current_y = l;
                            current_shiftR = shiftR;
                            current_shiftG = shiftG;
                            current_shiftB = shiftB;
                            current_distance = distance;
                            
                        }
                        else
                        {
                            if (distance < current_distance)
                            {
                                current_x = k;
                                current_y = l;
                                current_shiftR = shiftR;
                                current_shiftG = shiftG;
                                current_shiftB = shiftB;
                                current_distance = distance;
                            }
                        }
                        if (oneflag == true)
                            break;
                    }
                    if (oneflag == true)
                        break;
                }

                rangeTree.main.Active = true;
                rangeTree.main.Coeff.X = current_x;
                rangeTree.main.Coeff.Y = current_y;
                rangeTree.main.Coeff.shiftR = (int)current_shiftR;
                rangeTree.main.Coeff.shiftG = (int)current_shiftG;
                rangeTree.main.Coeff.shiftB = (int)current_shiftB;
                rangeTree.main.Coeff.Depth = rangeTree.main.Depth;
                if (BlockArray.blockcheck == 0)
                    rangeTree.main.Coeff.Depth *= (-1);
                ++BlockArray.blockcheck;
                BlockArray.listcoef.Add(rangeTree.main.Coeff);
                
            }
        }

        public void implementtree(BlockTree range, BlockArray.BlockArrayFull[] domain,ref Color[,] colnewimage)
        {
            if (range.main.Active == false)
            {
                implementtree(range.ul,domain, ref colnewimage);
                implementtree(range.ur,domain, ref colnewimage);
                implementtree(range.dr,domain, ref colnewimage);
                implementtree(range.dr,domain, ref colnewimage);
            }
            else
            {
                ++QuadTree.numlock;
                int presentsize = 0;
                for (int i = 0; i < domain.Length; ++i)
                {
                    if ((domain[i].BlockSize) == (range.main.BlockSize * 2))
                    {
                        presentsize = i;
                    }
                }

                BlockArray.Block domainblock = domain[presentsize].Blocks[range.main.Coeff.X, range.main.Coeff.Y];
                for (int x = 0; x < range.main.BlockSize; ++x)
                {
                    for (int y = 0; y < range.main.BlockSize; ++y)
                    {
                        Color col = colnewimage[domainblock.X + (x * 2), domainblock.Y + (y * 2)];
                        int r = (int)(0.75 * col.R + (range.main.Coeff.shiftR));
                        if (r < 0)
                            r = 0;
                        if (r > 255)
                            r = 255;
                        int g = (int)(0.75 * col.G + (range.main.Coeff.shiftG));
                        if (g < 0)
                            g = 0;
                        if (g > 255)
                            g = 255;
                        int b = (int)(0.75 * col.B + (range.main.Coeff.shiftB));
                        if (b < 0)
                            b = 0;
                        if (b > 255)
                            b = 255;
                        Color newColor = Color.FromArgb(b, g, r);
                        colnewimage[range.main.X + x, range.main.Y + y] = newColor;

                    }
                }


            }
        }
        
    }
}