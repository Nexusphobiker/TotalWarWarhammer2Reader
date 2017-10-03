using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalWarPackReader
{
    public partial class MainForm : Form
    {
        PACK CurrentPACK;
        PackFile CurrentPreviewFile;
        Encoding encodConf = Encoding.Unicode;
        public MainForm()
        {
            InitializeComponent();
            fileTreeView.PathSeparator = "\\";
            fileTreeView.AfterExpand += fileTreeViewLoadPartial;
        }
        
        //First time working with tree views so bear with me
        private void openToolStripMenuItemFileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Opening " + openFileDialog.FileName);
                try
                {
                    CurrentPACK = new PACK(openFileDialog.FileName);
                    Console.WriteLine("Populating treeView");
                    fileTreeView.Nodes.Clear();
                    foreach(PackFile file in CurrentPACK.FileList)
                    {
                        string[] pathArr = file.name.Split('\\');
                        string node = pathArr[0];
                        if (fileTreeView.Nodes.Find(node,true).Length == 0)
                        {
                            if (pathArr.Length > 1)
                            {
                                //The TEMP nodes are added to make the nodes expandable
                                fileTreeView.Nodes.Add(node, node).Nodes.Add("TEMP", "You shouldnt see this");
                            }
                            else
                            {
                                fileTreeView.Nodes.Add(node, node);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Exception while loading file " + ex.Message,"!");
                }
            }
            else
            {
                Console.WriteLine("No file specified");
            }
        }
        
        private void fileTreeViewLoadPartial(object sender, TreeViewEventArgs e)
        {
            string[] path = e.Node.FullPath.Split('\\');
            bool isPart;
            foreach (PackFile file in CurrentPACK.FileList)
            {
                isPart = true;
                string[] tempPath = file.name.Split('\\');
                int i = 0;
                while (i < path.Length)
                {
                    if (tempPath[i] != path[i])
                    {
                        isPart = false;
                        break;
                    }
                    i++;
                }
                if (i < tempPath.Length && isPart)
                {
                    if (e.Node.Nodes.Find(tempPath[i], true).Length == 0)
                    {
                        if (i < tempPath.Length - 1)
                        {
                            e.Node.Nodes.Add(tempPath[i], tempPath[i]).Nodes.Add("TEMP", "You shouldnt see this");
                        }
                        else
                        {
                            e.Node.Nodes.Add(tempPath[i], tempPath[i]);
                        }
                    }
                }
            }
            e.Node.Nodes.RemoveByKey("TEMP");
        }

        private void fileTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Console.WriteLine("Selected "+e.Node.FullPath);
            foreach(PackFile file in CurrentPACK.FileList)
            {
                if(file.name == e.Node.FullPath)
                {
                    CurrentPreviewFile = file;
                    //Load preview
                    splitContainerMainForm.Panel2.Controls.Clear();
                    KnownFileTypes.Types fileType = KnownFileTypes.IdentifyFile(file.name);
                    Console.WriteLine(fileType);
                    switch (fileType)
                    {
                        case KnownFileTypes.Types.PNG:
                            PictureBox prev = new PictureBox();
                            prev.Dock = DockStyle.Fill;
                            byte[] data = CurrentPACK.getFileData(file);
                            Image tempIMG;
                            using (MemoryStream stream = new MemoryStream(data))
                            {
                                tempIMG = Image.FromStream(stream);
                            }
                            prev.Image = tempIMG;
                            prev.SizeMode = PictureBoxSizeMode.Zoom;
                            splitContainerMainForm.Panel2.Controls.Add(prev);
                            break;
                        case KnownFileTypes.Types.TXT:
                            RichTextBox textprev = new RichTextBox();
                            textprev.Dock = DockStyle.Fill;
                            byte[] textdata = CurrentPACK.getFileData(file);
                            splitContainerMainForm.Panel2.Controls.Add(textprev);
                            textprev.Text = encodConf.GetString(textdata);
                            break;
                        case KnownFileTypes.Types.XML:
                            RichTextBox xmlprev = new RichTextBox();
                            xmlprev.Dock = DockStyle.Fill;
                            byte[] xmldata = CurrentPACK.getFileData(file);
                            splitContainerMainForm.Panel2.Controls.Add(xmlprev);
                            xmlprev.Text = encodConf.GetString(xmldata);
                        
                            break;
                        default:
                            break;
                    }
                    break;
                }
            }
        }

        private void unicodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            encodConf = Encoding.Unicode;
            Console.WriteLine("Encoding set to Unicode");
        }

        private void uTF8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            encodConf = Encoding.UTF8;
            Console.WriteLine("Encoding set to UTF8");
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(fileTreeView.SelectedNode == null)
            {
                Console.WriteLine("No node to extract selected");
                return;
            }
            Console.WriteLine("Extracting " + fileTreeView.SelectedNode.FullPath);
            string[] pathToExtract = fileTreeView.SelectedNode.FullPath.Split('\\');
            int i = 0;
            bool isExtractionTarget;
            while(i < CurrentPACK.FileList.Length)
            {
                isExtractionTarget = true;
                string[] potentialFile = CurrentPACK.FileList[i].name.Split('\\');
                int a = 0;
                while(a < pathToExtract.Length)
                {
                    if(pathToExtract[a] != potentialFile[a])
                    {
                        isExtractionTarget = false;
                    }
                    a++;
                }
                if (isExtractionTarget)
                {
                    Console.WriteLine("Extracting " + CurrentPACK.FileList[i].name);
                    CurrentPACK.Unpack(i);
                }
                i++;
            }
            Console.WriteLine("Done extracting.");
        }
    }
}
