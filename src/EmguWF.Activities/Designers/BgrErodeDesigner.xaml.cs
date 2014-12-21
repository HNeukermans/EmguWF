using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.ComponentModel;
using System.Drawing;
using EmguWF.Activities.Activities;
using Microsoft.Win32;
using System.Linq;
using System.Windows;

namespace EmguWF.Activities.Designers
{

    public partial class BgrErodeDesigner
    {
        public BgrErodeDesigner()
        {
            InitializeComponent();
        }
        


        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(
                typeof(BgrErode),
                new DesignerAttribute(typeof(BgrErodeDesigner)),
                new DescriptionAttribute("Erodes an image."),
                new ToolboxBitmapAttribute(typeof(ImageLoader), @"Images.save_sequences.png"));
        } 
    }
}
