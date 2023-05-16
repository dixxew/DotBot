using System.Text.Json.Serialization;

namespace DotBot.Models
{
    class GptMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }
    class GptRequest
    {
        [JsonPropertyName("model")]
        public string ModelId { get; set; } = "";
        [JsonPropertyName("messages")]
        public List<GptMessage> Messages { get; set; } = new();
    }

    class GptResponseData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";
        [JsonPropertyName("object")]
        public string Object { get; set; } = "";
        [JsonPropertyName("created")]
        public ulong Created { get; set; }
        [JsonPropertyName("choices")]
        public List<GptChoice> Choices { get; set; } = new();
        [JsonPropertyName("usage")]
        public GptUsage Usage { get; set; } = new();
    }

    class GptChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("message")]
        public GptMessage Message { get; set; } = new();
        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = "";
    }

    class GptUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
