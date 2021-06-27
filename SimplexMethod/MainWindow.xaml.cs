using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace SimplexMethod
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int n = 0;
        int m = 0;
        double[,] tab;
        double[] function;
        Fraction[,] tabF;
        Fraction[] functionF;
        ComboBox extr;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateMatr_Click(object sender, RoutedEventArgs e)
        {
            matr.Children.Clear();
            func.Children.Clear();
            if (!int.TryParse(lim.Text, out m))
            {
                MessageBox.Show("Количество ограничений и переменных должно находится в диапазоне от 1 до 16!\nКоличество ограничений <= количеству переменных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(var.Text, out n))
            {
                MessageBox.Show("Количество ограничений и переменных должно находится в диапазоне от 1 до 16!\nКоличество ограничений <= количеству переменных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!(n > 0 && n < 17 && m > 0 && m < 17 && m <= n))
            {
                MessageBox.Show("Количество ограничений и переменных должно находится в диапазоне от 1 до 16!\nКоличество ограничений <= количества переменных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            cal.IsEnabled = true;
            matr.Rows = m + 1;
            matr.Columns = n + 1;
            func.Columns = n + 1;
            list.Items.Clear();
            for (int i = 0; i < n; i++)
            {
                list.Items.Add("x" + (i + 1));
            }
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    if (i == 0)
                    {
                        Label l = new Label();
                        l.Content = "x" + (j + 1);
                        if (j == n)
                            l.Content = "extr";
                        l.Width = 30;
                        l.Height = 23;
                        Grid.SetRow(l, 0);
                        Grid.SetColumn(l, j);
                        func.Children.Add(l);
                    }
                    else
                    {
                        if (j == n)
                        {
                            extr = new ComboBox();
                            extr.Items.Add("min");
                            extr.Items.Add("max");
                            extr.SelectedIndex = 0;
                            extr.Height = 23;
                            extr.Width = 50;
                            Grid.SetRow(extr, i);
                            Grid.SetColumn(extr, j);
                            func.Children.Add(extr);
                            continue;
                        }
                        TextBox x = new TextBox();
                        x.Width = 30;
                        x.Height = 23;
                        x.Name = "fx" + j;
                        Grid.SetRow(x, i);
                        Grid.SetColumn(x, j);
                        func.Children.Add(x);
                    }
                }
            }
            for (int i = 0; i < m + 1; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    if (i == 0)
                    {
                        Label l = new Label();
                        l.Content = "x" + (j + 1);
                        if (j == n)
                            l.Content = "B";
                        l.Width = 30;
                        l.Height = 23;
                        Grid.SetRow(l, 0);
                        Grid.SetColumn(l, j);
                        matr.Children.Add(l);
                    }
                    else
                    {
                        TextBox x = new TextBox();
                        x.Width = 30;
                        x.Height = 23;
                        x.Name = "x" + i + j;
                        Grid.SetRow(x, i);
                        Grid.SetColumn(x, j);
                        matr.Children.Add(x);
                    }
                }
            }
        }

        private void SimplexRun_Click(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItems.Count != m && start.IsChecked == true)
            {
                MessageBox.Show("Количество базисных переменных должно быть равно количеству ограничений!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (fract.IsChecked == true)
            {
                tabF = new Fraction[m, n + 1];
                functionF = new Fraction[n];
                for (int i = 0; i < n; i++)
                {
                    functionF[i] = new Fraction(0);
                }
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        tabF[i, j] = new Fraction(0);
                    }
                }
            }
            else
            {
                tab = new double[m, n + 1];
                function = new double[n];
                for (int i = 0; i < n; i++)
                {
                    function[i] = 0;
                }
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        tab[i, j] = 0;
                    }
                }
            }
            List<TextBox> cells = matr.Children
            .OfType<TextBox>()
            .OrderBy(Grid.GetRow)
            .ThenBy(Grid.GetColumn)
            .ToList();
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    if (fract.IsChecked == true)
                    {
                        if (Regex.IsMatch(cells[i * (n + 1) + j].Text, "^-?[0-9]+/[0-9]+$") || Regex.IsMatch(cells[i * (n + 1) + j].Text, "^-?[0-9]+$"))
                        {
                            MatchCollection matches = Regex.Matches(cells[i * (n + 1) + j].Text, "-?[0-9]+");
                            if (matches.Count == 1)
                            {
                                int tmp = int.Parse(matches[0].Value);
                                Fraction fr = new Fraction(tmp);
                                tabF[i, j] = fr;
                            }
                            else
                                tabF[i, j] = new Fraction(int.Parse(matches[0].Value), int.Parse(matches[1].Value));
                        }
                        else
                        {
                            MessageBox.Show("Неверные значения коэффицентов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (!Regex.IsMatch(cells[i * (n + 1) + j].Text, "^-?[0-9]+,?[0-9]*$"))
                        {
                            MessageBox.Show("Неверные значения коэффицентов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        tab[i, j] = double.Parse(cells[i * (n + 1) + j].Text);
                    }
                }
            }
            List<TextBox> cells2 = func.Children
            .OfType<TextBox>()
            .OrderBy(Grid.GetRow)
            .ThenBy(Grid.GetColumn)
            .ToList();
            for (int i = 0; i < n; i++)
            {
                if (fract.IsChecked == true)
                {
                    if (Regex.IsMatch(cells2[i].Text, "^-?[0-9]+/[0-9]+$") || Regex.IsMatch(cells2[i].Text, "^-?[0-9]+$"))
                    {
                        MatchCollection matches = Regex.Matches(cells2[i].Text, "-?[0-9]+");
                        if (matches.Count == 1)
                        {
                            functionF[i] = new Fraction(int.Parse(matches[0].Value));
                        }
                        else
                            functionF[i] = new Fraction(int.Parse(matches[0].Value), int.Parse(matches[1].Value));
                    }
                    else
                    {
                        MessageBox.Show("Неверные значения коэффицентов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    if (!Regex.IsMatch(cells2[i].Text, "^-?[0-9]+,?[0-9]*$"))
                    {
                        MessageBox.Show("Неверные значения коэффицентов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    function[i] = double.Parse(cells2[i].Text);
                }
            }
            Calculation calc = new Calculation();
            if (fract.IsChecked == true)
            {
                calc.setupF(tabF, functionF, n, m, extr, start.IsChecked, list, auto.IsChecked);
            }
            else
                calc.setup(tab, function, n, m, extr, start.IsChecked, list, auto.IsChecked);
            calc.Owner = this;
            calc.ShowDialog();
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            //Считывание полей
            if (fract.IsChecked == true)
            {
                tabF = new Fraction[m, n + 1];
                functionF = new Fraction[n];
                for (int i = 0; i < n; i++)
                {
                    functionF[i] = new Fraction(0);
                }
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        tabF[i, j] = new Fraction(0);
                    }
                }
            }
            else
            {
                tab = new double[m, n + 1];
                function = new double[n];
                for (int i = 0; i < n; i++)
                {
                    function[i] = 0;
                }
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        tab[i, j] = 0;
                    }
                }
            }
            try
            {
                List<TextBox> cells = matr.Children
                .OfType<TextBox>()
                .OrderBy(Grid.GetRow)
                .ThenBy(Grid.GetColumn)
                .ToList();
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (cells[i * (n + 1) + j].Text == "")
                        {
                            continue;
                        }
                        if (fract.IsChecked == true)
                        {
                            MatchCollection matches = Regex.Matches(cells[i * (n + 1) + j].Text, "-?[0-9]+");
                            if (matches.Count == 1)
                            {
                                int tmp = int.Parse(matches[0].Value);
                                Fraction fr = new Fraction(tmp);
                                tabF[i, j] = fr;
                            }
                            else
                                tabF[i, j] = new Fraction(int.Parse(matches[0].Value), int.Parse(matches[1].Value));
                        }
                        else
                            tab[i, j] = double.Parse(cells[i * (n + 1) + j].Text);
                    }
                }
                List<TextBox> cells2 = func.Children
                .OfType<TextBox>()
                .OrderBy(Grid.GetRow)
                .ThenBy(Grid.GetColumn)
                .ToList();
                for (int i = 0; i < n; i++)
                {
                    if (cells2[i].Text == "")
                    {
                        continue;
                    }
                    if (fract.IsChecked == true)
                    {
                        MatchCollection matches = Regex.Matches(cells2[i].Text, "-?[0-9]+");
                        if (matches.Count == 1)
                        {
                            functionF[i] = new Fraction(int.Parse(matches[0].Value));
                        }
                        else
                            functionF[i] = new Fraction(int.Parse(matches[0].Value), int.Parse(matches[1].Value));
                    }
                    else
                        function[i] = double.Parse(cells2[i].Text);
                }

                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == true)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        string text = "Количество переменных: " + n + "\n" + "Количество ограничений: " + m + "\n" + "Целевая функции:\n ";
                        if (fract.IsChecked == true)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                if (j == n - 1)
                                {
                                    text = text + functionF[j] + "*" + "x" + (j + 1) + " -> " + extr.Text;
                                }
                                else
                                    text = text + functionF[j] + "*" + "x" + (j + 1) + " + ";
                            }
                            text = text + "\n" + "Ограничения:\n ";
                            for (int i = 0; i < m; i++)
                            {
                                for (int j = 0; j < n; j++)
                                {
                                    if (j == n - 1)
                                    {
                                        text = text + tabF[i, j].ToString() + "*x" + (j + 1) + " = " + tabF[i, j + 1];
                                    }
                                    else
                                        text = text + tabF[i, j].ToString() + "*x" + (j + 1) + " + ";
                                }
                                text = text + "\n ";
                            }
                        }
                        else
                        {
                            for (int j = 0; j < n; j++)
                            {
                                if (j == n - 1)
                                {
                                    text = text + function[j] + "*" + "x" + (j + 1) + " -> " + extr.Text;
                                }
                                else
                                    text = text + function[j] + "*" + "x" + (j + 1) + " + ";
                            }
                            text = text + "\n" + "Ограничения:\n ";
                            for (int i = 0; i < m; i++)
                            {
                                for (int j = 0; j < n; j++)
                                {
                                    if (j == n - 1)
                                    {
                                        text = text + tab[i, j].ToString() + "*x" + (j + 1) + " = " + tab[i, j + 1];
                                    }
                                    else
                                        text = text + tab[i, j].ToString() + "*x" + (j + 1) + " + ";
                                }
                                text = text + "\n ";
                            }
                        }
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(myStream))
                            {
                                sw.WriteLine(text);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        // Code to write the stream goes here.
                        myStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Использовать дроби?", "Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                fract.IsChecked = true;
            }
            var fileContent = string.Empty;
            var filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();

                string extrFile = string.Empty;

                try
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        for (int i = 0; !reader.EndOfStream; i++)
                        {
                            string str = string.Empty;
                            str = reader.ReadLine();
                            if (i == 3)
                            {
                                Match match = Regex.Match(str, @"(min|max)");
                                extrFile = match.Value;
                            }
                            if (fract.IsChecked == true)
                            {
                                MatchCollection matches = Regex.Matches(str, @" -?[0-9]+/?[0-9]*");
                                if (i < 2)
                                {
                                    if (i == 0)
                                    {
                                        n = int.Parse(matches[0].Value);
                                        continue;
                                    }
                                    else
                                    {
                                        m = int.Parse(matches[0].Value);
                                        if (m < 1 || n < 1)
                                        {
                                            MessageBox.Show("Число ограничений и число переменных должно быть больше 1!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                            return;
                                        }
                                        if (m > n)
                                        {
                                            MessageBox.Show("Число ограничений не может быть больше числа переменных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                            return;
                                        }
                                        tabF = new Fraction[m, n + 1];
                                        functionF = new Fraction[n];
                                        continue;
                                    }
                                }
                                for (int j = 0; j < matches.Count; j++)
                                {
                                    if (matches[j].Value.Contains("/"))
                                    {
                                        string[] str2 = matches[j].Value.Split('/');
                                        if (i == 3)
                                            functionF[j] = new Fraction(int.Parse(str2[0]), int.Parse(str2[1]));
                                        else
                                            tabF[i - 5, j] = new Fraction(int.Parse(str2[0]), int.Parse(str2[1]));
                                }
                                    else
                                    {
                                        if (i == 3)
                                            functionF[j] = new Fraction(int.Parse(matches[j].Value));
                                        else
                                            tabF[i - 5, j] = new Fraction(int.Parse(matches[j].Value));
                                    }

                                }
                            }
                            else
                            {
                                MatchCollection matches = Regex.Matches(str, @" -?[0-9]+,?[0-9]*");
                                if (i < 2)
                                {
                                    if (i == 0)
                                    {
                                        n = int.Parse(matches[0].Value);
                                        continue;
                                    }
                                    else
                                    {
                                        m = int.Parse(matches[0].Value);
                                        if (m < 1 || n < 1)
                                        {
                                            MessageBox.Show("Число ограничений и число переменных должно быть больше 1!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                            return;
                                        }
                                        if (m > n)
                                        {
                                            MessageBox.Show("Число ограничений не может быть больше числа переменных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                            return;
                                        }
                                        tab = new double[m, n + 1];
                                        function = new double[n];
                                        continue;
                                    }
                                }
                                for (int j = 0; j < matches.Count; j++)
                                {
                                    if (i == 3)
                                        function[j] = double.Parse(matches[j].Value);
                                    else
                                        tab[i - 5, j] = double.Parse(matches[j].Value);
                                }
                            }
                        }
                    }
                    cal.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                list.Items.Clear();
                for (int i = 0; i < n; i++)
                {
                    list.Items.Add("x" + (i + 1));
                }
                var.Text = n.ToString();
                lim.Text = m.ToString();
                matr.Children.Clear();
                func.Children.Clear();
                matr.Rows = m + 1;
                matr.Columns = n + 1;
                func.Columns = n + 1;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (i == 0)
                        {
                            Label l = new Label();
                            l.Content = "x" + (j + 1);
                            if (j == n)
                                l.Content = "extr";
                            l.Width = 30;
                            l.Height = 23;
                            Grid.SetRow(l, 0);
                            Grid.SetColumn(l, j);
                            func.Children.Add(l);
                        }
                        else
                        {
                            if (j == n)
                            {
                                extr = new ComboBox();
                                extr.Items.Add("min");
                                extr.Items.Add("max");
                                extr.Height = 23;
                                extr.Width = 50;
                                if (extrFile == "min")
                                {
                                    extr.SelectedIndex = 0;
                                }
                                else
                                {
                                    extr.SelectedIndex = 1;
                                }
                                Grid.SetRow(extr, i);
                                Grid.SetColumn(extr, j);
                                func.Children.Add(extr);
                                continue;
                            }
                            TextBox x = new TextBox();
                            x.Width = 30;
                            x.Height = 23;
                            x.Name = "fx" + j;
                            if (fract.IsChecked == true)
                                x.Text = functionF[j].ToString();
                            else
                                x.Text = function[j].ToString();
                            Grid.SetRow(x, i);
                            Grid.SetColumn(x, j);
                            func.Children.Add(x);
                        }
                    }
                }
                for (int i = 0; i < m + 1; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (i == 0)
                        {
                            Label l = new Label();
                            l.Content = "x" + (j + 1);
                            if (j == n)
                                l.Content = "B";
                            l.Width = 30;
                            l.Height = 23;
                            Grid.SetRow(l, 0);
                            Grid.SetColumn(l, j);
                            matr.Children.Add(l);
                        }
                        else
                        {
                            TextBox x = new TextBox();
                            x.Width = 30;
                            x.Height = 23;
                            x.Name = "x" + j + i;
                            if (fract.IsChecked == true)
                                x.Text = tabF[i - 1, j].ToString();
                            else
                                x.Text = tab[i - 1, j].ToString();
                            Grid.SetRow(x, i);
                            Grid.SetColumn(x, j);
                            matr.Children.Add(x);
                        }
                    }
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }
    }
}
