// 文件职责：定义 ConstGroups，承担 Common 模块中的对应职责。
// 所属模块：Common。

//此代码由工具自动生成, 请勿手动修改
public static partial class Const
{
#if ENABLE_OBFUZ
	[Obfuz.ObfuzIgnore]
#endif
	public enum EntityGroup
	{
		Default,
		Effect,
		Persistent
	}
#if ENABLE_OBFUZ
	[Obfuz.ObfuzIgnore]
#endif
	public enum UIGroup
	{
		Default,
		Dialog,
		Overlay
	}
#if ENABLE_OBFUZ
	[Obfuz.ObfuzIgnore]
#endif
	public enum SoundGroup
	{
		Music,
		Sound,
		BGM,
		UI,
		Player,
		Boss,
		Environment
	}
}
