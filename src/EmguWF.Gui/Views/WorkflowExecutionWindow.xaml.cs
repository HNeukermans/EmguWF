using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EmguWF.ViewModels;
using JulMar.Windows.UI;

namespace EmguWF.Views
{
    /// <summary>
    /// Interaction logic for WorkflowExecutionWindow.xaml
    /// </summary>
    [ExportUIVisualizer("ExecutionWindow")]
    public partial class WorkflowExecutionWindow
    {
        public WorkflowExecutionWindow()
        {
            Loaded += OnLoaded;
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ((ExecutionViewModel) DataContext).Start();
        }

        private void OnLogTextChanged(object sender, DataTransferEventArgs e)
        {
            TextBox tb = (TextBox) sender;
            tb.ScrollToEnd();
        }
    }
}
