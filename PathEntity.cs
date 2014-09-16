using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebFolderSync
{
    class PathEntity
    {
        public string FullPath { get; set; }
        public string ComparePath { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int Count { get; set; }

        public PathEntity()
        {
            FullPath = string.Empty;
            ComparePath = string.Empty;
            ModifiedDate = DateTime.MinValue;
            Count = 0;
        }

        public PathEntity(string fullPath, string comparePath, DateTime modifiedDate, int count)
        {
            FullPath = fullPath;
            ComparePath = comparePath;
            ModifiedDate = modifiedDate;
            Count = count;
        }

        public PathEntity(string fullPath, string comparePath, DateTime modifiedDate)
        {
            FullPath = fullPath;
            ComparePath = comparePath;
            ModifiedDate = modifiedDate;
            Count = 0;
        }

    }
}
