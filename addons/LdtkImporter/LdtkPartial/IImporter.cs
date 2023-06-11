using Godot;
using Godot.Collections;

namespace LdtkImporter;

public interface IImporter
{
    Error PreImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles);
    Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles);
    Error PostImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles);
}