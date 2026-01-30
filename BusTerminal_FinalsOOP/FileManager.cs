using System;
using System.Collections.Generic;
using System.IO;

namespace BusTerminal_FinalsOOP
{
    public class FileManager
    {
        private List<string> lines = new List<string>();
        private string filePath = null;
        private bool status = false;

        public FileManager(string path) { filePath = path; status = Read(); }
        public List<string> getLines() { return lines; }
        public bool getStatus() { return status; }
        public bool Read()
        {
            lines = new List<string>();
            if (!File.Exists(filePath)) return false;
            try { using (StreamReader sr = new StreamReader(filePath)) { string l; while ((l = sr.ReadLine()) != null) lines.Add(l); } return true; }
            catch { return false; }
        }
        public void Write(List<string> content, bool append = true)
        {
            if (append) lines.AddRange(content); else lines = content;
            using (StreamWriter sw = new StreamWriter(filePath, false)) { foreach (string l in lines) sw.WriteLine(l); }
        }
    }
}