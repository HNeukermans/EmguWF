using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using EmguWF.Extensions;
using JulMar.Windows.Interfaces;
using JulMar.Windows.Mvvm;
using System.Collections.Generic;
using System.Windows;

namespace EmguWF.ViewModels
{
    /// <summary>
    /// This view model is used to execute a workflow.
    /// </summary>
    public sealed class ExecutionViewModel : ViewModel
    {
        private readonly string _filename;
        private bool _isRunning;
        private Activity _workflow;
        private WorkflowInvoker _invoker;
        private Dictionary<string, object> _inputs;
        private TextReader _uiTextReader;

        /// <summary>
        /// Title for this workflow execution
        /// </summary>
        public string Title
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_filename);
            }
        }

        /// <summary>
        /// Save window to clipboard.
        /// </summary>
        public IDelegateCommand CopyToClipboard { get; private set; }

        /// <summary>
        /// Log used to output results
        /// </summary>
        public IList<string> Log { get; private set; }

        /// <summary>
        /// The log for UI
        /// </summary>
        private readonly StringBuilder _logText = new StringBuilder();
        public string ReadOnlyLogText
        {
            get { return _logText.ToString(); }
        }

        /// <summary>
        /// True if the workflow is currently executing
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            private set
            {
                if (SetPropertyValue(ref _isRunning, value))
                {
                    Cancel.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Cancellation support
        /// </summary>
        public IDelegateCommand Cancel { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">Filename of the workflow to run</param>
        public ExecutionViewModel(string filename)
        {
            _filename = filename;
            Cancel = new DelegateCommand(OnCancel, () => IsRunning);
            CopyToClipboard = new DelegateCommand(OnCopyToClipboard);

            var log = new ObservableCollection<string>();
            log.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in e.NewItems.Cast<string>())
                    {
                        _logText.AppendLine(item);
                    }
                }
                else
                {
                    _logText.Clear();
                    foreach (var item in log)
                    {
                        _logText.AppendLine(item);
                    }
                }
                RaisePropertyChanged(() => ReadOnlyLogText);
            };
            Log = log;
        }

        /// <summary>
        /// Save the log to the clipboard.
        /// </summary>
        private void OnCopyToClipboard()
        {
            Clipboard.SetText(ReadOnlyLogText);
        }

        /// <summary>
        /// 2-phase initialization to parse parameters
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            XamlXmlReader reader = new XamlXmlReader(_filename, new XamlXmlReaderSettings { LocalAssembly = typeof(MainViewModel).Assembly });
            _workflow = ActivityXamlServices.Load(reader);

            var argumentViewModel = new ArgumentCollectorViewModel(_workflow as DynamicActivity);
            if (argumentViewModel.HasArguments)
            {
                IUIVisualizer uiVisualizer = Resolve<IUIVisualizer>();
                if (uiVisualizer.ShowDialog("WorkflowArgumentsView", argumentViewModel) == false)
                    return false;
            }
            
            _inputs = argumentViewModel.CollectArguments();

            //_inputs.Add("img",typeof(Image<Bgr,Byte>));
            //_inputs.Add("gimg", typeof(Image<Gray, Byte>));

            return true;
        }

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        private void OnCancel()
        {
            if (IsRunning)
            {
                Log.Add("Canceling execution.");
                _invoker.CancelAsync(this);
            }
        }

        /// <summary>
        /// Starts the workflow execution
        /// </summary>
        public void Start()
        {
            if (_workflow == null)
                return;

            _invoker = new WorkflowInvoker(_workflow);
            _invoker.InvokeCompleted += InvokerOnInvokeCompleted;
            _invoker.Extensions.Add(new OutputWriter(this.Log));
            _invoker.Extensions.Add((_uiTextReader = new GuiTextReader()));
            _invoker.Extensions.Add(new EmguWFImageStore());

            if (_inputs != null && _inputs.Count > 0)
            {
                Log.Add("Input values:");
                foreach (var item in _inputs)
                {
                    Log.Add(string.Format("  {0} = {1}", item.Key, item.Value));
                }
            }

            try
            {
                IsRunning = true;
                _invoker.InvokeAsync(_inputs, this);
            }
            catch (Exception ex)
            {
                Log.Add(string.Format("Failed to start workflow: {0}", ex.Message));
                Log.Add(ex.StackTrace);
            }
        }

        /// <summary>
        /// Called when the workflow is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InvokerOnInvokeCompleted(object sender, InvokeCompletedEventArgs e)
        {
            IsRunning = false;

            if (_uiTextReader != null)
            {
                _uiTextReader.Dispose();
                _uiTextReader = null;
            }

            if (e.Cancelled)
            {
                Log.Add("Run was cancelled.");
                return;
            }

            if (e.Error != null)
            {
                Log.Add(string.Format("Run failed: {0}", e.Error.Message));
                Log.Add(e.Error.StackTrace);
                return;
            }

            Log.Add("Run completed successfully.");

            var outputs = e.Outputs;
            if (outputs != null)
            {
                Log.Add("Output values:");
                foreach (var item in outputs)
                {
                    Log.Add(string.Format("  {0} = {1}", item.Key, item.Value));
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_uiTextReader != null)
                {
                    _uiTextReader.Dispose();
                    _uiTextReader = null;
                }
            }

            base.Dispose(isDisposing);
        }
    }
}
