using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NemoPark
{
    public interface IIdentifiable<T>
    {
        T Id { get; set; }
    }
}
