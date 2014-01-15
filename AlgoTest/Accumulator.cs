using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoTest
{
  private class Accumulator
  {
    private int[,] _Votes;
    private int _DeltaTheta;
    private int _Rho;
    private int _Size;
    private int _Offset;
    public Accumulator(int theta1 = 0, int theta2 = 180)
    {
      Theta1 = theta1;
      _DeltaTheta = theta2 - Theta1;
    }
    public int Theta1 { get; private set; }
    public int Theta2
    {
      get { return (Theta1 + _DeltaTheta); }
    }
    public int Size
    {
      get { return _Size; }
      set
      {
        _Size  = value;
        _Rho   = (int)(value * Math.Sqrt(2));
        _Votes = new int[_DeltaTheta, _Rho];
      }
    }
    public int Rho
    {
      get { return _Rho; }
    }
    public int Offset
    {
      get { return _Offset; }
      set
      {
        if (value < _Rho)
          _Offset = value;
        else
        {
          // throw exception.
        }
      }
    }
    public void Clear()
    {
      Array.Clear(_Votes, 0, _Votes.Length);
    }
    public void Add(int x, int y)
    {
      try
      {
        int radius;
        double thetaRad;
        for (int delta = 0; delta <= _DeltaTheta; ++delta)
        {
          thetaRad = Math.PI * (delta + Theta1) / 180.0;
          radius = (int)(x * Math.Cos(thetaRad) + y * Math.Sin(thetaRad) + _Offset);
          ++_Votes[delta, radius];
        }
      }
      catch (Exception)
      {

      }
    }
    public bool Strongest(ref Line line)
    {
      return (Strongest(0, _Rho, ref line));
    }
    public bool Strongest(int rhoLoLimit, int rhoHiLimit, ref Line line)
    {
      int vote = -1;
      try
      {
        for (int rho = rhoLoLimit; rho < rhoHiLimit; ++rho)
        {
          for (int delta = 0; delta <= _DeltaTheta; ++delta)
          {
            if (_Votes[delta, rho] > vote)
            {
              vote = _Votes[delta, rho];
              line.Theta  = Theta1 + delta;
              line.Radius = rho;
            }
          }
        }
      }
      catch (Exception)
      {
        vote = 0;
      }

      if (vote <= 0)
        return (false);

      return (true);
    }
  }
}
