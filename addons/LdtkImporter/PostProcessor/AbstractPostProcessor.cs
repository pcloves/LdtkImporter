using System.Reflection;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

public abstract partial class AbstractPostProcessor : Resource
{
    /// <summary>
    /// Entity导入后处理，可以对生成的Entity进行后处理，包括修改子节点，甚至返回一个全新的类型
    /// </summary>
    /// <param name="ldtkJson">LdtkJson数据</param>
    /// <param name="options"></param>
    /// <param name="baseNode">需要修改的Entity节点</param>
    /// <returns>返回修改后的节点，可以返回完全不同的其他节点</returns>
    public abstract Node2D PostProcess(LdtkJson ldtkJson, Dictionary options, Node2D baseNode);


    /// <summary>
    /// 本后置处理器是否可以处理
    /// </summary>
    /// <param name="processorType">要处理的数据</param>
    /// <returns></returns>
    public bool Handle(ProcessorType processorType)
    {
        var attribute = GetType().GetCustomAttribute<PostProcessorAttribute>();

        return attribute != null && attribute.ProcessorType.HasFlag(processorType);
    }
}