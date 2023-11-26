# Godot LDtk C# Importer

Godot 4 C# [LDtk](https://ldtk.io/) 导入插件

![](https://img.shields.io/badge/Godot-4.2%2B-%20?logo=godotengine&color=%23478CBF) ![](https://img.shields.io/badge/LDtk%201.4.1-%20?color=%23FFCC00)

> ⚠️ 目前该插件仍处于开发阶段，许多特性还处于调整中，请勿将本插件应用于生产环境！在使用本插件前，请确保已经对项目文件进行备份！

# 📖 安装

1. 使用C#版Godot 4.2+
2. 将`addons\LdtkImporter`目录放到项目的`addons`目录下
3. 通过 `Project > Project Settings > Plugins`开启本插件
4. 此时`.ldtk`文件可以被Godot识别，并且会自动生成`.tscn`场景和`.tres`Tileset资源

# ✨ 特性

## 🌏 World

- [x]`.ldtk`文件导入后生成同名的`.tscn`场景，内部包含所有的`关卡`节点 ⬇️

![](img/World.png)


## 🏔️ Level

- [x] LDTK`Level`导入后，生成同名的`关卡`场景 ⬇️
- [x] `关卡`场景独立导出为`.tscn`文件

![](img/Level.png)


## 📄 Layer
- [x] `AutoLayer`、`IntGrid`类型Layer生成为Godot TileMap，`Entity`类型Layer生成为Godot Node2D
- [x] 支持将 `IntGrid` 作为子节点生成 ⬇️

![](img/LayerIntGrid.gif)

- [x] 支持同一个`Layer`下多个Tile叠加


## 🧱 Tilesets
- [x] 为每个 LDTK Tilesets
  生成 [TileSetAtlasSource](https://docs.godotengine.org/en/stable/classes/class_tilesetatlassource.html#class-tilesetatlassource)
  样式的 Godot Tileset
- [x] 支持 [Tile](https://ldtk.io/json/#ldtk-Tile;f) X轴/Y轴翻转，并为其生成专门的 `AlternativeTile`


## 🐸 Entity
- [x] 为每个Entity生成一个


# 🚩 导入选项
当在Godot的`FileSystem`选中一个`.ldtk`文件时，可以看到如下图所示的导入选项： ⬇️

![](img/ImportOptions.png)



* General
    * Prefix: 该前缀表示当执行导入时，生成的 `Node2D`、`TileMap`等节点的名称前缀（例如：`LDTK_World_Level_0`）以及导入的元数据的名称前缀
* World
    * World Scenes: 表示要生成的世界场景的文件名称
* Tileset
    * Add Tileset Definition to Meta: 将所有 [LDTK Tileset definition](https://ldtk.io/json/#ldtk-TilesetDefJson)数据作为元数据存储到Godot tileset中，其中元数据的key为：`${Prefix}_tilesetDefinition`，例如：`LDTK_tilesetDefinition`
    * Resources: 根据LDTK中Tilesets的不同，插件自动生成对应的配置
* Entity
    * Add Entity Definition to Meta: 将[LDTK Entity Definition](https://ldtk.io/json/#ldtk-EntityDefJson)数据作为元数据存储到导入后的Entity Scene以及节点中，其中元数据的key为：`${Prefix}
      _entityDefinition`，例如：`LDTK_entityDefinition`
    * Add Entity Instance to Meta: 将[LDTK Entity Instance](https://ldtk.io/json/#ldtk-EntityInstanceJson)数据作为元数据存储到导入后的Entity节点中，其中元数据的key为：`${Prefix}_entityInstance`，例如：`LDTK_entityInstance`
    * Scenes: 根据LDTK中Entity的数量，插件自动生成对于的配置
* Level
    * Add Level to Meta: 将[LDTK Level](https://ldtk.io/json/#ldtk-LevelJson)数据作为元数据存储到导入后的Level节点中，其中元数据的key为：`${Prefix}_levelInstance`，例如：`LDTK_levelInstance`
    * Add Layer Instance to Meta: 将[LDTK Layer Instance](https://ldtk.io/json/#ldtk-LayerInstanceJson)数据作为元数据存储到导入后的Layer节点中，其中其中元数据的key为：`${Prefix}_layerInstance`，例如：`LDTK_layerInstance`
    * Add Layer Definition to Meta: 将[LDTK Layer Definition](https://ldtk.io/json/#ldtk-LayerDefJson)数据作为元数据存储到导入后的Layer节点中，其中其中元数据的key为：`${Prefix}_layerDefinition`，例如：`LDTK_layerDefinition`
    * Import Int Grid: 是否导入`IntGrid`，效果参考[Layer](#-layer)中动图展示
    * Scenes: 根据LDTK中Level的不同，插件自动生成对应的配置

# ❓FAQ
### 在同TileMap（对应于LDTK的`IntGrid`或`AutoLayer`图层）中，如何支持在同一个位置叠加多个Tile的？
Godot TileMap支持多个[Layer](https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html#creating-tilemap-layers)，插件在导入前前提前为每个[LDTK Tile Instance]
(https://ldtk.io/json/#ldtk-Tile)计算它在TileMap中的Layer图层索引（从0开始），同时算出所有`Tile Instance`的最大索引，进而创建出足够多的TileMap Layer，并在导入时，将每个`LDTK Tile Instance`放入对应的`TileMap 
Layer`即可

### 如果使用该插件作为LDTK和Godot的桥梁，那么工作流应该是怎样的？
这也是本插件作者一直在思考并且还没要找到答案的问题，在LDTK和Godot结合的工作流中，LDTK的主要起到地图编辑器的作用，然而并不能在LDTK中完成所有的地图编辑工作，例如需要为TileSet配置物理碰撞、导航时，又例如由于开发需求对导入后的Entity
场景进行编辑修改，这都需要在导入后进行二次修改。这导致了一个核心矛盾点的产生：`如何解决重复导入而不影响在Godot中已经进行的修改。`，目前的思路是：
1. 不支持重复导入，每次导入都覆盖原来的资源（Tileset、Scene）
2. 支持重复导入
   1. 在导入时，假如原资源（Tileset、Scene等）已经存在，在原数据的基础上修改
   2. 通过前缀名区分节点是`用户节点`还是`LDTK节点`，也用来区分是`用户元数据`还是`LDTK元数据`

目前插件选用的是方案1的思路，如果有更好的思路，欢迎一起探讨！

# 💣 TODO

- [ ] 运行时
  - [ ] 支持运行动态修改`IntGrid`，并根据`[LDTK Auto-layer rule definition](https://ldtk.io/json/#ldtk-AutoRuleDef)`实施更新并渲染受影响的`IntGrid`和`AutoLayer`
- [ ] World
  - [ ] 导入后处理脚本支持
  - [ ] LDTK [Multi-worlds](https://github.com/deepnight/ldtk/issues/231) 支持
  - [ ] LDTK 默认`Level`背景色支持
- [ ] Level
  - [ ] 支持`Level`背景色和背景图的导入
  - [ ] LDTK Level fields
  - [ ] `Level` 导入后处理脚本支持
- [ ] Entity
  - [ ] Entity视觉显示支持（`Sprite2D`）
  - [ ] `Entity`导入后处理脚本支持
  - [ ] Enum支持