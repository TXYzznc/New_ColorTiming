// 文件职责：定义跨 Boss 通用播放器使用的稳定音效语义标识。
// 所属模块：ColorTiming / Presentation / Audio。

using System;

namespace ColorTiming.Presentation.Audio
{
    /// <summary>运行时只读的 Boss 音效语义标识；实际音频引用由 Catalog 提供。</summary>
    public readonly struct BossSoundCueId : IEquatable<BossSoundCueId>
    {
        private readonly string _value;

        public BossSoundCueId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Boss sound cue id cannot be empty.", nameof(value));
            }

            _value = value;
        }

        public string Value => _value;

        public bool Equals(BossSoundCueId other) =>
            string.Equals(_value, other._value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is BossSoundCueId other && Equals(other);

        public override int GetHashCode() => _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

        public override string ToString() => _value ?? string.Empty;

        public static bool operator ==(BossSoundCueId left, BossSoundCueId right) => left.Equals(right);

        public static bool operator !=(BossSoundCueId left, BossSoundCueId right) => !left.Equals(right);
    }
}
