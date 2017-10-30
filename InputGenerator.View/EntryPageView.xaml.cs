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

namespace InputGenerator.View
{
    /// <summary>
    /// Interaction logic for EntryPageView.xaml
    /// </summary>
    public partial class EntryPageView : Window
    {
        public EntryPageView()
        {
            InitializeComponent();
        }

        public void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public void MaximizeButton_Click(object sender, EventArgs e)
        {
            //this.Height = this.MaxHeight;
            //  this.Width = this.MaxWidth;
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;

        }
        public void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnpatchFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (!String.IsNullOrEmpty(txtPath.Text))
                    dialog.SelectedPath = txtPath.Text;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnInputFile_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = txtPath.Text;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtOutput.Text = dialog.SelectedPath;// + "\\patch.input";
                }
            }
        }

    }
}
