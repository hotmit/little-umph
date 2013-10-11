using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
// using System.Web.Util; need manually add "System.Web" in the references section
using System.Web.Util;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;

///Todo: cleanup and remove iffy functions, remove all firefox cookie (perhap move it to a seperate dll)

namespace LittleUmph {

    /// <summary>
    /// Send Status.
    /// </summary>
    public struct SendStatus {
        public bool Success;
        public int ByteSent;
        public string ErrorMessage;
    }

    /// <summary>
    /// Send and receive HTTP traffics.
    /// </summary>
    public class WebTools
    {
        #region [ Properties ]
        /// <summary>
        /// 60 sec timeout
        /// </summary>
        private const int TIME_OUT = 60000;
        
        /// <summary>
        /// 5mb receiving limit
        /// </summary>
        private const long MAX_RECEIVING_LEN = 5242880;

        /// <summary>
        /// Set to true to pass the http data through Fiddler Proxy
        /// </summary>
        private static bool _debugWithFiddler;

        /// <summary>
        /// Set autoredirect for getpage and post
        /// </summary>
        private static bool _autoRedirect;

        private static string _userAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.4) Gecko/2008102920 Firefox/3.0.4 (.NET CLR 3.5.30729)";
        /// <summary>
        /// Get and set user-agent
        /// </summary>
        public static string UserAgent {
            get {
                return _userAgent;
            }
            set {
                if (value.Length > 0) {
                    _userAgent = value;
                }
            }
        }
        #endregion

        #region [ Static Properties ]
        /// <summary>
        /// Set autoredirect for getpage and post
        /// </summary>
        public static bool AutoRedirect
        {
            get { return _autoRedirect; }
            set { _autoRedirect = value; }
        }
        #endregion

        #region [ Debug Options ]
        /// <summary>
        /// Gets or sets a value indicating whether debug with fiddler.
        /// </summary>
        /// <value><c>true</c> if [debug with fiddler]; otherwise, <c>false</c>.</value>
        public static bool DebugWithFiddler
        {
            get { return _debugWithFiddler; }
            set { _debugWithFiddler = value; }
        }
        #endregion

        #region [ GET ]
        /// <summary>
        /// Get complete header string in text format
        /// </summary>
        /// <param name="header"></param>
        /// <returns>Text representation of the passed header</returns>
        public static string GetHeader(WebHeaderCollection header) {
            string header_string = header.ToString();
            /*
            foreach (string key in header.Keys) {
                header_string += key + ": " + header.GetValues(key) + "\r\n";
            }
            header_string += "\r\n";
            */
            return header_string;
        }

        /// <summary>
        /// GET with IE cookie and no referal
        /// </summary>
        /// <param name="url">http://...</param>
        /// <returns>Page content</returns>
        public static string GetPage(string url) {
            return GetPage(url, "", "");
        }

        /// <summary>
        /// Gets the page data.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="referal">The referal.</param>
        /// <returns></returns>
        public static string GetPage(string url, string cookie, string referal) {
            return GetPage(url, cookie, referal, true);
        }

