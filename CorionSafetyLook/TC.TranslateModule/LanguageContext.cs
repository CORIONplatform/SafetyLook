using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace TC.Translate
{
    public sealed class LanguageContext : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public static readonly LanguageContext Instance = new LanguageContext();

        private CultureInfo _cultureInfo;
        private Dictionary<string, string> _dictionary = null;


        public Dictionary<string, string> Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = this.LoadDictionary(this.Culture);
                }
                return _dictionary;
            }
            set
            {
                if (value != null && value != _dictionary)
                {
                    _dictionary = value;
                    OnPropertyChanged("Dictionary");
                }
            }
        }

        public CultureInfo Culture
        {
            get { return _cultureInfo ?? (_cultureInfo = CultureInfo.CurrentUICulture); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Culture nem lehet null!");
                }
                if (value == _cultureInfo)
                {
                    return;
                }

                _cultureInfo = value;
                
                var dict = this.LoadDictionary(_cultureInfo);
                Thread.CurrentThread.CurrentUICulture = _cultureInfo;
                Dictionary = dict;
                OnPropertyChanged("Culture");
            }
        }


        private LanguageContext() { }


        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }



        private Dictionary<string, string> LoadDictionary(CultureInfo language)
        {
            var filePath = "Dictionaries\\" + language.ToString().Replace("-", "") + ".dic";
            if(!System.IO.File.Exists(filePath) && language.ToString() != "en-US")
            {
                return LoadDictionary(CultureInfo.GetCultures(CultureTypes.AllCultures).Single(c => c.Name == "en-US"));
            }
            var lines = System.IO.File.ReadAllLines(filePath);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var line in lines.Where(p => p.Contains("=")))
            {
                var key = line.Split('=')[0].Trim();
                var value = line.Split('=')[1].Trim();
                dict.Add(key, value);
            }
            return dict;
        }

        public string GetText(string Define)
        {
            if (this.Dictionary.ContainsKey(Define))
                return this.Dictionary[Define];
            return Define;
        }
    }
}