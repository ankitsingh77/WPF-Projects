using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PathInputFile
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var output = File.Create(ConfigurationManager.AppSettings["OutputPath"]))
            {
                StringBuilder text = new StringBuilder();
                string[] products = ConfigurationManager.AppSettings["PRODUCTS"].Split(',');
                string productsPath = ConfigurationManager.AppSettings["ProductsPath"];
                foreach (var product in products)
                {
                    if (product.EndsWith("DB"))
                    {
                        string DBName = product.Substring(0, product.Length - 2);
                        string DBPath = productsPath + "\\" + product;
                        if (Directory.Exists(DBPath))
                        {
                            text.Append("#---------" + DBName + " DB---------#" + Environment.NewLine);
                            DirectoryInfo rootDir = new DirectoryInfo(DBPath);
                            foreach (var dir in rootDir.GetDirectories())
                            {
                                var files = dir.GetFiles().ToList().Where(o => !o.Name.ToUpper().Contains("ROLLBACK")).ToList();

                                foreach (var file in files)
                                {
                                    string filepath = file.FullName.Substring(productsPath.LastIndexOf('\\')+1);
                                    text.Append(FormDBCommand(DBName, filepath));
                                }
                            }
                          
                        }
                    }
                    else if (Directory.Exists(productsPath + "\\" + product))
                    {
                        text.Append("#---------" + product + " ---------#" + Environment.NewLine);
                        DirectoryInfo productDir = new DirectoryInfo(productsPath + "\\" + product);
                        productDir.GetDirectories("*", SearchOption.AllDirectories);
                       // bool addUpdateExists = false;
                        //foreach (var directory in productdir.getdirectories())
                        //{
                        //    if (directory.name == "add")
                        //    {
                        //        addupdateexists = true;
                        //        foreach (var file in directory.getfiles())
                        //        {
                        //            string removeadd = file.fullname.remove(productspath.length + product.length + 2, 4);
                        //            string filepath = removeadd.substring(productspath.lastindexof('\\'));
                        //            text.append(formaddfilecommand(product, filepath));
                        //        }
                        //    }
                        //    else if (directory.name == "update")
                        //    {
                        //        addupdateexists = true;
                        //        foreach (var file in directory.getfiles())
                        //        {
                        //            string removeupdate = file.fullname.remove(productspath.length + product.length + 2, 7);
                        //            string filepath = removeupdate.substring(productspath.lastindexof('\\'));
                        //            text.append(formupdatefilecommand(product, filepath));
                        //        }
                        //    }
                        //}
                        //if (!addupdateexists)
                        //{
                        //    foreach (var file in productdir.getfiles())
                        //    {
                        //        string filepath = file.fullname.substring(productspath.lastindexof('\\'));
                        //        text.append(formupdatefilecommand(product, filepath));
                        //    }
                        //}
                    }
                }
                byte[] info = new UTF8Encoding(true).GetBytes(text.ToString());
                output.Write(info, 0, info.Length);
            }
        }

        static string FormDBCommand(string DBName, string filePath)
        {
            return "DBUpgrade,${" + DBName + "_DB_IP},${" + DBName + "_DB_NAME},${" + DBName + "_DB_USERNAME},${" + DBName + "_DB_PASSWORD},launcher\\patch\\" + filePath + "," + DBName + "_DB" + Environment.NewLine + Environment.NewLine;
        }

        static string FormAddFileCommand(string productName, string filePath)
        {
            return "FileAdd,launcher\\patch" + filePath + ",${" + productName + "_PATH}\\," + productName + Environment.NewLine + Environment.NewLine;
        }

        static string FormUpdateFileCommand(string productName, string filePath)
        {
            return "FileUpdate,launcher\\patch" + filePath + ",${" + productName + "_PATH}\\," + productName + Environment.NewLine + Environment.NewLine;
        }
    }
}
