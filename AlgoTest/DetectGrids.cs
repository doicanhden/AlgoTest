using System;
using System.Drawing;
using System.Collections.Generic;
namespace AlgoTest
{
  public class Rect
  {
    public Rect() { }
    public Rect(Rect other)
    {
      LT = other.LT;
      RT = other.RT;
      LB = other.LB;
      RB = other.RB;
    }
    public Rect(Point leftTop, Point leftBottom, Point rightTop, Point rightBottom)
    {
      LT = leftTop;
      RT = rightTop;
      LB = leftBottom;
      RB = rightBottom;
    }
    public Point LT;
    public Point RT;
    public Point LB;
    public Point RB;
  }



  public class DetectGrids
  {
    private double sinTheta1;
    private double cosTheta1;
    private double cosDivSin1;
    private double sinTheta2;
    private double cosTheta2;
    private double cosDivSin2;
    private byte[,] _Mono;
    private int _Height;
    private int _Width;
    private int _Rhos;
    private int _Left, _Top, _Right, _Bottom;
    public Point LeftTop     { get; private set; }
    public Point LeftBottom  { get; private set; }
    public Point RightTop    { get; private set; }
    public Point RightBottom { get; private set; }

    static int MaxInRange(int[] array, int loLimit, int hiLimit, bool reverse = false)
    {
      int max, maxIdx;
      if (reverse)
      {
        max = array[hiLimit];
        maxIdx = hiLimit;
        for (int i = hiLimit - 1; i > loLimit; --i)
        {
          if (max < array[i])
          {
            max = array[i];
            maxIdx = i;
          }
        }
      }
      else
      {
        max = array[loLimit];
        maxIdx = loLimit;
        for (int i = loLimit + 1; i < hiLimit; ++i)
        {
          if (max < array[i])
          {
            max = array[i];
            maxIdx = i;
          }
        }
      }

      return (maxIdx);
    }

    public DetectGrids(byte[,] mono, int rhos, int theta)
    {
      _Width = mono.GetLength(0);
      _Height = mono.GetLength(1);

      sinTheta1 = Math.Sin(Deg2Rad(theta));
      cosTheta1 = Math.Cos(Deg2Rad(theta));
      cosDivSin1 = cosTheta1 / sinTheta1;

      int theta2 = 180 - theta;
      sinTheta2 = Math.Sin(Deg2Rad(theta2));
      cosTheta2 = Math.Cos(Deg2Rad(theta2));
      cosDivSin2 = cosTheta2 / sinTheta2;

      _Rhos = rhos;
      _Mono = mono;

    }
    
