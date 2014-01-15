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

    public Point LT;
    public Point RT;
    public Point LB;
    public Point RB;
  }

  public class Angles
  {
    public double SinTheta1  { get; private set; }
    public double CosTheta1  { get; private set; }
    public double CosDivSin1 { get; private set; }

    public double SinTheta2  { get; private set; }
    public double CosTheta2  { get; private set; }
    public double CosDivSin2 { get; private set; }

    public Angles(int theta)
    {
      SinTheta1 = Math.Sin(HoughTransform.Deg2Rad(theta));
      CosTheta1 = Math.Cos(HoughTransform.Deg2Rad(theta));
      CosDivSin1 = CosTheta1 / SinTheta1;

      int theta2 = 180 - theta;
      SinTheta2 = Math.Sin(HoughTransform.Deg2Rad(theta2));
      CosTheta2 = Math.Cos(HoughTransform.Deg2Rad(theta2));
      CosDivSin2 = CosTheta2 / SinTheta2;
    }
  }

  public class Table
  {
    private byte[,] _Mono;
    private List<Line> _Row;
    private List<Line> _Col;
    private Rect _Range;
    public Rect Range
    {
      get { return (_Range); }
      set
      { 

      }
    }

    public Table(byte[,] mono, Rect rect)
    {
      _Mono = mono;
    }

    public bool DetectGrids(Angles anges, HoughTransform _detect)
    {
      int offBottom = LB.Y + (LT.Y - LB.Y) / 9;
      int offTop = LT.Y - (LT.Y - LB.Y) / 9;
      return (true);
    }
  }

  public class WhiteStrips
  {
    private double sinTheta1;
    private double cosTheta1;
    private double cosDivSin1;
    private double sinTheta2;
    private double cosTheta2;
    private double cosDivSin2;
    private int _Height;
    private int _Width;

    public int[] WhiteH { get; private set; }
    public int[] WhiteV { get; private set; }


    public WhiteStrips(byte[,] mono)
    {
      sinTheta1 = cosTheta1 = cosDivSin1 = 0;
      sinTheta2 = cosTheta2 = cosDivSin2 = 0;
    }

    public bool DetectWhiteRegion(int theta, int rect = 1)
    {
      int threshold = (rect * 2 + 1) * (rect * 2 + 1)/2;

      int x, y, rho;
      int beg, end;
      int sum;
      int maxValue;

      WhiteV = new int[_Width];      // vertical uninterrupted white pixel counter
      WhiteH = new int[_Height];     
      ////////////////////////////////////
      // HORIZONTAL white strips detection
      beg = _Rhos;
      end = _Rhos + (int)(_Height * sinTheta2);
      for (rho = beg; rho < end; rho += 3) // todo: try with 5 for performance
      {
        // sweep down->up with parallel lines and count white pixels. Our goal is to find longest uninterrupted white strip line. Strip consists of rectangles 3x3 pixels.
        int whiteLen = 0;
        int longestwhiteLen = 0;
        for (x = rect; x < _Width - rect; x += 2)
        {
          y = (int)((x * cosTheta2 + rho - _Rhos) / sinTheta2);
          if (y >= rect && y < _Height - rect)
          {
            sum = 0;
            for (int yy = y - rect; yy <= y + rect; ++yy) 
            {
              for (int xx = x - rect; xx <= x + rect; ++xx)
              {
                if (_Mono[xx, yy] == 0)
                  ++sum;
              }
            }
            
            if (threshold > sum)  // white
              ++whiteLen;
            else        // black
            {
              if (longestwhiteLen < whiteLen)
                longestwhiteLen = whiteLen;
              whiteLen = 0;
            }
          }
          if (longestwhiteLen < whiteLen)
            longestwhiteLen = whiteLen;
        }
        WhiteH[rho - beg] = longestwhiteLen;
      }
      
      ////////////////////////////////////////
      // detect 2 peaks of white horizontal strips
      maxValue = 0;
      for (y = 1 + _Height / 2; y > 0; y--)
      {
        if (maxValue < WhiteH[y])
        {
          maxValue = WhiteH[y];
          bottom = y + beg;
        }
      }

      maxValue = 0;
      for (y = _Height / 2; y < _Height - 1; ++y)
      {
        if (maxValue < whiteH[y])
        {
          maxValue = whiteH[y];
          top = y + beg;
        }
      }

      //////////////////////////////////
      // VERTICAL white strips detection
      beg = _Rhos;
      end = _Rhos + (int)(_Width * sinTheta1);
      
      for (rho = beg; rho < end; rho += 3)      // todo: try 5 for performance
      {
        int whiteLen = 0;
        int longestwhiteLen = 0;
        
        int yD = (int)(((rho - beg) * cosTheta2 + bottom - _Rhos) / sinTheta2);
        int yU = yD + top - bottom;
        
        for (y = yD; y < yU; y += 2)
        {
          x = (int)((y * cosTheta1 + rho - _Rhos) / sinTheta1);
          if (x >= rect && x < _Width - rect && y >= rect && y < _Height - rect)
          {
            sum = 0;
            for (int yy = y - rect; yy <= y + rect; ++yy)
            {
              for (int xx = x - rect; xx <= x + rect; ++xx)
              {
                if (_Mono[xx, yy] == 0)
                  ++sum;
              }
            }

            if (threshold > sum)  // white
              ++whiteLen;
            else        // black
            {
              if (longestwhiteLen < whiteLen)
                longestwhiteLen = whiteLen;
              whiteLen = 0;
            }
          }

          if (longestwhiteLen < whiteLen)
            longestwhiteLen = whiteLen;
        }
        whiteV[rho - beg] = longestwhiteLen;
      }

      // detect 2 peaks of white vertical strips
      maxValue = 0;
      for (x = 1 + _Width / 10; x > 0; --x)
      {
        if (maxValue < whiteV[x])
        {
          maxValue = whiteV[x];
          left = x + beg;
        }
      }

      maxValue = 0;
      for (x = _Width / 10; x < _Width - 1; ++x)
      {
        if (maxValue < whiteV[x])
        {
          maxValue = whiteV[x];
          right = x + beg;
        }
      }
    }

    public void DrawRect(Color color, ref Bitmap bitmap)
    {

      int x, y;

      // left line
      for (y = lb.Y; y < lt.Y; ++y)
      {
        x = (int)(y * cosDivSin1 + (left - _Rhos) / sinTheta1);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
      
      // bottom line
      for (x = lb.X; x < rb.X; ++x)
      {
        y = (int)(x * cosDivSin2 + (bottom - _Rhos) / sinTheta2);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
      
      // right line
      for (y = rb.Y; y < rt.Y; ++y)
      {
        x = (int)(y * cosDivSin1 + (right - _Rhos) / sinTheta1);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
      // top line
      for (x = lt.X; x < rt.X; ++x)
      {
        y = (int)(x * cosDivSin2 + (top - _Rhos) / sinTheta2);
        if (x > 0 && x < _Width && y > 0 && y < _Height)
          bitmap.SetPixel(x, y, color);
      }
    }
    public Rect CalcIntersections(int rhos, int left, int top, int right, int bottom)
    {
      ///////////////////////////////
      // calculate line intersections points of white rectangle
      Rect rect = new Rect();
      double temp = (1 - cosDivSin1 * cosDivSin2);

      double c1 = (left - rhos) / sinTheta1;
      double c2 = (top - rhos) / sinTheta2;
      rect.LT.X = (int)((c2 * cosDivSin1 + c1) / temp);
      rect.LT.Y = (int)(rect.LT.X * cosDivSin2 + c2);

      c2 = (bottom - rhos) / sinTheta2;
      rect.LB.X = (int)((c2 * cosDivSin1 + c1) / temp);
      rect.LB.Y = (int)(rect.LB.X * cosDivSin2 + c2);

      c1 = (right - rhos) / sinTheta1;
      rect.RB.X = (int)((c2 * cosDivSin1 + c1) / temp);
      rect.RB.Y = (int)(rect.RB.X * cosDivSin2 + c2);

      c2 = (top - rhos) / sinTheta2;
      rect.RT.X = (int)((c2 * cosDivSin1 + c1) / temp);
      rect.RT.Y = (int)(rect.RT.X * cosDivSin2 + c2);

      return (rect);
    }

    public bool DetectRect(int theta)
    {
      sinTheta1 = Math.Sin(Deg2Rad(theta));
      cosTheta1 = Math.Cos(Deg2Rad(theta));

      int theta2 = 180 - theta;
      sinTheta2 = Math.Sin(Deg2Rad(theta2));
      cosTheta2 = Math.Cos(Deg2Rad(theta2));
      cosDivSin1 = cosTheta1 / sinTheta1;
      cosDivSin2 = cosTheta2 / sinTheta2;

      DetectWhiteRegion(1);

      ///////////////////////////////
      // calculate line intersections points of white rectangle
      double temp = (1 - cosDivSin1 * cosDivSin2);
      double c1, c2;

      c1 = (left - _Rhos) / sinTheta1;
      c2 = (top - _Rhos) / sinTheta2;
      lt.X = (int)((c2 * cosDivSin1 + c1) / temp);
      lt.Y = (int)(lt.X * cosDivSin2 + c2);
  
      c2 = (bottom - _Rhos) / sinTheta2;
      lb.X = (int)((c2 * cosDivSin1 + c1) / temp);
      lb.Y = (int)(lb.X * cosDivSin2 + c2);
      
      c1 = (right - _Rhos) / sinTheta1;
      rb.X = (int)((c2 * cosDivSin1 + c1) / temp);
      rb.Y = (int)(rb.X * cosDivSin2 + c2);
  
      c2 = (top - _Rhos) / sinTheta2;
      rt.X = (int)((c2 * cosDivSin1 + c1) / temp);
      rt.Y = (int)(rt.X * cosDivSin2 + c2);

      return (true);
    }
  }
}
