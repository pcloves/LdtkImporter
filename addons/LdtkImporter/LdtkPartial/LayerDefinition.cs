using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class LayerDefinition : IJsonOnDeserialized
{
    [JsonIgnore] public TypeEnum TypeEnum;

    public bool IsAutoLayer()
    {
        return (TypeEnum == TypeEnum.IntGrid && TilesetDefUid.HasValue) || TypeEnum == TypeEnum.AutoLayer;
    }

    public bool AutoLayerRulesCanBeUsed()
    {
        if (!IsAutoLayer())
            return false;

        if (!TilesetDefUid.HasValue)
            return false;

        if (TypeEnum == TypeEnum.AutoLayer && !AutoSourceLayerDefUid.HasValue)
            return false;

        return true;
    }

    public void OnDeserialized()
    {
        TypeEnum = Type.ToEnum<TypeEnum>();
    }
}