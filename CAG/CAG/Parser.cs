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
using System.Text.RegularExpressions;

namespace CAG
{
    /// <summary>
    /// Class for parsing text from protocol CAS+ and writing to file DOT
    /// </summary>
    class Parser : MyFile
    {
        /// <summary>
        /// Name of file with DOT language. This file is temporary because
        /// it serves for showing attack graph.
        /// </summary>
        private string nameDotFile;

        /// <summary>
        /// Name of file with DOT language. This file is newer than nameDotFile
        /// because parsing session_instancies add new changes to it.
        /// </summary>
        private string nameDotFile1;

        /// <summary>
        /// Stream for writing to file with DOT language.
        /// </summary>
        private StreamWriter writeDot;

        /// <summary>
        /// List of knowledges which are in protocol CAS+ in third part.
        /// </summary>
        private List<string> knowledges;

        /// <summary>
        /// Count of files with DOT. In fourth part (session_instancies) can be more 
        /// session --> more graphs.
        /// </summary>
        private int countDotFiles = 0;

        /// <summary>
        /// Enum for type of key. None means that type isn't known. 
        /// </summary>
        private enum typKey { PUBLIC, PRIVATE, SYMMETRIC, NONE };

        /// <summary>
        /// Map of keys, where key is string and value is type of key. 
        /// Protocol CAS+ can contain more keys and different type.
        /// </summary>
        private Dictionary<string, typKey> keys = new Dictionary<string, typKey>();

        /// <summary>
        /// Count of done encrypt and converse function. It serves for numbering encrypt functions.
        /// </summary>
        private int countEncrypt = 0, countConverse = 0;

        /// <summary>
        /// Dictionary of name of function and his number.
        /// </summary>
        private Dictionary<string, int> numberOfFunction = new Dictionary<string, int>();

        /// <summary>
        /// Constructor which saves name of protocol and name of file with DOT.
        /// </summary>
        /// <param name="nameProtocol">Name of file containing protocol CAS+</param>
        /// <param name="nameDotF">Name of file containing DOT language</param>
        public Parser(string nameProtocol, string nameDotF) : base(nameProtocol) { nameDotFile = nameDotF;}

        /// <summary>
        /// Method reads all lines from protocol CAS+ and opens stream for writing to file with DOT.
        /// Then method calls parsing lines.
        /// </summary>
        /// <returns>Count of files with DOT</returns>
        public int parse()
        {
            if (!existFile() || !openFile())
                return -1;
            
            File.WriteAllText(nameDotFile, string.Empty);
            writeDot = new StreamWriter(nameDotFile, true);
            lines = new List<string>(File.ReadAllLines(nameFile));
            
            // Name of file with CAS+ is name of diagraph.
            // Those lines split full path by '\' and take last part.
            List<string> parts = new List<string>(nameFile.Split('\\'));
            
            string name = parts[parts.Count -1];
            name = name.Remove(name.Length - 4);
            name = name.Replace(" ", "");
            
            writeDot.WriteLine("digraph " + name + "{");
                
            bool throwException = false;
            try
            {
                parseLines();
            }
            catch (System.IndexOutOfRangeException)
            {
                throwException = true;
            }
            catch (Exception ex)
            {
                throwException = true;
                throw ex;
            }
            finally
            {
                writeDot.Close();
                StreamReader fm = new StreamReader(nameDotFile);
                fm.Close();
                File.Delete(nameDotFile);
                if (throwException)
                {
                    for (int j = 1; j <= countDotFiles; j++)
                    {
                        File.Delete(nameDotFile.Insert(nameDotFile.Length - 4, j.ToString()));
                    }
                    countDotFiles = -2;
                }
            }
            File.Delete(nameDotFile);
            return countDotFiles;
        }

