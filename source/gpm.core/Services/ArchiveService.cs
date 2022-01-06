using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace gpm.Core.Services
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
        public async Task<List<string>> ExtractAsync(
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

            var extractedFiles = new List<string>();

            // Hacky method to determine which files were extracted. This implementation needs work
            while (archiveReader.MoveToNextEntry())
            {
                // Possible speedup here if we enqueue all extract Tasks and use Task.WhenAll to await them at the same time.
                await Task.Run(() => archiveReader.WriteEntryToDirectory(destination, archiveOptions));

                extractedFiles.Add(Path.Combine(destination, archiveReader.Entry.Key));
            }

            return extractedFiles;
        }

        public async Task<List<string>> ExtractFilesAsync(
            string archive,
            string destination,
            List<string> targetFiles,
            bool overwrite = false,
            bool preserveRelativePaths = false)
        {
            if (targetFiles.Count == 0)
            {
                throw new Exception($"`ExtractFilesAsync` failed, `targetFiles` argument contains no entries.");
            }

            using var archiveStream = File.OpenRead(archive);

            var archiveReader = ArchiveFactory.Open(archiveStream);
            var extractOptions = new ExtractionOptions
            {
                Overwrite = overwrite,
                ExtractFullPath = preserveRelativePaths
            };

            var extractedFiles = new List<string>();

            // Warning: This method may be slow for certain compression formats -- most notably, LZMA and RAR. Needs future optimization.
            foreach (var entry in archiveReader.Entries.Where(x => targetFiles.Contains(x.Key)))
            {
                // Skipping directories for now as they should be created when files are extracted; should examine need for config.
                if (entry.IsDirectory)
                {
                    continue;
                }

                await Task.Run(() => entry.WriteToDirectory(destination, extractOptions));
                extractedFiles.Add(Path.Combine(destination, entry.Key));
            }

            return extractedFiles;
        }
    }
}