    public static double Deg2Rad(double angle)
    {
      return (Math.PI * angle / 180.0);
    }
    private static int NBlackPixels(byte[,] mono, int x, int y, int rect)
    {
      int result = 0;
      for (int j = -rect; j <= rect; ++j) 
      {
        for (int i = -rect; i <= rect; ++i)
        {
          if (mono[x + i, y + j] == 0)
            ++result;
        }
      }
      return (result);
    }
    public void DetectRectangleAround(int rect = 1)
    {
      int[] white;
      int nWhiteSpace, nWhiteSpaceMax = 0;

      int x, y;
      int step, size;
      int threshold = (rect * 2 + 1) * (rect * 2 + 1) / 3;



      ////////////////////////////////////
      // HORIZONTAL white strips detection
      step = 3; // todo: try 5 for performance
      size = (int)(_Height * sinTheta2) / step;
      white = new int[size];

      for (int rho = 0; rho < size; ++rho)
      {
        // sweep down->up with parallel lines and count white pixels.
        // Our goal is to find longest uninterrupted white strip line.
        // Strip consists of rectangles 3x3 pixels.
        nWhiteSpace = 0;
        nWhiteSpaceMax = 0;
        for (x = rect; x < _Width - rect; x += 4)
        {
          y = (int)((x * cosTheta2 + rho * step) / sinTheta2);
          if (y >= rect && y < _Height - rect)
          {
            if (threshold > NBlackPixels(_Mono, x, y, rect))
              ++nWhiteSpace;
            else
            {
              if (nWhiteSpaceMax < nWhiteSpace)
                nWhiteSpaceMax = nWhiteSpace;
              nWhiteSpace = 0;
            }
          }

          if (nWhiteSpaceMax < nWhiteSpace)
            nWhiteSpaceMax = nWhiteSpace;
        }
        white[rho] = nWhiteSpaceMax;
      }

      ////////////////////////////////////////
      // detect 2 peaks of white horizontal strips
      _Top = _Rhos + MaxInRange(white, size / 2, size - 1) * step;
      _Bottom = _Rhos +  MaxInRange(white, 0, size / 2 - 1 , true) * step;



      //////////////////////////////////
      // VERTICAL white strips detection
      int yLo, yHi, yOffset = _Bottom - _Rhos;

      step = 3; // todo: try 5 for performance
      size = (int)(_Width * sinTheta1) / step;
      white = new int[size];

      for (int rho = 0; rho < size; ++rho)
      {
        nWhiteSpace = 0;
        nWhiteSpaceMax = 0;

        yLo = (int)((rho * step * cosTheta2 + yOffset) / sinTheta2);
        yHi = yLo + _Top - _Bottom;
        
        for (y = yLo; y < yHi; y += 4)
        {
          x = (int)((y * cosTheta1 + rho * step) / sinTheta1);
          if (x >= rect && x < _Width - rect && y >= rect && y < _Height - rect)
          {
            if (threshold > NBlackPixels(_Mono, x, y, rect))
              ++nWhiteSpace;
            else
            {
              if (nWhiteSpaceMax < nWhiteSpace)
                nWhiteSpaceMax = nWhiteSpace;
              nWhiteSpace = 0;
            }
          }

          if (nWhiteSpaceMax < nWhiteSpace)
            nWhiteSpaceMax = nWhiteSpace;
        }
        white[rho] = nWhiteSpaceMax;
      }

      // detect 2 peaks of white vertical strips
      _Right = _Rhos + MaxInRange(white, size / 2, size - 1) * step;
      _Left = _Rhos + MaxInRange(white, 0, size / 2 - 1, true) * step;




      ///////////////////////////////
      // calculate line intersections points of white rectangle
      double temp = (1 - cosDivSin1 * cosDivSin2);
      double c1, c2;

      c1 = (_Left - _Rhos) / sinTheta1;
      c2 = (_Top - _Rhos) / sinTheta2;
      x = (int)((c2 * cosDivSin1 + c1) / temp);
      y = (int)(x * cosDivSin2 + c2);
      LeftTop = new Point(x, y);

      c2 = (_Bottom - _Rhos) / sinTheta2;
      x = (int)((c2 * cosDivSin1 + c1) / temp);
      y = (int)(x * cosDivSin2 + c2);
      LeftBottom = new Point(x, y);
      
      c1 = (_Right - _Rhos) / sinTheta1;
      x = (int)((c2 * cosDivSin1 + c1) / temp);
      y = (int)(x * cosDivSin2 + c2);
      RightBottom = new Point(x, y);

      c2 = (_Top - _Rhos) / sinTheta2;
      x = (int)((c2 * cosDivSin1 + c1) / temp);
      y = (int)(x * cosDivSin2 + c2);
      RightTop = new Point(x, y);
    }

    public void DrawRect(Rect rect, Color color, ref Bitmap bitmap)
    {

      int x, y;

      // left line
      for (y = rect.LB.Y; y < rect.LT.Y; ++y)
      {
        x = (int)(y * cosDivSin1 + (_Left - _Rhos) / sinTheta1);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
      // bottom line
      for (x = rect.LB.X; x < rect.RB.X; ++x)
      {
        y = (int)(x * cosDivSin2 + (_Bottom - _Rhos) / sinTheta2);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
      // right line
      for (y = rect.RB.Y; y < rect.RT.Y; ++y)
      {
        x = (int)(y * cosDivSin1 + (_Right - _Rhos) / sinTheta1);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
      // top line
      for (x = rect.LT.X; x < rect.RT.X; ++x)
      {
        y = (int)(x * cosDivSin2 + (_Top - _Rhos) / sinTheta2);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
    }
  }
}