        /// <summary>
        /// Method saves lines until get to other section ("identifiers", "messages",...)
        /// If method gets to new section, calls method for parsing saving lines by type of section.
        /// </summary>
        private void parseLines()
        {
            int section = 0;
            string[] parts = { "identifiers", "messages", "knowledge", 
                                 "session_instances", "intruder_knowledge",
                                 "goal"};
            List<string> descriptions = new List<string>();

            foreach (string line in lines)
            {
                if(parts.Any(item => line.Contains(item)))
                {
                    string compress = line;
                    if (line.Contains('%'))
                        compress = line.Remove(line.IndexOf('%'));
                    compress = compress.Trim();
                    switch(compress)
                    {
                        case "identifiers":
                            section++;
                            break;
                        case "messages":
                            //bad format in CAS+, previous section was forgotten
                            if (section != 1)
                                throw new IndexOutOfRangeException();
                            parseIdentifirers(descriptions);
                            section++;
                            break;
                        case "knowledge":
                            if (section != 2)
                                throw new IndexOutOfRangeException();
                            parseMessage(descriptions);
                            writeDot.Close();
                            section++;
                            break;
                        case "session_instances":
                            if (section != 3)
                                throw new IndexOutOfRangeException();
                            knowledges = new List<string>(descriptions);
                            section++;
                            break;
                        case "intruder_knowledge":
                            if (section != 4)
                                throw new IndexOutOfRangeException();
                            parseSession(descriptions);
                            section++;
                            break;
                        case "goal":
                            if (section != 5)
                                throw new IndexOutOfRangeException();
                            parseKnowledgeIntruder(descriptions);
                            section++;
                            break;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                    descriptions.Clear();
                    continue;
                }
                string lin = line.Trim();
                if (!String.IsNullOrEmpty(lin) && lin[0] != '%')
                {
                    if (section > 1 && lin.Contains('%'))
                        lin = lin.Remove(lin.IndexOf('%'));
                    descriptions.Add(lin);
                }
                
            }
        }
        /// <summary>
        /// Method parses section "identifiers". 
        /// </summary>
        /// <param name="descriptions">Lines containing variable and their types.</param>
        private void parseIdentifirers(List<string> descriptions)
        {
            foreach (string line in descriptions)
            {
                HashSet<string> finishProperties = new HashSet<string>();

                // Line for example: A, B : user
                // First line is divided by ':', then ','. 
                string[] byColon = line.Split(new char[] {':'},2);
                string[] singleNodes = byColon[0].Split(',');
                string[] typeAndEntropy = byColon[1].Split(new char[] {';'}, 2);
                string[] singleEntropy = { "" };

                // After "%" user can set entropy. 
                if(!string.IsNullOrEmpty(typeAndEntropy[1]))
                {
                    if (!(typeAndEntropy[1].Contains(':') || typeAndEntropy[1].Contains("<-")))
                        typeAndEntropy[1] = typeAndEntropy[1].Remove(0);
                    else
                    {
                        singleEntropy = typeAndEntropy[1].Split(';');
                        singleEntropy[0] = singleEntropy[0].Replace("%", string.Empty);
                        singleEntropy[0] = singleEntropy[0].Replace(" ", string.Empty);
                    }
                }
                for (int i = 0; i < singleNodes.Length; i++)
                {
                    singleNodes[i] = singleNodes[i].Trim();
                    
                    string output = singleNodes[i];
                    if (typeAndEntropy[0].ToLower().Contains("public_key"))
                    {
                        keys.Add(singleNodes[i], typKey.PUBLIC);
                        output += "_pub";
                    }
                    else if (typeAndEntropy[0].ToLower().Contains("private_key"))
                    {
                        keys.Add(singleNodes[i], typKey.PRIVATE);
                        output += "_priv";
                    }
                    else if (typeAndEntropy[0].ToLower().Contains("symmetric"))
                        keys.Add(singleNodes[i], typKey.SYMMETRIC);
                    else if (typeAndEntropy[0].ToLower().Contains("key"))
                        keys.Add(singleNodes[i], typKey.NONE);

                    if (typeAndEntropy[0].Contains("function"))
                    {
                        numberOfFunction.Add(output.Trim(), 1);
                        output += "1";
                    }
                    output += "[type=" + typeAndEntropy[0];
                    if(!string.IsNullOrEmpty(singleEntropy[0]))
                    {
                        List<int> indexs = new List<int>();
                        int k = 0;
                        while (k < singleEntropy.Length)
                        {
                            if (!finishProperties.Contains(singleEntropy[k]) && singleEntropy[k].Contains(singleNodes[i]))
                            {
                                indexs.Add(k);
                                finishProperties.Add(singleEntropy[k]);
                            }
                            k++;
                        }
                        string[] partEntropy;

                        foreach (int j in indexs)
                        {
                            if (singleEntropy[j].Contains("<-"))
                            {
                                partEntropy = singleEntropy[j].Split(new string[] { "<-" }, StringSplitOptions.None);
                                List<string> message = new List<string>();
                                message.Add(partEntropy[1]);
                                parsePartMessage(message);
                                countConverse++;
                                writeDot.WriteLine(partEntropy[1] + "-> converse" + countConverse.ToString() + ";");
                                writeDot.WriteLine("converse" + countConverse.ToString() + " -> " + partEntropy[0] + ";");
                                writeDot.WriteLine("converse" + countConverse.ToString() + "[type=function];");
                            }
                            else
                            {
                                partEntropy = singleEntropy[j].Split(new char[] { ':', ',' });
                                if (partEntropy[1].Contains("bit"))
                                {
                                    output += ",bit=";
                                    partEntropy[1] = Regex.Replace(partEntropy[1], "[^0-9.]", "");

                                }
                                else
                                {
                                    output += ",set=";
                                }
                                output += partEntropy[1];
                                if (partEntropy.Length > 2)
                                {
                                    output += ",set=";
                                    output += partEntropy[2];
                                }
                            }
                        }
                    }
                    output += "];";
                    writeDot.WriteLine(output);
                }
            }
        }

        /// <summary>
        /// Method does cycle of lines containing second part - Message
        /// and calls parsing of single line.
        /// </summary>
        /// <param name="descriptions">Lines containing messages between users</param>
        private void parseMessage(List<string> descriptions)
        {
            foreach (string line in descriptions)
            {
                List<string> twoSubstrings = new List<string>(line.Split(new char[] { ':' }, 2));
                if (twoSubstrings.Count > 1 && !String.IsNullOrEmpty(twoSubstrings[0]) && !String.IsNullOrEmpty(twoSubstrings[1]))
                {
                    List<string> parts = new List<string>(splitComma(twoSubstrings[1]));
                    parsePartMessage(parts);
                }
                else
                    throw new System.IndexOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Method removes commas in string and divided it. If comma is in bracket, comma won't be removed.
        /// </summary>
        /// <param name="line">Line where will be removes comma.</param>
        /// <returns>Line divided by comma</returns>
        private List<string> splitComma(string line)
        {
            int bracket = 0;
            List<string> splitString = new List<string>();
            string part = "";
            foreach (char c in line)
            {
                // adding char to string
                bool add = true;
                switch (c)
                {
                    case ',':
                        if (bracket == 0)
                        {
                            splitString.Add(part);
                            part = part.Remove(0);
                            add = false;
                        }
                        break;
                    case '[':
                    case '(':
                    case '{':
                        bracket++;
                        break;
                    case '}':
                    case ']':
                    case ')':
                        bracket--;
                        break;
                }
                if (add)
                    part += c;

            }
            splitString.Add(part);
            return splitString;
        }
        /// <summary>
        /// Method parses single line.
        /// </summary>
        /// <param name="parts">Line which contains part of message with contents of transfer.</param>
        private void parsePartMessage(List<string> parts)
        {
            Stack<int> indexs = new Stack<int>();
            for(int m = 0; m < parts.Count; m ++)
            {
                for (int i = 0; i < parts[m].Length; i++)
                {
                    switch (parts[m][i])
                    {
                        case '{':
                        case '(':
                        case ',':
                            indexs.Push(i);
                            break;
                        case '}':
                            int beginArgument1 = indexs.Pop();
                            while (parts[m][beginArgument1] != '{')
                            {
                                beginArgument1 = indexs.Pop();
                            }
                            string line = "{";
                            line += parts[m].Substring(beginArgument1 + 1, i - beginArgument1 - 1);
                            int k = i;

                            int indexKey = i++;
                            while ((i < parts[m].Length) && ((parts[m][i] != ',') && parts[m][i] != ')' && parts[m][i] != '}'))
                                i++;

                            string key = parts[m].Substring(indexKey + 1, i - indexKey - 1).Trim();

                            if(! (key.Contains("_pub") || key.Contains("_priv")))
                            {
                                if (key[key.Length - 1] == '\'')
                                {
                                    key = key.Remove(key.Length - 1);
                                    switch (keys[key])
                                    {
                                        case typKey.PUBLIC:
                                            key += "_priv";
                                            break;
                                        case typKey.PRIVATE:
                                            key += "_pub";
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (keys[key])
                                    {
                                        case typKey.PRIVATE:
                                            key += "_priv";
                                            break;
                                        case typKey.PUBLIC:
                                            key += "_pub";
                                            break;
                                    }
                                }
                            }
                            
                            line += "," + key;

                            line += "} -> encrypt" + countEncrypt.ToString() + ";";

                            string secondLine = "encrypt" + countEncrypt.ToString() + " -> E" + countEncrypt.ToString() + "(";
                            secondLine += line.Substring(1, line.IndexOf('}') - 1);
                            secondLine += ");";

                            writeDot.WriteLine("E" + countEncrypt.ToString() + "(" + line.Substring(1, line.IndexOf('}') - 1) + ")[type=data];");
                            writeDot.WriteLine("encrypt" + countEncrypt.ToString() + "[type=function];");

                            // symmetric vs asymmetric keys
                            if (keys.ContainsKey(key) && keys[key] == typKey.SYMMETRIC)
                            {
                                writeDot.WriteLine("encrypt" + countEncrypt.ToString() +
                                    " -> {" + parts[m].Substring(beginArgument1 + 1, i - beginArgument1 - 1));
                                writeDot.WriteLine("E" + countEncrypt.ToString() + "("
                                    + line.Substring(1, line.IndexOf('}') - 1)
                                    + ") -> encrypt" + countEncrypt.ToString());
                            }

                            
                            writeDot.WriteLine(line);
                            writeDot.WriteLine(secondLine);

                            int countCipher = 1;
                            if(countEncrypt > 0)
                                countCipher = Convert.ToInt32(Math.Floor(Math.Log10(countEncrypt) + 1));
                            writeDecrypt(key, secondLine.Substring(11 + countCipher
                                , secondLine.Length - 12 - countCipher), countEncrypt);

                            string replace = "E" + countEncrypt.ToString() + "(" + line.Substring(1, line.IndexOf('}') - 1) + ")";
                            parts[m] = parts[m].Replace(parts[m].Substring(beginArgument1, i - beginArgument1),
                                replace);
                            countEncrypt++;
                            i = beginArgument1 + replace.Length; 
                            
                            if ((i+1 == parts[m].Length) && (parts[m][i] == ')' || parts[m][i] == '}'))
                                i--;
                            break;
                        case ')':
                            int beginArgument2 = indexs.Pop();
                            while (parts[m][beginArgument2] != '(')
                            {
                                beginArgument2 = indexs.Pop();
                            }

                            string first = "{";
                            first += parts[m].Substring(beginArgument2 + 1, i - beginArgument2 - 1);
                            first += "} -> ";
                            int beginFunction = -1;
                            if(indexs.Count != 0)
                                beginFunction = indexs.Pop();
                            string nameFunction = parts[m].Substring(beginFunction + 1, beginArgument2 - beginFunction - 1).Trim();
                            first += nameFunction;
                            if (numberOfFunction.ContainsKey(nameFunction))
                            {
                                first+= numberOfFunction[nameFunction];
                            }
                            first += ";";

                            string second = nameFunction ;
                            if (numberOfFunction.ContainsKey(nameFunction))
                            {
                                second+= numberOfFunction[nameFunction];
                                writeDot.WriteLine(nameFunction + numberOfFunction[nameFunction] + "[type=function];");
                                numberOfFunction[nameFunction]++;
                            }
                            second += " -> ";
                            second += parts[m].Substring(beginFunction + 1, i - beginFunction) + ";";

                            writeDot.WriteLine(parts[m].Substring(beginFunction + 1, i - beginFunction) + "[type=data];");

                            writeDot.WriteLine(first);
                            writeDot.WriteLine(second);
                            
                            break;
                        default:
                            break;
                    }
                }
                indexs.Clear();
            }

        }

        /// <summary>
        /// Method writes to DOT decryption function by key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="input">Input parameter to decryption</param>
        /// <param name="j">Number of decrypt function</param>
        private void writeDecrypt(string key, string input, int j)
        {
            string line = "{";
            string pomKey = key;
            bool found = true;
            if (!keys.ContainsKey(key))
            {
                if (key.Contains("_pub"))
                    pomKey = key.Remove(key.Length - 4);
                else if (key.Contains("_priv"))
                    pomKey = key.Remove(key.Length - 5);
                if (!keys.ContainsKey(pomKey))
                    found = false;
               
            }
            if (found)
            {
                switch (keys[pomKey])
                {
                    case typKey.NONE:
                    case typKey.SYMMETRIC:
                        line += key + ",";
                        break;

                    case typKey.PUBLIC:
                    case typKey.PRIVATE:
                        if (key.Contains("_pub"))
                        {
                            key = key.Remove(key.Length - 4);
                            line += key + "_priv,";
                            writeDot.WriteLine(key + "_priv[type=private_key];");
                        }
                        else
                        {
                            key = key.Remove(key.Length - 5);
                            line += key + "_pub,";
                            writeDot.WriteLine(key + "_pub[type=public_key];");
                        }
                        break;
                }

                line += input + "} -> decrypt" + j.ToString() + ";";

                writeDot.WriteLine("decrypt" + j.ToString() + "[type=function];");
                writeDot.WriteLine(line);

                string output = "decrypt" + j.ToString() + " -> ";

                string parameters = input.Substring(input.IndexOf('(') + 1, input.LastIndexOf(')') - input.IndexOf('(') - 1);
                List<string> parts = new List<string>();
                int countBrackets = 0;
                string p = "";
                foreach (char c in parameters)
                {
                    if (c == ',')
                    {
                        if (countBrackets == 0)
                        {
                            parts.Add(p);
                            p = "";
                        }
                        else p += c;
                    }
                    else
                    {
                        if (c == '(')
                            countBrackets++;
                        else if (c == ')')
                            countBrackets--;
                        p += c;
                    }

                }
                if (!String.IsNullOrEmpty(p))
                {
                    parts.Add(p);
                }
                foreach (string part in parts)
                {
                    if (!part.Contains(key))
                        writeDot.WriteLine(output + part + ";");
                }
            }
        }

        /// <summary>
        /// Method parses fourth part - Session_instancies
        /// </summary>
        /// <param name="descriptions">Lines in fourth part</param>
        private void parseSession(List<string> descriptions)
        {
            List<string> dot = new List<string>(File.ReadAllLines(nameDotFile));
            
            int i = 0;
            foreach (string line in descriptions)
            {
                if (String.IsNullOrEmpty(line))
                    continue;
                i++;
                List<string> dotPom = new List<string>(dot);
                
                List<string> parts = new List<string>(line.Split(new char[] { ',', '[', ']' }));
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                foreach (string part in parts)
                {
                    List<string> pair = new List<string>(part.Split(':'));
                    if(pair.Count > 1)
                        pairs.Add(pair[0], pair[1]);
                }

                nameDotFile1 = nameDotFile.Insert(nameDotFile.Length - 4, i.ToString());
                if(!File.Exists(nameDotFile1))
                {
                    FileStream fs = File.Create(nameDotFile1);
                    fs.Close();
                }
                File.WriteAllText(nameDotFile1, string.Empty);

                for(int j =0; j <dot.Count; j++)
                {
                    foreach(string key in pairs.Keys)
                    {
                        if (dot[j].Contains(key))
                        {
                            if (dot[j].Contains('['))
                            {
                                if (dot[j].IndexOf(key) < dot[j].IndexOf('['))
                                {
                                    var regex = new Regex(Regex.Escape(key));
                                    dotPom[j] = regex.Replace(dotPom[j], pairs[key], 1);
                                }
                            }
                            else
                            {
                                dotPom[j] = dotPom[j].Replace(key, pairs[key]);
                            }
                        }
                    }

                }
                File.WriteAllLines(nameDotFile1, dotPom);
            }
            countDotFiles = i;
        }
        /// <summary>
        /// Method parse lines in fifth part.
        /// </summary>
        /// <param name="descriptions">Lines containing knowledge of attacker</param>
        private void parseKnowledgeIntruder(List<string> descriptions)
        {
            foreach (string line in descriptions)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    List<string> compromised = new List<string>();
                    string[] parts = line.Split(',');
                    parts[parts.Length - 1] = parts[parts.Length - 1].Replace(";", "");
                    for (int i = 0; i < parts.Length; i++)
                    {
                        compromised.Add(parts[i] + "[color=red];");
                    }
                    compromised.Add("}");
                    for (int i = 1; i <= countDotFiles; i++)
                    {
                        File.AppendAllLines(nameDotFile.Insert(nameDotFile.Length - 4, i.ToString()), compromised);
                    }
                    
                }
            }
        }
    }
}
