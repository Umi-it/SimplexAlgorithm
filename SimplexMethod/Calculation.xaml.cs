using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace SimplexMethod
{
    /// <summary>
    /// Логика взаимодействия для Calculation.xaml
    /// </summary>
    public partial class Calculation : Window
    {
        string extr; //Экстремум
        double[,] tab; //Ограничения
        double[] function; //Целевая функция
        double[,] matr; //Коэфиценты в симплекс таблице
        string[,] str; //X в симплекс таблице
        Fraction[,] tabF;
        Fraction[] functionF;
        Fraction[,] matrF;
        int n, m; //Размерность исходников
        int k = 0; //Счетчик для кнопки далее (1-4 шаг)
        bool fract; //Проверка на чекбокс дробей
        int iS = -1;//Индекс опорного для текущего шага
        int jS = -1; //Индекс опорного для текущего шага
        int ch = 0; //Проверка на отсутствие опорных элементов
        bool checkRez = false; //Открывает доступ к симплекс методу (последнему шагу)
        //Конечная симплекс таблица без искусственных базисов
        string[,] strRez;
        double[,] matrRez;
        Fraction[,] matrRezF; //Для дробей
        //Ее размерность
        int nRez;
        int mRez;
        bool end = false; //Проверка на окончание симплекс метода
        bool start; //Проверка на заданный базис
        //Симплекс метод для заданных базисов
        ListBox list; //Список базисных переменных
        double[,] matrS;
        Fraction[,] matrFS;
        bool auto = false; //Проверка на автоматический режим
        //История изменений матрицы
        Stack<Fraction[,]> stackF;
        Stack<double[,]> stack;
        Stack<string[,]> stackS;
        int t = -1; //Счетчик для кнопки далее (5-7 шаг)
        bool endPrev = false; //Проверка на конец для кнопки prev
        int s = -1; //Счетчик для симплекс метода

        public Calculation()
        {
            InitializeComponent();
        }

        //Инициализация переменных
        public void setup(double[,] tab1, double[] function1, int n1, int m1, ComboBox extr1, bool? start1, ListBox list1, bool? auto1)
        {
            stack = new Stack<double[,]>();
            stackS = new Stack<string[,]>();
            tab = tab1;
            function = function1;
            n = n1;
            m = m1;
            extr = extr1.Text;
            fract = false;
            if (auto1 == true)
                auto = true;
            str = new string[m + 1, n + 1];
            str[0, 0] = "`x(0)";
            for (int j = 1; j < n + 1; j++)
            {
                str[0, j] = "x" + j;
            }
            for (int i = 1; i < m + 1; i++)
            {
                str[i, 0] = "x" + (n + i);
            }
            if (start1 == true)
            {
                list = list1;
                s = 0;
                start = true;
                if (auto)
                {
                    autoCalcS();
                    return;
                }
                simplex();
                return;
            }
            if (auto)
            {
                autoCalc();
                return;
            }
            oneStep();
        }

        public void setupF(Fraction[,] tab1, Fraction[] function1, int n1, int m1, ComboBox extr1, bool? start1, ListBox list1, bool? auto1)
        {
            stackF = new Stack<Fraction[,]>();
            stackS = new Stack<string[,]>();
            tabF = tab1;
            functionF = function1;
            n = n1;
            m = m1;
            extr = extr1.Text;
            fract = true;
            if (auto1 == true)
                auto = true;
            str = new string[m + 1, n + 1];
            str[0, 0] = "`x(0)";
            for (int j = 1; j < n + 1; j++)
            {
                str[0, j] = "x" + j;
            }
            for (int i = 1; i < m + 1; i++)
            {
                str[i, 0] = "x" + (n + i);
            }
            if (start1 == true)
            {
                list = list1;
                s = 0;
                start = true;
                if (auto)
                {
                    autoCalcS();
                    return;
                }
                simplex();
                return;
            }
            if (auto)
            {
                autoCalc();
                return;
            }
            oneStep();
        }

        //Приведение к кан.в., создание матрицы и ее отрисовка
        public void simplex()
        {
            //Проверка на неотрицательные свободные члены
            if (fract == true)
            {
                for (int i = 0; i < m; i++)
                {
                    if (tabF[i, n] < 0)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            tabF[i, j] = tabF[i, j] * -1;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < m; i++)
                {
                    if (tab[i, n] < 0)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            tab[i, j] = tab[i, j] * -1;
                        }
                    }
                }
            }

            //Приведение целевой функции к каноническому виду
            string newExtr = extr;
            if (extr == "max")
            {
                if (fract)
                {
                    for (int i = 0; i < n; i++)
                    {
                        functionF[i] = functionF[i] * -1;
                    }
                    newExtr = "min";
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        function[i] = function[i] * -1;
                    }
                    newExtr = "min";
                }
            }

            //Создание матрицы
            if (fract)
            {
                matrFS = new Fraction[m, n + 1];
                for (int k = 0, l = 0; k < list.SelectedItems.Count; k++)
                {
                    int tmp = int.Parse(Regex.Match(list.SelectedItems[k].ToString(), "[0-9]+").Value);
                    for (int j = 0; j < n; j++)
                    {
                        if (j + 1 == tmp)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                matrFS[i, l] = tabF[i, j];
                            }
                            l++;
                            break;
                        }
                    }
                }
                for (int j = 0, l = list.SelectedItems.Count; j < n + 1; j++)
                {
                    if (list.SelectedItems.Contains("x" + (j + 1)))
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrFS[i, l] = tabF[i, j];
                    }
                    l++;
                }
            }
            else
            {
                matrS = new double[m, n + 1];
                for (int k = 0, l = 0; k < list.SelectedItems.Count; k++)
                {
                    int tmp = int.Parse(Regex.Match(list.SelectedItems[k].ToString(), "[0-9]+").Value);
                    for (int j = 0; j < n; j++)
                    {
                        if (j + 1 == tmp)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                matrS[i, l] = tab[i, j];
                            }
                            l++;
                            break;
                        }
                    }
                }
                for (int j = 0, l = list.SelectedItems.Count; j < n + 1; j++)
                {
                    if (list.SelectedItems.Contains("x" + (j + 1)))
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrS[i, l] = tab[i, j];
                    }
                    l++;
                }
            }

            //Отрисовка матрицы
            img1.Visibility = Visibility.Visible;
            img2.Visibility = Visibility.Visible;
            grid.Children.Clear();

            grid.Rows = m + 1;
            grid.Columns = n + 1;
            for (int i = 0; i < list.SelectedItems.Count; i++)
            {
                Label lab = new Label();
                lab.Width = 35;
                lab.Height = 23;
                lab.Content = list.SelectedItems[i];
                Grid.SetRow(lab, 0);
                Grid.SetColumn(lab, i);
                grid.Children.Add(lab);
            }
            for (int i = 0; i < n; i++)
            {
                if (list.SelectedItems.Contains("x" + (i + 1)))
                    continue;
                Label lab = new Label();
                lab.Width = 35;
                lab.Height = 23;
                lab.Content = "x" + (i + 1);
                Grid.SetRow(lab, 0);
                Grid.SetColumn(lab, i);
                grid.Children.Add(lab);
            }
            Label lab2 = new Label();
            lab2.Width = 35;
            lab2.Height = 23;
            lab2.Content = " ";
            Grid.SetRow(lab2, 0);
            Grid.SetColumn(lab2, n);
            grid.Children.Add(lab2);
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    Label lmatr = new Label();
                    if (fract)
                        lmatr.Content = matrFS[i, j];
                    else
                        lmatr.Content = matrS[i, j];
                    lmatr.Name = "a" + i + "b" + j;
                    lmatr.MaxWidth = 50;
                    lmatr.Height = 23;
                    Grid.SetRow(lmatr, i + 1);
                    Grid.SetColumn(lmatr, j);
                    grid.Children.Add(lmatr);
                }
            }

        }

        //Решение матрицы методом Гаусса, ее отрисовка, и результативная матрица
        public void gaus()
        {
            bool check = false;
            if (fract)
            {
                for (int k = 0; k < m; k++)
                {
                    if (matrFS[k, k] == 0)
                    {
                        for (int i = k + 1; i < m; i++)
                        {
                            if (matrFS[i, 0] != 0)
                            {
                                check = true;
                                for (int j = 0; j < n + 1; j++)
                                {
                                    Fraction tmp = matrFS[k, j];
                                    matrFS[k, j] = matrFS[k + 1, j];
                                    matrFS[k + 1, j] = tmp;
                                }
                                break;
                            }
                        }
                        if (!check)
                        {
                            MessageBox.Show("Ошибка в выражении базисов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    for (int j = k + 1; j < n + 1; j++)
                    {
                        matrFS[k, j] /= matrFS[k, k];
                        matrFS[k, j].Reduce();
                    }
                    matrFS[k, k] = new Fraction(1);
                    for (int i = k + 1; i < m; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrFS[i, j] -= matrFS[k, j] * matrFS[i, k];
                            matrFS[i, j].Reduce();
                        }
                        matrFS[i, k] = new Fraction(0);
                    }
                }
                for (int k = 1; k < m; k++)
                {
                    for (int i = 0; i < k; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrFS[i, j] -= matrFS[k, j] * matrFS[i, k];
                            matrFS[i, j].Reduce();
                        }
                        matrFS[i, k] = new Fraction(0);
                    }
                }
            }
            else
            {
                for (int k = 0; k < m; k++)
                {
                    if (matrS[k, k] == 0)
                    {
                        for (int i = k + 1; i < m; i++)
                        {
                            if (matrS[i, 0] != 0)
                            {
                                check = true;
                                for (int j = 0; j < n + 1; j++)
                                {
                                    double tmp = matrS[k, j];
                                    matrS[k, j] = matrS[k + 1, j];
                                    matrS[k + 1, j] = tmp;
                                }
                                break;
                            }
                        }
                        if (!check)
                        {
                            MessageBox.Show("Ошибка в выражении базисов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    for (int j = k + 1; j < n + 1; j++)
                    {
                        matrS[k, j] /= matrS[k, k];
                        if (Math.Abs(matrS[k, j] - Math.Round(matrS[k, j])) < 0.0000001)
                            matrS[k, j] = Math.Round(matrS[k, j]);
                    }
                    matrS[k, k] = 1;
                    for (int i = k + 1; i < m; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrS[i, j] -= matrS[k, j] * matrS[i, k];
                            if (Math.Abs(matrS[i, j] - Math.Round(matrS[i, j])) < 0.0000001)
                                matrS[i, j] = Math.Round(matrS[i, j]);
                        }
                        matrS[i, k] = 0;
                    }
                }
                for (int k = 1; k < m; k++)
                {
                    for (int i = 0; i < k; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrS[i, j] -= matrS[k, j] * matrS[i, k];
                            if (Math.Abs(matrS[i, j] - Math.Round(matrS[i, j])) < 0.0000001)
                                matrS[i, j] = Math.Round(matrS[i, j]);
                        }
                        matrS[i, k] = 0;
                    }
                }
            }

            //Отрисовка матрицы
            img1.Visibility = Visibility.Visible;
            img2.Visibility = Visibility.Visible;
            grid.Children.Clear();

            grid.Rows = m + 1;
            grid.Columns = n + 1;
            for (int i = 0; i < list.SelectedItems.Count; i++)
            {
                Label lab = new Label();
                lab.Width = 35;
                lab.Height = 23;
                lab.Content = list.SelectedItems[i];
                Grid.SetRow(lab, 0);
                Grid.SetColumn(lab, i);
                grid.Children.Add(lab);
            }
            for (int i = 0; i < n; i++)
            {
                if (list.SelectedItems.Contains("x" + (i + 1)))
                    continue;
                Label lab = new Label();
                lab.Width = 35;
                lab.Height = 23;
                lab.Content = "x" + (i + 1);
                Grid.SetRow(lab, 0);
                Grid.SetColumn(lab, i);
                grid.Children.Add(lab);
            }
            Label lab2 = new Label();
            lab2.Width = 35;
            lab2.Height = 23;
            lab2.Content = " ";
            Grid.SetRow(lab2, 0);
            Grid.SetColumn(lab2, n);
            grid.Children.Add(lab2);
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    Label lmatr = new Label();
                    if (fract)
                        lmatr.Content = matrFS[i, j];
                    else
                        lmatr.Content = matrS[i, j];
                    lmatr.Name = "a" + i + "b" + j;
                    lmatr.MaxWidth = 50;
                    lmatr.Height = 23;
                    Grid.SetRow(lmatr, i + 1);
                    Grid.SetColumn(lmatr, j);
                    grid.Children.Add(lmatr);
                }
            }

            if (fract)
            {
                nRez = n + 1 - list.SelectedItems.Count;
                mRez = m + 1;
                matrRezF = new Fraction[m + 1, n + 1 - m];
                strRez = new string[m + 1, n + 1 - m];
                int top;
                Fraction rez = new Fraction(0);
                for (int j = list.SelectedItems.Count, l = 0; j < n; j++)
                {
                    top = int.Parse(Regex.Match(((Label)grid.Children[j]).Content.ToString(), "[0-9]+").Value) - 1;
                    for (int i = 0; i < m; i++)
                    {
                        rez += -1 * matrFS[i, j] * functionF[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                    }
                    rez += functionF[top];
                    matrRezF[m, l] = rez;
                    rez = new Fraction(0);
                    l++;
                }
                for (int i = 0; i < m; i++)
                {
                    rez += matrFS[i, n] * functionF[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                }
                matrRezF[m, n - m] = rez * -1;


                for (int j = 0, l = 0; j < n + 1; j++)
                {
                    if (j < list.SelectedItems.Count)
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrRezF[i, l] = matrFS[i, j];
                    }
                    l++;
                }
            }
            else
            {
                nRez = n + 1 - list.SelectedItems.Count;
                mRez = m + 1;
                matrRez = new double[m + 1, n + 1 - m];
                strRez = new string[m + 1, n + 1 - m];
                int top;
                double rez = 0;
                for (int j = list.SelectedItems.Count, l = 0; j < n; j++)
                {
                    top = int.Parse(Regex.Match(((Label)grid.Children[j]).Content.ToString(), "[0-9]+").Value) - 1;
                    for (int i = 0; i < m; i++)
                    {
                        rez += -1 * matrS[i, j] * function[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                    }
                    rez += function[top];
                    matrRez[m, l] = rez;
                    rez = 0;
                    l++;
                }
                for (int i = 0; i < m; i++)
                {
                    rez += matrS[i, n] * function[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                }
                matrRez[m, n - m] = rez * -1;


                for (int j = 0, l = 0; j < n + 1; j++)
                {
                    if (j < list.SelectedItems.Count)
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrRez[i, l] = matrS[i, j];
                    }
                    l++;
                }
            }

            strRez[0, 0] = "x(0)";
            for (int i = 0; i < list.SelectedItems.Count; i++)
            {
                strRez[i + 1, 0] = list.SelectedItems[i].ToString();
            }
            for (int j = 0; j < nRez - 1; j++)
            {
                strRez[0, j + 1] = ((Label)grid.Children[list.SelectedItems.Count + j]).Content.ToString();
            }

            start = true;
            t = 1;
        }

        //Приведение к каноническому виду
        public void oneStep()
        {
            /* Шаг 1 */

            //Проверка на неотрицательные свободные члены
            if (fract == true)
            {
                for (int i = 0; i < m; i++)
                {
                    if (tabF[i, n] < 0)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            tabF[i, j] = tabF[i, j] * -1;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < m; i++)
                {
                    if (tab[i, n] < 0)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            tab[i, j] = tab[i, j] * -1;
                        }
                    }
                }
            }
            //Приведение целевой функции к каноническому виду
            string newExtr = extr;
            if (extr == "max")
            {
                if (fract)
                {
                    for (int i = 0; i < n; i++)
                    {
                        functionF[i] = functionF[i] * -1;
                    }
                    newExtr = "min";
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        function[i] = function[i] * -1;
                    }
                    newExtr = "min";
                }
            }

            string text = "Целевая функции:\n ";
            if (fract)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == n - 1)
                    {
                        text = text + functionF[j] + "*" + "x" + (j + 1) + " -> " + newExtr;
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
                @out.Text = text;
            }
            else
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == n - 1)
                    {
                        text = text + function[j] + "*" + "x" + (j + 1) + " -> " + newExtr;
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
                @out.Text = text;
            }
        }

        //Инициализация симплекс таблицы и добавление искусственного базиса
        public void twoStep()
        {
            /* Шаг 2 */
            //Добавление искусственного базиса

            string text = "`f(`x) = " + "x" + (n + 1);

            for (int i = 2; i < m + 1; i++)
            {
                text = text + " + " + "x" + (n + i);
            }
            text = text + " -> min \n\n";

            int count = 1;
            if (fract)
            {
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (j == n - 1)
                        {
                            text = text + tabF[i, j].ToString() + "*x" + (j + 1);
                            text = text + " + " + "1*x" + (n + count);
                            count++;
                            text = text + " = " + tabF[i, j + 1];
                        }
                        else
                            text = text + tabF[i, j].ToString() + "*x" + (j + 1) + " + ";
                    }
                    text = text + "\n ";
                }
                text += "\n`x(0) = (";
                for (int i = 0; i < n; i++)
                {
                    text += "0, ";
                }
                for (int i = 0; i < m; i++)
                {
                    if (i == m - 1)
                        text += tabF[i, n] + ")";
                    else
                        text += tabF[i, n] + ", ";
                }
                @out.Text = text;
            }
            else
            {
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (j == n - 1)
                        {
                            text = text + tab[i, j].ToString() + "*x" + (j + 1);
                            text = text + " + " + "1*x" + (n + count);
                            count++;
                            text = text + " = " + tab[i, j + 1];
                        }
                        else
                            text = text + tab[i, j].ToString() + "*x" + (j + 1) + " + ";
                    }
                    text = text + "\n ";
                }
                text += "\n`x(0) = (";
                for (int i = 0; i < n; i++)
                {
                    text += "0, ";
                }
                for (int i = 0; i < m; i++)
                {
                    if (i == m - 1)
                        text += tab[i, n] + ")";
                    else
                        text += tab[i, n] + ", ";
                }
                @out.Text = text;
            }

            //Инициализация симплекс таблицы
            if (fract)
            {
                matrF = new Fraction[m + 1, n + 1];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matrF[i, j] = tabF[i, j];
                        if (i == m - 1)
                        {
                            matrF[i + 1, j] = new Fraction(0);
                            for (int k = 0; k < m; k++)
                            {
                                matrF[i + 1, j] += tabF[k, j];
                            }
                            matrF[i + 1, j] = matrF[i + 1, j] * -1;
                        }
                    }
                }
            }
            else
            {
                matr = new double[m + 1, n + 1];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matr[i, j] = tab[i, j];
                        if (i == m - 1)
                        {
                            matr[i + 1, j] = 0;
                            for (int k = 0; k < m; k++)
                            {
                                matr[i + 1, j] += tab[k, j];
                            }
                            matr[i + 1, j] = matr[i + 1, j] * -1;
                        }
                    }
                }
            }
        }

        //Отрисовка таблицы с ИБ, поиск опорных, проверка на результат
        public void threeStep()
        {
            @out.Text = "";
            iS = -1;
            jS = -1;
            //Отображение симплекс таблицы

            grid.Rows = m + 2;
            grid.Columns = n + 2;
            for (int i = 0; i < n + 1; i++)
            {
                Label lab = new Label();
                lab.Width = 35;
                lab.Height = 23;
                lab.Content = str[0, i];
                Grid.SetRow(lab, 0);
                Grid.SetColumn(lab, i);
                grid.Children.Add(lab);
            }
            Label lab2 = new Label();
            lab2.Width = 35;
            lab2.Height = 23;
            lab2.Content = " ";
            Grid.SetRow(lab2, 0);
            Grid.SetColumn(lab2, n + 1);
            grid.Children.Add(lab2);
            for (int i = 0; i < m + 1; i++)
            {
                Label l = new Label();
                if (i == m)
                {
                    l.Content = " ";
                }
                else
                    l.Content = str[i + 1, 0];
                l.Width = 35;
                l.Height = 23;
                Grid.SetRow(l, i);
                Grid.SetColumn(l, 0);
                grid.Children.Add(l);
                for (int j = 0; j < n + 1; j++)
                {
                    Label lmatr = new Label();
                    if (fract)
                        lmatr.Content = matrF[i, j];
                    else
                        lmatr.Content = matr[i, j];
                    lmatr.Name = "a" + i + "b" + j;
                    lmatr.MaxWidth = 50;
                    lmatr.Height = 23;
                    Grid.SetRow(lmatr, i + 1);
                    Grid.SetColumn(lmatr, j + 1);
                    grid.Children.Add(lmatr);
                }
            }

            //Поиск опорного элемента
            List<Label> cells = grid.Children.OfType<Label>().OrderBy(Grid.GetRow).ThenBy(Grid.GetColumn).ToList();
            bool[,] sup = new bool[m, n];
            ch = 0;
            if (fract)
            {
                for (int j = 0; j < n; j++)
                {
                    if (matrF[m, j] < 0)
                    {
                        Fraction min = null;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        for (int i = 0; i < m; i++)
                        {
                            if (matrF[i, j] > 0)
                            {
                                if (min == null)
                                {
                                    min = matrF[i, n] / matrF[i, j];
                                    count = 0;
                                    i1 = i;
                                    j1 = j;
                                    continue;
                                }
                                if (matrF[i, n] / matrF[i, j] < min)
                                {
                                    min = matrF[i, n] / matrF[i, j];
                                    count = 0;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matrF[i, n] / matrF[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 0)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                if (matrF[i, n] / matrF[i, j] == min)
                                {
                                    sup[i, j] = true;
                                    ch++;
                                }
                            }
                        }
                        else
                        {
                            sup[i1, j1] = true;
                            ch++;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < n; j++)
                {
                    if (matr[m, j] < 0)
                    {
                        double min = int.MaxValue;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        for (int i = 0; i < m; i++)
                        {
                            if (matr[i, j] > 0)
                            {
                                if (matr[i, n] / matr[i, j] < min)
                                {
                                    min = matr[i, n] / matr[i, j];
                                    count = 0;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matr[i, n] / matr[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 0)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                if (matr[i, n] / matr[i, j] == min)
                                {
                                    sup[i, j] = true;
                                    ch++;
                                }
                            }
                        }
                        else
                        {
                            sup[i1, j1] = true;
                            ch++;
                        }
                    }
                }
            }
            if (ch == 1)
            {
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (sup[i, j])
                        {
                            cells[(i + 1) * (n + 2) + (j + 2)].Background = new SolidColorBrush(Colors.LightGreen);
                            iS = i;
                            jS = j;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (sup[i, j])
                        {
                            cells[(i + 1) * (n + 2) + (j + 2)].Background = new SolidColorBrush(Colors.LightGreen);
                            cells[(i + 1) * (n + 2) + (j + 2)].MouseDoubleClick += call;
                        }
                    }
                }
            }

            //Проверка на результат
            bool che = false;
            if (fract)
            {
                for (int i = 0; i < n; i++)
                {
                    if (int.Parse(Regex.Match(str[0, i + 1], "[0-9]+").Value) <= n)
                    {
                        if (matrF[m, i] != 0)
                        {
                            che = true;
                        }
                    }
                }
                if (che == false)
                {
                    if (matrF[m, n] > 0)
                        MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (matrF[m, n] < 0)
                        MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (matrF[m, n] == 0)
                    {
                        t = 0;
                    }
                    return;
                }
                if (ch == 0 && che == true && (-1 * matrF[m, n]) > 0)
                {
                    MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (ch == 0 && che == true && (-1 * matrF[m, n]) < 0)
                {
                    MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                //Проверка на результат
                for (int i = 0; i < n; i++)
                {
                    if (int.Parse(Regex.Match(str[0, i + 1], "[0-9]+").Value) <= n)
                    {
                        if (matr[m, i] != 0)
                        {
                            che = true;
                        }
                    }
                }
                if (che == false)
                {
                    if (matr[m, n] > 0)
                        MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (matr[m, n] < 0)
                        MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (matr[m, n] == 0)
                    {
                        t = 0;
                    }
                    return;
                }
                if (ch == 0 && che == true && (-1 * matr[m, n]) > 0)
                {
                    MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (ch == 0 && che == true && (-1 * matr[m, n]) < 0)
                {
                    MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
        }

        //Выбор опорного элемента
        private void call(object sender, EventArgs e)
        {
            Label lab = (Label)sender;
            sup.Content = "Опорный элемент выбран.";
            MatchCollection matches = Regex.Matches(lab.Name, "[0-9]+");
            iS = int.Parse(matches[0].Value);
            jS = int.Parse(matches[1].Value);
        }

        //Вычисление симплекс таблицы с исскуственными базисами
        public void fourStep()
        {
            sup.Content = "";
            if (fract)
            {
                Fraction[,] matrOld = new Fraction[m + 1, n + 1];
                for (int i = 0; i < m + 1; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matrOld[i, j] = matrF[i, j];
                    }
                }
                //Преобразование симплекс таблицы
                string tmp = str[0, jS + 1];
                str[0, jS + 1] = str[iS + 1, 0];
                str[iS + 1, 0] = tmp;

                for (int j = 0; j < n + 1; j++)
                {
                    if (j == jS)
                        continue;
                    matrF[iS, j] /= matrF[iS, jS];
                    matrF[iS, j].Reduce();
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    matrF[i, jS] /= (-1 * matrF[iS, jS]);
                    matrF[i, jS].Reduce();
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (j == jS)
                            continue;
                        matrF[i, j] -= matrOld[i, jS] * matrF[iS, j];
                        matrF[i, j].Reduce();
                    }
                }
                matrF[iS, jS] = 1 / matrF[iS, jS];
                matrF[iS, jS].Reduce();
            }
            else
            {
                double[,] matrOld = new double[m + 1, n + 1];
                for (int i = 0; i < m + 1; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matrOld[i, j] = matr[i, j];
                    }
                }
                //Преобразование симплекс таблицы
                string tmp = str[0, jS + 1];
                str[0, jS + 1] = str[iS + 1, 0];
                str[iS + 1, 0] = tmp;

                for (int j = 0; j < n + 1; j++)
                {
                    if (j == jS)
                        continue;
                    matr[iS, j] /= matr[iS, jS];
                    if (Math.Abs(matr[iS, j] - Math.Round(matr[iS, j])) < 0.0000001)
                        matr[iS, j] = Math.Round(matr[iS, j]);
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    matr[i, jS] /= (-1 * matr[iS, jS]);
                    if (Math.Abs(matr[i, jS] - Math.Round(matr[i, jS])) < 0.0000001)
                        matr[i, jS] = Math.Round(matr[i, jS]);
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (j == jS)
                            continue;
                        matr[i, j] -= matrOld[i, jS] * matr[iS, j];
                        if (Math.Abs(matr[i, j] - Math.Round(matr[i, j])) < 0.0000001)
                            matr[i, j] = Math.Round(matr[i, j]);
                    }
                }
                matr[iS, jS] = 1 / matr[iS, jS];
                if (Math.Abs(matr[iS, jS] - Math.Round(matr[iS, jS])) < 0.0000001)
                    matr[iS, jS] = Math.Round(matr[iS, jS]);
            }
            grid.Children.Clear();
            iS = -1;
            jS = -1;
            threeStep();
        }

        //Результативная матрица и ее отрисовка, подсчет целевой функции
        public void fiveStep()
        {
            int count = 0;
            //Вывод конечной симплекс таблицы без искусственного базиса
            if (fract)
            {
                for (int j = 0; j < n; j++)
                {
                    if (matrF[m, j] == 0)
                    {
                        count++;
                    }
                }
            }
            else
            {
                for (int j = 0; j < n; j++)
                {
                    if (matr[m, j] == 0)
                    {
                        count++;
                    }
                }
            }

            grid.Children.Clear();
            grid.Rows = m + 2;
            grid.Columns = count + 2;
            int col = 0;
            for (int i = 0; i < n + 1; i++)
            {
                if (i != 0)
                {
                    if (fract)
                    {
                        if (matrF[m, i - 1] == 1)
                        {
                            col++;
                            continue;
                        }
                    }
                    else
                    {
                        if (matr[m, i - 1] == 1)
                        {
                            col++;
                            continue;
                        }
                    }
                }
                Label lab = new Label();
                lab.Width = 35;
                lab.Height = 23;
                lab.Content = str[0, i];
                Grid.SetRow(lab, 0);
                Grid.SetColumn(lab, i - col);
                grid.Children.Add(lab);
            }
            Label lab2 = new Label();
            lab2.Width = 35;
            lab2.Height = 23;
            lab2.Content = " ";
            Grid.SetRow(lab2, 0);
            Grid.SetColumn(lab2, n + 1 - col);
            grid.Children.Add(lab2);
            for (int i = 0; i < m + 1; i++)
            {
                Label l = new Label();
                if (i == m)
                {
                    l.Content = " ";
                }
                else
                    l.Content = str[i + 1, 0];
                l.Width = 35;
                l.Height = 23;
                Grid.SetRow(l, i);
                Grid.SetColumn(l, 0);
                grid.Children.Add(l);
                col = 0;

                if (fract)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (matrF[m, j] == 1 && j != n)
                        {
                            col++;
                            continue;
                        }
                        Label lmatr = new Label();
                        lmatr.Content = matrF[i, j];
                        lmatr.MaxWidth = 50;
                        lmatr.Height = 23;
                        Grid.SetRow(lmatr, i + 1);
                        Grid.SetColumn(lmatr, j + 1 - col);
                        grid.Children.Add(lmatr);
                    }
                }
                else
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (matr[m, j] == 1 && j != n)
                        {
                            col++;
                            continue;
                        }
                        Label lmatr = new Label();
                        lmatr.Content = matr[i, j];
                        lmatr.MaxWidth = 50;
                        lmatr.Height = 23;
                        Grid.SetRow(lmatr, i + 1);
                        Grid.SetColumn(lmatr, j + 1 - col);
                        grid.Children.Add(lmatr);
                    }
                }
            }


            //Результативная матрица коэфицентов
            int n1 = grid.Columns - 1;
            int m1 = grid.Rows - 1;
            nRez = n1;
            mRez = m1;
            if (fract)
            {
                matrRezF = new Fraction[m1, n1];
                strRez = new string[m1, n1];
                for (int j = 0, j2 = 0; j < n + 1; j++)
                {
                    if (matrF[m, j] == 0)
                    {
                        for (int i = 0, i2 = 0; i < m + 1; i++)
                        {
                            matrRezF[i2, j2] = matrF[i, j];
                            i2++;
                        }
                        j2++;
                    }
                }
            }
            else
            {
                matrRez = new double[m1, n1];
                strRez = new string[m1, n1];
                for (int j = 0, j2 = 0; j < n + 1; j++)
                {
                    if (matr[m, j] == 0)
                    {
                        for (int i = 0, i2 = 0; i < m + 1; i++)
                        {
                            matrRez[i2, j2] = matr[i, j];
                            i2++;
                        }
                        j2++;
                    }
                }
            }

            strRez[0, 0] = str[0, 0];
            for (int i = 1, i2 = 1; i < m + 1; i++)
            {
                if (int.Parse(Regex.Match(str[i, 0], "[0-9]+").Value) <= n)
                {
                    strRez[i2, 0] = str[i, 0];
                    i2++;
                }
            }
            for (int j = 1, j2 = 1; j < n + 1; j++)
            {
                if (int.Parse(Regex.Match(str[0, j], "[0-9]+").Value) <= n)
                {
                    strRez[0, j2] = str[0, j];
                    j2++;
                }
            }

            //Угловая точка
            string tmp = string.Empty;
            bool ch = false;
            for (int j = 1; j < n + 1; j++)
            {
                ch = false;
                for (int i = 0; i < m1; i++)
                {
                    if (int.Parse(Regex.Match(strRez[i, 0], "[0-9]+").Value) == j)
                    {
                        if (fract)
                            tmp += matrRezF[i - 1, n1 - 1] + ";";
                        else
                            tmp += matrRez[i - 1, n1 - 1] + ";";
                        ch = true;
                    }

                }
                if (ch == false)
                    tmp += "0;";
            }
            tmp = tmp.Substring(0, tmp.Length - 1);
            op.Content = "x(0) = (" + tmp + ")";

            //Подсчет целевой функции в угловой точке
            int top;
            int left;
            if (fract)
            {
                Fraction rez = new Fraction(0);
                for (int j = 0; j < n1 - 1; j++)
                {
                    top = int.Parse(Regex.Match(strRez[0, j + 1], "[0-9]+").Value) - 1;
                    for (int i = 0; i < m1 - 1; i++)
                    {
                        left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                        rez += -1 * matrRezF[i, j] * functionF[left];
                    }
                    rez += functionF[top];
                    matrRezF[m1 - 1, j] = rez;
                    matrRezF[m1 - 1, j].Reduce();
                    rez = new Fraction(0);
                }

                for (int i = 0; i < m1 - 1; i++)
                {
                    left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                    rez += matrRezF[i, n1 - 1] * functionF[left];
                }
                matrRezF[m1 - 1, n1 - 1] = rez * -1;
                matrRezF[m1 - 1, n1 - 1].Reduce();
            }
            else
            {
                double rez = 0;
                for (int j = 0; j < n1 - 1; j++)
                {
                    top = int.Parse(Regex.Match(strRez[0, j + 1], "[0-9]+").Value) - 1;
                    for (int i = 0; i < m1 - 1; i++)
                    {
                        left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                        rez += -1 * matrRez[i, j] * function[left];
                    }
                    rez += function[top];
                    matrRez[m1 - 1, j] = rez;
                    rez = 0;
                }

                for (int i = 0; i < m1 - 1; i++)
                {
                    left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                    rez += matrRez[i, n1 - 1] * function[left];
                }
                matrRez[m1 - 1, n1 - 1] = rez * -1;
            }
        }

        //Отрисовка таблицы, поиск опорных, проверка на результат
        private void sixStep()
        {
            iS = -1;
            jS = -1;
            img1.Visibility = Visibility.Hidden;
            img2.Visibility = Visibility.Hidden;
            op.Content = "";
            sup.Content = "";
            //Формирование симплекс таблицы
            grid.Children.Clear();
            grid.Rows = mRez + 1;
            grid.Columns = nRez + 1;
            for (int i = 0; i < nRez; i++)
            {
                Label lab = new Label();
                lab.Width = 35;
                lab.Height = 23;
                lab.Content = strRez[0, i];
                Grid.SetRow(lab, 0);
                Grid.SetColumn(lab, i);
                grid.Children.Add(lab);
            }
            Label lab2 = new Label();
            lab2.Width = 35;
            lab2.Height = 23;
            lab2.Content = " ";
            Grid.SetRow(lab2, 0);
            Grid.SetColumn(lab2, nRez);
            grid.Children.Add(lab2);
            for (int i = 0; i < mRez; i++)
            {
                Label l = new Label();
                if (i == mRez - 1)
                {
                    l.Content = " ";
                }
                else
                    l.Content = strRez[i + 1, 0];
                l.Width = 35;
                l.Height = 23;
                Grid.SetRow(l, i);
                Grid.SetColumn(l, 0);
                grid.Children.Add(l);
                for (int j = 0; j < nRez; j++)
                {
                    Label lmatr = new Label();
                    if (fract)
                        lmatr.Content = matrRezF[i, j];
                    else
                        lmatr.Content = matrRez[i, j];
                    lmatr.Name = "a" + i + "a" + j;
                    lmatr.MaxWidth = 50;
                    lmatr.Height = 23;
                    Grid.SetRow(lmatr, i + 1);
                    Grid.SetColumn(lmatr, j + 1);
                    grid.Children.Add(lmatr);
                }
            }

            //Поиск опорного элемента
            List<Label> cells = grid.Children.OfType<Label>().OrderBy(Grid.GetRow).ThenBy(Grid.GetColumn).ToList();
            bool[,] supRez = new bool[mRez, nRez];
            ch = 0;
            bool chEnd = false;
            if (fract)
            {
                for (int j = 0; j < nRez - 1; j++)
                {
                    if (matrRezF[mRez - 1, j] < 0)
                    {
                        Fraction min = null;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        chEnd = true;
                        for (int i = 0; i < mRez - 1; i++)
                        {
                            if (matrRezF[i, j] > 0)
                            {
                                if (min == null)
                                {
                                    min = matrRezF[i, nRez - 1] / matrRezF[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                    continue;
                                }
                                if (matrRezF[i, nRez - 1] / matrRezF[i, j] < min)
                                {
                                    min = matrRezF[i, nRez - 1] / matrRezF[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matrRezF[i, nRez - 1] / matrRezF[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 1)
                        {
                            for (int i = 0; i < mRez; i++)
                            {
                                if (matrRezF[i, nRez - 1] / matrRezF[i, j] == min)
                                {
                                    supRez[i, j] = true;
                                    ch++;
                                }
                            }
                        }
                        if (count == 1)
                        {
                            supRez[i1, j1] = true;
                            ch++;
                        }
                        if (count == 0)
                        {
                            MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < nRez - 1; j++)
                {
                    if (matrRez[mRez - 1, j] < 0)
                    {
                        double min = int.MaxValue;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        chEnd = true;
                        for (int i = 0; i < mRez - 1; i++)
                        {
                            if (matrRez[i, j] > 0)
                            {
                                if (matrRez[i, nRez - 1] / matrRez[i, j] < min)
                                {
                                    min = matrRez[i, nRez - 1] / matrRez[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matrRez[i, nRez - 1] / matrRez[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 1)
                        {
                            for (int i = 0; i < mRez - 1; i++)
                            {
                                if (matrRez[i, nRez - 1] / matrRez[i, j] == min)
                                {
                                    supRez[i, j] = true;
                                    ch++;
                                }
                            }
                        }
                        if (count == 1)
                        {
                            supRez[i1, j1] = true;
                            ch++;
                        }
                        if (count == 0)
                        {
                            MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }
            if (ch == 1)
            {
                for (int i = 0; i < mRez; i++)
                {
                    for (int j = 0; j < nRez; j++)
                    {
                        if (supRez[i, j])
                        {
                            cells[(i + 1) * (nRez + 1) + (j + 2)].Background = new SolidColorBrush(Colors.LightGreen);
                            iS = i;
                            jS = j;
                        }
                    }
                }
            }
            if (ch > 1)
            {
                for (int i = 0; i < mRez; i++)
                {
                    for (int j = 0; j < nRez; j++)
                    {
                        if (supRez[i, j])
                        {
                            cells[(i + 1) * (nRez + 1) + (j + 2)].Background = new SolidColorBrush(Colors.LightGreen);
                            cells[(i + 1) * (nRez + 1) + (j + 2)].MouseDoubleClick += call;
                        }
                    }
                }
            }
            //Проверка на результат           
            if (chEnd == false)
            {
                end = true;
                return;
            }
        }

        //Вычисление симплекс таблицы
        public void sevenStep()
        {

            if (iS == -1 && jS == -1 && ch > 0)
            {
                sup.Content = "Выберите опорный элемент!";
                return;
            }

            if (fract)
            {
                Fraction[,] matrOld = new Fraction[mRez, nRez];
                for (int i = 0; i < mRez; i++)
                {
                    for (int j = 0; j < nRez; j++)
                    {
                        matrOld[i, j] = matrRezF[i, j];
                    }
                }
                //Вычисление симплекс таблицы
                string tmp2 = strRez[0, jS + 1];
                strRez[0, jS + 1] = strRez[iS + 1, 0];
                strRez[iS + 1, 0] = tmp2;
                for (int j = 0; j < nRez; j++)
                {
                    if (j == jS)
                        continue;
                    matrRezF[iS, j] /= matrRezF[iS, jS];
                    matrRezF[iS, j].Reduce();
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    matrRezF[i, jS] /= -1 * matrRezF[iS, jS];
                    matrRezF[i, jS].Reduce();
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < nRez; j++)
                    {
                        if (j == jS)
                            continue;
                        matrRezF[i, j] -= matrOld[i, jS] * matrRezF[iS, j];
                        matrRezF[i, j].Reduce();
                    }
                }
                matrRezF[iS, jS] = 1 / matrRezF[iS, jS];
                matrRezF[iS, jS].Reduce();
            }
            else
            {
                double[,] matrOld = new double[mRez, nRez];
                for (int i = 0; i < mRez; i++)
                {
                    for (int j = 0; j < nRez; j++)
                    {
                        matrOld[i, j] = matrRez[i, j];
                    }
                }
                //Вычисление симплекс таблицы
                string tmp2 = strRez[0, jS + 1];
                strRez[0, jS + 1] = strRez[iS + 1, 0];
                strRez[iS + 1, 0] = tmp2;
                for (int j = 0; j < nRez; j++)
                {
                    if (j == jS)
                        continue;
                    matrRez[iS, j] /= matrRez[iS, jS];
                    if (Math.Abs(matrRez[iS, j] - Math.Round(matrRez[iS, j])) < 0.0000001)
                        matrRez[iS, j] = Math.Round(matrRez[iS, j]);
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    matrRez[i, jS] /= -1 * matrRez[iS, jS];
                    if (Math.Abs(matrRez[i, jS] - Math.Round(matrRez[i, jS])) < 0.0000001)
                        matrRez[i, jS] = Math.Round(matrRez[i, jS]);
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < nRez; j++)
                    {
                        if (j == jS)
                            continue;
                        matrRez[i, j] -= matrOld[i, jS] * matrRez[iS, j];
                        if (Math.Abs(matrRez[i, j] - Math.Round(matrRez[i, j])) < 0.0000001)
                            matrRez[i, j] = Math.Round(matrRez[i, j]);
                    }
                }

                matrRez[iS, jS] = 1 / matrRez[iS, jS];
                if (Math.Abs(matrRez[iS, jS] - Math.Round(matrRez[iS, jS])) < 0.0000001)
                    matrRez[iS, jS] = Math.Round(matrRez[iS, jS]);
            }
            sixStep();
        }

        //Автоматическое решение с заданными базисами
        public void autoCalcS()
        {
            //Приведение целевой функции к каноническому виду
            string newExtr = extr;
            if (extr == "max")
            {
                if (fract)
                {
                    for (int i = 0; i < n; i++)
                    {
                        functionF[i] = functionF[i] * -1;
                    }
                    newExtr = "min";
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        function[i] = function[i] * -1;
                    }
                    newExtr = "min";
                }
            }
            //Создание матрицы
            if (fract)
            {
                matrFS = new Fraction[m, n + 1];
                for (int k = 0, l = 0; k < list.SelectedItems.Count; k++)
                {
                    int tmp = int.Parse(Regex.Match(list.SelectedItems[k].ToString(), "[0-9]+").Value);
                    for (int j = 0; j < n; j++)
                    {
                        if (j + 1 == tmp)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                matrFS[i, l] = tabF[i, j];
                            }
                            l++;
                            break;
                        }
                    }
                }
                for (int j = 0, l = list.SelectedItems.Count; j < n + 1; j++)
                {
                    if (list.SelectedItems.Contains("x" + (j + 1)))
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrFS[i, l] = tabF[i, j];
                    }
                    l++;
                }
            }
            else
            {
                matrS = new double[m, n + 1];
                for (int k = 0, l = 0; k < list.SelectedItems.Count; k++)
                {
                    int tmp = int.Parse(Regex.Match(list.SelectedItems[k].ToString(), "[0-9]+").Value);
                    for (int j = 0; j < n; j++)
                    {
                        if (j + 1 == tmp)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                matrS[i, l] = tab[i, j];
                            }
                            l++;
                            break;
                        }
                    }
                }
                for (int j = 0, l = list.SelectedItems.Count; j < n + 1; j++)
                {
                    if (list.SelectedItems.Contains("x" + (j + 1)))
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrS[i, l] = tab[i, j];
                    }
                    l++;
                }
            }
            //Решение матричной системы, метод Гаусса
            bool check = false;
            if (fract)
            {
                for (int k = 0; k < m; k++)
                {
                    if (matrFS[k, k] == 0)
                    {
                        for (int i = k + 1; i < m; i++)
                        {
                            if (matrFS[i, 0] != 0)
                            {
                                check = true;
                                for (int j = 0; j < n + 1; j++)
                                {
                                    Fraction tmp = matrFS[k, j];
                                    matrFS[k, j] = matrFS[k + 1, j];
                                    matrFS[k + 1, j] = tmp;
                                }
                                break;
                            }
                        }
                        if (!check)
                        {
                            MessageBox.Show("Ошибка в выражении базисов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    for (int j = k + 1; j < n + 1; j++)
                    {
                        matrFS[k, j] /= matrFS[k, k];
                        matrFS[k, j].Reduce();
                    }
                    matrFS[k, k] = new Fraction(1);
                    for (int i = k + 1; i < m; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrFS[i, j] -= matrFS[k, j] * matrFS[i, k];
                            matrFS[i, j].Reduce();
                        }
                        matrFS[i, k] = new Fraction(0);
                    }
                }
                for (int k = 1; k < m; k++)
                {
                    for (int i = 0; i < k; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrFS[i, j] -= matrFS[k, j] * matrFS[i, k];
                            matrFS[i, j].Reduce();
                        }
                        matrFS[i, k] = new Fraction(0);
                    }
                }
            }
            else
            {
                for (int k = 0; k < m; k++)
                {
                    if (matrS[k, k] == 0)
                    {
                        for (int i = k + 1; i < m; i++)
                        {
                            if (matrS[i, 0] != 0)
                            {
                                check = true;
                                for (int j = 0; j < n + 1; j++)
                                {
                                    double tmp = matrS[k, j];
                                    matrS[k, j] = matrS[k + 1, j];
                                    matrS[k + 1, j] = tmp;
                                }
                                break;
                            }
                        }
                        if (!check)
                        {
                            MessageBox.Show("Ошибка в выражении базисов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    for (int j = k + 1; j < n + 1; j++)
                    {
                        matrS[k, j] /= matrS[k, k];
                        if (Math.Abs(matrS[k, j] - Math.Round(matrS[k, j])) < 0.0000001)
                            matrS[k, j] = Math.Round(matrS[k, j]);
                    }
                    matrS[k, k] = 1;
                    for (int i = k + 1; i < m; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrS[i, j] -= matrS[k, j] * matrS[i, k];
                            if (Math.Abs(matrS[i, j] - Math.Round(matrS[i, j])) < 0.0000001)
                                matrS[i, j] = Math.Round(matrS[i, j]);
                        }
                        matrS[i, k] = 0;
                    }
                }
                for (int k = 1; k < m; k++)
                {
                    for (int i = 0; i < k; i++)
                    {
                        for (int j = k + 1; j < n + 1; j++)
                        {
                            matrS[i, j] -= matrS[k, j] * matrS[i, k];
                            if (Math.Abs(matrS[i, j] - Math.Round(matrS[i, j])) < 0.0000001)
                                matrS[i, j] = Math.Round(matrS[i, j]);
                        }
                        matrS[i, k] = 0;
                    }
                }
            }
            //Построение симплекс таблицы по конечной матрице
            if (fract)
            {
                nRez = n + 1 - list.SelectedItems.Count;
                mRez = m + 1;
                matrRezF = new Fraction[m + 1, n + 1 - m];
                strRez = new string[m + 1, n + 1 - m];
                int top;
                Fraction rez = new Fraction(0);
                for (int j = list.SelectedItems.Count, k = 0, l = 0; j < n; k++)
                {
                    if (list.SelectedItems.Contains("x" + (k + 1)))
                        continue;
                    top = k;
                    for (int i = 0; i < m; i++)
                    {
                        rez += -1 * matrFS[i, j] * functionF[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                    }
                    rez += functionF[top];
                    matrRezF[m, l] = rez;
                    rez = new Fraction(0);
                    l++;
                    j++;
                }
                for (int i = 0; i < m; i++)
                {
                    rez += matrFS[i, n] * functionF[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                }
                matrRezF[m, n - m] = rez * -1;


                for (int j = 0, l = 0; j < n + 1; j++)
                {
                    if (j < list.SelectedItems.Count)
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrRezF[i, l] = matrFS[i, j];
                    }
                    l++;
                }
            }
            else
            {
                nRez = n + 1 - list.SelectedItems.Count;
                mRez = m + 1;
                matrRez = new double[m + 1, n + 1 - m];
                strRez = new string[m + 1, n + 1 - m];
                int top;
                double rez = 0;
                for (int j = list.SelectedItems.Count, k = 0, l = 0; j < n; k++)
                {
                    if (list.SelectedItems.Contains("x" + (k + 1)))
                        continue;
                    top = k;
                    for (int i = 0; i < m; i++)
                    {
                        rez += -1 * matrS[i, j] * function[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                    }
                    rez += function[top];
                    matrRez[m, l] = rez;
                    rez = 0;
                    l++;
                    j++;
                }
                for (int i = 0; i < m; i++)
                {
                    rez += matrS[i, n] * function[int.Parse(Regex.Match(list.SelectedItems[i].ToString(), "[0-9]+").Value) - 1];
                }
                matrRez[m, n - m] = rez * -1;


                for (int j = 0, l = 0; j < n + 1; j++)
                {
                    if (j < list.SelectedItems.Count)
                        continue;
                    for (int i = 0; i < m; i++)
                    {
                        matrRez[i, l] = matrS[i, j];
                    }
                    l++;
                }
            }

            strRez[0, 0] = "x(0)";
            for (int i = 0; i < list.SelectedItems.Count; i++)
            {
                strRez[i + 1, 0] = list.SelectedItems[i].ToString();
            }
            for (int j = 0, l = 0; l < n; l++)
            {
                if (list.SelectedItems.Contains("x" + (l + 1)))
                {
                    continue;
                }
                strRez[0, j + 1] = "x" + (l + 1);
                j++;
            }

        repeat:
            //Поиск опорного элемента

            bool[,] supRez = new bool[mRez, nRez];
            ch = 0;
            bool chEnd = false;
            if (fract)
            {
                for (int j = 0; j < nRez - 1; j++)
                {
                    if (matrRezF[mRez - 1, j] < 0)
                    {
                        Fraction min = null;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        chEnd = true;
                        for (int i = 0; i < mRez - 1; i++)
                        {
                            if (matrRezF[i, j] > 0)
                            {
                                if (min == null)
                                {
                                    min = matrRezF[i, nRez - 1] / matrRezF[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                    continue;
                                }
                                if (matrRezF[i, nRez - 1] / matrRezF[i, j] < min)
                                {
                                    min = matrRezF[i, nRez - 1] / matrRezF[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matrRezF[i, nRez - 1] / matrRezF[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 1)
                        {
                            for (int i = 0; i < mRez; i++)
                            {
                                if (matrRezF[i, nRez - 1] / matrRezF[i, j] == min)
                                {
                                    supRez[i, j] = true;
                                    ch++;
                                    iS = i;
                                    jS = j;
                                    break;
                                }
                            }
                        }
                        if (count == 1)
                        {
                            supRez[i1, j1] = true;
                            iS = i1;
                            jS = j1;
                            ch++;
                        }
                        if (count == 0)
                        {
                            MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < nRez - 1; j++)
                {
                    if (matrRez[mRez - 1, j] < 0)
                    {
                        double min = int.MaxValue;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        chEnd = true;
                        for (int i = 0; i < mRez - 1; i++)
                        {
                            if (matrRez[i, j] > 0)
                            {
                                if (matrRez[i, nRez - 1] / matrRez[i, j] < min)
                                {
                                    min = matrRez[i, nRez - 1] / matrRez[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matrRez[i, nRez - 1] / matrRez[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 1)
                        {
                            for (int i = 0; i < mRez - 1; i++)
                            {
                                if (matrRez[i, nRez - 1] / matrRez[i, j] == min)
                                {
                                    supRez[i, j] = true;
                                    ch++;
                                    iS = i;
                                    jS = j;
                                    break;
                                }
                            }
                        }
                        if (count == 1)
                        {
                            supRez[i1, j1] = true;
                            iS = i1;
                            jS = j1;
                            ch++;
                        }
                        if (count == 0)
                        {
                            MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }

            //Проверка на результат           
            if (chEnd == false)
            {
                //Когда конец
                grid.Visibility = Visibility.Hidden;
                if (fract)
                {
                    if (extr == "min")
                        @out.Text = "f* = " + (matrRezF[mRez - 1, nRez - 1] * -1) + "\n";
                    else
                        @out.Text = "f* = " + matrRezF[mRez - 1, nRez - 1] + "\n";
                }
                else
                {
                    if (extr == "min")
                        @out.Text = "f* = " + (matrRez[mRez - 1, nRez - 1] * -1) + "\n";
                    else
                        @out.Text = "f* = " + matrRez[mRez - 1, nRez - 1] + "\n";
                }
                //Угловая точка
                string tmp = string.Empty;
                bool check2 = false;
                for (int j = 1; j < n + 1; j++)
                {
                    check2 = false;
                    for (int i = 0; i < mRez; i++)
                    {
                        if (int.Parse(Regex.Match(strRez[i, 0], "[0-9]+").Value) == j)
                        {
                            if (fract)
                                tmp += matrRezF[i - 1, nRez - 1] + ";";
                            else
                                tmp += matrRez[i - 1, nRez - 1] + ";";
                            check2 = true;
                        }

                    }
                    if (check2 == false)
                        tmp += "0;";
                }
                tmp = tmp.Substring(0, tmp.Length - 1);
                @out.Text += "x* = (" + tmp + ")";
                next.Visibility = Visibility.Hidden;
                prev.Visibility = Visibility.Hidden;
                return;
            }

            if (fract)
            {
                Fraction[,] matrOld = new Fraction[mRez, nRez];
                for (int i = 0; i < mRez; i++)
                {
                    for (int j = 0; j < nRez; j++)
                    {
                        matrOld[i, j] = matrRezF[i, j];
                    }
                }
                //Вычисление симплекс таблицы
                string tmp2 = strRez[0, jS + 1];
                strRez[0, jS + 1] = strRez[iS + 1, 0];
                strRez[iS + 1, 0] = tmp2;
                for (int j = 0; j < nRez; j++)
                {
                    if (j == jS)
                        continue;
                    matrRezF[iS, j] /= matrRezF[iS, jS];
                    matrRezF[iS, j].Reduce();
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    matrRezF[i, jS] /= -1 * matrRezF[iS, jS];
                    matrRezF[i, jS].Reduce();
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < nRez; j++)
                    {
                        if (j == jS)
                            continue;
                        matrRezF[i, j] -= matrOld[i, jS] * matrRezF[iS, j];
                        matrRezF[i, j].Reduce();
                    }
                }
                matrRezF[iS, jS] = 1 / matrRezF[iS, jS];
                matrRezF[iS, jS].Reduce();
            }
            else
            {
                double[,] matrOld = new double[mRez, nRez];
                for (int i = 0; i < mRez; i++)
                {
                    for (int j = 0; j < nRez; j++)
                    {
                        matrOld[i, j] = matrRez[i, j];
                    }
                }
                //Вычисление симплекс таблицы
                string tmp2 = strRez[0, jS + 1];
                strRez[0, jS + 1] = strRez[iS + 1, 0];
                strRez[iS + 1, 0] = tmp2;
                for (int j = 0; j < nRez; j++)
                {
                    if (j == jS)
                        continue;
                    matrRez[iS, j] /= matrRez[iS, jS];
                    if (Math.Abs(matrRez[iS, j] - Math.Round(matrRez[iS, j])) < 0.0000001)
                        matrRez[iS, j] = Math.Round(matrRez[iS, j]);
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    matrRez[i, jS] /= -1 * matrRez[iS, jS];
                    if (Math.Abs(matrRez[i, jS] - Math.Round(matrRez[i, jS])) < 0.0000001)
                        matrRez[i, jS] = Math.Round(matrRez[i, jS]);
                }
                for (int i = 0; i < mRez; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < nRez; j++)
                    {
                        if (j == jS)
                            continue;
                        matrRez[i, j] -= matrOld[i, jS] * matrRez[iS, j];
                        if (Math.Abs(matrRez[i, j] - Math.Round(matrRez[i, j])) < 0.0000001)
                            matrRez[i, j] = Math.Round(matrRez[i, j]);
                    }
                }

                matrRez[iS, jS] = 1 / matrRez[iS, jS];
                if (Math.Abs(matrRez[iS, jS] - Math.Round(matrRez[iS, jS])) < 0.0000001)
                    matrRez[iS, jS] = Math.Round(matrRez[iS, jS]);
            }

            iS = -1;
            jS = -1;
            goto repeat;
        }

        //Автоматическое решение искусственным базисом
        public void autoCalc()
        {
            /* Шаг 1 */

            bool[,] supRez;
            bool chEnd;

            //Проверка на неотрицательные свободные члены
            if (fract == true)
            {
                for (int i = 0; i < m; i++)
                {
                    if (tabF[i, n] < 0)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            tabF[i, j] = tabF[i, j] * -1;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < m; i++)
                {
                    if (tab[i, n] < 0)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            tab[i, j] = tab[i, j] * -1;
                        }
                    }
                }
            }
            //Приведение целевой функции к каноническому виду
            if (extr == "max")
            {
                if (fract)
                {
                    for (int i = 0; i < n; i++)
                    {
                        functionF[i] = functionF[i] * -1;
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        function[i] = function[i] * -1;
                    }
                }
            }

            //Инициализация симплекс таблицы
            if (fract)
            {
                matrF = new Fraction[m + 1, n + 1];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matrF[i, j] = tabF[i, j];
                        if (i == m - 1)
                        {
                            matrF[i + 1, j] = new Fraction(0);
                            for (int k = 0; k < m; k++)
                            {
                                matrF[i + 1, j] += tabF[k, j];
                            }
                            matrF[i + 1, j] = matrF[i + 1, j] * -1;
                        }
                    }
                }
            }
            else
            {
                matr = new double[m + 1, n + 1];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matr[i, j] = tab[i, j];
                        if (i == m - 1)
                        {
                            matr[i + 1, j] = 0;
                            for (int k = 0; k < m; k++)
                            {
                                matr[i + 1, j] += tab[k, j];
                            }
                            matr[i + 1, j] = matr[i + 1, j] * -1;
                        }
                    }
                }
            }

        step3:
            if (checkRez == true)
            {
                //Поиск опорного элемента
                supRez = new bool[mRez, nRez];
                ch = 0;
                chEnd = false;
                if (fract)
                {
                    for (int j = 0; j < nRez - 1; j++)
                    {
                        if (matrRezF[mRez - 1, j] < 0)
                        {
                            Fraction min = null;
                            int count = 0;
                            int i1 = 0;
                            int j1 = 0;
                            chEnd = true;
                            for (int i = 0; i < mRez - 1; i++)
                            {
                                if (matrRezF[i, j] > 0)
                                {
                                    if (min == null)
                                    {
                                        min = matrRezF[i, nRez - 1] / matrRezF[i, j];
                                        count = 1;
                                        i1 = i;
                                        j1 = j;
                                        continue;
                                    }
                                    if (matrRezF[i, nRez - 1] / matrRezF[i, j] < min)
                                    {
                                        min = matrRezF[i, nRez - 1] / matrRezF[i, j];
                                        count = 1;
                                        i1 = i;
                                        j1 = j;
                                    }
                                    else
                                    {
                                        if (matrRezF[i, nRez - 1] / matrRezF[i, j] == min)
                                        {
                                            count++;
                                        }
                                    }
                                }
                            }
                            if (count > 1)
                            {
                                for (int i = 0; i < mRez; i++)
                                {
                                    if (matrRezF[i, nRez - 1] / matrRezF[i, j] == min)
                                    {
                                        supRez[i, j] = true;
                                        ch++;
                                        iS = i;
                                        jS = j;
                                        break;
                                    }
                                }
                            }
                            if (count == 1)
                            {
                                supRez[i1, j1] = true;
                                iS = i1;
                                jS = j1;
                                ch++;
                            }
                            if (count == 0)
                            {
                                MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < nRez - 1; j++)
                    {
                        if (matrRez[mRez - 1, j] < 0)
                        {
                            double min = int.MaxValue;
                            int count = 0;
                            int i1 = 0;
                            int j1 = 0;
                            chEnd = true;
                            for (int i = 0; i < mRez - 1; i++)
                            {
                                if (matrRez[i, j] > 0)
                                {
                                    if (matrRez[i, nRez - 1] / matrRez[i, j] < min)
                                    {
                                        min = matrRez[i, nRez - 1] / matrRez[i, j];
                                        count = 1;
                                        i1 = i;
                                        j1 = j;
                                    }
                                    else
                                    {
                                        if (matrRez[i, nRez - 1] / matrRez[i, j] == min)
                                        {
                                            count++;
                                        }
                                    }
                                }
                            }
                            if (count > 1)
                            {
                                for (int i = 0; i < mRez - 1; i++)
                                {
                                    if (matrRez[i, nRez - 1] / matrRez[i, j] == min)
                                    {
                                        supRez[i, j] = true;
                                        ch++;
                                        iS = i;
                                        jS = j;
                                        break;
                                    }
                                }
                            }
                            if (count == 1)
                            {
                                supRez[i1, j1] = true;
                                iS = i1;
                                jS = j1;
                                ch++;
                            }
                            if (count == 0)
                            {
                                MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                        }
                    }
                }

                //Проверка на результат           
                if (chEnd == false)
                {
                    @out.Visibility = Visibility.Visible;
                    next.Visibility = Visibility.Hidden;
                    prev.Visibility = Visibility.Hidden;
                    //Когда конец
                    if (fract)
                    {
                        if (extr == "min")
                            @out.Text = "f* = " + (matrRezF[mRez - 1, nRez - 1] * -1) + "\n";
                        else
                            @out.Text = "f* = " + matrRezF[mRez - 1, nRez - 1] + "\n";
                    }
                    else
                    {
                        if (extr == "min")
                            @out.Text = "f* = " + (matrRez[mRez - 1, nRez - 1] * -1) + "\n";
                        else
                            @out.Text = "f* = " + matrRez[mRez - 1, nRez - 1] + "\n";
                    }
                    //Угловая точка
                    string tmp = string.Empty;
                    bool check2 = false;
                    for (int j = 1; j < n + 1; j++)
                    {
                        check2 = false;
                        for (int i = 0; i < mRez; i++)
                        {
                            if (int.Parse(Regex.Match(strRez[i, 0], "[0-9]+").Value) == j)
                            {
                                if (fract)
                                    tmp += matrRezF[i - 1, nRez - 1] + ";";
                                else
                                    tmp += matrRez[i - 1, nRez - 1] + ";";
                                check2 = true;
                            }

                        }
                        if (check2 == false)
                            tmp += "0;";
                    }
                    tmp = tmp.Substring(0, tmp.Length - 1);
                    @out.Text += "x* = (" + tmp + ")";
                    return;
                }

                if (fract)
                {
                    Fraction[,] matrOld = new Fraction[mRez, nRez];
                    for (int i = 0; i < mRez; i++)
                    {
                        for (int j = 0; j < nRez; j++)
                        {
                            matrOld[i, j] = matrRezF[i, j];
                        }
                    }
                    //Вычисление симплекс таблицы
                    string tmp2 = strRez[0, jS + 1];
                    strRez[0, jS + 1] = strRez[iS + 1, 0];
                    strRez[iS + 1, 0] = tmp2;
                    for (int j = 0; j < nRez; j++)
                    {
                        if (j == jS)
                            continue;
                        matrRezF[iS, j] /= matrRezF[iS, jS];
                        matrRezF[iS, j].Reduce();
                    }
                    for (int i = 0; i < mRez; i++)
                    {
                        if (i == iS)
                            continue;
                        matrRezF[i, jS] /= -1 * matrRezF[iS, jS];
                        matrRezF[i, jS].Reduce();
                    }
                    for (int i = 0; i < mRez; i++)
                    {
                        if (i == iS)
                            continue;
                        for (int j = 0; j < nRez; j++)
                        {
                            if (j == jS)
                                continue;
                            matrRezF[i, j] -= matrOld[i, jS] * matrRezF[iS, j];
                            matrRezF[i, j].Reduce();
                        }
                    }
                    matrRezF[iS, jS] = 1 / matrRezF[iS, jS];
                    matrRezF[iS, jS].Reduce();
                }
                else
                {
                    double[,] matrOld = new double[mRez, nRez];
                    for (int i = 0; i < mRez; i++)
                    {
                        for (int j = 0; j < nRez; j++)
                        {
                            matrOld[i, j] = matrRez[i, j];
                        }
                    }
                    //Вычисление симплекс таблицы
                    string tmp2 = strRez[0, jS + 1];
                    strRez[0, jS + 1] = strRez[iS + 1, 0];
                    strRez[iS + 1, 0] = tmp2;
                    for (int j = 0; j < nRez; j++)
                    {
                        if (j == jS)
                            continue;
                        matrRez[iS, j] /= matrRez[iS, jS];
                        if (Math.Abs(matrRez[iS, j] - Math.Round(matrRez[iS, j])) < 0.0000001)
                            matrRez[iS, j] = Math.Round(matrRez[iS, j]);
                    }
                    for (int i = 0; i < mRez; i++)
                    {
                        if (i == iS)
                            continue;
                        matrRez[i, jS] /= -1 * matrRez[iS, jS];
                        if (Math.Abs(matrRez[i, jS] - Math.Round(matrRez[i, jS])) < 0.0000001)
                            matrRez[i, jS] = Math.Round(matrRez[i, jS]);
                    }
                    for (int i = 0; i < mRez; i++)
                    {
                        if (i == iS)
                            continue;
                        for (int j = 0; j < nRez; j++)
                        {
                            if (j == jS)
                                continue;
                            matrRez[i, j] -= matrOld[i, jS] * matrRez[iS, j];
                            if (Math.Abs(matrRez[i, j] - Math.Round(matrRez[i, j])) < 0.0000001)
                                matrRez[i, j] = Math.Round(matrRez[i, j]);
                        }
                    }

                    matrRez[iS, jS] = 1 / matrRez[iS, jS];
                    if (Math.Abs(matrRez[iS, jS] - Math.Round(matrRez[iS, jS])) < 0.0000001)
                        matrRez[iS, jS] = Math.Round(matrRez[iS, jS]);
                }

                iS = -1;
                jS = -1;
                goto step3;
            }

            //Поиск опорного элемента

            supRez = new bool[m, n];
            ch = 0;
            chEnd = false;
            if (fract)
            {
                for (int j = 0; j < n; j++)
                {
                    if (matrF[m, j] < 0)
                    {
                        Fraction min = null;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        chEnd = true;
                        for (int i = 0; i < m; i++)
                        {
                            if (matrF[i, j] > 0)
                            {
                                if (min == null)
                                {
                                    min = matrF[i, n] / matrF[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                    continue;
                                }
                                if (matrF[i, n] / matrF[i, j] < min)
                                {
                                    min = matrF[i, n] / matrF[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matrF[i, n] / matrF[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 1)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                if (matrF[i, n] / matrF[i, j] == min)
                                {
                                    supRez[i, j] = true;
                                    ch++;
                                    iS = i;
                                    jS = j;
                                    break;
                                }
                            }
                        }
                        if (count == 1)
                        {
                            supRez[i1, j1] = true;
                            iS = i1;
                            jS = j1;
                            ch++;
                        }
                        if (count == 0)
                        {
                            MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < n; j++)
                {
                    if (matr[m, j] < 0)
                    {
                        double min = int.MaxValue;
                        int count = 0;
                        int i1 = 0;
                        int j1 = 0;
                        chEnd = true;
                        for (int i = 0; i < m; i++)
                        {
                            if (matr[i, j] > 0)
                            {
                                if (matr[i, n] / matr[i, j] < min)
                                {
                                    min = matr[i, n] / matr[i, j];
                                    count = 1;
                                    i1 = i;
                                    j1 = j;
                                }
                                else
                                {
                                    if (matr[i, n] / matr[i, j] == min)
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if (count > 1)
                        {
                            for (int i = 0; i < m; i++)
                            {
                                if (matr[i, n] / matr[i, j] == min)
                                {
                                    supRez[i, j] = true;
                                    ch++;
                                    iS = i;
                                    jS = j;
                                    break;
                                }
                            }
                        }
                        if (count == 1)
                        {
                            supRez[i1, j1] = true;
                            iS = i1;
                            jS = j1;
                            ch++;
                        }
                        if (count == 0)
                        {
                            MessageBox.Show("Целевая функция не ограничена снизу\nзадача не имеет решения!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }

            bool che = false;
            if (fract)
            {
                //Проверка на результат
                for (int i = 0; i < n; i++)
                {
                    if (int.Parse(Regex.Match(str[0, i + 1], "[0-9]+").Value) <= n)
                    {
                        if (matrF[m, i] != 0)
                        {
                            che = true;
                        }
                    }
                }
                if (che == false)
                {
                    if (matrF[m, n] > 0)
                        MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (matrF[m, n] < 0)
                        MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (matrF[m, n] == 0)
                    {
                        int count = 0;
                        //Вывод конечной симплекс таблицы без искусственного базиса
                        if (fract)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                if (matrF[m, j] == 0)
                                {
                                    count++;
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < n; j++)
                            {
                                if (matr[m, j] == 0)
                                {
                                    count++;
                                }
                            }
                        }
                        //Результативная матрица коэфицентов
                        int n1 = count + 1;
                        int m1 = m + 1;
                        nRez = n1;
                        mRez = m1;
                        if (fract)
                        {
                            matrRezF = new Fraction[m1, n1];
                            strRez = new string[m1, n1];
                            for (int j = 0, j2 = 0; j < n + 1; j++)
                            {
                                if (matrF[m, j] == 0)
                                {
                                    for (int i = 0, i2 = 0; i < m + 1; i++)
                                    {
                                        matrRezF[i2, j2] = matrF[i, j];
                                        i2++;
                                    }
                                    j2++;
                                }
                            }
                        }
                        else
                        {
                            matrRez = new double[m1, n1];
                            strRez = new string[m1, n1];
                            for (int j = 0, j2 = 0; j < n + 1; j++)
                            {
                                if (matr[m, j] == 0)
                                {
                                    for (int i = 0, i2 = 0; i < m + 1; i++)
                                    {
                                        matrRez[i2, j2] = matr[i, j];
                                        i2++;
                                    }
                                    j2++;
                                }
                            }
                        }

                        strRez[0, 0] = str[0, 0];
                        for (int i = 1, i2 = 1; i < m + 1; i++)
                        {
                            if (int.Parse(Regex.Match(str[i, 0], "[0-9]+").Value) <= n)
                            {
                                strRez[i2, 0] = str[i, 0];
                                i2++;
                            }
                        }
                        for (int j = 1, j2 = 1; j < n + 1; j++)
                        {
                            if (int.Parse(Regex.Match(str[0, j], "[0-9]+").Value) <= n)
                            {
                                strRez[0, j2] = str[0, j];
                                j2++;
                            }
                        }

                        //Подсчет целевой функции в угловой точке
                        int top;
                        int left;
                        if (fract)
                        {
                            Fraction rez = new Fraction(0);
                            for (int j = 0; j < n1 - 1; j++)
                            {
                                top = int.Parse(Regex.Match(strRez[0, j + 1], "[0-9]+").Value) - 1;
                                for (int i = 0; i < m1 - 1; i++)
                                {
                                    left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                    rez += -1 * matrRezF[i, j] * functionF[left];
                                }
                                rez += functionF[top];
                                matrRezF[m1 - 1, j] = rez;
                                matrRezF[m1 - 1, j].Reduce();
                                rez = new Fraction(0);
                            }

                            for (int i = 0; i < m1 - 1; i++)
                            {
                                left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                rez += matrRezF[i, n1 - 1] * functionF[left];
                            }
                            matrRezF[m1 - 1, n1 - 1] = rez * -1;
                            matrRezF[m1 - 1, n1 - 1].Reduce();
                        }
                        else
                        {
                            double rez = 0;
                            for (int j = 0; j < n1 - 1; j++)
                            {
                                top = int.Parse(Regex.Match(strRez[0, j + 1], "[0-9]+").Value) - 1;
                                for (int i = 0; i < m1 - 1; i++)
                                {
                                    left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                    rez += -1 * matrRez[i, j] * function[left];
                                }
                                rez += function[top];
                                matrRez[m1 - 1, j] = rez;
                                rez = 0;
                            }

                            for (int i = 0; i < m1 - 1; i++)
                            {
                                left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                rez += matrRez[i, n1 - 1] * function[left];
                            }
                            matrRez[m1 - 1, n1 - 1] = rez * -1;
                        }

                        //Доступ к симплекс методу
                        checkRez = true;
                        goto step3;
                    }
                    return;
                }
                if (ch == 0 && che == true && (-1 * matrF[m, n]) > 0)
                {
                    MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (ch == 0 && che == true && (-1 * matrF[m, n]) < 0)
                {
                    MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Fraction[,] matrOld = new Fraction[m + 1, n + 1];
                for (int i = 0; i < m + 1; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matrOld[i, j] = matrF[i, j];
                    }
                }

                //Преобразование симплекс таблицы
                string tmp = str[0, jS + 1];
                str[0, jS + 1] = str[iS + 1, 0];
                str[iS + 1, 0] = tmp;

                for (int j = 0; j < n + 1; j++)
                {
                    if (j == jS)
                        continue;
                    matrF[iS, j] /= matrF[iS, jS];
                    matrF[iS, j].Reduce();
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    matrF[i, jS] /= (-1 * matrF[iS, jS]);
                    matrF[i, jS].Reduce();
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (j == jS)
                            continue;
                        matrF[i, j] -= matrOld[i, jS] * matrF[iS, j];
                        matrF[i, j].Reduce();
                    }
                }
                matrF[iS, jS] = 1 / matrF[iS, jS];
                matrF[iS, jS].Reduce();

                iS = -1;
                jS = -1;
                goto step3;
            }
            else
            {
                //Проверка на результат
                for (int i = 0; i < n; i++)
                {
                    if (int.Parse(Regex.Match(str[0, i + 1], "[0-9]+").Value) <= n)
                    {
                        if (matr[m, i] != 0)
                        {
                            che = true;
                        }
                    }
                }
                if (che == false)
                {
                    if (matr[m, n] > 0)
                        MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (matr[m, n] < 0)
                        MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (matr[m, n] == 0)
                    {
                        int count = 0;
                        //Вывод конечной симплекс таблицы без искусственного базиса
                        if (fract)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                if (matrF[m, j] == 0)
                                {
                                    count++;
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < n; j++)
                            {
                                if (matr[m, j] == 0)
                                {
                                    count++;
                                }
                            }
                        }
                        //Результативная матрица коэфицентов
                        int n1 = count + 1;
                        int m1 = m + 1;
                        nRez = n1;
                        mRez = m1;
                        if (fract)
                        {
                            matrRezF = new Fraction[m1, n1];
                            strRez = new string[m1, n1];
                            for (int j = 0, j2 = 0; j < n + 1; j++)
                            {
                                if (matrF[m, j] == 0)
                                {
                                    for (int i = 0, i2 = 0; i < m + 1; i++)
                                    {
                                        matrRezF[i2, j2] = matrF[i, j];
                                        i2++;
                                    }
                                    j2++;
                                }
                            }
                        }
                        else
                        {
                            matrRez = new double[m1, n1];
                            strRez = new string[m1, n1];
                            for (int j = 0, j2 = 0; j < n + 1; j++)
                            {
                                if (matr[m, j] == 0)
                                {
                                    for (int i = 0, i2 = 0; i < m + 1; i++)
                                    {
                                        matrRez[i2, j2] = matr[i, j];
                                        i2++;
                                    }
                                    j2++;
                                }
                            }
                        }

                        strRez[0, 0] = str[0, 0];
                        for (int i = 1, i2 = 1; i < m + 1; i++)
                        {
                            if (int.Parse(Regex.Match(str[i, 0], "[0-9]+").Value) <= n)
                            {
                                strRez[i2, 0] = str[i, 0];
                                i2++;
                            }
                        }
                        for (int j = 1, j2 = 1; j < n + 1; j++)
                        {
                            if (int.Parse(Regex.Match(str[0, j], "[0-9]+").Value) <= n)
                            {
                                strRez[0, j2] = str[0, j];
                                j2++;
                            }
                        }

                        //Подсчет целевой функции в угловой точке
                        int top;
                        int left;
                        if (fract)
                        {
                            Fraction rez = new Fraction(0);
                            for (int j = 0; j < n1 - 1; j++)
                            {
                                top = int.Parse(Regex.Match(strRez[0, j + 1], "[0-9]+").Value) - 1;
                                for (int i = 0; i < m1 - 1; i++)
                                {
                                    left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                    rez += -1 * matrRezF[i, j] * functionF[left];
                                }
                                rez += functionF[top];
                                matrRezF[m1 - 1, j] = rez;
                                matrRezF[m1 - 1, j].Reduce();
                                rez = new Fraction(0);
                            }

                            for (int i = 0; i < m1 - 1; i++)
                            {
                                left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                rez += matrRezF[i, n1 - 1] * functionF[left];
                            }
                            matrRezF[m1 - 1, n1 - 1] = rez * -1;
                            matrRezF[m1 - 1, n1 - 1].Reduce();
                        }
                        else
                        {
                            double rez = 0;
                            for (int j = 0; j < n1 - 1; j++)
                            {
                                top = int.Parse(Regex.Match(strRez[0, j + 1], "[0-9]+").Value) - 1;
                                for (int i = 0; i < m1 - 1; i++)
                                {
                                    left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                    rez += -1 * matrRez[i, j] * function[left];
                                }
                                rez += function[top];
                                matrRez[m1 - 1, j] = rez;
                                rez = 0;
                            }

                            for (int i = 0; i < m1 - 1; i++)
                            {
                                left = int.Parse(Regex.Match(strRez[i + 1, 0], "[0-9]+").Value) - 1;
                                rez += matrRez[i, n1 - 1] * function[left];
                            }
                            matrRez[m1 - 1, n1 - 1] = rez * -1;
                        }

                        //Доступ к симплекс методу
                        checkRez = true;
                        goto step3;
                    }
                    return;
                }
                if (ch == 0 && che == true && (-1 * matr[m, n]) > 0)
                {
                    MessageBox.Show("Исходное ограничение несовместно, т.е решения не существует.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (ch == 0 && che == true && (-1 * matr[m, n]) < 0)
                {
                    MessageBox.Show("Ошибка в вычислениях!\nНевозможный результат!", "Результат", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                double[,] matrOld = new double[m + 1, n + 1];
                for (int i = 0; i < m + 1; i++)
                {
                    for (int j = 0; j < n + 1; j++)
                    {
                        matrOld[i, j] = matr[i, j];
                    }
                }

                //Преобразование симплекс таблицы
                string tmp = str[0, jS + 1];
                str[0, jS + 1] = str[iS + 1, 0];
                str[iS + 1, 0] = tmp;

                for (int j = 0; j < n + 1; j++)
                {
                    if (j == jS)
                        continue;
                    matr[iS, j] /= matr[iS, jS];
                    if (Math.Abs(matr[iS, j] - Math.Round(matr[iS, j])) < 0.0000001)
                        matr[iS, j] = Math.Round(matr[iS, j]);
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    matr[i, jS] /= (-1 * matr[iS, jS]);
                    if (Math.Abs(matr[i, jS] - Math.Round(matr[i, jS])) < 0.0000001)
                        matr[i, jS] = Math.Round(matr[i, jS]);
                }
                for (int i = 0; i < m + 1; i++)
                {
                    if (i == iS)
                        continue;
                    for (int j = 0; j < n + 1; j++)
                    {
                        if (j == jS)
                            continue;
                        matr[i, j] -= matrOld[i, jS] * matr[iS, j];
                        if (Math.Abs(matr[i, j] - Math.Round(matr[i, j])) < 0.0000001)
                            matr[i, j] = Math.Round(matr[i, j]);
                    }
                }
                matr[iS, jS] = 1 / matr[iS, jS];
                if (Math.Abs(matr[iS, jS] - Math.Round(matr[iS, jS])) < 0.0000001)
                    matr[iS, jS] = Math.Round(matr[iS, jS]);

                iS = -1;
                jS = -1;
                goto step3;
            }
        }

        //Кнопка назад
        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (k < 1 && s == -1)
                return;
            if (s < 1 && start)
                return;
            if (t == 0)
                t--;
            sup.Content = "";
            if (endPrev == true)
            {
                @out.Text = "";
                grid.Visibility = Visibility.Visible;
                endPrev = false;
                sixStep();
                return;
            }
            end = false;
            if (s > 0)
            {
                switch (s)
                {
                    case 1:
                        grid.Children.Clear();
                        simplex();
                        s--;
                        break;
                    case 2:
                        grid.Children.Clear();
                        simplex();
                        gaus();
                        s--;
                        break;
                    default:
                        if (fract)
                            matrRezF = stackF.Pop();
                        else
                            matrRez = stack.Pop();
                        strRez = stackS.Pop();
                        grid.Children.Clear();
                        sixStep();
                        s--;
                        break;
                }
                return;
            }
            if (t > 0)
            {
                switch (t)
                {
                    case 1:
                        op.Content = "";
                        if (fract)
                            matrF = stackF.Pop();
                        else
                            matr = stack.Pop();
                        str = stackS.Pop();
                        grid.Children.Clear();
                        threeStep();
                        break;
                    case 2:
                        if (fract)
                            matrF = stackF.Pop();
                        else
                            matr = stack.Pop();
                        str = stackS.Pop();
                        grid.Children.Clear();
                        if (fract)
                        {
                            Fraction[,] matrCopyF = new Fraction[m + 1, n + 1];
                            for (int i = 0; i < m + 1; i++)
                            {
                                for (int j = 0; j < n + 1; j++)
                                {
                                    matrCopyF[i, j] = matrF[i, j];
                                }
                            }
                            stackF.Push(matrCopyF);
                        }
                        else
                        {
                            double[,] matrCopy = new double[m + 1, n + 1];
                            for (int i = 0; i < m + 1; i++)
                            {
                                for (int j = 0; j < n + 1; j++)
                                {
                                    matrCopy[i, j] = matr[i, j];
                                }
                            }
                            stack.Push(matrCopy);
                        }
                        string[,] strCopy = new string[m + 1, n + 1];
                        for (int i = 0; i < m + 1; i++)
                            strCopy[i, 0] = str[i, 0];
                        for (int j = 0; j < n + 1; j++)
                            strCopy[0, j] = str[0, j];
                        stackS.Push(strCopy);
                        fiveStep();
                        t--;
                        break;
                    default:
                        if (fract)
                            matrRezF = stackF.Pop();
                        else
                            matrRez = stack.Pop();
                        strRez = stackS.Pop();
                        grid.Children.Clear();
                        sixStep();
                        t--;
                        break;
                }
            }
            else
            {
                switch (k)
                {
                    case 1:
                        oneStep();
                        k--;
                        break;
                    case 2:
                        grid.Children.Clear();
                        twoStep();
                        k--;
                        break;
                    default:
                        grid.Children.Clear();
                        if (fract)
                            matrF = stackF.Pop();
                        else
                            matr = stack.Pop();
                        str = stackS.Pop();
                        threeStep();
                        k--;
                        break;
                }
            }
        }

        //Кнопка далее
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Fraction[,] matrCopyF;
            double[,] matrCopy;
            string[,] strCopy;
            bool chS = false;
            if (iS == -1 && k + 1 > 2 && t == -1)
            {
                sup.Content = "Выберите опорный элемент!";
                return;
            }
            if (start && s > 1)
            {

                for (int j = 0; j < nRez; j++)
                {
                    if (fract)
                    {
                        if (matrRezF[m, j] < 0)
                            chS = true;
                    }
                    else
                    {
                        if (matrRez[m, j] < 0)
                            chS = true;
                    }
                }
                if (iS == -1 && chS)
                {
                    sup.Content = "Выберите опорный элемент!";
                    return;
                }
            }
            if (end == true)
            {
                endPrev = true;
                //Когда конец
                grid.Visibility = Visibility.Hidden;
                if (fract)
                {
                    if (extr == "min")
                        @out.Text = "f* = " + (matrRezF[mRez - 1, nRez - 1] * -1) + "\n";
                    else
                        @out.Text = "f* = " + matrRezF[mRez - 1, nRez - 1] + "\n";
                }
                else
                {
                    if (extr == "min")
                        @out.Text = "f* = " + (matrRez[mRez - 1, nRez - 1] * -1) + "\n";
                    else
                        @out.Text = "f* = " + matrRez[mRez - 1, nRez - 1] + "\n";
                }
                //Угловая точка
                string tmp = string.Empty;
                bool check = false;
                for (int j = 1; j < n + 1; j++)
                {
                    check = false;
                    for (int i = 0; i < mRez; i++)
                    {
                        if (int.Parse(Regex.Match(strRez[i, 0], "[0-9]+").Value) == j)
                        {
                            if (fract)
                                tmp += matrRezF[i - 1, nRez - 1] + ";";
                            else
                                tmp += matrRez[i - 1, nRez - 1] + ";";
                            check = true;
                        }

                    }
                    if (check == false)
                        tmp += "0;";
                }
                tmp = tmp.Substring(0, tmp.Length - 1);
                @out.Text += "x* = (" + tmp + ")";
                return;
            }

            if (s == 0)
            {
                if (fract)
                {
                    matrCopyF = new Fraction[m, n + 1];
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            matrCopyF[i, j] = matrFS[i, j];
                        }
                    }
                    stackF.Push(matrCopyF);
                }
                else
                {
                    matrCopy = new double[m, n + 1];
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n + 1; j++)
                        {
                            matrCopy[i, j] = matrS[i, j];
                        }
                    }
                    stack.Push(matrCopy);
                }
                gaus();
                s++;
                return;
            }
            if (t > -1)
            {
                switch (t + 1)
                {
                    case 1:
                        if (fract)
                        {
                            matrCopyF = new Fraction[m + 1, n + 1];
                            for (int i = 0; i < m + 1; i++)
                            {
                                for (int j = 0; j < n + 1; j++)
                                {
                                    matrCopyF[i, j] = matrF[i, j];
                                }
                            }
                            stackF.Push(matrCopyF);
                        }
                        else
                        {
                            matrCopy = new double[m + 1, n + 1];
                            for (int i = 0; i < m + 1; i++)
                            {
                                for (int j = 0; j < n + 1; j++)
                                {
                                    matrCopy[i, j] = matr[i, j];
                                }
                            }
                            stack.Push(matrCopy);
                        }
                        strCopy = new string[m + 1, n + 1];
                        for (int i = 0; i < m + 1; i++)
                            strCopy[i, 0] = str[i, 0];
                        for (int j = 0; j < n + 1; j++)
                            strCopy[0, j] = str[0, j];
                        stackS.Push(strCopy);
                        fiveStep();
                        t++;
                        break;
                    case 2:
                        sixStep();
                        t++;
                        if (start)
                            s++;
                        break;
                    default:
                        if (fract)
                        {
                            matrCopyF = new Fraction[mRez, nRez];
                            for (int i = 0; i < mRez; i++)
                            {
                                for (int j = 0; j < nRez; j++)
                                {
                                    matrCopyF[i, j] = matrRezF[i, j];
                                }
                            }
                            stackF.Push(matrCopyF);
                        }
                        else
                        {
                            matrCopy = new double[mRez, nRez];
                            for (int i = 0; i < mRez; i++)
                            {
                                for (int j = 0; j < nRez; j++)
                                {
                                    matrCopy[i, j] = matrRez[i, j];
                                }
                            }
                            stack.Push(matrCopy);
                        }
                        strCopy = new string[mRez, nRez];
                        for (int i = 0; i < mRez; i++)
                            strCopy[i, 0] = strRez[i, 0];
                        for (int j = 0; j < nRez; j++)
                            strCopy[0, j] = strRez[0, j];
                        stackS.Push(strCopy);
                        sevenStep();
                        t++;
                        if (start)
                            s++;
                        break;
                }
            }
            else
            {
                switch (k + 1)
                {
                    case 1:
                        twoStep();
                        k++;
                        break;
                    case 2:
                        threeStep();
                        k++;
                        break;
                    default:
                        if (fract)
                        {
                            matrCopyF = new Fraction[m + 1, n + 1];
                            for (int i = 0; i < m + 1; i++)
                            {
                                for (int j = 0; j < n + 1; j++)
                                {
                                    matrCopyF[i, j] = matrF[i, j];
                                }
                            }
                            stackF.Push(matrCopyF);
                        }
                        else
                        {
                            matrCopy = new double[m + 1, n + 1];
                            for (int i = 0; i < m + 1; i++)
                            {
                                for (int j = 0; j < n + 1; j++)
                                {
                                    matrCopy[i, j] = matr[i, j];
                                }
                            }
                            stack.Push(matrCopy);
                        }
                        strCopy = new string[m + 1, n + 1];
                        for (int i = 0; i < m + 1; i++)
                            strCopy[i, 0] = str[i, 0];
                        for (int j = 0; j < n + 1; j++)
                            strCopy[0, j] = str[0, j];
                        stackS.Push(strCopy);
                        fourStep();
                        k++;
                        break;
                }
            }
        }
    }
}
