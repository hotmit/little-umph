using System;
using System.Collections.Generic;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Text;
using System.Collections;
using System.IO;

namespace LittleUmph
{
    /// <summary>
    /// Natural Sort betwwen two string.
    /// </summary>
    public class NaturalSortComparer : IComparer    
    {
        public bool Reverse { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalSortComparer"/> class.
        /// </summary>
        public NaturalSortComparer() : this(false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalSortComparer"/> class.
        /// </summary>
        /// <param name="reverse">If set to <c>true</c> then negate the 
        /// order of the comparision. Use this to change the direction 
        /// of the sort.</param>
        public NaturalSortComparer(bool reverse)
        {
            Reverse = reverse;
        }


        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(object x, object y)
        {
            if (!Reverse)
            {
                return PInvoke.StrCmpLogicalW(x.ToString(), y.ToString());
            }

            int result = PInvoke.StrCmpLogicalW(x.ToString(), y.ToString());
            if (result == 0)
            {
                return 0;
            }

            // Reverse the direction
            return result > 0 ? -1 : 1;
        }
    }

    #region [ DirectoryInfo, FileInfo Comparer ]
    /// <summary>
    /// Comparer for FileInfo
    /// </summary>
    public class FileInfoNaturalSortComparer : NaturalSortComparer, IComparer<FileInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoNaturalSortComparer"/> class.
        /// </summary>
        public FileInfoNaturalSortComparer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoNaturalSortComparer"/> class.
        /// </summary>
        /// <param name="reverse">if set to <c>true</c> [reverse].</param>
        public FileInfoNaturalSortComparer(bool reverse)
            : base(reverse)
        {

        }

        /// <summary>
        /// Compares the specified savedX.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(FileInfo x, FileInfo y)
        {
            int result = base.Compare(x.Directory.FullName, x.Directory.FullName);

            if (result == 0)
            {
                return base.Compare((object)x.Name, (object)y.Name);
            }
            return result;
        }
    }

    /// <summary>
    /// Comparer for DirectoryInfo
    /// </summary>
    public class DirectoryInfoNaturalSortComparer : NaturalSortComparer, IComparer<DirectoryInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryInfoNaturalSortComparer"/> class.
        /// </summary>
        public DirectoryInfoNaturalSortComparer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryInfoNaturalSortComparer"/> class.
        /// </summary>
        /// <param name="reverse">if set to <c>true</c> [reverse].</param>
        public DirectoryInfoNaturalSortComparer(bool reverse)
            : base(reverse)
        {

        }

        /// <summary>
        /// Compares the specified savedX.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(DirectoryInfo x, DirectoryInfo y)
        {
            int result = base.Compare(x.Parent.FullName, x.Parent.FullName);

            if (result == 0)
            {
                return base.Compare((object)x.Name, (object)y.Name);
            }
            return result;
        }
    }
    #endregion
}
