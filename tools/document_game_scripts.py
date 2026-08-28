#!/usr/bin/env python3
"""为 Assets/Game/Scripts 生成目录 README，并补充中文职责与关键方法注释。"""

from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[1]
SCRIPT_ROOT = PROJECT_ROOT / "Assets/Game/Scripts"
HEADER_MARKER = "// 文件职责："


DIRECTORY_PURPOSES = {
    ".": "汇总项目通用扩展与 ColorTiming 业务代码，是业务接入 GF_X 框架的脚本根目录。",
    "ColorTiming": "ColorTiming 项目业务代码根目录，按应用、领域、基础设施和表现层组织。",
    "ColorTiming/Application": "编排业务用例与战斗会话，不直接依赖具体 Unity 表现组件。",
    "ColorTiming/Application/Battle": "维护战斗会话、快照和会话消费者契约。",
    "ColorTiming/Application/Ports": "定义应用层访问时间等外部能力的端口。",
    "ColorTiming/Bootstrap": "负责 ColorTiming 运行时组合、场景锚点和启动流程。",
    "ColorTiming/Bootstrap/Flow": "封装业务场景标识及场景切换流程。",
    "ColorTiming/Domain": "存放不依赖 Unity 表现层的核心业务规则。",
    "ColorTiming/Domain/Bosses": "聚合不同 Boss 的纯业务战斗规则。",
    "ColorTiming/Domain/Bosses/Boss1": "实现 Boss1 的距离判断、攻击选择和攻击循环。",
    "ColorTiming/Domain/Bosses/Boss2": "实现 Boss2 的行动选择、阶段和遁地流程。",
    "ColorTiming/Domain/Combat": "实现生命、伤害、弱点、武器词汇和时间协调等战斗规则。",
    "ColorTiming/Domain/Player": "实现玩家动作、攻击门控、武器库存和生成策略。",
    "ColorTiming/Infrastructure": "存放对框架与 Unity API 的具体适配实现。",
    "ColorTiming/Infrastructure/GF": "聚合 GF_X 的音频、实体、设置和 UI 适配器。",
    "ColorTiming/Infrastructure/GF/Audio": "将业务音频请求接入 GF.Sound。",
    "ColorTiming/Infrastructure/GF/Entity": "将临时业务对象的创建与回收接入 GF.Entity。",
    "ColorTiming/Infrastructure/GF/Settings": "通过 GF.Setting 读写项目设置。",
    "ColorTiming/Infrastructure/GF/UI": "将业务 UI 请求接入 GF.UI 表单生命周期。",
    "ColorTiming/Infrastructure/Unity": "聚合直接依赖 Unity API 的边界适配器。",
    "ColorTiming/Infrastructure/Unity/Input": "把 Unity 输入与屏幕坐标转换适配为业务输入接口。",
    "ColorTiming/Infrastructure/Unity/Time": "把 Unity 时间缩放能力适配为应用层时间端口。",
    "ColorTiming/Input": "定义语义化输入状态、帧快照和输入消费者契约。",
    "ColorTiming/Presentation": "存放 MonoBehaviour、UI 和视觉音频等运行时表现。",
    "ColorTiming/Presentation/Actors": "聚合玩家和 Boss 的场景角色表现及目标绑定契约。",
    "ColorTiming/Presentation/Actors/Boss1": "负责 Boss1 动画、感知、音效与战斗表现。",
    "ColorTiming/Presentation/Actors/Boss2": "负责 Boss2 本体、尾部、动画事件和音效表现。",
    "ColorTiming/Presentation/Actors/Player": "负责玩家动画、镜头、技能、死亡和音效表现。",
    "ColorTiming/Presentation/Audio": "定义业务音频服务与 UI 音效接收契约。",
    "ColorTiming/Presentation/Camera": "负责业务镜头视差及 URP 相机堆栈维护。",
    "ColorTiming/Presentation/Combat": "聚合战斗伤害接收契约和技能、武器表现。",
    "ColorTiming/Presentation/Combat/Skills": "实现战斗技能、命中特效及其实体生命周期表现。",
    "ColorTiming/Presentation/Combat/Weapons": "负责武器生成、拾取及不同 Boss 的生成表现。",
    "ColorTiming/Presentation/Entities": "定义临时实体服务及框架实体参与者契约。",
    "ColorTiming/Presentation/UI": "聚合 ColorTiming 的 GF.UI 表单、组件、模型和表现协调器。",
    "ColorTiming/Presentation/UI/Components": "存放可复用的界面组件、血量显示和 UI 音效桥接。",
    "ColorTiming/Presentation/UI/Contracts": "定义业务 UI 服务、表单能力和伤害信号契约。",
    "ColorTiming/Presentation/UI/Forms": "实现由 GF.UI 管理的完整业务表单。",
    "ColorTiming/Presentation/UI/Models": "定义 UI 展示使用的轻量状态和值对象。",
    "ColorTiming/Presentation/UI/Presenters": "将战斗快照转换为 HUD 可消费的展示状态。",
    "ColorTiming/Presentation/Utilities": "提供仅供表现层使用的小型通用函数。",
    "ColorTiming/Presentation/World": "负责草地交互等场景世界表现。",
    "ColorTiming/Settings": "定义 ColorTiming 设置读取与消费者契约。",
    "Common": "存放常量、引用参数、布局和 DOTween 等通用运行时组件。",
    "DataTable": "聚合 GF 数据表行模型。",
    "DataTable/Core": "定义框架启动和资源索引需要的核心数据表结构。",
    "Entity": "存放通用实体逻辑与具体实体类型。",
    "Entity/Core": "定义实体基类和实体参数。",
    "EventArgs": "定义项目通用的 GF 事件类型与事件参数。",
    "Extension": "存放对 GF_X、Unity 和常用组件的通用扩展方法。",
    "Extension/Animation": "提供 Animation 组件相关扩展。",
    "Extension/AwaitExtension": "把 GF 异步回调封装为可等待操作及结果对象。",
    "Extension/DataModel": "提供数据模型的注册、存储和生命周期组件。",
    "Extension/Variable": "提供 GF Variable 的项目类型封装。",
    "Extension/VariablePool": "管理运行时共享变量及延迟释放。",
    "Network": "定义网络包、包头、处理器和网络通道辅助器。",
    "Network/Packet": "存放具体客户端与服务端网络包。",
    "Network/PacketHandler": "存放服务端下行包的具体处理器。",
    "Procedures": "定义框架启动、预加载和场景切换流程。",
    "ScriptableObject": "存放项目级 ScriptableObject 配置入口。",
    "UI": "聚合项目通用 UI 基础设施。",
    "UI/Core": "定义 GF.UI 表单、UI 项、参数和视图索引等基础类型。",
}


