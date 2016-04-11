using System;
using System.Collections;
using System.Windows.Forms;

namespace PGLData
{
    //a comparison structure used for listview column based sorting
    public class ListViewSort : IComparer
    {
        private int col;
        private bool descK;
        private bool compareMode;


        public ListViewSort()
        {
            col = 0;
        }
        public ListViewSort(int column, object Desc,bool compareMode)
        {
            descK = (bool)Desc;
            col = column; 
            this.compareMode = compareMode;
        }
        public int Compare(object x, object y)
        {
            int tempInt = 0;

            if (compareMode && (col == 0 || col == 4))
            {
                double a;
                double b;

                if (((ListViewItem)x).SubItems[col].Text.Equals("-"))
                    a = 10000 + double.Parse(((ListViewItem)x).SubItems[4].Text);
                else
                    a = double.Parse(((ListViewItem)x).SubItems[col].Text);
                if (((ListViewItem)y).SubItems[col].Text.Equals("-"))
                    b = 10000 + double.Parse(((ListViewItem)y).SubItems[4].Text);
                else
                    b = double.Parse(((ListViewItem)y).SubItems[col].Text);


                if (a < b)
                    tempInt = -1;
                else if (a > b)
                    tempInt = 1;
            }
            else if (!compareMode && (col == 0 || col == 3))
            {
                double a;
                double b;

                if (((ListViewItem)x).SubItems[col].Text.Equals("-"))
                    a = 10000 + double.Parse(((ListViewItem)x).SubItems[3].Text);
                else
                    a = double.Parse(((ListViewItem)x).SubItems[col].Text);
                if (((ListViewItem)y).SubItems[col].Text.Equals("-"))
                    b = 10000 + double.Parse(((ListViewItem)y).SubItems[3].Text);
                else
                    b = double.Parse(((ListViewItem)y).SubItems[col].Text);


                if (a < b)
                    tempInt = -1;
                else if (a > b)
                    tempInt = 1;
            }
            else if (compareMode && col == 1)
            {
                double a;
                double b;

                if (((ListViewItem)x).SubItems[col].Text.Equals(""))
                    a = 0;
                else if (((ListViewItem)x).SubItems[col].Text[0] == '↑' && ((ListViewItem)x).SubItems[col].Text[1] != '新')
                    a = double.Parse(((ListViewItem)x).SubItems[col].Text.Replace("↑", ""));
                else if (((ListViewItem)x).SubItems[col].Text[0] == '↑')
                    a = 10000 + double.Parse(((ListViewItem)x).SubItems[0].Text);
                else if (((ListViewItem)x).SubItems[col].Text[1] == '落')
                    a = -10000;
                else
                    a = -double.Parse(((ListViewItem)x).SubItems[col].Text.Replace("↓", ""));

                if (((ListViewItem)y).SubItems[col].Text.Equals(""))
                    b = 0;
                else if (((ListViewItem)y).SubItems[col].Text[0] == '↑' && ((ListViewItem)y).SubItems[col].Text[1] != '新')
                    b = double.Parse(((ListViewItem)y).SubItems[col].Text.Replace("↑", ""));
                else if (((ListViewItem)y).SubItems[col].Text[0] == '↑')
                    b = 10000 + double.Parse(((ListViewItem)y).SubItems[0].Text);
                else if (((ListViewItem)y).SubItems[col].Text[1] == '落')
                    b = -10000;
                else
                    b = -double.Parse(((ListViewItem)y).SubItems[col].Text.Replace("↓", ""));
                if (a < b)
                    tempInt = -1;
                else if (a > b)
                    tempInt = 1;
            }
            else
                tempInt = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);

            if (descK)
            {
                return -tempInt;
            }
            else
            {
                return tempInt;
            }
        }
    }

}
