using System.Text.Json.Nodes;
using APIRunner.Enums;

namespace APIRunner.Models
{
  public record WebMessage
  {
    public WebMessageAction Action { get; init; }
    public string? Email { get; init; }
    public JsonNode? App { get; init; }
    public bool Enabled { get; init; }
    public int Index { get; init; }
    public string? Env { get; init; }
  }
}