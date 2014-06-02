using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Imb.Behaviours
{
    public class KeyCommand : Behavior<UIElement>
    {
        #region Command Property

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(KeyCommand),
                new UIPropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// This is called when the property on the bound DataContext changes.
        /// </summary>
        /// <param name="sender">the <see cref="KeyCommand"/> instance</param>
        /// <param name="e">The parameters including the new value</param>
        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var visibleWhenInstance = sender as KeyCommand;
            if (e.NewValue is KeyCommand && visibleWhenInstance != null)
            {
                var boundValue = (KeyCommand)e.NewValue;
            }
        }

        #endregion

        #region Key Property

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(Key), typeof(KeyCommand), new PropertyMetadata(default(Key)));

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += OnKeyUp;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            var key = (Key) GetValue(KeyProperty);
            if (e.Key == key && AssociatedObject.IsKeyboardFocusWithin)
            {
                var command = GetValue(CommandProperty) as ICommand;
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
        }
    }
}