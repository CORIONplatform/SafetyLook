using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TC.Translate;

namespace CorionSafetyLook
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //LanguageContext.Instance.Culture = CultureInfo.GetCultures(CultureTypes.AllCultures).First(c => c.Name.EndsWith("-US"));
            base.OnStartup(e);
        }
    }
}
