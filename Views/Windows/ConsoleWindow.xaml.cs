using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf.Views.Windows {
    public partial class ConsoleWindow : Window {
        private readonly UdpListenerService _listener;
        private readonly DmxRoutingService _routingService;
        private readonly Dictionary<int, Point3D> _entityMap;


        public int FromEntity { get; set; } = 100;
        public int ToEntity { get; set; } = 19858;
        public Color SelectedColor { get; set; } = Colors.Red;

        public ConsoleWindow(UdpListenerService listener, DmxRoutingService routingService,Dictionary<int, Point3D> entityMap ) {
            InitializeComponent();
            _listener = listener;
            _routingService = routingService;
            _entityMap = entityMap;
        }
    }
}
