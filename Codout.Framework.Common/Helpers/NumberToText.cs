using System;
using System.Collections.Generic;
using System.Text;

namespace Codout.Framework.Common.Helpers
{
    public class NumberToText
    {
        //array de 2 linhas e 14 colunas[2][14]
        private static readonly string[,] Qualificadores =
        {
            {"centavo", "centavos"}, //[1][0] e [1][1]
            {"", ""}, //[2][0],[2][1]
            {"mil", "mil"},
            {"milhão", "milhões"},
            {"bilhão", "bilhões"},
            {"trilhão", "trilhões"},
            {"quatrilhão", "quatrilhões"},
            {"quintilhão", "quintilhões"},
            {"sextilhão", "sextilhões"},
            {"setilhão", "setilhões"},
            {"octilhão", "octilhões"},
            {"nonilhão", "nonilhões"},
            {"decilhão", "decilhões"}
        };

        private static readonly string[,] Numeros =
        {
            {
                "zero", "um", "dois", "três", "quatro",
                "cinco", "seis", "sete", "oito", "nove",
                "dez", "onze", "doze", "treze", "quatorze",
                "quinze", "dezesseis", "dezessete", "dezoito", "dezenove"
            },
            {
                "vinte", "trinta", "quarenta", "cinqüenta", "sessenta",
                "setenta", "oitenta", "noventa", null, null, null, null, null, null, null, null, null, null, null, null
            },
            {
                "cem", "cento",
                "duzentos", "trezentos", "quatrocentos", "quinhentos", "seiscentos",
                "setecentos", "oitocentos", "novecentos", null, null, null, null, null, null, null, null, null, null
            }
        };

        private int _num;
        private readonly List<int> _numeroLista;

        public NumberToText()
        {
            _numeroLista = new List<int>();
        }

        public NumberToText(decimal dec)
        {
            _numeroLista = new List<int>();
            SetNumero(dec);
        }

        public void SetNumero(decimal dec)
        {
            dec = decimal.Round(dec, 2);
            dec = dec * 100;
            _num = Convert.ToInt32(dec);
            _numeroLista.Clear();

            if (_num == 0)
            {
                _numeroLista.Add(0);
                _numeroLista.Add(0);
            }
            else
            {
                AddRemainder(100);
                while (_num != 0)
                    AddRemainder(1000);
            }
        }

        private void AddRemainder(int divisor)
        {
            var div = _num / divisor;
            var mod = _num % divisor;
            var newNum = new[] {div, mod};

            _numeroLista.Add(mod);
            _num = div;
        }

        private bool TemMaisGrupos(int ps)
        {
            while (ps > 0)
            {
                if (_numeroLista[ps] != 00 && !TemMaisGrupos(ps - 1))
                    return true;

                ps--;
            }

            return true;
        }

        private bool EhPrimeiroGrupoUm()
        {
            return _numeroLista[_numeroLista.Count - 1] == 1;
        }

        private bool EhUltimoGrupo(int ps)
        {
            return ps > 0 && _numeroLista[ps] != 0 || !TemMaisGrupos(ps - 1);
        }

        private bool EhGrupoZero(int ps)
        {
            if (ps <= 0 || ps >= _numeroLista.Count)
                return true;

            return _numeroLista[ps] == 0;
        }

        private bool EhUnicoGrupo()
        {
            if (_numeroLista.Count <= 3) return false;

            if (!EhGrupoZero(1) && !EhGrupoZero(2)) return false;

            var hasOne = false;

            for (var i = 3; i < _numeroLista.Count; i++)
                if (_numeroLista[i] != 0)
                {
                    if (hasOne) return false;

                    hasOne = true;
                }

            return true;
        }

        private string NumToString(int numero, int escala)
        {
            var unidade = numero % 10;
            var dezena = numero % 100;
            var centena = numero / 100;
            var buf = new StringBuilder();

            if (numero != 0)
            {
                if (centena != 0)
                {
                    if (dezena == 0 && centena == 1)
                        buf.Append(Numeros[2, 0]);
                    else
                        buf.Append(Numeros[2, centena]);
                }

                if (buf.Length > 0 && dezena != 0)
                    buf.Append(" e ");

                if (dezena > 19)
                {
                    dezena = dezena / 10;
                    buf.Append(Numeros[1, dezena - 2]);
                    if (unidade != 0)
                    {
                        buf.Append(" e ");
                        buf.Append(Numeros[0, unidade]);
                    }
                }
                else if (centena == 0 || dezena != 0)
                {
                    buf.Append(Numeros[0, dezena]);
                }

                buf.Append(" ");
                if (numero == 1)
                    buf.Append(Qualificadores[escala, 0]);
                else
                    buf.Append(Qualificadores[escala, 1]);
            }

            return buf.ToString();
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            for (var count = _numeroLista.Count - 1; count > 0; count--)
            {
                if (buf.Length > 0 && !EhGrupoZero(count))
                    buf.Append(" e ");

                buf.Append(NumToString(_numeroLista[count], count));
            }

            if (buf.Length > 0)
            {
                while (buf.ToString().EndsWith(" "))
                    buf.Length = buf.Length - 1;

                if (EhUnicoGrupo())
                    buf.Append(" de ");

                if (EhPrimeiroGrupoUm())
                    buf.Insert(0, "h");

                if (_numeroLista.Count == 2 && _numeroLista[1] == 1)
                    buf.Append(" real");
                else
                    buf.Append(" reais");

                if (_numeroLista[0] != 0)
                    buf.Append(" e ");
            }

            if (_numeroLista[0] != 0)
                buf.Append(NumToString(_numeroLista[0], 0));

            return buf.ToString();
        }
    }
}