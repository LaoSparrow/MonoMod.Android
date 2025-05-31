extern alias New;
using New::MonoMod.RuntimeDetour;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace MonoMod.UnitTest.Github
{
    public class Issue220 : TestBase
    {
        public Issue220(ITestOutputHelper helper) : base(helper)
        {
        }

        private class DrawTarget
        {
            public bool IsDrawn { get; private set; }
            
            [MethodImpl(MethodImplOptions.NoInlining)]
            public void DrawMenu(GameTime gameTime)
            {
                IsDrawn = true;
            }
        }

        private class GameTime
        {
            // Minimal mock to match the scenario
        }

        [Fact]
        public void ConcurrentILHooksDoNotCauseRaceCondition()
        {
            for (var i = 0; i < 10; i++)
            {
                var target = new DrawTarget();
                
                // Simulate multiple hooks being added concurrently
                Parallel.For(0, 10, i =>
                {
                    using (var hook = new Hook(typeof(DrawTarget).GetMethod(nameof(DrawTarget.DrawMenu)),
                        (Action<Action<DrawTarget, GameTime>, DrawTarget, GameTime>)((orig, self, gameTime) =>
                        {
                            // Original method call
                            orig(self, gameTime);
                        })))
                    {
                        // Call the hooked method
                        target.DrawMenu(new GameTime());
                    }
                });

                // Verify that the method was drawn
                Assert.True(target.IsDrawn, "Method should have been drawn without raising an exception");
            }
        }
    }
}