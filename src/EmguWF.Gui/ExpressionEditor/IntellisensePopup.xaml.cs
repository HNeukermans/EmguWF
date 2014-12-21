using System.Windows.Input;

namespace EmguWF.ExpressionEditor
{
    /// <summary>
    /// Popup window for intellisense display and selection
    /// </summary>
    public partial class IntellisensePopup
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public IntellisensePopup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets/Sets the selected item
        /// </summary>
        public IntellisenseEntry SelectedItem
        {
            get
            {
                return (IntellisenseEntry) lbIntellisense.SelectedItem;
            }
            set
            {
                lbIntellisense.SelectedItem = value;
                lbIntellisense.ScrollIntoView(value);
            }
        }

        /// <summary>
        /// Gets/Sets the selected index
        /// </summary>
        public int SelectedIndex
        {
            get { return lbIntellisense.SelectedIndex; }
            set
            {
                int newValue = value;
                if (newValue < 0)
                    newValue = 0;
                if (newValue >= lbIntellisense.Items.Count)
                    newValue = lbIntellisense.Items.Count-1;

                lbIntellisense.SelectedIndex = newValue;
                lbIntellisense.ScrollIntoView(lbIntellisense.Items[newValue]);
            }
        }

        /// <summary>
        /// Returns the number of items in the list
        /// </summary>
        public int ItemCount
        {
            get { return lbIntellisense.Items.Count; }
        }

        /// <summary>
        /// Event raised when a keypress occurs in the list.
        /// </summary>
        public event KeyEventHandler ListKeyDown = delegate { };

        /// <summary>
        /// Event raised when a double-click occurs in the list.
        /// </summary>
        public event MouseButtonEventHandler ListItemDoubleClick = delegate { };

        /// <summary>
        /// Method to raise the DoubleClick event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void LbItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListItemDoubleClick.Invoke(this, e);
        }

        /// <summary>
        /// Method to raise the KeyPress event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void LbIntellisenseOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ListKeyDown.Invoke(this, e);
        }
    }
}
