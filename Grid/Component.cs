using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grid
{
    public static class ComponentManager
    {
        public static Canvas schematicsCanvas = VisualGrid.CurrentCanvas;
        public static int _ShiftX = -20;
        public static  int _ShiftY = -30;

        /// <summary>
        /// Creates a component object of specified type in given position and rotation.Sets the pin nodes and updates the grid automatically
        /// </summary>
        /// <param name="T">Type of the component</param>
        /// <param name="val">Primaty value of the component</param>
        /// <param name="pX">X axis component of position</param>
        /// <param name="pY">Y axis component of position</param>
        /// <param name="rot">rotation. HAS TO BE 0/90/180/270. Any other value is not valid</param>
        /// <param name="r">Rectangle on which the symbol is drawn</param>
        public static void CreateComponent (Type T, double val, int pX, int pY, int rot, Rectangle r)
        {
            if (T == typeof(Resistor))
            {
                new Resistor(val,pX,pY,rot,r);
            }
            else if(T == typeof(VoltageSource))
            {
                new VoltageSource(val, pX, pY, rot, r);
            }
            else if(T == typeof(CurrentSource))
            {
                new CurrentSource(val, pX, pY, rot, r);
            }
            else if(T == typeof(Capacitor))
            {
                new Capacitor(val, pX, pY, rot, r);
            }

            else if(T == typeof(GND))
            {
                new GND(pX, pY, rot, r);
            }
        }

        /// <summary>
        /// Deletes a component from grid
        /// </summary>
        /// <param name="posX">mouse position x</param>
        /// <param name="posY">mouse position y</param>
        /// <param name="selected">selected component</param>
        public static void DeleteComponent(int posX, int posY, Component selected)
        {
            int x = posX / 50;
            int y = posY / 50;
            selected.Disconnect();
            SystemGrid.ComponentGrid.Remove(y * SystemGrid.GridSize + x);
        }

        public static List<Type> GetSymbolList()
        {
            List<Type> sl = new List<Type>();
            foreach (Type t in VisualGrid.SymbolList.Keys)
            {
                sl.Add(t);
            }
            return sl;
        }
        
        public static void ResetAllComponentIndex()
        {
            Resistor.ResetAllIndices();
            VoltageSource.ResetAllIndices();
            CurrentSource.ResetAllIndices();
            Capacitor.ResetAllIndices();
        }

    }

    public class Component : INotifyPropertyChanged
    {
        #region Variables
        //graphical properties
        protected Rectangle _compSymb;      //rectangle that holds component symbol
        protected TextBlock label;          //label that shows component's name and values
        protected int rotation;             //rotaion of the component

        //indexing properties
        public int indexInOutput;       //index in final output file
        protected int index;            //index in local class.As in 
        protected int G_matShift, B_matIndex, C_matIndex, D_matShift, Z_matIndex, X_matIndex;    //matrix manipulation indices

        //Not accessible from outside
        protected string _name;         //Proper name of component.
        protected double _primaryValue; //primary value of component.For resistor it's resistance, for capacitor it's capacitace and vise versa

        //For UI interaction
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name 
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    label.Text = _name + ", " + _primaryValue.ToString();
                    OnPropertyChanged("Name");
                }
            }
        }
        public double PrimaryValue
        {
            get { return _primaryValue; }
            set 
            {
                if (value != _primaryValue)
                {
                    _primaryValue = value;
                    label.Text = _name + ", " + _primaryValue.ToString();
                    OnPropertyChanged("PrimaryValue");
                }
            }
        }

        [DisplayName("Positive pin Node")]
        public int NetPositive  
        {
            get { return _netPos; }
            protected set
            {
                _netPos = value;
                OnPropertyChanged("NetPositive");
            }
        }

        [DisplayName("Negative pin Node")]
        public int NetNegative
        {
            get { return _netNeg; }
            protected set
            {
                _netNeg = value;
                OnPropertyChanged("NetNegative");
            }
        }
        //end UI

        //Pin properties
        //these are index of nodes of this component in mainGrid 
        [Browsable(false)]
        public int PinPos { get; protected set; }
        [Browsable(false)]
        public int PinNeg { get; protected set; }

        int _netPos;    //Net value at positive node
        int _netNeg;    //Net value at negative node

        //electrical properties
        //Derrived values.
        public double VoltageAccross { get; protected set; }
        public double CurrentThrough { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a component object in specified position and rotation.Sets the pin nodes and updates the grid automatically
        /// </summary>
        /// <param name="val">Primaty value of the component</param>
        /// <param name="posX">X axis component of position</param>
        /// <param name="posY">Y axis component of position</param>
        /// <param name="rot">rotation. HAS TO BE 0/90/180/270. Any other value is not valid</param>
        /// <param name="rect">The rectangle on which this component is drawn</param>
        public Component(double val, int posX, int posY, int rot, Rectangle rect)
        {
            _primaryValue = val;
            int x = posX / 50;
            int y = posY / 50;
            int componentIndex = y * SystemGrid.GridSize + x;
            _compSymb = rect;
            //initially we consider both pins out of the grid
            _netNeg = -1;
            _netPos = -1;

            if (SystemGrid.ComponentGrid.ContainsKey(componentIndex)) throw new ComponentOverlapExecption("Components are overlapping!");
            if (SystemGrid.MainGrid.ContainsKey(y * SystemGrid.GridSize + x)) throw new ComponentOverlapExecption("Components are overlapping!");
            
            //means in a horizontal orientation
            if(rot == 0 || rot == 180)
            {
                int p1 = y * SystemGrid.GridSize + x - 1;
                int p2 = y * SystemGrid.GridSize + x + 1;

                ManageNodes(p1);
                ManageNodes(p2);

                if (rot == 0)
                {
                    PinPos = p1;
                    PinNeg = p2;
                }
                else
                {
                    PinNeg = p1;
                    PinPos = p2;
                }
            }
            else if(rot == 90 || rot == 270)
            {
                int p1 = (y-1) * SystemGrid.GridSize + x;
                int p2 = (y+1) * SystemGrid.GridSize + x;

                ManageNodes(p1);
                ManageNodes(p2);

                if (rot == 90)
                {
                    PinPos = p1;
                    PinNeg = p2;
                }
                else
                {
                    PinNeg = p1;
                    PinPos = p2;
                }
            }

            //else rotation is not valid
            else
            {
                throw new InvalidComponentRotation("Component rotation is not valid!");
            }
            SystemGrid.ComponentGrid.Add(componentIndex, this);

            //adding client to node class's net changed event so that it can be updated
            SystemGrid.MainGrid[PinPos].NetChanged += OnConnectedNetChanged;
            SystemGrid.MainGrid[PinNeg].NetChanged += OnConnectedNetChanged;
        }
        #endregion

        #region Private Functions

        /// <summary>
        /// duh..manages node.checks if something already exists there...story of my life
        /// </summary>
        /// <param name="index">bullshit</param>
        int ManageNodes(int index)
        {
            //means a node already exists in the desired point.
            //so we take that as our node
            if (SystemGrid.MainGrid.ContainsKey(index)) return index;
            else
            {
                Node n = new Node(index, index);
                n.OwnerComponentCount++;
                SystemGrid.MainGrid.Add(index, n);
                return index;
            }
        }
        #endregion

        #region Protected virtuals
        //called when any UI element value is changed
        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// When the net of a Node is updated, it raises an event so that connected components 
        /// can also update their net
        /// </summary>
        /// <param name="sender">Node that is updated</param>
        /// <param name="NewNet">New net value</param>
        /// <param name="NodeIndx">Node index in Main grid</param>
        protected void OnConnectedNetChanged(object sender, int NewNet, int NodeIndx)
        {
            if (PinNeg == NodeIndx) NetNegative = NewNet;
            if (PinPos == NodeIndx) NetPositive = NewNet;
        }

        protected static TextBlock SetName(string name, double value, int px, int py)
        {

            return new TextBlock()
            {
                Text = name + ", " + value.ToString(),
                RenderTransform = new TranslateTransform(px + ComponentManager._ShiftX, py + ComponentManager._ShiftY),
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };
        }
        #endregion

        #region public virtuals
        //preparing for matrix operations
        public virtual void SetIndex() { return; }

        //matrix operations
        public virtual void Modify_G_Matrix(double[,] m) { return; }
        public virtual void Modify_B_Matrix(double[,] m) { return; }
        public virtual void Modify_C_Matrix(double[,] m) { return; }
        public virtual void Modify_D_Matrix(double[,] m) { return; }
        public virtual void Modify_Z_Matrix(double[] m) { return; }
        /// <summary>
        /// Disconnects the component from grid.It's safe to delete afterwards
        /// </summary>
        public virtual void Disconnect() 
        {
            if (!SystemGrid.MainGrid[PinPos].CheckPriority()) SystemGrid.MainGrid.Remove(PinPos);
            if (!SystemGrid.MainGrid[PinNeg].CheckPriority()) SystemGrid.MainGrid.Remove(PinNeg);
            ComponentManager.schematicsCanvas.Children.Remove(_compSymb);
            ComponentManager.schematicsCanvas.Children.Remove(label);
        }

        /// <summary>
        /// Returns the total number of components of this type
        /// </summary>
        /// <returns></returns>
        public virtual int GetParentOffset() { return 0; }
        /// <summary>
        /// Tells components to update their values after a single transient cycle
        /// </summary>
        public virtual void UpdateValues() { return; }

        //Fetching results
        /// <summary>
        /// Each component tries to aquire their respective values after simulation
        /// </summary>
        public virtual void SetCalculatedInfo() { }

        /// <summary>
        /// Each component provides additional information
        /// </summary>
        /// <returns>a string of their calculated vales</returns>
        public virtual string GetCalculatedInfo() { return VoltageAccross.ToString() + " " + CurrentThrough.ToString(); }
        #endregion
    }

    public class Resistor : Component
    {
        //netlist name prefix of the component
        static string NamePrefix = "R";
        //objects instantiated of this type
        static int _Counter = 0;
        //stuff for indexing
        static int _indexer = 0;

        public Resistor(double val, int posX, int posY, int rot, Rectangle rect) : base(val, posX, posY, rot, rect) 
        {
            _Counter++;
            _name = NamePrefix + _Counter.ToString();
            label = SetName(Name, val, posX, posY);
            ComponentManager.schematicsCanvas.Children.Add(label);
        }

        #region overrides

        public override void Modify_G_Matrix(double[,] m)
        {
            int _posNet = SystemGrid.MainGrid[PinPos].Net - 1;
            int _negNet = SystemGrid.MainGrid[PinNeg].Net - 1;

            if (_posNet >= 0) m[_posNet, _posNet] += 1 / PrimaryValue;
            if (_negNet >= 0) m[_negNet, _negNet] += 1 / PrimaryValue;

            if (_posNet >= 0 && _negNet >= 0)
            {
                m[_posNet, _negNet] -= 1 / PrimaryValue;
                m[_negNet, _posNet] -= 1 / PrimaryValue;
            }
        }

        //when the simulation is ended, it collects it's value from solution matrix
        public override void SetCalculatedInfo()
        {
            base.SetCalculatedInfo();
            int _posNet = SystemGrid.MainGrid[PinPos].Net;
            int _NegNet = SystemGrid.MainGrid[PinNeg].Net;

            double vP = _posNet > 0 ? SystemGrid.solutionMatrix[_posNet-1] : 0;
            double vN = _NegNet > 0 ? SystemGrid.solutionMatrix[_NegNet-1] : 0;

            VoltageAccross = vP - vN;
            CurrentThrough = VoltageAccross / PrimaryValue;
        }

        //returns a string that holds it's voltage and current to be included in final output file
        public override string GetCalculatedInfo()
        {
            return base.GetCalculatedInfo();
        }

        //Disconnects it from Grid.then it's safe to delete
        public override void Disconnect()
        {
            //must do override
            base.Disconnect();
            _Counter--;
        }

        //returns number of components of this type
        public override int GetParentOffset() { return _Counter; }
        public static void ResetAllIndices()
        {
            _indexer = 0;
        }

        #endregion
    }


    public class VoltageSource : Component
    {
        //netlist name prefix of the component
        static readonly string NamePrefix = "V";
        //count objects instantiated of this type
        static int _Counter = 0;
        //stuff for indexing
        static int _indexer = 0;

        public VoltageSource(double val, int posX, int posY, int rot, Rectangle r) : base(val, posX, posY, rot, r)
        {
            _Counter++;
            _name = NamePrefix + _Counter.ToString();
            label = SetName(Name, val, posX, posY);
            ComponentManager.schematicsCanvas.Children.Add(label);
        }

        //Constructor to be used by derrived classes for their own nameprefix
        public VoltageSource(double val, int posX, int posY, int rot, int count, string NPrefix, Rectangle r) : base(val, posX, posY, rot, r)
        {
            _name = NPrefix + count.ToString();
            label = SetName(_name, val, posX, posY);
            ComponentManager.schematicsCanvas.Children.Add(label);
        }

        public override void SetIndex()
        {
            index = _indexer;
            _indexer++;
            B_matIndex = index;
            C_matIndex = index;
            X_matIndex = index + SystemGrid.NodeCount;
            Z_matIndex = index + SystemGrid.NodeCount;
        }

        public override void Modify_B_Matrix(double[,] m)
        {
            int _posNet = SystemGrid.MainGrid[PinPos].Net;
            int _NegNet = SystemGrid.MainGrid[PinNeg].Net;

            if (_posNet > 0) m[_posNet - 1, B_matIndex] += 1;
            if (_NegNet > 0) m[_NegNet - 1, B_matIndex] += -1;
        }

        public override void Modify_C_Matrix(double[,] m)
        {
            int _posNet = SystemGrid.MainGrid[PinPos].Net;
            int _NegNet = SystemGrid.MainGrid[PinNeg].Net;

            if (_posNet > 0) m[C_matIndex, _posNet - 1] += 1;
            if (_NegNet > 0) m[C_matIndex, _NegNet - 1] += -1;
        }


        public override void Modify_Z_Matrix(double[] m)
        {
            m[Z_matIndex] += PrimaryValue;
        }

        public override void SetCalculatedInfo()
        {
            CurrentThrough = SystemGrid.solutionMatrix[X_matIndex];
        }

        public override string GetCalculatedInfo()
        {
            return base.GetCalculatedInfo();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _Counter--;
        }

        public override int GetParentOffset()
        {
            return _Counter;
        }

        public static void ResetAllIndices()
        {
            _indexer = 0;
        }

    }

    public class CurrentSource : Component
    {
        //netlist name prefix of the component
        public static string NamePrefix = "I";
        //count objects instantiated of this type
        static int _Counter = 0;
        //stuff for indexing
        static int _indexer = 0;

        public CurrentSource(double val, int posX, int posY, int rot, Rectangle r) : base(val, posX, posY, rot, r) 
        {
            _Counter++;
            _name = NamePrefix + _Counter.ToString();
            label = SetName(Name, val, posX, posY);
            ComponentManager.schematicsCanvas.Children.Add(label);
        }

        public override void SetIndex()
        {
            index = _indexer;
            _indexer++;
        }

        public override void Modify_Z_Matrix(double[] m)
        {
            int _posNet = SystemGrid.MainGrid[PinPos].Net;
            int _NegNet = SystemGrid.MainGrid[PinNeg].Net;

            if (_posNet > 0) m[_posNet - 1] -= PrimaryValue;
            if (_NegNet > 0) m[_NegNet - 1] += PrimaryValue;
        }

        public override void SetCalculatedInfo()
        {
            int _posNet = SystemGrid.MainGrid[PinPos].Net;
            int _NegNet = SystemGrid.MainGrid[PinNeg].Net;

            double vP = _posNet > 0? SystemGrid.solutionMatrix[_posNet - 1] : 0;
            double vN = _NegNet > 0? SystemGrid.solutionMatrix[_NegNet - 1]: 0;
            VoltageAccross = vP - vN;
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _Counter--;
        }

        public override string GetCalculatedInfo()
        {
            return base.GetCalculatedInfo();
        }

        public override int GetParentOffset()
        {
            return _Counter;
        }

        public static void ResetAllIndices()
        {
            _indexer = 0;
        }
    }

    public class Capacitor : VoltageSource
    {
        //initial condition
        protected double _ic;
        static readonly string NamePrefix = "C";
        static int _Counter;
        static int _indexer = 0;
        public double IC
        {
            get { return _ic; }
            set
            {
                _ic = value;
                VoltageAccross = _ic;
                OnPropertyChanged("IC");
            }
        }

        public Capacitor(double val, int posX, int posY, int rot, Rectangle r):base(val, posX, posY, rot, _Counter+1, NamePrefix, r)
        {
            _ic = 0;
            _Counter++;
        }

        public override void SetIndex()
        {
            index = _indexer;
            _indexer++;
            B_matIndex = index + base.GetParentOffset();
            C_matIndex = B_matIndex;
            X_matIndex = base.GetParentOffset() + SystemGrid.NodeCount + index;
            Z_matIndex = X_matIndex;
        }

        public override void Modify_B_Matrix(double[,] m)
        {
            base.Modify_B_Matrix(m);
        }

        public override void Modify_C_Matrix(double[,] m)
        {
            base.Modify_C_Matrix(m);
        }

        public override void Modify_Z_Matrix(double[] m)
        {
            m[Z_matIndex] += VoltageAccross;
        }

        public override void SetCalculatedInfo()
        {
            CurrentThrough = SystemGrid.solutionMatrix[X_matIndex];
        }

        public override string GetCalculatedInfo()
        {
            return base.GetCalculatedInfo();
        }

        public override void UpdateValues()
        {
            VoltageAccross += (CurrentThrough / PrimaryValue) * SystemGrid.TimeSpace;
            Console.WriteLine(VoltageAccross);
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _Counter--;
        }

        public override int GetParentOffset()
        {
            return _Counter + base.GetParentOffset();
        }

        public static new void ResetAllIndices()
        {
            _indexer = 0;
        }

    }

    public class Inductor : Component
    {
        //static string NamePrefix = "L";
        public Inductor(double val, int posX, int posY, int rot, Rectangle r) : base(val, posX, posY, rot, r) 
        {

        }
    }

    public class GND
    {
        public int PinPos { get; private set; }
        Rectangle _symbolRect;

        public GND(int posX, int posY, int rot, Rectangle r)
        {
            int x = posX / 50;
            int y = posY / 50;
            int componentIndex = y * SystemGrid.GridSize + x;
            _symbolRect = r;

            if (SystemGrid.ComponentGrid.ContainsKey(componentIndex)) throw new ComponentOverlapExecption("Components are overlapping!");
            if (SystemGrid.MainGrid.ContainsKey(y * SystemGrid.GridSize + x)) throw new ComponentOverlapExecption("Components are overlapping!");

            //means in a horizontal orientation
            if (rot == 0 || rot == 180)
            {
                int p1 = (y + (rot == 0 ? -1 : 1)) * SystemGrid.GridSize + x;
                ManageNodes(p1);
                PinPos = p1;
            }
            else if (rot == 90 || rot == 270)
            {
                int p1 = y * SystemGrid.GridSize + (x + (rot == 90 ? 1 : -1));
                ManageNodes(p1);
                PinPos = p1;
            }
            SystemGrid.groundList.Add(this);
        }

        int ManageNodes(int index)
        {
            //means a node already exists in the desired point.
            //so we take that as our node
            if (SystemGrid.MainGrid.ContainsKey(index)) return index;
            else
            {
                Node n = new Node(index, 0);
                SystemGrid.MainGrid.Add(index, n);
                return index;
            }
        }

    }


}
