using System;
using System.IO;

namespace Codout.Framework.Common.Extensions;

/// <summary>
///     Extensões comuns para tipos relacionadas a <see cref="Stream" />.
/// </summary>
public static class Streams
{
    #region ReadFully

    /// <summary>
    ///     Lê um fluxo de dados até o fim. Os dados são retornados como um array de bytes.
    ///     Um <see cref="IOException" /> é lançada se qualquer uma das chamadas subjacentes de entrada/saída falhar.
    /// </summary>
    /// <param name="stream">Um <see cref="Stream" /> de origem.</param>
    public static byte[] ReadFully(this Stream stream)
    {
        // use 32K for initial length.
        var buffer = new byte[32768];
        var read = 0;

        int chunk;
        while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
        {
            read += chunk;

            // Se chegamos ao fim do nosso buffer, verificar para ver se há mais alguma informação
            if (read == buffer.Length)
            {
                var nextByte = stream.ReadByte();

                // Fim do stream? Se sim, terminamos
                if (nextByte == -1)
                    return buffer;

                // Não. Redimensionar o buffer, coloque no byte que acabamos de ler, e continue
                var newBuffer = new byte[buffer.Length * 2];
                Array.Copy(buffer, newBuffer, buffer.Length);
                newBuffer[read] = (byte)nextByte;
                buffer = newBuffer;
                read++;
            }
        }

        // Buffer é agora muito grande. Reduzir.
        var ret = new byte[read];
        Array.Copy(buffer, ret, read);
        return ret;
    }

    #endregion
}