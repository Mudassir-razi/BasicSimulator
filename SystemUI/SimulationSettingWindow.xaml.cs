using Grid;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Toolset;


namespace SystemUI
{
    /// <summary>
    /// Interaction logic for SimulationSettingWindow.xaml
    /// </summary>
    public partial class SimulationSettingWindow : Window
    {
        bool initialized;
        public SimulationSettingWindow()
        {
            InitializeComponent();
            initialized = false;
            this.EndTimeSet.Text = SystemGrid.EndTime.ToString();
            this.deltaTimeSet.Text = SystemGrid.TimeSpace.ToString();
            this.simulationTypeSelector.ItemsSource = Enum.GetValues(typeof(SimulationType)).Cast<SimulationType>();
            initialized = true;
        }

        private void EndTimeSet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (initialized)
            {
                TextBox tb = (TextBox)sender;
                SystemGrid.EndTime = Mathf.Scientific(tb.Text);
            }
        }

        private void DeltaTimeSet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (initialized)
            {
                TextBox tb = (TextBox)sender;
                SystemGrid.TimeSpace = Mathf.Scientific(tb.Text);
            }
        }
    }
}
