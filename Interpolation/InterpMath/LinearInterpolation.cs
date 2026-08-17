namespace Interpolation.InterpMath
{
    // Класс расчета интерполяции. Формулы
    public class LinearInterpolation
    {
        /// <summary>
        /// Подсчитывает интерполяцию для списка значений. 
        /// </summary>
        /// <param name="enterTable">Таблица из кортежей на основе которой происходит расчёт</param>
        /// <param name="xes">Таблица аргументов для расчета интерполяции</param>
        /// <returns>Список значений</returns>
        public static List<double> LinterpList(List<DataPoint> enterTable ,List<double> xes)
        {
            List<double> result = new List<double>(); // пустой список результата
            foreach (double x in xes) // для каждого аргумента
            {
                result.Add(Linterp(enterTable,x)); // считаем интерполицию и добавляем в список результата
            }
            return result;
        }

        /// <summary>
        /// Подсчитывает интеполяцию для одного значения
        /// </summary>
        /// <param name="enterTable">Таблица из кортежей на основе которой происходит расчёт</param>
        /// <param name="x">Аргемунт для которого считается интерполяция</param>
        /// <returns>Значение интерполяции</returns>
        public static double Linterp(List<DataPoint> enterTable, double x)
        {
            List<DataPoint> closedPairs = GetClosedPairs(enterTable, x); // получаем пару ближайших к аргументу кортежей
            (double x1, double y1, double x2, double y2) = UnwrapTable(closedPairs); // разворачиваем кортеж в 4 переменных и
            return PlainLinterp(x1, y1, x2, y2, x); // передаём их в функцию расчета интерполяции и возвращаем
        }

        /// <summary>
        /// Примитивная функция расчета интерполяции на основе 4 аргументов и искомого
        /// </summary>
        /// <param name="x1">Аргумент X1</param>
        /// <param name="y1">Значение Y1</param>
        /// <param name="x2">Аргумент X2</param>
        /// <param name="y2">Значение Y2</param>
        /// <param name="x3">Искомый аргумент X3</param>
        /// <returns>Значение Y3</returns>
        /// <exception cref="DivideByZeroException"></exception>
        public static double PlainLinterp(double x1, double y1, double x2, double y2, double x3)
        {
            if (x1 == x2 && y1 != y2) // бросаем исключение чтобы не было деления на ноль
                throw new DivideByZeroException("x1 и x2 не могут быть равны");
            if (x3 == x1)
                return y1;
            if (x3 == x2)
                return y2;
            return y1 + (x3 - x1) * ((y2 - y1) / (x2 - x1));
        }

        /// <summary>
        /// Метод получает ближайшие к x аргументы и возвращает кортежи (аргумент - значение)
        /// </summary>
        /// <param name="enterTable">Входная таблица</param>
        /// <param name="x">Аргумент для поиска значения</param>
        /// <returns>Список ближайших по аргументу к x кортежей</returns>
        public static List<DataPoint> GetClosedPairs(List<DataPoint> enterTable, double x)
        {
            // сортируем исходную таблицу по возрастанию
            enterTable = enterTable.OrderBy(p => p.X).ToList();
            // X меньше или равен минимальному X — берём первые две точки
            if (x <= enterTable[0].X)
                return new List<DataPoint> { enterTable[0], enterTable[1] };
            // X больше или равен максимальному X — берём последние две точки
            if (x >= enterTable[^1].X)
                return new List<DataPoint> { enterTable[^2], enterTable[^1] };
            // ищем пару, между которой лежит X (включая границы)
            for (int i = 0; i < enterTable.Count - 1; i++)
            {
                if (enterTable[i].X <= x && x <= enterTable[i + 1].X)
                    return new List<DataPoint> { enterTable[i], enterTable[i + 1] };
            }
            return null; // сюда не попадём, если данные корректны
        }

        /// <summary>
        /// Распаковывает список кортежей в 4 переменных
        /// </summary>
        /// <param name="table">Список кортежей (ближайшая к искомому аргументу пара кортежей)</param>
        /// <returns>Значения списка кортежей в порядке X1 Y1 X2 Y2</returns>
        public static (double x1, double y1, double x2, double y2) UnwrapTable(List<DataPoint> table)
        {
            return (table[0].X, table[0].Y, table[1].X, table[1].Y);
        }
    }
}
