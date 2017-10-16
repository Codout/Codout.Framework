namespace Codout.Framework.NetCore.Data
{

    public abstract class DatabaseCore
    {
        /// <summary>
        /// Executa um comando SQL SELECT no banco de dados conectado.
        /// ATENÇÃO na superclasse DB ela não faz nada!!!
        /// SOMENTE UTILIZAR QUANDO O OBJETO FOR DO TIPO DBCon
        /// </summary>
        /// <param name="sql">Query a ser executada</param>
        public virtual void ExecutaConsulta(string sql)
        {
        }

        /// <summary>
        /// Executa um comando SQL SELECT no banco de dados conectado.
        /// ATENÇÃO na superclasse DB ela não faz nada!!!
        /// SOMENTE UTILIZAR QUANDO O OBJETO FOR DO TIPO DBCon
        /// </summary>
        /// <param name="sql">Query a ser executada</param>
        /// <param name="upperCase">se true, converte para maiúsculo.</param>
        public virtual void ExecutaConsulta(string sql, bool upperCase)
        {
        }

        /// <summary>
        /// Move para o próximo registro.
        /// ATENÇÃO na superclasse DB não faz nada!!!
        /// </summary>
        /// <returns>Retorna true se obtiver sucesso</returns>
        public virtual bool MoveProximo()
        {
            return true;
        }

        /// <summary>
        /// Obtem o valor da coluna indicada, através do número da mesma (inicando em zero).
        /// ATENÇÃO na superclasse DB não faz nada!!!
        /// </summary>
        /// <param name="numero">Número da coluna</param>
        /// <returns>Retorna um objeto com o valor da coluna</returns>
        public virtual object Valor(int numero)
        {
            return null;
        }

        /// <summary>
        /// Obtem o valor da coluna indicada, através do nome da coluna.
        /// ATENÇÃO na superclasse DB não faz nada!!!
        /// </summary>
        /// <param name="nomeColuna">Nome da coluna</param>
        /// <returns>Retorna um objeto com o valor da coluna</returns>
        public virtual object Valor(string nomeColuna)
        {
            return null;
        }

        /// <summary>
        /// Obtem o valor da coluna indicada, através do nome da coluna.
        /// ATENÇÃO na superclasse DB não faz nada!!!
        /// </summary>
        /// <param name="nomeColuna">Nome da coluna</param>
        /// <returns>Retorna um byte[] com o valor da coluna</returns>
        public virtual byte[] ValorBytes(string nomeColuna)
        {
            return null;
        }

        /// <summary>
        /// Obtem o valor da coluna indicada, através do nome da coluna.
        /// ATENÇÃO na superclasse DB não faz nada!!!
        /// </summary>
        /// <param name="numeroColuna">Numero coluna</param>
        /// <returns>Retorna um byte[] com o valor da coluna</returns>
        public virtual byte[] ValorBytes(int numeroColuna)
        {
            return null;
        }
    }
}
