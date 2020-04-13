using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace server
{
    class FilesUsers
    {
        Dictionary<string, string> FUtable = new Dictionary<string,string>();


        public FilesUsers()
        {
            if (File.Exists("manifest"))
            {
                StreamReader FUReader = File.OpenText("manifest");
                string line, name, login;
                char[] sep = { ':' };

                line = FUReader.ReadLine();
                while (line != null)
                {
                    name = line.Split(sep, 2)[0];
                    login = line.Split(sep, 2)[1];
                    FUtable.Add(name, login);
                    line = FUReader.ReadLine();
                }

                FUReader.Close();
            }
        }

        public bool AddFile(string name, int sizeFile, string login)
        {
            if (GetUser(name) != null)
                return false;

            string data = sizeFile.ToString("D10") + login;

            FUtable.Add(name, data);

            SaveFile();
            return true;
            
        }

        public bool DeleteFile(string name)
        {
            if (GetUser(name) == null)
                return false;

            FUtable.Remove(name);

            SaveFile();
            return true;
        }

        public string GetUser(string name)
        {
            if (FUtable.ContainsKey(name))
            {
                string s=FUtable[name];
                return s.Substring(10);
            }
            else
                return null;
        }

        public string[] GetOtherFiles(string login)
        {
            int i = 0;
            int nFiles = 0;
            string[] ret;

            foreach (KeyValuePair<string, string> pair in FUtable)
                if (GetUser(pair.Key) != login)
                    nFiles++;

            ret = new string[nFiles];

            foreach (KeyValuePair<string, string> pair in FUtable)
                if (GetUser(pair.Key) != login)
                    ret[i++] = pair.Key;

            return ret;
        }

        public string[] GetUserFiles(string login)
        {
            int i = 0;
            int nFiles = 0;
            string[] ret;

            foreach (KeyValuePair<string, string> pair in FUtable)
                if (GetUser(pair.Key) == login)
                    nFiles++;

            ret = new string[nFiles];

            foreach (KeyValuePair<string, string> pair in FUtable)
                if (GetUser(pair.Key) == login)
                    ret[i++] = pair.Key;

            return ret;
        }

        public int GetFileSize(string name)
        {
            if (FUtable.ContainsKey(name))
            {
                string s = FUtable[name];
                return Convert.ToInt32(s.Substring(0,10));
            }
            else
                return 0;
        }

        private void SaveFile()
        {
            StreamWriter FUWrite = File.CreateText("manifest");

            string line, name, data;

            foreach (KeyValuePair<string, string> pair in FUtable)
            {
                name = pair.Key;
                data = pair.Value;
                line = name + ':' + data;
                FUWrite.WriteLine(line);
            }

            FUWrite.Close();
        }

    }
}
