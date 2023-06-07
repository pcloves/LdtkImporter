#if TOOLS
using Godot;

namespace LdtkImporter.addons.LdtkImporter;

[Tool]
public partial class LdtkImporter : EditorPlugin
{
    private LdtkWorldImporter _ldtkWorldImporter;
    public override string _GetPluginName() => "sss";

    public override void _EnterTree()
    {
        _ldtkWorldImporter = new LdtkWorldImporter();
        AddImportPlugin(_ldtkWorldImporter);
    }

    public override void _ExitTree()
    {
        RemoveImportPlugin(_ldtkWorldImporter);
        _ldtkWorldImporter = null;
    }
}
#endif