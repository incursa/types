// Copyright (c) Samuel McAravey
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;

namespace Incursa.Types.Tests;

public sealed class PrimitivePackageBoundaryTests
{
    private static readonly Type[] PrimitiveTypes =
    [
        typeof(BigDecimal),
        typeof(BigDecimalProfile),
        typeof(Duration),
        typeof(ElapsedDuration),
        typeof(FastId),
        typeof(LocalDateTime),
        typeof(Maybe<>),
        typeof(Money),
        typeof(Percentage),
        typeof(Period),
        typeof(ShortCode),
    ];

    [Fact]
    public void MovedTypesAreDefinedOnlyByPrimitivesAssembly()
    {
        Assembly primitivesAssembly = typeof(FastId).Assembly;

        primitivesAssembly.GetName().Name.ShouldBe("Incursa.Types.Primitives");
        PrimitiveTypes.ShouldAllBe(type => type.Assembly == primitivesAssembly);

        Assembly richTypesAssembly = typeof(BvFile).Assembly;
        richTypesAssembly.GetName().Name.ShouldBe("Incursa.Types");
        richTypesAssembly.GetReferencedAssemblies()
            .ShouldContain(reference => reference.Name == "Incursa.Types.Primitives");
    }

    [Fact]
    public void RichTypesAssemblyForwardsMovedPublicTypes()
    {
        Assembly primitivesAssembly = typeof(FastId).Assembly;
        Type[] forwardedTypes = typeof(BvFile).Assembly.GetForwardedTypes();

        primitivesAssembly.GetExportedTypes().ShouldAllBe(type => forwardedTypes.Contains(type));
    }

    [Fact]
    public void AssemblyQualifiedMoneyIdentityResolvesThroughRichPackageForwarder()
    {
        Type? resolvedType = Type.GetType("Incursa.Money, Incursa.Types", throwOnError: true);

        resolvedType.ShouldBe(typeof(Money));
    }

    [Fact]
    public void AssemblyQualifiedBigDecimalIdentityResolvesThroughRichPackageForwarder()
    {
        Type? resolvedType = Type.GetType("Incursa.BigDecimal, Incursa.Types", throwOnError: true);

        resolvedType.ShouldBe(typeof(BigDecimal));
    }

    [Fact]
    public void PrimitivesAssemblyHasNoExternalRuntimeReferences()
    {
        string[] runtimeReferences = typeof(FastId).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null &&
                           !name.StartsWith("System.", StringComparison.Ordinal) &&
                           !string.Equals(name, "System.Runtime", StringComparison.Ordinal) &&
                           !string.Equals(name, "netstandard", StringComparison.Ordinal))
            .Cast<string>()
            .ToArray();

        runtimeReferences.ShouldBeEmpty();
    }
}
