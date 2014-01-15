using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AlgoTest
{
  public class Line
  {
    public int Radius { get; set; }
    public int Theta  { get; set; }
  }

  public class HoughTransform
  {
    protected int _Width, _Height;
    protected byte[,] _Mono;

    public static double Deg2Rad(double angle)
    {
      return (Math.PI * angle / 180.0);
    }

    public HoughTransform(byte[,] mono)
    {
      _Mono   = mono;
      _Width  = mono.GetLength(0);
      _Height = mono.GetLength(1);
    }

    public void DrawRotation(int rhos, Line line, ref Bitmap bitmap)
    {
      double theta = Deg2Rad(180 - line.Theta);
      double sinTheta = Math.Sin(theta);
      double cosTheta = Math.Cos(theta);
      int d = line.Radius - rhos;
      for (int x = 0; x < bitmap.Width; x++)
      {
        int y = (int)((cosTheta * x + d) / sinTheta);
        if (y > 0 && y < bitmap.Height)
          bitmap.SetPixel(x, y, Color.FromArgb(70, 170, 0));
      }
    }

    public int DetectAngle(int angleLoLimit = 60, int angleHiLimit = 120)
    {
      int by = _Height / 4; // 2 / 4 of the height
      int ey = _Height - by;
      int bx = _Width / 3;  // 1 / 3 of the width; 
      int ex = _Width - bx;

      Accumulator accu = new Accumulator(angleLoLimit, angleHiLimit);
      accu.Size = _Width + _Height;
      accu.Offset = accu.Rho / 2;

      for (int y = by; y < ey; ++y) 
      {
        for (int x = bx; x < ex; x += 3)  // skip every 3 pixels
        {
          if (_Mono[x, y] == 0)
            accu.Add(x, y);
        }
      }

      Line line = new Line();
      if (accu.Strongest(ref line))
        return (line.Theta);

      return (-1);
    }

    public bool DetectLine(
      int theta, int offsetTheta,
      int loLimitH, int hiLimitH,
      int loLimitW, int hiLimitW,
      ref Line line,
      bool vertical = true)
    {
      double sina = Math.Sin(Deg2Rad(theta));
      double cosa = Math.Cos(Deg2Rad(theta));
      theta = 180 - theta;    // correct for bin orientation

      Accumulator accu = new Accumulator(theta - offsetTheta, theta + offsetTheta);
      accu.Size = _Width + _Height;
      accu.Offset = accu.Rho / 2;

      int x, y;
      if (vertical)
      {
        for (int rho = loLimitW; rho < hiLimitW; rho += 1) // try 5 for speed
        {
          for (y = loLimitH; y < hiLimitH; y += 2)  // try 4 for speed
          {
            x = (int)((y * cosa + rho - accu.Rho) / sina);
            if (x > 0 && x < _Width && y > 0 && y < _Height)
            {
              if (_Mono[x, y] == 0)
                accu.Add(y, x);
            }
          }
        }
      }
      else
      {
        for (int rho = loLimitW; rho < hiLimitW; rho += 1) // try 5 for speed
        {
          for (y = loLimitH; y < hiLimitH; y += 2)  // try 4 for speed
          {
            x = (int)((y * cosa + rho - accu.Rho) / sina);
            if (x > 0 && x < _Width && y > 0 && y < _Height)
            {
              if (_Mono[x, y] == 0)
                accu.Add(x, y);
            }
          }
        }
      }

      if (accu.Strongest(ref line))
      {
        // correct the angles for the outside world
        line.Theta = 180 - line.Theta;
        return (true);
      }
      return (false);
    }
  }
}