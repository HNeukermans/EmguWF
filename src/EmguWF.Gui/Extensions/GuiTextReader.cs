using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace EmguWF.Extensions
{
    public sealed class GuiTextReader : TextReader
    {
        private readonly ManualResetEvent _dataAvailable = new ManualResetEvent(false);
        private Window _window;
        private Thread _uiThread;
        private volatile bool _readyToRead;

        public string Text { get; set; }

        public GuiTextReader()
        {
            Text = string.Empty;
            _readyToRead = true;
        }

        private void RunTextEntryWindow()
        {
            _readyToRead = false;

            _window = new Window
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                Title = "Enter Text",
                Background = Brushes.GhostWhite,
                Topmost = true,
            };

            StackPanel sp = new StackPanel { Margin = new Thickness(10) };
            _window.Content = sp;

            var tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 300,
                Padding = new Thickness(5),
                Margin = new Thickness(5),
            };
            tb.SetBinding(TextBox.TextProperty, new Binding("Text") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            sp.Children.Add(new TextBlock { Text = "Enter Text:", Margin = new Thickness(5) });
            sp.Children.Add(tb);

            Button btn = new Button
            {
                Content = "OK",
                Margin = new Thickness(5),
                Padding = new Thickness(15, 5, 15, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            btn.Click += (s, e) => _window.DialogResult = (Text.Length > 0);
            sp.Children.Add(btn);
            if (_window.ShowDialog() == false)
            {
                Text = string.Empty;
            }

            _dataAvailable.Set();
            _uiThread = null;
            _window = null;
        }

        public override int Read()
        {
            if (_window == null && _readyToRead)
            {
                _dataAvailable.Reset();
                _uiThread = new Thread(RunTextEntryWindow);
                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.Start();
            }

            _dataAvailable.WaitOne();
            if (Text.Length > 0)
            {
                char ch = Text[0];
                Text = Text.Substring(1);
                return ch;
            }

            _readyToRead = true;
            return -1;
        }

        public override int Peek()
        {
            return Text.Length > 0 ? Text[0] : -1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_uiThread != null)
                {
                    _window.Dispatcher.InvokeAsync(() => _window.DialogResult = false);
                }
            }

            base.Dispose(disposing);
        }
    }
}
