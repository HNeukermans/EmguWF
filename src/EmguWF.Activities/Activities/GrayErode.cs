using System;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguWF.Activities.Extensions;
using EmguWF.Activities.Extensions.Contracts;

namespace EmguWF.Activities
{
    public class GrayErode : CodeActivity
    {
        [Description("Image")]
        [RequiredArgument]
        public InOutArgument<Image<Gray, byte>> Image { get; set; }

        [RequiredArgument]
        [Description("Number of Iterations")]
        [DefaultValue(1)]
        public int Iterations { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GrayErode()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            var image =  Image.Get(context);
            image = image.Erode(Iterations);
            context.GetExtension<IImageStore>().Store(image);
            Image.Set(context, image);
        }
    }
}