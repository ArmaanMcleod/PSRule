// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline;
using Xunit;

namespace PSRule
{
    public sealed class TargetNameBindingTests
    {
        internal class TestModel1
        {
            public string NotName { get; set; }
        }

        internal class TestModel2
        {
            public string NotName { get; set; }
        }

        internal class TestModel3 : ITargetInfo
        {
            public string Name { get; set; }

            string ITargetInfo.TargetName => "TestModel3";

            string ITargetInfo.TargetType => "TestModel3";

            TargetSourceInfo ITargetInfo.Source => null;
        }

        [Fact]
        public void UnboundObjectTargetName()
        {
            var testObject1 = new TestModel1 { NotName = "TestObject1" };
            var testObject2 = new TestModel2 { NotName = "TestObject1" };
            var pso1 = PSObject.AsPSObject(testObject1);
            var pso2 = PSObject.AsPSObject(testObject2);

            PipelineContext.CurrentThread = PipelineContext.New(GetOption(), null, null, null, null, null, null, null);
            var actual1 = PipelineHookActions.BindTargetName(null, false, false, pso1, out _);
            var actual2 = PipelineHookActions.BindTargetName(null, false, false, pso2, out _);

            Assert.Equal(expected: "f209c623345144be61087d91f30c17b01c6e86d2", actual: actual1);
            Assert.Equal(expected: "f209c623345144be61087d91f30c17b01c6e86d2", actual: actual2);
        }

        [Fact]
        public void PreferTargetInfo()
        {
            var testObject1 = new TestModel3 { Name = "OtherName" };
            var pso1 = PSObject.AsPSObject(testObject1);

            PipelineContext.CurrentThread = PipelineContext.New(GetOption(), null, null, null, null, null, null, null);

            var actual = PipelineHookActions.BindTargetName(new string[] { "Name" }, false, false, pso1, out string path);
            Assert.Equal("OtherName", actual);
            Assert.Equal("Name", path);

            actual = PipelineHookActions.BindTargetName(new string[] { "NotName" }, false, false, pso1, out path);
            Assert.Equal("TestModel3", actual);
            Assert.Null(path);

            actual = PipelineHookActions.BindTargetName(new string[] { "Name" }, false, true, pso1, out path);
            Assert.Equal("TestModel3", actual);
            Assert.Null(path);
        }

        private static PSRuleOption GetOption()
        {
            return new PSRuleOption();
        }
    }
}
