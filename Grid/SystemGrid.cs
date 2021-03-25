using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Toolset;

namespace Grid
{
    //Simulation Controller
    public static class SystemGrid 
    {
        //For stroing information about circuit.
        //like nodes, components, wires, grounds etc.
        public static Dictionary<int, Node> MainGrid = new Dictionary<int, Node>();
        public static Dictionary<int, Component> ComponentGrid = new Dictionary<int, Component>();
        public static List<GND> groundList = new List<GND>();
        public static List<Wire> wireList = new List<Wire>();
        public static List<Probe> probeList = new List<Probe>();

        //where the solution is stored after simulation is done
        public static double[] solutionMatrix;
       
        public static int NodeCount ;        //total number of 'Net' in the circuit
        public static int VoltageCount ;     //total number of object of class voltage or derrived class
        public static int DatasetLength { get; set; }
        public static int GridSize = 21;

        //Sim handle
        //for now.
        static readonly SimulationHandler SimCore = new SimulationHandler(SimulationType.TransientAnalysis);
        public static SimInformer Informer = SimCore.Informer;
        
        //Transient stuff
        public static double TimeSpace = 0.01;
        public static double EndTime = 0.5;
        public static double dTime;

        /// <summary>
        /// Attempts simulation..obviously
        /// </summary>
        public static void AttemptSimulation()
        {
            //un-comment on final build
            /*
            try
            {
                SimCore.StartSimulation();
            }
            
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            */
            SimCore.StartSimulation();
        }
    }

    //a wire is a class that holds every node in the given line
    public class Wire
    {

        public static int LastKnownNodeIndex = -1;
        public List<Node> InWireNode;

        public Wire(int x1, int y1, int x2, int y2)
        {
            InWireNode = new List<Node>();
            Node _PreviousNode;
            bool _vertical = IsVertical(x1, y1, x2, y2);
            int _StartNodeIndex;
            int _EndNodeIndex;
            int _IteratorBegin;
            int _IteratorBound;
            int _ValidNet;
            int _Shift;

            if(_vertical)
            {
                _Shift = x1 / 50;
                _IteratorBegin = (Math.Min(y1, y2) / 50) + 1;
                _IteratorBound = (Math.Max(y1, y2) / 50);

                _StartNodeIndex = (_IteratorBegin - 1) * SystemGrid.GridSize + _Shift;
                _EndNodeIndex = _IteratorBound * SystemGrid.GridSize + _Shift;
            }
            else
            {
                _Shift = y1 / 50;
                _IteratorBegin = (Math.Min(x1, x2) / 50) + 1;
                _IteratorBound = (Math.Max(x1, x2) / 50);
                _StartNodeIndex = (_Shift * SystemGrid.GridSize) + (_IteratorBegin - 1);
                _EndNodeIndex = (_Shift * SystemGrid.GridSize) + _IteratorBound;
            }

            //_IteratorBegin = 1;//_StartNodeIndex + 1;
            //_IteratorBound = _EndNodeIndex - _StartNodeIndex ;//_EndNodeIndex;
            _ValidNet = _StartNodeIndex;

            //checks if we are overlapping any component
            if (SystemGrid.ComponentGrid.ContainsKey(_StartNodeIndex)) throw new ComponentOverlapExecption("Component overlapping!");
            if (SystemGrid.ComponentGrid.ContainsKey(_EndNodeIndex)) throw new ComponentOverlapExecption("Component overlapping!");

            //creating Start node
            //to check if we are continuing the same wire
            if (SystemGrid.MainGrid.ContainsKey(_StartNodeIndex))
            {
                _PreviousNode = SystemGrid.MainGrid[_StartNodeIndex];
                if (LastKnownNodeIndex == _StartNodeIndex) _ValidNet = SystemGrid.MainGrid[LastKnownNodeIndex].Net;
            }

            //else we create a new wire with it's own net
            else
            {
                _PreviousNode = new Node(_StartNodeIndex, _ValidNet);
                SystemGrid.MainGrid.Add(_StartNodeIndex, _PreviousNode);
            }
            //Start node creation ended;

            //Creating in-between nodes
            for(int i = _IteratorBegin;i < _IteratorBound;i++)
            {
                InWireNode.Add(_PreviousNode);
                int _CurrentNodeIndex = _vertical ? i * SystemGrid.GridSize + _Shift : _Shift * SystemGrid.GridSize + i;

                //Checking component overlapping
                if (SystemGrid.ComponentGrid.ContainsKey(_CurrentNodeIndex)) throw new ComponentOverlapExecption("can not add wire over components");

                if (!SystemGrid.MainGrid.ContainsKey(_CurrentNodeIndex))
                {
                    Node __CurrentNode = new Node(_CurrentNodeIndex, _ValidNet);
                    if (_vertical)
                    {
                        __CurrentNode.up = _PreviousNode;
                        _PreviousNode.down = __CurrentNode;
                    }
                    else
                    {
                        __CurrentNode.left = _PreviousNode;
                        _PreviousNode.right = __CurrentNode;
                    }
                    SystemGrid.MainGrid.Add(_CurrentNodeIndex, __CurrentNode);
                    _PreviousNode = __CurrentNode;
                }

                else
                {
                    Node __DetaineeNode = SystemGrid.MainGrid[_CurrentNodeIndex];
                    if (_vertical && __DetaineeNode.up != null) throw new WireOverlapException("Wire overlapped!!");
                    else if (!_vertical && __DetaineeNode.left != null) throw new WireOverlapException("Wire overlapped!!");
                }
            }
            //in-between node creation ended

            //creating end node
            //checking if endNode already exists in the grid
            if(SystemGrid.MainGrid.ContainsKey(_EndNodeIndex))
            {
                Node __EndNode = SystemGrid.MainGrid[_EndNodeIndex];
                if (_vertical && __EndNode.up == null)
                {
                    __EndNode.up = _PreviousNode;
                    _PreviousNode.down = __EndNode;
                    InWireNode.Add(__EndNode);
                }
                else if (!_vertical && __EndNode.left == null)
                {
                    __EndNode.left = _PreviousNode;
                    _PreviousNode.right = __EndNode;
                    InWireNode.Add(__EndNode);
                }
                else throw new WireOverlapException("Wire overlapped!!");
            }

            //else we create a new one
            else
            {
                Node __EndNode = new Node(_EndNodeIndex, _ValidNet);
                if (_vertical)
                {
                    __EndNode.up = _PreviousNode;
                    _PreviousNode.down = __EndNode;
                }
                else
                {
                    __EndNode.left = _PreviousNode;
                    _PreviousNode.right = __EndNode;
                }
                SystemGrid.MainGrid.Add(_EndNodeIndex, __EndNode);
            }
            //end node creation ended
            LastKnownNodeIndex = _EndNodeIndex;
            SystemGrid.wireList.Add(this);
        }

