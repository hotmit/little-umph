using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing.Printing;

// Last Changed: Oct. 1/2009
namespace LittleUmph
{
    #region [ Exceptions ]
    /// <summary>
    /// Invalid Template Data Format Exception
    /// </summary>
    public class InvalidTemplateDataException : Exception
    {
        /// <summary>
        /// Invalid Template Data Format Exception
        /// </summary>
        /// <param name="message"></param>
        public InvalidTemplateDataException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// When the printer is not found
    /// </summary>
    public class PrinterNotFoundException : Exception
    {
        /// <summary>
        /// When the printer is not found
        /// </summary>
        /// <param name="message">The message.</param>
        public PrinterNotFoundException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception while printing the document
    /// </summary>
    public class PrintingException : Exception
    {
        /// <summary>
        /// Exception while printing the document
        /// </summary>
        /// <param name="message">The message.</param>
        public PrintingException(string message)
            : base(message)
        {
        }
    }
    #endregion

    /// <summary>
    /// Option for paragraph/multiline template fields
    /// </summary>    
    public enum PrinterMultilineFormat
    {
        /// <summary>
        /// Relace newline with a space
        /// </summary>
        NoNewLine = 0,

        /// <summary>
        /// Only keep one of the newline if found multiple consecutive newline
        /// </summary>
        SingleNewlineOnly = 1,

        /// <summary>
        /// Preserve the newline chracter in mutiline field
        /// </summary>
        PreserveNewline = 2
    }

    /// <summary>
    /// Print using preformated printer code (PRN Files)
    /// </summary>
    public class PrnPrint
    {
        #region [ Private Variables ]
        [DllImport("winspool.Drv", EntryPoint = "GetDefaultPrinter")]
        private static extern bool GetDefaultPrinter(
            StringBuilder pszBuffer,   // printer name buffer
            ref int pcchBuffer  // size of name buffer
        );

        private string _printerName = "";
        private PrinterMultilineFormat _multilineTreatment = PrinterMultilineFormat.SingleNewlineOnly;
        private bool _throwExceptions = false;
        private string _lastErrorMessage = string.Empty;
        private bool _hasError = false;
        private bool _writeOutputToDesktop;
        private bool _displayDebugMessages = false;  
        #endregion

        #region [ Properties ]
        /// <summary>
        /// The printer name
        /// </summary>
        public string PrinterName
        {
            get { return _printerName; }
            set { _printerName = value; }
        }

