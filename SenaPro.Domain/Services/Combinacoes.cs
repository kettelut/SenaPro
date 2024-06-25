using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenaPro.Domain.Services
{
    public class Combinacoes
    {
        public List<List<int>> CalculaCombinacoes(List<int> numeros, int tamanhoSaida)
        {
            var resultado = new List<List<int>>();
            if (numeros == null || numeros.Count == 0 || tamanhoSaida <= 0 || tamanhoSaida > numeros.Count)
            {
                return resultado;
            }

            CalculaCombinacoesRec(numeros, tamanhoSaida, 0, new List<int>(), resultado);
            return resultado;
        }

        private void CalculaCombinacoesRec(List<int> numeros, int tamanhoSaida, int inicio, List<int> atual, List<List<int>> resultado)
        {
            if (atual.Count == tamanhoSaida)
            {
                resultado.Add(new List<int>(atual));
                return;
            }

            for (int i = inicio; i < numeros.Count; i++)
            {
                atual.Add(numeros[i]);
                CalculaCombinacoesRec(numeros, tamanhoSaida, i + 1, atual, resultado);
                atual.RemoveAt(atual.Count - 1); // Remove o último elemento para voltar atrás na recursão
            }
        }
    }
}
