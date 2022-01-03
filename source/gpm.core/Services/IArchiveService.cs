namespace gpm.core.Services
{
    public interface IArchiveService
    {
        PlatformID[] SupportedPlatforms { get; }

        /// <summary>
        /// Determine if the target archive can be extracted by this ArchiveService instance.
        /// </summary>
        /// <param name="archive">The path of the target archive.</param>
        /// <returns></returns>
        Task<bool> IsSupportedArchive(string archive);

        /// <summary>
        /// Extract all files from the archive into the destination.
        /// </summary>
        /// <param name="archive">The archive to extract.</param>
        /// <param name="destination">The destination directory.</param>
        /// <param name="overwrite">True to overwrite existing files and directories; otherwise false.</param>
        /// <param name="preserveRelativePaths">True to preserve file and directory paths relative to the archive; otherwise false.</param>
        /// <returns></returns>
        Task<bool> ExtractAsync(
            string archive,
            string destination,
            bool overwrite = false,
            bool preserveRelativePaths = true);

        /// <summary>
        /// Extract one or more files from the archive into the destination.
        /// </summary>
        /// <param name="archive">The archive to extract.</param>
        /// <param name="destination">The destination directory.</param>
        /// <param name="targetFiles">One or more file paths, relative to the path of the archive itself.</param>
        /// <param name="fileDestinations">Destinations of files specified in targetFiles. Must be the same length if defined; otherwise null.</param>
        /// <param name="overwrite">True to overwrite existing files and directories; otherwise false.</param>
        /// <param name="preserveRelativePaths">True to preserve file and directory paths relative to the archive; otherwise false.</param>
        /// <returns></returns>
        Task<bool> ExtractFilesAsync(
            string archive,
            string destination,
            List<string> targetFiles,
            List<string>? fileDestinations = null,
            bool overwrite = false,
            bool preserveRelativePaths = false);
    }
}
