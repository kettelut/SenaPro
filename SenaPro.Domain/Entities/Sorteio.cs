namespace SenaPro.Domain.Entities
{
    /// <summary>
    /// Entidade de um Sorteio da Mega-Sena
    /// </summary>
    public class Sorteio
    {
        /// <summary>
        /// Número do Concurso
        /// </summary>
        public int NumeroConcurso { get; set; }

        /// <summary>
        /// Data de Realização
        /// </summary>
        public DateTime DataRealizacao { get; set; }

        /// <summary>
        /// Lista de números sorteados
        /// </summary>
        public List<int> NumerosSorteados { get; set; }

        /// <summary>
        /// Indicador se houve ganhadores com as 6 dezenas
        /// </summary>
        public bool HouveGanhadores { get; set; }

        /// <summary>
        /// Construtor da entidade do Sorteio
        /// </summary>
        public Sorteio() 
        { 
            NumerosSorteados = new List<int>();
        }
    }
}
