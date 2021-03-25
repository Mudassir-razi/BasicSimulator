using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Toolset;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.CodeDom;
using System.Windows.Media.Animation;

namespace Grid
{
    public static class VisualGrid
    {
        public static Canvas CurrentCanvas { get; private set; }
        static double zoom;
        static int shiftX, shiftY;
        static System.Windows.Shapes.Rectangle NodeIndicator;
        public static Dictionary<Type, BaseState> availableStates { get; private set; }

        public static Dictionary<Type, ImageBrush> SymbolList;
        public static BaseState currentState;
        public static TextBlock StatusIndicator;    //shows status of manager. For now

        public delegate void ObjectSelectionHandler(object o);
        public static event ObjectSelectionHandler SelectedObjectChanged;

        public static int NodeIndicatorIndex;
        public static int SystemReserveChildern;
        /// <summary>
        /// Generates grid lines in the given canvas
        /// </summary>
        /// <param name="c">Canvas to draw on</param>
        /// <param name="space">Space between grid lines</param>
        public static Canvas GenerateGrid(int space = 50)
        {
            zoom = 1;
            NodeIndicator = new Rectangle();
            NodeIndicator.Width = 4;
            NodeIndicator.Height = 4;
            NodeIndicator.Fill = Brushes.Black;

            //init canvas
            CurrentCanvas = new Canvas
            {
                Height = 1000,
                Width = 1000,
                Background = (Brush)new BrushConverter().ConvertFrom("#303030"),
                Focusable = true
            };

            //init states
            availableStates = new Dictionary<Type, BaseState>()
            {
                {typeof(SelectionState), new SelectionState(CurrentCanvas) },
                {typeof(WireState), new WireState(CurrentCanvas) },
                {typeof(ComponentState), new ComponentState(CurrentCanvas) },
                {typeof(ProbeState), new ProbeState(CurrentCanvas) }
            };

            currentState = availableStates.Values.First();

            if (CurrentCanvas.Width <= 0) throw (new InvalidCanvasSizeExeption("Canvas size is invalid"));

            //Drawing grid lines now
            int size = (int)CurrentCanvas.Width;
            for (int i = 0; i < size;)
            {
                //horizontal Lines
                Line l = new Line
                {
                    Stroke = Brushes.Gray,
                    //Width = 0.9,
                    X1 = 0,
                    X2 = size,
                    Y1 = i,
                    Y2 = i,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = false
                };
                CurrentCanvas.Children.Add(l);
                //vertical lines
                l = new Line
                {
                    Stroke = Brushes.Gray,
                    //Width = 0.9,
                    X1 = i,
                    X2 = i,
                    Y1 = 0,
                    Y2 = size,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = false
                };
                CurrentCanvas.Children.Add(l);
                i += space;
            }
            NodeIndicatorIndex = CurrentCanvas.Children.Count;
            CurrentCanvas.Children.Add(NodeIndicator);
            SystemReserveChildern = CurrentCanvas.Children.Count;
            return CurrentCanvas;
        }

        //zoom function.
        //called by states
        public static void ZoomInOut(double deltaZ)
        {
            if (deltaZ != 0)
            {
                if (deltaZ > 0)
                {
                    zoom += 0.1;
                }
                else
                {
                    zoom -= 0.1;
                }

            }
            zoom = Mathf.Clamp(zoom, 0.8, 1.3);
            Console.WriteLine(zoom);
            ScaleTransform st = new ScaleTransform(zoom, zoom);
            CurrentCanvas.RenderTransform = st;
        }

        //also called by states
        public static void Pan(int delta, bool vertical)
        {
            int del = 0;
            if (delta > 0) del = 50;
            else if (delta < 0) del = -50;

            if (!vertical) shiftY += del;
            else shiftX += del;

            Console.WriteLine(shiftX.ToString() + ", " + shiftY.ToString());
            Canvas.SetLeft(CurrentCanvas, shiftX);
            Canvas.SetTop(CurrentCanvas, shiftY);
        }

        //Handles statechange and transition functions
        public static void ChangeState(Type t)
        {
            currentState.OnExitState();
            currentState = availableStates[t];
            currentState.OnEnterState();
        }

        public static void SetStatusIndicator(TextBlock block) { StatusIndicator = block; }

        public static void RequestSelectionChange(Component o)
        {
            SelectedObjectChanged?.Invoke(o);
        }

    }

    //exeptions:not necessary tho
    public class InvalidCanvasSizeExeption : Exception
    {
        public InvalidCanvasSizeExeption(String Message) : base(Message) { }
    }

    //state machine's basestates.Contains all basic functions
    public class BaseState
    {
        protected Canvas currentCanvas;
        protected Type nextState;
        protected int posX, posY;

