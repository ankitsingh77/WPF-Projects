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
using System.Configuration;
using io = System.IO;

namespace InteractiveInputGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Folder> prods = null;
        List<StringBuilder> filePaths = new List<StringBuilder>();
        static StringBuilder stackString = new StringBuilder();
        public int PageNo { get; set; }

        string SourcePath;
        string PatchInputfile;
        string Productslist;


        public List<Folder> Products
        {
            get
            {
                if (prods != null)
                    return prods;
                else
                {
                    prods = LoadData();
                    return prods;
                }
            }
            set 
            {
                prods = value; 
            }
        }

        public MainWindow(string sourcePath, string outpath, string products):this()
        {
            this.SourcePath = sourcePath;
            this.PatchInputfile = outpath;
            this.Productslist = products;
            this.DataContext = Products;
        }

        public MainWindow()
        {
            InitializeComponent();
            PageNo = 1;
            this.Refresh.Visibility = System.Windows.Visibility.Collapsed;
        }

        private List<Folder> LoadData()
        {
            List<Folder> productList = new List<Folder>();
            try
            {
                string[] products = Productslist.Split(',');
                string productsPath = SourcePath;
                foreach (var product in products)
                {
                    if (!product.Contains(':') && io.Directory.Exists(productsPath + "\\" + product))
                    {
                        var productDir = new io.DirectoryInfo(productsPath + "\\" + product);
                        Folder prod = new Folder();
                        prod = this.GetHierarchy(productDir);
                        prod.FolderName = product;
                        productList.Add(prod);
                    }
                    else if(product.Contains("_DB"))
                    {
                        string DBName = product.Split(':')[1];
                        string DBPath = productsPath + "\\" + DBName;
                        StringBuilder text = new StringBuilder();
                        if (io.Directory.Exists(DBPath))
                        {
                            text.Append(Environment.NewLine+"#---------" + DBName + " DB---------#" + Environment.NewLine);
                            io.DirectoryInfo rootDir = new io.DirectoryInfo(DBPath);
                            foreach (var dir in rootDir.GetDirectories())
                            {
                                var files = dir.GetFiles().ToList().Where(o => !o.Name.ToUpper().Contains("ROLLBACK")).ToList();

                                foreach (var file in files)
                                {
                                    string filepath = file.FullName.Substring(productsPath.LastIndexOf('\\') + 1);
                                    text.Append(FormDBCommand(product.Split(':')[0], filepath));
                                }
                            }
                            filePaths.Add(text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return productList;
        }

        private Folder GetHierarchy(io.DirectoryInfo root)
        {
            Folder result = null;
            if (root != null)
            {
                result = new Folder();
                result.FolderName = root.Name;
                result.ParentFolder = root.Parent.Name;
                var fileList = root.GetFiles();
                foreach (var file in fileList)
                {
                    result.Files.Add(new File { FileName = file.Name, Operation = OperationType.Update, Parent = result });
                }
                if (root.GetDirectories().Count() > 0)
                {
                    foreach (var dir in root.GetDirectories())
                    {
                        result.Folders.Add(GetHierarchy(dir));
                    }
                }
            }
            return result;
        }

        private void IsAdd_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var product in Products)
            {
                filePaths.Add(new StringBuilder(Environment.NewLine).Append("#-----------").Append(product.FolderName).Append("-----------#" + Environment.NewLine));
                PrepareText(product);
                filePaths.Add(new StringBuilder(Environment.NewLine).Append("GenerateCheckSum,${").Append(product.FolderName).Append("_PATH},${").Append(product.FolderName).Append("_BACKUP_PATH},${").Append(product.FolderName).Append("_BACKUP_PATH}\\CheckSum_ModifiedFiles\\,").Append(product.FolderName).Append(Environment.NewLine));
            }
            using (var output = io.File.Create(PatchInputfile))
            {
                StringBuilder text = new StringBuilder();
                foreach (var statement in filePaths)
                {
                    text.Append(statement);
                }
                byte[] info = new UTF8Encoding(true).GetBytes(text.ToString());
                output.Write(info, 0, info.Length);
            }
            this.GenerateXml.IsEnabled = false;
            //this.Refresh.IsEnabled = true;
            this.ProductView.IsEnabled = false;
            MessageBox.Show("File generated successfully. Please check output path.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrepareText(Folder root)
        {
            Stack<Queue<Folder>> stackOfQueue = new Stack<Queue<Folder>>();
            Queue<Folder> q = new Queue<Folder>();
            q.Enqueue(root);
            stackOfQueue.Push(q);
            stackString.Append(root.FolderName);
            while (true)
            {
                if (stackOfQueue.Count == 0)
                    break;
                var f = stackOfQueue.Peek().Peek();
                if (!f.IsProcessed)
                {
                    foreach (var file in f.Files)
                    {
                        string command = string.Empty;
                        switch (file.Operation)
                        {
                            case OperationType.Add:
                                command = "FileAdd";    
                                break;
                            case OperationType.Update:
                                command = "FileUpdate";    
                                break;
                            case OperationType.AddorUpdate:
                                command = "FileAddorUpdate";    
                                break;

                        }
                        if(!String.IsNullOrEmpty(command))
                         filePaths.Add(new StringBuilder(FormFileCommand(root.FolderName, stackString.ToString() + "\\" + file.FileName, stackString.ToString(),command)));
                    }
                }
                if (f.Folders.Count > 0 && !f.IsProcessed)
                {
                    Queue<Folder> tempQ = new Queue<Folder>();
                    foreach (var fol in f.Folders)
                    {
                        tempQ.Enqueue(fol);
                    }
                    stackOfQueue.Push(tempQ);
                    stackString.Append("\\" + tempQ.Peek().FolderName);
                    f.IsProcessed = true;
                }
                else
                {
                    f.IsProcessed = true;
                    var removedFolder = stackOfQueue.Peek().Dequeue();
                    if (stackOfQueue.Peek().Count != 0)
                    {
                        stackString.Remove(stackString.Length-f.FolderName.Length,f.FolderName.Length);
                        stackString.Append(stackOfQueue.Peek().Peek().FolderName);
                    }
                    else 
                    {
                        var deletedQ = stackOfQueue.Pop();
                        if (removedFolder == root)
                            stackString.Clear();
                        else
                        stackString.Remove(stackString.Length - removedFolder.FolderName.Length - 1, removedFolder.FolderName.Length + 1);
                    }
                }
            }
        }

        static string FormDBCommand(string DBIdentifier, string filePath)
        {
            return "DBUpgrade,${" + DBIdentifier + "_IP},${" + DBIdentifier + "_NAME},${" + DBIdentifier + "_USERNAME},${" + DBIdentifier + "_PASSWORD},launcher\\patch\\" + filePath + "," + DBIdentifier + Environment.NewLine;
        }

        static string FormAddFileCommand(string productName, string filePath, string folderPath)
        {
            string folderSubPath = folderPath.Equals(productName) ? string.Empty : folderPath.Substring(productName.Length + 1);
            return new StringBuilder("FileAdd,launcher\\patch\\").Append(filePath).Append(",${").Append(productName).Append("_PATH}\\").Append(folderSubPath == String.Empty ? "," : folderSubPath + "\\,").Append(productName).Append(Environment.NewLine).ToString();
        }

        static string FormAddorUpdateFileCommand(string productName, string filePath, string folderPath)
        {
            string folderSubPath = folderPath.Equals(productName) ? string.Empty : folderPath.Substring(productName.Length + 1);
            return new StringBuilder("FileAddOrUpdate,launcher\\patch\\").Append(filePath).Append(",${").Append(productName).Append("_PATH}\\").Append(folderSubPath == String.Empty ? "," : folderSubPath + "\\,").Append(productName).Append(Environment.NewLine).ToString();
        }

        static string FormUpdateFileCommand(string productName, string filePath, string folderPath)
        {
            string folderSubPath = folderPath.Equals(productName)?string.Empty: folderPath.Substring(productName.Length+1);
            return new StringBuilder("FileUpdate,launcher\\patch\\").Append(filePath).Append(",${").Append(productName).Append("_PATH}\\").Append(folderSubPath == String.Empty ? "," : folderSubPath + "\\,").Append(productName).Append(Environment.NewLine).ToString();
        }

        static string FormFileCommand(string productName, string filePath, string folderPath,string command)
        {
            string folderSubPath = folderPath.Equals(productName) ? string.Empty : folderPath.Substring(productName.Length + 1);
            return new StringBuilder(command).Append(",launcher\\patch\\").Append(filePath).Append(",${").Append(productName).Append("_PATH}\\").Append(folderSubPath == String.Empty ? "," : folderSubPath + "\\,").Append(productName).Append(Environment.NewLine).ToString();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            prods = null;
            Products = null;
            filePaths = new List<StringBuilder>();
            stackString = new StringBuilder();
            this.GenerateXml.IsEnabled = true;
            this.Refresh.IsEnabled = false;
            this.ProductView.IsEnabled = true;
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
