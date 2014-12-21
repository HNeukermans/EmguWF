using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguWF.Activities;
using EmguWF.Activities.Extensions;

namespace EmguWF.ViewModels
{
    public class EmguWFImageStore : IImageStore
    {
        private string _folder;
        private int _counter;

        public EmguWFImageStore()
        {
            InitFolder();
            InitCounter();
        }

        private void InitFolder()
        {
            var basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            _folder = Path.Combine(basePath, "EmguWF");
            if (!Directory.Exists(_folder)) Directory.CreateDirectory(_folder);
        }

        private void InitCounter()
        {
            var getFiles = new DirectoryInfo(_folder).GetFiles("img_*.bmp").ToList();

            if (getFiles.Count == 0)
            {
                _counter = 0;
                return;
            } 

            var ordered = getFiles.OrderByDescending(f => f.Name);


            var fileName = ordered.First().Name;
            var nameLength = fileName.Length;
            _counter = int.Parse(fileName.Substring(nameLength - 7, 3));
        }

        private string GetUniqueFilePath()
        {
            return Path.Combine(_folder, "img_" + (++_counter).ToString("000") + ".bmp");
        }

        public void Store<T, K>(Image<T, K> image) where T : struct, IColor where K : new()
        {
            if (image == null) return;
            image.Save(GetUniqueFilePath());
        }
    }
}
