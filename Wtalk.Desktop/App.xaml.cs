using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WTalk.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       public App()
        {
            Core.Utils.FileCache.Initialize(System.IO.Directory.GetCurrentDirectory(), "cache");
        }
    }
}