        /// <summary>
        /// GET with specified cookie and referal
        /// </summary>
        /// <param name="url">http://...</param>
        /// <param name="cookie">cookie1=value1&cookie2=value2</param>
        /// <param name="referal">http://...</param>
        /// <returns>Page content</returns>
        public static string GetPage(string url, string cookie, string referal, bool showHeader)
        {
            return GetPage(url, cookie, referal, showHeader, false);
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="referal">The referal.</param>
        /// <param name="showHeader">if set to <c>true</c> [show header].</param>
        /// <param name="writeIECookie">if set to <c>true</c> [write IE cookie].</param>
        /// <returns></returns>
        public static string GetPage(string url, string cookie, string referal, bool showHeader, bool writeIECookie)
        {
            try 
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);

                wr.KeepAlive = false;

                wr.AllowAutoRedirect = AutoRedirect;
                wr.UseDefaultCredentials = true;
                wr.UserAgent = _userAgent;
                wr.Timeout = TIME_OUT;

                if (cookie.Length > 0)
                {
                    wr.Headers.Add("Cookie", cookie);
                }
                else
                {
                    //wr.CookieContainer = MCookie.IEGetCookieContainer(wr.RequestUri);
                }

                if (referal.Length > 0) {
                    wr.Referer = referal;
                }

                if (_debugWithFiddler) {
                    wr.Proxy = new WebProxy("127.0.0.1", 8888); 
                }

                string page = "";
                using (HttpWebResponse respond = (HttpWebResponse)wr.GetResponse())
                {
                    if (writeIECookie)
                    {
                        var list = respond.Cookies;
                        foreach (Cookie c in list)
                        {
                            //MCookie.IEWriteCookie(c);
                        }
                    }

                    if (showHeader)
                    {
                        page += GetHeader(respond.Headers);
                    }

                    if (respond.ContentLength < MAX_RECEIVING_LEN)
                    {
                        using (StreamReader sr = new StreamReader(respond.GetResponseStream()))
                        {
                            page += sr.ReadToEnd();
                            sr.Close();
                        }
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(respond.GetResponseStream()))
                        {
                            char[] buffer = new char[MAX_RECEIVING_LEN];
                            sr.Read(buffer, 0, (int)MAX_RECEIVING_LEN);

                            page += new string(buffer);
                            sr.Close();
                        }
                    }

                    respond.Close();
                }

