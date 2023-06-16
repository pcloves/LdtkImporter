using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class TileInstance
{
    [JsonIgnore] public int Layer { get; set; } = 0;
}