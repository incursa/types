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

using System.Diagnostics;
using Incursa;

const int Iterations = 200_000;
BigDecimal left = BigDecimal.Parse("12345678901234567890.125");
BigDecimal right = BigDecimal.Parse("2500.25");
BigDecimal terminatingDivisor = BigDecimal.Parse("125");
object? sink = null;

Measure("parse", () => sink = BigDecimal.Parse("12345678901234567890.125"));
Measure("format", () => sink = left.ToString());
Measure("compare", () => sink = left.CompareTo(right));
Measure("hash", () => sink = left.GetHashCode());
Measure("add", () => sink = left + right);
Measure("multiply", () => sink = left * right);
Measure("divide-exact", () => sink = left / terminatingDivisor);
Measure(
    "divide-rounded",
    () => sink = left.DivideRounded(right, 6, BigDecimalRoundingMode.ToNearestTiesToEven));
Measure("quantize", () => sink = left.Quantize(2, BigDecimalRoundingMode.ToNearestTiesToEven));
Measure("property-access", () => sink = (left.Coefficient, left.Exponent));
GC.KeepAlive(sink);

void Measure(string name, Action operation)
{
    for (int i = 0; i < 10_000; i++)
    {
        operation();
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    var stopwatch = Stopwatch.StartNew();
    for (int i = 0; i < Iterations; i++)
    {
        operation();
    }

    stopwatch.Stop();
    long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
    double nanosecondsPerOperation = stopwatch.Elapsed.TotalNanoseconds / Iterations;
    double bytesPerOperation = (double)allocated / Iterations;
    Console.WriteLine($"{name,-16} {nanosecondsPerOperation,10:F1} ns/op {bytesPerOperation,10:F1} B/op");
}
