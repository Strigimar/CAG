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
    /// Class for reading from file with DOT.
    /// </summary>
    class Reader : MyFile
    {
        /// <summary>
        /// Save name of file.
        /// </summary>
        /// <param name="nameFile">Name of file with DOT language</param>
        public Reader(string nameFile) : base(nameFile) { }

        /// <summary>
        /// Method reads from file and save to list of string. 
        /// </summary>
        /// <returns>It returns -1 if file doesn't exist, cannot open or cannot read. It returns -2, if format in DOT is not correct. 
        /// Otherwise 0.</returns>
        public int readFile()
        {
            if (!existFile() || !openFile())
                return -1;

            // read all lines from file to list of lines.
            lines = new List<string>(File.ReadAllLines(nameFile));

            // add quote around words which contain parenthesis, 
            // executable file dot.exe can't take parenthesis as names.
            lines = new List<string>(addRemoveQuote(lines, true));

            // start process dot.exe with input lines from file
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            string start = Environment.CurrentDirectory;
            process.StartInfo.FileName = "dot.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.Start();
            StreamWriter strWrit = process.StandardInput;
            foreach (string line in lines)
                strWrit.Write(line);
            strWrit.Close();
            string str = process.StandardOutput.ReadToEnd();

            if (String.IsNullOrEmpty(str))
                return -2;

            // Remove white spaces and remove quotes around words which contain parenthesis.
            lines = new List<string>(ExceptWhiteSpaces(str));
            lines = new List<string>(addRemoveQuote(lines, false));

            return 0;
        }

        /// <summary>
        /// Method parses lines from file to single nodes and edges.
        /// </summary>
        /// <param name="gr">Reference on graph which will edit its atributes</param>
        /// <returns>-1 if bad format, 0 if OK</returns>
        public int parseLines(ref Graph gr)
        {
            // first line contain digraph and name of graph.
            List<string> firstLine = new List<string>(lines[0].Split(' '));
            gr.Name = firstLine[1];

            // first three line can be left because they won't be need.
            try
            {
                for (int i = 3; i < lines.Count; i++)
                {
                    parseLine(lines[i], ref gr);
                }
            }
            catch (System.NullReferenceException)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Method parse line and save to node or edge by characteristic of line.
        /// </summary>
        /// <param name="line">One line from file</param>
        /// <param name="gr">Reference on graph</param>
        private void parseLine(string line, ref Graph gr)
        {
            
            string token = "";
            Node node1 = null, node2 = null;
            
            // value which tells if edge will be created
            bool direct = false;

            for (int j = 0; j < line.Count(); j++)
            {
                if (line.Count() == 1)
                    continue;
                string s = "";
                if ((j + 1) != line.Count())
                {
                    s += line[j];
                    s += line[j + 1];
                }
                if (s.Equals("->"))
                {
                    gr.setNode(token, ref node1, node2, direct);
                    node2 = node1;
                    token = "";
                    j++;
                    direct = true;
                }
                else
                {
                    switch (line[j])
                    {
                            // start of properties of node or edge
                        case '[':
                            gr.setNode(token, ref node1, node2, direct);
                            
                            token = token.Remove(0);
                            break;

                            // end of properties of node or edge
                        case ']':
                            token += line[j];
                            if (!direct)
                            {
                                node1.Bit = 128;
                                node1 = (Node)parseProperty(token, node1);
                                if ((node1.Type == "function") ||
                                    (String.IsNullOrEmpty(node1.Type) && 
                                    (node1.containSub("hmac") || node1.containSub("encrypt") || 
                                    node1.containSub("hash") || node1.containSub("prf"))))
                                    gr.addFunction(ref node1);
                            }
                            else
                            {
                                Edge ed = new Edge(node2, node1);
                                ed = (Edge)parseProperty(token, ed);
                                gr.addEdge(ed);
                            }
                            break;
                        default:
                            token += line[j];
                            break;
                    }
                }


            }
        }
        /// <summary>
        /// Method parses properties of node or edge.
        /// </summary>
        /// <param name="prop">String containing property of node, will be parsed</param>
        /// <param name="unit">Node or edge</param>
        /// <returns>Unit of graph (node or edge) containing new changes from parsing string</returns>
        private UnitGraph parseProperty(string prop, UnitGraph unit)
        {
            string part = "";
            string attribute = "";
            string value = "";
            bool quotes = false;
            foreach (char it in prop)
            {
                switch (it)
                {
                    case '=':
                        attribute = part;
                        part = part.Remove(0);
                        break;
                    case '\"':
                        quotes = !quotes;
                        break;
                    case ']':
                    case ',':
                        if (!quotes)
                        {
                            value = part;
                            if (attribute.Equals("color"))
                            {
                                switch (value)
                                {
                                    case "red":
                                        unit.Compromised = Compromise.EASY;
                                        break;
                                    case "orange":
                                        unit.Compromised = Compromise.HARD;
                                        break;
                                    case "green":
                                        unit.Compromised = Compromise.IMPOSSIBLE;
                                        break;
                                }
                                unit.Color = value;
                            }
                            if (attribute.Equals("type"))
                                unit.Type = value;
                            if (attribute.Equals("pos"))
                            {
                                List<string> numbers = new List<string>(value.Split(','));
                                if (numbers.Count == 2)
                                {
                                    numbers[0] = numbers[0].Replace('.', ',');
                                    numbers[1] = numbers[1].Replace('.', ',');
                                    Vertex v = unit.V;
                                    v.X = Double.Parse(numbers[0]);
                                    v.Y = Double.Parse(numbers[1]);
                                    unit.V = v;
                                }
                                else
                                {
                                    string newValue = value.Replace("\r", "" );
                                    newValue = newValue.Replace("\n", "");
                                    newValue = newValue.Replace("\\", "");

                                    // line for position of edge is in format: 
                                    // "e, end coordinates, start coordinates, coordinates of vertices on path, always step by step"
                                    numbers = new List<string>(newValue.Split(' '));
                                    numbers[0] = numbers[0].Remove(0, 2);
                                    numbers.RemoveAll(x => String.IsNullOrEmpty(x));
                                    List<Vertex> vertices = new List<Vertex>();
                                    
                                    for (int i = 1; i < numbers.Count; i++)
                                    {
                                        List<string> coord = new List<string>(numbers[i].Split(','));
                                        coord[0] = coord[0].Replace('.', ',');
                                        coord[1] = coord[1].Replace('.', ',');
                                        Vertex vert = new Vertex(coord[0], coord[1]);
                                        vertices.Add(vert);
                                    }

                                    List<string> coord2 = new List<string>(numbers[0].Split(','));
                                    coord2[0] = coord2[0].Replace('.', ',');
                                    coord2[1] = coord2[1].Replace('.', ',');
                                    vertices.Add(new Vertex(coord2[0], coord2[1]));
                                    unit.Vertices = vertices;

                                }
                                
                            }
                            if (attribute.Equals("height"))
                            {
                                value = value.Replace('.', ',');
                                Vertex v = unit.V;
                                v.Height = Double.Parse(value);
                                unit.V = v;
                                
                            }
                            if (attribute.Equals("width"))
                            {
                                value = value.Replace('.', ',');
                                Vertex v = unit.V;
                                v.Width = Double.Parse(value);
                                unit.V = v;
                            }
                            if (attribute.Equals("bit"))
                            {
                                unit.Bit = Int32.Parse(value); 
                            }
                            if (attribute.Equals("set"))
                            {
                                switch (value)
                                {
                                    case "D":
                                        unit.Set = Set.Input;
                                        break;
                                    case "A":
                                        unit.Set = Set.Attack;
                                        break;
                                    case "AD":
                                    case "DA":
                                        unit.Set = Set.Both;
                                        break;
                                }
                            }
                            part = part.Remove(0);
                            attribute = attribute.Remove(0);

                        }
                        else
                            part += it;
                        break;
                    default:
                        part += it;
                        break;
                }
            }
            return unit;
        }

        /// <summary>
        /// Method adds or removes quote from words which contain parenthesis.
        /// </summary>
        /// <param name="strings">Lines from file</param>
        /// <param name="add">If true -> add, if false -> remove</param>
        /// <returns>List of strings where was added or removed quotes.</returns>
        private List<string> addRemoveQuote(List<string> strings, bool add)
        {
            List<string> newStrings = new List<string>();
            int countBracket = 0;
            bool quote = false;
            for (int i = 0; i < strings.Count; i++)
            {
                string str = strings[i];
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '(')
                    {
                        countBracket++;
                        int k = j;
                        while (( k-1 >= 0) && (Char.IsLetter(str[k-1])|| str[k-1]=='-' || str[k-1]=='_' ||Char.IsDigit(str[k-1])))
                        {
                            k--;
                        }
                        if (countBracket == 1)
                        {
                            if (add)
                            {
                                str = str.Insert(k, "\"");
                                j++;
                                
                            }
                            else
                            {
                                if (k == 0)
                                    str = str.Remove(k, 1);
                                else
                                    str = str.Remove(k - 1, 1);

                            }
                            quote = true;
                        }
                    }
                    if (str[j] == ')')
                    {
                        countBracket--;
                        if (countBracket == 0)
                        {
                            if (add)
                            {
                                str = str.Insert(++j, "\"");
                            }
                            else
                            {
                                str = str.Remove(j + 1, 1);
                            }
                            quote = false;
                        }
                    }
                    if (!quote && str[j] == '\'')
                    {
                        int k = j;
                        while ((k - 1 >= 0) && (Char.IsLetter(str[k - 1]) || str[k - 1] == '-' || str[k] == '_' || Char.IsDigit(str[k - 1])))
                        {
                            k--;
                        }
                        if (add)
                        {
                            str = str.Insert(++j, "\"");
                            str = str.Insert(k, "\"");
                        }
                        else
                        {
                            str = str.Remove(j + 1, 1);
                            if (k == 0)
                                str = str.Remove(k, 1);
                            else
                                str = str.Remove(k - 1, 1);
                        }
                    }
                }
                newStrings.Add(str);
            }
            return newStrings;
        }

        /// <summary>
        /// Method removes white spaces and divide string to more strings by ';'
        /// </summary>
        /// <param name="str">String where white spaces will be removed</param>
        /// <returns>List of string - list of lines</returns>
        public static List<string> ExceptWhiteSpaces(string str)
        {
            string newString = "";
            List<string> newStrings = new List<string>();
            bool quotes = false;
            char lastQuote = '\'';
            int i = 0;
            while (i < str.Length && str[i] != '{')
            {
                newString += str[i];
                i++;
            }
            newString += str[i];
            newStrings.Add(newString);
            newString = newString.Remove(0);
            for (i++; i < str.Length; i++)
            {
                if (!quotes || lastQuote == str[i])
                {
                    if (str[i] == '\"' || str[i] == '\'')
                    {
                        quotes = !quotes;
                        lastQuote = str[i];
                    }
                }
                if (quotes)
                    newString += str[i];
                else if (str[i] != ' ' && str[i] != '\n' && str[i] != '\r' && str[i] != '\t' && str[i] != '{' && str[i] != '}')
                    newString += str[i];
                if (str[i] == ';')
                {
                    newStrings.Add(newString);
                    newString = newString.Remove(0);
                }

            }
            return newStrings;
            

        }
    }
}
