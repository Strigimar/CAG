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

namespace CAG
{
    /// <summary>
    /// Class represents coordinates x and y, height and width of node.
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// X-coordinate of vertex
        /// </summary>
        private double x;
        /// <summary>
        /// Y-coordinate of vertex
        /// </summary>
        private double y;
        /// <summary>
        /// Height of vertex
        /// </summary>
        private double height;
        /// <summary>
        /// Width of vertex
        /// </summary>
        private double width;

        /// <summary>
        /// Sets x, y, height and width as 0.
        /// </summary>
        public Vertex() : this(0, 0, 0, 0) { }

        /// <summary>
        /// Saves values to x, y, height and width.
        /// </summary>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="h">Height of node</param>
        /// <param name="w">Width of node</param>
        public Vertex(double x, double y, double h, double w)
        {
            this.x = x;
            this.y = y;
            this.height = h;
            this.width = w;
        }

        /// <summary>
        /// Saves values from strings to x and y. Width and height will be set on 0.
        /// </summary>
        /// <param name="x">String which will be parsed and saved to X</param>
        /// <param name="y">String which will be parsed and saved to Y</param>
        public Vertex(string x, string y)
        {
            this.x = Double.Parse(x);
            this.y = Double.Parse(y);
            height = 0;
            width = 0;
        }

        /// <summary>
        /// Copy coordinates, width and height from parameter and saves them.
        /// </summary>
        /// <param name="v">Vertex which will be used to save</param>
        public Vertex(Vertex v) : this(v.X, v.Y, v.Height, v.Width) { }
        
        /// <summary>
        /// Getter and setter of x-coordinate.
        /// </summary>
        public double X
        {
            get { return x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Getter and setter of y-coordinate.
        /// </summary>
        public double Y
        {
            get { return y; }
            set { this.y = value; }
        }
        /// <summary>
        /// Getter and setter of height of node.
        /// </summary>
        public double Height
        {
            get { return height; }
            set { this.height = value; }
        }

        /// <summary>
        /// Getter and setter of width of node.
        /// </summary>
        public double Width
        {
            get { return width; }
            set { this.width = value; }
        }

        /// <summary>
        /// Override method on comparing. Two objects are equal if they have same value of x and y.
        /// </summary>
        /// <param name="obj">Object which will be compared with this vertex</param>
        /// <returns>If parameter is null or isn't equal, return false. Otherwise true</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Vertex v = obj as Vertex;
            return (this.X == v.X && this.Y == v.Y
                && this.Width == v.Width && this.Height == v.Height);

        }

        /// <summary>
        /// Override method gets hash code of vertex.
        /// </summary>
        /// <returns>Hash code of vertex</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