FILE_ROLE_OVERRIDES = {
    "HotfixEntry": "提供热更程序集的统一初始化入口；当前也作为项目脚本启动边界。",
    "RuntimeObjectNaming": "统一运行时创建或复用对象的 Clone 后缀命名规则。",
    "AssemblyInfo": "向测试程序集开放 ColorTiming 内部类型。",
    "BattleSession": "协调单场战斗的生命周期、领域状态和展示快照。",
    "BattleRuntimeContext": "在场景中建立并释放战斗会话及运行时依赖。",
    "ColorTimingCompositionRoot": "集中创建、注入和释放 ColorTiming 运行时服务。",
    "ColorTimingStartupProcedure": "把 ColorTiming 业务启动流程接入 GF Procedure。",
    "GfColorTimingUiService": "通过 GF.UI 打开、关闭并跟踪 ColorTiming 业务表单。",
    "GfColorTimingSoundService": "通过 GF.Sound 播放、停止和管理业务音频。",
    "GfTransientEntityService": "通过 GF.Entity 创建和回收短生命周期业务实体。",
    "LegacyGameInputAdapter": "把旧版 Unity Input 读取转换为语义化游戏输入。",
    "GameplayPointerWorldAdapter": "把屏幕指针坐标转换为指定平面的世界坐标。",
    "DOTweenSequence": "提供可在 Inspector 配置并按顺序播放的 DOTween 动画组件。",
    "FlowLayoutGroup": "实现可换行排列子节点的自定义 UGUI 布局组。",
    "DataTableExtension": "提供数据表加载、查询和资源索引辅助方法。",
    "AwaitExtension": "将 GF 资源、场景、下载和网络操作转换为可等待任务。",
    "UIFormBase": "封装 GF.UI 表单的绑定、动画、关闭和资源清理生命周期。",
    "PreloadProcedure": "按配置预加载数据表、字典、字体和必要资源。",
    "NetworkChannelHelper": "连接 GF.Network 与项目 Protobuf 包的序列化流程。",
}


