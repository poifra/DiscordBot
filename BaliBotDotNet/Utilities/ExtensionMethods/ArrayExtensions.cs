using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Utilities.ExtensionMethods
{
    public static class ArrayExtensions
    {
        public static T[] GetRow<T>(this T[][] matrix, int rowNumber) =>    
             matrix[rowNumber].Select(x => x).ToArray();
        
    }
}
