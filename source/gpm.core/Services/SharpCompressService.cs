using SharpCompress.Archives;

namespace gpm.core.Services
{
    public class SharpCompressService : IArchiveService
    {
        public PlatformID[] SupportedPlatforms { get; } = { PlatformID.Win32NT, PlatformID.Unix };

        public async Task<bool> IsSupportedArchive(string archive)
        {
            // Quick and dirty way to determine if SharpCompress supports a given archive.
            try
            {
                _ = await Task.Run(() => ArchiveFactory.Open(archive));
                return true;
            }

            catch (InvalidOperationException e)
            {
                return false;
            }
        }

        public async Task<bool> ExtractAsync(string archive, string destination)
        {
            
        }

        public async Task<bool> ExtractFilesAsync(string archive, string destination, List<string> targetFiles) => throw new NotImplementedException();
    }
}
