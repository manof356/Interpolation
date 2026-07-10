namespace Interpolation.InterpMath
{
    public class LinearInterpolation
    {
        public static double Linterp(double x1, double y1, double x2, double y2, double x3)
        {
            // бросаем исключение чтобы не было деления на ноль
            if (x1 == x2 && y1 != y2)
                throw new DivideByZeroException("x1 и x2 не могут быть равны");
            if (x3 == x1)
                return y1;
            if (x3 == x2)
                return y2;
            return y1 + (x3 - x1) * ((y2 - y1) / (x2 - x1));
        }

        /*public static List<DataPoint> GetClosedPairs(List<DataPoint> enterTable, double x3)
        {
            // сортируем всю таблицу в порядке возрастания по X
            enterTable = enterTable.OrderBy(p => p.X).ToList();
            List<DataPoint> result = new List<DataPoint>();
            for (int i = 0; i < enterTable.Count - 1; i++)
            {
                if (x3 < enterTable[0].X)
                {
                    result.Add(enterTable[0]);
                    result.Add(enterTable[1]);
                    break;
                }
                else if (x3 > enterTable[^1].X)
                {
                    result.Add(enterTable[^2]); // предполедний
                    result.Add(enterTable[^1]); // последний
                    break;
                }
                else if (enterTable[i].X < x3 && x3 < enterTable[i + 1].X)
                {
                    result.Add(enterTable[i]);
                    result.Add(enterTable[i + 1]);
                    break;
                }
                else if (x3 == enterTable[i].X)
                {
                    result.Add(enterTable[i]);
                    result.Add(enterTable[i]);
                    break;
                }
            }
            return result;
        }*/

        public static List<DataPoint> GetClosedPairs(List<DataPoint> enterTable, double x3)
        {
            // сортируем по возрастанию
            enterTable = enterTable.OrderBy(p => p.X).ToList();
            // x3 меньше или равен минимальному X — берём первые две точки
            if (x3 <= enterTable[0].X)
                return new List<DataPoint> { enterTable[0], enterTable[1] };
            // x3 больше или равен максимальному X — берём последние две точки
            if (x3 >= enterTable[^1].X)
                return new List<DataPoint> { enterTable[^2], enterTable[^1] };
            // ищем пару, между которой лежит x3 (включая границы)
            for (int i = 0; i < enterTable.Count - 1; i++)
            {
                if (enterTable[i].X <= x3 && x3 <= enterTable[i + 1].X)
                    return new List<DataPoint> { enterTable[i], enterTable[i + 1] };
            }
            return null; // сюда не попадём, если данные корректны
        }

        public static (double x1, double y1, double x2, double y2) UnwrapTable(List<DataPoint> table)
        {
            return (table[0].X, table[0].Y, table[1].X, table[1].Y);
        }
    }
}
