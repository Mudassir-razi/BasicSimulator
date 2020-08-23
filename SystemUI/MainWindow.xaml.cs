using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Grid;
using Toolset;
using ResourceManager;
using System.Windows.Controls.DataVisualization;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Threading;

namespace SystemUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Canvas canvas;
        public ObservableCollection<Grid.Component> tc;
        public SimInformer Log;
        public MainWindow()
        {
            //splash screen
            //StartUpSplash splash = new StartUpSplash();
            //splash.Show();
            
            //main code
            InitializeComponent();
            
            this.WindowState = WindowState.Maximized;
            this.Title = "Node";
            ComponentSymbols.LoadSymbols();
            Log = SystemGrid.Informer;

            ConsoleManager.Show();
            try
            {
                canvas = VisualGrid.GenerateGrid();
                VisualGrid.SetStatusIndicator(this.Status);
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
            this.Output.ItemsSource = Log.GetInfo();


            //adding functions
            VisualGrid.SelectedObjectChanged += PropertyGridSelectionChange;
            this.ComponentSelector.SelectionChanged += ComponentSelector_SelectionChanged;
            this.canvas.MouseWheel += Canvas_MouseWheel;
            this.canvas.MouseMove += Canvas_MouseMove;
            this.canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            this.ParentCanvas.MouseWheel += ParentCanvas_MouseWheel;
            this.KeyDown += KeyBoarInputPreview;
            //test
            this.Button_Simulate.Click += Test_Click;

            //end splash
            //Thread.Sleep(2000);
            //splash.Close();
        }


        //test
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            SystemGrid.AttemptSimulation();
        }

        //when leftclick on canvas
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VisualGrid.currentState.OnMouseClick(e);
        }

        //when mouse moves over the canvas
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            VisualGrid.currentState.OnMouseMove(e);
        }

        //takes keyboard input
        private void KeyBoarInputPreview(object sender, KeyEventArgs e)
        {
            //VisualGridManager.Pan(e);
            VisualGrid.currentState.OnKeyBoardEvent(e);
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
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) VisualGrid.ZoomInOut(e.Delta);

            //pan while holding alter
            else VisualGrid.Pan(e.Delta, Keyboard.IsKeyDown(Key.LeftAlt));
        }


        //check it later
        private void ComponentSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisualGrid.ChangeState(typeof(ComponentState));
            if (VisualGrid.currentState.GetType() == typeof(ComponentState))
            {
                ComponentState cs = (ComponentState)VisualGrid.currentState;
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

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        //test
        private void Button_VoltageProbe_Click(object sender, RoutedEventArgs e)
        {
            VisualGrid.currentState.OnVoltageProbe();
        }

        public void PropertyGridSelectionChange(object o)
        {
            this.PropertyViewer.SelectedObject = o;
        }

        private void Slider_vertical_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
           
        }

        private void Button_simulationSetting_Click(object sender, RoutedEventArgs e)
        {
            SimulationSettingWindow simWindow = new SimulationSettingWindow();
            simWindow.Show();
        }

        private void Button_simulationOutput_Click(object sender, RoutedEventArgs e)
        {
            SimulationOutput outputWin = new SimulationOutput();
            outputWin.Show();
        }

        private void Button_CurrentProbe_Click(object sender, RoutedEventArgs e)
        {
            VisualGrid.currentState.OnCurrentProbe();
        }
    }



}
