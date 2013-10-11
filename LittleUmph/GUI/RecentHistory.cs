using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace LittleUmph
{
    public class RecentHistory
    {
        private string _FilePath;
        private int _MaxCount;
        private ComboBox _ComboBox;

        public RecentHistory(ComboBox cbo, int maxCount)
        {
            _ComboBox = cbo;
            _MaxCount = maxCount;

            _FilePath = GetFilePath(cbo.Name);

            RefreshList();
        }

        private string GetFilePath(string name)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IsoToX360\\";
            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }

            return appData + name + ".txt";
        }

        public void RefreshList()
        {
            if (File.Exists(_FilePath))
            {
                string[] lines = File.ReadAllLines(_FilePath);
                _ComboBox.Items.AddRange(lines);
            }
            else
            {
                _ComboBox.Items.Clear();
            }
        }

        public void AddEntry(string entry)
        {
            int index = _ComboBox.Items.IndexOf(entry);
            if (index != -1)
            {
                _ComboBox.Items.RemoveAt(index);
            }

            _ComboBox.Items.Insert(0, entry);
            _ComboBox.SelectedIndex = 0;

            string data = "";
            for (int i = 0; i < _ComboBox.Items.Count && i < _MaxCount; i++)
            {
                data += _ComboBox.Items[i].ToString() + "\r\n";
            }
            File.WriteAllText(_FilePath, data);
        }
    }
}
