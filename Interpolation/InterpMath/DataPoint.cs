using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolation.InterpMath
{
    // простой класс - аналог кортежа для того чтобы хорошо работало с datagrid (Claude так сказал)
    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
