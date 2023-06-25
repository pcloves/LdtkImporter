using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Godot;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class TileInstance
{
    [JsonIgnore] public int Layer { get; set; }

    [JsonIgnore] public AlternativeIdFlags AlternativeIdFlags { get; set; } = AlternativeIdFlags.None;

    [JsonIgnore] public Vector2I TextureOrigin { get; set; } = Vector2I.Zero;
}