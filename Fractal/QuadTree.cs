using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Fractal
{
    public unsafe class QuadTree 
    {
        private Bitmap image;
        private int colorbgr;
        private int imagewidth, imageheight,rangewidth,rangeheight;
        private int rangebuffer = 64;
        private BlockArray.Block[,] blockarray;
        private BlockTree[,] rangetree;
        public static Color[,] classcolor;
        public static double[,] brightness;
        public static int numlock;
        private BlockArray.Block[,] domain;
        private BlockArray.BlockArrayFull[] domainblocks;
        public static bool CheckMonotoneBlock(BlockArray.Block block)
        {
            int count = 0;
            Color currentColor = classcolor[block.X, block.Y];
            for (int i = 0; i < block.BlockSize - 1; ++i)
            {
                for (int j = 1; j < block.BlockSize - 1; ++j)
                {
                    if (classcolor[block.X + i, block.Y + j] != classcolor[block.X + i + 1, block.Y + j + 1])
                    {
                        ++count;
                    }
                }
            }
            return (count < ((block.BlockSize)));
        }

        public  void Compression(string filepath,string quality)
        {
            if (!File.Exists(filepath) || !filepath.EndsWith(".bmp"))
            {
                throw new Exception("Not found this file or this file not correct");
            }
            else
            {
                if (quality == "Black")
                {
                    colorbgr = 1;

                }
                else
                {
                    if (quality == "Color")
                    {
                        colorbgr = 3;
                    }
                    else
                    {
                        throw new Exception("Current quality not correct");
                    }
                }

                image = new Bitmap(filepath);
                imageheight = image.Height;
                imagewidth = image.Width;
                rangewidth = imagewidth / rangebuffer;
                rangeheight = imageheight / rangebuffer;
                classcolor = new Color[imagewidth,imageheight];
                brightness = new double[imagewidth, imageheight];
                for (int i = 0; i < imagewidth; ++i)
                {
                    for (int j = 0; j < imageheight; ++j)
                    {
                        Color c = image.GetPixel(i, j);
                        classcolor[i, j] = c;
                        brightness[i, j] = c.GetBrightness();
                    }
                    
                }

                blockarray = new BlockArray.Block[rangewidth, rangeheight];
                for (int i = 0; i < rangewidth; ++i)
                {
                    for (int j = 0; j < rangeheight; ++j)
                    {
                        blockarray[i, j] = BlockArray.CreateRange(i, j, classcolor, rangebuffer, brightness);

                    }
                    
                }

                rangetree = new BlockTree[rangewidth , rangeheight];
                for (int i = 0; i < rangewidth; ++i)
                {
                    for (int j = 0; j < rangeheight; ++j)
                    {
                        numlock = 1;
                        rangetree[i, j] = new BlockTree(blockarray[i, j], blockarray[i, j].BlockSize);

                    }
                    
                }

                domain = new BlockArray.Block[rangewidth - 1, rangeheight - 1];
                domainblocks = new BlockArray.BlockArrayFull[powed(rangebuffer * 2)];
                for (int i = 0; i < domainblocks.Length; ++i)
                {
                    if (i == 0)
                    {
                        domainblocks[i].BlockSize = rangebuffer * 2;
                    }
                    else
                    {
                        domainblocks[i].BlockSize = domainblocks[i - 1].BlockSize / 2;
                        domainblocks[i].num_width = imagewidth / domainblocks[i].BlockSize;
                        domainblocks[i].num_height = imageheight / domainblocks[i].BlockSize;
                        domainblocks[i].Blocks =
                            new BlockArray.Block[domainblocks[i].num_width, domainblocks[i].num_height];
                        for (int j = 0; j < domainblocks[i].num_width; ++j)
                        {
                            for (int k = 0; k < domainblocks[i].num_height; ++k)
                            {
                                domainblocks[i].Blocks[j, k] = BlockArray.CreateDomain(
                                    j * domainblocks[i].BlockSize / 2, k * domainblocks[i].BlockSize / 2, classcolor,
                                    domainblocks[i].BlockSize / 2, brightness);
                            }
                        }
                        
                    }
                    
                }

                BlockArray.listcoef = new List<BlockArray.Coefficients>();
                for (int i = 0; i <rangewidth; ++i)
                {
                    for (int j = 0; j < rangeheight; ++j)
                    {
                        BlockArray.blockcheck = 0;
                        rangetree[i,j].RoundTree(rangetree[i,j],domainblocks,classcolor,rangetree[i,j].main.BlockSize);

                    }
                    
                }

                Bitmap newimage = new Bitmap(imagewidth, imageheight);
                Color[,] colnewimage = new Color[imagewidth, imageheight];
                for (int i = 0; i < imagewidth; ++i)
                { 
                    for (int j = 0; j < imageheight; ++j)
                    {
                        colnewimage[i, j] = newimage.GetPixel(i, j);

                    }
                    
                }

                for (int i = 0; i < 10; ++i)
                {
                    for (int j = 0; j < rangewidth; ++j)
                    {
                        for (int k = 0; k <rangeheight; ++k)
                        {
                            rangetree[j, k].implementtree(rangetree[j, k], domainblocks,ref colnewimage);
                        }
                    }
                    
                }

                for (int i = 0; i < newimage.Width; ++i)
                {
                    for (int j = 0; j < newimage.Height; ++j)
                    {
                        newimage.SetPixel(i,j,colnewimage[i,j]);
                    }
                }
                newimage.Save("compressionphoto.bmp");
            }
            
            
            
        }

        private static int powed(int num)
        {
            for (int i = 0; i < num; ++i)
            {
                int conn = (int)Math.Pow((2), i);
                if (conn == num)
                {
                    return i;
                }
            }

            return 1;

        }


        
    }
}