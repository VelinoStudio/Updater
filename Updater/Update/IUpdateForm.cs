using System;

namespace VelinoStudio.Updater
{
    public interface IUpdateForm
    {
        event EventHandler<UpdateArgs> UpdateProgressing;
    }
}
