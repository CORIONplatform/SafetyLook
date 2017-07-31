using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var fileName = language.ToString().Replace("-", "") + ".dic";
            string[] lines = { };

            var assembly = Assembly.GetEntryAssembly();
            var fileData = assembly.GetManifestResourceNames().SingleOrDefault(p => p.Contains("Dictionaries." + fileName));
            if (!string.IsNullOrEmpty(fileData))
            {
                // try to get from embed resource
                using (Stream stream = assembly.GetManifestResourceStream(fileData))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            lines = reader.ReadToEnd().Split('\n');
                        }
                    }
                }
            }
            // try to get from files
            if (lines == null || lines.Count() == 0)
            {
                var filePath = "Dictionaries\\" + fileName;
                if (!System.IO.File.Exists(filePath) && language.ToString() != "en-US")
                {
                    return LoadDictionary(CultureInfo.GetCultures(CultureTypes.AllCultures).Single(c => c.Name == "en-US"));
                }
                lines = System.IO.File.ReadAllLines(filePath);
            }
            // load the dictionary
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