# FloatingIceSeal 浮冰海豹

一款基于 Unity 的 2D 水下平台游戏。玩家控制海豹在浮冰、障碍物和危险水域之间穿梭，利用气泡推进、管理氧气与生命值，并通过关卡挑战完成收束。

## 项目概述

- 2D platformer / action puzzle 游戏
- 以浮冰、气泡推进、水下移动和关卡机制组合为核心玩法
- 包含多种危险对象、关卡重置、存档切换与场景流转
- 以“可扩展的游戏系统设计”为重点，重视模块化与代码结构

## 技术栈

- Unity 2022.3.61f1
- C# / Unity Script
- URP 2D Renderer
- Tilemap + 2D Physics
- uGUI + TextMesh Pro
- Odin Inspector
- State Machine 状态机
- GameEvents 事件系统
- Information Pool 数据共享层
- MVC UI Framework
- 自定义关卡机制与水体检测工具

## 核心系统

- 角色状态控制：Life / Oxygen / Environment / Action 四套状态机
- 模块解耦：事件总线实现不同系统间通信
- 数据共享：统一信息池管理当前状态、关卡、存档和运行时数据
- UI 架构：菜单、设置、存档切换基于 MVC 设计
- 关卡系统：可重置关卡、动态物体恢复、多个关卡场景切换
- 水体与障碍：浮冰、水草、铁丝网、尖刺、水雷、木箱等机制设计

## 玩法亮点

- 水下气泡推进系统
- 氧气与生命值管理
- 多样化障碍与触发式关卡设计
- 多存档槽位与关卡重置流程
- 关卡元素与视觉效果结合，提升整体体验

## 项目结构

```text
Assets/
├── Scenes/
├── Scripts/
│   ├── Character/
│   ├── Bubble/
│   ├── Events/
│   ├── InformationPool/
│   ├── MVC/
│   ├── SenceObjects/
│   ├── GameProcess/
│   └── UI/
├── Prefabs/
├── Resources/
└── Rendering/
```

## 说明

这是一个面向 Unity 游戏开发与系统设计的个人项目，重点体现了：

- 玩法逻辑实现
- 模块化架构设计
- 事件驱动与状态管理
- 2D 关卡与交互系统整合
- UI 与数据流管理

## 运行方式

1. 使用 Unity Hub 打开项目
2. 选择 Unity 2022.3.61f1
3. 打开场景后直接运行即可
