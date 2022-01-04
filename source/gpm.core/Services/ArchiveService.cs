using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace gpm.core.Services
{
    public class ArchiveService : IArchiveService
    {
        public PlatformID[] SupportedPlatforms { get; } = { PlatformID.Win32NT, PlatformID.Unix };

        public async Task<bool> IsSupportedArchive(string archive)
        {
            try
            {
                _ = await Task.Run(() => ArchiveFactory.Open(archive));
                return true;
            }

            catch (InvalidOperationException)
            {
                return false;
            }
        }
        public async Task<bool> ExtractAsync(
            string archive,
            string destination,
            bool overwrite = false,
            bool preserveRelativePaths = true)
        {
            using var archiveStream = File.OpenRead(archive);

            var archiveReader = ReaderFactory.Open(archiveStream);
            var archiveOptions = new ExtractionOptions
            {
                Overwrite = overwrite,
                ExtractFullPath = preserveRelativePaths
            };

            await Task.Run(() => archiveReader.WriteAllToDirectory(destination, archiveOptions));

            return true;
        }

        public async Task<bool> ExtractFilesAsync(
            string archive,
            string destination,
            List<string> targetFiles,
            Dictionary<string, string>? fileDestinations = null,
            bool overwrite = false,
            bool preserveRelativePaths = false)
        {
            // fileDestinations and targetFiles must contain the same number of entries if the former is not null.
            if (fileDestinations is not null && targetFiles.Count != fileDestinations.Count)
            {
                // TODO: Replace with custom exception type.
                throw new Exception("Failed while validating ExtractFilesAsync arguments: Entry count of fileDestinations does not match entryCount of targetFiles.");
            }

            using var archiveStream = File.OpenRead(archive);

            var archiveReader = ArchiveFactory.Open(archiveStream);
            var extractOptions = new ExtractionOptions
            {
                Overwrite = overwrite,
                ExtractFullPath = preserveRelativePaths
            };

            // Warning: This method may be slow for certain compression formats -- most notably, LZMA and RAR. Needs future optimization.
            foreach (var entry in archiveReader.Entries)
            {
                // Skipping directories for now as they should be created when files are extracted; should examine need for config.
                if (entry.IsDirectory)
                {
                    continue;
                }

                // Extract the entry to the destination, using fileDestinations if an entry exists.
                if (fileDestinations is not null && fileDestinations.ContainsKey(entry.Key))
                {
                    await Task.Run(() => entry.WriteToFile(Path.Combine(destination, fileDestinations[entry.Key])));
                    continue;
                }

                // Otherwise, extract to the destination directory.
                await Task.Run(() => entry.WriteToDirectory(destination, extractOptions));
            }

            return true;
        }
    }
}
