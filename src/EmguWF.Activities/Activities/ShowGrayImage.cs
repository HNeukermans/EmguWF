﻿using System.Activities;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EmguWF.Activities
{
    public class ShowGrayImage : CodeActivity
    {
        [Description("Image")]
        [RequiredArgument]
        public InArgument<Image<Gray, byte>> Image { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ShowGrayImage()
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