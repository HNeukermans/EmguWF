using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using JulMar.Windows.Interfaces;
using JulMar.Windows.Mvvm;
using System.Collections.Generic;

namespace EmguWF.ViewModels
{
    /// <summary>
    /// Main view model used to drive the application
    /// </summary>
    public sealed class MainViewModel : ViewModel
    {
        private readonly object _errGuard = new object();
        private string _workflowFile;

        /// <summary>
        /// Currently active validation errors in the workflow.
        /// </summary>
        public IList<string> Errors { get; private set; }

        /// <summary>
        /// Event which is raised prior to execution to force the UI to save the
        /// active designed workflow.
        /// </summary>
        public event Action RequireSave;

        /// <summary>
        /// Workflow persistence file.
        /// </summary>
        public string WorkflowFile
        {
            get { return _workflowFile; }
            set
            {
                if (SetPropertyValue(ref _workflowFile, value))
                {
                    RaisePropertyChanged(() => HasWorkflow);
                }
            }
        }

        /// <summary>
        /// True/False if we have an active workflow
        /// </summary>
        public bool HasWorkflow
        {
            get { return !string.IsNullOrEmpty(WorkflowFile); }
        }

        /// <summary>
        /// Command to execute the workflow
        /// </summary>
        public ICommand Run { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            Errors = new ObservableCollection<string>();
            Run = new DelegateCommand(OnRunWorkflow, () => this.HasWorkflow && Errors.Count == 0);

            BindingOperations.EnableCollectionSynchronization(Errors, _errGuard);
        }

        /// <summary>
        /// This method executes the workflow.
        /// </summary>
        private void OnRunWorkflow()
        {
            if (RequireSave != null)
            {
                RequireSave();
            }

            var execViewModel = new ExecutionViewModel(WorkflowFile);
            if (!execViewModel.Initialize())
                return;

            IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
            uiVisualizer.Show("ExecutionWindow", execViewModel, true, 
                (s,e) => execViewModel.Dispose());
        }
    }
}
