﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using WMPLib;
using System.Threading;
using System.Collections;
namespace MusicSelector
{

    public partial class Form1 : Form
    {
        private List<String> dirs = new List<String>();
        private FolderBrowserDialog FBDOpen = new FolderBrowserDialog();
        private WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        private bool switchPlayback = true;
        private int MouseIndex;
        private Stack<String> fullRoute = new Stack<String>();
        private FolderBrowserDialog FBDKeep = new FolderBrowserDialog();
        private String KeepSelectedPath;

        public Form1()
        {
            InitializeComponent();
            axSlider1.Max = 100;
            wplayer.settings.volume = axSlider1.Value;

        }

        private void openFolder_Click(object sender, EventArgs e)
        {

            FBDOpen = new FolderBrowserDialog();
            fullRoute.Push(FBDOpen.SelectedPath);
            if (FBDOpen.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                dirs = new List<string>(Directory.GetDirectories(FBDOpen.SelectedPath));

                foreach (String dir in dirs)
                {
                    listBox1.Items.Add(Path.GetFileName(dir));
                }
            }

        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*
             * Para navegar entre los directorios.
             * 
             */
            MouseIndex = this.listBox1.IndexFromPoint(e.Location);
            String path = dirs[MouseIndex];
            List<String> dirNuevo = new List<String>(Directory.GetDirectories(path));
            listBox1.Items.Clear();
            foreach (String dir in dirNuevo)
            {
                listBox1.Items.Add(Path.GetFileName(dir));
            }
            String spl = path;
            List<String> splited = new List<string>(spl.Split('\\'));
            String finalPrevPath = splited[0];
            if (splited.Count == 1)
            {
                finalPrevPath = finalPrevPath + "\\";
            }

            for (int i = 1; i < splited.Count - 1; i++)
            {
                finalPrevPath = finalPrevPath + "\\" + splited[i];
            }
            fullRoute.Push(finalPrevPath + "\\");


            if (fullRoute.Count == 1)
            {
                button4.Enabled = false;
            }
            else
            {
                button4.Enabled = true;
            }
            /**************************************************************/
            dirs = dirNuevo;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (wplayer.currentMedia == null)
            {
                playBack();
                listBox2.SelectedIndex = 0;
            }
            else if (wplayer.currentMedia.sourceURL.Contains(dirs[listBox1.SelectedIndex]))
            {
                switchPlayback = !switchPlayback;
                if (switchPlayback == true)
                {
                    wplayer.controls.pause();
                }
                else
                {
                    wplayer.controls.play();
                }
            }
            else
            {
                timer2.Enabled = false;
                wplayer.controls.stop();
                playBack();
                listBox2.SelectedIndex = 0;
            }

        }

        private void playBack()
        {
            String path = dirs[listBox1.SelectedIndex];
            List<String> songs = new List<String>(Directory.GetFiles(path));
            wplayer = new WMPLib.WindowsMediaPlayer();
            WMPLib.IWMPMedia wt;
            listBox2.Items.Clear();
            foreach (String song in songs)
            {
                if (song.Contains(".png") || song.Contains(".jpg") || song.Contains(".jpeg")
                || song.Contains(".ico"))
                {
                    pictureBox2.Image = Image.FromFile(song);

                }
                    if (song.Contains(".mp3") || song.Contains(".flac") || song.Contains(".wav") || song.Contains(".alac"))
                {
                    wt = wplayer.newMedia(song);
                    wplayer.currentPlaylist.appendItem(wt);
                    listBox2.Items.Add(Path.GetFileName(song));
                }
            }
            wplayer.controls.play();
            wplayer.settings.volume = axSlider1.Value;
            timer2.Enabled = true;
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            MouseIndex = this.listBox1.IndexFromPoint(e.Location);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            String path = fullRoute.Pop();
            listBox1.Items.Clear();

            List<String> dirNuevo = new List<String>(Directory.GetDirectories(path));
            foreach (String dir in dirNuevo)
            {
                listBox1.Items.Add(Path.GetFileName(dir));
            }
            dirs = dirNuevo;

            if (fullRoute.Count == 1)
            {
                button4.Enabled = false;
            }
            else
            {
                button4.Enabled = true;
            }
        }

        private void keepFolder_Click(object sender, EventArgs e)
        {
            FBDKeep = new FolderBrowserDialog();
            if (FBDKeep.ShowDialog() == DialogResult.OK)
            {
                KeepSelectedPath = FBDKeep.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String path = dirs[listBox1.SelectedIndex];
            DirectoryCopy(path, KeepSelectedPath + "\\" + new DirectoryInfo(path).Name, true);
        }









        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            listBox1.SelectedIndex = listBox1.SelectedIndex + 1;
            wplayer.controls.stop();
            playBack();
            listBox2.SelectedIndex = 0;
        }

        private void slider_Scroll(object sender, EventArgs e)
        {
            wplayer.settings.volume = axSlider1.Value;
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            listBox2.SelectedIndex = listBox2.SelectedIndex - 1;
            wplayer.controls.previous();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox2.SelectedIndex = listBox2.SelectedIndex + 1;
            wplayer.controls.next();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form2 = new Form2();
            form2.Show();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            double percent = 0;
            if (wplayer.controls.currentPosition != 0 && wplayer.controls.currentItem.duration!=0) { 
                percent = ((double)wplayer.controls.currentPosition / wplayer.controls.currentItem.duration);
                progressBar1.Value = (int)(percent * progressBar1.Maximum);
                }
            
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            wplayer.controls.currentPosition += 20.0;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            wplayer.controls.currentPosition -= 20.0;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if(wplayer.controls.currentPosition > wplayer.controls.currentItem.duration-0.5)
            {
                listBox2.SelectedIndex = listBox2.SelectedIndex + 1;
            }
        }
    }
   
}