EXACT_METHOD_DESCRIPTIONS = {
    "Awake": "缓存本组件依赖，并完成不依赖外部服务的本地初始化。",
    "Start": "在首帧启动依赖就绪后的业务或表现流程。",
    "OnEnable": "组件启用时注册监听并同步当前状态。",
    "OnDisable": "组件停用时解除监听并停止临时流程。",
    "OnDestroy": "组件销毁时释放订阅、句柄和运行时资源。",
    "Update": "逐帧推进需要实时刷新的业务或表现状态。",
    "LateUpdate": "在普通帧更新完成后同步最终表现状态。",
    "FixedUpdate": "按物理帧推进与刚体或碰撞相关的状态。",
    "OnValidate": "在编辑器校验序列化配置并修正可安全归一化的值。",
    "Reset": "恢复组件的默认配置或初始运行状态。",
    "Dispose": "释放本对象持有的订阅、服务和临时资源。",
    "Clear": "清空当前保存的运行时状态，使对象可安全复用。",
    "OnInit": "在 GF 对象首次初始化时建立持久引用。",
    "OnOpen": "在 GF UI 表单打开时接收参数并刷新显示。",
    "OnClose": "在 GF UI 表单关闭时停止流程并清理临时状态。",
    "OnPause": "在 GF UI 表单暂停时冻结当前交互或表现。",
    "OnResume": "在 GF UI 表单恢复时重新同步交互和显示。",
    "OnRecycle": "对象回收前清理与本次使用相关的状态。",
    "OnShow": "实体显示时读取参数并建立本次生命周期状态。",
    "OnHide": "实体隐藏时清理本次显示产生的运行时状态。",
    "OnUpdate": "由 GF 生命周期逐帧推进当前对象状态。",
    "Enter": "进入当前流程或状态，并执行必要的初始化。",
    "Leave": "离开当前流程或状态，并释放阶段性资源。",
    "Shutdown": "停止服务并释放其管理的运行时资源。",
    "Tick": "按当前时间步推进核心状态，并发布必要的状态变化。",
    "Create": "创建并初始化新的实例。",
    "Get": "获取当前保存的值。",
    "Set": "写入新的值并替换旧状态。",
    "Acquire": "申请一个受控作用域，并返回用于释放的句柄。",
    "Pulse": "创建一次限时请求，并按持续时间自动结束。",
    "Play": "启动当前配置的动画、音频或其他表现。",
    "Handle": "处理收到的数据或事件，并更新相关状态。",
    "Release": "释放当前对象及其持有的临时资源。",
    "ResetProperties": "重置对象属性，使实例可以安全复用。",
    "EnsureCloneSuffix": "确保运行时对象名称带有 Clone 后缀。",
    "StartHotfixLogic": "启动项目脚本入口并注册业务流程。",
    "ClickUIButton": "处理 UI 按钮点击并触发配置的反馈。",
    "SubscribeEvent": "订阅完成当前流程所需的框架事件。",
    "ReloadInstanceEditor": "在编辑器中重新加载配置单例。",
}


