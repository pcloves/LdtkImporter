using System;

namespace LdtkImporter;

/// <summary>
/// 标记在后置处理器上，标记后置处理器可以处理哪些数据
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PostProcessorAttribute : Attribute
{
    public readonly ProcessorType ProcessorType;

    public PostProcessorAttribute(ProcessorType processorType)
    {
        ProcessorType = processorType;
    }
}

[Flags]
public enum ProcessorType
{
    World = 1 << 0,
    Level = 1 << 1,
    Entity = 1 << 2,
}