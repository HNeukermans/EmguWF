using System;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace EmguWF.ExpressionEditor
{
    /// <summary>
    /// Implementation of the IExpressionEditorInstance used by the Workflow host.
    /// </summary>
    public class EditorInstance : IExpressionEditorInstance
    {
        private readonly TextBox _editor;
        private IntellisensePopup _popup;
        private string _startText;

        /// <summary>
        /// Constructor
        /// </summary>
        public EditorInstance()
        {
            _editor = null;
            _popup = null;
            _startText = string.Empty;

            _editor = new TextBox();
            
            _editor.KeyDown += EditorKeyDown;
            _editor.TextChanged += EditorTextChanged;
            _editor.PreviewKeyDown += EditorPreviewKeyDown;
            _editor.GotFocus += EditorGotFocus;
            _editor.LostFocus += EditorLostFocus;
            _editor.Unloaded += EditorUnloaded;

        }

        /// <summary>
        /// The namespaces we know about (from WF)
        /// </summary>
        public List<string> ImportedNamespaces { get; set; }

        /// <summary>
        /// The expression type (from WF)
        /// </summary>
        public Type ExpressionType { get; set; }

        /// <summary>
        /// Unique Guid identifier
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Words to highlight
        /// </summary>
        public string HighlightWords { get; set; }

        /// <summary>
        /// Intellisense data (global)
        /// </summary>
        public IntellisenseEntry IntellisenseData { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the expression editor instance accepts the RETURN key.
        /// </summary>
        /// <returns>
        /// true if the expression editor accepts the RETURN key; otherwise, false.
        /// </returns>
        public bool AcceptsReturn
        {
            get { return _editor.AcceptsReturn; }
            set { _editor.AcceptsReturn = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the expression editor instance accepts the TAB key.
        /// </summary>
        /// <returns>
        /// true if the expression editor accepts the TAB key; otherwise, false.
        /// </returns>
        public bool AcceptsTab
        {
            get { return _editor.AcceptsTab; }
            set { _editor.AcceptsTab = value; }
        }

        /// <summary>
        /// Gets a value that indicates whether the instance has aggregate focus.
        /// </summary>
        /// <returns>
        /// true if the editor instance has aggregate focus; otherwise, false.
        /// </returns>
        public bool HasAggregateFocus
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the horizontal scrollbar is visible.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Windows.Controls.ScrollBarVisibility"/>.
        /// </returns>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return _editor.HorizontalScrollBarVisibility; }
            set { _editor.HorizontalScrollBarVisibility = value; }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Windows.Controls.Control"/> instance that can be used to display in the ExpressionTextBox.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Windows.Controls.Control"/>.
        /// </returns>
        public Control HostControl
        {
            get { return _editor; }
        }

        /// <summary>
        /// Gets or sets the maximum number of lines of text to be displayed by the ExpressionTextBox control. This property implicitly sets the height of the ExpressionTextBox control when growing to fit. 
        /// </summary>
        /// <returns>
        /// Returns an <see cref="T:System.Int32"/> that containing the maximum number of lines of text to be displayed by the ExpressionTextBox control.
        /// </returns>
        public int MaxLines
        {
            get { return _editor.MaxLines; }
            set { _editor.MaxLines = value; }
        }

        /// <summary>
        /// Gets or sets the minimum number of lines of text to be displayed by the ExpressionTextBox control. This property implicitly sets the height of the ExpressionTextBox control.
        /// </summary>
        /// <returns>
        /// Returns an<see cref="T:System.Int32"/> that contains the minimum number of lines of text to be displayed by the ExpressionTextBox control.
        /// </returns>
        public int MinLines
        {
            get { return _editor.MinLines; }
            set { _editor.MinLines = value; }
        }

        /// <summary>
        /// Gets or sets the value of the text.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.String"/>.
        /// </returns>
        public string Text
        {
            get { return _editor.Text; }
            set { _editor.Text = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the vertical scrollbar is visible.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Windows.Controls.ScrollBarVisibility"/>.
        /// </returns>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return _editor.VerticalScrollBarVisibility; }
            set { _editor.VerticalScrollBarVisibility = value; }
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the expression editor instance can complete the string being typed by the user.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="T:System.Boolean"/> set to true.
        /// </returns>
        public bool CanCompleteWord()
        {
            return true;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the expression editor instance can be closed.
        /// </summary>
        /// <returns>
        /// true if the expression editor instance can be closed; otherwise, false.
        /// </returns>
        public bool CanCopy()
        {
            return true;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the expression can be cut.
        /// </summary>
        /// <returns>
        /// true if the expression can be cut; otherwise, false.
        /// </returns>
        public bool CanCut()
        {
            return true;
        }

        /// <summary>
        /// Returns a value that indicates whether the filter level can be decreased when using Intellisense filtering.
        /// </summary>
        /// <returns>
        /// true if the filter level can be decreased; otherwise, false.
        /// </returns>
        public bool CanDecreaseFilterLevel()
        {
            return false;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the expression editor instance can retrieve global IntelliSense on the expression.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="T:System.Boolean"/> set to true.
        /// </returns>
        public bool CanGlobalIntellisense()
        {
            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether the filter level can be increased when using Intellisense filtering.
        /// </summary>
        /// <returns>
        /// true if the filter level can be increased; otherwise, false.
        /// </returns>
        public bool CanIncreaseFilterLevel()
        {
            return false;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the expression editor instance can retrieve parameter information on the expression.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="T:System.Boolean"/> set to true.
        /// </returns>
        public bool CanParameterInfo()
        {
            return false;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the expression can be pasted.
        /// </summary>
        /// <returns>
        /// true if the expression can be pasted; otherwise, false.
        /// </returns>
        public bool CanPaste()
        {
            return true;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the expression editor instance can retrieve type information to be shown in an IntelliSense quick info tool tip.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="T:System.Boolean"/> set to true.
        /// </returns>
        public bool CanQuickInfo()
        {
            return false;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the system can redo the operation.
        /// </summary>
        /// <returns>
        /// true if the system can redo the operation; otherwise, false.
        /// </returns>
        public bool CanRedo()
        {
            return _editor.CanRedo;
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the system can undo the operation.
        /// </summary>
        /// <returns>
        /// true if the system can undo the operation; otherwise, false.
        /// </returns>
        public bool CanUndo()
        {
            return _editor.CanUndo;
        }

        /// <summary>
        /// Clears the selection in the editor instance.
        /// </summary>
        public void ClearSelection()
        {
        }

        /// <summary>
        /// Closes and purges the editor items. This will close the specific expression editor instance.
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Determines whether the expression editor instance can provide a list of completions for the partial word typed by the user. For example, member, argument, and method names can be shown to the user in an attempt to help them complete the word they are typing. 
        /// </summary>
        /// <returns>
        /// true if this method succeeds; otherwise, false.
        /// </returns>
        public bool CompleteWord()
        {
            return true;
        }

        /// <summary>
        /// Copies the active expression.
        /// </summary>
        /// <returns>
        /// true if the copy is successful; otherwise, false.
        /// </returns>
        public bool Copy()
        {
            return true;
        }

        /// <summary>
        /// Cuts the active expression.
        /// </summary>
        /// <returns>
        /// true if the cut is successful; otherwise, false.
        /// </returns>
        public bool Cut()
        {
            return true;
        }

        /// <summary>
        /// Decreases the filter level to show all items in the Intellisense filter list and returns a value that indicates whether this operation was successful.
        /// </summary>
        /// <returns>
        /// true if this method succeeds; otherwise, false.
        /// </returns>
        public bool DecreaseFilterLevel()
        {
            return false;
        }

        /// <summary>
        /// Sets focus on the editor instance.
        /// </summary>
        public void Focus()
        {
            _editor.Focus();
        }

        /// <summary>
        /// Gets the text used to generate an expression.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="T:System.String"/> that contains the text used to generate an expression.
        /// </returns>
        public string GetCommittedText()
        {
            return _editor.Text;
        }

        /// <summary>
        /// Retrieves global IntelliSense on the expression in the expression editor and returns a value that indicates whether this operation was successful.
        /// </summary>
        /// <returns>
        /// true if this method succeeds; otherwise, false.
        /// </returns>
        public bool GlobalIntellisense()
        {
            return false;
        }

        /// <summary>
        /// Increases the filter level to show common items in the Intellisense filter list and returns a value that indicates whether this operation was successful.
        /// </summary>
        /// <returns>
        /// true if this method succeeds; otherwise, false.
        /// </returns>
        public bool IncreaseFilterLevel()
        {
            return false;
        }

        /// <summary>
        /// Retrieves parameter information on the expression in the expression editor and returns a value that indicates whether this operation was successful.
        /// </summary>
        /// <returns>
        /// true if this method succeeds; otherwise, false.
        /// </returns>
        public bool ParameterInfo()
        {
            return false;
        }

        /// <summary>
        /// Pastes the active expression.
        /// </summary>
        /// <returns>
        /// true if the paste is successful; otherwise, false.
        /// </returns>
        public bool Paste()
        {
            return true;
        }

        /// <summary>
        /// Determines whether type information to be shown in an IntelliSense quick info tool tip.
        /// </summary>
        /// <returns>
        /// true if this method succeeds; otherwise, false.
        /// </returns>
        public bool QuickInfo()
        {
            return false;
        }

        /// <summary>
        /// Reapplies the last operation that was undone in the editor, that is, reverse the effects of the undo operation. 
        /// </summary>
        /// <returns>
        /// true if the redo is successful; otherwise, false.
        /// </returns>
        public bool Redo()
        {
            return true;
        }

        /// <summary>
        /// Undoes the last operation in the editor.
        /// </summary>
        /// <returns>
        /// true if the undo is successful; otherwise, false.
        /// </returns>
        public bool Undo()
        {
            return true;
        }

        /// <summary>
        /// Occurs when the expression editor instance is closing.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Occurs when the expression editor instance has aggregate focus. 
        /// </summary>
        public event EventHandler GotAggregateFocus;

        /// <summary>
        /// Represents an event that is raised when the expression editor instance loses aggregate focus. 
        /// </summary>
        public event EventHandler LostAggregateFocus;

        /// <summary>
        /// Represents an event that is raised when the text in an expression editor instance is changed.
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// This is called when a key press occurs in the text editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (!AcceptsReturn && Keyboard.Modifiers == ModifierKeys.None &&
                (e.Key == Key.Return || e.Key == Key.Tab))
            {
                e.Handled = true;
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                var focusedElement = (Keyboard.FocusedElement as UIElement);
                if (focusedElement != null)
                {
                    focusedElement.MoveFocus(request);
                }
            }

            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// This is called just before a new key is added to the text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isControlKeyDown = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

            // Move up/down the list
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (_popup == null || !_popup.IsOpen)
                    return;
                _popup.SelectedIndex = (e.Key == Key.Up) ? _popup.SelectedIndex - 1 : _popup.SelectedIndex + 1;
                e.Handled = true;
            }

            else if (e.Key == Key.PageUp)
            {
                if (_popup == null || !_popup.IsOpen)
                    return;
                _popup.SelectedIndex = _popup.SelectedIndex - 5;
                e.Handled = true;
            }

            else if (e.Key == Key.PageDown)
            {
                if (_popup == null || !_popup.IsOpen)
                    return;
                _popup.SelectedIndex = _popup.SelectedIndex + 5;
                e.Handled = true;
            }

            else if (e.Key == Key.Home || e.Key == Key.End)
            {
                if (_popup == null || !_popup.IsOpen)
                    return;
                _popup.SelectedIndex = (e.Key == Key.Home) ? 0 : _popup.ItemCount - 1;
                e.Handled = true;
            }

            // Close the popup
            else if (e.Key == Key.Escape)
            {
                DestroyPopup();
                _startText = string.Empty;
                e.Handled = true;
            }

            // CTRL+Space or "."
            else if ((e.Key == Key.Decimal || e.Key == Key.OemPeriod) ||
                     (e.Key == Key.Space && isControlKeyDown))
            {
                if (_popup == null || !_popup.IsOpen)
                {
                    string rawText = _editor.Text;
                    int pos = Math.Max(Math.Max(
                        rawText.LastIndexOf(".", StringComparison.Ordinal),
                        rawText.LastIndexOf("(", StringComparison.Ordinal)),
                        rawText.LastIndexOf(" ", StringComparison.Ordinal));
                    _startText = (pos == -1) ? rawText : rawText.Substring(0, pos + 1);

                    string inputText = rawText;
                    if (inputText.ToLowerInvariant().StartsWith("new "))
                        inputText = inputText.Substring(4);
                    if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
                        inputText = inputText + ".";

                    IntellisenseEntry intellisenseList =
                        await IntellisenseData.SearchNodesAsync(AddScopedNamespaces(inputText)) ?? IntellisenseData;
                    CreatePopup(intellisenseList.Children);
                    e.Handled = (e.Key == Key.Space && isControlKeyDown);
                }
            }
            else if (e.Key == Key.Return || e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Tab)
            {
                if (_popup == null || !_popup.IsOpen)
                    return;

                IntellisenseEntry selectedItem = _popup.SelectedItem;
                if (selectedItem != null)
                {
                    CommitIntellisenseNode(selectedItem);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Called when the text box gets focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorGotFocus(object sender, RoutedEventArgs e)
        {
            var gotFocus = GotAggregateFocus;
            if (gotFocus != null)
                gotFocus.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the text box loses focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorLostFocus(object sender, EventArgs e)
        {
            var focusedElement = (Keyboard.FocusedElement as ListBoxItem);
            if (focusedElement == null)
            {
                if ((_popup != null) && _popup.IsOpen)
                {
                    DestroyPopup();
                }

                var lostFocus = LostAggregateFocus;
                if (lostFocus != null)
                    lostFocus.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Generates variations of the search text based on known namespaces.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private IEnumerable<string> AddScopedNamespaces(string text)
        {
            yield return text;
            foreach (string ns in ImportedNamespaces.Select(ns => ns + "." + text))
                yield return ns;
        }

        /// <summary>
        /// Called when the text in the TextBox changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EditorTextChanged(object sender, EventArgs e)
        {
            Text = _editor.Text;
            if (_popup != null)
            {
                string inputText = Text;
                if (inputText.ToLowerInvariant().StartsWith("new"))
                    inputText = inputText.Substring(3).Trim();

                IntellisenseEntry targetNode = await IntellisenseData.SearchNodesAsync(AddScopedNamespaces(inputText)) ??
                                              IntellisenseData;
                if (targetNode.Children.Count == 0)
                    DestroyPopup();
                else
                    _popup.DataContext = targetNode.Children;
            }

            var textChanged = TextChanged;
            if (textChanged != null)
                textChanged.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Creates the intellisense popup
        /// </summary>
        /// <param name="isSource"></param>
        private void CreatePopup(List<IntellisenseEntry> isSource)
        {
            DestroyPopup();

            _popup = new IntellisensePopup
            {
                DataContext = isSource,
                PlacementTarget = _editor,
                Placement = PlacementMode.Bottom,
                Width = Math.Max(300, Math.Min(300, _editor.ActualWidth)),
                Height = 200,
            };

            _popup.ListKeyDown += ListKeyDown;
            _popup.ListItemDoubleClick += ListItemDoubleClick;
            _popup.IsOpen = true;
        }

        /// <summary>
        /// Called when a double-click occurs on the intellisense window item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IntellisenseEntry dataContext = _popup.SelectedItem;
            if (dataContext != null)
            {
                _editor.Focus();
                CommitIntellisenseNode(dataContext);
            }
        }

        /// <summary>
        /// Called when a keypress occurs in the intellisense window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DestroyPopup();
                _editor.Focus();
            }
            else if (e.Key == Key.Return || e.Key == Key.Space || e.Key == Key.Tab)
            {
                _editor.Focus();
                IntellisenseEntry selectedItem = _popup.SelectedItem;
                if (selectedItem != null)
                {
                    string fullPath = selectedItem.FullPath;
                    _editor.Text = _startText + fullPath;
                    _editor.SelectionStart = _editor.Text.Length;
                    DestroyPopup();
                }
            }
        }

        /// <summary>
        /// Called to destroy the intellisense popup
        /// </summary>
        private void DestroyPopup()
        {
            if (_popup != null)
            {
                _popup.ListKeyDown -= ListKeyDown;
                _popup.ListItemDoubleClick -= ListItemDoubleClick;
                if (_popup.IsOpen)
                    _popup.IsOpen = false;
                _startText = string.Empty;
                _popup = null;
            }
        }

        /// <summary>
        /// This commits the text to the text box from the intellisense prompt.
        /// </summary>
        /// <param name="selectedNodes"></param>
        private void CommitIntellisenseNode(IntellisenseEntry selectedNodes)
        {
            string str = _startText + selectedNodes.Name;
            DestroyPopup();
            
            _editor.Text = str;
            _editor.SelectionStart = _editor.Text.Length;
            _editor.UpdateLayout();
            
        }

        /// <summary>
        /// Raises the Closing event when the editor is unloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorUnloaded(object sender, RoutedEventArgs e)
        {
            var closing = Closing;
            if (closing != null)
                closing.Invoke(this, EventArgs.Empty);
        }
    }
}