TOKEN_TRANSLATIONS = {
    "Battle": "战斗", "Boss": "Boss", "Player": "玩家", "Health": "生命值", "Damage": "伤害",
    "Weapon": "武器", "Weakness": "弱点", "Input": "输入", "State": "状态", "Session": "会话",
    "Scene": "场景", "Flow": "流程", "Form": "表单", "View": "视图", "UI": "UI", "Ui": "UI",
    "Sound": "音效", "Audio": "音频", "Entity": "实体", "Resource": "资源", "Settings": "设置",
    "Setting": "设置", "Time": "时间", "Camera": "相机", "Target": "目标", "Result": "结果",
    "Animation": "动画", "Progress": "进度", "Loading": "加载", "Menu": "菜单", "Pause": "暂停",
    "Runtime": "运行时", "Context": "上下文", "Data": "数据", "Model": "模型", "Table": "数据表",
    "Packet": "网络包", "Channel": "通道", "Value": "值", "Position": "位置", "Scale": "缩放",
    "Color": "颜色", "Type": "类型", "Action": "动作", "Attack": "攻击", "Skill": "技能",
    "Spawn": "生成", "Pickup": "拾取", "Pointer": "指针", "World": "世界坐标", "Frame": "帧",
    "Snapshot": "快照", "Presentation": "展示", "Lifecycle": "生命周期", "Config": "配置",
    "Localization": "本地化", "Language": "语言", "Download": "下载", "Request": "请求",
    "Event": "事件", "Handler": "处理器", "Group": "分组", "Item": "项目", "Sequence": "序列",
    "Transform": "变换", "Layout": "布局", "Text": "文本", "Effect": "效果", "Random": "随机源",
    "Paused": "暂停状态", "Move": "移动输入", "Moving": "移动状态", "Active": "激活状态",
    "Begin": "开始", "End": "结束", "Dash": "冲刺", "Drop": "丢弃", "Held": "按住状态",
    "Invulnerable": "无敌状态", "Successful": "成功", "Remaining": "剩余数量", "Current": "当前项",
    "Base": "基类", "Consumer": "消费者", "Coordinator": "协调器", "Queue": "队列", "Slot": "槽位",
    "Ledger": "记录器", "Vocabulary": "词汇", "Identity": "标识", "Contracts": "契约", "Anchors": "锚点",
    "Id": "ID", "Ids": "ID", "Requested": "请求", "Changed": "变化", "Inspector": "检视面板",
    "Row": "行", "Pending": "待处理", "Success": "成功", "Failure": "失败", "Complete": "完成",
    "Fail": "失败", "Cooldown": "冷却", "Head": "本体", "Tail": "尾部", "Hidden": "隐藏状态",
    "Entering": "进入阶段", "Emerging": "出现阶段", "Observe": "观察", "Count": "数量", "Of": "",
}


KEY_PRIVATE_PREFIXES = (
    "On", "Handle", "Refresh", "Update", "Apply", "Build", "Create", "Initialize", "Bind", "Unbind",
    "Register", "Unregister", "Spawn", "Release", "Play", "Stop", "Show", "Hide", "Open", "Close",
    "Load", "Save", "Resolve", "Select", "Execute", "Tick", "Change", "Enter", "Exit", "Try", "Set",
)


SKIP_METHOD_NAMES = {"Equals", "GetHashCode", "ToString"}


