#if TOOLS
using Godot;

namespace LdtkImporter;

[Tool]
public partial class Plugin : EditorPlugin
{
    private global::LdtkImporter.LdtkImporterPlugin _ldtkImporterPlugin;
    public override string _GetPluginName() => "sss";

    public override void _EnterTree()
    {
        _ldtkImporterPlugin = new global::LdtkImporter.LdtkImporterPlugin();
        AddImportPlugin(_ldtkImporterPlugin);
    }

    public override void _ExitTree()
    {
        RemoveImportPlugin(_ldtkImporterPlugin);
        _ldtkImporterPlugin = null;
    }
}
#endif