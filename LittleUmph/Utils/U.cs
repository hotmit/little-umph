using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Management;

namespace LittleUmph
{
    #region [ Enum Declaration ]
    

    /// <summary>
    /// Time Format
    /// </summary>
    public enum TimeFormatType
    {
        /// <summary>
        /// eg. 14:00
        /// </summary>
        Time24HoursNoSecond,

        /// <summary>
        /// eg. 16:45:32
        /// </summary>
        Time24HoursWithSecond,

        /// <summary>
        /// eg. 23:02:34.953
        /// </summary>
        Time24HoursWithmillisecond,

        /// <summary>
        /// eg. 04/25/2009 23:02:34.953
        /// </summary>
        Date24HoursAndmillisecond
    }

    /// <summary>
    /// Major : Assemblies with the same name but different major versions are not interchangeable. This would be appropriate, for example, for a major rewrite of a product where backward compatibility cannot be assumed.
    /// Minor : If the name and major number on two assemblies are the same, but the minor number is different, this indicates significant enhancement with the intention of backward compatibility. This would be appropriate, for example, on a point release of a product or a fully backward compatible new version of a product.
    /// Build : A difference in build number represents a recompilation of the same source. This would be appropriate because of processor, platform, or compiler changes.
    /// Revision : Assemblies with the same name, major, and minor version numbers but different revisions are intended to be fully interchangeable. This would be appropriate to fix a security hole in a previously released assembly. 
    /// </summary>
    public enum VersionFormat
    {
        /// <summary>
        /// eg. 1.0
        /// </summary>
        MajorMinor,

        /// <summary>
        /// eg. 1.0.17
        /// </summary>
        MajorMinorBuild,

        /// <summary>
        /// eg. 1.7.2
        /// </summary>
        MajorMinorRevision,
        
        /// <summary>
        /// eg. 1.3.2.7
        /// </summary>
        MajorMinorBuildRevision,
    }
    #endregion

    /// <summary>
    /// Sets of function to help with reqular programming
    /// </summary>
    public class U
    {
        #region [ App Version String ]
        /// <summary>
        /// Gets the application version string.
        /// </summary>
        /// <returns>" v[Major.Minor.Revision]. Example: v1.8.2</returns>
        public static string GetAppVersion()
        {
            return GetAppVersion(" v", VersionFormat.MajorMinorRevision);
        }

        /// <summary>
        /// Gets the application version string.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string GetAppVersion(string prefix, VersionFormat format)
        {
            return prefix + GetAppVersion(format);
        }

        /// <summary>
        /// Gets the application version string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string GetAppVersion(VersionFormat format)
        {
            Version appVer = new Version(Application.ProductVersion);

            switch (format)
            {
                case VersionFormat.MajorMinor:
                    return String.Format("{0}.{1}", appVer.Major, appVer.Minor);
                case VersionFormat.MajorMinorBuild:
                    return String.Format("{0}.{1}.{2}", appVer.Major, appVer.Minor, appVer.Build);
                case VersionFormat.MajorMinorRevision:
                    return String.Format("{0}.{1}.{2}", appVer.Major, appVer.Minor, appVer.Revision);
                case VersionFormat.MajorMinorBuildRevision:
                    return String.Format("{0}.{1}.{2}.{3}", appVer.Major, appVer.Minor, appVer.Build, appVer.Revision);
            }

            return appVer.ToString();
        }
        #endregion

        #region [ Coalesce ]
        /// <summary>
        /// Return the first NON-null value in the parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static T Coalesce<T>(params T[] arguments)
        {
            for (int i = 0, c = arguments.Length; i < c; i++)
            {
                if (arguments[i] != null)
                {
                    return arguments[i];
                }
            }
            // Return null for ref type and return default value for value type 
            return default(T);
        }
        #endregion
    }
}
