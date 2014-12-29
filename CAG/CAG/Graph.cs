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
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CAG
{
    /// <summary>
    /// Class Graph represents attack graph with nodes and edges.
    /// </summary>
    class Graph
    {
        /// <summary>
        /// Array of string which mean some level of gray.
        /// </summary>
        private string[] colorsFunction = { "lightGray", "gray", "darkGray", "dimGray", "slateGray" };
        
        /// <summary>
        /// Name of graph.
        /// </summary>
        private string name;

        /// <summary>
        /// List of nodes of graph.
        /// </summary>
        private List<Node> nodes = new List<Node>();

        /// <summary>
        /// HashSet of edges of graph.
        /// </summary>
        private HashSet<Edge> edges = new HashSet<Edge>();

        /// <summary>
        /// List of nodes which are functions
        /// </summary>
        private List<Node> functions = new List<Node>();

        /// <summary>
        /// HashSet of compromised nodes of graph.
        /// </summary>
        private HashSet<Node> compromised = new HashSet<Node>();

        /// <summary>
        /// Value for analyse, if in analyse was found new compromited node, 
        /// so value of change is true, otherwise false
        /// </summary>
        private bool change = true;

        /// <summary>
        /// Value for searching minimal set of defined set. If searching was successful,
        /// value change to true;
        /// </summary>
        private bool foundMinSet = false;

        /// <summary>
        /// Number of found minimal set.
        /// </summary>
        private int numberOfFound = 1;

        /// <summary>
        /// Getter or setter of name of graph
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Method adds node to list of nodes.
        /// </summary>
        /// <param name="node">Node which will be added to list</param>
        public void addNode(Node node)
        {
            this.nodes.Add(node);
        }
        
        /// <summary>
        /// Method gets node by value (name)
        /// </summary>
        /// <param name="value">String name for finding node</param>
        /// <returns>Founded node or null</returns>
        public Node getNode(string value)
        {
            return this.nodes.Find(x => x.Value == value);
        }

        /// <summary>
        /// Method adds function to list of functions and also change his color on level of gray.
        /// </summary>
        /// <param name="fce">
        /// Node, which will be analysed if it is function. 
        /// If yes, it will be added to list
        /// </param>
        public void addFunction(ref Node fce)
        {
            fce.Value = fce.Value.Trim();

            // Long condition which tests if node contains type as function.
            // Otherwise also tests if node's value contains "hmac", "hash", "encrypt" on begin.
            if ((fce.Type == "function") ||
                (String.IsNullOrEmpty(fce.Type) &&
                (fce.Value.Substring(0, 4).ToLower() == "hmac" ||
                (fce.Value.Substring(0, 4).ToLower() == "hash" && (fce.Value.Length <= 4 || !fce.Value.Contains('('))) ||
                fce.Value.Substring(0, 7).ToLower() == "encrypt")))
            {
                if (fce.containSub("encrypt") || fce.containSub("decrypt"))
                {
                    fce.Color = colorsFunction[0];
                }
                else if (fce.containSub("hash"))
                {
                    fce.Color = colorsFunction[1];
                }
                else if (fce.containSub("prf"))
                {
                    fce.Color = colorsFunction[2];
                }
                else if (fce.containSub("hmac"))
                {
                    fce.Color = colorsFunction[3];
                }
                else
                {
                    fce.Color = colorsFunction[4];
                }
                if (!functions.Contains(fce))
                    functions.Add(fce);
            }
        }

        /// <summary>
        /// Method, in case no existence of node, add node to list. 
        /// If direct is true, so node adds parent.
        /// </summary>
        /// <param name="name">Name of node</param>
        /// <param name="child">Child which can be added to list or can add parent</param>
        /// <param name="parent">Parent of node</param>
        /// <param name="direct">Value which mean as if can create edge</param>
        public void setNode(string name, ref Node child, Node parent, bool direct)
        {
            if ((child = getNode(name)) == null)
            {
                child = new Node(name);
                addNode(child);

            }
            if (direct)
            {
                child.addParents(parent);
                parent.addChild(child);
            }
        }

        /// <summary>
        /// Method gets list of nodes.
        /// </summary>
        /// <returns>List of nodes</returns>
        public List<Node> getNodes() { return this.nodes; }

        /// <summary>
        /// Method gets set of edges.
        /// </summary>
        /// <returns>Set of edges</returns>
        public HashSet<Edge> getEdges() { return this.edges; }

        /// <summary>
        /// Method adds edge to set of edges.
        /// </summary>
        /// <param name="edge">Edges which will be added to set.</param>
        public void addEdge(Edge edge) { this.edges.Add(edge); }

        /// <summary>
        /// Method adds compromised node to set of compromised nodes.
        /// </summary>
        /// <param name="node">Compromise node, which will be added to set.</param>
        public void addCompromisedNode(Node node) { this.compromised.Add(node); }
        
        /// <summary>
        /// Method removes compromised node from set.
        /// </summary>
        /// <param name="node">Node will be removed from set and will be uncompromised.</param>
        public void deleteCompromisedNode(Node node) { this.compromised.Remove(node); }
        
        /// <summary>
        /// Method return hash set of compromised nodes.
        /// </summary>
        /// <returns>Set of compromised nodes.</returns>
        public HashSet<Node> getCompromiseNodes() { return this.compromised; }

        /// <summary>
        /// Method create new hash set of new compromised nodes from dictionary.
        /// </summary>
        /// <param name="newCompr">Dictionary of new compromised nodes with their compromise value.</param>
        public void setCompromiseNodes(Dictionary<Node, Compromise> newCompr)
        {
            compromised = new HashSet<Node>();
            foreach (KeyValuePair<Node, Compromise> compr in newCompr)
            {
                switch (compr.Value)
                {
                    case Compromise.EASY:
                        compr.Key.Color = "red";
                        compr.Key.Compromised = Compromise.EASY;
                        break;
                    case Compromise.HARD:
                        compr.Key.Color = "orange";
                        compr.Key.Compromised = Compromise.HARD;
                        break;
                }
                compromised.Add(compr.Key);
            }
        }
        /// <summary>
        /// Method tests if node is function, i.e. node is contained in hashSet of functions.
        /// </summary>
        /// <param name="node">Node which will be tested</param>
        /// <returns>True if hashSet of functions contains node. Otherwise false.</returns>
        public bool isFunction(Node node) { return functions.Contains(node); }
        
        /// <summary>
        /// Method analyses function for symmetric encrypt.
        /// Find out edges to and from function and what neighbours is compromited. 
        /// </summary>
        /// <param name="n">Function for encrypt</param>
        private void analyseEncrypt(ref Node n)
        {
            int bit = 0, countCompromited = 0;
            Node encryptData = new Node();
            Node unCompromised = new Node();
            string number = n.Value.Substring(7);

            foreach (Node par in n.getParents())
            {
                // For summation of entropies of uncompromised nodes the analyse doesn't want
                // to take node which is result of function. 
                // For example encrypt1 --> E1(...); where encrypt1 is function, E1(...) is child and 
                // output of function 
                if (!(par.Value.Length > 1
                    && char.ToUpper(par.Value[0]) == char.ToUpper(n.Value[0])
                    && par.Value.Substring(1, number.Length) == number))
                {
                    if (par.Compromised == Compromise.IMPOSSIBLE)
                    {
                        bit += par.Bit;
                        unCompromised = par;
                    }
                    else
                        countCompromited++;
                }
                else
                {
                    encryptData = par;
                }
                    
            }

            compareEntropy(ref n, bit);

            // Method finds out if attacker can get original data from encryption data and key -->bidirectionally.
            if (unCompromised.Compromised == Compromise.HARD)
            {
                if (encryptData.getParents().Contains(n) && encryptData.Compromised != Compromise.IMPOSSIBLE &&
                    countCompromited == n.getParents().Count - 3 && n.getChildren().Contains(unCompromised))
                {
                    compromiteNode(ref unCompromised);
                }
            }
            else
            {
                if (encryptData.getParents().Contains(n) && encryptData.Compromised != Compromise.IMPOSSIBLE &&
                    countCompromited == n.getParents().Count - 2 && n.getChildren().Contains(unCompromised))
                {
                    compromiteNode(ref unCompromised);
                }
            }
        }

        /// <summary>
        /// Method changes compromise property of node on other by string for color.
        /// Method adds or also delete node to/from list of compromised node.
        /// </summary>
        /// <param name="n">Node which will be compromised</param>
        /// <param name="color">Color of node. Default color is red.</param>
        private void compromiteNode(ref Node n, string color = "red")
        {
                // functions' color must have same color, not change.
                if (!functions.Contains(n))
                {
                    if (n.Color != color)
                        change = true;
                    switch (color)
                    {
                        case "orange":
                            n.Compromised = Compromise.HARD;
                            addCompromisedNode(n);
                            
                            break;
                        case "red":
                            n.Compromised = Compromise.EASY;
                            addCompromisedNode(n);
                            break;
                        case "green":
                            n.Compromised = Compromise.IMPOSSIBLE;
                            deleteCompromisedNode(n);
                            break;
                    }
                    n.Color = color;
                }

            

        }

        /// <summary>
        /// Method changes node on uncompromite node, i.e. color of node will be green and
        /// bool value compromised of node will be false.
        /// </summary>
        /// <param name="n">Node which will be uncompromised</param>
        private void uncompromiteNode(ref Node n)
        {
            if(!functions.Contains(n))
                n.Color = "green";
            n.Compromised = Compromise.IMPOSSIBLE;
            deleteCompromisedNode(n);
        }

        /// <summary>
        /// Method analyses one-way-function as hash, hmac, asymmetric encryption.
        /// </summary>
        /// <param name="n">Function which will be analysed</param>
        private void analyseOneWayFunction(ref Node n)
        {
            int bitImpossible = 0;
            foreach (Node par in n.getParents())
            {
                if ( par.Compromised == Compromise.IMPOSSIBLE)
                {
                    bitImpossible += par.Bit;
                }
            }
            compareEntropy(ref n, bitImpossible);
        }

        /// <summary>
        /// Method measures size of entropy
        /// and uses coloration on parents of node.
        /// </summary>
        /// <param name="n">Function which parents will be coloration.</param>
        /// <param name="bit">Total entropy of uncompromised nodes.</param>
        private void compareEntropy(ref Node n, int bit)
        {
            if (bit < 60)
                coloration(ref n, "red");
            else if (bit >= 60 && bit <= 80)
                coloration(ref n, "orange");
        }

        /// <summary>
        /// Method changes color of node and change type of compromised of node.
        /// </summary>
        /// <param name="n">Node which will be changed</param>
        /// <param name="color">Name of color - red, orange or green</param>
        private void coloration(ref Node n, string color)
        {
            foreach (Node par in n.getParents())
            {
                if ( par.Compromised == Compromise.IMPOSSIBLE)
                {
                    Node p = par;
                    compromiteNode(ref p, color);
                }
            }
            if (n.getChildren().Count > 0)
            {
                foreach (Node child in n.getChildren())
                {
                    if(child.Compromised == Compromise.IMPOSSIBLE)
                    {
                        Node ch = child;
                        compromiteNode(ref ch, color); 
                    }
                }
            }

        }

        /// <summary>
        /// General analyse for all functions. Method find out type of functions and
        /// call method on specific analyse.
        /// </summary>
        public void analyse()
        {
            foreach (Node n in nodes)
            {
                Node node = n;
                if (n.Compromised == Compromise.HARD)
                    uncompromiteNode(ref node);
            }
            change = true;
            while (change)
            {
                change = false;
                foreach (Node node in functions)
                {
                    Node temp = node;
                    if (node.containSub("encrypt"))
                        analyseEncrypt(ref temp);

                    else analyseOneWayFunction(ref temp);
                }
            }
        }

        /// <summary>
        /// Method find out if list of node is empty.
        /// </summary>
        /// <param name="source">List of node</param>
        /// <returns>If list of node is null or empty, return true. Otherwise false</returns>
        public bool isEmpty(List<Node> source)
        {
            if (source == null)
                return true;
            return !source.Any();
        }

        /// <summary>
        /// Method for creating array k-size and then calling searching all combination and
        /// finding minimal set of input.
        /// </summary>
        /// <param name="k">K-combination of set</param>
        /// <param name="input">Input set of nodes which was defined by user</param>
        /// <param name="attack">Attack set of nodes which will be interested by attacker</param>
        /// <param name="file">File for writing result</param>
        /// <param name="typeFile">Output value - type of file (png, dot)</param>
        /// <returns>Return integer value.. 2 if minimal set was found, otherwise 3.</returns>
        public int findCombination(int k, ref List<Node> input, ref List<Node> attack, ref string file, MainWindow.Output typeFile)
        {
            Node[] data = new Node[k];
            foundMinSet = false;
            numberOfFound = 1;
            int result = combinationUtil(ref input, data, attack, 0, input.Count - 1, 0, k, ref file, typeFile);
            if (result == 3 && foundMinSet)
                return 2;
            else return result;
        }

        /// <summary>
        /// Method searches recursively all k-combination of nodes of input set.
        /// </summary>
        /// <param name="input">Input set of nodes which was defined by user</param>
        /// <param name="data">Array of nodes which has k-size. There will be different k-combination</param>
        /// <param name="attack">Attack set of nodes which is interested by attacker</param>
        /// <param name="start">Start index</param>
        /// <param name="end">End index</param>
        /// <param name="index">Current index</param>
        /// <param name="k">K-combination of set</param>
        /// <param name="file">File for writing result</param>
        /// <param name="typeFile">Output value - type of file (png, dot)</param>
        /// <returns>Return integer value.. 2 if minimal set was found, otherwise 3.</returns>
        private int combinationUtil(ref List<Node> input, Node[] data, List<Node> attack, int start, int end, int index, int k, 
            ref string file, MainWindow.Output typeFile)
        {
            if (index == k)
            {
                for (int j = 0; j < k; j++)
                {
                    Node node = data[j];
                    compromiteNode(ref node);
                }
                return 1;
            }

            for (int i = start; i <= end && end - i + 1 >= k - index; i++)
            {
                Node n = input[i];
                compromiteNode(ref n);
                data[index] = input[i];
                int result = combinationUtil(ref input, data, attack, i + 1, end, index + 1, k, ref file, typeFile);
                if (result == 1)
                {
                    analyse();
                    if (findMinSet(attack))
                    {
                        uncompromiteAll();
                        for (int j = 0; j < data.Length; j++)
                        {
                            compromiteNode(ref data[j], "brown");
                        }
                        for (int j = 0; j < attack.Count; j++)
                        {
                            Node attackNode = attack[j];
                            compromiteNode(ref attackNode);
                        }

                        writeFile(ref file, typeFile);
                    }
                }
                uncompromiteAll();
                
            }
            return 3;
        }

        /// <summary>
        /// Method tests if all nodes in attack set was compromised.
        /// </summary>
        /// <param name="attack">Attack set which is interested by attacker</param>
        /// <returns>False if at least one node of attack set isn't compromised, otherwise true.</returns>
        private bool findMinSet(List<Node> attack)
        {
            foreach (Node n in attack)
            {
                if (!compromised.Contains(n))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Method changes all nodes in graph on uncompromised. 
        /// </summary>
        public void uncompromiteAll()
        {
            foreach (Node n in nodes)
            {
                Node p = n;
                uncompromiteNode(ref p);
            }
        }

        /// <summary>
        /// Method for opening dialog of saving, then create png file. 
        /// </summary>
        /// <param name="file">Name of file</param>
        private void createPngDialog(ref string file)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (*.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                file = dlg.FileName;

                Writer wr = new Writer("replacement.gv");
                wr.writeFile(this, false);

                wr.createPng(file);
            }
        }

        /// <summary>
        /// Method write graph to file.
        /// </summary>
        /// <param name="file">Name of file</param>
        /// <param name="value">Type of file (png, dot)</param>
        private void writeFile(ref string file, MainWindow.Output value)
        {
            foundMinSet = true;
            // Writing result of attack graph to file.
            switch (value)
            {
                case MainWindow.Output.PNG:
                    Writer wr1 = new Writer("replacement" + numberOfFound.ToString() + ".gv");
                    wr1.writeFile(this, false);
                    wr1.createPng(file);
                    break;
                case MainWindow.Output.DOT:
                    Writer wr2 = new Writer(file);
                    wr2.writeFile(this, true);
                    break;
                case MainWindow.Output.NOTHING:
                    if (String.IsNullOrEmpty(file))
                        createPngDialog(ref file);
                    else
                    {
                        Writer wr3 = new Writer("replacement" + numberOfFound.ToString() + ".gv");
                        wr3.writeFile(this, false);
                        wr3.createPng(file);
                    }
                    break;
            }
            if (!String.IsNullOrEmpty(file))
            {
                if (numberOfFound > 1)
                {
                    int length = numberOfFound.ToString().Length;
                    file = file.Remove(file.Length - 4 - length, length);
                }
                numberOfFound++;
                file = file.Insert(file.Length - 4, numberOfFound.ToString());
            }

        }
    }
    
}
