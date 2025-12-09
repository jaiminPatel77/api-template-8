using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConfidoSoft.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods related to IO/File.
    /// </summary>
    public static class FileUtile
    {
        /// <summary>
        /// Read all text from file.
        /// Helper method as .NET core 2.2 missing such utility function.
        /// </summary>
        /// <param name="fileName">Full name of file including path detail</param>
        /// <returns>all text detail from text file.</returns>
        public static async Task<string> ReadAllTextAsync( string fileName)
        {
            string fileText;
            using (var sourceReader = File.OpenText(fileName))
            {
                fileText = await sourceReader.ReadToEndAsync();
            }
            return fileText;
        }
    }
}
