#!/bin/bash
# Performance Test Runner Script
# This script runs the NetCorePal performance tests with the available .NET 8.0 SDK

cd "$(dirname "$0")"

echo "NetCorePal Framework Performance Tests"
echo "====================================="
echo ""

# Set the SDK version temporarily for this test
export DOTNET_ROLL_FORWARD=Major

echo "Building performance tests..."
dotnet build NetCorePal.Extensions.Performance.Tests.csproj -c Release -f net8.0

if [ $? -eq 0 ]; then
    echo ""
    echo "Running performance tests (dry run)..."
    echo ""
    
    # Run different benchmark categories
    echo "1. Dependency Injection Performance:"
    dotnet run -c Release -f net8.0 -- --filter "*DependencyInjectionBenchmark*" -j Dry
    
    echo ""
    echo "2. String Operations Performance:"
    dotnet run -c Release -f net8.0 -- --filter "*StringOperationsBenchmark*" -j Dry
    
    echo ""
    echo "3. Collection Operations Performance:"
    dotnet run -c Release -f net8.0 -- --filter "*CollectionOperationsBenchmark*" -j Dry
    
    echo ""
    echo "Performance test demonstration completed!"
    echo "For full benchmarks, run: dotnet run -c Release -f net8.0"
else
    echo "Build failed. Please check the project configuration."
fi