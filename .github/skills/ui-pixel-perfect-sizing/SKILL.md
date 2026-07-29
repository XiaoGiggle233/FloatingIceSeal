---
name: ui-pixel-perfect-sizing
description: '根据 PixelPerfectCamera 参考分辨率调整 Canvas 和 UI 元素大小，使 UI 与游戏画面像素比例一致。Use when: 为像素风格游戏添加 UI、UI 大小与游戏世界不匹配、Canvas Scaler 配置需要匹配 PixelPerfectCamera、Screen Space - Camera 模式下 UI 被场景物体遮挡、需要根据摄像机参考分辨率调整按钮/文字大小。触发词：UI大小调整、PixelPerfectCamera、CanvasScaler、参考分辨率、UI缩放、ScreenSpaceCamera、UI遮挡、按钮大小、文字大小、像素游戏UI。'
argument-hint: '[Camera 或 UI 元素描述]'
---

# UI 像素完美缩放

当游戏使用 PixelPerfectCamera 时，UI Canvas 需要匹配其参考分辨率，使 UI 与游戏世界保持一致的像素比例。

## 核心原理

PixelPerfectCamera 以固定的参考分辨率（如 384×216）渲染游戏画面，然后放大到实际屏幕。CanvasScaler 的参考分辨率应与之一致，UI 元素的尺寸才能与游戏世界中的像素对应。

**关键映射关系：**
- UI 元素的 `RectTransform.sizeDelta` = 在参考分辨率中的像素尺寸

## 步骤

### 步骤 1：读取 PixelPerfectCamera 参数

```csharp
var cam = Camera.main;
var ppc = cam.GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>();
// 记录：ppc.refResolutionX, ppc.refResolutionY
```

### 步骤 2：配置 Canvas

```csharp
var canvas = GameObject.Find("Canvas");
var c = canvas.GetComponent<Canvas>();

// 使用 Screen Space - Camera 模式，指定主摄像机
c.renderMode = RenderMode.ScreenSpaceCamera;
c.worldCamera = Camera.main;
c.sortingOrder = 100;  // 确保 UI 渲染在场景物体之上
```

### 步骤 3：配置 CanvasScaler

```csharp
var scaler = canvas.GetComponent<CanvasScaler>();

scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.matchWidthOrHeight = 1f;  // 按高度匹配（推荐），或 0.5f 取平均
```

### 步骤 4：按虚拟像素设置 UI 元素尺寸

此后 UI 元素的尺寸直接使用虚拟像素值（对应参考分辨率中的像素数）。例如参考分辨率为 384×216 时：

| 元素 | 推荐尺寸 | 占比 |
|------|---------|------|
| 小按钮 | 40~50 × 18~24 | ~12% 宽 |
| 按钮文字 | 10~12 号字 | — |
| 边距 | ~10~20 | ~5% |

```csharp
var btn = GameObject.Find("ResetBtn");
var rect = btn.GetComponent<RectTransform>();

// 固定在右上角，尺寸和边距为虚拟像素
rect.anchorMin = new Vector2(1, 1);
rect.anchorMax = new Vector2(1, 1);
rect.pivot = new Vector2(0.5f, 0.5f);
rect.sizeDelta = new Vector2(44, 20);         // 按钮宽高（虚拟像素）
rect.anchoredPosition = new Vector2(-26, -14); // 距右上角偏移
```

### 步骤 5：确保 Canvas 渲染在场景之上

```csharp
// ScreenSpaceCamera 模式下，sortingOrder 决定 UI 与场景的渲染顺序
c.sortingOrder = 100;
```

## 常见问题

| 问题 | 原因 | 解决 |
|------|------|------|
| UI 被场景物体遮挡 | Canvas.sortingOrder 太低 | 设为 ≥ 100 |
| UI 大小与游戏不匹配 | CanvasScaler 参考分辨率与 PPC 不一致 | 对齐参考分辨率 |
| Canvas 渲染模式不对 | 误用 WorldSpace 或 Overlay | 使用 ScreenSpaceCamera |
 Scaler 配置不当或 UI 元素尺寸不对 | 检查 Canvas Scaler 设置和元素尺寸