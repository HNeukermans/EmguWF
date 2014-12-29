using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using EmguWF.Activities.Designers;
using EmguWF.Activities.Extensions.Contracts;

namespace EmguWF.Activities.Activities
{
    [Designer(typeof (GrayBinaryAdaptiveThresholdDesigner))]
    public class GrayBinaryAdaptiveThreshold : CodeActivity
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

        [RequiredArgument]
        [Description("The size of the adaptivity filter (3,5,7,...).")]
        [DefaultValue(5)]
        public int BlockSize { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GrayBinaryAdaptiveThreshold()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            var image = Image.Get(context);
            image = image.ThresholdAdaptive(new Gray(MaxValue), ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_GAUSSIAN_C,
                                            THRESH.CV_THRESH_BINARY, BlockSize, new Gray(0) /*constant subtracted for mean or weighed mean*/);
            context.GetExtension<IImageStore>().Store(image);
            Image.Set(context, image);
        }
    }
}