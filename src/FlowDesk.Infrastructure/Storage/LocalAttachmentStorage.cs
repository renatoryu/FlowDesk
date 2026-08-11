using FlowDesk.Application.Abstractions.Storage;
using Microsoft.Extensions.Hosting;

namespace FlowDesk.Infrastructure.Storage;

public sealed class LocalAttachmentStorage
    : IAttachmentStorage
{
    private const int BufferSize = 81920;

    private static readonly HashSet<string>
        SupportedExtensions =
        new(
            StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".png",
            ".jpg",
            ".jpeg"
        };

    private readonly string _rootPath;

    public LocalAttachmentStorage(
        AttachmentStorageOptions options,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        if (string.IsNullOrWhiteSpace(options.RootPath))
        {
            throw new InvalidOperationException(
                "Attachment storage root path was not configured.");
        }

        _rootPath = Path.GetFullPath(
            Path.IsPathRooted(options.RootPath)
                ? options.RootPath
                : Path.Combine(
                    environment.ContentRootPath,
                    options.RootPath));
    }

    public async Task<string> SaveAsync(
        Guid ticketId,
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        ValidateTicketId(ticketId);
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanRead)
        {
            throw new ArgumentException(
                "Attachment content must be readable.",
                nameof(content));
        }

        string normalizedExtension =
            NormalizeExtension(fileExtension);

        string ticketDirectory =
            GetTicketDirectory(ticketId);

        Directory.CreateDirectory(ticketDirectory);

        string storedFileName =
            $"{Guid.NewGuid():N}{normalizedExtension}";

        string filePath = GetSafeFilePath(
            ticketId,
            storedFileName);

        try
        {
            await using var output = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous);

            await content.CopyToAsync(
                output,
                cancellationToken);

            return storedFileName;
        }
        catch
        {
            TryDeleteFile(filePath);
            throw;
        }
    }

    public Task<Stream?> OpenReadAsync(
        Guid ticketId,
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        ValidateTicketId(ticketId);

        string filePath = GetSafeFilePath(
            ticketId,
            storedFileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous |
            FileOptions.SequentialScan);

        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(
        Guid ticketId,
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        ValidateTicketId(ticketId);

        string filePath = GetSafeFilePath(
            ticketId,
            storedFileName);

        TryDeleteFile(filePath);

        return Task.CompletedTask;
    }

    private string GetTicketDirectory(Guid ticketId)
    {
        return Path.Combine(
            _rootPath,
            ticketId.ToString("N"));
    }

    private string GetSafeFilePath(
        Guid ticketId,
        string storedFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            storedFileName);

        if (!string.Equals(
                storedFileName,
                Path.GetFileName(storedFileName),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Stored file name cannot contain a path.",
                nameof(storedFileName));
        }

        string directory =
            Path.GetFullPath(
                GetTicketDirectory(ticketId));

        string filePath =
            Path.GetFullPath(
                Path.Combine(
                    directory,
                    storedFileName));

        string directoryPrefix =
            directory +
            Path.DirectorySeparatorChar;

        StringComparison comparison =
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        if (!filePath.StartsWith(
                directoryPrefix,
                comparison))
        {
            throw new InvalidOperationException(
                "Attachment path is outside the storage root.");
        }

        return filePath;
    }

    private static string NormalizeExtension(
        string fileExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileExtension);

        string normalized =
            fileExtension.Trim().ToLowerInvariant();

        if (!normalized.StartsWith('.'))
        {
            normalized = $".{normalized}";
        }

        if (!SupportedExtensions.Contains(normalized))
        {
            throw new ArgumentException(
                "Unsupported attachment extension.",
                nameof(fileExtension));
        }

        return normalized;
    }

    private static void ValidateTicketId(Guid ticketId)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ticket identifier cannot be empty.",
                nameof(ticketId));
        }
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
