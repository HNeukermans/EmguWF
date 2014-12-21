using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.ComponentModel;
using System.Drawing;
using Microsoft.Win32;
using System.Linq;
using System.Windows;

namespace EmguWF.Activities.Designers
{

    public partial class ImageLoaderDesigner
    {
        public ImageLoaderDesigner()
        {
            InitializeComponent();
        }

        private void ShowFileDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            
            if (ofd.ShowDialog() == true)
            {
                ModelProperty item = ModelItem.Properties["File"];
                item.SetValue(ofd.FileName);
            }
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(
                typeof(ImageLoader),
                new DesignerAttribute(typeof(ImageLoaderDesigner)),
                new DescriptionAttribute("Saves a set of sequences to a supported file format."),
                new ToolboxBitmapAttribute(typeof(ImageLoader), @"Images.save_sequences.png"));
        } 
    }
}
