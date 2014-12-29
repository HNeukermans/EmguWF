using System;
using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguWF.Activities.Extensions;
using EmguWF.Activities.Extensions.Contracts;

namespace EmguWF.Activities
{
    public class BgrGrayConvertor : CodeActivity
    {
        [Description("Image")]
        [RequiredArgument]
        public InArgument<Image<Bgr, byte>> Image { get; set; }

        [Description("Gray Image")]
        [RequiredArgument]
        public OutArgument<Image<Gray, byte>> GrayImage { get; set; }
        
       
        protected override void Execute(CodeActivityContext context)
        {
            var image = Image.Get(context);
            var grayImage = image.Convert<Gray,Byte>();
            context.GetExtension<IImageStore>().Store(grayImage);
            GrayImage.Set(context, grayImage);
        }
    }
}