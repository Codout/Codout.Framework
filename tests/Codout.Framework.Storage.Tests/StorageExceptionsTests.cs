using Codout.Framework.Storage.Exceptions;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Storage.Tests;

public class StorageExceptionsTests
{
    [Fact]
    public void StorageException_ComMensagem_DevePreservarMensagem()
    {
        var ex = new StorageException("falhou");

        ex.Message.Should().Be("falhou");
        ex.Container.Should().BeNull();
        ex.FileName.Should().BeNull();
    }

    [Fact]
    public void StorageException_ComInnerException_DevePreservarInner()
    {
        var inner = new InvalidOperationException("causa raiz");
        var ex = new StorageException("falhou", inner);

        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void StorageException_ComContainerEArquivo_DevePreencherPropriedades()
    {
        var ex = new StorageException("falhou", "meu-container", "arquivo.txt");

        ex.Container.Should().Be("meu-container");
        ex.FileName.Should().Be("arquivo.txt");
    }

    [Fact]
    public void StorageException_ComContainerArquivoEInner_DevePreencherTudo()
    {
        var inner = new TimeoutException();
        var ex = new StorageException("falhou", "c", "f.txt", inner);

        ex.Container.Should().Be("c");
        ex.FileName.Should().Be("f.txt");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void StorageNotFoundException_DeveMontarMensagemComContainerEArquivo()
    {
        var ex = new StorageNotFoundException("docs", "relatorio.pdf");

        ex.Message.Should().Be("File 'relatorio.pdf' not found in container 'docs'.");
        ex.Container.Should().Be("docs");
        ex.FileName.Should().Be("relatorio.pdf");
        ex.Should().BeAssignableTo<StorageException>();
    }

    [Fact]
    public void StorageNotFoundException_ComInner_DevePreservarInner()
    {
        var inner = new Exception("404");
        var ex = new StorageNotFoundException("docs", "f.txt", inner);

        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void StorageFileAlreadyExistsException_DeveMontarMensagem()
    {
        var ex = new StorageFileAlreadyExistsException("docs", "f.txt");

        ex.Message.Should().Be("File 'f.txt' already exists in container 'docs'.");
        ex.Container.Should().Be("docs");
        ex.FileName.Should().Be("f.txt");
    }

    [Fact]
    public void StorageContainerException_DeveUsarFileNameVazio()
    {
        var ex = new StorageContainerException("docs", "falha no container");

        ex.Message.Should().Be("falha no container");
        ex.Container.Should().Be("docs");
        ex.FileName.Should().BeEmpty();
    }

    [Fact]
    public void StorageContainerException_ComInner_DevePreservarInner()
    {
        var inner = new Exception("x");
        var ex = new StorageContainerException("docs", "falha", inner);

        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void StorageQuotaExceededException_DevePreencherLimiteEUso()
    {
        var ex = new StorageQuotaExceededException(quotaLimit: 1000, currentUsage: 1500);

        ex.QuotaLimit.Should().Be(1000);
        ex.CurrentUsage.Should().Be(1500);
        ex.Message.Should().Be("Storage quota exceeded. Limit: 1000 bytes, Current: 1500 bytes.");
    }
}
