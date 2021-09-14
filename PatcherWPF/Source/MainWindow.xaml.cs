using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private static readonly string NEWS_URL = "http://62.210.104.245/news/news.json";

        private static List<string> FILE_TO_DOWNLOAD = new List<string>();
        private static int NBR_FILE_TO_DLL = 0;
        private static int NBR_ATM = 1;

        DateTime _startedAt;

        private static bool isShowed = false;
        bool _shown;

        private static string NEUZ_RESOLUTION;
        private static bool NEUZ_FULLSCREEN;
        private static string NEUZ_SHADOW;
        private static string NEUZ_SIGHT;
        private static string NEUZ_TEXTURE;
        private static string NEUZ_DETAILS;

        private static string NEUZ_N_RESOLUTION = "";
        private static bool NEUZ_N_FULLSCREEN = NEUZ_FULLSCREEN;
        private static string NEUZ_N_SHADOW = "";
        private static string NEUZ_N_SIGHT = "";
        private static string NEUZ_N_TEXTURE = "";
        private static string NEUZ_N_DETAILS = "";

        public MainWindow()
        {
            if (Process.GetProcessesByName("Neuz").Length > 0)
            {
                Process.Start("Neuz.exe", "62.210.104.245 sunkist");
                Application.Current.Shutdown();
            }
            InitializeComponent();
            startBtn.IsEnabled = false;
        }

        //Main function
        private void window_Loaded(object sender, EventArgs e)
        {
            string path = "./neuz.ini";
            //If the neuz.ini file doesn't exist
            if (!File.Exists(path))
            {
                //Display a box saying that the file doesn't exist
                MessageBox.Show("Le fichier Neuz.ini n'existe pas ! Verifiez que vous avez bien placé le launcher dans le dossier de jeu FlyFF !", "Fichier manquant !");
            }
            //Read the neuz.ini file and store it into a table
            string[] neuz_ini = File.ReadAllLines(path);
            //Doing some useless changes
            string[] temp_res = neuz_ini[3].Split(' ');
            string[] temp_fullscreen = neuz_ini[4].Split(' ');
            string[] temp_shadow = neuz_ini[9].Split(' ');
            string[] temp_sight = neuz_ini[6].Split(' ');
            string[] temp_texture = neuz_ini[5].Split(' ');
            string[] temp_detail = neuz_ini[7].Split(' ');

            NEUZ_RESOLUTION = temp_res[1] + "x" + temp_res[2];
            if (temp_fullscreen[1] == "0")
            {
                NEUZ_FULLSCREEN = false;
            }
            else
            {
                NEUZ_FULLSCREEN = true;
            }
            NEUZ_SHADOW = temp_shadow[1];
            NEUZ_SIGHT = temp_sight[1];
            NEUZ_TEXTURE = temp_texture[1];
            NEUZ_DETAILS = temp_detail[1];
        }

        private void quitOnClick(object sender, RoutedEventArgs e)
        {
            saveOptions();
            Application.Current.Shutdown();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (_shown) return;
            _shown = true;

            //Chargement options
            var comboBoxItem = choixResolution.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == NEUZ_RESOLUTION);
            int choix = choixResolution.SelectedIndex = choixResolution.Items.IndexOf(comboBoxItem);
            choixResolution.SelectedIndex = choix;
            if (NEUZ_FULLSCREEN)
            {
                fullscreen.IsChecked = true;
            }
            int.TryParse(NEUZ_SIGHT, out int tempsight);
            sightBar.Value = tempsight;
            int.TryParse(NEUZ_SHADOW, out int tempshadow);
            shadowBar.Value = tempshadow;
            int.TryParse(NEUZ_DETAILS, out int tempdetail);
            detailBar.Value = tempdetail;
            int.TryParse(NEUZ_TEXTURE, out int temptexture);
            textBar.Value = temptexture;

            //PATCH
            //We delete patchlog file
            File.Delete(@"./patchlog.txt");
            WriteLog("Connexion au serveur...\n");
            //System.Threading.Thread.Sleep(15000);
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
            DownloadNews();
            string pat = "./list.txt.gz";
            Source.Gzip.Decompress(new FileInfo(pat));
            File.Delete("./list.txt.gz");

            //Lecture liste
            string[] list = File.ReadAllLines("./list.txt");
            readList(list);

            string news = File.ReadAllText("news.json");
            readNews(news);
            File.Delete("news.json");

            if (FILE_TO_DOWNLOAD.Count == 0)
            {
                WriteLog("Tous les fichiers sont à jour\n");
                filesLeft.Content = "1 / 1";
                dllLeft.Content = "Téléchargement terminé";
                vitesseDLL.Content = "0.00 Mo/s";
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
                    //Check if directory is created
                    string[] dirs = i.Split('\\');
                    string temp = "./";
                    foreach (string x in dirs)
                    {
                        if (!x.Contains('.'))
                        {
                            temp += x + '/';
                        }
                    }
                    DirectoryInfo di = Directory.CreateDirectory("./" + temp);
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

        private void readNews(string txtNews)
        {
            Source.ListNews test = JsonConvert.DeserializeObject<Source.ListNews>(txtNews);
            newsLabel.Content += "\n\n";
            foreach(Source.News x in test.news)
            {
                newsLabel.Content += x.date + " - " + x.title + " : \n" + x.content;
                newsLabel.Content += "\n\n -----------------------------------------\n\n";
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
        private void DownloadNews()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(NEWS_URL), "news.json");
            WriteLog("News téléchargées\n");
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
            if (_startedAt == default(DateTime))
            {
                _startedAt = DateTime.Now;
            }
            else
            {
                var timeSpan = DateTime.Now - _startedAt;
                try
                {
                    var bytesPerSecond = e.BytesReceived / (long)timeSpan.TotalSeconds;
                    vitesseDLL.Content = (bytesPerSecond / 1024d / 1024d).ToString("0.00") + " Mo/s";
                } catch(System.DivideByZeroException)
                {
                    vitesseDLL.Content = "0.00 Mo/s";
                }
                    
            }
            dllLeft.Content = string.Format("{0} Mo / {1} Mo",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            barAvAct.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            string filename = ((WebClient)(sender)).QueryString["file"];
            barAvTot.Value = NBR_ATM * 100 / NBR_FILE_TO_DLL;
            filesLeft.Content = NBR_ATM + " / " + NBR_FILE_TO_DLL;
            NBR_ATM++;
            if (NBR_ATM == NBR_FILE_TO_DLL + 1)
            {
                startBtn.IsEnabled = true;
            }

            Source.Gzip.Decompress(new FileInfo("./" + filename + ".gz"));
            File.Delete("./" + filename + ".gz");
            int size = FILE_TO_DOWNLOAD.Count();
            if(filename == FILE_TO_DOWNLOAD[size-1])
            {
                dllLeft.Content = "Téléchargement terminé";
                vitesseDLL.Content = "0.00 Mo/s";
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            saveOptions();

            //On lance le jeu
            Process.Start("Neuz.exe", "62.210.104.245 sunkist");
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
            var converter = new System.Windows.Media.BrushConverter();
            if (isShowed)
            {
                CanvasOptions.Opacity = 0;
                isShowed = false;
            }
            else
            {
                CanvasOptions.Opacity = 1;
                isShowed = true;
            }
        }

        public void saveOptions()
        {
            string path = "./neuz.ini";

            if (!File.Exists(path))
            {
                MessageBox.Show("Le fichier Neuz.ini n'existe pas ! Verifiez que vous avez bien placé le launcher dans le dossier de jeu FlyFF !", "Fichier manquant !");
            }

            string[] neuz = File.ReadAllLines(path);
            if (NEUZ_N_RESOLUTION != "")
            {
                string[] temp = NEUZ_N_RESOLUTION.Split('x');
                neuz[3] = "resolution " + temp[0] + " " + temp[1];
            }
            if (NEUZ_N_FULLSCREEN)
            {
                neuz[4] = "fullscreen 1";
            }
            else
            {
                neuz[4] = "fullscreen 0";
            }
            if (NEUZ_N_SIGHT != "")
            {
                neuz[6] = "view " + NEUZ_N_SIGHT;
            }
            if (NEUZ_N_TEXTURE != "")
            {
                neuz[5] = "texture " + NEUZ_N_TEXTURE;
            }
            if (NEUZ_N_SHADOW != "")
            {
                neuz[9] = "shadow " + NEUZ_N_SHADOW;
            }
            if (NEUZ_N_DETAILS != "")
            {
                neuz[7] = "detail " + NEUZ_N_DETAILS;
            }
            File.WriteAllLines("./neuz.ini", neuz);
        }

        private void test_Click(object send, RoutedEventArgs e)
        {
            //Fonction click (ouvrir Discord par exemple)
            var psi = new ProcessStartInfo
            {
                FileName = DISCORD_LINK,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void test_Pressed(object send, RoutedEventArgs e)
        {
            testBtn.Opacity = 0.5;
        }

        private void test_Release(object send, RoutedEventArgs e)
        {
            testBtn.Opacity = 1;
        }

        private void choixResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)choixResolution.SelectedItem;
            NEUZ_N_RESOLUTION = typeItem.Content.ToString();
        }

        private void fullscreen_Checked(object sender, RoutedEventArgs e)
        {
            NEUZ_N_FULLSCREEN = (bool)fullscreen.IsChecked;
        }

        private void sightBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)sightBar.Value;
            NEUZ_N_SIGHT = val.ToString();
        }

        private void textBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)textBar.Value;
            NEUZ_N_TEXTURE = val.ToString();
        }

        private void detailBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)detailBar.Value;
            NEUZ_N_DETAILS = val.ToString();
        }

        private void shadowBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)shadowBar.Value;
            NEUZ_N_SHADOW = val.ToString();
        }
    }
}
