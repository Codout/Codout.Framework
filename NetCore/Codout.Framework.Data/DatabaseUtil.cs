using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Codout.Framework.NetCore.Data
{
    public abstract class DatabaseUtil : DatabaseCore
    {

        #region Atributos locais
        /// <summary>
        /// Nome da classe para identificação
        /// </summary>
        private readonly string _className = "Codout.Framework.NetCore.Data.DatabaseUtil.";

        /// <summary>
        /// indica se o reader está aberto ou não
        /// </summary>
        private bool _readerAberto;

        /// <summary>
        /// indicador para controlar a primeira execução de um comando preparado
        /// </summary>
        private bool _primeiroExecutaComandoPrep = true;

        private DbCommand _comando;
        private DbDataReader _reader;
        private DbTransaction _transacao;
        #endregion

        #region Construtor / Destrutor
        /// <summary>
        /// Construtor com conexão
        /// </summary>
        /// <param name="objDbConnection">Objeto defidamente tipado da conexão</param>
        protected DatabaseUtil(DbConnection objDbConnection)
        {

            Conexao = objDbConnection;
        }

        /// <summary>
        /// Destrutor, libera a conexão com o banco de dados caso ainda não esteja feito.
        /// </summary>
        ~DatabaseUtil()
        {
            Desconecta();
        }

        public DataTable GetSchema()
        {
            return Conexao.GetSchema();
        }
        #endregion

        #region Conexão / Desconexão
        /// <summary>
        /// Efetua a conexão com o banco de dados, já disponiblizando os métodos de consultas através de reader e comandos (comand).
        /// </summary>
        /// <param name="stringConexao">Indica a string da conexão a ser utilizada para conectar no banco</param>
        public void Conecta(string stringConexao)
        {
            try
            {

                if (Conectado)
                    Conexao.Close();
                Conexao.ConnectionString = stringConexao;
                Conexao.Open();
                _comando = Conexao.CreateCommand();
                Conectado = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "Conecta()", ex);
            }

        }

        /// <summary>
        /// Realiza a desconexão com o banco de dados.
        /// </summary>
        public void Desconecta()
        {
            try
            {
                if (Conectado)
                    Conexao.Close();
                Conexao.Dispose();
                _comando.Dispose();
                Conectado = false;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "Desconecta()", ex);
            }
        }
        #endregion

        #region Controle de Transações
        /// <summary>
        /// Inicia uma transação na conexão atual.
        /// </summary>
        public void IniciaTransacao()
        {
            try
            {
                //se o reader estiver aberto é preciso fechar, se não dá erro
                if (_readerAberto)
                {
                    if (!_reader.IsClosed) _reader = null;
                    _readerAberto = false;
                }

                if (Conectado)
                {
                    _transacao = Conexao.BeginTransaction(IsolationLevel.ReadCommitted);
                    _comando.Transaction = _transacao;
                }
                else
                {
                    throw new Exception("Transação não pode ser iniciada, sem a conexão com o banco, utilize antes DbCon.Conecta()");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "IniciaTransacao()", ex);
            }
        }

        /// <summary>
        /// Efetua o commit (confirmando toda transação) no banco de dados, dos comandos executados após o início da transação com o método IniciaTransacao.
        /// </summary>
        public void ConfirmaTransacao()
        {
            try
            {
                //se o reader estiver aberto é preciso fechar, se não dá erro
                if (_readerAberto)
                {
                    if (!_reader.IsClosed) _reader = null;
                    _readerAberto = false;
                }
                if (_transacao != null)
                    _transacao.Commit();
                else
                {
                    throw new Exception("Transação não pode ser confimada, provavelmente não foi iniciada.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ConfirmaTransacao()", ex);
            }
        }

        /// <summary>
        /// Efetua o rollback nos comandos realizados após o início da transação com o método IniciaTransacao.
        /// </summary>
        public void CancelaTransacao()
        {
            try
            {
                //se o reader estiver aberto é preciso fechar, se não dá erro
                if (_readerAberto)
                {
                    if (!_reader.IsClosed) _reader = null;
                    _readerAberto = false;
                }
                if (_transacao != null)
                    _transacao.Rollback();
                else
                {
                    throw new Exception("Transação não pode ser cancelada, provavelmente não foi iniciada.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "CancelaTransacao()", ex);
            }
        }
        #endregion

        #region Execução de comandos (INSERT, UPDATE e DELETE)
        /// <summary>
        /// Executa um comando SQL no banco de dados conectado
        /// Somente comandos (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="sql">Comando SQL a ser executado</param>
        /// <returns>returna o número de linhas afetadas pelo commando SQL</returns>
        public int ExecutaComando(string sql)
        {
            return ExecutaComando(sql, true);
        }

        /// <summary>
        /// Executa um comando SQL no banco de dados conectado
        /// Somente comandos (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="sql">Comando SQL a ser executado</param>
        /// <param name="upperCase">true se deseja converter a query para maiúsculas</param>
        /// <returns>returna o número de linhas afetadas pelo commando SQL</returns>
        public int ExecutaComando(string sql, bool upperCase)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            //se o reader estiver aberto é preciso fechar, se não dá erro
            if (_readerAberto)
            {
                if (!_reader.IsClosed) _reader = null;
                _readerAberto = false;
            }

            int linhas;
            try
            {
                _comando.CommandText = (upperCase) ? sql.ToUpper() : sql;
                _comando.Parameters.Clear();
                linhas = _comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ExecutaComando()", ex);
            }

            return linhas;
        }

        /// <summary>
        /// Pepara o objeto command para executar querys parametrizadas, deve ser utilizada antes do método ExecutaComandoPrep, pois inicializa os parametros.
        /// </summary>
        /// <param name="sql">Comando SQL a ser executado</param>
        /// /// <param name="camposKeyValuePair">Lista de camos com nome(chave) e tipo(valor) do campo</param>
        public void PreparaComando(string sql, List<KeyValuePair<string, DbType>> camposKeyValuePair)
        {
            PreparaComando(sql, camposKeyValuePair, true);
        }

        /// <summary>
        /// Pepara o objeto command para executar querys parametrizadas, deve ser utilizada antes do método ExecutaComandoPrep, pois inicializa os parametros.
        /// </summary>
        /// <param name="sql">Comando SQL a ser executado</param>
        /// <param name="camposKeyValuePair">Lista de camos com nome(chave) e tipo(valor) do campo</param>
        /// <param name="upperCase">se true, converte a query para maiúsculos</param>
        public void PreparaComando(string sql, List<KeyValuePair<string, DbType>> camposKeyValuePair, bool upperCase)
        {

            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {

                _comando.CommandText = (upperCase) ? sql.ToUpper() : sql;
                _comando.Parameters.Clear();

                foreach (var campo in camposKeyValuePair)
                {
                    var parametro = _comando.CreateParameter();
                    parametro.ParameterName = campo.Key;
                    parametro.DbType = campo.Value;
                    _comando.Parameters.Add(parametro);
                }

                _primeiroExecutaComandoPrep = true;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "PreparaComando()", ex);
            }
        }

        /// <summary>
        /// Executa comandos (INSERT, UPDATE e DELETE) parametrizados.
        /// os valores devem estar na seqüência utilizada na chamada ao método PreparaComando().
        /// </summary>
        /// <param name="valores">Valores para serem utilizados na execução do comando parametrizado</param>
        public void ExecutaComandoPrep(List<object> valores)
        {
            ExecutaComandoPrep(valores, true);
        }

        /// <summary>
        /// Executa comandos (INSERT, UPDATE e DELETE) parametrizados.
        /// os valores devem estar na seqüência utilizada na chamada ao método PreparaComando().
        /// </summary>
        /// <param name="valores">Valores para serem utilizados na execução do comando parametrizado</param>
        /// <param name="upperCase">se true, converte os valores para maiúsculos conforme o caso.</param>
        public void ExecutaComandoPrep(List<object> valores, bool upperCase)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                var i = 0;
                foreach (var valor in valores)
                {
                    //se o valor estiver nulo, então vamos setar o valor nulo também
                    _comando.Parameters[i].Value = valor.Equals(null) ? DBNull.Value : valor;
                    i++;
                }

                //se for a primeira vez então devemos executar o Prepare antes de tudo
                if (_primeiroExecutaComandoPrep)
                {
                    _primeiroExecutaComandoPrep = false;

                    //se o reader estiver aberto é preciso fechar, se não dá erro
                    if (_readerAberto)
                    {
                        if (!_reader.IsClosed) _reader = null;
                        _readerAberto = false;
                    }

                    _comando.Prepare();
                }

                _comando.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ExecutaComandoPrep()", ex);
            }
        }
        #endregion

        #region Execução de comandos com PREPARE e EXECUÇÃO DIRETAMENTE
        /// <summary>
        /// Executa comandos (INSERT, UPDATE e DELETE) parametrizados.
        /// Já executa o prepare e o comando diretamente, não precisa utilizar o método PreparaComando antes,
        /// até porque não tem efeito nenhum :)
        /// </summary>
        /// <param name="sql">Query a ser executada, com parametro sinalizados com '@'</param>
        /// <param name="camposKeyValuePair">Vetor de chave(nome campo)/valor(dbType do banco) de parametros da query, NÃO inserir '@' antes de cada nome de campo</param>
        /// <param name="valores">Valores para serem utilizados na execução do comando parametrizado</param>
        public void ExecutaComandoParametrizado(string sql, List<KeyValuePair<string, DbType>> camposKeyValuePair, List<object> valores)
        {
            ExecutaComandoParametrizado(sql, camposKeyValuePair, valores, true);
        }

        /// <summary>
        /// Executa comandos (INSERT, UPDATE e DELETE) parametrizados.
        /// Já executa o prepare e o comando diretamente, não precisa utilizar o método PreparaComando antes,
        /// até porque não tem efeito nenhum :)
        /// </summary>
        /// <param name="sql">Query a ser executada, com parametro sinalizados com '@'</param>
        /// <param name="camposKeyValuePair">Vetor de chave(nome campo)/valor(dbType do banco) de parametros da query, NÃO inserir '@' antes de cada nome de campo</param>
        /// <param name="valores">Valores para serem utilizados na execução do comando parametrizado</param>
        /// <param name="upperCase">indica se coloca o sql em uppercase (maíusculas)</param>
        public void ExecutaComandoParametrizado(string sql, List<KeyValuePair<string, DbType>> camposKeyValuePair, List<object> valores, bool upperCase)
        {
            ExecutaComandoParametrizado(sql, camposKeyValuePair, valores, upperCase, false);
        }

        /// <summary>
        /// Executa comandos (INSERT, UPDATE e DELETE) parametrizados.
        /// Já executa o prepare e o comando diretamente, não precisa utilizar o método PreparaComando antes,
        /// até porque não tem efeito nenhum :)
        /// </summary>
        /// <param name="sql">Query a ser executada, com parametro sinalizados com '@'</param>
        /// <param name="camposKeyValuePair">Vetor de chave(nome campo)/valor(dbType do banco) de parametros da query, NÃO inserir '@' antes de cada nome de campo</param>
        /// <param name="valores">Valores para serem utilizados na execução do comando parametrizado</param>
        /// <param name="upperCase">se true, converte a query para maiúsculas</param>
        /// <param name="isConsultaSql">indica se é uma consulta (SELECT)</param>
        internal void ExecutaComandoParametrizado(string sql, List<KeyValuePair<string, DbType>> camposKeyValuePair, List<object> valores, bool upperCase, bool isConsultaSql)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {

                var fullName = Conexao.GetType().FullName;
                var simboloParametro = "@";
                //quandor for Oracle ou PostGreesql, trocamos @ por :
                if (fullName.Contains("Oracle") || fullName.Contains("Npgsql"))
                {
                    simboloParametro = ":";
                    sql = sql.Replace("@", simboloParametro);
                }

                _comando.CommandText = (upperCase) ? sql.ToUpper() : sql;
                _comando.Parameters.Clear();

                var i = 0;
                foreach (var campo in camposKeyValuePair)
                {
                    var parametro = _comando.CreateParameter();
                    parametro.ParameterName = simboloParametro + campo.Key;
                    parametro.DbType = campo.Value;
                    parametro.Value = valores[i].Equals(null) ? DBNull.Value : valores[i];

                    _comando.Parameters.Add(parametro);
                    i++;
                }

                //se for a primeira vez então devemos executar o Prepare antes de tudo
                //se o reader estiver aberto é preciso fechar, se não dá erro
                if (_readerAberto)
                {
                    if (!_reader.IsClosed) _reader = null;
                    _readerAberto = false;
                }

                _comando.Prepare();

                if (isConsultaSql)
                {
                    _reader = _comando.ExecuteReader();
                    _readerAberto = true;
                }
                else
                    _comando.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ExecutaComandoParametrizado()", ex);
            }
        }
        #endregion

        #region Execução de Consultas
        /// <summary>
        /// Executa um comando SQL SELECT COUNT no banco de dados conectado.
        /// Retorna um valor através do ExecuteScalar, que é mais rápido pois busca apenas a primeira linha/coluna do cursor retornado pelo banco.
        /// </summary>
        /// <param name="sql">Query a ser executada</param>
        /// <returns>Retorna o resultado da contagem</returns>
        public int ExecutaCount(string sql)
        {
            return ExecutaCount(sql, true);
        }

        /// <summary>
        /// Executa um comando SQL SELECT COUNT no banco de dados conectado.
        /// Retorna um valor através do ExecuteScalar, que é mais rápido pois busca apenas a primeira linha/coluna do cursor retornado pelo banco.
        /// </summary>
        /// <param name="sql">Query a ser executada</param>
        /// <param name="upperCase">se true, converte a query para maiúsculas</param>
        /// <returns>Retorna o resultado da contagem</returns>
        public int ExecutaCount(string sql, bool upperCase)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                {
                    if (!_reader.IsClosed) _reader = null;
                    _readerAberto = false;
                }
                _comando.Parameters.Clear();
                _comando.CommandText = sql.ToUpper();
                return (int)_comando.ExecuteScalar();
            }
            catch (Exception ex)
            {
                _readerAberto = false;
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ExecutaCount()", ex);
            }
        }

        /// <summary>
        /// Executa um comando SQL SELECT no banco de dados conectado e prepara um cursor (DataReader) para ser lido.
        /// Vale lembrar que o datareader somente permite leitura para frente.
        /// </summary>
        /// <param name="sql">Query a ser executada</param>
        public override void ExecutaConsulta(string sql)
        {
            ExecutaConsulta(sql, true);
        }

        /// <summary>
        /// Executa um comando SQL SELECT no banco de dados conectado e prepara um cursor (DataReader) para ser lido.
        /// Vale lembrar que o datareader somente permite leitura para frente.
        /// </summary>
        /// <param name="sql">Query a ser executada</param>
        /// <param name="upperCase">se true, converte a query para maiúsculas</param>
        public override void ExecutaConsulta(string sql, bool upperCase)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                {
                    if (!_reader.IsClosed) _reader = null;
                    _readerAberto = false;
                }
                _comando.Parameters.Clear();
                _comando.CommandText = (upperCase) ? sql.ToUpper() : sql;
                _reader = _comando.ExecuteReader();
                _readerAberto = true;
            }
            catch (Exception ex)
            {
                _readerAberto = false;
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ExecutaConsulta()", ex);
            }
        }

        /// <summary>
        /// Executa uma SELECT com parâmetros indicados por @, exemplo: SELECT * FROM TESTE WHERE DATA = @DATAFILTRO
        /// </summary>
        /// <param name="sql">Query a ser executada, com parametro sinalizados com '@'</param>
        /// <param name="camposKeyValuePair">Vetor de chave(nome campo)/valor(dbType do banco) de parametros da query, NÃO inserir '@' antes de cada nome de campo</param>
        /// <param name="valores">Valores para serem utilizados na execução do comando parametrizado</param>
        public void ExecutaConsultaParametrizada(string sql, List<KeyValuePair<string, DbType>> camposKeyValuePair, List<object> valores)
        {
            ExecutaComandoParametrizado(sql, camposKeyValuePair, valores, true, true);
        }

        /// <summary>
        /// Libera o datareader caso esteja aberto
        /// </summary>
        internal void LiberaDataReader()
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                {
                    if (!_reader.IsClosed) _reader = null;
                    _readerAberto = false;
                }
            }
            catch (Exception ex)
            {
                _readerAberto = false;
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "LiberaDataReader()", ex);
            }
        }
        #endregion

        #region Navegação e busca de dados
        /// <summary>
        /// Move o datareader para o próximo registro
        /// </summary>
        /// <returns>Retorna true se obtiver sucesso</returns>
        public override bool MoveProximo()
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                    if (_reader.Read())
                        return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "MoveProximo()", ex);
            }

            return false;
        }

        /// <summary>
        /// Obtem o valor da coluna indicada, através do número da mesma (inicando em zero).
        /// </summary>
        /// <param name="numero">Número da coluna</param>
        /// <returns>Retorna um objeto com o valor da coluna</returns>
        public override object Valor(int numero)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                    return _reader.GetValue(numero);
                else
                {
                    throw new Exception("O reader não está aberto, provavelmente não foi executado o método ExecutaConsulta");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "Valor()", ex);
            }
        }

        /// <summary>
        /// Obtem um vetor de bytes do conteúdo da coluna, através do nome da coluna.
        /// </summary>
        /// <param name="nomeColuna">Nome da coluna</param>
        /// <returns>Retorna um vetor de bytes com o valor da coluna</returns>
        public override byte[] ValorBytes(string nomeColuna)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                {
                    return ValorBytes(_reader.GetOrdinal(nomeColuna));
                }
                throw new Exception("O reader não está aberto, provavelmente não foi executado o método ExecutaConsulta");
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ValorBytes()", ex);
            }
        }


        /// <summary>
        /// Obtem um vetor de bytes do conteúdo da coluna, através do número da mesma (inicando em zero).
        /// </summary>
        /// <param name="numeroColuna">Número da coluna</param>
        /// <returns>Retorna um vetor de bytes com o valor da coluna</returns>
        public override byte[] ValorBytes(int numeroColuna)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                {
                    return (byte[])_reader.GetValue(numeroColuna);
                }
                else
                {
                    throw new Exception("O reader não está aberto, provavelmente não foi executado o método ExecutaConsulta");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ValorBytes()", ex);
            }
        }

        /// <summary>
        /// Obtem o valor da coluna indicada, através do nome da coluna.
        /// </summary>
        /// <param name="nomeColuna">Nome da coluna</param>
        /// <returns>Retorna um objeto com o valor da coluna</returns>
        public override object Valor(string nomeColuna)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                    return _reader.GetValue(_reader.GetOrdinal(nomeColuna));
                else
                {
                    throw new Exception("O reader não está aberto, provavelmente não foi executado o método ExecutaConsulta");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "Valor()", ex);
            }
        }

        /// <summary>
        /// Obtem o número de campos retornados no DataReader atual.
        /// </summary>
        public int NumeroCampos()
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                    return _reader.FieldCount;
                else
                {
                    throw new Exception("O reader não está aberto, provavelmente não foi executado o método ExecutaConsulta");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "NumeroCampos()", ex);
            }
        }

        /// <summary>
        /// Obtem o tipo de dados da coluna indicada, no DataReader atual.
        /// </summary>
        /// <param name="nomeColuna">Nome da coluna a ser verificada</param>
        /// <returns>System.Type do campo</returns>
        public Type ObtemTipoCampo(string nomeColuna)
        {
            if (!Conectado)
            {
                throw new Exception("Comando não pode ser executado sem antes conectar ao banco, use DbCon.Conecta!");
            }

            try
            {
                if (_readerAberto)
                    return _reader.GetFieldType(_reader.GetOrdinal(nomeColuna));
                else
                {
                    throw new Exception("O reader não está aberto, provavelmente não foi executado o método ExecutaConsulta");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message + Environment.NewLine + "Origem->" + _className + "ObtemTipoCampo()", ex);
            }
        }
        #endregion

        #region Propriedades
        /// <summary>
        /// Propriedade que indica se está conectado no banco de dados.
        /// </summary>
        public bool Conectado { get; private set; }

        /// <summary>
        /// Propriedade que retorna o objeto DbConnection utilizado no momento.
        /// </summary>
        public DbConnection Conexao { get; }
        #endregion

    }
}
