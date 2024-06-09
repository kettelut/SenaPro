using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenaPro.Domain.Entities
{
    public class CalculoSorteiosAnteriores
    {
        public int QntDeNumeros { get; set; }
        public int QntMaximaDeSorteios { get; set; }
        public int QntMinimaDeSorteios { get; set; }
        public int QntMediaDeSorteios { get; set; }
    }
}
