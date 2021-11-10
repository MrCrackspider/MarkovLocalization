using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using testforms;

namespace testforms
{
    public partial class Form1 : Form
    {
        // ширина высота карты
        int MapWidth = 15;
        int MapHeight = 15;

        // вероятности
        static double pHit = 0.6;
        static double pMiss = 0.2;
        static double pExact = 0.8;
        static double pOvershoot = 0.1;
        static double pUndershoot = 0.1;

        // начальные условия
        int StartPositionX = 0;
        int StartPositionY = 0;

        int AmountOfSteps = 0;
        double[,] map;
        double[,] ResultProbabilities;

        int[] motions;
        int[] motionDirection;
        //int[,] motions;
        int[] measurements;

        // панель движений/измерений
        FlowLayoutPanel[] Steps;
        NumericUpDown[] StepDistance;
        ComboBox[] Direction;
        ComboBox[] Measure;

        // главная панель
        static Label[,] panel;
        Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        // инициализация при загрузке
        private void Form1_Load(object sender, EventArgs e)
        {
            MapWidth = (int)numericUpDown1.Value;
            MapHeight = (int)numericUpDown2.Value;

            pHit = (double)numericUpDown3.Value;
            pMiss = (double)numericUpDown4.Value;
            pExact = (double)numericUpDown5.Value;
            pOvershoot = (double)numericUpDown6.Value;
            pUndershoot = (double)numericUpDown7.Value;

            StartPositionX = (int)numericUpDown9.Value;
            StartPositionY = (int)numericUpDown10.Value;
            
            // общее кол-во движений
            AmountOfSteps = 0;

            Steps = new FlowLayoutPanel[AmountOfSteps];
            StepDistance = new NumericUpDown[AmountOfSteps];
            Direction = new ComboBox[AmountOfSteps];
            Measure = new ComboBox[AmountOfSteps];
            measurements = new int[AmountOfSteps];
            motions = new int[AmountOfSteps];
            motionDirection = new int[AmountOfSteps];

            panel = new Label[MapHeight, MapWidth];
            
            // генерация карты
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    
                    panel[i, j] = new Label();
                    panel[i, j].Width = 25;
                    panel[i, j].Height = 25;
                    panel[i, j].BackColor = Color.Green;
                    panel[i, j].BorderStyle = BorderStyle.FixedSingle;
                    panel[i, j].Location = new Point(9 + 26 * j, 9 + 26 * i);
                    panel[i, j].Text = "";
                    panel[i, j].Click += new System.EventHandler(this.panel_Click);
                    panel[i, j].ForeColor = Color.White;
                    panel[i, j].Padding = new Padding(0, 5, 0, 0);
                    panel[i, j].Font = new Font("Microsoft Sans Serif", 6.25F, FontStyle.Regular);
                    this.Controls.Add(panel[i, j]);
                }
            }
            panel[StartPositionY, StartPositionX].Text = "1";
        }

        // нажатие на ячейку меняет её цвет
        private void panel_Click(object sender, EventArgs e)
        {
            Label temp = (Label)sender;
            if (temp.BackColor == Color.Red)
            {
                temp.BackColor = Color.Green;
            }
            else if (temp.BackColor == Color.Green)
            {
                temp.BackColor = Color.Red;
            }
        }

        // кнопка выполнить
        private void button1_Click(object sender, EventArgs e)
        {
            if (AmountOfSteps > 0)
            {
                // создаём массив для обработки: 1 - красный, 0 - зелёный
                map = new double[MapHeight, MapWidth];
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        if (panel[i, j].BackColor == Color.Red)
                        {
                            map[i, j] = 1;
                        }
                        else if (panel[i, j].BackColor == Color.Green)
                        {
                            map[i, j] = 0;
                        }
                    }
                }

                // начальные условия: неизвестно/определено
                ResultProbabilities = new double[MapHeight, MapWidth];
                if (checkBox1.Checked)
                {
                    for (int i = 0; i < MapHeight; i++)
                    {
                        for (int j = 0; j < MapWidth; j++)
                        {
                            ResultProbabilities[i, j] = 1.0 / ((double)MapHeight * (double)MapWidth);
                        }
                    }
                }
                else
                {
                    ResultProbabilities[(int)numericUpDown10.Value, (int)numericUpDown9.Value] = 1;
                }

                // загрузка данных в массивы motions и measurement
                for (int i = 0; i < AmountOfSteps; i++)
                {
                    // сколько
                    motions[i] = (int)StepDistance[i].Value;

                    // куда
                    switch (Direction[i].SelectedItem.ToString())
                    {
                        case "Up":
                            motionDirection[i] = 1;
                            break;
                        case "Left":
                            motionDirection[i] = 2;
                            break;
                        case "Down":
                            motionDirection[i] = 3;
                            break;
                        case "Right":
                            motionDirection[i] = 4;
                            break;
                    }

                    // результат измерения
                    switch (Measure[i].SelectedItem.ToString())
                    {
                        case "Red":
                            measurements[i] = 1;
                            break;
                        case "Green":
                            measurements[i] = 0;
                            break;
                    }
                }

                for (int i = 0; i < AmountOfSteps; i++)
                {
                    switch (motionDirection[i])
                    {
                        case 1:
                            ResultProbabilities = moveUp(ResultProbabilities, motions[i]);
                            break;
                        case 2:
                            ResultProbabilities = moveLeft(ResultProbabilities, motions[i]);
                            break;
                        case 3:
                            ResultProbabilities = moveDown(ResultProbabilities, motions[i]);
                            break;
                        case 4:
                            ResultProbabilities = moveRight(ResultProbabilities, motions[i]);
                            break;
                    }
                    ResultProbabilities = sense(ResultProbabilities, measurements[i], map);
                }
                label14.Visible = false;
                Form2 Result = new Form2(MapWidth, MapHeight, ref ResultProbabilities, map);
                Result.Show();
            }
            else
            {
                label14.Visible = true;
            }
        }

        /*
        private void Show(double[,] p)
        {
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    panel[i, j].Text = p[i, j].ToString();
                }
            }
        }
        */

        // снятие показания датчика
        private double[,] sense(double[,] p, int measurements, double[,] map)
        {
            double[,] q = new double[MapHeight, MapWidth];
            double summ = 0;
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    if (measurements == map[i, j])
                    {
                        q[i, j] = p[i, j] * pHit;
                    }
                    else
                    {
                        q[i, j] = p[i, j] * pMiss;
                    }
                }
            }

            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    summ += q[i, j];
                }
            }

            if (summ != 0)
            {
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        q[i, j] /= summ; // нормирование
                    }
                }
            }
            
            return q;
        }

        // перемещение (текущие вероятности[,], длина перемещения)
        private double[,] moveUp(double[,] p, int motions)
        {
            double[,] q = new double[MapHeight, MapWidth];
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    q[i, j] = p[(MapHeight + i + motions) % MapHeight, j] * pExact +
                    p[(MapHeight + i + motions - 1) % MapHeight, j] * pUndershoot +
                    p[(MapHeight + i + motions + 1) % MapHeight, j] * pOvershoot;
                }
            }
            return q;
        }
        private double[,] moveLeft(double[,] p, int motions)
        {
            double[,] q = new double[MapHeight, MapWidth];
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    q[i, j] = p[i, (MapWidth + j + motions) % MapWidth] * pExact +
                    p[i, (MapWidth + j + motions - 1) % MapWidth] * pUndershoot +
                    p[i, (MapWidth + j + motions + 1) % MapWidth] * pOvershoot;
                }
            }
            return q;
        }
        private double[,] moveDown(double[,] p, int motions)
        {
            double[,] q = new double[MapHeight, MapWidth];
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    q[i, j] = p[(MapHeight + i - motions) % MapHeight, j] * pExact +
                    p[(MapHeight + i - motions - 1) % MapHeight, j] * pUndershoot +
                    p[(MapHeight + i - motions + 1) % MapHeight, j] * pOvershoot;
                }
            }
            return q;
        }
        private double[,] moveRight(double[,] p, int motions)
        {
            double[,] q = new double[MapHeight, MapWidth];
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    q[i, j] = p[i, (MapWidth + j - motions) % MapWidth] * pExact +
                    p[i, (MapWidth + j - motions - 1) % MapWidth] * pUndershoot +
                    p[i, (MapWidth + j - motions + 1) % MapWidth] * pOvershoot;
                }
            }
            return q;
        }

        // обновление переменных в соответствии с введёнными данными
        private void numericUpDown1_Leave(object sender, EventArgs e)
        {
            int temp = MapWidth; // то что было
            MapWidth = (int)numericUpDown1.Value; // то что стало
            if (MapWidth < temp) // если на понижение
            {
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = MapWidth; j < temp; j++)
                    {
                        panel[i, j].Enabled = false;
                        panel[i, j].BackColor = Color.Gray;
                        panel[i, j].Text = "";
                        numericUpDown9.Value = 0;
                        numericUpDown10.Value = 0;
                    }
                }
            }
            else if (temp < MapWidth)
            {
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = temp; j < MapWidth; j++)
                    {
                        panel[i, j].Enabled = true;
                        panel[i, j].BackColor = Color.Green;
                    }
                }
            }
            if (checkBox1.Checked)
            {
                double temp1 = 1.0 / ((double)MapHeight * (double)MapWidth);
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        panel[i, j].Text = temp1.ToString();
                    }
                }
            }
        }
        private void numericUpDown2_Leave(object sender, EventArgs e)
        {
            int temp = MapHeight;
            MapHeight = (int)numericUpDown2.Value;
            if (MapHeight < temp) // если на понижение
            {
                for (int i = 0; i < MapWidth; i++)
                {
                    for (int j = MapHeight; j < temp; j++)
                    {
                        panel[j, i].Enabled = false;
                        panel[j, i].BackColor = Color.Gray;
                        panel[j, i].Text = "";
                        numericUpDown9.Value = 0;
                        numericUpDown10.Value = 0;
                    }
                }
            }
            else if (temp < MapHeight)
            {
                for (int i = 0; i < MapWidth; i++)
                {
                    for (int j = temp; j < MapHeight; j++)
                    {
                        panel[j, i].Enabled = true;
                        panel[j, i].BackColor = Color.Green;
                    }
                }
            }
            if (checkBox1.Checked)
            {
                double temp1 = 1.0 / ((double)MapHeight * (double)MapWidth);
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        panel[i, j].Text = temp1.ToString();
                    }
                }
            }
        }
        private void numericUpDown3_Leave(object sender, EventArgs e)
        {
            pHit = (double)numericUpDown3.Value;
        }
        private void numericUpDown4_Leave(object sender, EventArgs e)
        {
            pMiss = (double)numericUpDown4.Value;
        }
        private void numericUpDown5_Leave(object sender, EventArgs e)
        {
            pExact = (double)numericUpDown5.Value;
        }
        private void numericUpDown6_Leave(object sender, EventArgs e)
        {
            pOvershoot = (double)numericUpDown6.Value;
        }
        private void numericUpDown7_Leave(object sender, EventArgs e)
        {
            pUndershoot = (double)numericUpDown7.Value;
        }

        // создание доп шагов
        private void button2_Click(object sender, EventArgs e)
        {
            AmountOfSteps++;

            // выделяем место под новый массив, который на 1 больше предыдущего
            Array.Resize(ref Steps, AmountOfSteps);
            Array.Resize(ref StepDistance, AmountOfSteps);
            Array.Resize(ref Direction, AmountOfSteps);
            Array.Resize(ref Measure, AmountOfSteps);
            Array.Resize(ref measurements, AmountOfSteps);
            Array.Resize(ref motions, AmountOfSteps);
            Array.Resize(ref motionDirection, AmountOfSteps);

            // инициализируем последний добавленный элемент
            Steps[AmountOfSteps - 1] = new FlowLayoutPanel();
            StepDistance[AmountOfSteps - 1] = new NumericUpDown();
            Direction[AmountOfSteps - 1] = new ComboBox();
            Measure[AmountOfSteps - 1] = new ComboBox();
            measurements[AmountOfSteps - 1] = new int();
            motions[AmountOfSteps - 1] = new int();
            motionDirection[AmountOfSteps - 1] = new int();

            //Array.Resize(ref Steps, AmountOfSteps);

            // задаём настройки новых созданных элементов
            Steps[AmountOfSteps - 1].Width = 344;
            Steps[AmountOfSteps - 1].Height = 25;
            Steps[AmountOfSteps - 1].BackColor = Color.DarkSlateGray;

            StepDistance[AmountOfSteps - 1].Width = 60;
            StepDistance[AmountOfSteps - 1].Minimum = 1;

            Direction[AmountOfSteps - 1].Width = 100;
            Direction[AmountOfSteps - 1].Items.AddRange(new string[] { "Up", "Left", "Down", "Right" });
            Direction[AmountOfSteps - 1].Text = Direction[AmountOfSteps - 1].Items[0].ToString();

            Measure[AmountOfSteps - 1].Width = 100;
            Measure[AmountOfSteps - 1].Items.AddRange(new string[] { "Red", "Green" });
            Measure[AmountOfSteps - 1].Text = Measure[AmountOfSteps - 1].Items[1].ToString();

            // добавляем stepdistance direction и measure на панель steps
            Steps[AmountOfSteps - 1].Controls.Add(StepDistance[AmountOfSteps - 1]);
            Steps[AmountOfSteps - 1].Controls.Add(Direction[AmountOfSteps - 1]);
            Steps[AmountOfSteps - 1].Controls.Add(Measure[AmountOfSteps - 1]);

            // добавляем панель steps на форму
            flowLayoutPanel1.Controls.Add(Steps[AmountOfSteps - 1]);
        }

        // задание стартовой позиции
        private void numericUpDown9_Leave(object sender, EventArgs e)
        {
            numericUpDown9.Maximum = MapWidth - 1;
            numericUpDown10.Maximum = MapHeight - 1;
            panel[StartPositionY, StartPositionX].Text = "";
            StartPositionX = (int)numericUpDown9.Value;
            StartPositionY = (int)numericUpDown10.Value;
            panel[StartPositionY, StartPositionX].Text = "1";
        }

        // положение робота определено/неопределено
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                double temp = 1.0 / ((double)MapHeight * (double)MapWidth);
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        panel[i, j].Text = temp.ToString();
                    }
                }
                numericUpDown9.Enabled = false;
                numericUpDown10.Enabled = false;
            }
            else
            {
                for (int i = 0; i < MapHeight; i++)
                {
                    for (int j = 0; j < MapWidth; j++)
                    {
                        panel[i, j].Text = "";
                    }
                }
                numericUpDown9.Enabled = true;
                numericUpDown10.Enabled = true;
                panel[(int)numericUpDown10.Value, (int)numericUpDown9.Value].Text = "1";
            }
        }
        
        // кнопка рандома
        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    int color = rnd.Next(2);
                    if (color == 0)
                    {
                        panel[i, j].BackColor = Color.Green;
                    }
                    else if (color == 1)
                    {
                        panel[i, j].BackColor = Color.Red;
                    }
                }
            }
        }
    }
}
