using System;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EmguWF.ExpressionEditor
{
    /// <summary>
    /// Implementation of the IExpressionEditorService used to create the expression text boxes.
    /// </summary>
    public class EditorService : IExpressionEditorService
    {
        /// <summary>
        /// Known VB keywords
        /// </summary>
        const string vbKeywords = 
               "AddHandler|AddressOf|Alias|And|AndAlso|As|Boolean|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|" +
               "CChar|CDate|CDbl|CDec|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|" +
               "CUShort|Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|Each|Else|ElseIf|End|EndIf|Enum|" +
               "Erase|Error|Event|Exit|False|Finally|For|Friend|Function|Get|GetType|GetXMLNamespace|Global|GoSub|GoTo|" +
               "Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Is|IsNot|Let|Lib|Like|Long|Loop|Me|Mod|Module|" +
               "MustInherit|MustOverride|MyBase|MyClass|Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|" +
               "Object|Of|On|Operator|Option|Optional|Or|OrElse|Out|Overloads|Overridable|Overrides|ParamArray|Partial|" +
               "Private|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|REM|RemoveHandler|Resume|Return|SByte|Select|" +
               "Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|Then|Throw|To|True|Try|" +
               "TryCast|TypeOf|UInteger|ULong|UShort|Using|Variant|Wend|While|Widening|With|WithEvents|WriteOnly|Xor|" +
               "#Const|#Else|#ElseIf|#End|#If";

        private readonly object _guard = new object();
        private readonly Dictionary<string, EditorInstance> _editorInstances = new Dictionary<string, EditorInstance>();
        private readonly Task<IntellisenseEntry> _intellisenseData;

        /// <summary>
        /// Constructor
        /// </summary>
        public EditorService()
        {
            // Load the intellisense from all known assemblies.
            _intellisenseData = IntellisenseBuilder.LoadAsync();
        }

        /// <summary>
        /// Closes all the active expression editors.
        /// </summary>
        public void CloseExpressionEditors()
        {
            foreach (EditorInstance instance in _editorInstances.Values)
                instance.LostAggregateFocus -= LostFocus;
            this._editorInstances.Clear();
        }

        /// <summary>
        /// Creates a new expression editor.
        /// </summary>
        /// <returns>
        /// Returns a<see cref="T:System.Activities.Presentation.View.IExpressionEditorInstance"/>.
        /// </returns>
        /// <param name="assemblies">Used to set the context for the editor session.</param><param name="importedNamespaces">The imported namespaces to be used by the expression editor.</param><param name="variables">Local variables for the expression editor.</param><param name="text">A string used to populate the expression editor.</param>
        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text)
        {
            return this.CreateExpressionEditor(assemblies, importedNamespaces, variables, text, null, new Size());
        }

        /// <summary>
        /// Creates a new expression editor using the specified assemblies, imported namespaces, variables, expression text, and expression type.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="T:System.Activities.Presentation.View.IExpressionEditorInstance"/>.
        /// </returns>
        /// <param name="assemblies">The local and referenced assemblies in the environment.</param><param name="importedNamespaces">The imported namespaces used by the expression editor.</param><param name="variables">Local variables for the expression editor.</param><param name="text">A string used to populate the expression editor.</param><param name="expressionType">The type of the expression.</param>
        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Type expressionType)
        {
            return this.CreateExpressionEditor(assemblies, importedNamespaces, variables, text, expressionType, new Size());
        }

        /// <summary>
        /// Creates a new expression editor using the specified assemblies, imported namespaces, variables, expression text, and the initial size.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="T:System.Activities.Presentation.View.IExpressionEditorInstance"/>.
        /// </returns>
        /// <param name="assemblies">The local and referenced assemblies in the environment.</param><param name="importedNamespaces">The imported namespaces used by the expression editor.</param><param name="variables">Local variables for the expression editor.</param><param name="text">A string used to populate the expression editor.</param><param name="initialSize">The initial height and width of the expression editor control.</param>
        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Size initialSize)
        {
            return this.CreateExpressionEditor(assemblies, importedNamespaces, variables, text, null, initialSize);
        }

        /// <summary>
        /// Creates a new expression editor using the specified assemblies, imported namespaces, variables, expression text, expression type, and the initial size.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="T:System.Activities.Presentation.View.IExpressionEditorInstance"/>.
        /// </returns>
        /// <param name="assemblies">The local and referenced assemblies in the environment.</param><param name="importedNamespaces">The imported namespaces used by the expression editor.</param><param name="variables">Local variables for the expression editor.</param><param name="text">A string used to populate the expression editor.</param><param name="expressionType">The type of the expression.</param><param name="initialSize">The initial height and width of the expression editor control.</param>
        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Type expressionType, Size initialSize)
        {
            var instance = new EditorInstance
            {
                IntellisenseData = this.CreateUpdatedIntellisense(variables),
                ImportedNamespaces = importedNamespaces.ImportedNamespaces.ToList(),
                HighlightWords = vbKeywords,
                ExpressionType = expressionType,
                Guid = Guid.NewGuid(),
                Text = text
            };

            instance.LostAggregateFocus += this.LostFocus;
            this._editorInstances.Add(instance.Guid.ToString(), instance);
            return instance;
        }

        /// <summary>
        /// This updates the known intellisense to include defined variables.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private IntellisenseEntry CreateUpdatedIntellisense(IEnumerable<ModelItem> vars)
        {
            IntellisenseEntry tn = new IntellisenseEntry();
            tn.Children.AddRange(_intellisenseData.Result.Children);

            lock (_guard)
            {
                int count = 0;
                foreach (var vs in vars)
                {
                    var vsProp = vs.Properties["Name"];
                    if (vsProp == null)
                        continue;

                    var varName = vsProp.ComputedValue as String;
                    var res = tn.Children.Where(x => x.Name == varName).ToList();
                    if (res.Count == 0)
                    {
                        Type sysType = null;
                        var sysTypeProp = vs.Properties["Type"];
                        if (sysTypeProp != null)
                        {
                            sysType = (Type) sysTypeProp.ComputedValue;
                        }

                        var node = new IntellisenseEntry
                        {
                            Name = varName,
                            Type = IntellisenseEntryType.Primitive,
                            SystemType = sysType,
                            Description = string.Empty
                        };

                        if (sysType != null)
                        {
                            var tempNode = new IntellisenseEntry();
                            IntellisenseBuilder.AddTypeNode(tempNode, sysType, true);
                            node.Children.AddRange(tempNode.Children[0].Children);
                        }

                        tn.Children.Insert(count++, node);
                    }
                }
            }

            return tn;
        }

        /// <summary>
        /// Called when the editor loses focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LostFocus(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box != null)
            {
                DesignerView.CommitCommand.Execute(box.Text);
            }
        }

        /// <summary>
        /// Updates the context for the editing session.
        /// </summary>
        /// <param name="assemblies">Used to set the context for the editor session.</param><param name="importedNamespaces">The imported namespaces used by the expression editor.</param>
        public void UpdateContext(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces)
        {
        }
    }
}