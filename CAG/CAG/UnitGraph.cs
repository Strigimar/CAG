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
using System.Windows.Controls; 

namespace CAG
{
    /// <summary>
    /// Abstract class UnitGraph represents basic unit - node or edge of graph.
    /// </summary>
    public abstract class UnitGraph
    {
        /// <summary>
        /// Color of border of node
        /// </summary>
        private string color;
        
        /// <summary>
        /// Value or name of node
        /// </summary>
        private string value;

        /// <summary>
        /// Save value (name) and set color to black.
        /// </summary>
        /// <param name="value">Value which will be saved</param>
        public UnitGraph(string value) { 
            this.Value = value;
        }

        /// <summary>
        /// Getter and setter of color
        /// </summary>
        public string Color
        {
            get { return color; }
            set { this.color = value; }
        }

        /// <summary>
        /// Getter and setter of value
        /// </summary>
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Virtual getter and setter of type of node.
        /// </summary>
        public virtual string Type { get; set; }
        
        /// <summary>
        /// Virtual getter and setter of class vertex representing coordinates x and y of node.
        /// </summary>
        public virtual Vertex V { get; set; }

        /// <summary>
        /// Virtual getter and setter of bits of node.
        /// </summary>
        public virtual int Bit { get; set; }
        
        /// <summary>
        /// Virtual getter and setter of determination, what set this node belongs to.
        /// </summary>
        public virtual Set Set{
            get;
            set;
        }

        /// <summary>
        /// Virtual getter and setter of determination, how much node is compromised.
        /// </summary>
        public virtual Compromise Compromised
        {
            get;
            set;
        }

        /// <summary>
        /// Virtual getter and setter of list of vertex for edge.
        /// </summary>
        public virtual List<Vertex> Vertices
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Enum which marks node if it belongs to defined input set or 
    /// to set about which attacker is interested. This enum is be need
    /// for find minimal set of input set.
    /// </summary>
    public enum Set { 
        /// <summary>
        /// Node isn't in set.
        /// </summary>
        Nothing,
        /// <summary>
        /// Node is in defined(input) set.
        /// </summary>
        Input, 
        /// <summary>
        /// Node is in set which attacker is interested in.
        /// </summary>
        Attack, 
        /// <summary>
        /// Node is in defined and also attacked set.
        /// </summary>
        Both 
    };

    /// <summary>
    /// Enum which determines how much node is compromised.
    /// </summary>
    public enum Compromise { 
        /// <summary>
        /// Count of bits is less than 60
        /// </summary>
        EASY, 
        /// <summary>
        /// Count of bits is between 60 and 80
        /// </summary>
        HARD, 
        /// <summary>
        /// Count of bits is more than 80
        /// </summary>
        IMPOSSIBLE };
}
