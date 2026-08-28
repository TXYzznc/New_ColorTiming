using System;
using System.Linq;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using NUnit.Framework;

namespace ColorTiming.Tests.EditMode
{
    public sealed class ArchitectureBoundaryTests
    {
        private static readonly string[] ForbiddenPrefixes =
        {
            "UnityEngine",
            "UnityGameFramework",
            "GameFramework",
            "Spine",
            "Cinemachine",
            "Hotfix",
        };

        [Test]
        public void DomainAndApplication_DoNotReferenceUnityOrGfAssemblies()
        {
            AssertBoundary(typeof(ActorId).Assembly);
            AssertBoundary(typeof(BattleSession).Assembly);
        }

        [Test]
        public void Application_ReferencesDomainButDomainDoesNotReferenceApplication()
        {
            var domain = typeof(ActorId).Assembly;
            var application = typeof(BattleSession).Assembly;
            Assert.That(application.GetReferencedAssemblies().Select(value => value.Name), Does.Contain(domain.GetName().Name));
            Assert.That(domain.GetReferencedAssemblies().Select(value => value.Name), Does.Not.Contain(application.GetName().Name));
        }

        private static void AssertBoundary(System.Reflection.Assembly assembly)
        {
            var forbidden = assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => ForbiddenPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                .ToArray();
            Assert.That(forbidden, Is.Empty, $"{assembly.GetName().Name} has forbidden references: {string.Join(", ", forbidden)}");
        }
    }
}
