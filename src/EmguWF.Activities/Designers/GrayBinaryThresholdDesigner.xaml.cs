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

    public partial class GrayBinaryThresholdDesigner
    {
        public GrayBinaryThresholdDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(
                typeof (GrayBinaryThreshold),
                new DesignerAttribute(typeof (GrayBinaryThresholdDesigner)),
                new DescriptionAttribute(
                    "Threshold the image such that: dst(x,y) = max_value, if src(x,y)>threshold; 0, otherwise "));
        } 
    }
}
