using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguWF.Activities.Extensions;

namespace EmguWF.Activities.Activities
{
    public class BgrErode : CodeActivity
    {
        [Description("Image")]
        [RequiredArgument]
        public InOutArgument<Image<Bgr, byte>> Image { get; set; }

        [RequiredArgument]
        [Description("Number of Iterations")]
        [DefaultValue(1)]
        public int Iterations { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BgrErode()
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