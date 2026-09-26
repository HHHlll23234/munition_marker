# Munition Markers（弹药标记）

Sea Power 0.8.x 的 BepInEx / AnchorChain 插件，版本 1.0.0。

## 1. 这个插件是做什么的

游戏原版战术地图只标绘"接触"里的一部分单位。**非制导炸弹**、**无制导航空火箭弹**
（`AerialRocket`）以及**反潜火箭深弹 RBU** 虽然会被己方跟踪，但被刻意从地图上隐藏。
本插件把它们当作普通接触标绘出来，并复用现有的导弹 / 鱼雷图标。

导弹、鱼雷、制导炸弹、声呐浮标**本来就显示**，本插件不改变它们。

## 2. 原理

- 战术地图的单位列表在 `SeapowerUI.MapKnownUnits` 里由
  `taskforce.PlottingTable.Vehicles.Filtering(u => u.IsVisible())` 构建。
- `SeaPower.Vehicle.IsVisible()` 对上述弹药返回 `false`；
  `SeaPower.Vehicle.GetMapType()` 对它们返回 `MapVisualType.Invisible`。
- 插件用 Harmony 后置补丁（postfix）在这两个方法返回"隐藏/不可见"时，
  只对目标类型改回"可见"，并给出一个已有符号类型（炸弹/火箭→`Missile`，RBU→`Torpedo`）。

XAML 里只有枚举中已存在的图标，所以复用现有图标；新增独立图标需要改内嵌的 Noesis 资源，不在本版范围。

## 3. 覆盖范围

| 类别 | 对象类型 | 本插件 |
| --- | --- | --- |
| 非制导炸弹 | `Bomb`（`GuidanceType=None`） | ✅ |
| 无制导航空火箭 | `AerialRocket`（`UnguidedRocket`） | ✅ |
| 反潜火箭深弹 | `RBU` | ✅ |
| 导弹 / 鱼雷 / 制导炸弹 | `Missile` / `Torpedo` / `ASROC` | 原版已显示，不处理 |
| 舰炮 / CIWS / AAA 炮弹 | `SeaPower.Projectile`（`ProjectileManager`，非 `ObjectBase`） | ❌ 不在绘图桌上，需另做叠加图层 |

> 注意：`IsVisible()` 也被 AI 空袭目标列表和"新接触"报告使用。空袭目标只接受
> `Vessel`/`LandUnit`，不受影响；`ShowBothSides=false` 可避免为敌方弹药产生接触报告。

## 4. 构建

```powershell
# 自动探测 Steam 库
dotnet build -c Release

# 或指定游戏目录
dotnet build -c Release -p:SeaPowerDir="F:\SteamLibrary\steamapps\common\Sea Power"

# 构建并部署到游戏的 StreamingAssets
.\build.ps1 -Deploy
```

部署位置：`<Sea Power>\Sea Power_Data\StreamingAssets\MunitionMarkers\`。

AnchorChain 会扫描 `StreamingAssets` 下所有子目录里的 DLL，放进去即可。

## 5. 配置

首次运行后生成：`BepInEx\config\MunitionMarkers.cfg`

| 键 | 默认 | 说明 |
| --- | --- | --- |
| `Markers.ShowBombs` | true | 显示非制导炸弹 |
| `Markers.ShowRockets` | true | 显示无制导航空火箭弹 |
| `Markers.ShowRbu` | true | 显示反潜火箭深弹 |
| `Markers.ShowBothSides` | true | true=双方；false=仅己方 |

## 6. 验证

启动游戏后看 `BepInEx\LogOutput.log`，应出现：

```
[MunitionMarkers] loaded. Patched 2 classes, 0 failed. bombs=True, rockets=True, rbu=True, bothSides=True
```

进入任务，发射火箭 / 投弹 / 反潜，打开战术地图即可看到对应标记。
