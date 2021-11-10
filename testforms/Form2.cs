using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testforms
{
    public partial class Form2 : Form
    {

        static Label[,] ResultPanel;
        int MapWidth = 1;
        int MapHeight = 1;
        double[,] p = new double[15, 15];
        double[,] map;
        double max;
        int MaxX;
        int MaxY;

        public Form2(int MapWidth, int MapHeight, ref double[,] q, double[,] map)
        {
            InitializeComponent();
            this.MapHeight = MapHeight;
            this.MapWidth = MapWidth;
            p = q;
            this.map = map;
            max = p[0, 0];
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    if (p[i,j] > max)
                    {
                        max = p[i, j]; // минимальное значение
                        MaxX = j;
                        MaxY = i;
                    }
                }
            }
            
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            ResultPanel = new Label[MapHeight, MapWidth];
            double temp = 1 / p[MaxY, MaxX];
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    ResultPanel[i, j] = new Label();
                    ResultPanel[i, j].Width = 25;
                    ResultPanel[i, j].Height = 25;
                    ResultPanel[i, j].BackColor = Color.FromArgb(0, (int)(255.0 * p[i, j] * temp), 0);
                    ResultPanel[i, j].BorderStyle = BorderStyle.FixedSingle;
                    ResultPanel[i, j].Location = new Point(9 + 26 * j, 9 + 26 * i);
                    ResultPanel[i, j].Text = p[i,j].ToString();
                    ResultPanel[i, j].ForeColor = Color.White;
                    ResultPanel[i, j].Padding = new Padding(0, 5, 0, 0);
                    ResultPanel[i, j].Font = new Font("Microsoft Sans Serif", 6.25F, FontStyle.Regular);
                    this.Controls.Add(ResultPanel[i, j]);
                }
            }
            ResultPanel[MaxY, MaxX].ForeColor = Color.Yellow;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        ResultPanel[i, j].BackColor = Color.FromArgb(0, (int)((double)255 * p[i, j]), 0);
                    }
                }
            }
            else if (radioButton2.Checked)
            {
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        if (map[i, j] == 1)
                        {
                            ResultPanel[i, j].BackColor = Color.Red;
                        }
                        else if (map[i, j] == 0)
                        {
                            ResultPanel[i, j].BackColor = Color.Green;
                        }
                    }
                }
            }
        }
    }
}
