using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BC_Customer
{
    public class clsComunes
    {
        public bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;

            isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
    }
}
