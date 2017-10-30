using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputGenerator.Model
{
   public class File
    {
        public string FileName { get; set; }
        public Folder Parent { get; set; }
        public OperationType Operation { get; set; }
    }
}