                return page;
            }
            catch {
                return "";
            }
        }

        /// <summary>
        /// Gets the page header.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string GetPageHeader(string url) {
            return GetPageHeader(url, "", "");
        }

        /// <summary>
        /// Get the just the header
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="cookie">Cookie string or blank to get the cookie from IE</param>
        /// <param name="referal">Referal string or blank for not including referal</param>
        /// <returns>Header content</returns>
        public static string GetPageHeader(string url, string cookie, string referal) {
            try {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                wr.AllowAutoRedirect = false;                
                wr.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; InfoPath.2; .NET CLR 2.0.50727)";
                wr.Timeout = TIME_OUT;

                cookie = (cookie.Trim().Length == 0) ? GetCookie(url) : cookie;
                if (cookie.Length > 0) {
                    wr.Headers.Add("Cookie", cookie);
                }

                if (referal.Length > 0) {
                    wr.Referer = referal;
                }

                if (_debugWithFiddler) {
                    wr.Proxy = new WebProxy("127.0.0.1", 8888);
                }

                WebResponse respond = wr.GetResponse();
                string page = GetHeader(respond.Headers);
                
                respond.Close();
                return page;
            }
            catch {
                return "";
            }
        }
        #endregion

        #region [ POST ]
        /// <summary>
        /// POST without data
        /// </summary>
        /// <param name="url">http://...</param>
        /// <returns></returns>
        public static string HttpPost(string url) {
            return HttpPost(url, "");
        }

        /// <summary>
        /// POST with data as array
        /// </summary>
        /// <param name="url">http://...</param>
        /// <param name="data">array("VarName1", "VarValue1", "VarName2", "VarValue2")</param>
        /// <returns>Page content</returns>
        public static string HttpPost(string url, string[] data) {
            return HttpPost(url, data, "", "");
        }

        /// <summary>
        /// POST with string data
        /// </summary>
        /// <param name="url">http://...</param>
        /// <param name="post_data">VarName1=url_encoded(VarValue1)&VarName2=url_encoded(VarValue2)</param>
        /// <returns>Page content</returns>
        public static string HttpPost(string url, string post_data) {
            return HttpPost(url, post_data, "", "");
        }

        /// <summary>
        /// POST with cookie and referer
        /// </summary>
        /// <param name="url">http://...</param>
        /// <param name="data">array("VarName1", "VarValue1", "VarName2", "VarValue2")</param>
        /// <param name="cookie">cookie1=value1&cookie2=value2</param>
        /// <param name="referal">http://...</param>
        /// <returns>Page content</returns>
        public static string HttpPost(string url, string[] data, string cookie, string referal) {
            return HttpPost(url, data, cookie, referal, true);
        }

        /// <summary>
        /// HTTPs post.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="referal">The referal.</param>
        /// <param name="show_header">if set to <c>true</c> [show_header].</param>
        /// <returns></returns>
        public static string HttpPost(string url, string[] data, string cookie, string referal, bool show_header) {
            return HttpPost(url, PreparePostData(data), cookie, referal, show_header);
        }

        /// <summary>
        /// HTTPs post.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="post_data">The post_data.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="referal">The referal.</param>
        /// <returns></returns>
        public static string HttpPost(string url, string post_data, string cookie, string referal) {
            return HttpPost(url, post_data, cookie, referal, true);
        }

        /// <summary>
        /// POST with cookie and referer
        /// </summary>
        /// <param name="url">http://...</param>
        /// <param name="post_data">VarName1=url_encoded(VarValue1)&VarName2=url_encoded(VarValue2)</param>
        /// <param name="cookie">cookie1=value1&cookie2=value2</param>
        /// <param name="referal">http://...</param>
        /// <returns>Page content</returns>
        public static string HttpPost(string url, string post_data, string cookie, string referal, bool show_header)
        {
            return HttpPost(url, post_data, cookie, referal, show_header, false);
        }

        /// <summary>
        /// HTTPs the post.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="post_data">The post_data.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="referal">The referal.</param>
        /// <param name="show_header">if set to <c>true</c> [show_header].</param>
        /// <param name="writeIECookie">if set to <c>true</c> [write IE cookie].</param>
        /// <returns></returns>
        public static string HttpPost(string url, string post_data, string cookie, string referal, bool show_header, bool writeIECookie) {
            try 
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                wr.CookieContainer = new CookieContainer();

                wr.AllowAutoRedirect = AutoRedirect;
                wr.UseDefaultCredentials = true;
                wr.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; InfoPath.2; .NET CLR 2.0.50727)";
                wr.Timeout = TIME_OUT;

                if (url.Contains("megaupload")){
                    wr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; MEGAUPLOAD TOOLBAR 1.0)";
                }

                if (_debugWithFiddler) {
                    wr.Proxy = new WebProxy("127.0.0.1", 8888);
                }
                
                //cookie = (cookie.Trim().Length == 0) ? GetCookie(url) : cookie;
                if (cookie.Length > 0)
                {
                    wr.Headers.Add("Cookie", cookie);
                }
                else
                {
                    //wr.CookieContainer = MCookie.IEGetCookieContainer(wr.RequestUri);
                }
                
                if (referal.Length > 0) {
                    wr.Referer = referal;
                }

                wr.ContentType = "application/x-www-form-urlencoded";
                wr.Method = "POST";

                byte[] payload = Encoding.UTF8.GetBytes(post_data);

                wr.ContentLength = payload.Length;

                Stream writer = wr.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();

                HttpWebResponse respond = (HttpWebResponse)wr.GetResponse();
                string page = "";

                if (writeIECookie)
                {
                    var list = respond.Cookies;
                    foreach (Cookie c in list)
                    {
                        //MCookie.IEWriteCookie(c);
                    }
                }
                
                if (show_header) {
                    page += GetHeader(respond.Headers);
                }                

                if (respond.ContentLength < MAX_RECEIVING_LEN) {
                    using (StreamReader sr = new StreamReader(respond.GetResponseStream()))
                    {
                        page += sr.ReadToEnd();
                        sr.Close();
                    }
                }

                respond.Close();
                wr.Abort();

                return page;
            }
            catch (Exception xpt){
                return xpt.Message;
            }
        }
        #endregion

        #region [ Encode/Decode ]
        /// <summary>
        /// Encode data with url safe encoding
        /// </summary>
        /// <param name="data">Plain text</param>
        /// <returns></returns>
        public static string UrlEncode(string data) {
            return HttpUtility.UrlEncode(data);
        }

        /// <summary>
        /// Decode data with url safe decoding
        /// </summary>
        /// <param name="data">Encoded string</param>
        /// <returns></returns>
        public static string UrlDecode(string data) {
            return HttpUtility.UrlDecode(data);
        }

        /// <summary>
        /// Turn & into &amp;
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HtmlEncode(string data)
        {
            return HttpUtility.HtmlEncode(data);
        }

        /// <summary>
        /// Turn &amp; into &
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HtmlDecode(string data)
        {
            return HttpUtility.HtmlDecode(data);
        }

        /// <summary>
        /// Encode text using base64 encoding
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Base64Decode(string text)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(text));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Decode base64 messages
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Base64Encode(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Get all the cookies associated with the domain.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetCookie(string url) {
            //return GetCookie(url, null);
            //return MCookie.GetCookieString(url);
            return "";
        }
        #endregion         

        #region [ Utilities ]
        /// <summary>
        /// Convert array into a proper POST DATA string
        /// </summary>
        /// <param name="data">array("VarName1", "VarValue1", "VarName2", "VarValue2")</param>
        /// <returns>Post string: VarName1=url_encoded(VarValue1)&VarName2=url_encoded(VarValue2)</returns>
        public static string PreparePostData(string[] data)
        {
            if (data.Length == 0)
            {
                return "";
            }

            string post_data = "";

            post_data += data[0] + "=" + UrlEncode(data[1]);
            for (int i = 2; i < data.Length; i = i + 2)
            {
                post_data += "&" + data[i] + "=" + UrlEncode(data[i + 1]);
            }
            return post_data;
        }

        /// <summary>
        /// Get forward Link
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string ExtractForwardLink(string header)
        {
            Match m = Regex.Match(header, "Location: (.+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return m.Groups[1].Value;
            }
            return "";
        }

        /// <summary>
        /// Extract the UserAgent from http header
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string ExtractUserAgent(string header, string defaultValue)
        {
            Match m = Regex.Match(header, "User-Agent: (.+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string agent = m.Groups[1].Value;
                if (agent.Length > 10)
                {
                    return agent;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Remove html tags in an html document
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string StripHTML(string data)
        {
            throw new Exception("Function not yet implemented");

            //Match m = Regex.Match(data, @"<body>(.|\s)+?</body>", RegexOptions.IgnoreCase & RegexOptions.Singleline);
            //if (m.Success)
            //{
            //    data = m.Value;
            //}
            //data = Regex.Replace(data, @"<(.|\s)*?>", string.Empty);
            //data = Regex.Replace(data, "(\n\r?)+", "\n");

            //data = HtmlDecode(data);

            //return data;
        }
        #endregion

        #region [ HTTP Commands ]
        /// <summary>
        /// Return 503 Header
        /// </summary>
        /// <param name="connection"></param>
        public static void Send503Header(Socket connection)
        {
            String buffer = "HTTP/1.1 503 Service Temporarily Unavailable\r\nServer: Apache/2.2.4 (Win32)\r\nX-Powered-By: PHP/5.2.0\r\nConnection: close\r\nContent-Type: text/html\r\n\r\n<H1>Service Temporarily Unavailable!</H1>";
            byte[] data = Encoding.UTF8.GetBytes(buffer);
            SendData(connection, data);
        }

        /// <summary>
        /// Return 404 Header
        /// </summary>
        /// <param name="connection"></param>
        public static void Send404Header(Socket connection)
        {
            String buffer = "HTTP/1.1 404 Not Found\r\nServer: Apache/2.2.4 (Win32)\r\nX-Powered-By: PHP/5.2.0\r\nConnection: close\r\nContent-Type: text/html\r\n\r\n<H1>HTTP/1.1 404 Not Found!</H1>";
            byte[] data = Encoding.UTF8.GetBytes(buffer);
            SendData(connection, data);
        }

        /// <summary>
        /// Send header 200 down the socket.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public static void Send200Header(Socket connection)
        {
            string buffer = "HTTP/1.1 200 OK\r\n\r\n<h1>HTTP/1.1 200 OK</h1>";
            byte[] data = Encoding.UTF8.GetBytes(buffer);
            SendData(connection, data);
        }

        /// <summary>
        /// Send back dir.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="content">The content.</param>
        public static void SendFile(Socket connection, string filename, string content)
        {
            filename = IOFunc.FilenameEncoding(filename);
            String buffer = "HTTP/1.1 200 OK\r\nCache-control: private\r\nPragma: no-cache\r\nContent-Type: application/octet-stream\r\nContent-Transfer-Encoding: binary\r\nContent-Length: " + content.Length + "\r\nAccept-Ranges: bytes\r\nContent-Disposition: attachment; filename=\"" + filename + "\"\r\n\r\n" + content + "\r\n";
            byte[] data = Encoding.UTF8.GetBytes(buffer);
            SendData(connection, data);
        }

        /// <summary>
        /// HTTP forward.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="url">The URL.</param>
        public static void HttpForward(Socket connection, string url)
        {
            String buffer = "HTTP/1.1 302 Found\r\nServer: Apache/2.2.4 (Win32)\r\nX-Powered-By: PHP/5.2.0\r\n";
            buffer += "Location: " + url + "\r\n";
            buffer += "Content-Length: 0\r\nConnection: close\r\nContent-Type: text/html\r\n\r\n";
            byte[] data = Encoding.UTF8.GetBytes(buffer);
            SendData(connection, data);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static SendStatus SendData(Socket connection, string data)
        {
            byte[] byte_data = Encoding.UTF8.GetBytes(data);
            return SendData(connection, byte_data);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static SendStatus SendData(Socket connection, byte[] data)
        {
            SendStatus status;
            try
            {
                if (connection.Connected)
                {
                    status.ByteSent = connection.Send(data, data.Length, SocketFlags.None);

                    status.Success = true;
                    status.ErrorMessage = "";

                    return status;
                }

                status.ErrorMessage = "Connection is not open!";
            }
            catch (Exception xpt)
            {
                status.ErrorMessage = xpt.Message;
            }
            status.Success = false;
            status.ByteSent = 0;

            return status;
        } 
        #endregion

        #region [ Advance Functions ]
        /// <summary>
        /// Download dir, if exist overwrite
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fullpath"></param>
        public static void Download(string url, string fullpath)
        {
            Download(url, fullpath, true);
        }

        /// <summary>
        /// Download dir from the web.
        /// </summary>
        /// <param name="url">Direct link to the dir</param>
        /// <param name="fullpath">Fullpath including filename</param>
        /// <param name="overwrite"></param>
        public static void Download(string url, string fullpath, bool overwrite)
        {
            using (WebClient web = new WebClient())
            {
                if (File.Exists(fullpath))
                {
                    if (overwrite)
                    {
                        File.Delete(fullpath);
                    }
                    else
                    {
                        //throw new IOException("File (" + fullpath + ") already exist, cannot overwrite!");
                        return;
                    }
                }
                string path = IOFunc.GetPath(fullpath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string tempPath = fullpath + ".download";
                try
                {
                    web.DownloadFile(url, tempPath);
                }
                catch (Exception)
                {
                    return;
                }
                File.Move(tempPath, fullpath);
            }
        }

        /// <summary>
        /// Download a dir with timeout
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="overwrite">if set to <c>true</c> overwrite the existing dir.</param>
        /// <param name="timeout">The timeout measure in seconds.</param>
        public static void Download(string url, string fullpath, bool overwrite, int timeout)
        {
            if (File.Exists(fullpath))
            {
                if (overwrite)
                {
                    File.Delete(fullpath);
                }
                else
                {
                    return;
                }
            }
            string path = IOFunc.GetPath(fullpath);
            if (!Directory.Exists(path))
            {
                IOFunc.ConstructPath(path);
            }

            string tempPath = String.Format("{0}.download", fullpath);
            try
            {
                var wreq = (HttpWebRequest)WebRequest.Create(url);
                wreq.Timeout = timeout;
                wreq.KeepAlive = false;
                wreq.AllowAutoRedirect = true;
                wreq.UserAgent = WebTools.UserAgent;

                using (var resp = (HttpWebResponse)wreq.GetResponse())
                {
                    using (Stream strm = resp.GetResponseStream())
                    {
                        var buffer = new byte[2047];

                        using (var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            int count;
                            while ((count = strm.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                fs.Write(buffer, 0, count);
                            }
                            fs.Close();
                        }
                        strm.Close();
                    }
                    resp.Close();
                }
            }
            catch (Exception xpt)
            {
                Console.WriteLine(xpt.Message);
                return;
            }
            if (!File.Exists(fullpath))
            {
                File.Move(tempPath, fullpath);
            }
        }

        /// <summary>
        /// Upload dir with multipart format
        /// </summary>
        /// <param name="url">http://www.posting.url</param>
        /// <param name="postFile">Array [input_name, fullfile_path]</param>
        /// <param name="postData">Array [name1, value1,  name2, value2 ...]</param>
        /// <param name="cookie">cookie_name1=cookie_value1; cookie_name2=cookie_value2 or empty to fetch cookie from IE</param>
        /// <param name="referal">Referal url or empty to ignore referal field</param>
        /// <param name="showHeader">if set to <c>true</c> show header.</param>
        /// <returns>Response text</returns>
        public static string FileUpload(string url, string[] postFile, string[] postData, string cookie, string referal, bool showHeader)
        {
            try
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                byte[] closingbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                wr.ProtocolVersion = HttpVersion.Version11;
                wr.UseDefaultCredentials = true;
                //wr.AllowAutoRedirect = true;
                wr.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; InfoPath.2; .NET CLR 2.0.50727)";
                wr.Method = "POST";
                wr.KeepAlive = true;

                // Remove the Expect: 100-continue from the header [A MUST]
                ServicePointManager.Expect100Continue = false;

                if (_debugWithFiddler)
                {
                    wr.Proxy = new WebProxy("127.0.0.1", 8888);
                }
                cookie = (cookie.Trim().Length == 0) ? GetCookie(url) : cookie;
                if (cookie.Length > 0)
                {
                    wr.Headers.Add("Cookie", cookie);
                }
                if (referal.Length > 0)
                {
                    wr.Referer = referal;
                }
                wr.ContentType = "multipart/form-data; boundary=" + boundary;

                Stream mstream = new MemoryStream();
                for (int i = 0; i < postData.Length; i = i + 2)
                {
                    string formitem = string.Format(formdataTemplate, postData[i], postData[i + 1]);
                    // Remove \r\n for the first item
                    if (i == 0)
                    {
                        formitem = formitem.TrimStart();
                    }
                    byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                    mstream.Write(formitembytes, 0, formitembytes.Length);
                }

                mstream.Write(boundarybytes, 0, boundarybytes.Length);
                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                FileInfo file = new FileInfo(postFile[1]);
                MimeType mime_type = new MimeType();
                string fileheader = string.Format(headerTemplate, postFile[0], file.Name, mime_type.GetMineTypeByExtension(file.Extension));
                byte[] fileheaderbytes = Encoding.UTF8.GetBytes(fileheader);
                mstream.Write(fileheaderbytes, 0, fileheaderbytes.Length);

                wr.ContentLength = mstream.Length + file.Length + closingbytes.Length;

                /// Start the writing process
                Stream requestStream = wr.GetRequestStream();

                mstream.Position = 0;
                byte[] tempBuffer = new byte[mstream.Length];
                mstream.Read(tempBuffer, 0, tempBuffer.Length);
                mstream.Close();
                requestStream.Write(tempBuffer, 0, tempBuffer.Length);

                // File writing
                FileStream filestream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                byte[] filebuffer = new byte[1024];
                int bytecount = 0;
                while ((bytecount = filestream.Read(filebuffer, 0, 1024)) > 0)
                {
                    requestStream.Write(filebuffer, 0, bytecount);
                }

                filestream.Close();
                requestStream.Write(closingbytes, 0, closingbytes.Length);
                requestStream.Close();

                /// End writing process


                HttpWebResponse respond = (HttpWebResponse)wr.GetResponse();
                string page = "";
                if (showHeader)
                {
                    page += GetHeader(respond.Headers);
                }

                if (respond.ContentLength < MAX_RECEIVING_LEN)
                {
                    StreamReader sr = new StreamReader(respond.GetResponseStream());
                    page += sr.ReadToEnd();
                    sr.Close();
                }

                respond.Close();

                return page;
            }
            catch (Exception xpt)
            {
                return xpt.Message;
            }
        } 
        #endregion





        /// <summary>
        /// Gets the fire fox profile path.
        /// </summary>
        /// <returns></returns>
        public static string GetFireFoxProfilePath()
        {
            //string cookiepath = mcookie.getcookiepath(mcookie.ff3cookiename);
            //string profilepath = IOFunc.getpath(cookiepath);
            //return profilepath;
            return "";
        }

        #region [ Instance ]
        private Socket _connection;

        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>The connection.</value>
        public Socket Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTools"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public WebTools(Socket connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Send 503 header.
        /// </summary>
        public void Send503Header()
        {
            Send503Header(Connection);
        }

        /// <summary>
        /// HTTP forward.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void HttpForward(string url)
        {
            HttpForward(Connection, url);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public SendStatus SendData(string data)
        {
            return SendData(Connection, data);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public SendStatus SendData(byte[] data)
        {
            return SendData(Connection, data);
        } 
        #endregion
    }

    /// <summary>
    /// Mime Type
    /// </summary>
    public class MimeType {
        private Hashtable _mimeTypes;

        /// <summary>
        /// Gets the MIME types.
        /// </summary>
        /// <value>The MIME types.</value>
        public Hashtable MimeTypes
        {
            get { return _mimeTypes; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeType"/> class.
        /// </summary>
        public MimeType() {
            initHashtable();
        }

        /// <summary>
        /// Inits the hashtable.
        /// </summary>
        private void initHashtable() {
            // http://www.webmaster-toolkit.com/mime-types.shtml
            _mimeTypes = new Hashtable();

            #region [ Mime Types Data ]
            _mimeTypes.Add(".arj", "application/arj");
            _mimeTypes.Add(".asp", "text/asp");
            _mimeTypes.Add(".asx", "application/x-mplayer2");
            _mimeTypes.Add(".au", "audio/basic");
            _mimeTypes.Add(".avi", "video/avi");
            _mimeTypes.Add(".bz", "application/x-bzip");
            _mimeTypes.Add(".f", "text/plain");
            _mimeTypes.Add(".f++", "text/plain");
            _mimeTypes.Add(".cc", "text/plain");
            _mimeTypes.Add(".class", "application/java");
            _mimeTypes.Add(".com", "application/octet-stream");
            _mimeTypes.Add(".cpp", "text/x-f");
            _mimeTypes.Add(".csh", "application/x-csh");
            _mimeTypes.Add(".css", "text/css");
            _mimeTypes.Add(".doc", "application/msword");
            _mimeTypes.Add(".exe", "application/octet-stream");
            _mimeTypes.Add(".gif", "image/gif");
            _mimeTypes.Add(".gtar", "application/x-gtar");
            _mimeTypes.Add(".gz", "application/x-gzip");
            _mimeTypes.Add(".gzip", "application/x-gzip");
            _mimeTypes.Add(".h", "text/plain");
            _mimeTypes.Add(".help", "application/x-helpfile");
            _mimeTypes.Add(".htm", "text/html");
            _mimeTypes.Add(".html", "text/html");
            _mimeTypes.Add(".htmls", "text/html");
            _mimeTypes.Add(".ico", "image/x-icon");
            _mimeTypes.Add(".java", "text/plain");
            _mimeTypes.Add(".jpeg", "image/jpeg");
            _mimeTypes.Add(".jpg", "image/jpeg");
            _mimeTypes.Add(".js", "application/x-javascript");
            _mimeTypes.Add(".lha", "application/octet-stream");
            _mimeTypes.Add(".m1v", "video/mpeg");
            _mimeTypes.Add(".m2a", "audio/mpeg");
            _mimeTypes.Add(".m2v", "video/mpeg");
            _mimeTypes.Add(".m3u", "audio/x-mpequrl");
            _mimeTypes.Add(".mht", "message/rfc822");
            _mimeTypes.Add(".mhtml", "message/rfc822");
            _mimeTypes.Add(".mid", "audio/midi");
            _mimeTypes.Add(".midi", "audio/midi");
            _mimeTypes.Add(".mime", "www/mime");
            _mimeTypes.Add(".mod", "audio/mod");
            _mimeTypes.Add(".mov", "video/quicktime");
            _mimeTypes.Add(".mp2", "audio/mpeg");
            _mimeTypes.Add(".mp3", "audio/mpeg3");
            _mimeTypes.Add(".mpa", "video/mpeg");
            _mimeTypes.Add(".mpe", "video/mpeg");
            _mimeTypes.Add(".mpeg", "video/mpeg");
            _mimeTypes.Add(".mpg", "video/mpeg");
            _mimeTypes.Add(".mpga", "audio/mpeg");
            _mimeTypes.Add(".mpv", "application/x-project");
            _mimeTypes.Add(".png", "image/png");
            _mimeTypes.Add(".pps", "application/mspowerpoint");
            _mimeTypes.Add(".ppt", "application/mspowerpoint");
            _mimeTypes.Add(".ps", "application/postscript");
            _mimeTypes.Add(".psd", "application/octet-stream");
            _mimeTypes.Add(".ra", "audio/x-pn-realaudio");
            _mimeTypes.Add(".ram", "audio/x-pn-realaudio");
            _mimeTypes.Add(".rm", "audio/x-pn-realaudio");
            _mimeTypes.Add(".rpm", "audio/x-pn-realaudio-plugin");
            _mimeTypes.Add(".sh", "application/x-sh");
            _mimeTypes.Add(".shtml", "text/html");
            _mimeTypes.Add(".tar", "application/x-tar");
            _mimeTypes.Add(".tif", "image/tiff");
            _mimeTypes.Add(".tiff", "image/tiff");
            _mimeTypes.Add(".wav", "audio/wav");
            _mimeTypes.Add(".xml", "application/xml");
            _mimeTypes.Add(".zip", "application/zip");
            #endregion
        }

        /// <summary>
        /// Gets the type of the MIME.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <returns></returns>
        public string GetMimeType(string filepath) {
            return GetMimeType(new FileInfo(filepath));
        }

        /// <summary>
        /// Gets the type of the MIME.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns></returns>
        public string GetMimeType(FileInfo file) {
            return GetMineTypeByExtension(file.Extension);   
        }

        /// <summary>
        /// Gets the mine type by extension.
        /// </summary>
        /// <param name="oldExt">The oldExt.</param>
        /// <returns></returns>
        public string GetMineTypeByExtension(string ext) {
            ext = ext.StartsWith(".") ? ext : String.Format(".{0}", ext);
            ext = ext.ToLower();

            if (_mimeTypes.ContainsKey(ext)) {
                string mime = (string)_mimeTypes[ext];
                return mime;
            }
            return "application/octet-stream";
        }
    }
}