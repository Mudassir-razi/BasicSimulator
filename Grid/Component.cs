using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ResourceManager;

namespace Grid
{
    public static class ComponentManager
    {
        public static Canvas schematicsCanvas;
        static readonly int _ShiftX = -20;
        static readonly int _ShiftY = -30;

        public static Dictionary<Type, String> SymbolDir = new Dictionary<Type, string>()
        {
            {typeof(Resistor), "file:///C:/Users/Czone/source/repos/Nodes/resources/resistor.png" },
            {typeof(VoltageSource), "file:///C:/Users/Czone/source/repos/Nodes/resources/voltage_independent.png" },
            {typeof(CurrentSource), "file:///C:/Users/Czone/source/repos/Nodes/resources/current_independent.png" },
            {typeof(Capacitor), "file:///C:/Users/Czone/source/repos/Nodes/resources/meme1.png" },
            {typeof(Inductor), "file:///C:/Users/Czone/source/repos/Nodes/resources/gnd.png" },
            {typeof(GND), "file:///C:/Users/Czone/source/repos/Nodes/resources/gnd.png" },

        };


        /// <summary>
        /// Creates a component object of specified type in given position and rotation.Sets the pin nodes and updates the grid automatically
        /// </summary>
        /// <param name="T">Type of the component</param>
        /// <param name="val">Primaty value of the component</param>
        /// <param name="pX">X axis component of position</param>
        /// <param name="pY">Y axis component of position</param>
        /// <param name="rot">rotation. HAS TO BE 0/90/180/270. Any other value is not valid</param>
        public static void CreateComponent (Type T, double val, int pX, int pY, int rot)
        {
            if (T == typeof(Resistor))
            {
                new Resistor(val,pX,pY,rot);
                SystemGrid.OnProptertyChanged("hello");
            }
            else if(T == typeof(VoltageSource))
            {
                new VoltageSource(val, pX, pY, rot);
            }
            else if(T == typeof(CurrentSource))
            {
                new CurrentSource(val, pX, pY, rot);
            }
            else if(T == typeof(GND))
            {
                new GND(pX, pY, rot);
            }
        }

        //Has dependency
        public static void LoadSymbols()
        {
            ComponentSymbols.LoadSymbols(SymbolDir);
        }

        public static List<Type> GetSymbolList()
        {
            List<Type> sl = new List<Type>();
            foreach (Type t in SymbolDir.Keys)
            {
                sl.Add(t);
            }
            return sl;
        }


        public static TextBlock SetComponentName(string Prefix, double value, int pX, int pY)
        {
            return new TextBlock()
            {
                Text = Prefix + ", " + value.ToString(),
                RenderTransform = new TranslateTransform(pX + _ShiftX, pY + _ShiftY),
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };
        }

        public static void UpdateComponentIndex()
        {
            foreach(Component c in SystemGrid.ComponentGrid.Values)
            {
                c.UpdateIndex();
            }
        }

    }

    public class Component
    {
        protected static string SymbolDirectory;
        protected int indexInCanvas;
        public string Name { get; protected set; }
        public double PrimaryValue { get; protected set; }
        public int PinPos { get; protected set; }
        public int PinNeg { get; protected set; }

        protected TextBlock label;

        protected int rotation;
        protected int index, G_matShift, B_matIndex, C_matIndex, D_matShift, Z_matIndex, X_matIndex;

        //electrical values
        protected double voltageAccross;
        protected double currentThrough;
        protected int solutionIndex;

        //no need.delete it.from here and from inherited classes.
        public Component(double val)
        {
            PrimaryValue = val;
        }


        /// <summary>
        /// Creates a component object in specified position and rotation.Sets the pin nodes and updates the grid automatically
        /// </summary>
        /// <param name="val">Primaty value of the component</param>
        /// <param name="posX">X axis component of position</param>
        /// <param name="posY">Y axis component of position</param>
        /// <param name="rot">rotation. HAS TO BE 0/90/180/270. Any other value is not valid</param>
        public Component(double val, int posX, int posY, int rot)
        {
            PrimaryValue = val;
            int x = posX / 50;
            int y = posY / 50;
            int componentIndex = y * SystemGrid.GridSize + x;

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
        }


        /// <summary>
        /// dug..manages node.checks if something already exists there...story of my life
        /// </summary>
        /// <param name="index">bullshit</param>
        int ManageNodes(int index)
        {
            //means a node already exists in the desired point.
            //so we take that as our node
            if (SystemGrid.MainGrid.ContainsKey(index))return index;
            else
            {
                Node n = new Node(index, index);
                SystemGrid.MainGrid.Add(index, n);
                return index;
            }
        }

