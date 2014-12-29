using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using EmguWF.Activities.Extensions;
using EmguWF.Activities.Extensions.Contracts;

namespace EmguWF.Activities
{
    public class InvertImage<T, K> : CodeActivity
        where T : struct, IColor
        where K : new()
    {
        [Description("Image")]
        [RequiredArgument]
        public InOutArgument<Image<T, K>> Image { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public InvertImage()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            var image = Image.Get(context);

            image = image.Not();
            context.GetExtension<IImageStore>().Store(image);
            Image.Set(context, image);
        }
    }
}