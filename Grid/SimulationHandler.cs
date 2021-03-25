using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolset;


namespace Grid
{
    //Simulation core
    internal class SimulationHandler
    {
        public SimulationType _simType;
        public SimInformer Informer;
        List<CircuitError> _error;
        List<CircuitWarning> _warning;
        List<String> _SimOutput;
        Stopwatch clock;

        //ref
        IEnumerable<Component> _comps;
        Dictionary<int, Node> _nodes;

        delegate void CircuitValidationHandler(object sender, List<CircuitError> error, List<CircuitWarning> warning);
        event CircuitValidationHandler ValidatingCircuit;
        event CircuitValidationHandler ValidatedCircuit;

        public SimulationHandler(SimulationType st)
        {
            _simType = st;
            _error = new List<CircuitError>();
            _warning = new List<CircuitWarning>();
            _comps = SystemGrid.ComponentGrid.Values;
            _nodes = SystemGrid.MainGrid;
            _SimOutput = new List<string>();
            Informer = new SimInformer();
            clock = new Stopwatch();
        }

        public void StartSimulation()
        {
            Informer.SendInfo(".....................................");
            //resetting all errors and warnings
            _error.Clear();
            _warning.Clear();
            bool _errorPrinted = false;

            //resetting all previous calculations to default for overrinding.
            ComponentManager.ResetAllCalculatedValues();

            //resetting clock to calculate elapsed time
            clock.Reset();
            clock.Start();

            try
            {
                //starting to name nodes and stuff
                BuildCircuit();
                //checking if there is anything wrong in circuit
                ValidateCircuit();

                //printing errors and warningns
                Informer.SendInfo("......................................");
                PrintError_Warnings();
                _errorPrinted = true;

                //Running simulation
                RunSim();
                //feeding probes
                Feed_Probes();
            }
            catch (Exception)
            {
                Informer.SendAbortInfo();
                if (!_errorPrinted) PrintError_Warnings();
                return;
            }
            clock.Stop();
            Informer.SendInfo("Job done in: "+ clock.ElapsedMilliseconds.ToString() + "ms.");
        }

        void ValidateCircuit()
        {
            //referencing stuff
            Informer.SendInfo("Validating circuit");
            ValidatingCircuit?.Invoke(this, _error, _warning);
            //validate circuit here
            

            //checking if there are atleast two component.
            if(SystemGrid.ComponentGrid.Count <= 1)
            {
                _error.Add(new CircuitError("No Element found, simulation aborted"));
                ValidatedCircuit?.Invoke(this, _error, _warning);
                throw new ComponentFloatingException("Not enough component");
            }

            if(SystemGrid.groundList.Count <= 0)
            {
                _error.Add(new CircuitError("No ground detected. Circuit is floating"));
                ValidatedCircuit?.Invoke(this, _error, _warning);
                throw new ComponentFloatingException("Component is floating");
            }

            int i = 0;
            foreach(Component c in SystemGrid.ComponentGrid.Values)
            {
                c.SetIndex();

                //in which index, there is output for this component
                c.indexInOutput = i;
                i += 2;
                if (c.NetPositive == c.NetNegative)
                {
                    _warning.Add(new CircuitWarning(c.Name + " is shorted."));
                }
            }

            //at the end of validation
            ValidatedCircuit?.Invoke(this, _error, _warning);
            Informer.SendInfo("Validation ended");

        }

        /// <summary>
        /// Prints errors and warnings
        /// </summary>
        void PrintError_Warnings()
        {
            //printing out errors
            int warningIndex = 0;

            foreach (CircuitError cw in _error)
            {
                Informer.SendInfo(string.Format("Error {0}:", warningIndex + 1) + cw.Message);
                warningIndex++;
            }

            //printing out warnings
            warningIndex = 0;
            foreach (CircuitWarning cw in _warning)
            {
                Informer.SendInfo(string.Format("Warning {0}:", warningIndex + 1) + cw.Message);
                warningIndex++;
            }

            Informer.SendInfo(_error.Count.ToString() + " errors, " + _warning.Count.ToString() + " warnings");
            Informer.SendInfo("..............................");
        }

