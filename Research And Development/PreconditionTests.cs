using HQ;
using HQ.Attributes;
using HQ.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RnD
{
    [TestClass]
    public class PreconditionTests
    {
        public class NumberContext : IContextObject
        {
            public NumberContext(CommandRegistry registry)
            {
                Registry = registry;
                Storage = new ConcurrentDictionary<object, object>();
            }

            public dynamic this[object key] { get => Retrieve(key); set => Store(key, value); }

            public CommandRegistry Registry { get; }
            public bool Finalized { get; set; }

            public ConcurrentDictionary<object, object> Storage { get; }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public dynamic Retrieve(object key)
            {
                return Storage[key];
            }

            public void Store(object key, object value)
            {
                Storage.AddOrUpdate(key, value, (k, v) => value);
            }

            public bool TryRetrieve(object key, out dynamic value)
            {
                return Storage.TryGetValue(key, out value);
            }
        }

        [CommandClass]
        public class TestCommand
        {
            [Precondition]
            public InputResult Precondition(NumberContext context)
            {
                return context["count"] > 0 ? InputResult.Success : InputResult.Failure;
            }

            /// <summary>
            /// A test executor for this command.
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            [CommandExecutor("A unit testing command",
                @"unit-test",
                RegexStringOptions.None,
                "unit-test")]
            public object TestExecutor(NumberContext context)
            {
                context["count"] += 1;
                return context["count"];
            }
        }

        private readonly object _key = new object();

        [TestMethod]
        public void TestPersistedContext()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand(typeof(TestCommand));
                NumberContext context = new NumberContext(registry);
                context["count"] = 0;

                registry.Contexts.Store(_key, context);

                //Count should be 0 - result should be a failure due to precondition
                registry.HandleInput("unit-test", _key, (result, output) =>
                {
                    Assert.AreEqual(InputResult.Failure, result);
                    mre.Set();
                });
                mre.WaitOne();
                mre.Reset();

                context["count"] = 1;

                //Count should be 2
                registry.HandleInput("unit-test", _key, (result, output) =>
                {
                    mre.Set();
                });
                mre.WaitOne();
                mre.Reset();

                //Count should be 3
                registry.HandleInput("unit-test", _key, (result, output) =>
                {
                    mre.Set();
                });
                mre.WaitOne();
                mre.Reset();

                int num = context["count"];
                context["count"] = 0;
                //Count is 0, result should be failure
                registry.HandleInput("unit-test", _key, (result, output) =>
                {
                    Assert.AreEqual(InputResult.Failure, result);
                    mre.Set();
                });
                mre.WaitOne();
                mre.Reset();

                context["count"] = num;

                //Count should be 4
                registry.HandleInput("unit-test", _key, (result, output) =>
                {
                    mre.Set();
                });
                mre.WaitOne();
                mre.Reset();

                NumberContext retrieved = registry.Contexts.Retrieve(_key) as NumberContext;

                //Ensure that context and retrieved actually map to the same objects with the same values
                Assert.AreEqual(context["count"], retrieved["count"]);
                //And that the count is actually 3
                Assert.AreEqual(4, retrieved["count"]);
            }
        }
    }
}
