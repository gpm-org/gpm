namespace gpm.Core.Extensions
{
    public static class FileS
    {
        /// <summary>
        /// Tries to delete a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="action"></param>
        /// <returns>false if an error was thrown</returns>
        public static bool TryDeleteFile(string path, Action? action = null)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                action?.Invoke();
                return false;
            }
        }
    }
}
