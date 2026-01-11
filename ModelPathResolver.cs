using System;
using System.IO;

namespace reversi_cs
{
    /**
     * NNモデルファイルの固定配置を解決する。
     * 
     * - 実行ファイルと同じディレクトリ
     * - もしくは 1 階層下のディレクトリ
     */
    public static class ModelPathResolver
    {
        /**
         * 既定のモデルファイル名。
         */
        public const string DefaultModelFileName = "model.json";

        /**
         * モデルファイルパスを解決する。
         * @param fileName モデルファイル名（未指定なら既定名）
         * @param subDir 1階層下の検索先ディレクトリ名（未指定なら "models"）
         */
        public static string Resolve(string? fileName = null, string? subDir = null)
        {
            fileName ??= DefaultModelFileName;
            subDir ??= "models";

            string baseDir = AppContext.BaseDirectory;

            string p1 = Path.Combine(baseDir, fileName);
            if (File.Exists(p1)) return p1;

            string p2 = Path.Combine(baseDir, subDir, fileName);
            if (File.Exists(p2)) return p2;

            return p1; // fallback (for error message at load)
        }
    }
}
