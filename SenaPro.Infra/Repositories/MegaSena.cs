using ClosedXML.Excel;
using SenaPro.Domain.Entities;
using SenaPro.Domain.Repositories;

namespace SenaPro.Infra.Repositories
{
    /// <summary>
    /// Classe responsável por manipular e obter informações dos resultados da Mega-Sena.
    /// </summary>
    public class MegaSena : IMegaSena
    {
        /// <summary>
        /// Caminho para o arquivo XLSX contendo os resultados dos concursos realizados.
        /// </summary>
        private string _filePath;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MegaSena"/> com o caminho padrão para o arquivo de resultados.
        /// </summary>
        public MegaSena()
        {
            _filePath = @"C:\Projetos\SenaPro\SenaPro.Infra\Files\Mega-Sena.xlsx";
        }

        /// <summary>
        /// Obtém a lista de sorteios da Mega-Sena a partir do arquivo XLSX.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Sorteio"/> contendo os resultados dos sorteios.</returns>
        public List<Sorteio> ObterSorteios() 
        {
            var workbook = new XLWorkbook(_filePath);
            var worksheet = workbook.Worksheet(1);
            var response = new List<Sorteio>();

            foreach (var row in worksheet.RowsUsed())
            {
                var sorteio = new Sorteio();
                sorteio.NumeroConcurso = Convert.ToInt32(row.Cell(1).Value.ToString().Replace("Number", ""));
                sorteio.DataRealizacao = System.DateTime.ParseExact(row.Cell(2).Value.ToString().Replace("Number", ""), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                for (int i = 0; i < 6; i++)
                    sorteio.NumerosSorteados.Add(Convert.ToInt32(row.Cell(i + 3).Value.ToString().Replace("Number", "")));
                
                sorteio.HouveGanhadores = !Convert.ToInt32(row.Cell(9).Value.ToString().Replace("Number", "")).Equals(0);
                response.Add(sorteio);
            }

            return response.OrderBy(x => x.NumeroConcurso).ToList();
        }
    }
}
