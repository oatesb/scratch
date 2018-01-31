using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        Foo _f;
        public MainWindow()
        {
            
            InitializeComponent();
            //this.DataContext = def;

            //this.DataContext = list;
            //List<Foo> foolist;
            foolist = new ObservableCollection<Foo>();
            foolist.Add(new Foo("Ben1", 7));
            foolist.Add(new Foo("Ben2", 17));
            foolist.Add(new Foo("Ben3", 27));
            foolist.Add(new Foo("Ben4", 37));

        }



        public ObservableCollection<Foo> foolist
        {
            get { return (ObservableCollection<Foo>)GetValue(foolistProperty); }
            set { SetValue(foolistProperty, value); }
        }

        // Using a DependencyProperty as the backing store for foolist.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty foolistProperty =
            DependencyProperty.Register("foolist", typeof(ObservableCollection<Foo>), typeof(MainWindow), new PropertyMetadata(null));



        private void button_Click(object sender, RoutedEventArgs e)
        {

            foreach (var cell in myGrid.SelectedCells)
            {
                var h = cell.Column.Header;
                _f = (Foo)cell.Item;
                _f.MyInt++;
                _f.MyComment = $"Age Changed {_f.MyInt} when {h} column was clicked";
            }
        }
    }

    public class Foo : INotifyPropertyChanged
    {
        public Foo( string s, int i)
        {
            MyStr = s;
            MyInt = i;
            MyComment = "";

        }
        private string _MyStr;
        private string _MyComment;
        private int _MyInt;

        public string MyStr
        {
            get { return _MyStr; }
            set
            {
                _MyStr = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MyStr"));
            }
        }

        public string MyComment
        {
            get { return _MyComment; }
            set
            {
                _MyComment = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MyComment"));
            }
        }

        public int MyInt
        {
            get { return _MyInt; }
            set
            {
                _MyInt = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MyInt"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    
}
