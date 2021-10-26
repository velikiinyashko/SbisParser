using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbisParser.Models
{
    public class SbisSettings
    {
        public string DocumentsPath { get; set; }
        public string SchemesPath { get; set; }
        public string IsIncorectDocumentsPath { get; set; }
        public NameTableDataBase NameTableDataBase { get; set; }
        public bool IsLoadItems { get; set; }
        public string MaskFile { get; set; }
        public string[] Function { get; set; }
        public bool WriteToBase { get; set; }
        public string PdfPath { get; set; }
        public string FileListDocument { get; set; }
        public string FileNotFindDocPath { get; set; }
        public string FileFindDocPath { get; set; }
    }
}
