using Codout.Mailer.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Mailer.Tests;

public class ExtensionsTests
{
    [Fact]
    public void ReadFully_DeveLerTodoOConteudoDoStream()
    {
        var bytes = Enumerable.Range(0, 100_000).Select(i => (byte)(i % 256)).ToArray();
        using var stream = new MemoryStream(bytes);

        var resultado = stream.ReadFully();

        resultado.Should().Equal(bytes);
    }

    [Fact]
    public void ReadFully_ComPosicaoNoFinal_DeveReposicionarELerTudo()
    {
        var bytes = "conteudo de teste"u8.ToArray();
        using var stream = new MemoryStream(bytes);
        stream.Seek(0, SeekOrigin.End);

        var resultado = stream.ReadFully();

        resultado.Should().Equal(bytes);
    }

    [Fact]
    public void ReadFully_ComStreamVazio_DeveRetornarArrayVazio()
    {
        using var stream = new MemoryStream();

        stream.ReadFully().Should().BeEmpty();
    }

    [Fact]
    public void ReadFully_ComStreamNaoSeekable_LancaNotSupportedException()
    {
        // BUG?: ReadFully força stream.Position = 0, o que exige stream seekable.
        // Streams de rede/pipe (não-seekable) lançam NotSupportedException.
        // Teste de caracterização do comportamento atual.
        using var inner = new MemoryStream("abc"u8.ToArray());
        using var naoSeekable = new NonSeekableStream(inner);

        var acao = () => naoSeekable.ReadFully();

        acao.Should().Throw<NotSupportedException>();
    }

    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
