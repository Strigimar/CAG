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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CAG
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        /// <summary>
        /// Attack graph
        /// </summary>
        private Graph g = new Graph();

        /// <summary>
        /// Reader of file with DOT language
        /// </summary>
        private Reader f;

        /// <summary>
        /// Dictionary where key is Border and value is node.
        /// For linking between borders in graph and node.
        /// </summary>
        private Dictionary<Border, Node> borders = new Dictionary<Border, Node>();

        /// <summary>
        /// Window for properties of graphs -> entropies and set
        /// </summary>
        private PropertiesGraph window = null;

        /// <summary>
        /// Value for indicating if during analysing of graph was a change.
        /// </summary>
        private bool changes = false;

        /// <summary>
        /// Name of file with DOT language.
        /// </summary>
        private string fileName;

        /// <summary>
        /// Constructor which can work with command line. 
        /// </summary>
        public MainWindow()
        {
            string[] input = Environment.GetCommandLineArgs();
            if (input.Length > 1)
            {

                // -4 in case bad input
                // -3 in case not to open or find file
                // -2 if format in CAS+ is not correct
                // -1 if format in DOT is not correct
                // 0 if all was correct
                // 1 if windows must be showed
                switch (parseInput(input))
                {
                    case -4:
                        if ((input[1].CompareTo("-r") == 0 || input[1].CompareTo("-R") == 0) && input.Length == 2)
                        {
                            InitializeComponent();
                            return;
                        }
                        else
                            writeToCommandLine("Error - bad input format");
                        break;
                    case -3:
                        writeToCommandLine("Error - file wasn't found or couldn't be opened");
                        break;
                    case -2:
                        writeToCommandLine("Error - bad format in CAS+ protocol");
                        break;
                    case -1:
                        writeToCommandLine("Error - bad format in DOT");
                        break;
                    case 1: return;
                }
                this.Close();
            }
            else
                InitializeComponent();
        }

        /// <summary>
        /// Constructor which read file by parametr and show graph.
        /// </summary>
        /// <param name="file">Name of file with DOT</param>
        public MainWindow(string file)
        {
            fileName = file;
            InitializeComponent();
            showGraph();
        }

        /// <summary>
        /// Method write error output to command line.
        /// </summary>
        /// <param name="output">Output text which will be showed in command line</param>
        private void writeToCommandLine(string output)
        {
            if (!AttachConsole(-1))
                AllocConsole();
            Console.WriteLine("");
            Console.WriteLine(output);
            FreeConsole();
        }

        /// <summary>
        /// Method parse command line. 
        /// -fd = find min set from defined set
        /// -fa = find min set from all nodes
        /// -a = analyse
        /// -r = run
        /// -t/d = .txt/.dot
        /// -p/d = .png/.dot
        /// </summary>
        /// <param name="input">Input from command line</param>
        /// <returns>
        /// Returns those values:
        /// -4 in case bad input
        /// -3 in case not to open or find file
        /// -2 if format in CAS+ is not correct
        /// -1 if format in DOT is not correct
        /// 0 if all was correct
        /// 1 if windows musts be showed
        /// </returns>
        private int parseInput(string[] input)
        {
            if ((input.Length != 6) && (!(input[1].ToLower().Contains("-r") && input.Length == 4)))
                return -4;
            switch (input[1])
            {
                case "-fd":
                case "-FD":
                    switch (input[2])
                    {
                        case "-t":
                        case "-T":
                            return cmdLineProtocol(input, Analyse.FIND_MIN_DEFINED);
                        case "-d":
                        case "-D":
                            return cmdLineDot(input, Analyse.FIND_MIN_DEFINED);
                        default:
                            return -4;
                    }
                case "-fa":
                case "-FA":
                    switch (input[2])
                    {
                        case "-t":
                        case "-T":
                            return cmdLineProtocol(input, Analyse.FIND_MIN_ALL);
                        case "-d":
                        case "-D":
                            return cmdLineDot(input, Analyse.FIND_MIN_ALL);
                        default:
                            return -4;
                    }
                case "-a":
                case "-A":
                    switch (input[2])
                    {
                        case "-t":
                        case "-T":
                            return cmdLineProtocol(input, Analyse.ANALYSE);
                        case "-d":
                        case "-D":
                            return cmdLineDot(input, Analyse.ANALYSE);
                        default:
                            return -4;
                    }
                case "-r":
                case "-R":
                    switch (input[2])
                    {
                        case "-t":
                        case "-T":
                            string dotName = input[3].Remove(input[3].LastIndexOf('.'));
                            dotName += ".dot";
                            while (System.IO.File.Exists(dotName))
                                dotName = dotName.Insert(dotName.Length - 4, "1");

                            System.IO.FileStream fs = System.IO.File.Create(dotName);
                            fs.Close();

                            Parser myParser = new Parser(input[3], dotName);
                            int count = myParser.parse();
                            if (count == -1)
                                return -3;
                            else if (count == -2)
                                return -2;
                            else if (count != -2)
                            {

                                for (int i = 1; i <= count; i++)
                                {
                                    fileName = dotName.Insert(dotName.Length - 4, i.ToString());
                                    if (i == 1)
                                    {
                                        InitializeComponent();
                                        showGraph();

                                    }
                                    else
                                    {
                                        MainWindow newWindow = new MainWindow(fileName);
                                        newWindow.Show();
                                    }
                                    System.IO.File.Delete(fileName);
                                }
                            }
                            return 1;
                        case "-d":
                        case "-D":
                            fileName = input[3];
                            InitializeComponent();
                            showGraph();
                            return 1;
                        default:
                            return -4;
                    }
            }
            return -4;
        }

        /// <summary>
        /// Method read from protocol and parse it.
        /// </summary>
        /// <param name="input">Input from command line.</param>
        /// <param name="analyse">Analyse, if it is for analysing, Find_min_defined for finding minimal set from defined, 
        /// otherwise Find_min_all for finding minimal set from all nodes.</param>
        /// <returns>
        /// Return those values:
        /// -4 if bad input format
        /// -3 if file can't be opened or found.
        /// -2 if bad format in CAS+.
        /// -1 if bad format in DOT.
        /// 0 if all was correct
        /// </returns>
        private int cmdLineProtocol(string[] input, Analyse analyse)
        {
            fileName = input[3];
            if (fileName.Substring(fileName.Length - 3).ToLower() != "txt")
                return -4;

            if (!((input[4].ToLower() == "-p" && input[5].Substring(input[5].Length - 3).ToLower() == "png")
                || (input[4].ToLower() == "-d" && input[5].Substring(input[5].Length - 3).ToLower() == "dot")))
                return -4;
            string dotName = fileName.Remove(fileName.LastIndexOf('.'));
            dotName += ".dot";
            while (System.IO.File.Exists(dotName))
                dotName = dotName.Insert(dotName.Length - 4, "1");

            System.IO.FileStream fs = System.IO.File.Create(dotName);
            fs.Close();

            Parser myParser = new Parser(input[3], dotName);
            int count = myParser.parse();
            if (count == -1)
                return -3;
            else if (count == -2)
                return -2;
            else if (count != -2)
            {
                for (int i = 1; i <= count; i++)
                {
                    fileName = dotName.Insert(dotName.Length - 4, i.ToString());
                    switch (readFile())
                    {
                        case -2:
                            return -1;
                        case -1:
                            return -3;
                        case 0:
                            cmdLineFourthArgument(analyse, input);
                            break;

                    }
                    System.IO.File.Delete(fileName);

                }
            }
            return 0;
        }

        /// <summary>
        /// Method by value does analysing or finding minimal set.
        /// Then write result to file.
        /// </summary>
        /// <param name="analyse">Analyse, if it is for analysing, Find_min_defined for finding minimal set from defined, 
        /// otherwise Find_min_all for finding minimal set from all nodes.</param>
        /// <param name="input">Input from command line.</param>
        private void cmdLineFourthArgument(Analyse analyse, string[] input)
        {
            switch (analyse)
            {
                case Analyse.ANALYSE:
                    {
                        g.analyse();
                        switch (input[4])
                        {
                            case "-p":
                            case "-P":
                                Writer wr = new Writer("replacement.gv");
                                wr.writeFile(g, false);
                                if (!wr.createPng(input[5]))
                                    writeToCommandLine(
                                        "Incorrect path to file. Probably name of some folder is bad. Please rewrite old names on new names without spaces.");
                                break;
                            case "-d":
                            case "-D":
                                Writer wr1 = new Writer(input[5]);
                                wr1.writeFile(g, true);
                                break;

                        }
                        break;
                    }
                case Analyse.FIND_MIN_DEFINED:
                    {
                        switch (input[4])
                        {
                            case "-p":
                            case "-P":
                                findMinSet(input[5], Output.PNG);
                                break;
                            case "-d":
                            case "-D":
                                findMinSet(input[5], Output.DOT);
                                break;
                        }
                        break;
                    }
                case Analyse.FIND_MIN_ALL:
                    switch (input[4])
                    {
                        case "-p":
                        case "-P":
                            findMinSetAll(input[5], Output.PNG);
                            break;
                        case "-d":
                        case "-D":
                            findMinSetAll(input[5], Output.DOT);
                            break;
                    }
                    break;
            }

        }

        /// <summary>
        /// Method read from file with DOT.
        /// </summary>
        /// <param name="input">Input from command line.</param>
        /// <param name="analyse">Analyse, if it is for analysing, Find_min_defined for finding minimal set from defined, 
        /// otherwise Find_min_all for finding minimal set from all nodes.</param>
        /// <returns>
        /// Returns those values:
        /// -3 if file can't be opened or found.
        /// -1 if bad format in DOT.
        /// 0 if OK
        /// </returns>
        private int cmdLineDot(string[] input, Analyse analyse)
        {
            fileName = input[3];
            if ((fileName.Substring(fileName.Length - 3).ToLower() != "dot")
                && fileName.Substring(fileName.Length - 2).ToLower() != "gv")
                return -4;
            if (!((input[4].ToLower() == "-p" && input[5].Substring(input[5].Length - 3).ToLower() == "png")
                || (input[4].ToLower() == "-d" && input[5].Substring(input[5].Length - 3).ToLower() == "dot")))
                return -4;
            
            switch (readFile())
            {
                case -2:
                    // error format in DOT
                    return -1;
                case -1:
                    // error opening of file
                    return -3;
                case 0:
                    cmdLineFourthArgument(analyse, input);
                    break;
            }
            return 0;
                
        }

        /// <summary>
        /// Method set width and height of canvas, scrollViewer after changing size of window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Size Changed Event Args</param>
        private void Analysis_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(myCanvas.Height < this.Height - 27)
                myCanvas.Height = this.Height - 27;
            if(myCanvas.Width < this.Width - 18)
                myCanvas.Width = this.Width;
            myScroll.Width = this.Width - 18;
            myScroll.Height = this.Height - 63;
        }

        /// <summary>
        /// Method clears canvas, reads from file and draws new graph. 
        /// </summary>
        private void showGraph()
        {
            clearAll();
            switch(readFile())
            {
                case 0:
                    g.analyse();
                    drawGraph();

                    MenuItem_CreatePng.IsEnabled = true;
                    MenuItem_Mark.IsEnabled = true;
                    MenuItem_MinSet.IsEnabled = true;
                    MenuItem_Save.IsEnabled = true;
                    MenuItem_Unmark.IsEnabled = true;
                    MenuItem_Properties.IsEnabled = true;
                    MenuItem_MinSet2.IsEnabled = true;
                    break;
                case -1:
                    MessageBox.Show("Error file - can't find or is already open!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case -2:
                    MessageBox.Show("Error format in DOT", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }


        }

        /// <summary>
        /// Method for clearing canvas - clear borders and edges in graph.
        /// </summary>
        private void clearAll()
        {
            g = new Graph();
            borders.Clear();
            changes = false;
            myCanvas.Children.Clear();
        }
        
        /// <summary>
        /// Method for reading from file and parsing it.
        /// </summary>
        /// <returns>
        /// Return 0 if reading and parsing file was correct.
        /// Return -1 if file was not found or couldn't be opened.
        /// Otherwise error in format DOT.
        /// </returns>
        private int readFile()
        {
            f = new Reader(fileName);
            switch (f.readFile())
            {
                case -2:
                    return -2;
                case -1:
                    return -1;
                case 0:
                    if(f.parseLines(ref g) == -1)
                        return -2;
                    break;
            }
            return 0;
            
        }

        /// <summary>
        /// Method open new empty window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void NewWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win2 = new MainWindow();
            win2.Show();
        }

        /// <summary>
        /// Method save name of file, which user chose, and show graph from file.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (openDialog("GV files (*.gv)|*.gv| DOT files (*.dot)|*.dot") == true)
            {
                showGraph();
            }
        }

        /// <summary>
        /// Method read from protocol and write to DOT. Then show graph from file with DOT.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void OpenProtocol_Click(object sender, RoutedEventArgs e)
        {
            if (openDialog("TXT files (*.txt)|*.txt") == true)
            {
                clearAll();
                List<string> parts = new List<string>(fileName.Split('\\'));
                string dotName = fileName.Remove(fileName.LastIndexOf('.'));
                dotName += ".dot";
                
                while (System.IO.File.Exists(dotName))
                    dotName = dotName.Insert(dotName.Length - 4, "1");
                System.IO.FileStream fs = System.IO.File.Create(dotName);
                fs.Close();
                
                Parser myParser = new Parser(fileName, dotName);                
                int count = myParser.parse();
                if (count == -1)
                {
                    MessageBox.Show("Error file - can't find or is already open!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (count == -2)
                    System.Windows.MessageBox.Show("Error format in CAS+", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                else if (count != -2)
                {
                    fileName = dotName.Insert(dotName.Length - 4, "1");

                    // for change on correct size of window. Also change Scrollviewers.
                    this.myCanvas.Height = 0;
                    this.myCanvas.Width = 0;
                    Analysis_StateChanged(null, null);
                    Analysis_SizeChanged(null, null);

                    showGraph();
                    System.IO.File.Delete(fileName);
                    for (int j = 2; j <= count; j++)
                    {
                        string nextFile = dotName.Insert(dotName.Length - 4, j.ToString());
                        MainWindow window = new MainWindow(nextFile);
                        System.IO.File.Delete(nextFile);
                        window.Show();
                    }
                }
            }
        }

        /// <summary>
        /// Open dialog and save name of file.
        /// </summary>
        /// <param name="suffixes">Suffixes of required files.</param>
        /// <returns>Bool value if dialog finished correctly.</returns>
        private Nullable<bool> openDialog(string suffixes)
        {
            MessageBoxResult res = sendWarning();
            if (res == MessageBoxResult.Yes)
                Save_Click(null, null);
            if(res != MessageBoxResult.None)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = suffixes;
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    fileName = dlg.FileName;
                    if (window != null)
                    {
                        window.Close();
                        window = null;
                    }
                }
                return result;
            }
            return false;
        
        }

        /// <summary>
        /// Method for saving graph to file with DOT.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void Save_Click(object sender, RoutedEventArgs e)
         {
            string dotName = "";
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".gv";
            dlg.Filter = "GV files (*.gv)|*.gv| DOT files (*.dot)|*.dot";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                dotName = dlg.FileName;
                Writer wr = new Writer(dotName);
                wr.writeFile(g, true);
                changes = false;
            }
        }

        /// <summary>
        /// Method for creating PNG file from graph. 
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void CreatePng_Click(object sender, RoutedEventArgs e)
        {
            string pngName = "";
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (*.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                pngName = dlg.FileName;

                Writer wr = new Writer("replacement.gv");
                wr.writeFile(g, false);

                if(!wr.createPng(pngName))
                    MessageBox.Show("Incorrect path to file. Probably name of some folder is bad. Please rewrite old names on new names without spaces.", 
                        "Incorrect path", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                
            }
            

        }

        /// <summary>
        /// After click in menu "Find minimal set of defined set" call finding minimal set.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void FindMinSet_Click(object sender, RoutedEventArgs e)
        {
            findMinSet("", Output.NOTHING);
        }

        /// <summary>
        /// After click in menu "Find minimal set of all nodes" call finding minimal set of all nodes. 
        /// Method calls findMinSetGraph.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void FindMinSetGraph_Click(object sender, RoutedEventArgs e)
        {
            findMinSetAll("", Output.NOTHING);
        }

        /// <summary>
        /// Method joins to every nodes their set from Nothing on Input except nodes in Attack set.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="value"></param>
        private void findMinSetAll(string file, Output value)
        {
            List<Node> oldNothing = new List<Node>();

            foreach (Node node in g.getNodes())
            {
                if (node.Type != "function" && node.Set == Set.Nothing)
                {
                    oldNothing.Add(node);
                    node.Set = Set.Input;
                }
            }

            findMinSet(file, value);
            foreach (Node n in oldNothing)
            {
                Node node = n;
                node.Set = Set.Nothing;
            }
        }

        /// <summary>
        /// Method for finding minimal set of input set.
        /// </summary>
        /// <param name="file">Name of file which result will be written to.</param>
        /// <param name="value">Output value - where result will be written to
        /// Output.PNG = to png
        /// Output.DOT = to dot
        /// Output.Nothing = open dialog and user will choose file.</param>
        private void findMinSet(string file, Output value)
        {
            Dictionary<Node, Compromise> saveCompromised = new Dictionary<Node, Compromise>();
            foreach (Node node in g.getCompromiseNodes())
            {
                saveCompromised.Add(node, node.Compromised);
            }
            g.uncompromiteAll();
            bool found = false;
            List<Node> input = new List<Node>();
            List<Node> attack = new List<Node>();
            foreach (Node node in g.getNodes())
            {
                Node n = node;
                switch (n.Set)
                {
                    case Set.Input:
                        input.Add(n);
                        break;
                    case Set.Attack:
                        attack.Add(n);
                        break;
                    case Set.Both:
                        attack.Add(n);
                        input.Add(n);
                        break;
                }
            }

            for (int k = 1; k <= input.Count; k++)
                if (g.findCombination(k, ref input, ref attack, ref file, value) == 2)
                {
                    g.uncompromiteAll();
                    k = input.Count;
                    found = true;
                }
            if (value == Output.NOTHING && !found)
                MessageBox.Show("Not found minimal set.", "Not found", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (value != Output.NOTHING && !found)
                writeToCommandLine("Not found minimal set");

            g.setCompromiseNodes(saveCompromised);
            update();
        }

        /// <summary>
        /// Method for close this window after click on "Exit" in menu.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// If in graphs was a change, method send warning.
        /// </summary>
        /// <returns>MessageBoxResult - user choose yes, no or cancel.</returns>
        private MessageBoxResult sendWarning()
        {
            if (changes)
                return MessageBox.Show("Do you want to save changes?", "Save changes",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            else
                return MessageBoxResult.No;
        }

        /// <summary>
        /// Method draw nodes and edges to graph.
        /// </summary>
        private void drawGraph()
        {
            foreach (Node n in g.getNodes())
            {
                drawNode(n);
            }
            foreach (Edge e in g.getEdges())
            {
                drawEdge(e);
            }
        }

        /// <summary>
        /// Method draw node to graph.
        /// </summary>
        /// <param name="n">Node</param>
        private void drawNode(Node n)
        {
            if (n.V.Y * 96 / 72 > myCanvas.Height)
            {
                myCanvas.Height = (n.V.Y * 96 / 72) + n.V.Height * 96;
            }
            if (n.V.X * 96 / 72 > myCanvas.Width)
            {
                myCanvas.Width = (n.V.X * 96 / 72) + n.V.Width * 96;
            }
            Border bor = new Border();
            bor.CornerRadius = new System.Windows.CornerRadius(360);
            // 1 pixel = 1/96 inches
            bor.Width = n.V.Width * 96;
            bor.Height = n.V.Height * 96;

            BrushConverter conv = new BrushConverter();
            SolidColorBrush brush = conv.ConvertFromString(n.Color) as SolidColorBrush;
            switch(n.Color)
            {
                case "red":
                    n.Compromised = Compromise.EASY;
                    g.addCompromisedNode(n);
                    break;
                case "orange":
                    n.Compromised = Compromise.HARD;
                    g.addCompromisedNode(n);
                    break;
            }
            bor.BorderBrush = brush;
            bor.BorderThickness = new System.Windows.Thickness(2);
            TextBlock txt = new TextBlock();
            txt.Text = n.Value;
            txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            txt.FontSize = 20;

            bor.Background = Brushes.White;
            bor.Child = txt;
            // 1 point = pixels * 72 / 96 --> pixel = 96 / 72
            Canvas.SetTop(bor, (n.V.Y * 96 / 72) - (bor.Height / 2));
            Canvas.SetLeft(bor, (n.V.X * 96 / 72) - (bor.Width / 2));
            bor.MouseLeftButtonUp += bor_mouseLeftButtonUp;

            myCanvas.Children.Add(bor);
            borders.Add(bor, n);

        }

        /// <summary>
        /// Method changes color and compromised value of node. Then does analysis of graph.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Mouse button event args</param>
        private void bor_mouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            changes = true;
            e.Handled = true;
            Border bor = new Border();
            if (e.Source is Border)
            {
                bor = e.Source as Border;
            }
            else
            {
                TextBlock txt = e.Source as TextBlock;
                bor = txt.Parent as Border;
            }

            if (bor.BorderBrush == Brushes.Red)
            {
                changeNode(ref bor, "green", Compromise.IMPOSSIBLE);
            }
            else
            {
                changeNode(ref bor, "red", Compromise.EASY);
                g.analyse();
                update();
            }

        }

        /// <summary>
        /// Method changes color and compromise value of node. 
        /// Also method adds or deletes node to/from list of compromised node.
        /// </summary>
        /// <param name="bor">Border which is associated with node.</param>
        /// <param name="color">New color</param>
        /// <param name="value">New compromised value</param>
        private void changeNode(ref Border bor, string color, Compromise value)
        {
            Node n = new Node();
            borders.TryGetValue(bor, out n);
            if (!g.isFunction(n))
            {
                n.Color = color;
                if (value == Compromise.IMPOSSIBLE)
                    g.deleteCompromisedNode(n);
                else
                    g.addCompromisedNode(n);
                BrushConverter conv = new BrushConverter();
                bor.BorderBrush = conv.ConvertFromString(n.Color) as SolidColorBrush;
            }
            n.Compromised = value;

        }

        /// <summary>
        /// Method draws edge to graph.
        /// </summary>
        /// <param name="e">Edge</param>
        private void drawEdge(Edge e)
        {
            int count = e.Vertices.Count;
            PathFigure pthFigure = new PathFigure(); 
            pthFigure.StartPoint = new Point(e.Vertices[0].X * 96 / 72, e.Vertices[0].Y * 96 / 72);
            PolyBezierSegment pbzSeg = new PolyBezierSegment();
            
            for (int i = 1; i < count-1; i++)
            {
                pbzSeg.Points.Add(new Point(e.Vertices[i].X * 96 / 72, e.Vertices[i].Y * 96 / 72));
            }

            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(pbzSeg);

            pthFigure.Segments = myPathSegmentCollection;

            PathFigureCollection pthFigureCollection = new PathFigureCollection();
            pthFigureCollection.Add(pthFigure);

            PathGeometry pthGeometry = new PathGeometry();
            pthGeometry.Figures = pthFigureCollection;

            Path arcPath = new Path();
            arcPath.Stroke = new SolidColorBrush(Colors.Black);
            arcPath.StrokeThickness = 1;
            arcPath.Data = pthGeometry;

            myCanvas.Children.Add(arcPath);

            // Imported library from Petzold
            Petzold.Media2D.ArrowLine arrowLine = new Petzold.Media2D.ArrowLine();

            
            BrushConverter conv = new BrushConverter();
            SolidColorBrush brush = conv.ConvertFromString(e.Color) as SolidColorBrush;
            arrowLine.Stroke = brush;
            arrowLine.X1 = e.Vertices[count - 2].X * 96 / 72;
            arrowLine.Y1 = e.Vertices[count - 2].Y * 96 / 72;
            arrowLine.X2 = e.Vertices[count - 1].X * 96 / 72;
            arrowLine.Y2 = e.Vertices[count - 1].Y * 96 / 72;
            arrowLine.StrokeThickness = 1;
            arrowLine.ArrowEnds = Petzold.Media2D.ArrowEnds.End;
            
            myCanvas.Children.Add(arrowLine);
        }
        
        /// <summary>
        /// Getter and setter of changes.
        /// </summary>
        public bool Changes
        {
            get;
            set;
        }

        /// <summary>
        /// Method change color and compromise value of all nodes on "red" and easy compromise.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void Mark_all_Click(object sender, RoutedEventArgs e)
        {
            List<Border> listBor = new List<Border>(borders.Keys);
            foreach (Border border in listBor)
            {
                Border bor = border;
                changeNode(ref bor, "red", Compromise.EASY);
            }
        }

        /// <summary>
        /// Method change color and compromise value of all nodes on "green" and impossible compromise.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void Unmark_all_Click(object sender, RoutedEventArgs e)
        {
            List<Border> listBor = new List<Border>(borders.Keys);
            foreach (Border border in listBor)
            {
                Border bor = border;
                changeNode(ref bor, "green", Compromise.IMPOSSIBLE);
            }
        }

        /// <summary>
        /// Method open window with entropies and sets of nodes after click on "Properties" in menu.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Routed event args</param>
        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            window = new PropertiesGraph();
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            window.addNodes(g.getNodes());
            window.Show();
        }

        /// <summary>
        /// Method update new changes in graph.
        /// </summary>
        private void update()
        {
            if (myCanvas != null)
            {
                myCanvas.Children.Clear();
                borders.Clear();
                drawGraph();
            }
        }

        /// <summary>
        /// Enum for type of output file. 
        /// </summary>
        public enum Output { 
            /// <summary>
            /// PNG file
            /// </summary>
            PNG, 
            /// <summary>
            /// File in DOT language
            /// </summary>
            DOT, 
            /// <summary>
            /// Without format.
            /// </summary>
            NOTHING 
        };

        /// <summary>
        /// Enum for type of analyse for input command line. 
        /// FIND_MIN_DEFINED == find minimal set of defined set
        /// FIND_MIN_ALL == find minimal set of all nodes
        /// ANALYSE == analyse attack graph
        /// </summary>
        private enum Analyse { FIND_MIN_DEFINED, FIND_MIN_ALL, ANALYSE };

        /// <summary>
        /// Method for change width and height of this window in case maximized of window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void Analysis_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case System.Windows.WindowState.Maximized:
                    this.Width = System.Windows.SystemParameters.PrimaryScreenWidth + 10;
                    this.Height = System.Windows.SystemParameters.PrimaryScreenHeight + 10 ;
                    break;
            }
        }

        /// <summary>
        /// Method closes all window (also properties)
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">EventArgs</param>
        private void Analysis_Closed(object sender, EventArgs e)
        {
            if (window != null)
                window.Close();
        }


    }
}
