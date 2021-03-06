﻿using Emgu.CV;

namespace EmguWF.Activities.Extensions.Contracts
{
    public interface IImageStore
    {
        void Store<T,K>(Image<T, K> image) where T:struct,IColor where K:new();
    }
}