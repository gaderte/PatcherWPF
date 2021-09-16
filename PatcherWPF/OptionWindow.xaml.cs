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
using System.Windows.Shapes;

namespace PatcherWPF.Source
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
        public OptionWindow()
        {
            InitializeComponent();
        }
     }
}
