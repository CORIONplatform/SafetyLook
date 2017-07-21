using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace TC.Translate
{
    public class LanguageConverter : IValueConverter, IMultiValueConverter
    {
        #region Fields

        private string _define;
        private string _value;

        #endregion

        #region Initialization

        public LanguageConverter(string define, string value)
        {
            this._define = define;
            this._value = value;
        }

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LanguageContext.Instance.GetText(_define);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
             throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }

        #endregion

    }
}

