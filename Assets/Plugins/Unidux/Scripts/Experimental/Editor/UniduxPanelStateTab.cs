﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unidux.Example.Counter;
using Unidux.Util;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Unidux.Experimental.Editor
{
    public partial class UniduxPanelStateTab
    {
        public delegate TResult Func6<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3,
            T4 arg4,
            T5 arg5);

        private Vector2 _scrollPosition = Vector2.zero;
        private Dictionary<string, bool> _foldingMap = new Dictionary<string, bool>();

        private Dictionary<string, int> _pageMap = new Dictionary<string, int>();

        private object _newState = null;
        private ISubject<object> _stateSubject;
        private const int PerPage = 100;

        public void Render(IStoreAccessor _store)
        {
            if (_store == null)
            {
                EditorGUILayout.HelpBox("Please Set IStoreAccessor", MessageType.Warning);
                return;
            }

            // scrollview of state
            {
                this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition, GUI.skin.box);
                var state = _store.StoreObject.ObjectState;
                var names = new List<string>();
                var type = state.GetType();

                if (!state.Equals(this._newState))
                {
                    this._newState = CloneUtil.MemoryClone(state);
                }

                var dirty = this.RenderObject(names, state.GetType().Name, this._newState, type, _ => { });
                EditorGUILayout.EndScrollView();

                // XXX: it might be slow and should be updated less frequency.
                if (dirty)
                {
                    _store.StoreObject.ObjectState = this._newState;
                    this._newState = null;
                }
            }
        }

        Func6<List<string>, string, object, Type, Action<object>, bool> SelectObjectRenderer(Type type, object element)
        {
            // struct
            if (type.IsValueType)
            {
                return this.SelectValueRender(type, element);
            }
            else
            {
                return this.SelectClassRender(type, element);
            }
        }

        bool RenderObject(
            List<string> rootNames,
            string name,
            object element,
            Type type,
            Action<object> setter
        )
        {
            return this.SelectObjectRenderer(type, element)(rootNames, name, element, type, setter);
        }

        int RenderPagerContent(
            string foldingKey,
            System.Collections.ICollection collection,
            Action<object, int> renderValueWithIndex
        )
        {
            var listSize = collection.Count;
            var page = this._pageMap.GetOrDefault(foldingKey, 0);
            var lastPage = (listSize - 1) / PerPage;
            page = this.RenderPagerHeader(page, lastPage);
            this._pageMap[foldingKey] = page;

            var startIndex = page * PerPage;
            var endIndex = Math.Min((page + 1) * PerPage, listSize);

            var i = 0;
            foreach (var element in collection)
            {
                if (i >= startIndex && i < endIndex)
                {
                    renderValueWithIndex(element, i);
                }

                i++;
            }

            return page;
        }

        int RenderPagerHeader(
            int page,
            int lastPage
        )
        {
            bool hasPrev = page > 0;
            bool hasNext = lastPage > page;

            EditorGUILayout.BeginHorizontal();

            {
                EditorGUI.BeginDisabledGroup(!hasPrev);
                if (GUILayout.Button("<<") && hasPrev)
                {
                    page -= 10;
                }
                if (GUILayout.Button("<") && hasPrev)
                {
                    page--;
                }
                EditorGUI.EndDisabledGroup();
            }

            page = EditorGUILayout.IntField("page", page);
            EditorGUILayout.LabelField("/" + lastPage);

            {
                EditorGUI.BeginDisabledGroup(!hasNext);
                if (GUILayout.Button(">") && hasNext)
                {
                    page++;
                }
                if (GUILayout.Button(">>") && hasNext)
                {
                    page += 10;
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();

            page = page < 0 ? 0 : page;
            page = page > lastPage ? lastPage : page;

            return page;
        }

        string GetFoldingName(ICollection<string> collection, string name)
        {
            return name;
        }

        string GetFoldingKey(IEnumerable<string> rootNames)
        {
            return string.Join(".", rootNames.ToArray());
        }
    }
}