using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace PatcherWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OptionWindow : Window
    {
        private Dictionary<string, int> options = new Dictionary<string, int>();
        private Dictionary<string, string> nameDistance;
        private string[] listeOptions;
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
        public bool _isShown { get; set; }
        public OptionWindow()
        {
            InitializeComponent();
            OptionWin.Hide();
            _isShown = false;
            listeOptions = new string[] { "resolution", "fullscreen", "texture", "view", "detail", "shadow",
                "ANTIALIASING", "ANISOTROPIC", "MIPMAP", "NameViewDistance"};
            nameDistance = new Dictionary<string, string>()
            {
                { "0", "40.000000" },
                { "1", "60.000000" },
                { "2", "80.000000" },
                { "3", "130.000000" },
            };
        }

        public void loadOptions(string path)
        {
            //Read the neuz.ini file and store it into a table
            string[] neuz_ini = File.ReadAllLines(path);
            for (int i = 0; i < listeOptions.Length; i++)
            {
                if (Array.FindIndex(neuz_ini, s => s.StartsWith(listeOptions[i])) == -1)
                {
                    if (listeOptions[i] == "resolution")
                    {
                        File.AppendAllText(path, "\n" + listeOptions[i] + " 800 600");
                        neuz_ini = File.ReadAllLines(path);
                    }
                    else if (listeOptions[i] == "NameViewDistance")
                    {
                        File.AppendAllText(path, "\n" + listeOptions[i] + " 130.000000");
                        neuz_ini = File.ReadAllLines(path);
                    }
                    else
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
            NEUZ_NVD = nameDistance[nameDistance.FirstOrDefault(x => x.Value == temp_nvd[1]).Key].ToString();

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
            int.TryParse(nameDistance.FirstOrDefault(x => x.Value == NEUZ_NVD).Key, out int key);
            displayName.Value = key;
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

        private void quitOnClick(object sender, RoutedEventArgs e)
        {
            saveOptions();
            _isShown = false;
            OptionWin.Hide();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
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