        /// <summary>
        /// Assigns components and nodes their appropriate 'NET's.
        /// Sets other simulation properties
        /// </summary>
        void BuildCircuit()
        {
            int NodeCount = 1;
            int VoltageCount = 0;

            
            foreach (Node n in _nodes.Values) { n.ResetNode(); }        //resetting node if simulation already happened to count in changes
            ComponentManager.ResetAllComponentIndex();              //resetting components for the same reason

            Informer.SendInfo("Building circuit");
            try
            {
                foreach (GND g in SystemGrid.groundList)
                {
                    SystemGrid.MainGrid[g.PinPos].ModifyConnected(0);
                }
            }
            catch(Exception e)
            {
                Informer.SendInfo("Component is floating");
                Informer.SendAbortInfo();
                _error.Add(new CircuitError("Component is floating"));
                throw new Exception(e.Message);
            }
            try
            {
                foreach (Component c in _comps)
                {
                    int pinPos = c.PinPos;
                    int pinNeg = c.PinNeg;
                    if (_nodes[pinPos].ModifyConnected(NodeCount)) NodeCount++;
                    if (_nodes[pinNeg].ModifyConnected(NodeCount)) NodeCount++;
                    if (c is VoltageSource) VoltageCount++;
                }
            }
            catch (ComponentFloatingException ex)
            {
                Informer.SendInfo(ex.Message);
                Informer.SendAbortInfo();
                _error.Add(new CircuitError(ex.Message));
                throw new Exception(ex.Message);
            }

            SystemGrid.NodeCount = NodeCount - 1;
            SystemGrid.VoltageCount = VoltageCount;
            Informer.SendInfo("Circuit building done");
        }

        /// <summary>
        /// Well,...simulates the circuit
        /// </summary>
        void RunSim()
        {
            Informer.SendInfo("Resuming simulation");
            Informer.SendInfo("Simulation type: " + _simType.ToString());
            
            Informer.OnSimulationStart("started");

            if(_simType == SimulationType.RunOnce)
            {
                SIM_RunOnce();
            }

            else if(_simType == SimulationType.TransientAnalysis)
            {
                SIM_Transient();
            }

            Informer.OnSimulationEnd("Ended");
            Informer.SendInfo("Simulation ended successfully");
        }

        //simulation run once
        /// <summary>
        /// run once simulation.Simulate single cycle of transient.
        /// </summary>
        /// <returns>true if simulation completet successfully. Otherwise returns false</returns>
        bool SIM_RunOnce()
        {
            int NodeCount = SystemGrid.NodeCount;
            int VoltageCount = SystemGrid.VoltageCount;

            double[,] _Gmat = Mathf.GenerateMatrix<double>(NodeCount, NodeCount, 0);
            double[,] _Bmat = Mathf.GenerateMatrix<double>(NodeCount, VoltageCount, 0);
            double[,] _Cmat = Mathf.GenerateMatrix<double>(VoltageCount, NodeCount, 0);
            double[,] _Dmat = Mathf.GenerateMatrix<double>(VoltageCount, VoltageCount, 0);
            double[] _Zmat = Mathf.GenerateMatrix<double>(VoltageCount + NodeCount, 0);
            
            foreach(Component c in _comps)
            {
                c.Modify_G_Matrix(_Gmat);
                c.Modify_B_Matrix(_Bmat);
                c.Modify_C_Matrix(_Cmat);
                c.Modify_D_Matrix(_Dmat);
                c.Modify_Z_Matrix(_Zmat);
            }

            double[,] A = Mathf.ConcatenateMatrix(_Gmat, _Bmat, _Cmat, _Dmat);
            SystemGrid.solutionMatrix = Mathf.GaussianReduction(A, _Zmat);

            foreach(Component c in _comps)
            {
                //components receive their calculated value
                c.SetCalculatedInfo();
                _SimOutput.Add(c.GetCalculatedInfo());
            }

            return true;
        }

