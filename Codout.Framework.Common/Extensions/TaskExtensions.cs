using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Codout.Framework.Common.Extensions;

/// <summary>
/// Extension methods para permitir operações fire-and-forget com Tasks
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Permite que uma Task execute em background sem aguardar seu resultado.
    /// Captura exceções não observadas para evitar crashes da aplicação.
    /// </summary>
    /// <param name="task">A task a ser executada em background</param>
    /// <param name="logger">Logger opcional para registrar exceções</param>
    /// <param name="continueOnCapturedContext">Define se deve capturar o contexto de sincronização</param>
    public static void Forget(this Task task, ILogger logger = null, bool continueOnCapturedContext = false)
    {
        if (task == null) return;

        // Não aguarda a task, mas captura exceções para evitar UnobservedTaskException
        _ = ForgetAsync(task, logger, continueOnCapturedContext);
    }

    /// <summary>
    /// Permite que uma Task&lt;T&gt; execute em background sem aguardar seu resultado.
    /// </summary>
    /// <typeparam name="T">Tipo do resultado da task</typeparam>
    /// <param name="task">A task a ser executada em background</param>
    /// <param name="logger">Logger opcional para registrar exceções</param>
    /// <param name="continueOnCapturedContext">Define se deve capturar o contexto de sincronização</param>
    public static void Forget<T>(this Task<T> task, ILogger logger = null, bool continueOnCapturedContext = false)
    {
        if (task == null) return;

        // Converte para Task não-genérica e chama o método base
        ((Task)task).Forget(logger, continueOnCapturedContext);
    }

    /// <summary>
    /// Versão com callback personalizado para tratamento de exceções
    /// </summary>
    /// <param name="task">A task a ser executada em background</param>
    /// <param name="onException">Callback para tratar exceções</param>
    /// <param name="continueOnCapturedContext">Define se deve capturar o contexto de sincronização</param>
    public static void Forget(this Task task, Action<Exception> onException, bool continueOnCapturedContext = false)
    {
        if (task == null) return;

        _ = ForgetWithCallbackAsync(task, onException, continueOnCapturedContext);
    }

    /// <summary>
    /// Implementação interna assíncrona para capturar exceções
    /// </summary>
    private static async Task ForgetAsync(Task task, ILogger logger, bool continueOnCapturedContext)
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (Exception ex)
        {
            // Log da exceção se logger foi fornecido
            logger?.LogError(ex, "Exceção não tratada em operação fire-and-forget");

            // Em ambiente de desenvolvimento, pode ser útil quebrar o debugger
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
#endif
        }
    }

    /// <summary>
    /// Implementação interna assíncrona com callback personalizado
    /// </summary>
    private static async Task ForgetWithCallbackAsync(Task task, Action<Exception> onException, bool continueOnCapturedContext)
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (Exception ex)
        {
            try
            {
                onException?.Invoke(ex);
            }
            catch
            {
                // Se o callback de exceção também falhar, apenas ignora
                // para evitar exceções em cascata
            }
        }
    }
}
