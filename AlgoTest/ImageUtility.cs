using System;
using System.Drawing;

namespace AlgoTest
{
  class ImageUtility
  {
    static private void ConvertToGrayscale(Bitmap source, out byte[,] grayscale, out int[,] integral)
    {
      grayscale = new byte[source.Width, source.Height];
      integral = new int[source.Width, source.Height];

      LockBitmap bm = new LockBitmap(source);
      bm.LockBits();
      for (int y = 0; y < bm.Height; ++y)
      {
        for (int x = 0; x < bm.Width; ++x)
        {
          Color c = bm.GetPixel(x, y);
          integral[x, y] = grayscale[x, y] = (byte)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);
        }
      }

      bm.UnlockBits();
    }

    static private void SumIntegralImage(ref int[,] array)
    {
      int w = array.GetLength(0);
      int h = array.GetLength(1);

      // first row
      for (int i = 1; i < w; ++i)
        array[i, 0] += array[i - 1, 0];

      for (int j = 1; j < h; ++j)
      {
        array[0, j] += array[0, j - 1];
        for (int i = 1; i < w; ++i)
          array[i, j] += array[i - 1, j] + array[i, j - 1] - array[i - 1, j - 1];
      }
    }

    static public byte[,] ConvertToBW(Bitmap source, int boxSize = 5, int c = -5)
    {
      if (boxSize < 5)
        boxSize = 5;

      int w = source.Width;
      int h = source.Height;
      int s = (boxSize * 2 + 1) * (boxSize * 2 + 1);

      byte[,] grayscale;
      int[,] integral;

      ConvertToGrayscale(source, out grayscale, out integral);
      SumIntegralImage(ref integral);

      byte mean;
      byte[,] binary = new byte[w - 2 * boxSize - 1, h - 2 * boxSize - 1];

      for (int i = 1 + boxSize; i < w - boxSize; ++i)
      {
        for (int j = 1 + boxSize; j < h - boxSize; ++j)
        {
          mean = (byte)(Math.Max(0, c + (
            integral[i + boxSize, j + boxSize] -
            integral[i + boxSize, j - boxSize - 1] -
            integral[i - boxSize - 1, j + boxSize] +
            integral[i - boxSize - 1, j - boxSize - 1]) / s));

          binary[i - boxSize - 1, j - boxSize - 1] = (byte)(grayscale[i, j] < mean ? 0 : 255);
        }
      }

      return (binary);
    }
  }

}