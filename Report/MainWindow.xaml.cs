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
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Report
{
    public partial class MainWindow : Window
    {
        public DbSet<Collector2> collectors { get; set; }
        ObservableCollection<Collector2> Collectors;

        public static Collector2 collector;

        public MainWindow()
        {
            Collectors = new ObservableCollection<Collector2>();
            InitializeComponent();
            dGrid.DataContext = Collectors;
            FillData();
            Name.TextChanged += SearchButton_Click;
            Automaton.TextChanged += SearchButton_Click2;
            datePicker1.SelectedDate = DateTime.Today;
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
            foreach (var item in Collector2.GetAllCollector())
            {
                Collectors.Add(item);
            }
        }

        private Dictionary<string, bool> previousAutomatons = new Dictionary<string, bool>(); // словарь предыдущих значений Automaton

        private void dGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;        
        }





        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = dGrid.SelectedItem as Collector2;
            if (selectedService == null)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var result = MessageBox.Show("Вы уверены?", "Удалить запись", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // получение выбранных строк
                List<Collector2> selectedServices = dGrid.SelectedItems.Cast<Collector2>().ToList();
                {
                    // проход по списку выбранных строк
                    foreach (Collector2 service in selectedServices)
                    {
                        using (var db = new SQLiteConnection("Data Source=Uspeh.db"))
                        {
                            db.Open();
                            var command = new SQLiteCommand(db);
                            command.CommandText = "DELETE FROM Collectors2 WHERE Id = @id";
                            command.Parameters.AddWithValue("@id", service.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                  
                }
                FillData();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            datePicker1.SelectedDate = DateTime.Today;
        }


        private void SetTodayDate(DatePicker datePicker)
        {
            datePicker.SelectedDate = DateTime.Today;
        }

        private void Exsport_Click(object sender, RoutedEventArgs e)
        {
            Name.Text = "";
            Automaton.Text = "";
            try {
               
                Excel.Application excel = new Excel.Application();
            excel.Visible = true;
            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet1 = (Worksheet)workbook.Sheets[1];
           

            for (int j = 0; j < dGrid.Columns.Count; j++)
            {
                Range myRange = (Range)sheet1.Cells[1, j + 1];
                sheet1.Cells[1, j + 1].Font.Bold = true;
                sheet1.Cells[1, j + 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                myRange.Value2 = dGrid.Columns[j].Header;
            }


                // Отслеживаем повторяющиеся значения в 4 столбце (Automaton)
                HashSet<string> automatonValues = new HashSet<string>();

                // Заполнение ячеек
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

                        // Установка стиля рамки ячейки
                        myRange.Borders.LineStyle = XlLineStyle.xlContinuous;
                        myRange.Borders.Weight = XlBorderWeight.xlThin;

                        // Установка стиля шрифта для ячейки
                        if (j == 0 || j == 5)
                        {
                            myRange.Font.Bold = true;
                            myRange.Font.Size = 11;
                        }

                        // Подсветка повторяющихся значений в 4 столбце (Automaton) серым
                        if (j == 3 && !string.IsNullOrEmpty(b.Text))
                        {
                            if (automatonValues.Contains(b.Text))
                            {
                                myRange.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
                            }
                            else
                            {
                                automatonValues.Add(b.Text);
                            }
                        }
                    }
                }
            }

 
                // Удаление строк 2, 3 и 4

                Range row2 = (Range)sheet1.Rows[2];
            row2.Delete();

            Range row3 = (Range)sheet1.Rows[3];
            row3.Delete();

            Range row4 = (Range)sheet1.Rows[4];
            row4.Delete();

            // Автоматическое подгонение ширины колонок под содержимое
            for (int j = 1; j <= dGrid.Columns.Count + 1; j++)
            {
                Range column = (Range)sheet1.Columns[j];
                column.ColumnWidth = 15;
            }
            // Установить первую строку как сквозную строку при печати
            sheet1.PageSetup.PrintTitleRows = "$1:$1";
            // Перейти в режим разметки страницы
            excel.ActiveWindow.View = XlWindowView.xlPageLayoutView;
            sheet1.PageSetup.RightHeader = "&\"Arial\"&10&K000000" + "sviatoslavyun@gmail.com";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        private void Exsport_Click2(object sender, RoutedEventArgs e)
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
                ClearTable();
                int a = 0;
                int b = 0;
                int c = 0;
                int d = 0;
                string name2="t65%^";
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

                    // создание SQL-запроса для вставки данных в таблицу Collectors2
                    string insertQuery = "INSERT INTO Collectors2 (Name, Gun, Automaton_serial, Automaton, Permission, Meaning, Certificate, Token, Power, Armor) VALUES (@Name, @Gun, @Automaton_serial, @Automaton, @Permission, @Meaning, @Certificate, @Token, @Power, @Armor)";



                    // привязка SQL-запросов к объекту команды
                    command.CommandText = insertQuery;


                    //создание параметров для SQL-запроса вставки данных
                    command.Parameters.Add(new SQLiteParameter("@Name", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Gun", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Automaton_serial", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Automaton", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Permission", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Meaning", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Certificate", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Token", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Power", DbType.String));
                    command.Parameters.Add(new SQLiteParameter("@Armor", DbType.String));



                    // проход по строкам диапазона
                    for (int row = 1; row <= range.Rows.Count; row++)
                    {
                        if (range.Cells[row, 2].Value2 == "водитель автомобиля") { c = 2; };
                        if (range.Cells[row, 2].Value2 == "инкассатор-сборщик") { d = 2; };
                        if (range.Cells[row, 3].Value2 != null) { a = 3; b = 1; };
                        if (range.Cells[row, 3].Value2 == null) { a = 2; b = 2; };
                        // проверка наличия значения в столбце Name диапазона
                        if (range.Cells[row, a].Value2 != null)
                        {
                            string name = (range.Cells[row, a] as Excel.Range).Value2.ToString();

                            //чтение данных из таблицы Collectors
                            string gun = "";
                            string automatonSerial = "";
                            string automaton = "";
                            string permission = "";
                            string meaning = "";
                            string certificate = "";
                            string token = "";
                            string power = "";

                            string selectQuery2 = "SELECT Gun, Automaton_serial, Automaton, Permission, Meaning, Certificate, Token, Power FROM Collectors WHERE REPLACE(REPLACE(Name, ' ', ''), '.', '') = REPLACE(REPLACE(@Name, ' ', ''), '.', '')";


                            SQLiteCommand selectCommand2 = new SQLiteCommand(selectQuery2, connection);

                            // привязка значения параметра в SQL-запрос получения данных
                            selectCommand2.Parameters.Add(new SQLiteParameter("@Name", DbType.String));
                            selectCommand2.Parameters["@Name"].Value = name;

                            // выполнение SQL-запроса получения данных
                            SQLiteDataReader reader2 = selectCommand2.ExecuteReader();

                            // чтение данных из таблицы Collectors
                            if (reader2.Read())
                            {
                                gun = reader2.GetString(0);
                                automatonSerial = reader2.GetString(1);
                                automaton = reader2.GetString(2);
                                permission = reader2.GetString(3);
                                meaning = reader2.GetString(4);
                                certificate = reader2.GetString(5);
                                token = reader2.GetString(6);
                                power = reader2.GetString(7);
                            }

                            if (name == "-2146826265" || name == "" || name == "старший бригады инкассаторов" || name == "инкассатор-сборщик" || name == "водитель автомобиля" || name == "Фамилия и инициалы работника службы инкассации" || name == " " || name == "  " || name == "Резеерв" || name == "Резерв")
                            {
                                continue;
                            }
                            if (name != ".")
                            {
                                if (name2 == name) { automaton = " " + automaton; } else { automaton = automaton; }
                                if (b == 2) { automatonSerial = name; certificate = name; name = ""; }
                                if (c == 2) { automaton = automaton; name2 = name; } else { automaton = ""; }
                                if (d == 2) { power = power; } else { power = ""; }

                         
                                // привязка значений параметров в SQL-запрос вставки данных
                                command.Parameters["@Name"].Value = name;
                                command.Parameters["@Gun"].Value = gun;
                                command.Parameters["@Automaton_serial"].Value = automatonSerial;
                                command.Parameters["@Automaton"].Value = automaton;
                                command.Parameters["@Permission"].Value = permission;
                                command.Parameters["@Meaning"].Value = name;
                                command.Parameters["@Certificate"].Value = certificate;
                                command.Parameters["@Token"].Value = token;
                                command.Parameters["@Power"].Value = power;
                                command.Parameters["@Armor"].Value = meaning;

                                // выполнение SQL-запроса вставки данных
                                command.ExecuteNonQuery();
                                c = 0; d = 0;
                                reader2.Close();
                            }
                            else
                            {  // закрытие книги Excel
                                workbook.Close(false);

                                // закрытие приложения Excel
                                excel.Quit();
                            }
                        }
                    }

                  
                }
            }
            catch 
            {
                MessageBox.Show("Данные добавленны");
            }
        }


     

        //private void ImportExcelToDatabase(string filePath)
        //{
        //    ClearTable();
        //    int a = 0;
        //    int b = 0;
        //    // строка подключения к базе данных SQLite
        //    string connectionString = @"Data Source=Uspeh.db;Version=3;";
        //    // создание объекта подключения
        //    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        //    {
        //        // открытие подключения
        //        connection.Open();

        //        // создание объекта команды
        //        SQLiteCommand command = new SQLiteCommand();

        //        // привязка команды к объекту подключения
        //        command.Connection = connection;

        //        // создание объекта Excel
        //        Excel.Application excel = new Excel.Application();

        //        // открытие книги Excel по пути к файлу
        //        Excel.Workbook workbook = excel.Workbooks.Open(filePath);

        //        // выбор листа Excel для чтения данных
        //        Excel._Worksheet worksheet = workbook.Sheets[1];

        //        // получение диапазона ячеек для чтения данных
        //        Excel.Range range = worksheet.UsedRange;

        //        // создание SQL-запроса для вставки данных в таблицу Collectors2
        //        string insertQuery = "INSERT INTO Collectors2 (Name, Gun, Automaton_serial, Automaton, Permission, Meaning, Certificate, Token, Power) VALUES (@Name, @Gun, @Automaton_serial, @Automaton, @Permission, @Meaning, @Certificate, @Token, @Power)";

        //        // привязка SQL-запросов к объекту команды
        //        command.CommandText = insertQuery;

        //        // создание параметров для SQL-запроса вставки данных
        //        command.Parameters.Add(new SQLiteParameter("@Name", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Gun", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Automaton_serial", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Automaton", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Permission", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Meaning", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Certificate", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Token", DbType.String));
        //        command.Parameters.Add(new SQLiteParameter("@Power", DbType.String));



        //        // проход по строкам диапазона
        //        for (int row = 2; row <= range.Rows.Count; row++)
        //        {
        //            if (range.Cells[row, 3].Value2 != null) { a = 3; b = 1; };
        //            if (range.Cells[row, 3].Value2 == null) { a = 2; b = 2; };
        //            // проверка наличия значения в столбце Name диапазона
        //            if (range.Cells[row, a].Value2 != null)
        //            {
        //                string name = (range.Cells[row, a] as Excel.Range).Value2.ToString();

        //                // чтение данных из таблицы Collectors
        //                string gun = "";
        //                string automatonSerial = "";
        //                string automaton = "";
        //                string permission = "";
        //                string meaning = "";
        //                string certificate = "";
        //                string token = "";
        //                string power = "";

        //                string selectQuery2 = "SELECT * FROM Collectors WHERE Name=@Name";

        //                SQLiteCommand selectCommand2 = new SQLiteCommand(selectQuery2, connection);

        //                // привязка значения параметра в SQL-запрос получения данных
        //                selectCommand2.Parameters.Add(new SQLiteParameter("@Name", DbType.String));
        //                selectCommand2.Parameters["@Name"].Value = name;

        //                // выполнение SQL-запроса получения данных
        //                SQLiteDataReader reader2 = selectCommand2.ExecuteReader();

        //                // чтение данных из таблицы Collectors
        //                if (reader2.Read())
        //                {

        //                    gun = reader2.GetString(0);
        //                    automatonSerial = reader2.GetString(1);
        //                    automaton = reader2.GetString(2);
        //                    permission = reader2.GetString(3);
        //                    meaning = reader2.GetString(4);
        //                    certificate = reader2.GetString(5);
        //                    token = reader2.GetString(6);
        //                    power = reader2.GetString(7);
        //                }
        //                reader2.Close();
        //                if (b == 2) { automatonSerial = collector.Name; collector.Name = ""; }
        //                // привязка значений параметров в SQL-запрос вставки данных
        //                command.Parameters["@Name"].Value = collector.Name;
        //                command.Parameters["@Gun"].Value = gun;
        //                command.Parameters["@Automaton_serial"].Value = automatonSerial;
        //                command.Parameters["@Automaton"].Value = automaton;
        //                command.Parameters["@Permission"].Value = permission;
        //                command.Parameters["@Meaning"].Value = gun;
        //                command.Parameters["@Certificate"].Value = automatonSerial;
        //                command.Parameters["@Token"].Value = automaton;
        //                command.Parameters["@Power"].Value = permission;


        //                // выполнение SQL-запроса вставки данных
        //                command.ExecuteNonQuery();

        //            }
        //        }

        //        // закрытие книги Excel
        //        workbook.Close(false);

        //        // закрытие приложения Excel
        //        excel.Quit();
        //    }

        //}

        private void SearchButton_Click(object sender, TextChangedEventArgs e)
        {
            try
            {
                // строка подключения к базе данных SQLite
                string connectionString = @"Data Source=Uspeh.db;Version=3;";
                string searchTerm = Name.Text;
                List<Collector2> collectors = new List<Collector2>();

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand($"SELECT * FROM collectors2 WHERE name LIKE '%{searchTerm}%'", connection);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Collector2 collector = new Collector2
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString(),
                            Gun = reader["gun"].ToString(),
                            Automaton_serial = reader["automaton_serial"].ToString(),
                            Automaton = reader["automaton"].ToString(),
                            Permission = reader["permission"].ToString(),
                            Meaning = reader["meaning"].ToString(),
                            Certificate = reader["certificate"].ToString(),
                            Token = reader["token"].ToString(),
                            Power = reader["power"].ToString(),
                            Armor = reader["armor"].ToString()
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

        private void SearchButton_Click2(object sender, TextChangedEventArgs e)
        {         
            try
            {
                // строка подключения к базе данных SQLite
                string connectionString = @"Data Source=Uspeh.db;Version=3;";
                string searchTerm = Automaton.Text;
                List<Collector2> collectors = new List<Collector2>();

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        // Первый запрос на поиск записей в таблице "collectors" по полю "Name"
                        var command1 = new SQLiteCommand($"SELECT * FROM collectors WHERE Name LIKE '%{searchTerm}%'", connection);
                        var reader1 = command1.ExecuteReader();

                        while (reader1.Read())
                        {
                            // Получаем значение поля "Automaton" из найденной записи
                            string automatonValue = reader1["Automaton"].ToString();
                            string automaton_serialValue = reader1["Automaton_serial"].ToString();

                            // Второй запрос на поиск записей в таблице "collectors2" по полю "Automaton"
                            var command2 = new SQLiteCommand($"SELECT * FROM collectors2 WHERE Automaton LIKE '%{automatonValue}%'", connection);
                            var reader2 = command2.ExecuteReader();
                            Automaton2.Text=automatonValue;
                            Automaton_serial.Text = automaton_serialValue;
                            while (reader2.Read())
                            {
                                int a = 0;
                                Collector2 collector = new Collector2
                                {
                                    Id = Convert.ToInt32(reader2["id"]),
                                    Name = reader2["name"].ToString(),
                                    Gun = reader2["gun"].ToString(),
                                    Automaton_serial = reader2["automaton_serial"].ToString(),
                                    Automaton = reader2["automaton"].ToString(),
                                    Permission = reader2["permission"].ToString(),
                                    Meaning = reader2["meaning"].ToString(),
                                    Certificate = reader2["certificate"].ToString(),
                                    Token = reader2["token"].ToString(),
                                    Power = reader2["power"].ToString(),
                                    Armor = reader2["armor"].ToString()
                                };  
                                
                                collectors.Add(collector);
                              
                            }
                          
                        }
                        if (collectors.Count == 0)
                        {
                            MessageBox.Show("Автомат не пересекается");
                        }
                        else
                        {
                            MessageBox.Show("Автомат пересекается");
                        }

                    }
                    else
                    {
                        // Если значение поля "Automaton" пустое, выводим все записи из таблицы "collectors2"
                        var command = new SQLiteCommand("SELECT * FROM collectors2", connection);
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            Collector2 collector = new Collector2
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Gun = reader["gun"].ToString(),
                                Automaton_serial = reader["automaton_serial"].ToString(),
                                Automaton = reader["automaton"].ToString(),
                                Permission = reader["permission"].ToString(),
                                Meaning = reader["meaning"].ToString(),
                                Certificate = reader["certificate"].ToString(),
                                Token = reader["token"].ToString(),
                                Power = reader["power"].ToString(),
                                Armor = reader["armor"].ToString()
                            };
                            collectors.Add(collector);
                        }
                    }
                }

                dGrid.ItemsSource = collectors;
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
                        if (item is Collector2 selectedCollector)
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
                        if (item is Collector2 selectedCollector)
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

        private void btnWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow2 mainWindow2= new MainWindow2();
            mainWindow2.Show();
           
        }

        public void ClearTable()
        {
            // строка подключения к базе данных Uspeh.db
            string connectionString = "Data Source=Uspeh.db";

            // SQL-запрос для удаления всех записей из таблицы Collectors2
            string query = "DELETE FROM Collectors2";

            // создаем новое подключение к базе данных
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // открываем соединение
                connection.Open();

                // создаем новую команду для выполнения SQL-запроса
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    // выполняем команду
                    command.ExecuteNonQuery();
                }

                // закрываем соединение
                connection.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
          

            UpdateCollector2();
            UpdateCollector();
            Name.Text= string.Empty; Automaton.Text= string.Empty; Automaton2.Text= string.Empty; Automaton_serial.Text= string.Empty;
        }

        private void UpdateCollector2()
        {
            if (dGrid.Items.Count == 0)
            {

                string connectionString = "Data Source=Uspeh.db";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string sql = "UPDATE Collectors2 SET Automaton_serial = @automatonSerial, Automaton = @automaton WHERE Name = @name ORDER BY ID ASC LIMIT 1";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@automatonSerial", Automaton_serial.Text);
                        command.Parameters.AddWithValue("@automaton", Automaton2.Text);
                        command.Parameters.AddWithValue("@name", Automaton.Text);
                        command.ExecuteNonQuery();

                    }
                }
            }
            else
            {
                MessageBox.Show("Автомат пересекается данные не могут быть вставленны");
            }
        }

        private void UpdateCollector()
        {
            if (dGrid.Items.Count == 0)
            {
                string connectionString = "Data Source=Uspeh.db";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "UPDATE Collectors2 SET Automaton_serial = @automatonSerial, Automaton = @automaton WHERE Name = @name ORDER BY ID ASC LIMIT 1 OFFSET 1";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@automatonSerial", Automaton_serial.Text);
                    command.Parameters.AddWithValue("@automaton"," " +Automaton2.Text);
                    command.Parameters.AddWithValue("@name", Automaton.Text);
                    command.ExecuteNonQuery();

                }
            }
            }
            else
            {

            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Name.Text = string.Empty; Automaton.Text = string.Empty; Automaton2.Text = string.Empty; Automaton_serial.Text = string.Empty;

        }
    }
}

