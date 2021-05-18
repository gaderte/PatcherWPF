using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PatcherWPF.Source
{
    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
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

        public Window1()
        {
            InitializeComponent();
            this.Top = MainWindow.coord.Y + 480;
            this.Left = MainWindow.coord.X;
        }

        private void Options_Load(object sender, EventArgs e)
        {
            string[] neuz_ini = File.ReadAllLines("./neuz.ini");

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

        protected override void OnContentRendered(EventArgs e)
        {
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
        }

        public void saveOptions()
        {
            string[] neuz = File.ReadAllLines("./neuz.ini");
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
