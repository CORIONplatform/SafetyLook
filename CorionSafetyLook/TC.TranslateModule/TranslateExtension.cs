using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace TC.Translate
{
    [ContentProperty("Parameters")]
    public class Translate : MarkupExtension
    {
        #region Fields

        private DependencyProperty _property;
        private DependencyObject _target;
        private string _define;

        private readonly Collection<BindingBase> _parameters = new Collection<BindingBase>();

        #endregion

        #region Initialization

        public Translate() { }

        public Translate(object defaultValue)
        {
            this._define = defaultValue.ToString();
        }

        #endregion

        #region Properties


        public string Define => _define;

        public Collection<BindingBase> Parameters => _parameters;


        public static readonly DependencyProperty DefineProperty =
          DependencyProperty.RegisterAttached("Define", typeof(string), typeof(Translate), new UIPropertyMetadata(string.Empty));

        #region UidProperty DProperty

        public static string GetDefine(DependencyObject obj)
        {
            return (string)obj.GetValue(DefineProperty);
        }

        public static void SetDefine(DependencyObject obj, string value)
        {
            obj.SetValue(DefineProperty, value);
        }


        #endregion

        #endregion

        #region Overrides

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (service == null)
            {
                return this;
            }

            DependencyProperty property = service.TargetProperty as DependencyProperty;
            DependencyObject target = service.TargetObject as DependencyObject;
            if (property == null || target == null)
            {
                return this;
            }

            this._target = target;
            this._property = property;

            return BindDictionary(serviceProvider);
        }

        #endregion

        #region Privates

        private object BindDictionary(IServiceProvider serviceProvider)
        {
            string define = _define ?? GetDefine(_target);
            string value = _property.Name;

            Binding binding = new Binding("Dictionary")
            {
                Source = LanguageContext.Instance,
                Mode = BindingMode.TwoWay
            };
            LanguageConverter converter = new LanguageConverter(define, value);
            if (_parameters.Count == 0)
            {
                binding.Converter = converter;
                object realvalue = binding.ProvideValue(serviceProvider);
                return realvalue;
            }
            else
            {
                MultiBinding multiBinding = new MultiBinding
                {
                    Mode = BindingMode.TwoWay,
                    Converter = converter
                };
                multiBinding.Bindings.Add(binding);
                if (string.IsNullOrEmpty(define))
                {
                    Binding defineBinding = _parameters[0] as Binding;
                    if (defineBinding == null)
                    {
                        throw new ArgumentException("Define Binding parameter must be the first, and of type Binding");
                    }
                }
                foreach (Binding parameter in _parameters)
                {
                    multiBinding.Bindings.Add(parameter);
                }
                object realvalue = multiBinding.ProvideValue(serviceProvider);
                return realvalue;
            }
        }
        #endregion
    }
}
