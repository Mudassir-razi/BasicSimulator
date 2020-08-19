using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Grid;
using Toolset;

namespace SystemUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    { 
        Canvas canvas;
        public ObservableCollection<Grid.Component> tc;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.Title = "Node";
            this.DataContext = this;

            //testing
            tc = new ObservableCollection<Grid.Component>(SystemGrid.ComponentGrid.Values);
            this.TestDataGrid.ItemsSource = tc;


            ComponentManager.LoadSymbols();


            ConsoleManager.Show();
            try
            {
                canvas = VisualGridManager.GenerateGrid();
                VisualGridManager.SetStatusIndicator(this.Status);
                ParentCanvas.Children.Add(canvas);
            }
            catch (InvalidCanvasSizeExeption e)
            {
                Console.WriteLine(e.Message);
                MessageBox.Show(e.Message);
            }

            ComponentManager.schematicsCanvas = canvas;
            //databinding
            this.ComponentSelector.ItemsSource = ComponentManager.GetSymbolList();

            //adding functions
            this.ComponentSelector.SelectionChanged += ComponentSelector_SelectionChanged;
            this.canvas.MouseWheel += Canvas_MouseWheel;
            this.canvas.MouseMove += Canvas_MouseMove;
            this.canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            this.ParentCanvas.MouseWheel += ParentCanvas_MouseWheel;
            this.KeyDown += KeyBoarInputPreview;

            //test
            this.Button_Simulate.Click += Test_Click;
        }


        //test
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            SystemGrid.DebugGridInfo();
        }

        //when leftclick on canvas
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VisualGridManager.currentState.OnMouseClick(e);
        }

        //when mouse moves over the canvas
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            VisualGridManager.currentState.OnMouseMove(e);
        }

        //takes keyboard input
        private void KeyBoarInputPreview(object sender, KeyEventArgs e)
        {
            //VisualGridManager.Pan(e);
            VisualGridManager.currentState.OnKeyBoardEvent(e);
        }

        //does the same thing with parent canvas
        private void ParentCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Canvas_MouseWheel(sender, e);
        }

        //happens when mouse wheel is turned on canvas
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //control if pressed it zooms in or out.
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) VisualGridManager.ZoomInOut(e.Delta);

            //pan while holding alter
            else VisualGridManager.Pan(e.Delta, Keyboard.IsKeyDown(Key.LeftAlt));
        }


        //check it later
        private void ComponentSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisualGridManager.ChangeState(typeof(ComponentState));
            if(VisualGridManager.currentState.GetType() == typeof(ComponentState))
            {
                ComponentState cs = (ComponentState)VisualGridManager.currentState;
                cs.ComponentType = (Type)ComponentSelector.SelectedItem;
                cs.CreateTemporaryComp_Visual();
            }
        }

        /// <summary>
        /// Sets the color of the rectangles surrounding the visual Node grid
        /// </summary>
        /// <param name="c"> desired color </param>
        public void SetOverlayRectanglesColor(Brush br)
        {
            this.OverlayRect0.Fill = br;
            this.OverlayRect1.Fill = br;
            this.OverlayRect2.Fill = br;
            this.OverlayRect3.Fill = br;
        }

        private void updateDataGrid_Click(object sender, RoutedEventArgs e)
        {
            if(SystemGrid.ComponentGrid.Count > 0)
            {
                
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

    }

    public class TestClass
    {
        int id;

        public int Id
        {
            get { return id; }
            set { if (value != id) id = value; }
        }

        string name;
        public string Name
        {
            get { return name; }
            set { if (name != value) name = value; }
        }

        public TestClass(int i, string n) { id = i;name = n; }

    }

}