GENERATED_COMMENT_PATTERNS = (
    re.compile(r"^// 初始化.+实例及其核心依赖。$"),
    re.compile(r"^// 执行.+对应的主要流程。$"),
    re.compile(r"^// 尝试.+，并通过返回值报告是否成功。$"),
    re.compile(r"^// 获取.+。$"),
    re.compile(r"^// 设置.+，并使后续流程使用最新状态。$"),
    re.compile(r"^// 创建.+并完成必要的初始配置。$"),
    re.compile(r"^// 根据当前配置构建.+。$"),
    re.compile(r"^// 初始化.+及其依赖关系。$"),
    re.compile(r"^// 根据最新数据刷新.+。$"),
    re.compile(r"^// 根据当前状态更新.+。$"),
    re.compile(r"^// 把当前规则或配置应用到.+。$"),
    re.compile(r"^// 处理.+，并同步受影响的状态。$"),
    re.compile(r"^// 响应.+回调，并更新本对象状态。$"),
    re.compile(r"^// 绑定.+依赖或事件监听。$"),
    re.compile(r"^// 解除.+依赖或事件监听。$"),
    re.compile(r"^// 注册.+，使其加入当前生命周期管理。$"),
    re.compile(r"^// 注销.+，避免残留引用或重复回调。$"),
    re.compile(r"^// 生成.+并交给对应生命周期系统管理。$"),
    re.compile(r"^// 释放.+及其临时资源。$"),
    re.compile(r"^// 播放.+对应的动画、音频或表现。$"),
    re.compile(r"^// 停止.+并清理临时播放状态。$"),
    re.compile(r"^// 显示.+并同步当前数据。$"),
    re.compile(r"^// 隐藏.+并停止相关交互。$"),
    re.compile(r"^// 打开.+并传入本次使用参数。$"),
    re.compile(r"^// 关闭.+并结束本次生命周期。$"),
    re.compile(r"^// 加载.+，并处理完成或失败结果。$"),
    re.compile(r"^// 保存.+的当前状态。$"),
    re.compile(r"^// 解析.+并返回可供上层使用的结果。$"),
    re.compile(r"^// 根据当前规则选择.+。$"),
    re.compile(r"^// 添加.+并维护相关集合状态。$"),
    re.compile(r"^// 移除.+并清理相关引用。$"),
    re.compile(r"^// 清空.+的运行时状态。$"),
)


def read_text_preserved(path: Path) -> tuple[str, str, bool]:
    data = path.read_bytes()
    has_bom = data.startswith(b"\xef\xbb\xbf")
    text = data.decode("utf-8-sig")
    newline = "\r\n" if b"\r\n" in data else "\n"
    return text, newline, has_bom


def write_text_preserved(path: Path, text: str, newline: str, has_bom: bool) -> None:
    normalized = text.replace("\r\n", "\n").replace("\r", "\n").replace("\n", newline)
    payload = normalized.encode("utf-8")
    if has_bom:
        payload = b"\xef\xbb\xbf" + payload
    path.write_bytes(payload)


def split_identifier(name: str) -> list[str]:
    name = re.sub(r"[^A-Za-z0-9]+", " ", name)
    parts: list[str] = []
    for chunk in name.split():
        parts.extend(re.findall(r"[A-Z]+(?=[A-Z][a-z]|\d|$)|[A-Z]?[a-z]+|\d+", chunk))
    return parts


def translate_subject(name: str) -> str:
    parts = split_identifier(name)
    if not parts:
        return "相关状态"
    composite = "".join(parts)
    composite_overrides = {
        "ColorTiming": "ColorTiming",
        "RandomSource": "随机源",
        "GameTime": "游戏时间",
        "DataRow": "数据行",
        "UIForm": "UI 表单",
    }
    if composite in composite_overrides:
        return composite_overrides[composite]
    translated: list[str] = []
    index = 0
    while index < len(parts):
        if index + 1 < len(parts) and parts[index] == "Color" and parts[index + 1] == "Timing":
            translated.append("ColorTiming")
            index += 2
            continue
        translated.append(TOKEN_TRANSLATIONS.get(parts[index], parts[index]))
        index += 1
    return "".join(translated)


def module_label(path: Path) -> str:
    relative = path.parent.relative_to(SCRIPT_ROOT).as_posix()
    return "Scripts 根模块" if relative == "." else relative.replace("/", " / ")