        public BaseState(Canvas c)
        {
            currentCanvas = c;
            nextState = null;
        }

        public virtual void OnEnterState() { }
        public virtual void OnExitState() { }

        public virtual void OnVoltageProbe()
        {
            ProbeState p = (ProbeState)VisualGrid.availableStates[typeof(ProbeState)];
            p.ptype = ProbeType.voltage;
            VisualGrid.ChangeState(typeof(ProbeState));
        }

        public virtual void OnCurrentProbe()
        {
            ProbeState p = (ProbeState)VisualGrid.availableStates[typeof(ProbeState)];
            p.ptype = ProbeType.current;
            VisualGrid.ChangeState(typeof(ProbeState));
        }

        public virtual void OnMouseClick(MouseEventArgs e) { }
        public virtual void OnMouseMove(MouseEventArgs e)
        {
            posX = (int)Math.Round(e.GetPosition(currentCanvas).X / 50) * 50;
            posY = (int)Math.Round(e.GetPosition(currentCanvas).Y / 50) * 50;
            Canvas.SetLeft((Rectangle)currentCanvas.Children[VisualGrid.NodeIndicatorIndex], posX - 2);
            Canvas.SetTop((Rectangle)currentCanvas.Children[VisualGrid.NodeIndicatorIndex], posY - 2);
        }

        public virtual void OnKeyBoardEvent(KeyEventArgs e) 
        {
            if(e.Key == Key.Enter)
            {
                currentCanvas.Focus();
                Console.WriteLine("Defocusing");
            }
        }


    }

    //state for drawing wires.
    public class WireState : BaseState
    {
        bool started;
        int lastKnownX;
        int lastKnownY;
        int tempLineIndex;
        Line tempLine;

        public WireState(Canvas c) : base(c) { }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            //adds connection in grid 
            if (started)
            {
                //create wire
                try
                {
                    Wire newWire = new Wire((int)tempLine.X1, (int)tempLine.Y1, (int)tempLine.X2, (int)tempLine.Y2);
                }
                catch(WireOverlapException ex)
                {
                    MessageBox.Show(ex.Message, "Error!",MessageBoxButton.OK,MessageBoxImage.Error);
                    tempLine = null;
                    currentCanvas.Children.RemoveAt(tempLineIndex);
                    started = false;
                    return;
                }
            }

            //creates new wire.
            //and continues to create from the end point of the last created wire

            tempLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 2,
                X1 = started ? lastKnownX : posX,
                Y1 = started ? lastKnownY : posY,
                X2 = posX,
                Y2 = posY,
                SnapsToDevicePixels = true,
                UseLayoutRounding = false,
            };

            
            tempLineIndex = currentCanvas.Children.Count;
            currentCanvas.Children.Add(tempLine);

            //Do stuff for node wire
            
            if(!started) started = true;


            VisualGrid.StatusIndicator.Text = "Wire holding: " + started.ToString();
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            VisualGrid.StatusIndicator.Text = "Wire holding: " + started.ToString();
            if(tempLine != null)VisualGrid.StatusIndicator.Text = String.Format("Thickness:{0}", tempLine.StrokeThickness);

