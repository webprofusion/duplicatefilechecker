using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuplicateFileChecker
{
    public enum DuplicateStatusType
    {
        None,
        SameSizeAndName,
        AppearsIdentical,
        ContentAppearsIdentical
    }
    
    public class FileEntry
    {
        public string FileName { get; set; }
        public DateTime DateModified { get; set; }
        public string Path { get; set; }
        public long SizeBytes { get; set; }
        public string FileHash { get; set; }
    }

    public class DuplicateEntry
    {
        public FileEntry FileEntry1 { get; set; }
        public FileEntry FileEntry2 { get; set; }
        public DuplicateStatusType DuplicateStatus { get; set; }
    }
}