def file_role(path: Path) -> str:
    stem = path.stem
    if stem in FILE_ROLE_OVERRIDES:
        return FILE_ROLE_OVERRIDES[stem]
    parent = path.parent.name
    if stem.startswith("I") and len(stem) > 1 and stem[1].isupper():
        return f"定义 {translate_subject(stem[1:])} 的依赖契约，供模块间解耦使用。"
    if stem.endswith("Extension"):
        return f"提供 {translate_subject(stem[:-9])} 相关的通用扩展方法。"
    if stem.endswith("Form"):
        return f"实现 {translate_subject(stem[:-4])} GF.UI 表单及其交互生命周期。"
    if stem.endswith("View"):
        return f"负责 {translate_subject(stem[:-4])} 的场景或界面表现。"
    if stem.endswith("Service"):
        return f"实现 {translate_subject(stem[:-7])} 服务，并管理对应运行时资源。"
    if stem.endswith("Adapter"):
        return f"把 {translate_subject(stem[:-7])} 的具体实现适配到上层接口。"
    if stem.endswith("Procedure"):
        return f"实现 {translate_subject(stem[:-9])} 的 GF 流程节点。"
    if stem.endswith("Table"):
        return f"定义 {translate_subject(stem[:-5])} 数据表的行结构与解析规则。"
    if stem.endswith("Params"):
        return f"承载 {translate_subject(stem[:-6])} 创建或调用所需参数。"
    if stem.endswith("Logic"):
        return f"实现 {translate_subject(stem[:-5])} 的核心业务规则。"
    if stem.endswith("State") or stem.endswith("Snapshot") or stem.endswith("Result"):
        return f"定义 {translate_subject(stem)} 数据及其状态语义。"
    if parent == "Skills":
        return f"实现战斗技能 {stem} 的运行时表现和回收行为。"
    if parent in {"Packet", "PacketHandler"} or "Packet" in stem:
        return f"定义 {translate_subject(stem)} 的网络传输或处理行为。"
    return f"定义 {translate_subject(stem)}，承担 {translate_subject(parent)} 模块中的对应职责。"


def method_description(name: str) -> str:
    if name in EXACT_METHOD_DESCRIPTIONS:
        return EXACT_METHOD_DESCRIPTIONS[name]
    rules = (
        ("Try", "尝试{0}，并通过返回值报告是否成功。"),
        ("Get", "获取{0}。"),
        ("Set", "设置{0}，并使后续流程使用最新状态。"),
        ("Create", "创建{0}并完成必要的初始配置。"),
        ("Build", "根据当前配置构建{0}。"),
        ("Initialize", "初始化{0}及其依赖关系。"),
        ("Refresh", "根据最新数据刷新{0}。"),
        ("Update", "根据当前状态更新{0}。"),
        ("Apply", "把当前规则或配置应用到{0}。"),
        ("Handle", "处理{0}，并同步受影响的状态。"),
        ("On", "响应{0}回调，并更新本对象状态。"),
        ("Bind", "绑定{0}依赖或事件监听。"),
        ("Unbind", "解除{0}依赖或事件监听。"),
        ("Register", "注册{0}，使其加入当前生命周期管理。"),
        ("Unregister", "注销{0}，避免残留引用或重复回调。"),
        ("Spawn", "生成{0}并交给对应生命周期系统管理。"),
        ("Release", "释放{0}及其临时资源。"),
        ("Play", "播放{0}对应的动画、音频或表现。"),
        ("Stop", "停止{0}并清理临时播放状态。"),
        ("Show", "显示{0}并同步当前数据。"),
        ("Hide", "隐藏{0}并停止相关交互。"),
        ("Open", "打开{0}并传入本次使用参数。"),
        ("Close", "关闭{0}并结束本次生命周期。"),
        ("Load", "加载{0}，并处理完成或失败结果。"),
        ("Save", "保存{0}的当前状态。"),
        ("Resolve", "解析{0}并返回可供上层使用的结果。"),
        ("Parse", "解析{0}并写入当前数据结构。"),
        ("Read", "从数据流读取并解析{0}。"),
        ("Select", "根据当前规则选择{0}。"),
        ("Execute", "执行{0}对应的完整流程。"),
        ("Tick", "按当前时间步推进{0}。"),
        ("Add", "添加{0}并维护相关集合状态。"),
        ("Remove", "移除{0}并清理相关引用。"),
        ("Clear", "清空{0}的运行时状态。"),
    )
    for prefix, template in rules:
        if name.startswith(prefix) and len(name) > len(prefix):
            return template.format(translate_subject(name[len(prefix):]))
    return f"执行{translate_subject(name)}对应的主要流程。"


def preceding_has_chinese_comment(lines: list[str], index: int) -> bool:
    for line in lines[max(0, index - 4):index]:
        stripped = line.strip()
        if stripped.startswith("//") and re.search(r"[\u4e00-\u9fff]", stripped):
            return True
    return False


