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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CAG
{
    /// <summary>
    /// Interaction logic for Entropies.xaml
    /// </summary>
    public partial class PropertiesGraph : Window
    {
        /// <summary>
        /// Dictionary where key is Slider and value is TextBox.
        /// </summary>
        private Dictionary<Slider, TextBox> entropy = new Dictionary<Slider, TextBox>();

        /// <summary>
        /// Dictionary where key is Slider and value is Node.
        /// </summary>
        private Dictionary<Slider, Node> sliders = new Dictionary<Slider, Node>();

        /// <summary>
        /// Dictionary where key is ComboBox and value is Node.
        /// </summary>
        private Dictionary<ComboBox, Node> setNode = new Dictionary<ComboBox, Node>();

        /// <summary>
        /// Constructor which creates and shows window.
        /// </summary>
        public PropertiesGraph()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method creates new Label, ComboBox and Slider every node from list of nodes.
        /// </summary>
        /// <param name="nodes">List of nodes of graph</param>
        public void addNodes(List<Node> nodes)
        {
            int x = 20, y =30;
            foreach (Node node in nodes)
            {
                if ((!String.IsNullOrEmpty(node.Type) && node.Type != "function") || (!node.containSub("encrypt")
                    && !node.containSub("hash") && !node.containSub("decrypt")
                    && !node.containSub("hmac")))
                {
                    BrushConverter conv = new BrushConverter();
                    SolidColorBrush brush = conv.ConvertFromString("lightGray") as SolidColorBrush;

                    TextBlock textBlock = new TextBlock();
                    textBlock.TextTrimming = TextTrimming.WordEllipsis;
                    textBlock.Width = 125;
                    textBlock.Height = 27;
                    textBlock.Text = node.Value;
                    textBlock.FontSize = 15;
                    textBlock.Background = Brushes.LightGray;
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.IsEnabled = false;
                    textBlock.TextAlignment = TextAlignment.Center;

                    Label label = new Label();
                    label.Width = 125;
                    label.Height = 27;
                    label.Content = node.Value;
                    label.BorderThickness = new Thickness(0.5);
                    label.BorderBrush = brush;
                    label.FontWeight = FontWeights.Bold;

                    Canvas.SetLeft(textBlock, x);
                    Canvas.SetTop(textBlock, y);

                    Slider slider = new Slider();
                    slider.Width = 80;
                    slider.Height = 26;
                    slider.Maximum = 256;
                    slider.Minimum = 0;
                    slider.Value = node.Bit;
                    slider.ValueChanged += slider_ValueChanged;

                    Canvas.SetLeft(slider, 152);
                    Canvas.SetTop(slider, y);

                    TextBox txt = new TextBox();
                    txt.Width = 50;
                    txt.Height = 26;
                    txt.IsEnabled = false;
                    txt.Text = node.Bit.ToString();
                    txt.FontWeight = FontWeights.Bold;
                    txt.TextAlignment = TextAlignment.Center;

                    Canvas.SetLeft(txt, 247);
                    Canvas.SetTop(txt, y);

                    entropy.Add(slider, txt);
                    sliders.Add(slider, node);

                    ComboBox com = new ComboBox();
                    com.Width = 50;
                    com.Height = 26;
                    ComboBoxItem comItem1 = new ComboBoxItem();
                    ComboBoxItem comItem2 = new ComboBoxItem();
                    ComboBoxItem comItem3 = new ComboBoxItem();
                    ComboBoxItem comItem4 = new ComboBoxItem();
                    comItem1.Content = "D";
                    comItem2.Content = "A";
                    comItem3.Content = "A/D";
                    comItem4.Content = "-";
                    com.Items.Add(comItem1);
                    com.Items.Add(comItem2);
                    com.Items.Add(comItem3);
                    com.Items.Add(comItem4);
                    switch (node.Set)
                    {
                        case Set.Attack:
                            com.Text = "A";
                            break;
                        case Set.Input:
                            com.Text = "D";
                            break;
                        case Set.Both:
                            com.Text = "A/D";
                            break;
                    }
                    com.SelectionChanged += com_SelectionChanged;

                    Canvas.SetLeft(com, 307);
                    Canvas.SetTop(com, y);

                    setNode.Add(com, node);

                    myCanvas.Children.Add(textBlock);
                    myCanvas.Children.Add(slider);
                    myCanvas.Children.Add(txt);
                    myCanvas.Children.Add(com);
                    y += 50;

                    if (y > this.Height)
                        myCanvas.Height = y;
                }
            }
        }

        /// <summary>
        /// Method changes type of set of node after new select in ComboBox.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Selection changed event args</param>
        void com_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox com = sender as ComboBox;
            ComboBoxItem comItem = (ComboBoxItem)com.SelectedItem;
            switch (comItem.Content.ToString())
            {
                case "A": setNode[com].Set = Set.Attack; break;
                case "D": setNode[com].Set = Set.Input; break;
                case "A/D": setNode[com].Set = Set.Both; break;
                default: setNode[com].Set = Set.Nothing; break;
            }
        }

        /// <summary>
        /// Method changes entropy of node after change in Slider
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed property changed event args</param>
        void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            double value = slider.Value;
            entropy[slider].Text = value.ToString("0");
            sliders[slider].Bit = Convert.ToInt32(slider.Value);
        }

        /// <summary>
        /// Method change new entropies and sets of nodes before window close.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void Window_Close(object sender, EventArgs e)
        {
            foreach (KeyValuePair<Slider, Node> pair in sliders)
            {
                pair.Value.Bit = Convert.ToInt32(pair.Key.Value);
            }
            foreach (KeyValuePair<ComboBox, Node> pair in setNode)
            {
                ComboBoxItem item = (ComboBoxItem)pair.Key.SelectedItem;
                
                switch (item.Content.ToString())
                {
                    case "A": pair.Value.Set = Set.Attack; break;
                    case "D": pair.Value.Set = Set.Input; break;
                    case "A/D": pair.Value.Set = Set.Both; break;
                    default: pair.Value.Set = Set.Nothing; break;
                }
            }
            this.Close();
        }
        
    }
}
