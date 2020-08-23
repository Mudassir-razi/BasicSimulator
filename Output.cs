using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grid;

namespace ResourceManager
{
    /// <summary>
    /// Used for easy file I/O
    /// </summary>
    internal class Output
    {
        StreamWriter _fileWriter;
        string _filePath;
        bool fileOpened;

        public Output(string path)
        {
            _filePath = path;
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
            _fileWriter = File.CreateText(_filePath);
            fileOpened = true;
            SystemGrid.Informer.SimulationEnded += FileClose;
            SystemGrid.Informer.SimulationStarted += Resimulate;
            SystemGrid.Informer.SimulationCycleEnd += OutputInfo;
        }

        void Createfile()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
            _fileWriter = File.CreateText(_filePath);
        }

        void OutputInfo(object sender, string s)
        {
            _fileWriter.WriteLine(s);
        }

        void FileClose(object sender, string s)
        {
            _fileWriter.Close();
            fileOpened = false;
        }

        void Resimulate(object sender, string s)
        {
            if(!fileOpened)Createfile();
        }
    }
}