        /// <summary>
        /// What to do with multiline in the "Wrap" template field
        /// </summary>
        public PrinterMultilineFormat MultilineTreatment
        {
            get { return _multilineTreatment; }
            set { _multilineTreatment = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to throw exceptions (default to false).
        /// </summary>
        /// <value><c>true</c> if [throw exceptions]; otherwise, <c>false</c>.</value>
        public bool ThrowExceptions
        {
            get { return _throwExceptions; }
            set { _throwExceptions = value; }
        }

        /// <summary>
        /// Gets or sets the last error message (empty if no error occured).
        /// </summary>
        /// <value>The last error.</value>
        public string LastErrorMessage
        {
            get { return _lastErrorMessage; }
            set
            {
                if (value.Trim().Length == 0)
                {
                    _lastErrorMessage = "";
                    _hasError = false;
                }
                else
                {
                    _lastErrorMessage = "Print Error: " + value;
                    _hasError = true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has error.
        /// </summary>
        /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
        public bool HasError
        {
            get { return _hasError; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether write output to desktop.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if write output to desktop; otherwise, <c>false</c>.
        /// </value>
        public bool WriteOutputToDesktop
        {
            get { return _writeOutputToDesktop; }
            set { _writeOutputToDesktop = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display debug messages.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if display debug messages; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayDebugMessages
        {
            get { return _displayDebugMessages; }
            set { _displayDebugMessages = value; }
        }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Create new instance using the default printer name
        /// </summary>
        public PrnPrint()
            : this(GetDefaultPrinter())
        {
        }

        /// <summary>
        /// Create new instance using the specified printer name 
        /// (do not throw exception and treat newline with SingleNewlineOnly)
        /// </summary>
        /// <param name="printerName">Name of the printer.</param>
        public PrnPrint(string printerName)
            : this(printerName, false, PrinterMultilineFormat.SingleNewlineOnly)
        {
        }

        /// <summary>
        /// Create new instance using the specified printer name
        /// </summary>
        /// <param name="printerName">Name of the printer.</param>
        /// <param name="throwException">if set to <c>true</c> then exception will be thrown.</param>
        /// <param name="multiLineTreatment">The multi line treatment.</param>
        public PrnPrint(string printerName, bool throwException, PrinterMultilineFormat multiLineTreatment)
        {
            LastErrorMessage = "";
            PrinterName = printerName;
            ThrowExceptions = throwException;
            MultilineTreatment = multiLineTreatment;
        }
        #endregion

        #region [ Print Using Template ]
        /// <summary>
        /// Print label using the string from the .prn template
        /// </summary>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="content">The .prn label dir's content</param>
        /// <param name="data">Pairing of name and data (Format: "$Name", "John McDougle", "Center12$Header", "Machine", "$Sex:N/A", sex, "Wrap14$Paragraph", "long text" etc.)</param>
        /// <returns></returns>
        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
        /// <example>p.PrintFile(@"D:\Desktop\Label.prn", "$Title", "Work Order", "$BarCode", "4279");</example>
        public bool PrintString(string documentName, int quantity, string content, params string[] data)
        {
            #region [ Error Checking ]
            if (data.Length % 2 != 0)
            {
                LastErrorMessage = "Number of fields and number of data must match. Example \"$Title\", \"Me Title\", \"$BarCode\", \"4279\"";

                // This is development error, this class will throw the exception 
                // regardless of the value of property "ThrowExceptions".
                throw new InvalidTemplateDataException(LastErrorMessage);
            }
            #endregion

            if (quantity > 1)
            {
                #region [ Citizen Printers: More Efficient "Print Quantity" Logic ]
                // Citizen Printer Code for "Print Quantity"
                Match m = Regex.Match(content, @"(?<Header>\s+?Q)(?<Quantity>\d+)(?<Footer>\s+?E)");
                if (m.Success)
                {
                    int embededLabelQty = Convert.ToInt32(m.Groups["Quantity"].Value);
                    string qtyString = string.Format("{0}{1:0000}{2}", m.Groups["Header"].Value,
                                                        embededLabelQty * quantity,
                                                        m.Groups["Footer"].Value);
                    content = content.Replace(m.Value, qtyString);

                    // Since we already handled the print quantity inside the printer code
                    // we do NOT need to handle it with the driver anymore.
                    quantity = 1;
                }
                #endregion

                #region [ Zebra Printers: More Efficient "Print Quantity" Logic ]
                else
                {
                    m = Regex.Match(content, @"(?<Header>\^PQ)(?<Quantity>\d+)(?<Footer>[,\s]|$)");
                    if (m.Success)
                    {
                        int embededLabelQty = Convert.ToInt32(m.Groups["Quantity"].Value);
                        string qtyString = string.Format("{0}{1}{2}", m.Groups["Header"].Value,
                                                            embededLabelQty * quantity,
                                                            m.Groups["Footer"].Value);
                        content = content.Replace(m.Value, qtyString);                       

                        // Since we already handled the print quantity inside the printer code
                        // we do NOT need to handle it with the driver anymore.
                        quantity = 1;
                    }
                }
                #endregion
            }

            #region [ Replace Template Fields ]
            for (int i = 0, c = data.Length; i < c; i = i + 2)
            {
                string inputName = data[i] ?? "";
                string defaultValue = "";
                string varName;

                #region [ Default Value Replacement: $FieldName:DefaultValue ]
                if (inputName.Contains(":"))
                {
                    Match defaultValueMatch = Regex.Match(inputName, "(.+):(.+)");
                    if (defaultValueMatch.Success)
                    {
                        inputName = defaultValueMatch.Groups[1].Value;
                        defaultValue = defaultValueMatch.Groups[2].Value;
                    }
                }
                #endregion

                string inputData = (data[i + 1] == null || data[i + 1].Trim().Length == 0) ? defaultValue : data[i + 1];


                #region [ Wrap Treatment: Wrap###$FieldName ]
                if (inputName.ToUpper().Contains("WRAP"))
                {
                    Match m = Regex.Match(inputName, @"Wrap(\d+)\$(.+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        int wrapCount = Convert.ToInt32(m.Groups[1].Value);
                        string fieldName = m.Groups[2].Value;
                        inputData = inputData.Replace("\r\n", "\n");

                        if (MultilineTreatment == PrinterMultilineFormat.NoNewLine)
                        {
                            inputData = inputData.Replace("\n", " ");
                        }
                        else if (MultilineTreatment == PrinterMultilineFormat.SingleNewlineOnly)
                        {
                            inputData = inputData.Replace("\n\n", "\n");
                            inputData = inputData.Replace("\n\n", "\n");
                            inputData = inputData.Replace("\n\n", "\n");
                        }
                        else if (MultilineTreatment == PrinterMultilineFormat.PreserveNewline)
                        {
                            // Do nothing   
                        }

                        List<string> lines = Str.WordWrapLine(inputData, wrapCount);

                        for (int j = 0; j < 100; j++)
                        {
                            varName = string.Format("${0}{1}", fieldName, j + 1);
                            string fieldValue = "";
                            fieldValue = j < lines.Count ? lines[j] : " ";

                            if (!content.Contains(varName))
                            {
                                break;
                            }
                            content = content.Replace(varName, fieldValue);
                        }
                    }
                }
                #endregion

                #region [ Center String: Center###$FieldName ]
                else if (inputName.ToUpper().Contains("CENTER"))
                {
                    Match m = Regex.Match(inputName, @"Center(\d+)\$(.+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        // Remove new line from the data, cuz newline will break the prn format
                        inputData = inputData.Replace("\r\n", " ");
                        inputData = inputData.Replace("\n", " ");

                        int totalLength = Convert.ToInt32(m.Groups[1].Value);
                        varName = "$" + m.Groups[2].Value;
                        string fieldComputedValue = "";
                        int padLeft = 0;

                        // Only pad if the input length is less than the total length
                        if (inputData.Length < totalLength)
                        {
                            padLeft = Convert.ToInt32(Math.Floor((totalLength - inputData.Length) / 2f));
                            padLeft = compensatePadding(padLeft, inputData);

                            if (padLeft + inputData.Length > totalLength)
                            {
                                padLeft = totalLength - inputData.Length;
                            }
                        }

                        // Total size is less than the actual string length,
                        // then there is no need to pad
                        if (padLeft <= 0)
                        {
                            fieldComputedValue = inputData;
                        }
                        else
                        {
                            // Put space infront of the string to center the string in the field
                            fieldComputedValue = new string(' ', padLeft) + inputData;
                        }

                        if (DisplayDebugMessages)
                        {
                            MessageBox.Show(string.Format("Field: {0}, Total: {1}, Input: {2}, Pad: {3}, Result: '{4}'", m.Value, totalLength, inputData.Length, padLeft, fieldComputedValue));
                        }
                        content = content.Replace(varName, fieldComputedValue);
                    }
                }
                #endregion

                #region [ Straight Replacement ]
                else
                {
                    // Remove new line from the data, cuz newline will break the prn format
                    inputData = inputData.Replace("\r\n", " ");
                    inputData = inputData.Replace("\n", " ");

                    varName = inputName;
                    content = content.Replace(varName, inputData);
                }
                #endregion
            }
            #endregion

            if (WriteOutputToDesktop)
            {
                IOFunc.WriteToDesktop("LabelOutput.txt", content, false);
            }

            byte[] byteContent = Encoding.Default.GetBytes(content);
            return PrintBytes(documentName, quantity, byteContent);
        }

        /// <summary>
        /// Print label using the .prn template
        /// </summary>
        /// <param name="documentName">Name of the document appear on the printer spool.</param>
        /// <param name="quantity">Number of copies.</param>
        /// <param name="prnPath">The .PRN dir path.</param>
        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
        /// <returns>True if no error occured, false otherwise.</returns>
        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
        public bool PrintFile(string documentName, int quantity, string prnPath, params string[] data)
        {
            LastErrorMessage = "";
            if (!File.Exists(prnPath))
            {
                if (!ThrowExceptions)
                {
                    LastErrorMessage = "Label file not found";
                    return false;
                }
            }
            string content = File.ReadAllText(prnPath, Encoding.Default);
            return PrintString(documentName, quantity, content, data);
        }
        #endregion

        #region [ Print Bytes ]
        /// <summary>
        /// Prints the bytes.
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public bool PrintBytes(string documentName, int quantity, byte[] content)
        {
            LastErrorMessage = "";

            if (quantity > 0)
            {
                List<byte> multipleCopies = new List<byte>(content.Length * quantity);
                for (int i = 0; i < quantity; i++)
                {
                    multipleCopies.AddRange(content);
                }
                content = multipleCopies.ToArray();
            }

            IntPtr ptr = Marshal.AllocHGlobal(content.Length);
            try
            {
                Marshal.Copy(content, 0, ptr, content.Length);
                return SendBytesToPrinter(ptr, content.Length, documentName);
            }
            catch
            {
                if (ThrowExceptions)
                {
                    throw;
                }
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        #endregion

        #region [ Assist Functions ]
        /// <summary>
        /// Get the default printer's name
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultPrinter()
        {
            try
            {
                StringBuilder name = new StringBuilder(256);
                int length = 256;
                GetDefaultPrinter(name, ref length);
                return name.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// Compensates the padding to help centering the string more accurately. 
        /// Non alpha char take up less space, therefore need more padding to make it center.
        /// </summary>
        /// <param name="pad">The calculated padding.</param>
        /// <param name="text">The text that need centering</param>
        /// <returns>The more accurate padding value</returns>
        private static int compensatePadding(int pad, string text)
        {
            const string tinyChars = ":;!.,`|il";
            const string littleChars = "[]{}()1";
            const string normalChars = "@#$%&*-+=<>?\"";
            const string bigChars = "wWmM";

            double extraPadding = 0;
            for (int i = 0; i < text.Length; i++)
            {
                string curChar = text[i].ToString();

                if (bigChars.Contains(curChar))
                {
                    extraPadding--;
                }
                else if (normalChars.Contains(curChar))
                {
                    continue;
                }
                else if (littleChars.Contains(curChar))
                {
                    // [1.2]    to ensure one more extra space if contains more than 3 chars (3x1.2 => 3.6 => 4)
                    extraPadding += 1.2;
                }
                else if (tinyChars.Contains(curChar))
                {
                    // [1.51]   2x1.51 => 3.2 => 3
                    extraPadding += 1.51;
                }
            }

            int compensation = Convert.ToInt32(Math.Round(extraPadding));
            return pad + compensation;
        }
        #endregion

        #region [ Print Raw Data ]
        /// <summary>
        /// Print from a dir
        /// </summary>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="filepath">The filepath.</param>
        /// <returns></returns>
        public bool PrintRawFile(string documentName, int quantity, string filepath)
        {
            LastErrorMessage = "";
            if (!File.Exists(filepath))
            {
                if (!ThrowExceptions)
                {
                    LastErrorMessage = "Label file not found";
                    return false;
                }
            }

            byte[] content = File.ReadAllBytes(filepath);
            return PrintBytes(documentName, quantity, content);
        }

        /// <summary>
        /// Print from a string
        /// </summary>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="content">The .prn label content.</param>
        /// <returns></returns>
        public bool PrintRawString(string documentName, int quantity, string content)
        {
            byte[] bContent = Encoding.Default.GetBytes(content);
            return PrintBytes(documentName, quantity, bContent);
        }
        #endregion

        #region [ Copied From RawPrinterHelper Class In DDEngine.Lib ]
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        /// <summary>
        /// SendBytesToPrinter()
        /// When the function is given a printer name and an unmanaged array
        /// of bytes, the function sends those bytes to the printer queue.
        /// </summary>
        /// <param name="pBytes">The bytes array.</param>
        /// <param name="dwCount">The array length.</param>
        /// <param name="documentName">Document name that will appear on the printer pool list.</param>
        /// <returns></returns>
        private bool SendBytesToPrinter(IntPtr pBytes, Int32 dwCount, string documentName)
        {
            LastErrorMessage = "";

            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter;
            DOCINFOA di = new DOCINFOA();
            bool success = false; // Assume failure unless you specifically succeed.

            di.pDocName = Str.IsEmpty(documentName) ? "Label Printer" : documentName;
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(PrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        success = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    else
                    {
                        LastErrorMessage = "Cannot start the print page.";
                        if (ThrowExceptions)
                        {
                            throw new PrintingException(LastErrorMessage);
                        }
                    }
                    EndDocPrinter(hPrinter);
                }
                else
                {
                    LastErrorMessage = "Cannot start the print document.";
                    if (ThrowExceptions)
                    {
                        throw new PrintingException(LastErrorMessage);
                    }
                }
                ClosePrinter(hPrinter);
            }
            else
            {
                LastErrorMessage = "Cannot open printer '" + PrinterName + "'.";
                if (ThrowExceptions)
                {
                    throw new PrinterNotFoundException(LastErrorMessage);
                }
            }

            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (success == false)
            {
                dwError = Marshal.GetLastWin32Error();
                if (LastErrorMessage.Trim().Length == 0)
                {
                    LastErrorMessage = "Error occured while printing [Error Code: " + dwError + "].";
                }

                if (ThrowExceptions)
                {
                    throw new PrinterNotFoundException(LastErrorMessage);
                }
            }
            else
            {
                // No errors
                LastErrorMessage = "";
            }

            return success;
        }
        #endregion



        #region [ iPrinter .Net Only Function ]
        /// <summary>
        /// Print label using the .prn template
        /// </summary>
        /// <param name="documentName">Name of the document appear on the printer spool.</param>
        /// <param name="quantity">Number of copies.</param>
        /// <param name="prnPath">The .PRN dir path.</param>
        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
        /// <returns>True if no error occured, false otherwise.</returns>
        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
        public bool PrintFile(string documentName, int quantity, string prnPath, Dictionary<string, string> data)
        {
            string[] arrData = new string[data.Count * 2];
            int index = 0;
            foreach (KeyValuePair<string, string> pair in data)
            {
                arrData[index] = pair.Key;
                index++;
                arrData[index] = pair.Value;
                index++;
            }
            return PrintFile(documentName, quantity, prnPath, arrData);
        }

        /// <summary>
        /// Print label using the string from the .prn template
        /// </summary>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="content">The .prn label dir's content</param>
        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
        /// <returns></returns>
        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
        public bool PrintString(string documentName, int quantity, string content, Dictionary<string, string> data)
        {
            string[] arrData = new string[data.Count * 2];
            int index = 0;
            foreach (KeyValuePair<string, string> pair in data)
            {
                arrData[index] = pair.Key;
                index++;
                arrData[index] = pair.Value;
                index++;
            }
            return PrintString(documentName, quantity, content, arrData);
        }

        /// <summary>
        /// Gets the printer list.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPrinterList()
        {
            List<string> printList = new List<string>();
            foreach (string strPrinter in PrinterSettings.InstalledPrinters)
            {
                printList.Add(strPrinter);
            }
            printList.Sort();
            return printList;
        }
        #endregion
    }
}
























//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using System.Runtime.InteropServices;
//using DDEngine.Debug;
//using System.IO;
//using System.Text.RegularExpressions;
//using DDEngine.Util;

//// Last Changed: Jan 30/2009
//namespace DDEngine.IO
//{
//    #region [ Exceptions ]
//    /// <summary>
//    /// Invalid Template Data Format Exception
//    /// </summary>
//    public class InvalidTemplateDataException : Exception
//    {
//        /// <summary>
//        /// Invalid Template Data Format Exception
//        /// </summary>
//        /// <param name="message"></param>
//        public InvalidTemplateDataException(string message)
//            : base(message)
//        {
//        }
//    }

//    /// <summary>
//    /// When the printer is not found
//    /// </summary>
//    public class PrinterNotFoundException : Exception
//    {
//        /// <summary>
//        /// When the printer is not found
//        /// </summary>
//        /// <param name="message">The message.</param>
//        public PrinterNotFoundException(string message)
//            : base(message)
//        {
//        }
//    } 

//    /// <summary>
//    /// Exception while printing the document
//    /// </summary>
//    public class PrintingException : Exception
//    {
//        /// <summary>
//        /// Exception while printing the document
//        /// </summary>
//        /// <param name="message">The message.</param>
//        public PrintingException(string message)
//            : base(message)
//        {
//        }
//    } 
//    #endregion

//    #region [ Enum ]
//    /// <summary>
//    /// Option for paragraph/multiline template fields
//    /// </summary>    
//    public enum iPrinterMultilineFormat
//    {
//        /// <summary>
//        /// Relace newline with a space
//        /// </summary>
//        NoNewLine = 0,

//        /// <summary>
//        /// Only keep one of the newline if found multiple consecutive newline
//        /// </summary>
//        SingleNewlineOnly = 1,

//        /// <summary>
//        /// Preserve the newline chracter in mutiline field
//        /// </summary>
//        PreserveNewline = 2
//    } 
//    #endregion

//    /// <summary>
//    /// Print using preformated printer code (PRN Files)
//    /// </summary>
//    public class iPrinter
//    {
//        #region [ Private Variables ]
//        [DllImport("winspool.Drv", EntryPoint = "GetDefaultPrinter")]
//        private static extern bool GetDefaultPrinter(
//            StringBuilder pszBuffer,   // printer name buffer
//            ref int pcchBuffer  // size of name buffer
//        );

//        private string _printerName = "";
//        private iPrinterMultilineFormat _multilineTreatment = iPrinterMultilineFormat.SingleNewlineOnly;
//        private bool _throwExceptions = false;
//        private string _lastErrorMessage = string.Empty;
//        private bool _hasError = false;
//        #endregion

//        #region [ Properties ]
//        /// <summary>
//        /// The printer name
//        /// </summary>
//        public string PrinterName
//        {
//            get { return _printerName; }
//            set { _printerName = value; }
//        }

//        /// <summary>
//        /// What to do with multiline in the "Wrap" template field
//        /// </summary>
//        public iPrinterMultilineFormat MultilineTreatment
//        {
//            get { return _multilineTreatment; }
//            set { _multilineTreatment = value; }
//        }

//        /// <summary>
//        /// Gets or sets a value indicating whether to throw exceptions (default to false).
//        /// </summary>
//        /// <value><c>true</c> if [throw exceptions]; otherwise, <c>false</c>.</value>
//        public bool ThrowExceptions
//        {
//            get { return _throwExceptions; }
//            set { _throwExceptions = value; }
//        }

//        /// <summary>
//        /// Gets or sets the last error message (empty if no error occured).
//        /// </summary>
//        /// <value>The last error.</value>
//        public string LastErrorMessage
//        {
//            get { return _lastErrorMessage; }
//            set
//            {
//                if (value.Trim().Length == 0)
//                {
//                    _lastErrorMessage = "";
//                    _hasError = false;
//                }
//                else
//                {
//                    _lastErrorMessage = "Print Error: " + value;
//                    _hasError = true;
//                }
//            }
//        }

//        /// <summary>
//        /// Gets a value indicating whether this instance has error.
//        /// </summary>
//        /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
//        public bool HasError
//        {
//            get { return _hasError; }
//        }
//        #endregion

//        #region [ Constructors ]
//        /// <summary>
//        /// Create new instance using the default printer name
//        /// </summary>
//        public iPrinter(): this (GetDefaultPrinter())
//        {            
//        }

//        /// <summary>
//        /// Create new instance using the specified printer name 
//        /// (do not throw exception and treat newline with SingleNewlineOnly)
//        /// </summary>
//        /// <param name="printerName">Name of the printer.</param>
//        public iPrinter(string printerName)
//            : this(printerName, false, iPrinterMultilineFormat.SingleNewlineOnly)
//        {
//        }

//        /// <summary>
//        /// Create new instance using the specified printer name
//        /// </summary>
//        /// <param name="printerName">Name of the printer.</param>
//        /// <param name="throwException">if set to <c>true</c> then exception will be thrown.</param>
//        /// <param name="multiLineTreatment">The multi line treatment.</param>
//        public iPrinter(string printerName, bool throwException, iPrinterMultilineFormat multiLineTreatment)
//        {
//            LastErrorMessage = "";
//            PrinterName = printerName;
//            ThrowExceptions = throwException;
//            MultilineTreatment = multiLineTreatment;
//        }
//        #endregion

//        #region [ Print File Using Template ]
//        /// <summary>
//        /// Print label using the .prn template
//        /// </summary>
//        /// <param name="filepath">The filepath.</param>
//        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
//        /// <returns>
//        /// True if no error occured, false otherwise.
//        /// </returns>
//        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
//        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
//        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
//        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
//        public bool PrintFile(string filepath, params string[] data)
//        {
//            int quantity = 1;
//            return PrintFile(quantity, filepath, data);
//        }

//        /// <summary>
//        /// Print label using the .prn template
//        /// </summary>
//        /// <param name="quantity">Number of copies.</param>
//        /// <param name="filepath">The filepath.</param>
//        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
//        /// <returns>
//        /// True if no error occured, false otherwise.
//        /// </returns>
//        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
//        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
//        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
//        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
//        public bool PrintFile(int quantity, string filepath, params string[] data)
//        {
//            string documentName = String.Empty;
//            return PrintFile(documentName, quantity, filepath, data);
//        }

//        /// <summary>
//        /// Print label using the .prn template
//        /// </summary>
//        /// <param name="documentName">Name of the document appear on the printer spool.</param>
//        /// <param name="quantity">Number of copies.</param>
//        /// <param name="prnPath">The .PRN dir path.</param>
//        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
//        /// <returns>True if no error occured, false otherwise.</returns>
//        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
//        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
//        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
//        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
//        public bool PrintFile(string documentName, int quantity, string prnPath, params string[] data)
//        {
//            LastErrorMessage = "";
//            if (!File.Exists(prnPath))
//            {
//                if (!ThrowExceptions)
//                {
//                    LastErrorMessage = "Label dir not found";
//                    return false;
//                }
//            }
//            string content = File.ReadAllText(prnPath, Encoding.Default);
//            return PrintString(documentName, quantity, content, data);
//        }


//        #endregion

//        #region [ Print String Using Template ]
//        /// <summary>
//        /// Print label using the string from the .prn template
//        /// </summary>
//        /// <param name="content">The .prn label dir's content</param>
//        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
//        /// <returns></returns>
//        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
//        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
//        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
//        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
//        public bool PrintString(string content, params string[] data)
//        {
//            int quantity = 1;
//            return PrintString(quantity, content, data);
//        }

//        /// <summary>
//        /// Print label using the string from the .prn template
//        /// </summary>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="content">The .prn label dir's content</param>
//        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
//        /// <returns></returns>
//        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
//        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
//        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
//        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
//        public bool PrintString(int quantity, string content, params string[] data)
//        {
//            string documentName = String.Empty;
//            return PrintString(documentName, quantity, content, data);
//        }

//        /// <summary>
//        /// Print label using the string from the .prn template
//        /// </summary>
//        /// <param name="documentName">Name of the document.</param>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="content">The .prn label dir's content</param>
//        /// <param name="data">Pairing of name and data (Format: "$name1", "v1", "$name2", "v2", "$name3", "v3", "Wrap14$Paragraph", "long text" etc.)</param>
//        /// <returns></returns>
//        /// <remarks>Template's field name must start with a dolar sign follow by a name of the field (ex. "$City").
//        /// To Wrap text, label the template field's name follow by 1-index number (ex. $Note1, $Note2, $Note3 for 3 lines).
//        /// When call specify the field name as "Wrap20$Note" (without the index)</remarks>
//        /// <example>p.PrintFile(@"D:\Desktop\Hung Kaboom.prn", "$Title", "Me Title", "$BarCode", "4279");</example>
//        public bool PrintString(string documentName, int quantity, string content, params string[] data)
//        {
//            if (data.Length % 2 != 0)
//            {
//                LastErrorMessage = "Number of fields and number of data must match. Example \"$Title\", \"Me Title\", \"$BarCode\", \"4279\"";

//                // This is development error, it will throw the exception 
//                // regardless of the property "ThrowExceptions" value
//                throw new InvalidTemplateDataException(LastErrorMessage);
//            }

//            #region [ Replace Template Fields ]
//            string varName, defaultValue;
//            for (int i = 0, c = data.Length; i < c; i = i + 2)
//            {
//                string inputName = MagicU.IsEmpty(data[i]) ? "" : data[i];
//                defaultValue = "";
//                if (inputName.Contains(":"))
//                {
//                    Match defaultValueMatch = Regex.Match(inputName, "(.+):(.+)");
//                    if (defaultValueMatch.Success)
//                    {
//                        inputName = defaultValueMatch.Groups[1].Value;
//                        defaultValue = defaultValueMatch.Groups[2].Value;
//                    }
//                }
//                string inputData = MagicU.IsEmpty(data[i + 1]) ? defaultValue : data[i + 1];

//                if (inputName.ToUpper().Contains("WRAP"))
//                {
//                    #region [ Wrap Treatment ]
//                    Match m = Regex.Match(inputName, @"Wrap(\d+)\$(.+)", RegexOptions.IgnoreCase);
//                    if (m.Success)
//                    {
//                        int wrapCount = Convert.ToInt32(m.Groups[1].Value);
//                        string fieldName = m.Groups[2].Value;
//                        string fieldValue = "";
//                        inputData = inputData.Replace("\r\n", "\n");

//                        if (MultilineTreatment == iPrinterMultilineFormat.NoNewLine)
//                        {
//                            inputData = inputData.Replace("\n", " ");
//                        }
//                        else if (MultilineTreatment == iPrinterMultilineFormat.SingleNewlineOnly)
//                        {
//                            inputData = inputData.Replace("\n\n", "\n");
//                            inputData = inputData.Replace("\n\n", "\n");
//                            inputData = inputData.Replace("\n\n", "\n");
//                        }
//                        else if (MultilineTreatment == iPrinterMultilineFormat.PreserveNewline)
//                        {
//                            // Do nothing   
//                        }

//                        string[] lines = MagicU.WordWrapLine(inputData, wrapCount);

//                        for (int j = 0; j < 20; j++)
//                        {
//                            varName = string.Format("${0}{1}", fieldName, j + 1);
//                            if (j < lines.Length)
//                            {
//                                fieldValue = lines[j];
//                            }
//                            else
//                            {
//                                fieldValue = " ";
//                            }
//                            content = content.Replace(varName, fieldValue);
//                        }
//                    }
//                    #endregion
//                }
//                else if (inputName.ToUpper().Contains("CENTER"))
//                {
//                    #region [ CenterControls String ]
//                    Match m = Regex.Match(inputName, @"CenterControls(\d+)\$(.+)", RegexOptions.IgnoreCase);
//                    if (m.Success)
//                    {
//                        // Remove new line from the data, cuz newline will break the prn format
//                        inputData = inputData.Replace("\r\n", " ");
//                        inputData = inputData.Replace("\n", " ");

//                        int totalLength = Convert.ToInt32(m.Groups[1].Value);
//                        varName = "$" + m.Groups[2].Value;
//                        string fieldComputedValue = "";

//                        int padLeft = Convert.ToInt32(Math.Floor((double)((totalLength - inputData.Length) / 2)));
//                        int padRight = totalLength - padLeft - inputData.Length;

//                        padLeft = compensatePadding(padLeft, inputData);

//                        // Total size is less than the actual string length,
//                        // then there is no need to pad
//                        if (padLeft <= 0)
//                        {
//                            fieldComputedValue = inputData;
//                        }
//                        else
//                        {
//                            // Put space infront of the string to center the string in the field
//                            fieldComputedValue = inputData.PadLeft(padLeft, ' ');
//                            //if (padRight > 0)
//                            //{
//                            //    fieldComputedValue = fieldComputedValue.PadRight(padRight, ' ');
//                            //}
//                        }

//                        content = content.Replace(varName, fieldComputedValue);
//                    }
//                    #endregion
//                }
//                else
//                {
//                    #region [ Straight Replacement ]
//                    // Remove new line from the data, cuz newline will break the prn format
//                    inputData = inputData.Replace("\r\n", " ");
//                    inputData = inputData.Replace("\n", " ");

//                    varName = inputName;
//                    content = content.Replace(varName, inputData);
//                    #endregion
//                }
//            }
//            #endregion

//            byte[] byteContent = Encoding.Default.GetBytes(content);
//            return PrintBytes(documentName, quantity, byteContent);
//        }


//        #endregion

//        #region [ Print Bytes ]
//        /// <summary>
//        /// Prints raw bytes data.
//        /// </summary>
//        /// <param name="content">The content.</param>
//        /// <returns></returns>
//        public bool PrintBytes(byte[] content)
//        {
//            return PrintBytes(1, content);
//        } 

//        /// <summary>
//        /// Prints the bytes.
//        /// </summary>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="content">The content.</param>
//        /// <returns></returns>
//        public bool PrintBytes(int quantity, byte[] content)
//        {
//            string documentName = String.Empty;
//            return PrintBytes(documentName, quantity, content);
//        }

//        /// <summary>
//        /// Prints the bytes.
//        /// </summary>
//        /// <param name="documentName"></param>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="content">The content.</param>
//        /// <returns></returns>
//        public bool PrintBytes(string documentName, int quantity, byte[] content)
//        {
//            LastErrorMessage = "";

//            List<byte> multipleCopies = new List<byte>(content.Length * quantity);
//            for (int i = 0; i < quantity; i++)
//            {
//                multipleCopies.AddRange(content);
//            }

//            content = multipleCopies.ToArray();

//            IntPtr ptr = Marshal.AllocHGlobal(content.Length);
//            try
//            {
//                Marshal.Copy(content, 0, ptr, content.Length);
//                return SendBytesToPrinter(ptr, content.Length, documentName);
//            }
//            catch
//            {
//                if (ThrowExceptions)
//                {
//                    throw;
//                }
//                return false;
//            }
//            finally
//            {
//                Marshal.FreeHGlobal(ptr);
//            }
//        }
//        #endregion
        
//        #region [ Assist Functions ]
//        /// <summary>
//        /// Get the default printer's name
//        /// </summary>
//        /// <returns></returns>
//        public static string GetDefaultPrinter()
//        {
//            try
//            {
//                StringBuilder name = new StringBuilder(256);
//                int length = 256;
//                iPrinter.GetDefaultPrinter(name, ref length);
//                return name.ToString();
//            }
//            catch (Exception ex)
//            {
//                QDebug.Log(ex);
//                return "";
//            }
//        }

//        /// <summary>
//        /// Compensates the padding to help centering the string more accurately. 
//        /// Non alpha char take up less space, therefore need more padding to make it center.
//        /// </summary>
//        /// <param name="pad">The calculated padding.</param>
//        /// <param name="text">The text that need centering</param>
//        /// <returns>The more accurate padding value</returns>
//        private int compensatePadding(int pad, string text)
//        {
//            string tinyChars = ":;!.,`|il";
//            string littleChars = "[]{}()1";
//            string normalChars = "@#$%&*-+=<>?\"";
//            string bigChars = "wWmM";

//            double extraPadding = 0;
//            string curChar;
//            for (int i = 0; i < text.Length; i++)
//            {
//                curChar = text[i].ToString();

//                if (bigChars.Contains(curChar))
//                {
//                    extraPadding--;
//                }
//                else if (normalChars.Contains(curChar))
//                {
//                    continue;
//                }
//                else if (littleChars.Contains(curChar))
//                {
//                    // [1.2]    to ensure one more extra space if contains more than 3 chars (3x1.2 => 3.6 => 4)
//                    extraPadding += 1.2;
//                }
//                else if (tinyChars.Contains(curChar))
//                {
//                    // [1.51]   2x1.51 => 3.2 => 3
//                    extraPadding += 1.51;
//                }
//            }

//            int compensation = Convert.ToInt32(Math.Round(extraPadding));
//            return pad + compensation;
//        }
//        #endregion

//        #region [ Copy From RawPrinterHelper Class In DDEngine.Lib ]
//        // Structure and API declarions:
//        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
//        private class DOCINFOA
//        {
//            [MarshalAs(UnmanagedType.LPStr)]
//            public string pDocName;
//            [MarshalAs(UnmanagedType.LPStr)]
//            public string pOutputFile;
//            [MarshalAs(UnmanagedType.LPStr)]
//            public string pDataType;
//        }
//        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
//        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

//        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
//        private static extern bool ClosePrinter(IntPtr hPrinter);

//        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
//        private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

//        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
//        private static extern bool EndDocPrinter(IntPtr hPrinter);

//        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
//        private static extern bool StartPagePrinter(IntPtr hPrinter);

//        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
//        private static extern bool EndPagePrinter(IntPtr hPrinter);

//        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
//        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

//        /// <summary>
//        /// SendBytesToPrinter()
//        /// When the function is given a printer name and an unmanaged array
//        /// of bytes, the function sends those bytes to the printer queue.
//        /// </summary>
//        /// <param name="pBytes">The bytes array.</param>
//        /// <param name="dwCount">The array length.</param>
//        /// <returns></returns>
//        private bool SendBytesToPrinter(IntPtr pBytes, Int32 dwCount)
//        {
//            string documentName = String.Empty;
//            return SendBytesToPrinter(pBytes, dwCount, documentName);
//        }

//        /// <summary>
//        /// SendBytesToPrinter()
//        /// When the function is given a printer name and an unmanaged array
//        /// of bytes, the function sends those bytes to the printer queue.
//        /// </summary>
//        /// <param name="pBytes">The bytes array.</param>
//        /// <param name="dwCount">The array length.</param>
//        /// <param name="documentName">Document name that will appear on the printer pool list.</param>
//        /// <returns></returns>
//        private bool SendBytesToPrinter(IntPtr pBytes, Int32 dwCount, string documentName)
//        {
//            LastErrorMessage = "";

//            Int32 dwError = 0, dwWritten = 0;
//            IntPtr hPrinter = new IntPtr(0);
//            DOCINFOA di = new DOCINFOA();
//            bool bSuccess = false; // Assume failure unless you specifically succeed.

//            di.pDocName = MagicU.IsEmpty(documentName) ? "DDEngine iPrinter Driver" : documentName;
//            di.pDataType = "RAW";

//            // Open the printer.
//            if (OpenPrinter(this.PrinterName.Normalize(), out hPrinter, IntPtr.Zero))
//            {
//                // Start a document.
//                if (StartDocPrinter(hPrinter, 1, di))
//                {
//                    // Start a page.
//                    if (StartPagePrinter(hPrinter))
//                    {
//                        // Write your bytes.
//                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
//                        EndPagePrinter(hPrinter);
//                    }
//                    else
//                    {
//                        LastErrorMessage = "Cannot start the page.";
//                        if (ThrowExceptions)
//                        {
//                            throw new PrintingException(LastErrorMessage);
//                        }
//                    }
//                    EndDocPrinter(hPrinter);
//                }
//                else
//                {
//                    LastErrorMessage = "Cannot start the document.";
//                    if (ThrowExceptions)
//                    {
//                        throw new PrintingException(LastErrorMessage);
//                    }
//                }
//                ClosePrinter(hPrinter);
//            }
//            else
//            {
//                LastErrorMessage = "Cannot open printer " + this.PrinterName + ".";
//                if (ThrowExceptions)
//                {
//                    throw new PrinterNotFoundException(LastErrorMessage);
//                }
//            }

//            // If you did not succeed, GetLastError may give more information
//            // about why not.
//            if (bSuccess == false)
//            {
//                dwError = Marshal.GetLastWin32Error();
//                LastErrorMessage = "Error occured while printing [Error Code: " + dwError + "].";
//                if (ThrowExceptions)
//                {
//                    throw new PrinterNotFoundException(LastErrorMessage);
//                }
//            }
//            else
//            {
//                // No errors
//                LastErrorMessage = "";
//            }

//            return bSuccess;
//        }
//        #endregion
        
//        #region [ Print Raw Data ]
//        /// <summary>
//        /// Print from a dir
//        /// </summary>
//        /// <param name="filepath">The filepath.</param>
//        /// <returns></returns>
//        public bool PrintFile(string filepath)
//        {
//            return PrintFile(1, filepath);
//        }

//        /// <summary>
//        /// Print from a dir
//        /// </summary>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="filepath">The filepath.</param>
//        /// <returns></returns>
//        public bool PrintFile(int quantity, string filepath)
//        {
//            string documentName = String.Empty;
//            return PrintFile(documentName, quantity, filepath);
//        }

//        /// <summary>
//        /// Print from a dir
//        /// </summary>
//        /// <param name="documentName">Name of the document.</param>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="filepath">The filepath.</param>
//        /// <returns></returns>
//        public bool PrintFile(string documentName, int quantity, string filepath)
//        {
//            LastErrorMessage = "";
//            if (!File.Exists(filepath))
//            {
//                if (!ThrowExceptions)
//                {
//                    LastErrorMessage = "Label dir not found";
//                    return false;
//                }
//            }

//            byte[] content = File.ReadAllBytes(filepath);
//            return PrintBytes(documentName, quantity, content);
//        }

//        /// <summary>
//        /// Print from a string
//        /// </summary>
//        /// <param name="content"></param>
//        /// <returns></returns>
//        public bool PrintString(string content)
//        {
//            return PrintString(1, content);
//        }

//        /// <summary>
//        /// Print from a string
//        /// </summary>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="content">The .prn label content.</param>
//        /// <returns></returns>
//        public bool PrintString(int quantity, string content)
//        {
//            string documentName = String.Empty;
//            return PrintString(documentName, quantity, content);
//        }

//        /// <summary>
//        /// Print from a string
//        /// </summary>
//        /// <param name="documentName"></param>
//        /// <param name="quantity">The quantity.</param>
//        /// <param name="content">The .prn label content.</param>
//        /// <returns></returns>
//        public bool PrintString(string documentName, int quantity, string content)
//        {
//            byte[] bContent = Encoding.Default.GetBytes(content);
//            return PrintBytes(documentName, quantity, bContent);
//        }
//        #endregion
//    }
//}

//#region [ Usage & Example ]

///*
// * USAGE:
// * 
// * 
// * Template: DieLabel.prn
//qC
//n
//e
//c0000
//f220
//t0
//V0
//O0220
//M1016
//L
//A1
//D11
//z
//PG
//SG
//H10
//1X1100000070003b0388038400020002
//1X1100003180011l03760001
//1X1100000980015l03670001
//1a6305002370005$DieNo
//191100402900025DIE NO.:
//1X1100002910015l00800023
//191100401690205SIZE:
//1X1100001680200l00480024
//1X1100001940015l03670001
//191100200770019NOTES:
//1X1100000750014l00580021
//1X1100000990196l00010097
//191100401690015ACROSS:
//191100401370015AROUND:
//191100401050015TEETH:
//191100401690124$Across
//191100401370124$Around
//191100401050125$Teeth
//191100401400200$WD W s1 $LD L
//191100100620019$Note1
//191100100520019$Note2
//191100100420019$Note3
//191100100320019$Note4
//191100100230019$Note5
//191100701960045$DieNo
//^01
//Q0001
//E

// * Using Instance Method
// * 
// * iPrinter p = new iPrinter(@"\\psg4264\Citizen CLP-7201e");
//   p.PrintFile(@"D:\Desktop\Multi die-label.prn", "$DieNo", "12345678", "$Across", "iAcross", "$Around", "iAround", "$Teeth", "iTeeth",
//                    "$WD", "iWD", "$LD", "iLD", "Wrap40$Note", "yippy yippy yippy yo yes yes yes yes yes no no no no nono please wrap to the next line or else hung will be pissed");
 
 
// * Using Static Method
// * 
// * iPrinter.PrintFile(@"\\psg4264\Citizen CLP-7201e"
//               @"D:\Desktop\Multi die-label.prn", "$DieNo", "12345678", "$Across", "iAcross", "$Around", "iAround", "$Teeth", "iTeeth",
//               "$WD", "iWD", "$LD", "iLD", "Wrap40$Note", "yippy yippy yippy yo yes yes yes yes yes no no no no nono please wrap to the next line or else hung will be pissed");
//*/

//#endregion