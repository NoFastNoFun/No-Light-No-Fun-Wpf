using System.Windows.Controls;
using System.Windows.Media;
using No_Fast_No_Fun_Wpf.ViewModels;

namespace No_Fast_No_Fun_Wpf.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour DmxMonitorControl.xaml
    /// </summary>
    public partial class DmxMonitorControl : UserControl
    {
        public DmxMonitorControl()
        {
            InitializeComponent();
            // blabal
            var viewModel = DataContext as DmxMonitorViewModel;
            if (viewModel != null) {
                viewModel.Logs.CollectionChanged += (s, e) =>
                {
                    if (VisualTreeHelper.GetChild(this, 0) is DockPanel panel &&
                        panel.Children.OfType<ListBox>().FirstOrDefault() is ListBox lb) {
                        lb.ScrollIntoView(lb.Items[lb.Items.Count - 1]);
                    }
                };
            }
        }
    }
}
