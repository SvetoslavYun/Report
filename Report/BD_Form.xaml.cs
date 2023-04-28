using System;
using System.Windows;

namespace Report
{

    public partial class BD_Form : Window
    {
        public BD_Form()
        {
            InitializeComponent();
            grid.DataContext = MainWindow.collector;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var collector = new Collector()
            {
                Name = Name.Text,
                Gun = Convert.ToString(Gun.Text),
                Automaton_serial = Convert.ToString(Automaton_serial.Text),
                Automaton = Convert.ToString(Automaton.Text),
                Permission = Convert.ToString(Permission.Text),
                Meaning = Convert.ToString(Meaning.Text),
                Certificate = Convert.ToString(Certificate.Text),
                Token = Convert.ToString(Token.Text),
                Power = Convert.ToString(Power.Text)
            };
            collector.Insert();
            Close();
        }
    }
}
