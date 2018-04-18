using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;

#if NET35_OR_GREATER
using System.Linq;
#endif

using LittleUmph.GUI.Forms;

namespace LittleUmph
{
    /// <summary>
    /// GUI related helper functions.
    /// </summary>
    public class UI
    {
        #region [ TextBox Functions ]
        /// <summary>
        /// Selects all the text and focus on the textbox afterward (ThreadSafe).
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        public static void SelectAll(TextBox textbox)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke((MethodInvoker)delegate { SelectAll(textbox); });
            }
            else
            {
                textbox.SelectAll();
                textbox.Focus();
            }
        }

        /// <summary>
        /// Put cursor at the end of of the textbox and set the focus on the control (ThreadSafe).
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        public static void ScrollToEnd(TextBox textbox)
        {
            ScrollToEnd(textbox, true);
        }

        /// <summary>
        /// Put the cursor at the end of line and also can change the focus as well (ThreadSafe).
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        /// <param name="setfocus">if set to <c>true</c> to set focus on the textbox after moving the cursor.</param>
        public static void ScrollToEnd(TextBox textbox, bool setfocus)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke((MethodInvoker)delegate { ScrollToEnd(textbox, setfocus); });
            }
            else
            {
                textbox.SelectionStart = textbox.TextLength;
                textbox.SelectionLength = 0;
                textbox.ScrollToCaret();

                if (setfocus)
                {
                    textbox.Focus();
                }
            }
        }

        /// <summary>
        /// Move cursor to the specified location (ThreadSafe).
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        /// <param name="index">The index to move the cursor to.</param>
        /// <param name="setfocus">if set to <c>true</c> to set focus on the textbox after moving the cursor.</param>
        public static void ScrollTo(TextBox textbox, int index, bool setfocus)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke((MethodInvoker)delegate { ScrollTo(textbox, index, setfocus); });
            }
            else
            {
                index = Num.Filter(index, 0, textbox.TextLength);

                textbox.SelectionStart = index;
                textbox.SelectionLength = 0;
                textbox.ScrollToCaret();

                if (setfocus)
                {
                    textbox.Focus();
                }
            }
        }


        /// <summary>
        /// Called when the enter key is pressed.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="action">The action.</param>
        /// <param name="enterKeyOnly">if true Ctrl+Enter do not raise the action (similaly Shift+Enter or Alt+Enter)</param>
        public static void OnEnter(Control control, Action action, bool enterKeyOnly = true)
        {
            control.KeyUp += (s, evt) =>
            {
                if (evt.KeyCode == Keys.Enter)
                {
                    if (enterKeyOnly && (evt.Control || evt.Alt || evt.Shift))
                    {
                        return;
                    }
                    action();
                    evt.Handled = true;
                }
            };
        }

        /// <summary>
        /// Use the key input from textbox to navigate the selection of the listbox.
        /// Up/Dow arrow to move up and down one space, PgUp/PgDn to move up and down 5 items.
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        /// <param name="listbox">The listbox.</param>
        public static void NavigateListBox(TextBox textbox, ListBox listbox, bool loop = true)
        {
            textbox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down)
                {
                    MoveDownTheList(listbox, 1, loop);
                }
                else if (e.KeyCode == Keys.Up)
                {
                    MoveUpTheList(listbox, 1, loop);
                }
                else if (e.KeyCode == Keys.PageDown)
                {
                    MoveDownTheList(listbox, 5, loop);
                }
                else if (e.KeyCode == Keys.PageUp)
                {
                    MoveUpTheList(listbox, 5, loop);
                }

            };
        }


        /// <summary>
        /// Load autocomplete data from a list of strings,
        /// this function will remove any duplicate entries.
        /// Autocomplete doesn't work when it is in multiline mode.
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        /// <param name="data">The data.</param>
        public static void AutoComplete(TextBox textbox, IList<string> data)
        {
            textbox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            if (textbox.AutoCompleteMode == AutoCompleteMode.None)
            {
                textbox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            }

#if NET35_OR_GREATER
            textbox.AutoCompleteCustomSource.AddRange(data.Distinct().ToArray());
#else
            string old = "";
            foreach (var l in data)
            {
                if (Str.IsNotEmpty(l) && !Str.Contains(old, l + "{|}"))
                {
                    old += l + "{|}";
                    textbox.AutoCompleteCustomSource.Add(l);
                }
            }
#endif
        }
        #endregion 

        #region [ RichTextbox ]
        /// <summary>
        /// Move cursor to specified index.
        /// </summary>
        /// <param name="richtextbox">The richtextbox.</param>
        /// <param name="index">The index.</param>
        /// <param name="focus">if set to <c>true</c> [focus].</param>
        public static void ScrollTo(RichTextBox richtextbox, int index, bool focus)
        {
            // out of bound
            if (richtextbox.TextLength <= index)
            {
                return;
            }

            richtextbox.SelectionStart = index;
            richtextbox.SelectionLength = 0;

            if (focus)
            {
                richtextbox.Focus();
            }
        }
        #endregion

        #region [ Disable Close Button ]
        /// <summary>
        /// Disables the close button.
        /// </summary>
        /// <param name="form">The form.</param>
        public static void DisableCloseButton(Form form)
        {
            EnableMenuItem(GetSystemMenu(form.Handle, false), SC_CLOSE, MF_GRAYED);
        }

        private const int SC_CLOSE = 0xF060;
        private const int MF_GRAYED = 0x1;
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern int EnableMenuItem(IntPtr hMenu, int wIDEnableItem, int wEnable);
        #endregion

        #region [ Radio Button]
        /// <summary>
        /// Get selected radio button within a control (example groupbox).
        /// Useful when need to save/restore radio button in a group box.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        /// <param name="defaultIndex">The default value.</param>
        /// <param name="depth">How many layer to search for the RadioButton.</param>
        /// <returns></returns>
        public static int GetRadIndex(Control parent, int defaultIndex, int depth = 3)
        {
            int index = -1;

            foreach (RadioButton rad in GetInnerControls<RadioButton>(parent, depth))
            {
                index++;
                if (rad.Checked)
                {
                    return index;
                }
            }
            return defaultIndex;
        }

        /// <summary>
        /// Set selected radio button within a control.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        /// <param name="checkedIndex">The index to check the radio button.</param>
        /// <param name="depth">How many layer to search for the RadioButton.</param>
        public static void SetRadIndex(Control parent, int checkedIndex, int depth = 3)
        {
            int count = -1;
            foreach (RadioButton rad in GetInnerControls<RadioButton>(parent, depth))
            {
                count++;
                if (count == checkedIndex)
                {
                    rad.Checked = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Make a collection of toolstrip menu items to 
        /// behave like a radio button.
        /// </summary>
        /// <param name="defaultSelection">The default selection.</param>
        /// <param name="items">The items.</param>
        public static void ToolStripMenuRadio(int defaultSelection, params ToolStripMenuItem[] items)
        {
            int checkedIndex = Num.Filter(defaultSelection, 0, items.Length);
            ToolStripMenuItem lastChecked = null;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Checked)
                {
                    checkedIndex = i;
                }

                items[i].CheckedChanged += (o, e) =>
                {
                    ToolStripMenuItem item = (ToolStripMenuItem)o;
                    if (item.Checked)
                    {
                        if (lastChecked != null && lastChecked != item)
                        {
                            var old = lastChecked;
                            lastChecked = item;
                            old.Checked = false;
                        }
                        else
                        {
                            lastChecked = item;
                        }
                    }
                    else if (item == lastChecked)
                    {
                        item.Checked = true;
                    }

                };
            }

            items[checkedIndex].Checked = true;
        }
        #endregion

        #region [ GetInnerControls ]
        /// <summary>
        /// Gets the inner controls (depth => 3)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c">The control.</param>
        /// <returns></returns>
        public static List<T> GetInnerControls<T>(Control c) where T : Control
        {
            return GetInnerControls<T>(c, 3);
        }

        /// <summary>
        /// Gets the inner controls with type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c">The control.</param>
        /// <param name="dept">The dept.</param>
        /// <returns></returns>
        public static List<T> GetInnerControls<T>(Control c, int dept) where T : Control
        {
            List<T> found = new List<T>();
            getInnerControls(ref found, c, dept);

            return found;
        }

        /// <summary>
        /// Gets the first inner control that matches the specified control type (T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c">The control.</param>
        /// <returns></returns>
        public static T GetInnerControl<T>(Control c) where T : Control
        {
            List<T> found = GetInnerControls<T>(c);

            if (found.Count > 0)
            {
                return found[0];
            }
            return null;
        }

        /// <summary>
        /// Gets the inner controls.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="found">The found.</param>
        /// <param name="c">The control.</param>
        /// <param name="dept">The dept.</param>
        private static void getInnerControls<T>(ref List<T> found, Control c, int dept) where T : Control
        {
            if (dept <= 0)
            {
                return;
            }

            foreach (Control innerControl in c.Controls)
            {
                if (innerControl is T)
                {
                    found.Add((T)innerControl);
                }

                if (innerControl.Controls.Count > 0)
                {
                    getInnerControls(ref found, innerControl, dept - 1);
                }
            }
        }
        #endregion

        #region [ Serialize Data ]
        /// <summary>
        /// Save the content of the ListView to serialize dir.
        /// </summary>
        /// <param name="listView">The list view.</param>
        /// <param name="targetPath">The target path.</param>
        public static void ListViewSave(ListView listView, string targetPath)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream fs = File.OpenWrite(targetPath))
                {
                    bf.Serialize(fs, new ArrayList(listView.Items));
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("UI.ListViewSave()", xpt);
            }
        }

        /// <summary>
        /// Load the ListView with serialized data.
        /// </summary>
        /// <param name="listView">The list view.</param>
        /// <param name="sourcePath">The source path.</param>
        public static void ListViewLoad(ListView listView, string sourcePath)
        {
            if (!File.Exists(sourcePath))
            {
                return;
            }

            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream fs = File.OpenRead(sourcePath))
                {
                    ArrayList rows = (ArrayList)bf.Deserialize(fs);
                    ListViewItem[] items = (ListViewItem[])rows.ToArray(typeof(ListViewItem));
                    listView.Items.AddRange(items);
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("UI.ListViewLoad()", xpt);
            }
        }
        #endregion

        #region [ Drag Drop Handling ]
        /// <summary>
        /// Get text from a drag event [Html, StringFormat, Text, Rtf, UnicodeText]
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetDragDropText(DragEventArgs e)
        {
            string text = GetDragDropText(e, DataFormats.Html) + "\n";
            text += GetDragDropText(e, DataFormats.StringFormat) + "\n";
            text += GetDragDropText(e, DataFormats.Text) + "\n";
            text += GetDragDropText(e, DataFormats.Rtf) + "\n";
            text += GetDragDropText(e, DataFormats.UnicodeText);

            return text;
        }

        /// <summary>
        /// Get text from drag event based on the specified format
        /// </summary>
        /// <param name="e"></param>
        /// <param name="format">DataFormats.format</param>
        /// <returns></returns>
        public static string GetDragDropText(DragEventArgs e, string format)
        {
            try
            {
                object drop = (e.Data.GetData(format));
                string text = (string)drop;
                return text;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Get href from <a href=""></a> tag
        /// </summary>
        /// <param name="e"></param>
        /// <returns>link</returns>
        public static string GetDropLink(DragEventArgs e)
        {
            string text = GetDragDropText(e);
            // Firefox format
            Match m = Regex.Match(text, @"(?<=href=['""])(http|https|ftp)[^\s\]""'<>]+");

            // If the ideal sitution above failed, then use this more generic one
            if (!m.Success)
            {
                m = Regex.Match(text, @"(http|https|ftp)[^\s\)\]""']+");
            }
            return WebTools.HtmlDecode(m.Value);
        }

        /// <summary>
        /// Get all the href
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<string> GetDropLinks(DragEventArgs e)
        {
            string text = GetDragDropText(e);
            // First pass decode
            text = WebTools.UrlDecode(text);
            // Second pass decode
            text = WebTools.UrlDecode(text);

            List<string> list = new List<string>();
            string link;

            MatchCollection matches = Regex.Matches(text, @"(http|https|ftp)[^\s\]""'<>]+");
            foreach (Match m in matches)
            {
                link = WebTools.UrlDecode(m.Value);

                if (!list.Contains(link))
                {
                    list.Add(link);
                }
            }
            return list;
        }

        /// <summary>
        /// Get link from drag content. Assume the source is HTML content.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetDragSourceLink(DragEventArgs e)
        {
            string text = GetDragDropText(e, DataFormats.Html) + " ";
            Match m = Regex.Match(text, @"SourceURL:(.+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string link = m.Groups[1].Value.Trim();
                return link;
            }
            return "";
        }

        /// <summary>
        /// Get all the files that have been dropped.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        public static List<FileInfo> GetDropFileInfoList(DragEventArgs e)
        {
            var pathList = (string[])e.Data.GetData(DataFormats.FileDrop);

            List<FileInfo> filelist = new List<FileInfo>();
            foreach (string f in pathList)
            {
                if (!Directory.Exists(f) && File.Exists(f))
                {
                    filelist.Add(new FileInfo(f));
                }
            }
            return filelist;
        }

        /// <summary>
        /// Get all the directory that have been dropped.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        public static List<DirectoryInfo> GetDropDirectoryInfoList(DragEventArgs e)
        {
            var pathList = (string[])e.Data.GetData(DataFormats.FileDrop);

            List<DirectoryInfo> dirlist = new List<DirectoryInfo>();
            foreach (string d in pathList)
            {
                if (Directory.Exists(d))
                {
                    dirlist.Add(new DirectoryInfo(d));
                }
            }
            return dirlist;
        }
        #endregion

        #region [ CenterPictureBoxImage ]
        /// <summary>
        /// Resize the Image and put the picture in the center of the picture box
        /// </summary>
        /// <param name="pictureBox">The picture box.</param>
        public static void CenterPictureBoxImage(PictureBox pictureBox)
        {
            if (pictureBox.Image == null)
            {
                return;
            }
            Size bestfit = Img.GetMaxSize(pictureBox.Size, pictureBox.Image.Size);
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            Bitmap bmp = new Bitmap(pictureBox.Image, bestfit.Width, bestfit.Height);
            pictureBox.Image = bmp;
        }
        #endregion

        #region [ Richtext Highlight & Context ]
        /// <summary>
        /// Highlight the text in the richtextbox and return the list of indexes
        /// </summary>
        /// <param name="richTextBox">The rich text box.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="highlightColor">Color of the highlight.</param>
        /// <param name="highlightBackColor">Color of the highlight back.</param>
        /// <returns></returns>
        public static List<int> RichtextHighlight(RichTextBox richTextBox, string searchText, Color highlightColor, Color highlightBackColor)
        {
            return RichtextHighlight(richTextBox, searchText, richTextBox.ForeColor, richTextBox.Font, highlightColor, highlightBackColor);
        }
        /// <summary>
        /// Highlight the text in the richtextbox and return the list of indexes
        /// </summary>
        /// <param name="richTextBox">The rich text box.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="unselectedTextColor">Color of the unselected text.</param>
        /// <param name="highlightTextFont">The highlight text font.</param>
        /// <param name="highlightColor">Color of the highlight.</param>
        /// <param name="highlightBackColor">Color of the highlight back.</param>
        /// <returns></returns>
        public static List<int> RichtextHighlight(RichTextBox richTextBox, string searchText, Color unselectedTextColor, Font highlightTextFont, Color highlightColor, Color highlightBackColor)
        {
            if (Str.IsEmpty(searchText))
            {
                richTextBox.Text = richTextBox.Text;
                richTextBox.ForeColor = unselectedTextColor;

                return new List<int>();
            }

            Regex xpr = new Regex(Regex.Escape(searchText), RegexOptions.IgnoreCase);
            return RichtextHighlight(richTextBox, xpr, unselectedTextColor, highlightTextFont, highlightColor, highlightBackColor);
        }

        /// <summary>
        /// Highlight the text in the richtextbox and return the list of indexes
        /// </summary>
        /// <param name="richTextBox">The rich text box.</param>
        /// <param name="searchExpression">The search expression.</param>
        /// <param name="unselectedTextColor">Color of the unselected text.</param>
        /// <param name="highlightTextFont">The highlight text font.</param>
        /// <param name="highlightColor">Color of the highlight.</param>
        /// <param name="highlightBackColor">Color of the highlight back.</param>
        /// <returns></returns>
        public static List<int> RichtextHighlight(RichTextBox richTextBox, Regex searchExpression, 
            Color unselectedTextColor, Font highlightTextFont, 
            Color highlightColor, Color highlightBackColor)
        {
            try
            {
                richTextBox.SuspendLayout();
                MatchCollection matches = searchExpression.Matches(richTextBox.Text);
                var indexes = new List<int>(matches.Count);

                richTextBox.Text = richTextBox.Text;
                richTextBox.ForeColor = unselectedTextColor;

                if (matches.Count > 0)
                {
                    foreach (Match m in matches)
                    {
                        int index = m.Index;
                        indexes.Add(m.Index);

                        richTextBox.SelectionStart = index;
                        richTextBox.SelectionLength = m.Value.Length;

                        richTextBox.SelectionColor = highlightColor;
                        richTextBox.SelectionFont = highlightTextFont;
                        richTextBox.SelectionBackColor = highlightBackColor;
                    }

                    richTextBox.SelectionStart = matches[0].Index;
                    richTextBox.SelectionLength = 0;
                    richTextBox.ScrollToCaret();
                }
                return indexes;
            }
            catch
            {
                return new List<int>();
            }
            finally
            {
                richTextBox.ResumeLayout(false);
            }
        }
        #endregion
        
        #region [ ListBox ]
        /// <summary>
        /// Remove the selected item on the listbox.
        /// </summary>
        /// <param name="listbox">The listbox.</param>
        public static void RemoveSelected(ListBox listbox)
        {
            ListBox.SelectedIndexCollection indexes = listbox.SelectedIndices;
            if (indexes.Count > 0)
            {
                for (int i = indexes.Count - 1; i >= 0; i--)
                {
                    listbox.Items.RemoveAt(indexes[i]);
                }
            }
        }

        /// <summary>
        /// Select item by right click on the listbox (attach this on MouseDown).
        /// </summary>
        /// <param name="listbox">The listbox.</param>
        /// <param name="coord">The coord.</param>
        public static void ListBoxCoordSelect(ListBox listbox, Point coord)
        {
            int index = listbox.IndexFromPoint(coord.X, coord.Y);

            if (index >= 0)
            {
                listbox.SelectedIndex = -1;
                listbox.SetSelected(index, true);
            }
        }

        #region [ ListBox & ListView Navigation ]
        /// <summary>
        /// Move up the list
        /// </summary>
        /// <param name="listbox">The listbox.</param>
        /// <param name="offset">The offset (if this is set to 2 it will jumb 2 spaces).</param>
        /// <param name="loop">if set to <c>true</c> [loop].</param>
        public static void MoveUpTheList(ListBox listbox, int offset = 1, bool loop = true)
        {
            int count = listbox.Items.Count;

            if (count == 1)
            {
                listbox.SelectedIndex = 0;
            }
            else if (count > 0)
            {
                int index = listbox.SelectedIndex - offset;

                if (index < 0)
                {
                    if (loop && listbox.SelectedIndex == 0)
                    {
                        SetSingleSelection(listbox, count - 1);
                    }
                    else
                    {
                        SetSingleSelection(listbox, 0);
                    }
                }
                else
                {
                    SetSingleSelection(listbox, index);
                }
            }
        }

        /// <summary>
        /// Clear all the current selection and sets the specified index as the only selection.
        /// </summary>
        /// <param name="listbox">The listbox.</param>
        /// <param name="index">The index.</param>
        public static void SetSingleSelection(ListBox listbox, int index)
        {
            ClearSelection(listbox);
            listbox.SelectedIndex = index;
        }

        /// <summary>
        /// Clear the listbox selection.
        /// </summary>
        /// <param name="listbox">The listbox.</param>
        public static void ClearSelection(ListBox listbox)
        {
            ListBox.SelectedIndexCollection indexes = listbox.SelectedIndices;
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                listbox.SetSelected(indexes[i], false);
            }
        }

        /// <summary>
        /// Clear all the current selection and sets the specified index as the only selection.
        /// </summary>
        /// <param name="listview">The listbox.</param>
        /// <param name="index">The index.</param>
        public static void SetSingleSelection(ListView listview, int index)
        {
            ClearSelection(listview);
            listview.Items[index].Selected = true;
        }

        /// <summary>
        /// Clear the listbox selection.
        /// </summary>
        /// <param name="listview">The listbox.</param>
        public static void ClearSelection(ListView listview)
        {
            ListView.SelectedIndexCollection indexes = listview.SelectedIndices;
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                listview.Items[indexes[i]].Selected = false;
            }
        }
        /// <summary>
        /// Move down the list
        /// </summary>
        /// <param name="listbox">The listbox.</param>
        /// <param name="offset">The offset, how many step to jump when the key is pressed (default is 1).</param>
        /// <param name="loop">if set to <c>true</c> [loop].</param>
        public static void MoveDownTheList(ListBox listbox, int offset = 1, bool loop = true)
        {
            int count = listbox.Items.Count;

            if (count == 1)
            {
                listbox.SelectedIndex = 0;
            }
            else if (count > 0)
            {
                int index = listbox.SelectedIndex + offset;

                if (index >= count)
                {
                    if (loop && listbox.SelectedIndex == count - 1)
                    {
                        SetSingleSelection(listbox, 0);
                    }
                    else
                    {
                        SetSingleSelection(listbox, count - 1);
                    }
                }
                else
                {
                    SetSingleSelection(listbox, index);
                }
            }
        }

        /// <summary>
        /// Move up the list
        /// </summary>
        /// <param name="listview">The listview.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="loop">if set to <c>true</c> [loop].</param>
        public static void MoveUpTheList(ListView listview, int offset = 1, bool loop = true)
        {
            int count = listview.Items.Count;

            if (count == 1 || listview.SelectedIndices.Count == 0)
            {
                listview.Items[0].Selected = true;
            }
            else if (count > 0)
            {
                int index = listview.SelectedIndices[0] - offset;

                if (index < 0)
                {
                    if (loop && listview.SelectedIndices[0] == 0)
                    {
                        listview.Items[count - 1].Selected = true;
                    }
                    else
                    {
                        listview.Items[0].Selected = true;
                    }
                }
                else
                {
                    listview.Items[index].Selected = true;
                }
            }
        }

        /// <summary>
        /// Move down the list
        /// </summary>
        /// <param name="listview">The listview.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="loop">if set to <c>true</c> [loop].</param>
        public static void MoveDownTheList(ListView listview, int offset = 1, bool loop = true)
        {
            int count = listview.Items.Count;

            if (count == 1 || listview.SelectedIndices.Count == 0)
            {
                listview.Items[0].Selected = true;
            }
            else if (count > 0)
            {
                int index = listview.SelectedIndices[0] + offset;

                if (index >= count)
                {
                    if (loop && listview.SelectedIndices[0] == count - 1)
                    {
                        listview.Items[0].Selected = true;
                    }
                    else
                    {
                        listview.Items[count - 1].Selected = true;
                    }
                }
                else
                {
                    listview.Items[index].Selected = true;
                }
            }
        }
        #endregion
        #endregion

        #region [ ComboBox ]
        /// <summary>
        /// Select item by the display name.
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="name"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="defaultIndex"></param>
        public static void CbSelectByName(ComboBox cbo, string name, bool ignoreCase = true, int defaultIndex = 0)
        {
            bool foundIt = false;
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (Str.IsEqual(cbo.Items[i].ToString(), name, ignoreCase))
                {
                    cbo.SelectedIndex = i;
                    foundIt = true;
                    break;
                }
            }

            if (!foundIt && defaultIndex >= -1 && defaultIndex < cbo.Items.Count)
            {
                cbo.SelectedIndex = defaultIndex;
            }
        }

        public static void CbRemoveByName(ComboBox cbo, string name, bool ignoreCase = true)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (Str.IsEqual(cbo.Items[i].ToString(), name, ignoreCase))
                {
                    cbo.Items.RemoveAt(i);
                    break;
                }
            }
        }
        #endregion

        #region [ ShowContext ]
        /// <summary>
        /// Show context menu above the supplied control
        /// </summary>
        /// <param name="contextMenu">The context menu.</param>
        /// <param name="control">The control.</param>
        public static void ShowContext(ContextMenuStrip contextMenu, Control control)
        {
            contextMenu.Show(control.Parent, new Point(control.Location.X, control.Location.Y - 25 - contextMenu.Height));
        }
        #endregion

        #region [ ClearOnEscape ]
        public static void ClearOnEscape(TextBox textbox)
        {
            bool autoComplete = textbox.AutoCompleteMode != AutoCompleteMode.None;
            ClearOnEscape(textbox, autoComplete);
        }

        /// <summary>
        /// Clear the textbox when the escape key is pressed (this also avoid the "ding" noise)
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        /// <param name="singleLineOnly">In order to fix the ding noise when press escape, the textbox must be multiline
        /// but sometime this mess up certain feature. Set this to true to force the textbox to remain a single line mode.</param>
        public static void ClearOnEscape(TextBox textbox, bool singleLineOnly = false)
        {
            if (!singleLineOnly)
            {
                // Use this to get rid of the "ding" noise
                textbox.Multiline = true;
                // This to keep the text on one line
                textbox.WordWrap = false;

                // This will swallow the Enter Key
                textbox.KeyPress += (s, e) =>
                {
                    // Enter Key
                    if (e.KeyChar == Ascii.CaretReturn)
                    {
                        e.Handled = true;
                    }
                };
            }

            // This will handle the Escape Key
            textbox.KeyUp += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = true;
                    textbox.Clear();
                    textbox.Focus();
                }
            };
        }

        /// <summary>
        /// Empty the text on escape button is pressed.
        /// </summary>
        /// <param name="controls">The controls.</param>
        public static void ClearOnEscape(params Control[] controls)
        {
            foreach (var c in controls)
            {
                if (c is TextBox)
                {
                    ClearOnEscape((TextBox)c);
                    continue;
                }

                c.KeyUp += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        e.Handled = true;
                        c.Text = "";
                    }
                };
            }
        }
        #endregion

        #region [ GetInput ]
        private static FrmInput _inputForm;

        /// <summary>
        /// Gets the user input.
        /// </summary>
        /// <param name="parent">Center this screen when the dialog is shown.</param>
        /// <param name="prompt">The prompt.</param>
        /// <param name="title">The title.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="cancelValue">Return this value when the cancel button is pressed.</param>
        /// <returns></returns>
        public static string GetInput(Form parent, string prompt, string title = "Input", 
            string initialValue = "", string cancelValue = "")
        {
            if (_inputForm == null)
            {
                _inputForm = new FrmInput();
                _inputForm.StartPosition = FormStartPosition.CenterParent;
            }

            _inputForm.Text = title;
            _inputForm.Prompt = prompt;
            _inputForm.Input = initialValue;

            if (_inputForm.ShowDialog(parent) == DialogResult.OK)
            {
                return _inputForm.Input;
            }
            return cancelValue;
        }
        #endregion

        #region [ Wait/Resume On Process Load ]
        /// <summary>
        /// Disable the form and set cursor to wait.
        /// </summary>
        /// <param name="form">The form.</param>
        public static void ShowWaitOnLoad(Form form)
        {
            form.Refresh();
            form.Enabled = false;
            form.Cursor = Cursors.WaitCursor;

        }

        /// <summary>
        /// Enable to true and set cursor to default.
        /// </summary>
        /// <param name="form">The form.</param>
        public static void ResumeFromLoad(Form form)
        {
            form.Enabled = true;
            form.Refresh();
            form.Cursor = Cursors.Default;
        }
        #endregion

        #region [ Windows UI ]
        /// <summary>
        /// Open file Explorers and highlight the item.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void ExplorerHighlight(string path)
        {
            if (!IOFunc.IsFile(path))
            {
                path = IOFunc.PathFinder(path);
            }

            IOFunc.RunCmd("Explorer.exe", string.Format("/select ,\"{0}\"", path));
        }
        #endregion

        /// <summary>
        /// Clears the format of the richtextbox and restore the text selection and position.
        /// </summary>
        /// <param name="richTextBox">The RCHTXT list.</param>
        public static void ClearFormat(RichTextBox richTextBox)
        {
            try 
	        {
                richTextBox.SuspendLayout();

                var start = richTextBox.SelectionStart;
                var length = richTextBox.SelectionLength;

                richTextBox.Text = richTextBox.Text;
                richTextBox.SelectAll();
                richTextBox.SelectionColor = richTextBox.ForeColor;
                richTextBox.SelectionFont = richTextBox.Font;
                richTextBox.SelectionBackColor = richTextBox.BackColor;

                if (start != 0 && length != 0)
                {
                    richTextBox.SelectionStart = start;
                    richTextBox.SelectionLength = length;

                    UI.ScrollTo(richTextBox, start, true);
                }
	        }
	        finally
	        {
                richTextBox.ResumeLayout(true);
	        }
        }
    }
}
