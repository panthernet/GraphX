﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphX.Common;
using GraphX.Common.Exceptions;
using GraphX.Common.Interfaces;
using GraphX.Common.Models;
using QuikGraph;
using System.Windows;

namespace GraphX.Controls.Models
{
    public class StateStorage<TVertex, TEdge, TGraph>: IDisposable
        where TEdge : class, IGraphXEdge<TVertex>
        where TVertex: class, IGraphXVertex
        where TGraph: class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        private readonly Dictionary<string, GraphState<TVertex, TEdge, TGraph>> _states;
        private GraphArea<TVertex, TEdge, TGraph> _area;

        public StateStorage(GraphArea<TVertex, TEdge, TGraph> area)
        {
            _area = area;
            _states = new Dictionary<string, GraphState<TVertex, TEdge, TGraph>>();
        }

        /// <summary>
        /// Returns true if state with supplied ID exists in the current states collection
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsState(string id)
        {
            return _states.ContainsKey(id);
        }

        /// <summary>
        /// Save current graph state into memory (including visual and data controls)
        /// </summary>
        /// <param name="id">New unique state id</param>
        /// <param name="description">Optional state description</param>
        public virtual void SaveState(string id, string description = "")
        {
            _states.Add(id, GenerateGraphState(id, description));
        }

        /// <summary>
        /// Save current graph state into memory (including visual and data controls) or update existing state
        /// </summary>
        /// <param name="id">State id</param>
        /// <param name="description">Optional state description</param>
        public virtual void SaveOrUpdateState(string id, string description = "")
        {
            if (ContainsState(id))
                _states[id] = GenerateGraphState(id, description);
            else SaveState(id, description);
        }

        protected virtual GraphState<TVertex, TEdge, TGraph> GenerateGraphState(string id, string description = "")
        {
            if (_area.LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            var vposlist = _area.VertexList.ToDictionary(item => item.Key, item => item.Value.GetPositionGraphX());
            var vedgelist = (from item in _area.EdgesList where item.Value.Visibility == Visibility.Visible select item.Key).ToList();            

            return new GraphState<TVertex, TEdge, TGraph>(id, _area.LogicCore.Graph, _area.LogicCore.AlgorithmStorage, vposlist, vedgelist, description);
        }

        /// <summary>
        /// Import specified state to the StateStorage
        /// </summary>
        /// <param name="key">State key</param>
        /// <param name="state">State object</param>
        public virtual void ImportState(string key, GraphState<TVertex, TEdge, TGraph> state)
        {
            if (ContainsState(key))
                throw new GX_ConsistencyException(string.Format("Graph state {0} already exist in state storage", key));

            //if(!unsafeImport && (_area.LogicCore == null || _area.LogicCore.Graph == null || _area.LogicCore.Graph != state.Graph))
           //     throw new GX_ConsistencyException("Can't validate that imported graph state belong to the target area Graph! You can try to import the state with unsafeImport parameter set to True.");
            _states.Add(key, state);
        }

        /// <summary>
        /// Load previously saved state into layout
        /// </summary>
        /// <param name="id">Unique state id</param>
        public virtual void LoadState(string id)
        {
            if (_area.LogicCore == null)
                throw new GX_InvalidDataException("GraphArea.LogicCore -> Not initialized!");

            if (!_states.ContainsKey(id))
            {
                Debug.WriteLine(string.Format("LoadState() -> State id {0} not found! Skipping...", id));
                return;
            }

           // _area.RemoveAllVertices();
           // _area.RemoveAllEdges();
            //One action: clear all, preload vertices, assign Graph property
            _area.PreloadVertexes(_states[id].Graph, true, true);
            _area.LogicCore.Graph = _states[id].Graph;
            _area.LogicCore.AlgorithmStorage = _states[id].AlgorithmStorage;

            //setup vertex positions
            foreach (var item in _states[id].VertexPositions)
            {
                _area.VertexList[item.Key].SetPosition(item.Value.X, item.Value.Y);
                _area.VertexList[item.Key].SetCurrentValue(GraphAreaBase.PositioningCompleteProperty, true);
            }
            //setup visible edges
            foreach (var item in _states[id].VisibleEdges)
            {
               var edgectrl =  _area.ControlFactory.CreateEdgeControl(_area.VertexList[item.Source], _area.VertexList[item.Target],
                                                       item);
                _area.InsertEdge(item, edgectrl);
                //edgectrl.UpdateEdge();
            }
            _area.UpdateLayout();
            foreach (var item in _area.EdgesList.Values)
            {
                item.UpdateEdge();
            }
        }

        /// <summary>
        /// Remove state by id
        /// </summary>
        /// <param name="id">Unique state id</param>
        public virtual void RemoveState(string id)
        {
            if (_states.ContainsKey(id))
                _states.Remove(id);
        }

        /// <summary>
        /// Get all states from the storage
        /// </summary>
        public virtual Dictionary<string, GraphState<TVertex, TEdge, TGraph>> GetStates()
        {
            return _states;
        }

        /// <summary>
        /// Get all states from the storage
        /// </summary>
        /// <param name="id">Unique state id</param>
        public virtual GraphState<TVertex, TEdge, TGraph>? GetState(string id)
        {
            return ContainsState(id) ? _states[id] : null;
        }
    
        public virtual void Dispose()
        {
            _states.ForEach(a=> a.Value.Dispose());
            _states.Clear();
            _area = null!;
        }
    }
}