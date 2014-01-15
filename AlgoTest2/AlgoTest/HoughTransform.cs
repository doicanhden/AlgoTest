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
    int _Width, _Height, _Rhos;
    byte[,] _Mono;

    public int Rhos { get { return _Rhos; } }

    public static double Deg2Rad(double angle)
    {
      return (Math.PI * angle / 180.0);
    }
    private class Accumulator
    {
      int _Theta1, _Theta2;
      int _RhosLo, _RhosHi, _RhoOff;
      int[,] _Votes;
      int _Width, _Height;
      public Accumulator(int Width, int Height, int Theta1, int Theta2, int MaxNLines = 7)
      {
        int Rhos = (int)((Width + Height) * Math.Sqrt(2));
        _RhoOff = Rhos / 2;
        _RhosLo = Rhos / 7;
        _RhosHi = Rhos - Rhos / 7;
        _Theta1 = Theta1;
        _Theta2 = Theta2;
        _Votes  = new int[180, Rhos];
        _Width = Width;
        _Height = Height;
      }

      public void Add(int x, int y)
      {
        int _Radius;
        double _Theta;
        for (int theta = _Theta1; theta <= _Theta2; ++theta)
        {
          _Theta = Deg2Rad(theta);
          _Radius = (int)(x * Math.Cos(_Theta) + y * Math.Sin(_Theta) + _RhoOff);
          ++_Votes[theta, _Radius];
        }
      }

      public bool Strongest(ref Line Line1, ref Bitmap bitmap)
      {
        int vote = -1;
        for (int rho = _RhosLo; rho < _RhosHi; ++rho)
        {
          for (int theta = _Theta1; theta <= _Theta2; ++theta)
          {
            if (_Votes[theta, rho] > vote)
            {
              vote = _Votes[theta, rho];
              Line1.Theta = theta;
              Line1.Radius = rho;
            }
          }
        }
        if (vote == -1)
          return (false);
        
/*        byte b;
        double factor = _Height / ((_Width + _Height) * Math.Sqrt(2));
        for (int rho = _RhosLo; rho < _RhosHi; ++rho)
        {
          for (int theta = 0; theta < 180; ++theta)
          {
            {
              b = (byte)(_Votes[theta, rho] * 5);
              int y = (int)(rho * factor);
              bitmap.SetPixel(theta, y, Color.FromArgb(b, b, b));
            }
          }
        }
        */
        return (true);
      }
    };

    public HoughTransform(byte[,] Mono)
    {
      _Mono = Mono;
    _Width  = Mono.GetLength(0);
      _Height = Mono.GetLength(1);
      _Rhos = (int)((_Width + _Height) * Math.Sqrt(2) / 2);
    }

    public void DrawRotation(Line line, ref Bitmap bitmap)
    {
      double theta = Deg2Rad(180 - line.Theta);
      double sinTheta = Math.Sin(theta);
      double cosTheta = Math.Cos(theta);
      

      for (int x = 0; x < _Width; x++)
      {
        int y = (int)((cosTheta * x + line.Radius - _Rhos) / sinTheta);

        if (x > 0 && x < bitmap.Width && y > 0 && y < bitmap.Height)
        {
          bitmap.SetPixel(x, y, Color.Red);
          for (int k = 0; k < 100; k += 10)
          {
            bitmap.SetPixel(x, y + k, Color.Green);
          }

        }
      }

      for (int x = 0; x < _Width; x++)
      {
        int y = _Height / 2;

        if (x > 0 && x < bitmap.Width && y > 0 && y < bitmap.Height)
        {
          bitmap.SetPixel(x, y, Color.FromArgb(70, 170, 0));
          bitmap.SetPixel(x, y + 10, Color.FromArgb(70, 170, 0));
          bitmap.SetPixel(x, y - 10, Color.FromArgb(70, 170, 0));

        }
      }

    }

    public bool Center(ref Line refLine, ref Bitmap bitmap)
    {
      int strip = Math.Min(_Width , _Height) / 7;

      int by = 3 * strip;
      int ey = 4 * strip;
      int bx = _Width / 3;
      int ex = _Width - bx;

      // looking for horizontal lines around 90(+-30) degrees
      Accumulator accumulator = new Accumulator(_Width, _Height, 60, 120);
      for (int y = 0; y < _Height; ++y) 
      {
        for (int x = 0; x < _Width; x += 1)  // 1/3 of the width; skip every 3 pixels
        {
          if (_Mono[x, y] == 0)
            accumulator.Add(x, y);
        }
      }

      return (accumulator.Strongest(ref refLine, ref bitmap));
    }

    public bool Area(Line currLine, ref Line nextLine, int offset, int loLimit, int hiLimit, int thetaOffset, bool Vertical)
    {
      int start, end;

      if (offset > 0)
      {
        start = currLine.Radius;
        end = start + offset;
      }
      else
      {
        end = currLine.Radius;
        start = end + offset;
      }

      double sina = Math.Sin(Deg2Rad(currLine.Theta));
      double cosa = Math.Cos(Deg2Rad(currLine.Theta));
      currLine.Theta = 180 - currLine.Theta;    // correct for bin orientation

      Accumulator accumulator = new Accumulator(_Width, _Height,
        currLine.Theta - thetaOffset, currLine.Theta + thetaOffset);

      if (Vertical) 
      {
        int x, y;
        for (int rho = start; rho < end; rho += 1) // try 5 for speed
        {
          for (y = loLimit; y < hiLimit; y += 2)  // try 4 for speed
          {
            x = (int)((y * cosa + rho - _Rhos) / sina);
            if (x > 0 && x < _Width && y > 0 && y < _Height)
            {
              if (_Mono[x, y] == 0)
                accumulator.Add(y, x);
            }
          }
        }
      }
      else
      {
        int x, y;
        for (int rho = start; rho < end; rho += 1) // try 5 for speed
        {
          for (y = loLimit; y < hiLimit; y += 2)  // try 4 for speed
          {
            x = (int)((y * cosa + rho - _Rhos) / sina);
            if (x > 0 && x < _Width && y > 0 && y < _Height)
            {
              if (_Mono[x, y] == 0)
                accumulator.Add(x, y);
            }
          }
        }
      }
/*
      if (accumulator.Strongest(ref nextLine))
      {
        // correct the angles for the outside world
        nextLine.Theta = 180 - nextLine.Theta;
        return (true);
      }
 */     

      return (false);
    }
  };
}
