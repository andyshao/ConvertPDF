using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using ImageMagick;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertPDF
{
    public partial class Form1 : Form
    {
        public List<string> outputPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string data = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string path = Path.Combine(data, name);

            if (!Directory.Exists(path))
            {
                Form2 form2 = new Form2();
                form2.ShowDialog();
                if (form2.checkPass)
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
                else
                {
                    this.Dispose();
                }
            }

            //TODO:
            GetSerialNum();
            listView1.AllowDrop = true;
            listView1.DragDrop += new DragEventHandler(listView1_DragDrop);
            listView1.DragEnter += new DragEventHandler(listView1_DragEnter);
        }

        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            listView1.DoDragDrop(listView1.SelectedItems[0].Tag.ToString(), DragDropEffects.Copy);
        }

        void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string item in fileList)
            {
                listView1.Items.Add(item);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (CheckBtnSave() == false) { return; }

            string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (checkBox1.Checked)
            {
                if (!PdfToImage(binPath, ".png", false))
                {                    
                    return;
                }
            }
            if (checkBox2.Checked)
            {
                if (!PdfToImage(binPath, ".jpg", false))
                {
                    return;
                }
            }
            if (checkBox3.Checked)
            {
                if (checkBox1.Checked)
                {
                    if (ImageToSvg(outputPath, "*.png"))
                    {
                        listView1.Clear();
                        MessageBox.Show("Success");                        
                    }                    
                    return;
                }
                if (checkBox2.Checked)
                {
                    if (ImageToSvg(outputPath, "*.jpg"))
                    {
                        listView1.Clear();
                        MessageBox.Show("Success");                        
                    }
                    return;
                }
                if (checkBox1.Checked == false && checkBox2.Checked == false)
                {
                    if(PdfToImage(binPath, ".png", true))
                    {
                        if(!ImageToSvg(outputPath, "*.png"))
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            listView1.Clear();
            MessageBox.Show("Success");
        }

        private void GetSerialNum()
        {
            ArrayList serialNoList = new ArrayList();
            ManagementObjectSearcher moSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject wmi_HD in moSearcher.Get())
            {
                string SerialNo = wmi_HD["SerialNumber"].ToString();
                serialNoList.Add(SerialNo);
            }
        }

        private Boolean CheckBtnSave()
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("Please import file");
                return false;
            }
            if (checkBox1.Checked == false && checkBox2.Checked == false && checkBox3.Checked == false)
            {
                MessageBox.Show("Please select the file format you want to export");
                checkBox1.Focus();
                return false;
            }
            return true;
        }

        private bool PdfToImage(string binPath, string extension, bool svgFlg)
        {
            int desired_dpi = 600;
            string gsDllPath = Path.Combine(binPath, "gsdll64.dll");
            GhostscriptVersionInfo gvi = new GhostscriptVersionInfo(gsDllPath);
            outputPath = new List<string>();
            string pageFilePath;
            try
            {
                using (var rasterizer = new GhostscriptRasterizer())
                {
                    foreach (ListViewItem item in listView1.Items)
                    {
                        var stream = File.Open(item.Text, FileMode.Open);
                        rasterizer.Open(stream, GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.GPL | GhostscriptLicense.AFPL, GhostscriptLicense.GPL), true);

                        for (var pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                        {
                            if (svgFlg)
                            {
                                pageFilePath = Path.Combine(binPath, string.Format(Path.GetFileNameWithoutExtension(item.Text) + "-page" + pageNumber + extension, pageNumber));
                            }
                            else
                            {
                                pageFilePath = Path.Combine(Path.GetDirectoryName(item.Text), string.Format(Path.GetFileNameWithoutExtension(item.Text) + "-page" + pageNumber + extension, pageNumber));
                            }


                            var img = rasterizer.GetPage(desired_dpi, pageNumber);
                            if (extension == ".png")
                            {
                                img.Save(pageFilePath, ImageFormat.Png);
                                outputPath.Add(pageFilePath);
                            }
                            else if (extension == ".jpg")
                            {
                                img.Save(pageFilePath, ImageFormat.Jpeg);
                                outputPath.Add(pageFilePath);
                            }

                        }
                        stream.Dispose();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("PdfToImage Fail: " + e.Message);
                return false;
            }
        }

        private bool ImageToSvg(List<string> imgPath, string imgExtension)
        {
            try
            {
                //string[] filePaths = Directory.GetFiles(imgPath, imgExtension, SearchOption.TopDirectoryOnly);
                using (MagickImageCollection images = new MagickImageCollection())
                {
                    foreach (ListViewItem item in listView1.Items)
                    {
                        foreach (string imgfilePath in imgPath)
                        {
                            if (imgfilePath.Contains(Path.GetFileNameWithoutExtension(item.Text)))
                            {
                                images.Read(imgfilePath);
                                using (IMagickImage vertical = images.AppendVertically())
                                {
                                    vertical.Format = MagickFormat.Svg;
                                    vertical.Density = new Density(600);
                                    vertical.Write(Path.GetDirectoryName(item.Text) + @"\" + Path.GetFileNameWithoutExtension(imgfilePath) + ".svg");
                                }
                            }
                            if (Path.GetDirectoryName(imgfilePath) == Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                            {
                                File.Delete(imgfilePath);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("ImageToSvg Fail: " + e.Message);
                return false;
            }                        
        }
    }
}
