``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19041.1415 (2004/May2020Update/20H1)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.100-preview.1.22110.4
  [Host]   : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  .NET 6.0 : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
| Method |            File |          Mean |        Error |     StdDev | Mean Throughput | Median Throughput |        Median |     Gen 0 |     Gen 1 |     Gen 2 |  Allocated |
|------- |---------------- |--------------:|-------------:|-----------:|----------------:|------------------:|--------------:|----------:|----------:|----------:|-----------:|
|    **Lex** |        **anim.lua** |      **29.02 μs** |     **0.370 μs** |   **0.328 μs** |      **95.47MiB/s** |        **95.81MiB/s** |      **28.91 μs** |    **2.6245** |    **0.0305** |         **-** |      **43 KB** |
|    **Lex** | **rustic-24mb.lua** | **221,573.45 μs** | **1,162.542 μs** | **907.636 μs** |     **108.37MiB/s** |       **108.51MiB/s** | **221,290.05 μs** | **1000.0000** | **1000.0000** | **1000.0000** | **191,257 KB** |
|    **Lex** |      **rustic.lua** |  **56,290.59 μs** |   **878.310 μs** | **778.599 μs** |     **106.67MiB/s** |       **107.33MiB/s** |  **55,944.28 μs** |  **555.5556** |  **555.5556** |  **555.5556** |  **47,822 KB** |
