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
using System.Drawing;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;

namespace CAG
{
    /// <summary>
    /// Class represents node of graph.
    /// </summary>
    public class Node : UnitGraph
    {

        /// <summary>
        /// Vertex of node. It contains x and y coordinates, height and width of node.
        /// </summary>
        private Vertex v = new Vertex(); 

        /// <summary>
        /// List of parents of node
        /// </summary>
        private List<Node> parents = new List<Node>();

        /// <summary>
        /// List of children of node
        /// </summary>
        private List<Node> children = new List<Node>();

        /// <summary>
        /// Property of node which says, how much node is compromised.
        /// </summary>
        private Compromise compromise = Compromise.IMPOSSIBLE;

        /// <summary>
        /// Count of bits of data.
        /// </summary>
        private int bit = 0;

        /// <summary>
        /// Type of node
        /// </summary>
        private string type;

        /// <summary>
        /// Value of enum means if node is in defined input set or set about which attacker interests.
        /// </summary>
        private Set set;

        /// <summary>
        /// Constructor without parametres, do nothing.
        /// </summary>
        public Node() : base("") { this.Color = "green"; }

        /// <summary>
        /// Set belonging to Set.Nothing and sets color on green.
        /// </summary>
        /// <param name="value">Name of node</param>
        public Node(string value) : base(value) { 
            set = Set.Nothing;
            this.Color = "green";
        }

        /// <summary>
        /// Getter and setter of type of node.
        /// </summary>
        public override string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// Getter and setter of compromise value of node.
        /// </summary>
        public override Compromise Compromised
        {
            get { return this.compromise; }
            set { this.compromise = value; }
        }

        /// <summary>
        /// Getter and setter of vertex
        /// </summary>
        public override Vertex V
        {
            get { return this.v; }
            set { this.v = new Vertex(value); }
        }

        /// <summary>
        /// Getter and setter of count of bits.
        /// </summary>
        public override int Bit
        {
            get
            {
                return this.bit;
            }
            set
            {
                this.bit = value;
            }
        }

        /// <summary>
        /// Getter and setter of belonging to what set. 
        /// </summary>
        public override Set Set
        {
            get
            {
                return this.set;
            }
            set
            {
                this.set = value;
            }
        }

        /// <summary>
        /// Method gets parents of node.
        /// </summary>
        /// <returns>List of parents of node</returns>
        public List<Node> getParents() { return this.parents; }

        /// <summary>
        /// Method gets children of node.
        /// </summary>
        /// <returns>List of children of node</returns>
        public List<Node> getChildren() { return this.children; }

        /// <summary>
        /// Method adds parent to list of parents
        /// </summary>
        /// <param name="parent">Parent of node</param>
        public void addParents(Node parent) { this.parents.Add(parent); }
        
        /// <summary>
        /// Method adds child to list of children
        /// </summary>
        /// <param name="child">Child of node</param>
        public void addChild(Node child) { this.children.Add(child); }

        /// <summary>
        /// Method compare this node with object. 
        /// They are equal if all attributes are equal.
        /// </summary>
        /// <param name="obj">Object which will be compared with.</param>
        /// <returns>If object is null or isn't equal with this node, return false. Otherwise true</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Node node = obj as Node;
            if (node.Color != this.Color)
                return false;
            if (node.Value != this.Value)
                return false;
            if (!this.V.Equals(node.V))
                return false;

            List<Node> nodeParents = node.getParents();
            if (this.parents.Count() != nodeParents.Count())
                return false;
            for (int i = 0; i < this.parents.Count(); i++)
            {
                if(this.parents[i].Color != nodeParents[i].Color ||
                    this.parents[i].Value != nodeParents[i].Value ||
                    !this.parents[i].V.Equals(nodeParents[i].V))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Override method gets hash code of node.
        /// </summary>
        /// <returns>Hash code of node</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Method finds out, if value of node contains parameter.
        /// </summary>
        /// <param name="sub">Substring</param>
        /// <returns>True, if value of node contains substring, otherwise false</returns>
        public bool containSub(string sub)
        {
            string value = this.Value.ToLower();
            return value.Contains(sub.ToLower());
        }


    }
}