        bool IsVertical(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2) return true;
            if (y1 == y2) return false;
            return true;
        }
    }

    public class Node
    {
        public delegate void NetChangeEventHandler(object sender, int NewNet, int GridIndex);
        public event NetChangeEventHandler NetChanged;

        public Int16 OwnerComponentCount;
        public int Net { get; private set; }
        public int NodePosition { get; private set; }
        bool isModified;

        public Node left, right, up, down;

        public Node(int p)
        {
            //node's position in grid
            NodePosition = p;

            //node number is the value which is really gonna count in simultaion
            Net = p;
            //nullifying connecting 
            left = null;
            right = null;
            up = null;
            down = null;
        }

        public Node(int p, int netNumber)
        {
            Net = netNumber;
            NodePosition = p;
            left = null;
            right = null;
            up = null;
            down = null;
            OwnerComponentCount = 0;
        }

        public String PrintInfo()
        {
            return Net.ToString() + ", Node index in MainGrid: " + NodePosition.ToString(); 
        }

        /// <summary>
        /// add or subtract from net value
        /// use carefully.Can fu@k up the whole circuit
        /// </summary>
        /// <param name="x">value to add or subtract</param>
        public void OverrideNet(int x)
        {
            Net += x;
        }

        public bool ModifyConnected(int n)
        {
            if (!isModified)
            {
                bool k = false;
                isModified = true;
                Net = n;
                if (left != null) { left.ModifyConnected(n);k = true; }
                if (right != null) { right.ModifyConnected(n);k = true; }
                if (up != null) { up.ModifyConnected(n);k = true; }
                if (down != null) { down.ModifyConnected(n); k = true; }
                if (OwnerComponentCount > 0) k = true;
                if (!k) throw new ComponentFloatingException("Compontnt is floating!");
                //alerting dependent classes that net of this node is updated
                NetChanged?.Invoke(this, Net, NodePosition);

                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Checks if this node is attached to other things when deleting any component
        /// </summary>
        /// <returns>true if node is necessary, false otherwise</returns>
        public bool CheckPriority()
        {
            bool k = false;
            if (left != null) {  k = true; }
            if (right != null) {k = true; }
            if (up != null) { k = true; }
            if (down != null) { k = true; }
            if (OwnerComponentCount > 0) k = true;
            return k;
        }

        /// <summary>
        /// Returns Voltage at this node
        /// </summary>
        /// <returns>voltage</returns>
        public double GetNodeVoltage()
        {
            if (Net == 0) return 0;
            return SystemGrid.solutionMatrix[Net - 1];
        }

        public void ResetNode() { isModified = false; }

    }

    public class Probe
    {
        TextBlock _Label;
        Rectangle _rect;
        int _key;
        ProbeType ptype;
        public static int compCount = 0;

        public Probe(int posX, int posY, ProbeType p, Rectangle rect)
        {
            int x = posX / 50;
            int y = posY / 50;
            int key = SystemGrid.GridSize * y + x;
            _key = key;
            ptype = p;
            if (!SystemGrid.MainGrid.ContainsKey(key) && p == ProbeType.voltage) throw new Exception("Probe can only be added on wires and terminals");
            else if(!SystemGrid.ComponentGrid.ContainsKey(key) && p == ProbeType.current) throw new Exception("Probe can only be added on components");
            _rect = rect;
            _Label = new TextBlock();
            _Label.Text = p.ToString();
            _Label.FontWeight = FontWeights.Bold;
            _Label.Background = ptype == ProbeType.current? Brushes.CadetBlue : Brushes.DeepPink;
            _Label.SnapsToDevicePixels = true;
            
            VisualGrid.CurrentCanvas.Children.Add(_Label);
            Canvas.SetLeft(_Label, posX + 35);
            Canvas.SetTop(_Label, posY - 87);
            SystemGrid.probeList.Add(this);
        }

        /// <summary>
        /// Gets value to display at schematics display
        /// </summary>
        public void GetValues()
        {
            string val = "";
            if(ptype == ProbeType.voltage)
            {
                int net = SystemGrid.MainGrid[_key].Net - 1;
                if (net + 1 == 0) val = "0.0 V";
                else val = SystemGrid.solutionMatrix[net].ToString("0.00") + " V";
            }
            else
            {
                val = SystemGrid.ComponentGrid[_key].CurrentThrough.ToString("0.00") + " A";
            }
            _Label.Text = val;
        }

        /// <summary>
        /// Get value to display in simulation output window
        /// </summary>
        public int GetValuesForProbe()
        {
            int rowNum;
            if (compCount == 0) compCount = SystemGrid.ComponentGrid.Count;
            if(ptype == ProbeType.voltage)
            {
                int net = SystemGrid.MainGrid[_key].Net - 1;
                rowNum = compCount * 2 + net;
                return rowNum;
            }
            else
            {
                rowNum = SystemGrid.ComponentGrid[_key].indexInOutput + 1;
                return rowNum;
            }
        }

        /// <summary>
        /// Returns the appropriate description of the probe
        /// </summary>
        /// <returns>The description</returns>
        public string GetAppropriateName()
        {
            string str = " ";

            if(ptype == ProbeType.current)
            {
                str = string.Format("I({0})", SystemGrid.ComponentGrid[_key]);
            }

            else
            {
                str = string.Format("V({0})", _key);
            }
            return str;
        }

    }

    public enum ProbeType
    {
        voltage,
        current
    }

    //exceptions
    public class WireOverlapException : Exception
    {
        public WireOverlapException(String message) : base(message) { }
    }

    public class InvalidComponentRotation : Exception
    {
        public InvalidComponentRotation(string message) : base(message) { }
    }

    public class ComponentOverlapExecption : Exception
    {
        public ComponentOverlapExecption(string message) : base(message) { }
    }

    public class ComponentFloatingException : Exception
    {
        public ComponentFloatingException(string message) : base(message) { }
    }

}
