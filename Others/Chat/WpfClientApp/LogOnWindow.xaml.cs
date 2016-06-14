namespace WpfClientApp
{
    /// <summary>
    /// Interaction logic for LogOnWindow.xaml
    /// </summary>
    public partial class LogOnWindow : TitleBarFreeWindow
    {
        public LogOnWindow()
        {
            InitializeComponent();
            DataContext = new LogOnViewModel();
        }       
    }
}
