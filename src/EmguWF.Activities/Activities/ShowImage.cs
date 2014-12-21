using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EmguWF.Activities
{
    public class ShowImage<T,K> : CodeActivity  where T: struct, IColor where K: new()
    {
        [Description("Image")]
        [RequiredArgument]
        public InArgument<Image<T, K>> Image { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ShowImage()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            var image = Image.Get(context);

            string winname = "First window";

            CvInvoke.cvNamedWindow(winname);

            CvInvoke.cvShowImage(winname, image.Ptr);

            CvInvoke.cvWaitKey(0);

            CvInvoke.cvDestroyWindow(winname);
        }
    }
}