/*
 * Copyright © 2014, Martin Paulík
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse 
 * or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER 
 * OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; 
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
 * TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CAG
{
    /// <summary>
    /// Class for writing to file and creating png file
    /// </summary>
    class Writer : MyFile
    {
        /// <summary>
        /// Constructor saves name of file.
        /// </summary>
        /// <param name="nameFile">Name of file</param>
        public Writer(string nameFile) : base(nameFile) { }

        /// <summary>
        /// Method takes information from graph and writes it to file.
        /// </summary>
        /// <param name="gr">Graph which contain nodes and edges</param>
        /// <param name="write">Value, if is true, this graph will be written to DOT. If false, this graph will be displayed to picture</param>
        /// <returns>If file doesn't exist, can't open or can't write, return false. Otherwise true</returns>
        public bool writeFile(Graph gr, bool write)
        {
            if (!existFile())
            {
                FileStream fs = File.Create(nameFile);
                fs.Close();
            }
            if (!openFile())
                return false;

            // Create file with dot language
            if (write)
                lines = createOutput(gr, "");
            else lines = createOutput(gr, "\"");
            File.WriteAllText(nameFile, string.Empty);
            File.WriteAllLines(nameFile, lines);

            return true;
        }

        /// <summary>
        /// Method writes information about edges and nodes to list of string.
        /// </summary>
        /// <param name="gr">Graph containing information about edges and nodes</param>
        /// <param name="str">String with quotes or empty.</param>
        /// <returns>List of lines</returns>
        private List<string> createOutput(Graph gr, string str)
        {
            List<string> lin = new List<string>();
            lin.Add("digraph " + gr.Name + " {" + System.Environment.NewLine);
            foreach (Node n in gr.getNodes())
            {
                string node = "";
                if (n.Value.Contains('('))
                    node += str + n.Value + str;
                else node += n.Value;
                node += "[";//fontsize = 36,";
                if (!String.IsNullOrEmpty(n.Color))
                    node += "color=" + n.Color + ", ";
                node += "bit=" + n.Bit.ToString();

                if (n.Set != Set.Nothing)
                {
                    node += ",set=";
                    switch (n.Set)
                    {
                        case Set.Attack:
                            node += "A";
                            break;
                        case Set.Input:
                            node += "D";
                            break;
                        case Set.Both:
                            node += "AD";
                            break;
                    }
                }
                if (!String.IsNullOrEmpty(n.Type))
                {
                    node += ",type=" + n.Type;
                }
                node += "];" + System.Environment.NewLine;
                lin.Add(node);
            }
            foreach (Edge e in gr.getEdges())
            {
                string edge = "";
                if (e.FromTo.Item1.Value.Contains('('))
                    edge += str + e.FromTo.Item1.Value + str;
                else edge += e.FromTo.Item1.Value;

                edge += "->";

                if (e.FromTo.Item2.Value.Contains('('))
                    edge += str + e.FromTo.Item2.Value + str;
                else edge += e.FromTo.Item2.Value;
                
                edge += ";" + System.Environment.NewLine;
                lin.Add(edge);
            }
            lin.Add("}");
            return lin;
        }
        /// <summary>
        /// Method creates png file as picture of graph.
        /// </summary>
        /// <param name="pngFile">Name of png file</param>
        /// <returns>True if file was created. Otherwise false.</returns>
        public bool createPng(string pngFile)
        {
            // pngFile contain absolute path to file. It need to divide by '\' and 
            // take last part - only name of png file
            if (!String.IsNullOrEmpty(pngFile))
            {
                //Old png file will be removed because of test of length of new file under.
                if (File.Exists(pngFile))
                    File.Delete(pngFile);

                string start = Environment.CurrentDirectory;
                string fullPath = Path.GetFullPath(pngFile);
                start += '\\';
                System.Uri uri1 = new Uri(fullPath);
                System.Uri uri2 = new Uri(start);
                string path = uri2.MakeRelativeUri(uri1).ToString();

                System.Uri uri3 = new Uri(Path.GetFullPath(nameFile));
                string path2 = uri2.MakeRelativeUri(uri3).ToString();
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/K dot -Tpng " + path2 + " -o " + path;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.Start();

                while (!process.HasExited)
                {
                    process.Refresh();
                    System.Threading.Thread.Sleep(200);
                    if (File.Exists(pngFile))
                    {
                        FileInfo f = new FileInfo(pngFile);
                        if (f.Length > 0)
                            break;
                    }
                }
                process.Close();
                this.nameFile = fullPath;
                int i = 0;
                while (!openFile() && i < 20)
                {
                    System.Threading.Thread.Sleep(200);
                    i++;
                }           
                File.Delete(path2);
                if (i == 20)
                    return false;
                return true;
            }
            return false;

        }
    }
}