def find_method_candidates(lines: list[str]) -> list[tuple[int, str, str, bool]]:
    candidates: list[tuple[int, str, str, bool]] = []
    control_words = {"if", "for", "foreach", "while", "switch", "catch", "using", "lock", "return"}
    index = 0
    while index < len(lines):
        original = lines[index]
        stripped = original.strip()
        if not stripped or stripped.startswith(("//", "/*", "*", "#", "[")):
            index += 1
            continue
        # 仅处理参数括号出现在首行的方法声明，避免把属性/字段与后续方法跨行拼接。
        if "(" not in stripped:
            index += 1
            continue
        start = index
        signature_parts = [stripped]
        paren_balance = stripped.count("(") - stripped.count(")")
        while index + 1 < len(lines) and (paren_balance > 0 or not re.search(r"[;{]\s*$|=>", " ".join(signature_parts))):
            if len(signature_parts) >= 8:
                break
            index += 1
            next_part = lines[index].strip()
            signature_parts.append(next_part)
            paren_balance += next_part.count("(") - next_part.count(")")
        signature = " ".join(signature_parts)
        signature = re.sub(r"\s+", " ", signature)
        if "(" not in signature or re.match(r"^(if|for|foreach|while|switch|catch|using|lock)\s*\(", signature):
            index += 1
            continue
        before_paren = signature.split("(", 1)[0].strip()
        if any(token in before_paren for token in ("=", "=>")) or re.search(r"\b(new|return|throw|await)\b", before_paren):
            index += 1
            continue
        name_match = re.search(r"(~?[A-Za-z_]\w*|operator\s*[^\s]+)\s*$", before_paren)
        if not name_match:
            index += 1
            continue
        name = name_match.group(1).replace(" ", "")
        if name in control_words or name in SKIP_METHOD_NAMES or "." in before_paren.split()[-1]:
            index += 1
            continue
        access_match = re.match(r"^(public|protected|internal|private)\b", signature)
        access = access_match.group(1) if access_match else ""
        declaration_prefix = before_paren[:name_match.start()].strip()
        declaration_prefix = re.sub(
            r"\b(public|protected|internal|private|static|virtual|override|abstract|async|sealed|new|extern|partial)\b",
            " ",
            declaration_prefix,
        )
        declaration_tokens = declaration_prefix.split()
        # 普通方法至少应在名称前包含返回类型；只有带访问修饰符的构造函数可以没有返回类型。
        if not declaration_tokens and not access:
            index += 1
            continue
        is_lifecycle = name in EXACT_METHOD_DESCRIPTIONS
        is_key_private = any(name.startswith(prefix) for prefix in KEY_PRIVATE_PREFIXES)
        ends_like_declaration = re.search(r"\)\s*(?:where\b[^{}]*)?(?:\{|=>|;)", signature) is not None
        is_interface_signature = not access and bool(declaration_tokens) and signature.rstrip().endswith(";")
        if ends_like_declaration and (
            access in {"public", "protected", "internal"}
            or is_lifecycle
            or (is_key_private and bool(declaration_tokens))
            or is_interface_signature
        ):
            is_constructor = bool(access) and not declaration_tokens
            candidates.append((start, name, original[: len(original) - len(original.lstrip())], is_constructor))
        index += 1
    return candidates


def document_script(path: Path) -> tuple[bool, int]:
    text, newline, has_bom = read_text_preserved(path)
    changed = False
    if not text.startswith(HEADER_MARKER):
        header = f"{HEADER_MARKER}{file_role(path)}{newline}// 所属模块：{module_label(path)}。{newline}{newline}"
        text = header + text
        changed = True
    lines = text.replace("\r\n", "\n").replace("\r", "\n").split("\n")
    inserted = 0
    for index, name, indent, is_constructor in reversed(find_method_candidates(lines)):
        if preceding_has_chinese_comment(lines, index):
            continue
        description = (
            f"初始化{translate_subject(name)}实例及其核心依赖。"
            if is_constructor
            else method_description(name)
        )
        lines.insert(index, f"{indent}// {description}")
        inserted += 1
    if inserted:
        text = "\n".join(lines)
        changed = True
    if changed:
        write_text_preserved(path, text, newline, has_bom)
    return changed, inserted


