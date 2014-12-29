using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguWF.Activities.Extensions;
using EmguWF.Activities.Extensions.Contracts;

namespace EmguWF.Activities
{
    public class GrayDilate : CodeActivity
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
        public GrayDilate()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            var image = Image.Get(context);
            image = image.Dilate(Iterations);
            context.GetExtension<IImageStore>().Store(image);
            Image.Set(context, image);
        }
    }
}