using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguWF.Activities.Extensions;

namespace EmguWF.Activities.Activities
{
    public class GrayBinaryThreshold : CodeActivity
    {
        [Description("Image")]
        [RequiredArgument]
        public InOutArgument<Image<Gray, byte>> Image { get; set; }

        [RequiredArgument]
        [Description("If gray value in source image if above Threshold then if will have maxvalue in destination image")]
        [DefaultValue(125)]
        public int Threshold { get; set; }

        [RequiredArgument]
        [DefaultValue(255)]
        public int MaxValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GrayBinaryThreshold()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            var image = Image.Get(context);
            image = image.ThresholdBinary(new Gray(Threshold),new Gray(MaxValue));
            context.GetExtension<IImageStore>().Store(image);
            Image.Set(context, image);
        }
    }
}