﻿using System.Threading.Tasks;

using Unity.Interception.PolicyInjection.MatchingRules;

namespace vm.Aspects.Policies.Tests
{
    public abstract class BaseTestCalls
    {
        public const string Trace = nameof(Trace);
        public const string Track = nameof(Track);
    }

    [Tag(Track)]
    public class TrackTestCalls : BaseTestCalls, ITestCalls
    {
        public void Test1()
        {
        }

        public int Test2()
        {
            return 12;
        }

        public int Test3(string i)
        {
            return int.Parse(i);
        }

        public Task AsyncTest1()
        {
            return Task.Delay(50);
        }

        public async Task<int> AsyncTest2()
        {
            await Task.Delay(50);
            return 22;
        }

        public async Task<int> AsyncTest3(string i)
        {
            await Task.Delay(50);
            return int.Parse("23");
        }
    }

    [Tag(Trace)]
    public class TraceTestCalls : BaseTestCalls, ITestCalls
    {
        public void Test1()
        {
        }

        public int Test2()
        {
            return 12;
        }

        public int Test3(string i)
        {
            return int.Parse(i);
        }

        public Task AsyncTest1()
        {
            return Task.Delay(50);
        }

        public async Task<int> AsyncTest2()
        {
            await Task.Delay(50);
            return 22;
        }

        public async Task<int> AsyncTest3(string i)
        {
            await Task.Delay(50);
            return int.Parse("23");
        }
    }
}
