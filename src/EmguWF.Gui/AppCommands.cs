using System.Windows.Input;

namespace EmguWF.Gui
{
    public static class AppCommands
    {
        public static readonly RoutedCommand Exit = new RoutedCommand("Exit", typeof(AppCommands));
    }
}