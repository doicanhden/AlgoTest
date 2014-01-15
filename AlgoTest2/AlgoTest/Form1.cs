using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Windows;
namespace AlgoTest
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      // Wrap the creation of the OpenFileDialog instance in a using statement,
      // rather than manually calling the Dispose method to ensure proper disposal
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Title = "Open Image";

      if (dlg.ShowDialog() == DialogResult.OK)
        pictureBox1.Image = new Bitmap(dlg.FileName);
    }

    private void button2_Click(object sender, EventArgs e)
    {
      Bitmap image = new Bitmap(pictureBox1.Image);

      byte[,] mono = ImageUtility.ConvertToBW(image);
      
      HoughTransform h = new HoughTransform(mono);
      Line line = new Line();

      LockBitmap bm = new LockBitmap(image);
      bm.LockBits();
      for (int y = 0; y < mono.GetLength(1); ++y)
      {
        for (int x = 0; x < mono.GetLength(0); ++x) 
        {
          bm.SetPixel(x, y, mono[x, y] == 0 ? Color.Black : Color.White);
        }
      }
      bm.UnlockBits();
      h.Center(ref line, ref image);
      label2.Text = (180 - line.Theta).ToString();
      h.DrawRotation(line, ref image);

      pictureBox1.Image = image;

    }
  }

}