        //preparing for matrix operations
        public virtual void UpdateIndex() { return; }

        //matrix operations
        public virtual void Modify_G_Matrix(double[,] m) { return; }
        public virtual void Modify_B_Matrix(double[,] m) { return; }
        public virtual void Modify_C_Matrix(double[,] m) { return; }
        public virtual void Modify_D_Matrix(double[,] m) { return; }
        public virtual void Modify_Z_Matrix(double[] m) { return; }

        //Fetching results
        /// <summary>
        /// Each component tries to aquire their respective values after simulation
        /// </summary>
        public virtual void SetCalculatedInfo() { }

        /// <summary>
        /// Each component provides additional information
        /// </summary>
        /// <returns>a string of their calculated vales</returns>
        public virtual string GetCalculatedInfo() { return null; }
    }

    public class Resistor : Component
    {
        //netlist name prefix of the component
        static string NamePrefix = "R";

        //objects instantiated of this type
        public static int Counter = 0;

        public Resistor(double val) : base(val) { }

        public Resistor(double val, int posX, int posY, int rot) : base(val, posX, posY, rot) 
        {
            Counter++;
            Name = NamePrefix + Counter.ToString();
            label = ComponentManager.SetComponentName(Name, PrimaryValue, posX, posY);
            ComponentManager.schematicsCanvas.Children.Add(label);
        }

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

        public override void SetCalculatedInfo()
        {
            base.SetCalculatedInfo();
            int _posNet = SystemGrid.MainGrid[PinPos].Net;
            int _NegNet = SystemGrid.MainGrid[PinNeg].Net;

            double vP = _posNet > 0 ? SystemGrid.solutionMatrix[_posNet-1] : 0;
            double vN = _NegNet > 0 ? SystemGrid.solutionMatrix[_NegNet-1] : 0;

            voltageAccross = vP - vN;
            currentThrough = voltageAccross / PrimaryValue;
        }

        public override string GetCalculatedInfo()
        {
            return Name + " Voltage accross: " + voltageAccross.ToString() + ", Current through: " + currentThrough.ToString();
        }

    }

    
    public class VoltageSource : Component
    {
        //netlist name prefix of the component
        public static string NamePrefix = "V";

        //count objects instantiated of this type
        public static int Counter = 0;

        //stuff for indexing
        public static int indexer = 0;

        public VoltageSource(double val) : base(val) { }

        public VoltageSource(double val, int posX, int posY, int rot) : base(val, posX, posY, rot)
        {
            index = Counter;
            Counter++;
            Name = NamePrefix + Counter.ToString();
            label = ComponentManager.SetComponentName(Name, PrimaryValue, posX, posY);
            ComponentManager.schematicsCanvas.Children.Add(label);
        }

        public override void UpdateIndex()
        {
            index = indexer;
            indexer++;
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
            currentThrough = SystemGrid.solutionMatrix[X_matIndex];
        }

        public override string GetCalculatedInfo()
        {
            return Name + " Current through: " + currentThrough.ToString();
        }

    }

    public class CurrentSource : Component
    {
        //netlist name prefix of the component
        public static string NamePrefix = "I";

        //count objects instantiated of this type
        public static int Counter = 0;

        //stuff for indexing
        public static int indexer = 0;

        public CurrentSource(double val) : base(val) { }
        public CurrentSource(double val, int posX, int posY, int rot) : base(val, posX, posY, rot) 
        {
            index = Counter;
            Counter++;
            Name = NamePrefix + Counter.ToString();
            label = ComponentManager.SetComponentName(Name, PrimaryValue, posX, posY);
            ComponentManager.schematicsCanvas.Children.Add(label);
        }

        public override void UpdateIndex()
        {
            index = indexer;
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
            voltageAccross = vP - vN;
        }

        public override string GetCalculatedInfo()
        {
            return Name + " voltage accross: " + voltageAccross.ToString();
        }

    }

    public class Capacitor : Component
    {
        public Capacitor(double val) : base(val) { }
        public Capacitor(double val, int posX, int posY, int rot) : base(val, posX, posY, rot) { }
    }

    public class Inductor : Component
    {
        public Inductor(double val) : base(val) { }
        public Inductor(double val, int posX, int posY, int rot) : base(val, posX, posY, rot) { }
    }

    public class GND
    {
        public int PinPos { get; private set; }

        public GND(int posX, int posY, int rot)
        {
            int x = posX / 50;
            int y = posY / 50;
            int componentIndex = y * SystemGrid.GridSize + x;

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
