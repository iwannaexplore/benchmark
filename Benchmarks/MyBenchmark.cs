using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public class MyBenchmark
{
    private List<int> array;

    [GlobalSetup]
    public void Setup()
    {
        array = new List<int>()
        {
            2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000,2000
        };
    }

    [Benchmark]
    public async Task<List<long>> FactorialForArrayInTaskAsyncWay()
    {
        var list = new List<Task<long>>();
        foreach (var item in array)
        {
            var task = Task.Run(() => Factorial(item));
            list.Add(task);
        }
        await Task.WhenAll(list);
        return list.Select(x => x.Result).ToList();
    }

    [Benchmark]
    public List<long> FactorialForArraySyncWay()
    {
        var list = new List<long>();
        foreach (var elem in array)
        {
            list.Add(Factorial(elem));
        }
        return list;
    }

    [Benchmark]
    public List<long> FactorialForArrayInParallelLINQWay()
    {
        var result = array.AsParallel().Select(Factorial);
        return result.ToList();
    }

    [Benchmark]
    public List<long> FactorialForArrayInParallelWay()
    {
        ConcurrentBag<long> results = new ConcurrentBag<long>();

        Parallel.ForEach(array, (number) => { results.Add(Factorial(number)); });
        return results.ToList();
    }
    [Benchmark]
    public async Task<List<long>> FactorialForArrayInDataflowWay()
    {
        var transformBlock = new TransformBlock<int, long>(Factorial);

        var results = new List<long>();

        var actionBlock = new ActionBlock<long>(result =>
        {
            results.Add(result);
        });

        transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (int number in array)
        {
            transformBlock.Post(number);
        }

        transformBlock.Complete();
        await actionBlock.Completion;

        return results;
    }

    [Benchmark]
    public async Task<List<Container>> FactorialForArrayToContainerInTaskAsyncWay()
    {
        var list = new List<Task<List<Container>>>();
        foreach (var item in array)
        {
            var task = Task.Run(() => FactorialContainer(item));
            list.Add(task);
        }
        await Task.WhenAll(list);
        return list.SelectMany(x => x.Result).ToList();
    }

    [Benchmark]
    public List<Container> FactorialForArrayToContainerSyncWay()
    {
        var list = new List<Container>();
        foreach (var elem in array)
        {
            list.AddRange(FactorialContainer(elem));
        }
        return list;
    }

    [Benchmark]
    public List<Container> FactorialForArrayToContainerInParallelLINQWay()
    {
        var result = array.AsParallel().SelectMany(FactorialContainer);
        return result.ToList();
    }

    [Benchmark]
    public List<Container> FactorialForArrayToContainerInParallelWay()
    {
        ConcurrentBag<Container> results = new ConcurrentBag<Container>();

        Parallel.ForEach(array, (number) =>
        {
            {
                var factorialContainer = FactorialContainer(number);
                foreach (var container in factorialContainer)
                {
                    results.Add(container);
                }
            }
        });
        return results.ToList();
    }
    [Benchmark]
    public async Task<List<Container>> FactorialForArrayToContainerInDataflowWay()
    {
        var transformBlock = new TransformBlock<int, List<Container>>(FactorialContainer);

        var results = new List<Container>();

        var actionBlock = new ActionBlock<List<Container>>(result =>
        {
            results.AddRange(result);
        });

        transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (int number in array)
        {
            transformBlock.Post(number);
        }

        transformBlock.Complete();
        await actionBlock.Completion;

        return results;
    }

    public long Factorial(int number)
    {

        if (number == 0)
        {
            return 1;
        }
        else
        {
            return number * Factorial(number - 1);
        }
    }

    public List<Container> FactorialContainer(int number)
    {
        var list = new List<Container>();
        list.Add(new Container(number, number * 2, number * 3));
        if (number == 0)
        {
            return list;
        }
        else
        {
            list.AddRange(FactorialContainer(number - 1));
            return list;
        }
    }
}

public class Container
{
    public Container(int number, int doubleNumber, int tripleNumber)
    {
        Number = number;
        DoubleNumber = doubleNumber;
        TripleNumber = tripleNumber;
    }

    public int Number { get; set; }
    public int DoubleNumber { get; set; }
    public int TripleNumber { get; set; }
}

