using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InputGenerator.ViewModel
{
    public class EntryPageViewModel : INotifyPropertyChanged
    {
        private ButtonCommand AddCommand;

        private ButtonCommand AutoSelectProductCommand;

        private ButtonCommand PatchPropertiesCommand;

        private ButtonCommand PatchInputCommand;

        //private KeyBindingCommand ProductAddByEnterKeyCommand;

        private string _path = string.Empty;
        private string _outputPath = string.Empty;
        private string _buildName = string.Empty;
        private string _product = string.Empty;
        private string _productList = string.Empty;
        private bool _isDatabase = false;

        private bool _isValidPath = false;
        public EntryPageViewModel()
        {
            this.AddCommand = new ButtonCommand(AddCommand_ExecuteDelegate, AddCommand_CanExecuteDelegate);
            this.AutoSelectProductCommand = new ButtonCommand(AutoSelectProductCommand_ExecuteDelegate, AutoSelectProductCommand_CanExecuteDelegate);
            this.PatchPropertiesCommand = new ButtonCommand(PatchPropertiesCommand_ExecuteDelegate, PatchPropertiesCommand_CanExecuteDelegate);
            this.PatchInputCommand = new ButtonCommand(PatchInputCommand_ExecuteDelegate, PatchInputCommand_CanExecuteDelegate);
            //this.ProductAddByEnterKeyCommand = new KeyBindingCommand(ProductAddByEnterKeyCommand_ExecuteDelagate, ProductAddByEnterKeyCommand_CanExecuteDelegate);
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                if (!String.IsNullOrEmpty(_path))
                {
                    var dInfo = new DirectoryInfo(_path);
                    if (dInfo.Exists)
                        _isValidPath = true;
                    else
                        _isValidPath = false;
                }
                else
                    _isValidPath = false;
                AutoSelectProductCommand.Refresh();
            }
        }

        public string OutputPath
        {
            get
            {
                return _outputPath;
            }
            set
            {
                _outputPath = value;
            }
        }

        public string BuildName
        {
            get
            {
                return _buildName;
            }
            set
            {
                _buildName = value;
                PatchPropertiesCommand.Refresh();
            }
        }

        public string Product
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
                AddCommand.Refresh();
            }
        }

        public string ProductList
        {
            get
            {
                return _productList;
            }
            set
            {
                _productList = value;
                PatchPropertiesCommand.Refresh();
            }
        }

        public bool IsDatabase
        {
            get
            {
                return _isDatabase;
            }
            set
            {
                _isDatabase = value;
            }
        }

        public ICommand btnAddCommand
        {
            get
            {
                return AddCommand;
            }
        }

        public ICommand btnAutoSelectProductCommand
        {
            get
            {
                return AutoSelectProductCommand;
            }
        }

        public ICommand btnPatchPropertiesCommand
        {
            get
            {
                return PatchPropertiesCommand;
            }
        }

        public ICommand btnPatchInputCommand
        {
            get
            {
                return PatchInputCommand;
            }
        }

        //public ICommand enterKeyPressCommand
        //{
        //    get
        //    {
        //        return ProductAddByEnterKeyCommand;
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddCommand_ExecuteDelegate()
        {

            if (IsDatabase == true)
            {
                if (String.IsNullOrEmpty(ProductList))
                    ProductList = Product.Substring(0, Product.Length - 8) + "_DB:" + Product;
                else
                    ProductList += "," + Product.Substring(0, Product.Length - 8) + "_DB:" + Product;
            }
            else
            {
                if (String.IsNullOrEmpty(ProductList))
                {
                    ProductList = Product;
                }
                else
                {
                    ProductList += "," + Product;
                }
            }
            Product = string.Empty;
            IsDatabase = false;
            RaisePropertyChangeEvent("Product");
            RaisePropertyChangeEvent("ProductList");
        }

        private bool AddCommand_CanExecuteDelegate()
        {
            if (Product.Contains(' '))
            {
                return false;
            }
            else if (String.IsNullOrEmpty(Product))
            {
                return false;
            }
            else if (IsDatabase == true)
            {
                if (!Product.ToUpper().EndsWith("DATABASE"))
                    return false;
            }
            return true;
        }

        private void AutoSelectProductCommand_ExecuteDelegate()
        {
            List<string> productNames = new List<string>();
            var products = new DirectoryInfo(_path).GetDirectories();
            if (products != null && products.Length > 0)
            {
                foreach (var product in products)
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
                ProductList = String.Join(",", productNames);
            }
            RaisePropertyChangeEvent("ProductList");
        }

        private bool AutoSelectProductCommand_CanExecuteDelegate()
        {
            return _isValidPath;
        }

        private void PatchPropertiesCommand_ExecuteDelegate()
        {
            List<string> products = new List<string>();
            products = ProductList.Split(',').ToList();
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
            builder.Append("BUILD_NAME=").Append(this.BuildName);
            using (var output = System.IO.File.Create(this.OutputPath + "\\patch.properties"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(builder.ToString());
                output.Write(info, 0, info.Length);
            }
        }

        private bool PatchPropertiesCommand_CanExecuteDelegate()
        {
            if (String.IsNullOrEmpty(this.ProductList))
            {
                return false;
            }
            else if (String.IsNullOrEmpty(this.BuildName))
            {
                return false;
            }
            return true;
        }

        private void PatchInputCommand_ExecuteDelegate()
        {

        }

        private bool PatchInputCommand_CanExecuteDelegate()
        {
            return true;
        }


        public void RaisePropertyChangeEvent(string propertyName)
        {
            if(PropertyChanged!=null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        //private void ProductAddByEnterKeyCommand_ExecuteDelagate()
        //{
            
        //}

        //private bool ProductAddByEnterKeyCommand_CanExecuteDelegate()
        //{
        //    return true;
        //}
    }
}
