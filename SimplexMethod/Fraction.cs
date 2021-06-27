using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplexMethod
{
    public class Fraction
    {
        private int numerator;              // Числитель
        private int denominator;            // Знаменатель
        private int sign;					// Знак

        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("В знаменателе не может быть нуля");
            }
            this.numerator = Math.Abs(numerator);
            this.denominator = Math.Abs(denominator);
            if (numerator * denominator < 0)
            {
                this.sign = -1;
            }
            else
            {
                this.sign = 1;
            }
        }
        // Вызов первого конструктора со знаменателем равным единице
        public Fraction(int number) : this(number, 1) { }

        // Возвращает наибольший общий делитель (Алгоритм Евклида)
        private static int getGreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        // Возвращает наименьшее общее кратное
        private static int getLeastCommonMultiple(int a, int b)
        {
            // В формуле опущен модуль, так как в классе
            // числитель всегда неотрицательный, а знаменатель -- положительный
            // ...
            // Деление здесь -- целочисленное, что не искажает результат, так как
            // числитель и знаменатель делятся на свои делители,
            // т.е. при делении не будет остатка
            return a * b / getGreatestCommonDivisor(a, b);
        }

        // Метод создан для устранения повторяющегося кода в методах сложения и вычитания дробей.
        // Возвращает дробь, которая является результатом сложения или вычитания дробей a и b,
        // В зависимости от того, какая операция передана в параметр operation.
        // P.S. использовать только для сложения и вычитания
        private static Fraction performOperation(Fraction a, Fraction b, Func<int, int, int> operation)
        {
            // Наименьшее общее кратное знаменателей
            int leastCommonMultiple = getLeastCommonMultiple(a.denominator, b.denominator);
            // Дополнительный множитель к первой дроби
            int additionalMultiplierFirst = leastCommonMultiple / a.denominator;
            // Дополнительный множитель ко второй дроби
            int additionalMultiplierSecond = leastCommonMultiple / b.denominator;
            // Результат операции
            int operationResult = operation(a.numerator * additionalMultiplierFirst * a.sign,
            b.numerator * additionalMultiplierSecond * b.sign);
            return new Fraction(operationResult, a.denominator * additionalMultiplierFirst);
        }

        // Перегрузка оператора "+" для случая сложения двух дробей
        public static Fraction operator +(Fraction a, Fraction b)
        {
            return performOperation(a, b, (int x, int y) => x + y);
        }
        // Перегрузка оператора "+" для случая сложения дроби с числом
        public static Fraction operator +(Fraction a, int b)
        {
            return a + new Fraction(b);
        }
        // Перегрузка оператора "+" для случая сложения числа с дробью
        public static Fraction operator +(int a, Fraction b)
        {
            return b + a;
        }
        // Перегрузка оператора "-" для случая вычитания двух дробей
        public static Fraction operator -(Fraction a, Fraction b)
        {
            return performOperation(a, b, (int x, int y) => x - y);
        }
        // Перегрузка оператора "-" для случая вычитания из дроби числа
        public static Fraction operator -(Fraction a, int b)
        {
            return a - new Fraction(b);
        }
        // Перегрузка оператора "-" для случая вычитания из числа дроби
        public static Fraction operator -(int a, Fraction b)
        {
            return b - a;
        }
        // Перегрузка оператора "*" для случая произведения двух дробей
        public static Fraction operator *(Fraction a, Fraction b)
        {
            return new Fraction(a.numerator * a.sign * b.numerator * b.sign, a.denominator * b.denominator);
        }
        // Перегрузка оператора "*" для случая произведения дроби и числа
        public static Fraction operator *(Fraction a, int b)
        {
            return a * new Fraction(b);
        }
        // Перегрузка оператора "*" для случая произведения числа и дроби
        public static Fraction operator *(int a, Fraction b)
        {
            return b * a;
        }
        // Перегрузка оператора "/" для случая деления двух дробей
        public static Fraction operator /(Fraction a, Fraction b)
        {
            return a * b.GetReverse();
        }
        // Перегрузка оператора "/" для случая деления дроби на число
        public static Fraction operator /(Fraction a, int b)
        {
            return a / new Fraction(b);
        }
        // Перегрузка оператора "/" для случая деления числа на дробь
        public static Fraction operator /(int a, Fraction b)
        {
            return new Fraction(a) / b;
        }
        // Перегрузка оператора "унарный минус"
        public static Fraction operator -(Fraction a)
        {
            return a.GetWithChangedSign();
        }
        // Перегрузка оператора "++"
        public static Fraction operator ++(Fraction a)
        {
            return a + 1;
        }
        // Перегрузка оператора "--"
        public static Fraction operator --(Fraction a)
        {
            return a - 1;
        }

        // Возвращает дробь, обратную данной
        private Fraction GetReverse()
        {
            return new Fraction(this.denominator * this.sign, this.numerator);
        }
        // Возвращает дробь с противоположным знаком
        private Fraction GetWithChangedSign()
        {
            return new Fraction(-this.numerator * this.sign, this.denominator);
        }

        // Мой метод Equals
        public bool Equals(Fraction that)
        {
            Fraction a = this.Reduce();
            Fraction b = that.Reduce();
            return a.numerator == b.numerator &&
            a.denominator == b.denominator &&
            a.sign == b.sign;
        }
        // Переопределение метода Equals
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is Fraction)
            {
                result = this.Equals(obj as Fraction);
            }
            return result;
        }
        // Переопределение метода GetHashCode
        public override int GetHashCode()
        {
            return this.sign * (this.numerator * this.numerator + this.denominator * this.denominator);
        }

        // Перегрузка оператора "Равенство" для двух дробей
        public static bool operator ==(Fraction a, Fraction b)
        {
            // Приведение к Object необходимо для того, чтобы
            // можно было сравнивать дроби с null.
            // Обычное сравнение a.Equals(b) в данном случае не подходит,
            // так как если a есть null, то у него нет метода Equals,
            // следовательно будет выдано исключение, а если
            // b окажется равным null, то исключение будет вызвано в
            // методе this.Equals
            Object aAsObj = a as Object;
            Object bAsObj = b as Object;
            if (aAsObj == null || bAsObj == null)
            {
                return aAsObj == bAsObj;
            }
            return a.Equals(b);
        }
        // Перегрузка оператора "Равенство" для дроби и числа
        public static bool operator ==(Fraction a, int b)
        {
            return a == new Fraction(b);
        }
        // Перегрузка оператора "Равенство" для числа и дроби
        public static bool operator ==(int a, Fraction b)
        {
            return new Fraction(a) == b;
        }
        // Перегрузка оператора "Неравенство" для двух дробей
        public static bool operator !=(Fraction a, Fraction b)
        {
            return !(a == b);
        }
        // Перегрузка оператора "Неравенство" для дроби и числа
        public static bool operator !=(Fraction a, int b)
        {
            return a != new Fraction(b);
        }
        // Перегрузка оператора "Неравенство" для числа и дроби
        public static bool operator !=(int a, Fraction b)
        {
            return new Fraction(a) != b;
        }

        // Метод сравнения двух дробей
        // Возвращает	 0, если дроби равны
        //				 1, если this больше that
        //				-1, если this меньше that
        private int CompareTo(Fraction that)
        {
            if (this.Equals(that))
            {
                return 0;
            }
            Fraction a = this.Reduce();
            Fraction b = that.Reduce();
            if (a.numerator * a.sign * b.denominator > b.numerator * b.sign * a.denominator)
            {
                return 1;
            }
            return -1;
        }

        // Перегрузка оператора ">" для двух дробей
        public static bool operator >(Fraction a, Fraction b)
        {
            return a.CompareTo(b) > 0;
        }
        // Перегрузка оператора ">" для дроби и числа
        public static bool operator >(Fraction a, int b)
        {
            return a > new Fraction(b);
        }
        // Перегрузка оператора ">" для числа и дроби
        public static bool operator >(int a, Fraction b)
        {
            return new Fraction(a) > b;
        }
        // Перегрузка оператора "<" для двух дробей
        public static bool operator <(Fraction a, Fraction b)
        {
            return a.CompareTo(b) < 0;
        }
        // Перегрузка оператора "<" для дроби и числа
        public static bool operator <(Fraction a, int b)
        {
            return a < new Fraction(b);
        }
        // Перегрузка оператора "<" для числа и дроби
        public static bool operator <(int a, Fraction b)
        {
            return new Fraction(a) < b;
        }
        // Перегрузка оператора ">=" для двух дробей
        public static bool operator >=(Fraction a, Fraction b)
        {
            return a.CompareTo(b) >= 0;
        }
        // Перегрузка оператора ">=" для дроби и числа
        public static bool operator >=(Fraction a, int b)
        {
            return a >= new Fraction(b);
        }
        // Перегрузка оператора ">=" для числа и дроби
        public static bool operator >=(int a, Fraction b)
        {
            return new Fraction(a) >= b;
        }
        // Перегрузка оператора "<=" для двух дробей
        public static bool operator <=(Fraction a, Fraction b)
        {
            return a.CompareTo(b) <= 0;
        }
        // Перегрузка оператора "<=" для дроби и числа
        public static bool operator <=(Fraction a, int b)
        {
            return a <= new Fraction(b);
        }
        // Перегрузка оператора "<=" для числа и дроби
        public static bool operator <=(int a, Fraction b)
        {
            return new Fraction(a) <= b;
        }

        // Возвращает сокращенную дробь
        public Fraction Reduce()
        {
            Fraction result = this;
            int greatestCommonDivisor = getGreatestCommonDivisor(this.numerator, this.denominator);
            result.numerator /= greatestCommonDivisor;
            result.denominator /= greatestCommonDivisor;
            return result;
        }
        // Переопределение метода ToString
        public override string ToString()
        {
            if (this.numerator == 0)
            {
                return "0";
            }
            string result;
            if (this.sign < 0)
            {
                result = "-";
            }
            else
            {
                result = "";
            }
            if (this.numerator == this.denominator)
            {
                return result + "1";
            }
            if (this.denominator == 1)
            {
                return result + this.numerator;
            }
            return result + this.numerator + "/" + this.denominator;
        }


        public string Fin()
        {
            int i = 0;
            if (numerator > denominator)
            {
                i = numerator / denominator;
                Fraction ost = new Fraction(numerator - (denominator * i), denominator);
                if (sign == 1)
                    return i.ToString() + "+" + ost.ToString();
                else 
                    return "-("+ i.ToString() + "+" + ost.ToString() + ")";
            }
            else
                return this.ToString();
        }
    }
}
