using Grid;
using Prism.Mvvm;
using ResourceManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Toolset;

namespace SystemUI
{
    /// <summary>
    /// Interaction logic for SimulationOutput.xaml
    /// </summary>
    public partial class SimulationOutput : Window
    {
        List<DataHolder> SingleProbeDataset;
        List<List<DataHolder>> Dataset;
        

        Line leftMouse_vertical;
        Line LeftMouse_horizontal;

        public SimulationOutput()
        {
            InitializeComponent();
            this.plotHolder.MouseLeftButtonDown += Plot_MouseLeftButtonDown;
            leftMouse_vertical = new Line();
            LeftMouse_horizontal = new Line();
            this.plotHolder.Children.Add(leftMouse_vertical);
            this.plotHolder.Children.Add(LeftMouse_horizontal);
            Dataset = new List<List<DataHolder>>();

            
            //creating dataholder for each probe
            foreach(Probe p in SystemGrid.probeList)
            {
                SingleProbeDataset = new List<DataHolder>();
                Dataset.Add(SingleProbeDataset);
            }

            GetDataForProbe();
            for(int iter = 0;iter < SystemGrid.probeList.Count;iter++)
            {
                Style CurveStyle = new Style(typeof(LineSeries));

                Style lineStyle = new Style(typeof(Polyline));
                Style defaultLine = (Style)Resources["CommonLineSeriesPolyline"];
                //lineStyle.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, Shape.));

                lineStyle.Setters.Add(new Setter(Polyline.StrokeProperty, SequentialBrushGenerator(iter)));
                lineStyle.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, (double)2));

                CurveStyle.Setters.Add(new Setter(LineSeries.PolylineStyleProperty, lineStyle));

                LineSeries ls = new LineSeries();
                //ls.Name = "Hello";
                ls.Title = SystemGrid.probeList[iter].GetAppropriateName();
                ls.Background = Brushes.Black;
                ls.Foreground = Brushes.LightBlue;
                ls.ItemsSource = Dataset[iter];
                ls.IndependentValuePath = "ind_value";
                ls.DependentValuePath = "dep_value";
                ls.Style = CurveStyle;
                this.plot.Series.Add(ls);
            }
            
            //Foo();
        }

        private void Plot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p =  e.GetPosition((Canvas)sender);
            this.plot.BorderThickness = new Thickness(0);
            leftMouse_vertical.X1 = p.X;
            leftMouse_vertical.X2 = p.X;
            leftMouse_vertical.Y1 = 0;
            leftMouse_vertical.Y2 = this.plotHolder.ActualHeight;

            LeftMouse_horizontal.X1 = 0;
            LeftMouse_horizontal.X2 = this.plotHolder.ActualWidth;
            LeftMouse_horizontal.Y1 = p.Y;
            LeftMouse_horizontal.Y2 = p.Y;

            leftMouse_vertical.Stroke = Brushes.Red;
            LeftMouse_horizontal.Stroke = Brushes.Red;
        }

        void GetDataForProbe()
        {
            //first read files
            double time = 0;
            char[] separator = { ' ' };
            string[] valueSets = File.ReadAllLines(ComponentSymbols.filePath);
            int increment = 5;//valueSets.Length/1000;
            for(int i = 1;i < valueSets.Length;i += increment)
            {
                int probeIndex = 0;
                foreach(Probe p in SystemGrid.probeList)
                {
                    double iVal = time;
                    string singleRow = valueSets[i];
                    string[] splittedSingleRow = singleRow.Split(separator);
                    double dval = Mathf.Scientific(splittedSingleRow[p.GetValuesForProbe()]);
                    Dataset[probeIndex].Add(new DataHolder(iVal, dval));
                    probeIndex++;
                }
                time += SystemGrid.dTime * i;
            }
            
        }

        Brush SequentialBrushGenerator(int index)
        {
            Brush result = Brushes.Transparent;

            PropertyInfo[] info = typeof(Brushes).GetProperties();
            index = index >= info.Length ? 0 : index;
            result = (Brush)info[index].GetValue(null, null);
            return result;
        }

        //test function
        public void Foo()
        {
            LineSeries ls = new LineSeries();
            List<DataHolder> dh = new List<DataHolder>();
            double dtime = 0.0001, endtime = 0.5, time = 0;
            double freq = 10;
            for (; time < endtime; time += dtime)
            {
                Console.WriteLine(time);
                dh.Add(new DataHolder(time, Math.Sin(time * 2 * Math.PI * freq)));
            }
            ls.ItemsSource = dh;
            ls.IndependentValuePath = "ind_value";
            ls.DependentValuePath = "dep_value";
            ls.Style = (Style)Resources["lineStyle1"];
            this.plot.Series.Add(ls);
        }
    }

    public class DataHolder : BindableBase
    {
        double _ind_value;
        double _dep_value;

        public double ind_value
        {
            get { return _ind_value; }
            set { SetProperty(ref _ind_value, value); }
        }

        public double dep_value
        {
            get { return _dep_value; }
            set { SetProperty(ref _dep_value, value); }
        }

        public DataHolder(double independentVal, double dependentVal)
        {
            ind_value = independentVal;
            dep_value = dependentVal;
        }
    }
}
