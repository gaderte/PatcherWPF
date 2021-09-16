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

        Dictionary<string, int> options = new Dictionary<string, int>();
        Dictionary<string, float> nameDistance;
        string[] listeOptions;

        private static string NEUZ_RESOLUTION = "";
        private static bool NEUZ_FULLSCREEN = false;
        private static string NEUZ_SHADOW = "";
        private static string NEUZ_SIGHT = "";
        private static string NEUZ_TEXTURE = "";
        private static string NEUZ_DETAILS = "";
        private static bool NEUZ_ANTIALIASING = false;
        private static bool NEUZ_ANISOTROPIC = false;
        private static bool NEUZ_MIPMAP = false;
        private static string NEUZ_NVD = "";

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
                Application.Current.Shutdown();
            }
            listeOptions = new string[] { "resolution", "fullscreen", "texture", "view", "detail", "shadow",
                "ANTIALIASING", "ANISOTROPIC", "MIPMAP", "NameViewDistance"};
            nameDistance = new Dictionary<string, float>()
            {
                { "0", 40 },
                { "1", 60 },
                { "2", 80 },
                { "3", 130 },
            };
            WriteLog("Chargement des options...\n");
            loadOptions(path);
            WriteLog("Options correctement chargées\n");
            
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

            //PATCH
            //We delete patchlog file
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
            WriteLog("Connecté au serveur !\n");
            //Téléchagement et décompression de la liste
            WriteLog("Récupération de la liste...\n");
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
                WriteLog("Téléchargement de " + FILE_TO_DOWNLOAD.Count + " fichiers\n");
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

        private void loadOptions(string path)
        {
            //Read the neuz.ini file and store it into a table
            string[] neuz_ini = File.ReadAllLines(path);
            for (int i = 0; i < listeOptions.Length; i++)
            {
                if (Array.FindIndex(neuz_ini, s => s.StartsWith(listeOptions[i])) == -1) {
                    if (listeOptions[i] == "resolution")
                    {
                        File.AppendAllText(path, "\n" + listeOptions[i] + " 800 600");
                        neuz_ini = File.ReadAllLines(path);
                    } else if (listeOptions[i] == "NameViewDistance") {
                        File.AppendAllText(path, "\n" + listeOptions[i] + " 130");
                        neuz_ini = File.ReadAllLines(path);
                    } else
                    {
                        File.AppendAllText(path, "\n" + listeOptions[i] + " 0");
                        neuz_ini = File.ReadAllLines(path);
                    }
                }
                options.Add(listeOptions[i], Array.FindIndex(neuz_ini, s => s.StartsWith(listeOptions[i])));
            }

            //Doing some useless changes
            string[] temp_res = neuz_ini[options["resolution"]].Split(' ');
            string[] temp_fullscreen = neuz_ini[options["fullscreen"]].Split(' ');
            string[] temp_texture = neuz_ini[options["texture"]].Split(' ');
            string[] temp_view = neuz_ini[options["view"]].Split(' ');
            string[] temp_detail = neuz_ini[options["detail"]].Split(' ');
            string[] temp_shadow = neuz_ini[options["shadow"]].Split(' ');
            string[] temp_antialiasing = neuz_ini[options["ANTIALIASING"]].Split(' ');
            string[] temp_anisotropic = neuz_ini[options["ANISOTROPIC"]].Split(' ');
            string[] temp_mipmap = neuz_ini[options["MIPMAP"]].Split(' ');
            string[] temp_nvd = neuz_ini[options["NameViewDistance"]].Split(' ');
            
            NEUZ_FULLSCREEN = temp_fullscreen[1] == "0" ? false : true;
            NEUZ_RESOLUTION = temp_res[1] + "x" + temp_res[2];
            NEUZ_SHADOW = temp_shadow[1];
            NEUZ_SIGHT = temp_view[1];
            NEUZ_TEXTURE = temp_texture[1];
            NEUZ_DETAILS = temp_detail[1];
            NEUZ_ANTIALIASING = temp_antialiasing[1] == "0" ? false : true;
            NEUZ_ANISOTROPIC = temp_anisotropic[1] == "0" ? false : true;
            NEUZ_MIPMAP = temp_mipmap[1] == "0" ? false : true;
            float.TryParse(temp_nvd[1], out float temp);
            NEUZ_NVD = nameDistance[nameDistance.FirstOrDefault(x => x.Value == temp).Key].ToString();

            optionsDisplay();
        }

        private void optionsDisplay()
        {
            //Chargement options
            var comboBoxItem = choixResolution.Items.OfType<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == NEUZ_RESOLUTION);
            int choix = choixResolution.SelectedIndex = choixResolution.Items.IndexOf(comboBoxItem);
            choixResolution.SelectedIndex = choix;
            if (NEUZ_FULLSCREEN) fullscreen.IsChecked = true;
            if (NEUZ_ANTIALIASING) Antialiasing.IsChecked = true;
            if (NEUZ_ANISOTROPIC) Anisotropique.IsChecked = true;
            if (NEUZ_MIPMAP) mipMapping.IsChecked = true;
            int.TryParse(NEUZ_SIGHT, out int tempsight);
            sightBar.Value = tempsight;
            int.TryParse(NEUZ_SHADOW, out int tempshadow);
            shadowBar.Value = tempshadow;
            int.TryParse(NEUZ_DETAILS, out int tempdetail);
            detailBar.Value = tempdetail;
            int.TryParse(NEUZ_TEXTURE, out int temptexture);
            textBar.Value = temptexture;
            float.TryParse(NEUZ_NVD, out float tempnvd);
            int.TryParse(nameDistance.FirstOrDefault(x => x.Value == tempnvd).Key, out int key);
            displayName.Value = key;
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
            WriteLog("Décompression du fichier " + filename + "...\n");
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

            string[] temp = NEUZ_RESOLUTION.Split('x');
            neuz[options["resolution"]] = "resolution " + temp[0] + " " + temp[1];
            neuz[options["view"]] = "view " + NEUZ_SIGHT;
            neuz[options["texture"]] = "texture " + NEUZ_TEXTURE;
            neuz[options["shadow"]] = "shadow " + NEUZ_SHADOW;
            neuz[options["detail"]] = "detail " + NEUZ_DETAILS;
            neuz[options["NameViewDistance"]] = "NameViewDistance " + NEUZ_NVD;

            if (NEUZ_FULLSCREEN) neuz[options["fullscreen"]] = "fullscreen 1";
            else neuz[options["fullscreen"]] = "fullscreen 0";
            if (NEUZ_ANTIALIASING) neuz[options["ANTIALIASING"]] = "ANTIALIASING 0";
            else neuz[options["ANTIALIASING"]] = "ANTIALIASING 1";
            if (NEUZ_ANISOTROPIC) neuz[options["ANISOTROPIC"]] = "ANISOTROPIC 0";
            else neuz[options["ANISOTROPIC"]] = "ANISOTROPIC 1";
            if (NEUZ_MIPMAP) neuz[options["MIPMAP"]] = "MIPMAP 0";
            else neuz[options["MIPMAP"]] = "MIPMAP 1";

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
            NEUZ_RESOLUTION = typeItem.Content.ToString();
        }

        private void fullscreen_Checked(object sender, RoutedEventArgs e)
        {
            NEUZ_FULLSCREEN = (bool)fullscreen.IsChecked;
        }

        private void sightBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)sightBar.Value;
            NEUZ_SIGHT = val.ToString();
        }

        private void textBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)textBar.Value;
            NEUZ_TEXTURE = val.ToString();
        }

        private void detailBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)detailBar.Value;
            NEUZ_DETAILS = val.ToString();
        }

        private void shadowBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)shadowBar.Value;
            NEUZ_SHADOW = val.ToString();
        }

        private void displayName_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)displayName.Value;
            NEUZ_NVD = nameDistance[val.ToString()].ToString();
        }
    }
}
