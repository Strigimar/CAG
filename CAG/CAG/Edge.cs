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
using System.Windows.Shapes;
using System.Windows.Media;

namespace CAG
{
    /// <summary>
    /// Class edge inherits from UnitGraph, it represents edge from node to another node.
    /// </summary>
    class Edge : UnitGraph
    {
        /// <summary>
        /// Pair of nodes, which edge begins in first node and ends in second node.
        /// </summary>
        private Tuple<Node, Node> fromTo;

        /// <summary>
        /// List of vertices. Vertices notes places in edge where begin other direction.
        /// </summary>
        private List<Vertex> vertices;

        /// <summary>
        /// Saves nodes to pair of nodes from and to. 
        /// </summary>
        /// <param name="from">Node, where edge begins.</param>
        /// <param name="to">Node, where edge ends.</param>
        public Edge(Node from, Node to) :base("") { 
            fromTo = new Tuple<Node, Node>(from, to);
            this.Color = "black";
        }

        /// <summary>
        /// Getter of pair of nodes fromTo.
        /// </summary>
        public Tuple<Node, Node> FromTo
        {
            get { return fromTo; }
        }

        /// <summary>
        /// Override getter and setter of list of vertices.
        /// </summary>
        public override List<Vertex> Vertices
        {
            get
            {
                return vertices;
            }
            set
            {
                vertices = new List<Vertex>(value);
            }
        }

        /// <summary>
        /// Override method which compares this edge with parameter if edges are equal.
        /// Edges are equal if edges begin in same node and end in same node.
        /// </summary>
        /// <param name="obj">Object which will be compared with this edge</param>
        /// <returns>False, if parameter is null or isn't equal with this edge. Otherwise true.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Edge edge = obj as Edge;
            return (fromTo.Item1.Equals(edge.FromTo.Item1) && fromTo.Item2.Equals(edge.FromTo.Item2));
        }

        /// <summary>
        /// Override method which gets hash code.
        /// </summary>
        /// <returns>Hash code of this edge.</returns>
        public override int GetHashCode()
        {
            int hash = 11;
            hash = hash * 31 + fromTo.Item1.GetHashCode();
            hash = hash * 31 + fromTo.Item2.GetHashCode();
            return hash;
        }
    }
}