def is_generated_comment(line: str) -> bool:
    stripped = line.strip()
    if stripped.startswith((HEADER_MARKER, "// 所属模块：")):
        return True
    if stripped in {f"// {description}" for description in EXACT_METHOD_DESCRIPTIONS.values()}:
        return True
    return any(pattern.fullmatch(stripped) for pattern in GENERATED_COMMENT_PATTERNS)


def cleanup_generated_comments() -> int:
    cleaned = 0
    for script in sorted(SCRIPT_ROOT.rglob("*.cs")):
        text, newline, has_bom = read_text_preserved(script)
        lines = text.replace("\r\n", "\n").replace("\r", "\n").split("\n")
        filtered = [line for line in lines if not is_generated_comment(line)]
        while filtered and not filtered[0].strip():
            filtered.pop(0)
        if filtered != lines:
            write_text_preserved(script, "\n".join(filtered), newline, has_bom)
            cleaned += 1
    return cleaned


def make_readme(directory: Path) -> str:
    relative = directory.relative_to(SCRIPT_ROOT).as_posix()
    purpose = DIRECTORY_PURPOSES.get(relative, f"聚合 {directory.name} 相关脚本与子模块。")
    scripts = sorted(path.name for path in directory.glob("*.cs"))
    children = sorted(path.name for path in directory.iterdir() if path.is_dir())
    title = "Game Scripts" if relative == "." else directory.name
    lines = [f"# {title}", "", purpose]
    if scripts:
        lines.extend(["", "直接脚本：" + "、".join(f"`{name}`" for name in scripts) + "。"])
    else:
        lines.extend(["", "本目录不直接放置 C# 实现，仅用于组织下级模块。"])
    if children:
        lines.extend(["", "子目录：" + "、".join(f"`{name}`" for name in children) + "。"])
    lines.extend(["", "修改本目录代码后，应执行 Unity 编译和对应测试。", ""])
    return "\n".join(lines)


def restore_documented_scripts_from_head() -> int:
    restored = 0
    for script in sorted(SCRIPT_ROOT.rglob("*.cs")):
        current, _, _ = read_text_preserved(script)
        if not current.startswith(HEADER_MARKER):
            continue
        relative = script.relative_to(PROJECT_ROOT).as_posix()
        result = subprocess.run(
            ["git", "show", f"HEAD:{relative}"],
            cwd=PROJECT_ROOT,
            capture_output=True,
        )
        if result.returncode != 0:
            raise RuntimeError(f"无法从 HEAD 恢复 {relative}")
        script.write_bytes(result.stdout)
        restored += 1
    return restored


def main() -> None:
    if "--clean-generated" in sys.argv:
        print(f"cleaned_scripts={cleanup_generated_comments()}")

    if "--restore-head" in sys.argv:
        print(f"restored_scripts={restore_documented_scripts_from_head()}")

    directories = [SCRIPT_ROOT, *sorted(path for path in SCRIPT_ROOT.rglob("*") if path.is_dir())]
    readmes_written = 0
    for directory in directories:
        readme = directory / "README.md"
        content = make_readme(directory)
        if not readme.exists() or readme.read_text(encoding="utf-8-sig").replace("\r\n", "\n") != content:
            readme.write_text(content, encoding="utf-8", newline="\n")
            readmes_written += 1

    scripts_changed = 0
    method_comments = 0
    for script in sorted(SCRIPT_ROOT.rglob("*.cs")):
        changed, inserted = document_script(script)
        scripts_changed += int(changed)
        method_comments += inserted

    print(f"directories={len(directories)} readmes_written={readmes_written}")
    print(f"scripts={len(list(SCRIPT_ROOT.rglob('*.cs')))} scripts_changed={scripts_changed} method_comments={method_comments}")


if __name__ == "__main__":
    main()
