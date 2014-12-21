using System;
using System.Activities;
using System.Activities.Core.Presentation.Factories;
using System.Activities.Presentation;
using System.Activities.Presentation.Toolbox;
using System.Activities.Presentation.Validation;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Activities.Core.Presentation;
using EmguWF.ExpressionEditor;
using EmguWF.Extensions;
using EmguWF.Gui;
using EmguWF.ViewModels;
using Microsoft.Win32;
using System.Reflection;

namespace EmguWF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string NewFilePlaceholder = "NewIpp.ipp";
        private WorkflowDesigner _designer;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            DataContext = new MainViewModel();
            ViewModel.RequireSave += () => OnSave(null, null);
            Loaded += OnLoaded;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, OnNew));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OnLoad));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, OnSave));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, OnSaveAs));
            CommandBindings.Add(new CommandBinding(AppCommands.Exit, OnClose));

            InitializeComponent();
        }

        /// <summary>
        /// The ViewModel storage
        /// </summary>
        MainViewModel ViewModel
        {
            get { return (MainViewModel) DataContext; }
        }

        /// <summary>
        /// Called when the UI is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // Create a new (empty) document
            OnNew(null, null);
        }

        /// <summary>
        /// This creates a new (empty) document.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            // If we have a workflow which has been saved, make sure it's ok to throw it out.
            if (ViewModel.HasWorkflow && !ReferenceEquals(ViewModel.WorkflowFile, NewFilePlaceholder))
            {
                if (MessageBox.Show("You have an existing file open. Do you want to throw away changes?",
                    "File already open", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            // Set our new file placeholder.
            ViewModel.WorkflowFile = NewFilePlaceholder;
            string emptyFile = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\NewActivity.xaml";

            // Initialize the WF host
            InitializeDesigner();
            _designer.Load(emptyFile);
        }

        /// <summary>
        /// Close the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// This is used to load a new workflow file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnLoad(object sender, ExecutedRoutedEventArgs e)
        {
            // If we have a workflow which has been saved, make sure it's ok to throw it out.
            if (ViewModel.HasWorkflow && !ReferenceEquals(ViewModel.WorkflowFile, NewFilePlaceholder))
            {
                if (MessageBox.Show("You have an existing file open. Do you want to throw away changes?",
                    "File already open", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            // Prompt for a file and open it.
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Processing Pipeline (*.ipp)|*.ipp|All Files (*.*)|*.*" };

            if (ofd.ShowDialog(this) == true)
            {
                ViewModel.WorkflowFile = ofd.FileName;
                InitializeDesigner();
                _designer.Load(ViewModel.WorkflowFile);
            }
        }

        /// <summary>
        /// Called to save the current workflow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (ReferenceEquals(ViewModel.WorkflowFile, NewFilePlaceholder))
            {
                OnSaveAs(sender, e);
            }
            else
            {
                _designer.Save(ViewModel.WorkflowFile);
            }
        }

        /// <summary>
        /// Called to save the workflow to a file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Image Processing Pipline (*.ipp)|*.ipp|All Files (*.*)|*.*" };

            if (sfd.ShowDialog(this) == true)
            {
                ViewModel.WorkflowFile = sfd.FileName;
                _designer.Save(ViewModel.WorkflowFile);
            }
        }

        /// <summary>
        /// Initializes the WF designer host.
        /// </summary>
        private void InitializeDesigner()
        {
            bool firstTime = (_designer == null);

            _designer = new WorkflowDesigner();

            DesignerConfigurationService dcs = _designer.Context.Services.GetService<DesignerConfigurationService>();
            
            // Set the runtime Framework version to 4.5
            dcs.TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 5));

            // Turn on designer features
            dcs.AnnotationEnabled = true;
            dcs.AutoSurroundWithSequenceEnabled = true;
            dcs.LoadingFromUntrustedSourceEnabled = true;
            dcs.RubberBandSelectionEnabled = true;

            // Add the UI
            outlineView.Content = _designer.OutlineView;
            inspectorPlaceholder.Content = _designer.PropertyInspectorView;
            designerPlaceholder.Content = _designer.View;

            if (firstTime)
            {
                ToolboxControl toolbox = BuildToolbox();
                toolboxPlaceholder.Content = toolbox;

                (new DesignerMetadata()).Register();
            }

            _designer.Context.Services.Publish<IValidationErrorService>(new ValidationErrorService(ViewModel.Errors));
            _designer.Context.Services.Publish<IExpressionEditorService>(new EditorService());
        }

        /// <summary>
        /// Builds the WF toolbox.
        /// </summary>
        /// <returns></returns>
        private ToolboxControl BuildToolbox()
        {
            ToolboxControl toolbox = new ToolboxControl();

            // Load emgu .NET types
            var emguCategory = new ToolboxCategory("Image Pipeline Activities");
            foreach (var activity in GetActivityTypes(typeof(EmguWF.Activities.RegisterMetadata).Assembly))
            {
                emguCategory.Add(new ToolboxItemWrapper(activity, GetFriendlyName(activity)));
            }
            
            (new EmguWF.Activities.RegisterMetadata()).Register();
            toolbox.Categories.Add(emguCategory);

            // Add standard types
            foreach (var activityList in _standardActivities)
            {
                var category = new ToolboxCategory(activityList.Key);
                foreach (var activity in activityList.Value)
                {
                    category.Add(new ToolboxItemWrapper(activity, GetFriendlyName(activity)));
                }
                toolbox.Categories.Add(category);
            }

            return toolbox;
        }

        // Standard activities to load into designer host.
        private readonly Dictionary<string, Type[]> _standardActivities = new Dictionary<string, Type[]>
        {
          //  { "Standard", new[] { typeof(Assign), typeof(Delay), typeof(InvokeDelegate), typeof(InvokeMethod), typeof(WriteLine) }},
            { "Control Flow", new[] { typeof(DoWhile), typeof(ForEachWithBodyFactory<>), typeof(If), typeof(Parallel), typeof(ParallelForEachWithBodyFactory<>), typeof(Pick), typeof(PickBranch), typeof(Sequence), typeof(Switch<>), typeof(While) }},
           // { "Flowchart", new[] { typeof(Flowchart), typeof(FlowDecision), typeof(FlowSwitch<>) }},
           // { "State Machine", new[] { typeof(StateMachine), typeof(State), typeof(FinalState) }},
           // { "Messaging", new[] { typeof(CorrelationScope), typeof(InitializeCorrelation), typeof(Receive), typeof(ReceiveAndSendReplyFactory), typeof(Send), typeof(SendAndReceiveReplyFactory), typeof(TransactedReceiveScope) }},
           // { "Runtime", new[] { typeof(Persist), typeof(TerminateWorkflow), typeof(NoPersistScope) }},
           // { "Transaction", new[] { typeof(CancellationScope), typeof(CompensableActivity), typeof(Compensate), typeof(Confirm), typeof(TransactionScope) }},
           // { "Collection", new[] { typeof(AddToCollection<>), typeof(ClearCollection<>), typeof(ExistsInCollection<>), typeof(RemoveFromCollection<>) }},
            { "Error Handling", new[] { typeof(Rethrow), typeof(Throw), typeof(TryCatch) }},
        };

        /// <summary>
        /// Retrieves all the WF activities for a given assembly.
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        private IEnumerable<Type> GetActivityTypes(Assembly asm)
        {
            // Get core types
            return asm.GetTypes()
                .Where(type => type.IsPublic 
                               && !type.IsAbstract
                               && (typeof(Activity).IsAssignableFrom(type) ||
                                   typeof(IActivityTemplateFactory).IsAssignableFrom(type)))
                .OrderBy(t => t.Name);
        }

        /// <summary>
        /// Return a friendly name for a tool.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private string GetFriendlyName(Type activity)
        {
            var dnAttrib = activity.GetCustomAttributes(typeof(DisplayNameAttribute)).FirstOrDefault();
            if (dnAttrib != null)
                return ((DisplayNameAttribute) dnAttrib).DisplayName;

            var result = activity.Name;
            if (activity.IsGenericType)
            {
                if (result.Contains("`2"))
                {
                    result = result.Replace("`2", "<T1,T2>");
                }
                else
                {
                    result = result.Replace("`1", "<T>").Replace('`', 'T');
                }
            }

            // Remove any factory indication.
            result = result.Replace("WithBodyFactory", "");

            return result;
        }
    }
}
