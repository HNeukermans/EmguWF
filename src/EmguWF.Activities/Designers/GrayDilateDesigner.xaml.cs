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

    public partial class GrayDilateDesigner
    {
        public GrayDilateDesigner()
        {
            InitializeComponent();
        }
        


        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(
                typeof (GrayDilate),
                new DesignerAttribute(typeof (GrayDilateDesigner)),
                new DescriptionAttribute("Dilates an image."));
        } 
    }
}
