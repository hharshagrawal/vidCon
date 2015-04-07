using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrimoSoftware.AVBlocks;
using Microsoft.Win32;
using System.IO;

namespace VideoConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string openFilePath;
        private string mergeFolder = "";
        private string videoFilePath = "";
        List<string> Parts = new List<string>();
        //string SaveFileFolder = @"c:\";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void bOpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            string formats = "All Videos Files |*.dat; *.wmv; *.3g2; *.3gp; *.3gp2; *.3gpp; *.amv; *.asf;  *.avi; *.bin; *.cue; *.divx; *.dv; *.flv; *.gxf; *.iso; *.m1v; *.m2v; *.m2t; *.m2ts; *.m4v; " +
                  " *.mkv; *.mov; *.mp2; *.mp2v; *.mp4; *.mp4v; *.mpa; *.mpe; *.mpeg; *.mpeg1; *.mpeg2; *.mpeg4; *.mpg; *.mpv2; *.mts; *.nsv; *.nuv; *.ogg; *.ogm; *.ogv; *.ogx; *.ps; *.rec; *.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm";

            openFileDialog1.Filter = formats;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;
            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();
            openFilePath = openFileDialog1.FileName;
            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                tbResults.Text = openFilePath;
                videoFilePath = openFilePath;
            }            
        }
        private bool splitVid(string filePath)
        {
            var fileParts = 4;
            //List<string> Packets = new List<string>();
            bool Split = false;
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                int SizeofEachFile = (int)Math.Ceiling((double)fs.Length / fileParts);
                for (int i = 0; i < fileParts; i++)
                {
                    string baseFileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    string Extension = System.IO.Path.GetExtension(filePath);
                    string newFileName = System.IO.Path.GetDirectoryName(filePath) + "\\" + baseFileName + "." + i.ToString() + Extension + ".tmp";
                        //+ i.ToString() + Extension;
                     
                    FileStream outputFile = new FileStream(newFileName, FileMode.Create, FileAccess.Write);
                    mergeFolder = System.IO.Path.GetDirectoryName(filePath);
                    int bytesRead = 0;
                    byte[] buffer = new byte[SizeofEachFile];
                    if ((bytesRead = fs.Read(buffer, 0, SizeofEachFile)) > 0)
                    {
                        outputFile.Write(buffer, 0, bytesRead);
                        //string packet = baseFileName + "." + i.ToString().PadLeft(3, Convert.ToChar("0")) + Extension.ToString();
                        //Packets.Add(packet);
                    }
                    Parts.Add(newFileName);
                    outputFile.Close();
                }
                Split = true;
                fs.Close();
            }
            catch (Exception Ex)
            {
                throw new ArgumentException(Ex.Message);
            }
            return Split;
        }

        public bool MergeFile(string inputfoldername1)
        {
            bool Output = false;
            try
            {
                string[] tmpfiles = Directory.GetFiles(inputfoldername1, "*.tmp");
                FileStream outPutFile = null;
                string PrevFileName = "";
                foreach (string tempFile in tmpfiles)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(tempFile);
                    string baseFileName = fileName.Substring(0, fileName.IndexOf(Convert.ToChar(".")));
                    string extension = System.IO.Path.GetExtension(fileName);
                    if (!PrevFileName.Equals(baseFileName))
                    {
                        if (outPutFile != null)
                        {
                            outPutFile.Flush();
                            outPutFile.Close();
                        }
                        outPutFile = new FileStream(inputfoldername1 + "\\" + baseFileName+ "new" + extension, FileMode.OpenOrCreate, FileAccess.Write);
                    }
                    int bytesRead = 0;
                    byte[] buffer = new byte[1024];
                    FileStream inputTempFile = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.Read);
                    while ((bytesRead = inputTempFile.Read(buffer, 0, 1024)) > 0)
                        outPutFile.Write(buffer, 0, bytesRead);
                    inputTempFile.Close();
                    //File.Delete(tempFile);
                    PrevFileName = baseFileName;
                }
                Output = true;
                outPutFile.Close();
               // lblSendingResult.Text = "Files have been merged and saved at location C:\\";
            }
            catch
            {
            }
            return Output;
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            if(splitVid(videoFilePath))
            {
                Library.Initialize();
                //string[] tmpfiles = Directory.GetFiles(mergeFolder, "*.tmp");
                //foreach (string tempFile in videoFilePath)
                //{
                    var inputInfo = new MediaInfo()
                    {
                        InputFile = videoFilePath
                    };

                    if (inputInfo.Load())
                    {
                        var inputSocket = MediaSocket.FromMediaInfo(inputInfo);
                        var outputSocket = MediaSocket.FromPreset(Preset.iPad_H264_720p);
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(videoFilePath);
                        //string baseFileName = fileName.Substring(0, fileName.IndexOf(Convert.ToChar(".")));
                        outputSocket.File = mergeFolder + "\\" + fileName + ".mp4";

                        using (var transcoder = new Transcoder())
                        {
                            transcoder.Inputs.Add(inputSocket);
                            transcoder.Outputs.Add(outputSocket);

                            if (transcoder.Open())
                            {
                                transcoder.Run();
                                transcoder.Close();
                            }
                        }
                    }
                //}
                Library.Shutdown();
                
                if(MergeFile(mergeFolder))
                {
                    Conversion_result.Text = " Success";
                }
            }
        }
    }   
}
