// 文件职责：定义一次攻击及其伤害阶段的运行时上下文，不依赖 Unity 场景对象。
// 所属模块：ColorTiming / Domain / Combat。

using System;
using System.Collections.Generic;
using System.Threading;

namespace ColorTiming.Combat
{
    public enum NestedDamagePhasePolicy
    {
        InheritParent = 0,
        CreateNewPhase = 1,
    }

    public readonly struct DamagePhaseId : IEquatable<DamagePhaseId>
    {
        public DamagePhaseId(long executionId, int phaseIndex)
        {
            ExecutionId = executionId;
            PhaseIndex = phaseIndex;
        }

        public long ExecutionId { get; }
        public int PhaseIndex { get; }
        public bool Equals(DamagePhaseId other) => ExecutionId == other.ExecutionId && PhaseIndex == other.PhaseIndex;
        public override bool Equals(object obj) => obj is DamagePhaseId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ExecutionId, PhaseIndex);
        public override string ToString() => $"{ExecutionId}:{PhaseIndex}";
    }

    /// <summary>
    /// One authored damage phase. The per-target set belongs to the phase, rather than to any
    /// particular Collider or spawned effect, so sibling hitboxes cannot double-deliver damage.
    /// </summary>
    public sealed class DamagePhaseContext
    {
        private static long nextExecutionId;
        private readonly DamageExecutionContext execution;
        private readonly HashSet<int> deliveredTargetHandles = new HashSet<int>();

        private DamagePhaseContext(DamageExecutionContext execution, int phaseIndex)
        {
            this.execution = execution;
            Id = new DamagePhaseId(execution.Id, phaseIndex);
        }

        public DamagePhaseId Id { get; }

        public static DamagePhaseContext CreateRoot()
        {
            var execution = new DamageExecutionContext(Interlocked.Increment(ref nextExecutionId));
            return new DamagePhaseContext(execution, 1);
        }

        public DamagePhaseContext CreateNextPhase() =>
            new DamagePhaseContext(execution, execution.AllocateNextPhaseIndex());

        public bool TryRegisterTarget(int targetHandle) => deliveredTargetHandles.Add(targetHandle);

        private sealed class DamageExecutionContext
        {
            private int nextPhaseIndex = 1;

            public DamageExecutionContext(long id) => Id = id;
            public long Id { get; }
            public int AllocateNextPhaseIndex() => Interlocked.Increment(ref nextPhaseIndex);
        }
    }
}
