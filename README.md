# FloatingIceSeal 浮冰海豹

一款基于 Unity 的 2D 平台跳跃游戏。玩家操控小海豹,在浮冰与水域之间穿梭:吹出气泡帮助水下移动、管理氧气与生命值,躲避尖刺与水雷,突破木箱、水草、铁丝网等障碍,一路闯关抵达结局。

## 技术栈

| 项目 | 版本 |
|------|------|
| Unity | 2022.3.61f1 |
| 渲染管线 | URP 14.0.12(2D Renderer) |
| UI | uGUI + TextMesh Pro |
| 序列化扩展 | Odin Inspector |

## 项目结构

```
FloatingIceSeal/
├── Assets/
│   ├── Scenes/                  # 场景(主菜单/加载/关卡/结局)
│   │   ├── StartSence.unity     # 主菜单
│   │   ├── LoadingSence.unity   # 加载中转场景
│   │   ├── Level/               # 关卡 1-14(含教学关)
│   │   └── EndingSence.unity    # 结局
│   ├── Scripts/                 # 游戏脚本
│   │   ├── Animation/           # 动画控制
│   │   ├── Bubble/              # 气泡系统
│   │   ├── Character/           # 海豹角色(移动/吹泡/生命/氧气/状态机)
│   │   ├── Events/              # GameEvents 事件系统
│   │   ├── GameProcess/         # 关卡重置/存档/生成/音效音乐/关卡切换
│   │   ├── InformationPool/     # 信息池(跨模块共享数据)
│   │   ├── Interfaces/          # 通用接口(IDestroyable/ILevelResetable)
│   │   ├── MVC/                 # MVC UI 框架(主菜单/暂停/设置/切换存档)
│   │   ├── SenceObjects/        # 关卡机制物件(墙/浮冰/刺/水草/铁丝网/水体/木箱/水雷/小鱼)
│   │   └── UI/                  # HUD 等界面
│   ├── Prefabs/                 # 预制体
│   └── Resource/                # 材质/Shader 等资源
├── .github/skills/              # 开发技能文档(状态机/MVC/事件/机制扩展等)
├── _tools/                      # 开发辅助工具
├── _scene_backup_*/             # 场景备份
└── ProjectSettings/             # 项目设置(含自定义 Layer/Tag)
```

## 核心系统

- **状态机**:海豹拥有生命(Life)、氧气(Oxygen)、环境(Environment)、动作(Action)四套状态机。
- **GameEvents 事件系统**:模块间解耦通信,统一订阅/取消订阅/发布。
- **InformationPool 信息池**:跨模块共享数据(生命值、氧气、分数、当前关卡等),支持变更监听。
- **MVC UI 框架**:主菜单、暂停菜单、设置、切换存档均基于 BaseModel/BaseView/BaseController 建模。
- **关卡重置系统**:死亡或手动重置时,恢复瓦片快照与动态物件(位置/参数/子物体/渲染排序)。
- **存档系统**:最多 4 个存档槽位,支持创建/复制/删除/切换。
- **水体检测工具 WatterUtils**:向下射线检测水面、获取水面高度、判断是否在水下。

## 关卡机制

| 机制 | 说明 |
|------|------|
| Wall 墙 | 陆地判定 |
| FloatingIce 浮冰 | 在水中/空中以不同速度回归初始位置 |
| Spike 尖刺 | 冲刺碰撞伤害 |
| Seagrass 水草 | 物理阻挡 |
| SteelWireMesh 铁丝网 | 物理阻挡 |
| Watter 水体 | 触发判定,供水面检测 |
| WoodBox 木箱 | 浮力模拟,可被爆炸破坏 |
| Mine 水雷 | 检测范围内爆炸,连锁引爆,破坏可破坏瓦片 |
| SmallFish 小鱼 | 漫游,追踪玩家释放的大气泡 |

爆炸破坏基于 Stencil 蒙版:整图素材叠加在瓦片 Tilemap 之上,瓦片被炸毁后视觉同步出现缺口,关卡重置自动恢复。

## 自定义 Layer

| 名称 | Index |
|------|-------|
| Water | 4 |
| UI | 5 |
| Player | 6 |
| Bubble | 7 |
| SteelWireMesh | 8 |
| Seagrass | 9 |
| Watter | 10 |

## 运行

1. 使用 Unity Hub 打开项目,编辑器版本 `2022.3.61f1`。
2. 打开 `Assets/Scenes/StartSence.unity`,点击 Play 即可开始游戏。
3. 打包前请在 Build Settings 中确认场景顺序:StartSence → LoadingSence → Level 系列 → EndingSence。

## 开发指南

扩展功能前请先阅读 [.github/skills](.github/skills) 下的技能文档,涵盖状态机修改、事件系统接入、信息池使用、关卡机制扩展、MVC UI 建模、动画控制与水体检测等内容。

注意事项:
- 信息池/事件的注册尽量放在 `OnEnable`,注销放在 `OnDisable`;不要在 `OnEnable` 中依赖其它对象的注册顺序(使用 `Start` 或 `FindObjectOfType` 兜底)。
- 关卡名带后缀(如 `Level2_FWSW`),切换关卡请使用 `LevelSceneUtility.FindSceneName` 匹配,勿硬编码关卡名。
- 修改场景文件前建议使用 `_scene_backup_*` 目录备份。
