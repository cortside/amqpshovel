using System.Collections.Generic;
using Moq;

namespace AmqpTools.Test {
    public class UnitTestFixture {
        private readonly List<Mock> mocks;

        public UnitTestFixture() {
            mocks = new List<Mock>();
        }

        public Mock<T> Mock<T>() where T : class {
            var mock = new Mock<T>();
            mocks.Add(mock);
            return mock;
        }

        public void TearDown() {
            mocks.ForEach(m => m.VerifyAll());
        }
    }
}
