using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputGenerator.Model
{
    public class Folder
    {
        public ICollection<File> Files { get; set; }
        public ICollection<Folder> Folders { get; set; }
        public string FolderName { get; set; }
        public string ParentFolder { get; set; }
        public bool IsProcessed = false;

        public Folder()
        {
            Folders = new List<Folder>();
            Files = new List<File>();
        }
    }
}
