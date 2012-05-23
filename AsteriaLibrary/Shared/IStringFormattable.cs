using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Shared
{
    public interface IStringFormattable
    {
        /// <summary>
        /// Returns a string representing the object instance.
        /// </summary>
        /// <returns></returns>
        string ToFormatString();

        /// <summary>
        /// Parses the given string data and adjusts the instance members accordingly.
        /// </summary>
        /// <param name="data"></param>
        void FromFormatString(string data);
    }
}
