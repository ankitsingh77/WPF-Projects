using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InteractiveInputGenerator
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

        public IEnumerable Items
        {
            get
            {
                var items = new CompositeCollection();
                items.Add(new CollectionContainer { Collection = Folders });
                items.Add(new CollectionContainer { Collection = Files });
                return items;
            }
        }
    }

    public class File
    {
        public string FileName { get; set; }
        public Folder Parent { get; set; }
        public OperationType Operation { get; set; }
    }

    public enum OperationType
    {
        Add,
        Update,
        AddorUpdate
    }
}
