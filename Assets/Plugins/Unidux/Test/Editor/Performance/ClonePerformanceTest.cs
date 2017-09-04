﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace Unidux.Test
{
    public class ClonePerformanceTest
    {
        [Test]
        public void CloneTest()
        {
            var state = SampleState.Create(1000);
            var watch = new Stopwatch();

            {
                watch.Reset();
                watch.Start();
                var clone2 = state.CustomClone();
                watch.Stop();
                UnityEngine.Debug.Log("Clone2: " + watch.Elapsed.Milliseconds + "[ms]");
            }
            {
                watch.Reset();
                watch.Start();
                var clone1 = (SampleState)state.Clone();
                watch.Stop();
                UnityEngine.Debug.Log("Clone1: " + watch.Elapsed.Milliseconds + "[ms]");
            }

            Assert.IsTrue(true);
        }

        [Serializable]
        class SampleState : StateBase
        {
            public List<SampleEntity> List = new List<SampleEntity>();

            public static SampleState Create(int loop)
            {
                var state = new SampleState();

                for (int i = 0; i < loop; i++)
                {
                    state.List.Add(SampleEntity.Create(loop));
                }

                return state;
            }

//            public override TValue Clone<TValue>()
//            {
//                object clonee = CustomClone();
//                return (TValue)clonee;
//            }

            public SampleState CustomClone()
            {
                var state = new SampleState();

                foreach (var entity in state.List)
                {
                    state.List.Add(entity.CustomClone());
                }

                return state;
            }
        }

        [Serializable]
        class SampleEntity : StateElement
        {
            public int IntValue = 0;
            public long LongValue = 0;
            public float FloatValue = 0f;
            public double DoubleValue = 0;
            public string StringValue = "";
            public List<string> StringListValue = new List<string>();
            public Dictionary<int, string> DictionaryStringValue = new Dictionary<int, string>();

            public static SampleEntity Create(int loop)
            {
                var entity = new SampleEntity();
                entity.IntValue = 1;
                entity.LongValue = 1;
                entity.FloatValue = 1;
                entity.DoubleValue = 1;
                entity.StringValue = "abcdefghijklmnopqrstu";

                for (var i = 0; i < loop; i++)
                {
                    entity.StringListValue.Add("abc");
                }

                for (var i = 0; i < loop; i++)
                {
                    entity.DictionaryStringValue[i] = "abc";
                }

                return entity;
            }

            public SampleEntity CustomClone()
            {
                var entity = new SampleEntity();
                entity.IntValue = this.IntValue;
                entity.LongValue = this.LongValue;
                entity.FloatValue = this.FloatValue;
                entity.DoubleValue = this.DoubleValue;
                entity.StringValue = this.StringValue;

                foreach (var value in this.StringListValue)
                {
                    entity.StringListValue.Add(value);
                }

                foreach (var entry in this.DictionaryStringValue)
                {
                    entity.DictionaryStringValue[entry.Key] = entry.Value;
                }

                return entity;
            }
        }
    }
}