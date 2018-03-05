using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using System.Security.Permissions.FileIOPermission;

namespace PhotoFilter
{
    public partial class Form1 : Form
    {
        Bitmap image;
        public Form1()
        {
            InitializeComponent();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";
            dialog.Title = "Save an Image File"; 
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
            }
            pictureBox1.Image = image;
            pictureBox1.Refresh();
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";
            dialog.ShowDialog();
            if (dialog.FileName != "")  
            {  
                System.IO.FileStream fs =
                    (System.IO.FileStream)dialog.OpenFile();
                switch (dialog.FilterIndex)  
                {  
                    case 1 :
                        this.pictureBox1.Image.Save(fs, 
                            System.Drawing.Imaging.ImageFormat.Png); break;  

                    case 2 :
                        this.pictureBox1.Image.Save(fs,   
                            System.Drawing.Imaging.ImageFormat.Jpeg); break;  

                    case 3 :
                        this.pictureBox1.Image.Save(fs,   
                            System.Drawing.Imaging.ImageFormat.Bmp); break;  
                }  

            fs.Close();  
            } 
        }
        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
                image = newImage;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar2.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрГауссаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void чБToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sepia();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void увеличитьЯркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Brightness();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поОсиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter(true);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поОсиXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter(false);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void увеличитьРезкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sharpen();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sharp();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Tisnenie();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void ПрюиттапоОсиYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Pruitt(true);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void ПрюиттапоОсиXToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filter = new Pruitt(false);
            backgroundWorker1.RunWorkerAsync(filter);
        }
    }
}