            if (started)
            {
                Line l = (Line)currentCanvas.Children[tempLineIndex];
                VisualGrid.StatusIndicator.Text += "\n" + posX.ToString() + ", " + posY.ToString();

                //we can add wire only horizontally or vertically.
                double difX = Mathf.Difference(l.X1, posX);
                double dify = Mathf.Difference(l.Y1, posY);

                if(difX > dify)
                {
                    l.X2 = posX;
                    l.Y2 = l.Y1;
                }
                else
                {
                    l.X2 = l.X1;
                    l.Y2 = posY;
                }

                lastKnownX = (int)l.X2;
                lastKnownY = (int)l.Y2;
            }
        }

        public override void OnKeyBoardEvent(KeyEventArgs e)
        {
            base.OnKeyBoardEvent(e);
            if (e.Key == Key.Escape)
            {
                //means we were drawing and now want to quit
                if (started)
                {
                    started = false;
                    currentCanvas.Children.RemoveAt(tempLineIndex);
                    tempLine = null;
                }

                //if not, then we want to switch to selection mode
                else
                {
                    VisualGrid.ChangeState(typeof(SelectionState));
                }
            }
        }
    }
    //state for selecting components in grid.DOES NOT WORK PROPERLY right now.
    public class SelectionState : BaseState
    {
        Component _selectedComponent;

        public Component SelectedComponent
        {
            get { return _selectedComponent; }
            set { _selectedComponent = value; }
        }

        public SelectionState(Canvas c) : base(c) {}

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        public override void OnKeyBoardEvent(KeyEventArgs e)
        {
            base.OnKeyBoardEvent(e);
            if (e.Key == Key.W)
            {
                VisualGrid.ChangeState(typeof(WireState));
            }

            else if(e.Key == Key.P)
            {
                VisualGrid.ChangeState(typeof(ComponentState));
            }

            else if(e.Key == Key.Delete || e.Key == Key.Back)
            {
                ComponentManager.DeleteComponent(posX, posY, SelectedComponent);
                SelectedComponent = null;
            }
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            //when clicked, it searches for object.
            //and yes currently just for objects
            int px = posX / 50;
            int py = posY / 50;
            int selectionKey = py * SystemGrid.GridSize + px;
            if(SystemGrid.ComponentGrid.ContainsKey(selectionKey))
            {
                SelectedComponent = SystemGrid.ComponentGrid[selectionKey];
                Console.WriteLine("Selected component" + SelectedComponent.Name);
                VisualGrid.RequestSelectionChange(SelectedComponent);
            }
        }
    }
    //state for adding new component in the grid.
    public class ComponentState : BaseState
    {
        int tempComponentIndex;
        int rotation;
        public Type ComponentType;
        Rectangle tempR;

        public ComponentState(Canvas c) : base(c) 
        {
            rotation = 0;
            ComponentType = VisualGrid.SymbolList.Keys.First();
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
        }

        public override void OnExitState()
        {
            base.OnExitState();
            currentCanvas.Children.RemoveAt(tempComponentIndex);
            rotation = 0;
            tempR = null;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (tempR == null) CreateTemporaryComp_Visual();

            Canvas.SetLeft(tempR, posX - 50);
            Canvas.SetTop(tempR, posY - 50);
        }


        public override void OnKeyBoardEvent(KeyEventArgs e)
        {
            base.OnKeyBoardEvent(e);
            if(e.Key == Key.Escape)
            {
                VisualGrid.ChangeState(typeof(SelectionState));
            }
            if(e.Key == Key.R)
            {
                if(tempR != null)
                {
                    rotation += 90;
                    rotation = rotation > 270 ? 0 : rotation;
                    tempR.RenderTransform = new RotateTransform(rotation, 50, 50);
                }
            }
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            try
            {
                ComponentManager.CreateComponent(ComponentType, 100, posX, posY, rotation, tempR);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                currentCanvas.Children.RemoveAt(tempComponentIndex);
                tempR = null;
            }
            tempR = null;
            CreateTemporaryComp_Visual();
        }

        public void CreateTemporaryComp_Visual()
        {
            tempR = new Rectangle
            {
                Height = 100,
                Width = 100,
                Fill = VisualGrid.SymbolList[ComponentType]
            };
            tempComponentIndex = currentCanvas.Children.Count;
            tempR.RenderTransform = new RotateTransform(rotation, 50, 50);
            currentCanvas.Children.Add(tempR);
            Canvas.SetLeft(tempR, posX - 50);
            Canvas.SetTop(tempR, posY - 50);
        }
    }

    /// <summary>
    /// State for placing probes
    /// </summary>
    public class ProbeState : BaseState
    {
        public ProbeType ptype;
        Rectangle _tempR;
        public ProbeState(Canvas c) : base(c) { }

        public override void OnMouseClick(MouseEventArgs e)
        {
            try
            {
                new Probe(posX, posY, ptype, _tempR);
                CreateVisual();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            CreateVisual();
        }

        public override void OnKeyBoardEvent(KeyEventArgs e)
        {
            base.OnKeyBoardEvent(e);
            if (e.Key == Key.Escape)
            {
                VisualGrid.ChangeState(typeof(SelectionState));
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_tempR == null) CreateVisual();

            Canvas.SetLeft(_tempR, posX);
            Canvas.SetTop(_tempR, posY - 100);
        }

        public override void OnExitState()
        {
            base.OnExitState();
            if(_tempR != null)VisualGrid.CurrentCanvas.Children.Remove(_tempR);
            _tempR = null;
        }

        public override void OnCurrentProbe()
        {
            if (ptype == ProbeType.voltage) base.OnCurrentProbe();
        }

        public override void OnVoltageProbe()
        {
            if(ptype == ProbeType.current)base.OnVoltageProbe();
        }

        public void CreateVisual()
        {
            _tempR = new Rectangle
            {
                Fill = VisualGrid.SymbolList[typeof(Probe)],
                Height = 100,
                Width = 100
            };
            VisualGrid.CurrentCanvas.Children.Add(_tempR);
        }
    }
}
