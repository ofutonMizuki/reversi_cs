using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace reversi_cs
{
    /**
     * TypeScript 実装の NNEval.save() が出力する JSON を読み込むためのモデル。
     * 
     * - version: 3
     * - hiddenSizes: 任意層の中間ユニット数配列
     * - W: [65][L][out][in]
     * - b: [65][L][out]
     */
    public sealed class NNEvalModel
    {
        [JsonPropertyName("__type")]
        public string? Type { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("hiddenSizes")]
        public int[]? HiddenSizes { get; set; }

        [JsonPropertyName("W")]
        public List<List<List<List<double>>>>? W { get; set; }

        [JsonPropertyName("b")]
        public List<List<List<double>>>? B { get; set; }
    }
}
