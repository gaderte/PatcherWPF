﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PatcherWPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string SERVER_URL = "http://62.210.104.245/patcherupdate/";
        private static readonly string DISCORD_LINK = "https://discord.gg/AZkdPNURhd";
        private static readonly string WEBSITE_URL = "https://www.google.com";

        private static List<string> FILE_TO_DOWNLOAD = new List<string>();
        private static int NBR_FILE_TO_DLL = 0;
        private static int NBR_ATM = 1;

        public static Point coord;
        private static bool isShowed = false;
        bool _shown;

        Source.Window1 form = new Source.Window1();

        public MainWindow()
        {
            InitializeComponent();
            startBtn.IsEnabled = false;
            LocationChanged += new EventHandler(Window_LocationChanged);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            form.Top = this.Top + this.Height;
            form.Left = this.Left;
            coord = new Point(this.Left, this.Top);
        }

        private void quitOnClick(object sender, RoutedEventArgs e)
        {
            form.saveOptions();
            Application.Current.Shutdown();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (_shown) return;
            _shown = true;

            form.Show();
            form.Visibility = Visibility.Hidden;

            coord = new Point(this.Left, this.Top);
            //PATCH
            File.Delete(@"./patchlog.txt");
            WriteLog("Connexion au serveur...\n");
            bool isOk = false;
            while (!isOk)
            {
                if (!checkServerConn())
                {
                    MessageBoxResult result = MessageBox.Show("La connexion au serveur ne peut être établi. Réessayer ?", "Erreur", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if(result == MessageBoxResult.Yes)
                    {
                        isOk = false;
                    } else if (result == MessageBoxResult.No)
                    {
                        break;
                    }
                }
                else
                {
                    isOk = true;
                }

            }
            if (!isOk)
            {
                Application.Current.Shutdown();
            }
            //this.Controls.Add(siteWeb);
            WriteLog("Connecté au serveur !\n");
            //Téléchagement et décompression de la liste
            DownloadList();
            string pat = "./list.txt.gz";
            Source.Gzip.Decompress(new FileInfo(pat));
            File.Delete("./list.txt.gz");

            //Lecture liste
            string[] list = File.ReadAllLines("./list.txt");
            readList(list);

            if (FILE_TO_DOWNLOAD.Count == 0)
            {
                WriteLog("Tous les fichiers sont à jour\n");
                filesLeft.Content = "1 / 1";
                barAvAct.Value = 100;
                barAvTot.Value = 100;
                startBtn.IsEnabled = true;
            }
            else
            {
                filesLeft.Content = "0 / " + NBR_FILE_TO_DLL;
                //Télécharger les nouveaux fichiers
                foreach (string i in FILE_TO_DOWNLOAD)
                {
                    DownloadFile(i);
                }
            }
        }
        private void readList(string[] list)
        {
            int it = 2;
            for (it = 2; it < list.Length; it++)
            {
                readLine(list[it]);
            }
        }

        private void readLine(string line)
        {
            string[] linx = line.Split(' ');
            var tab = new List<string>();
            for (int i = 0; i < linx.Length; i++)
            {
                if (linx[i].Length > 4)
                {
                    tab.Add(linx[i]);
                }
            }
            isDownload(tab[0], tab[1], tab[2], tab[3]);
        }

        private void isDownload(string date, string heure, string taille, string fichier)
        {
            //Comparaison entre fichiers existants et fichiers à télécharger
            string path = "./" + fichier;
            FileInfo test = new FileInfo(path);
            try
            {
                long tailleSys = test.Length;
                if (taille != tailleSys.ToString())
                {
                    FILE_TO_DOWNLOAD.Add(fichier);
                    NBR_FILE_TO_DLL++;
                }
            }
            catch (FileNotFoundException)
            {
                FILE_TO_DOWNLOAD.Add(fichier);
                NBR_FILE_TO_DLL++;
            }
        }

        private bool checkServerConn()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(SERVER_URL);
                request.KeepAlive = false;
                request.Timeout = 5000;
                var response = (HttpWebResponse)request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void WriteLog(string log)
        {
            File.AppendAllText(@"./patchlog.txt", log);
        }

        private void DownloadList()
        {
            string tempUrl = SERVER_URL + "list.txt.gz";
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(tempUrl), "list.txt.gz");
            WriteLog("Liste téléchargée\n");

        }
        private void DownloadFile(string filename)
        {
            string tempUrl = SERVER_URL + filename;
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.QueryString.Add("file", filename);
            WriteLog("Téléchargement du fichier " + filename + "...\n");
            filename = filename.Replace("Data", "data");
            filename += ".gz";
            webClient.DownloadFileAsync(new Uri(tempUrl), filename);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            barAvAct.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            string filename = ((WebClient)(sender)).QueryString["file"];
            WriteLog("Téléchargement terminé !\n");
            barAvTot.Value = NBR_ATM * 100 / NBR_FILE_TO_DLL;
            filesLeft.Content = NBR_ATM + " / " + NBR_FILE_TO_DLL;
            NBR_ATM++;
            if (NBR_ATM == NBR_FILE_TO_DLL + 1)
            {
                startBtn.IsEnabled = true;
            }
            Source.Gzip.Decompress(new FileInfo("./" + filename + ".gz"));
            File.Delete("./" + filename + ".gz");
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            form.saveOptions();

            //On lance le jeu
            Process.Start("./Neuz.exe", "62.210.104.245 sunkist");
            Application.Current.Shutdown();
        }

        private void siteWebBtn_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = WEBSITE_URL,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void discordBtn_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = DISCORD_LINK,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void optionsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isShowed)
            {
                isShowed = false;
                form.Visibility = Visibility.Hidden;
            }
            else
            {
                isShowed = true;
                form.Visibility = Visibility.Visible;
            }
        }

        private void test_Click(object send, RoutedEventArgs e)
        {
            testBtn.Opacity = 1;
        }

        private void test_Pressed(object send, RoutedEventArgs e)
        {
            testBtn.Opacity = 0.5;
        }
    }
}
