using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguWF.Activities.Extensions;
using EmguWF.Activities.Extensions.Contracts;

namespace EmguWF.Activities
{
    public class ImageLoader : CodeActivity
    {
        /// <summary>
        /// First sequence to align.
        /// </summary>
        [RequiredArgument]
        [Category("Emgu")]
        [Description("File")]
        public string File { get; set; }

        /// <summary>
        /// First sequence to align.
        /// </summary>
        [Category("Emgu")]
        [RequiredArgument]
        [Description("Image")]
        public OutArgument<Image<Bgr,Byte>> Image { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ImageLoader()
        {
           
        }

        protected override void Execute(CodeActivityContext context)
        {
            Image<Bgr,Byte> image = new Image<Bgr, byte>(File);
            context.GetExtension<IImageStore>().Store(image);
            Image.Set(context, image);
        }
    }
}
