using System;
using System.Windows;
using System.Collections.ObjectModel;
using Excel = Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;
using Workbook = Microsoft.Office.Interop.Excel.Workbook;
using Worksheet = Microsoft.Office.Interop.Excel.Worksheet;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Data;
using Microsoft.Win32;
using Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;
using DataTable = System.Data.DataTable;
using System.Windows.Threading;

namespace Report
{
    public partial class MainWindow2 : Window
    {

        ObservableCollection<Collector> Collectors;

        public static Collector collector;

        public MainWindow2()
        {
            Collectors = new ObservableCollection<Collector>();
            InitializeComponent();
            dGrid.DataContext = Collectors;
            FillData();
            Name.TextChanged += SearchButton_Click;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DataContext = DateTime.Now;
        }
        public void FillData()//заполнить список
        {
            Collectors.Clear();
            foreach (var item in Collector.GetAllCollector())
            {
                Collectors.Add(item);
            }
        }

        private void dGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = dGrid.SelectedItem as Collector;
            if (selectedService == null)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var result = MessageBox.Show("Вы уверены?", "Удалить запись", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // получение выбранных строк
                List<Collector> selectedServices = dGrid.SelectedItems.Cast<Collector>().ToList();
                {
                    // проход по списку выбранных строк
                    foreach (Collector service in selectedServices)
                    {
                        using (var db = new SQLiteConnection("Data Source=Uspeh.db"))
                        {
                            db.Open();
                            var command = new SQLiteCommand(db);
                            command.CommandText = "DELETE FROM Collectors WHERE Id = @id";
                            command.Parameters.AddWithValue("@id", service.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                FillData();
            }
        }






        private void Exsport_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application();
            excel.Visible = true;
            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet1 = (Worksheet)workbook.Sheets[1];

            //Заполнение заголовков
            for (int j = 0; j < dGrid.Columns.Count; j++)
            {
                Range myRange = (Range)sheet1.Cells[1, j + 1];
                sheet1.Cells[1, j + 1].Font.Bold = true;
                sheet1.Cells[1, j + 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                myRange.Value2 = dGrid.Columns[j].Header;
            }

            //Заполнение ячеек таблицы
            for (int i = 0; i < dGrid.Items.Count; i++)
            {
                for (int j = 0; j < dGrid.Columns.Count; j++)
                {
                    DataGridCell cell = GetCell(i, j);
                    TextBlock b = cell.Content as TextBlock;
                    if (b != null)
                    {
                        Range myRange = (Range)sheet1.Cells[i + 2, j + 1];
                        myRange.Value2 = b.Text;
                        myRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    }
                }
            }

            // автоматическое подгонение ширины колонок под содержимое
            Range usedRange = sheet1.UsedRange;
            usedRange.Columns.AutoFit();
        }



        //Метод для получения ячейки таблицы DataGrid
        private DataGridCell GetCell(int row, int column)
        {
            DataGridRow rowContainer = GetRow(row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                if (presenter == null)
                {
                    dGrid.ScrollIntoView(rowContainer, dGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }

            return null;
        }

        //Метод для получения строки таблицы DataGrid
        private DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)dGrid.ItemContainerGenerator.ContainerFromIndex(index);

            if (row == null)
            {
                dGrid.UpdateLayout();
                dGrid.ScrollIntoView(dGrid.Items[index]);
                row = (DataGridRow)dGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return row;
        }

        //Метод для получения дочернего элемента элемента типа T
        private T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        private void btnAdd__Click(object sender, RoutedEventArgs e)
        {
            BD_Form bd_Form = new BD_Form();
            bd_Form.Owner = this;//первичное окно назначаем главным
            bd_Form.Show();//ждет закрытия окна
            FillData();
        }

        private void btnImport_Clickbtn(object sender, RoutedEventArgs e)
        {
            // создание диалогового окна для выбора файла Excel
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            // проверка, был ли выбран файл
            if (openFileDialog.ShowDialog() == true)
            {
                // вызов метода для импорта данных из Excel в базу данных
                ImportExcelToDatabase(openFileDialog.FileName);
            }

            FillData();
        }

        private void ImportExcelToDatabase(string filePath)
        {
          try
           {
                // строка подключения к базе данных SQLite
                string connectionString = @"Data Source=Uspeh.db;Version=3;";

            // создание объекта подключения
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // открытие подключения
                connection.Open();

                // создание объекта команды
                SQLiteCommand command = new SQLiteCommand();

                // привязка команды к объекту подключения
                command.Connection = connection;

                // создание объекта Excel
                Excel.Application excel = new Excel.Application();

                // открытие книги Excel по пути к файлу
                Excel.Workbook workbook = excel.Workbooks.Open(filePath);

                // выбор листа Excel для чтения данных
                Excel._Worksheet worksheet = workbook.Sheets[1];

                // получение диапазона ячеек для чтения данных
                Excel.Range range = worksheet.UsedRange;

                // определение количества колонок в таблице Excel
                int columnCount = range.Columns.Count;

                // создание SQL-запроса для вставки данных в таблицу Collectors
                string query = "INSERT INTO Collectors (Name, Gun, Automaton_serial, Automaton, Permission, Meaning, Certificate, Token, Power) VALUES (@Name, @Gun, @Automaton_serial, @Automaton, @Permission, @Meaning, @Certificate, @Token, @Power)";

                // привязка SQL-запроса к объекту команды
                command.CommandText = query;

                // создание параметров для SQL-запроса
                command.Parameters.Add(new SQLiteParameter("@Name", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Gun", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Automaton_serial", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Automaton", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Permission", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Meaning", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Certificate", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Token", DbType.String));
                command.Parameters.Add(new SQLiteParameter("@Power", DbType.String));

                // проход по строкам диапазона
                for (int row = 2; row <= range.Rows.Count; row++)
                {
                    // создание массива для хранения значений ячеек строки
                    object[] rowValues = new object[columnCount];

                    // проход по ячейкам строки и заполнение массива rowValues
                    for (int col = 1; col <= columnCount; col++)
                    {
                        if (range.Cells[row, col].Value2 != null)
                        {
                            rowValues[col - 1] = (range.Cells[row, col] as Excel.Range).Value2.ToString();
                        }
                        else
                        {
                            rowValues[col - 1] = "";
                        }
                    }

                    // проверка, что все необходимые ячейки в строке не пустые
                    if (rowValues[0] != null && rowValues[1] != null && rowValues[2] != null && rowValues[3] != null && rowValues[4] != null)
                    {
                        command.Parameters["@Name"].Value = rowValues[0].ToString();
                        command.Parameters["@Gun"].Value = rowValues[1].ToString();
                        command.Parameters["@Automaton_serial"].Value = rowValues[2]?.ToString() ?? "";
                        command.Parameters["@Automaton"].Value = rowValues[3]?.ToString() ?? "";
                        command.Parameters["@Permission"].Value = rowValues[4]?.ToString() ?? "";
                        command.Parameters["@Meaning"].Value = rowValues.Length > 5 ? rowValues[5]?.ToString() ?? "" : "";
                        command.Parameters["@Certificate"].Value = rowValues.Length > 6 ? rowValues[6]?.ToString() ?? "" : "";
                        command.Parameters["@Token"].Value = rowValues.Length > 7 ? rowValues[7]?.ToString() ?? "" : "";
                        command.Parameters["@Power"].Value = rowValues.Length > 8 ? rowValues[8]?.ToString() ?? "" : "";

                        // выполнение SQL-запроса
                        command.ExecuteNonQuery();
                    }
                }

                    // закрытие книги Excel
                    workbook.Close(false);

                    // закрытие приложения Excel
                    excel.Quit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите сохранить изменения?", "Сохранить изменения", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {


                    foreach (var item in dGrid.Items)
                    {
                        if (item is Collector selectedCollector)
                        {
                            selectedCollector.Update();
                        }
                    }
                    FillData(); // обновляем данные в таблице после обновления
                }
                else
                {
                    FillData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

  
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
               
                var result = MessageBox.Show("Сохранить изменения?", "Сохранить изменения", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                   
                    foreach (var item in dGrid.Items)
                    {
                        if (item is Collector selectedCollector)
                        {
                            selectedCollector.Update();
                        }
                    }
                    // обновляем данные в таблице после обновления
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true; // отменяем закрытие окна
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

      


        private void SearchButton_Click(object sender, TextChangedEventArgs e)
        {
          
            try
            { // строка подключения к базе данных SQLite
                string connectionString = @"Data Source=Uspeh.db;Version=3;";
                string searchTerm = Name.Text;
                List<Collector> collectors = new List<Collector>();

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand($"SELECT * FROM collectors WHERE name LIKE '%{searchTerm}%'", connection);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Collector collector = new Collector
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Gun = reader["Gun"].ToString(),
                            Automaton_serial = reader["Automaton_serial"].ToString(),
                            Automaton = reader["Automaton"].ToString(),
                            Permission = reader["Permission"].ToString(),
                            Meaning = reader["Meaning"].ToString(),
                            Certificate = reader["Certificate"].ToString(),
                            Token = reader["Token"].ToString(),
                            Power = reader["Power"].ToString()
                        };
                        collectors.Add(collector);
                    }
                }

                dGrid.ItemsSource = collectors;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}




