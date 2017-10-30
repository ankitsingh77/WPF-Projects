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
using System.IO;

namespace InteractiveInputGenerator
{
    /// <summary>
    /// Interaction logic for InformationPage.xaml
    /// </summary>
    public partial class InformationPage : Window
    {
        public InformationPage()
        {
            InitializeComponent();
        }

        private void btnpatchFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (!String.IsNullOrEmpty(txtPath.Text))
                    dialog.SelectedPath = txtPath.Text;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result==System.Windows.Forms.DialogResult.OK)
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

        private void btnProceed_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtPath.Text))
            {
                MessageBox.Show("Please select the patch source path");
                return;
            }
            else
            {
                try
                {
                    var dInfo = new DirectoryInfo(txtPath.Text);
                    if (!dInfo.Exists)
                        throw new Exception("Path not found");
                }
                catch
                {
                    MessageBox.Show("Source Path does not exists");
                    return;
                }
            }
            if (string.IsNullOrEmpty(txtOutput.Text))
            {
                MessageBox.Show("Please select the output path");
                return;
            }
            if (String.IsNullOrEmpty(txtProducts.Text))
            {
                MessageBox.Show("Please mention atleast one product which is available in the source path");
                return;
            }
            MainWindow mainWinodw = new MainWindow(txtPath.Text, txtOutput.Text+"\\patch.input", txtProducts.Text);
            mainWinodw.ShowDialog();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtProductName.Text.Contains(' '))
            {
                MessageBox.Show("Space is not allowed in Product Name");
                txtProductName.Clear();
                return;
            }
            if (chkIsDatabase.IsChecked == true)
            {
                if (txtProductName.Text.ToUpper().EndsWith("DATABASE"))
                {
                    if (String.IsNullOrEmpty(txtProducts.Text))
                        txtProducts.Text = txtProductName.Text.Substring(0, txtProductName.Text.Length - 8) + "_DB:" + txtProductName.Text;
                    else
                        txtProducts.Text += "," + txtProductName.Text.Substring(0, txtProductName.Text.Length - 8) + "_DB:" + txtProductName.Text;
                }
                else
                {
                    MessageBox.Show("Database name must be suffixed with Database");
                }
            }
            else
            {
                if (String.IsNullOrEmpty(txtProducts.Text))
                {
                    txtProducts.Text = txtProductName.Text;
                }
                else
                {
                    txtProducts.Text += "," + txtProductName.Text;
                }
            }
            txtProductName.Clear();
            chkIsDatabase.IsChecked = false;
        }

        private void txtProductName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Return)
            btnAdd_Click(null, null);
        }

        private void btnAutoSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(txtPath.Text))
            {
                try
                {
                    var dInfo = new DirectoryInfo(txtPath.Text);
                    if (!dInfo.Exists)
                        throw new Exception("Path not found");
                }
                catch
                {
                    return;
                }
                List<string> productNames = new List<string>();
                var products =  new DirectoryInfo(txtPath.Text).GetDirectories();
                if(products!=null && products.Length>0)
                {
                    foreach(var product in products)
                    {
                        if (product.Name.ToUpper().EndsWith("DATABASE"))
                        {
                            productNames.Add(product.Name.Substring(0, product.Name.Length - 8) + "_DB:" + product.Name);
                        }
                        else
                            productNames.Add(product.Name);
                    }
                }
                if (productNames.Count > 0)
                {
                    txtProducts.Text = String.Join(",", productNames);
                }
            }
        }

        private void btnPatchProperties_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtProducts.Text))
            {
                MessageBox.Show("Please input products. Cannot generate properties file.");
                return;
            }
            else if (String.IsNullOrEmpty(txtBuildName.Text))
            {
                MessageBox.Show("Please input build name. Cannot generate properties file.");
                return;
            }
            else
            {
                List<string> products = new List<string>();
                products = txtProducts.Text.Split(',').ToList();
                StringBuilder builder = new StringBuilder();
                builder.Append("PRODUCTS=");
                foreach (var product in products)
                {
                    if (product.Contains(':'))
                    {
                        builder.Append(product.Remove(product.LastIndexOf(':'))).Append(",");
                    }
                    else
                        builder.Append(product).Append(",");
                }
                builder.Remove(builder.Length - 1, 1);
                builder.Append(Environment.NewLine);
                builder.Append("BUILD_NAME=").Append(txtBuildName.Text);
                using (var output = System.IO.File.Create(txtOutput.Text+"\\patch.properties"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(builder.ToString());
                    output.Write(info, 0, info.Length);
                }
                MessageBox.Show("File generated successfully. Please check output path.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
    }
}
