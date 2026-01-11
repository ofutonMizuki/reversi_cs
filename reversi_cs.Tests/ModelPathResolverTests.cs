using System;
using System.IO;
using Xunit;

namespace reversi_cs.Tests
{
    public class ModelPathResolverTests
    {
        [Fact]
        public void Resolve_WhenNoFilesExist_ShouldReturnBaseDirCandidate()
        {
            string p = reversi_cs.ModelPathResolver.Resolve(fileName: "__no_such_model__.json", subDir: "__no_such_dir__");
            Assert.EndsWith("__no_such_model__.json", p, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Resolve_WhenFileExistsInBaseDir_ShouldReturnThatPath()
        {
            string fileName = "__test_model__.json";
            string baseDir = AppContext.BaseDirectory;
            string path = Path.Combine(baseDir, fileName);

            try
            {
                File.WriteAllText(path, "{}");
                string resolved = reversi_cs.ModelPathResolver.Resolve(fileName: fileName, subDir: "models");
                Assert.Equal(path, resolved, ignoreCase: true);
            }
            finally
            {
                try { if (File.Exists(path)) File.Delete(path); } catch { }
            }
        }
    }
}
