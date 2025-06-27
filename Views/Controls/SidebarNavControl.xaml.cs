using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.Views.Controls {
    public partial class SidebarNavControl : UserControl {
        public SidebarNavControl() {
            InitializeComponent();
        }

        // ItemsSource DP (IEnumerable of strings)
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(SidebarNavControl),
                new PropertyMetadata(null)
            );

        public IEnumerable ItemsSource {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // SelectedItem DP (string)
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(SidebarNavControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged)
            );

        public object SelectedItem {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctl = (SidebarNavControl)d;
            if (ctl.Command?.CanExecute(e.NewValue) == true)
                ctl.Command.Execute(e.NewValue);
        }

        // Command DP (ICommand)
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(SidebarNavControl),
                new PropertyMetadata(null)
            );

        public ICommand Command {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }
}
