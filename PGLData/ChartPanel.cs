using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PGLData
{
    public partial class  ChartPanel : Form
    {
        public ChartPanel(ArrayList points, string title, string yLabel, bool isLine, bool isInt)
        {
            InitializeComponent();


            if (GlobalConstants.dpiX == 120)
            {
                foreach (Control ct in this.Controls)
                    ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }

            chart1.Series.Clear();
            //chart1.ChartAreas[0].AxisY.Minimum = 0;
            //chart1.ChartAreas[0].AxisY.Maximum = 100;
            chart1.ChartAreas[0].CursorX.AutoScroll = true;
            Series series = new Series("StackedArea");
            //series.ChartType = SeriesChartType.StackedArea;
            if(isLine)
                series.ChartType = SeriesChartType.Line;
            else
                series.ChartType = SeriesChartType.Column;
            //series.IsValueShownAsLabel = true;
            //series.SmartLabelStyle.Enabled = true;
            series.BorderWidth = 2; series.ShadowOffset = 0;
            if (isInt)
            {
                foreach (KeyValuePair<int, int> point in points)
                {
                    series.Points.AddXY(point.Key, point.Value);
                }
            }
            else
            {
                foreach (KeyValuePair<int, double> point in points)
                {
                    series.Points.AddXY(point.Key, point.Value);
                }
            }
            chart1.Series.Add(series);
            chart1.Titles[0].Text = title;
            label15.Text = yLabel;
        }

        private void chart1_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];

                //show axis X and Y, {1:F2} means it'a float number of two accurate digits  
                e.Text = string.Format("X: {0}\nY: {1:F2} ", dp.XValue, dp.YValues[0]);
            }
        }
    }
}
