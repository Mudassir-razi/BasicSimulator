﻿#pragma checksum "..\..\SimulationSettingWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "9E913BD0C9BF0F8FD4C1B11057A1F675E3E5F208086EDC9EDF3400ADCFE2E118"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using SystemUI;


namespace SystemUI {
    
    
    /// <summary>
    /// SimulationSettingWindow
    /// </summary>
    public partial class SimulationSettingWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 30 "..\..\SimulationSettingWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox simulationTypeSelector;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\SimulationSettingWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EndTimeSet;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\SimulationSettingWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox deltaTimeSet;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SystemUI;component/simulationsettingwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\SimulationSettingWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.simulationTypeSelector = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 2:
            this.EndTimeSet = ((System.Windows.Controls.TextBox)(target));
            
            #line 31 "..\..\SimulationSettingWindow.xaml"
            this.EndTimeSet.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.EndTimeSet_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.deltaTimeSet = ((System.Windows.Controls.TextBox)(target));
            
            #line 32 "..\..\SimulationSettingWindow.xaml"
            this.deltaTimeSet.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.DeltaTimeSet_TextChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
