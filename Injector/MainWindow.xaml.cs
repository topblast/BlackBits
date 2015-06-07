using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;


namespace Injector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public class ProcessItem
        {
            public string DisplayName { get; set; }
            public int Id { get; set; }
            public bool is64Bit { get; set; }
        }

        public class MainDependency : DependencyObject
        {
            public static readonly DependencyProperty ProcessListProperty =
                DependencyProperty.Register("ProcessList", typeof(List<ProcessItem>), typeof(MainDependency), new PropertyMetadata(null));

            public static readonly DependencyProperty ProcessIdProperty =
                DependencyProperty.Register("ProcessId", typeof(int), typeof(MainDependency), new PropertyMetadata(0));

            public MainDependency()
            {
                ProcessList = new List<ProcessItem>();
            }

            public List<ProcessItem> ProcessList
            {
                get { return (List<ProcessItem>)GetValue(ProcessListProperty); }
                set { SetValue(ProcessListProperty, value); }
            }

            public int ProcessId
            {
                get { return (int)GetValue(ProcessIdProperty); }
                set { SetValue(ProcessIdProperty, value); }
            }
        }

        public MainDependency Dependency { get; set; }

        public MainWindow()
        {
            Dependency = new MainDependency();
            RefreshProcesses();
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcesses();
        }

        private void RefreshProcesses()
        {
            int pId = Dependency.ProcessId;

            Dependency.ProcessList = Process.GetProcesses().OrderBy(p => p.ProcessName).Select(p => new ProcessItem() 
                {
                    Id = p.Id,
                    DisplayName = string.Format("{0} - {1}", p.Id, p.ProcessName)
                }).ToList();

            Dependency.ProcessId = Dependency.ProcessList.Exists(p => p.Id == pId) ? pId : 0;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BlackBitInjector.Inject(Process.GetProcessById(Dependency.ProcessId));
        }
    }
}