        void SIM_Transient()
        {
            double deltaTime;

            int NodeCount = SystemGrid.NodeCount;
            int VoltageCount = SystemGrid.VoltageCount;
            int loopCount = 0;

            double[,] _Gmat = Mathf.GenerateMatrix<double>(NodeCount, NodeCount, 0);
            double[,] _Bmat = Mathf.GenerateMatrix<double>(NodeCount, VoltageCount, 0);
            double[,] _Cmat = Mathf.GenerateMatrix<double>(VoltageCount, NodeCount, 0);
            double[,] _Dmat = Mathf.GenerateMatrix<double>(VoltageCount, VoltageCount, 0);
            double[] _Zmat = Mathf.GenerateMatrix<double>(VoltageCount + NodeCount, 0);

            for(deltaTime = 0;deltaTime <= SystemGrid.EndTime;deltaTime += SystemGrid.TimeSpace)
            {
                //skip the first time cos it's already populated.
                //makes it a little bit efficient
                SystemGrid.dTime = deltaTime;
                loopCount++;
                
                if(deltaTime != 0)
                {
                    Mathf.PopulateArray<double>(_Gmat, 0, NodeCount, NodeCount);
                    Mathf.PopulateArray<double>(_Bmat, 0, NodeCount, VoltageCount);
                    Mathf.PopulateArray<double>(_Cmat, 0, VoltageCount, NodeCount);
                    Mathf.PopulateArray<double>(_Dmat, 0, VoltageCount, VoltageCount);
                    Mathf.PopulateArray<double>(_Zmat, 0);
                }

                foreach(Component c in _comps)
                {
                    c.Modify_G_Matrix(_Gmat);
                    c.Modify_B_Matrix(_Bmat);
                    c.Modify_C_Matrix(_Cmat);
                    c.Modify_D_Matrix(_Dmat);
                    c.Modify_Z_Matrix(_Zmat);
                }

                double[,] A = Mathf.ConcatenateMatrix(_Gmat, _Bmat, _Cmat, _Dmat);
                SystemGrid.solutionMatrix = Mathf.GaussianReduction(A, _Zmat);

                string output = "";
                foreach(Component c in _comps)
                {
                    c.SetCalculatedInfo();
                    c.UpdateValues();
                    output += c.GetCalculatedInfo() + " ";
                }
                for(int iter = 0;iter < SystemGrid.solutionMatrix.Length;iter++)
                {
                    output += SystemGrid.solutionMatrix[iter].ToString() + " ";
                }
                Informer.OnSimulationCycleEnd(output);
            }
            SystemGrid.DatasetLength = loopCount;
        }

        void Feed_Probes()
        {
            foreach(Probe p in SystemGrid.probeList)
            {
                p.GetValues();
            }
        }

    }

    //view model type stuff
    //used to send simulation information to other parts of the code
    public class SimInformer 
    {
        ObservableCollection<string> _message;
        static readonly string abort = "Simulation aborted";

        public delegate void SimulationStatusHandler(object sender, string output);
        public event SimulationStatusHandler SimulationStarted;
        public event SimulationStatusHandler SimulationCycleEnd;
        public event SimulationStatusHandler SimulationEnded;
        
        /// <summary>
        /// Sends out simulation abortion info
        /// </summary>
        public void SendAbortInfo()
        {
            SendInfo(abort);
        }

        /// <summary>
        /// Simulation informer
        /// Sends out information to other parts of information
        /// </summary>
        public SimInformer()
        {
            _message = new ObservableCollection<string>();
        }

        /// <summary>
        /// Sends out info
        /// </summary>
        /// <param name="info"></param>
        public void SendInfo(string info)
        {
            _message.Add(info);
        }

        /// <summary>
        /// Can collect information through this
        /// </summary>
        /// <returns>collection of messages</returns>
        public ObservableCollection<string> GetInfo()
        {
            return _message;
        }

        /// <summary>
        /// Reset messagelist
        /// </summary>
        public void ResetMessage()
        {
            _message.Clear();
        }

        public void OnSimulationStart(string s) { SimulationStarted?.Invoke(this, s); }
        public void OnSimulationEnd(string s) { SimulationEnded?.Invoke(this, s); }
        public void OnSimulationCycleEnd(string s) { SimulationCycleEnd?.Invoke(this, s); }
    }

    public enum SimulationType
    {
        RunOnce,
        TransientAnalysis,
    }
}
