using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Imb.Behaviours
{
    public class VisibleWhen : Behavior<UIElement>
    {
        #region Binding Property

        public object Binding
        {
            get { return (object)GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }

        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.Register("Binding", typeof(object), typeof(VisibleWhen),
                new UIPropertyMetadata(null, OnBindingChanged));

        /// <summary>
        /// This is called when the property on the bound DataContext changes.
        /// </summary>
        /// <param name="sender">the <see cref="VisibleWhen"/> instance</param>
        /// <param name="e">The parameters including the new value</param>
        private static void OnBindingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var visibleWhenInstance = sender as VisibleWhen;
            if (e.NewValue is bool && visibleWhenInstance != null)
            {
                var boundValue = (bool)e.NewValue;
                visibleWhenInstance.SetVisibility(boundValue);
            }
        }

        #endregion

        #region Condition Property

        public bool Condition
        {
            get { return (bool)GetValue(ConditionProperty); }
            set { SetValue(ConditionProperty, value); }
        }

        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(bool), typeof(VisibleWhen), new PropertyMetadata(default(bool)));

        #endregion

        #region Otherwise Property

        public Visibility Otherwise
        {
            get { return (Visibility) GetValue(OtherwiseProperty); }
            set { SetValue(OtherwiseProperty, value); }
        }

        public bool TakeFocus
        {
            get { return (bool) GetValue(TakeFocusProperty); }
            set { SetValue(TakeFocusProperty, value); }
        }

        public static readonly DependencyProperty OtherwiseProperty = DependencyProperty.Register("Otherwise", typeof (Visibility), typeof (VisibleWhen), new PropertyMetadata(default(Visibility)));
        public static readonly DependencyProperty TakeFocusProperty = DependencyProperty.Register("TakeFocus", typeof (bool), typeof (VisibleWhen), new PropertyMetadata(default(bool)));

        #endregion

        private void SetVisibility(bool boundValue)
        {
            if (AssociatedObject == null) return;

            var condition = (bool)GetValue(ConditionProperty);
            var otherwiseValue = (Visibility) GetValue(OtherwiseProperty);
            AssociatedObject.Visibility = condition == boundValue ? Visibility.Visible : otherwiseValue;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            var value = GetValue(BindingProperty);
            if (value != null)
            {
                SetVisibility((bool)value);
                var takeFocus = (bool) GetValue(TakeFocusProperty);
                if (takeFocus)
                    AssociatedObject.Focus();
            }
            Debug.Assert(value != null);
        }

    }
}