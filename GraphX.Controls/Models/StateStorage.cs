using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Common.Models;
using QuickGraph;
#if WPF
using System.Windows;
#elif METRO
using Windows.UI.Xaml;
#endif

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
        public void SaveState(string id, string description = "")
        {
            _states.Add(id, GenerateGraphState(id, description));
        }

        /// <summary>
        /// Save current graph state into memory (including visual and data controls) or update existing state
        /// </summary>
        /// <param name="id">State id</param>
        /// <param name="description">Optional state description</param>
        public void SaveOrUpdateState(string id, string description = "")
        {
            if (ContainsState(id))
                _states[id] = GenerateGraphState(id, description);
            else SaveState(id, description);
        }

        private GraphState<TVertex, TEdge, TGraph> GenerateGraphState(string id, string description = "")
        {
            if (_area.LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            var vposlist = _area.VertexList.ToDictionary(item => item.Key, item => item.Value.GetPositionGraphX());
            var vedgelist = (from item in _area.EdgesList where item.Value.Visibility == Visibility.Visible select item.Key).ToList();            

            return new GraphState<TVertex, TEdge, TGraph>(id, _area.LogicCore.Graph, _area.LogicCore.AlgorithmStorage, vposlist, vedgelist, description);
        }

        /// <summary>
        /// Load previously saved state into layout
        /// </summary>
        /// <param name="id">Unique state id</param>
        public void LoadState(string id)
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
                _area.VertexList[item.Key].Visibility = Visibility.Visible;
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
        public void RemoveState(string id)
        {
            if (_states.ContainsKey(id))
                _states.Remove(id);
        }

        /// <summary>
        /// Get all states from the storage
        /// </summary>
        public Dictionary<string, GraphState<TVertex, TEdge, TGraph>> GetStates()
        {
            return _states;
        }

        /// <summary>
        /// Get all states from the storage
        /// </summary>
        /// <param name="id">Unique state id</param>
        public GraphState<TVertex, TEdge, TGraph> GetState(string id)
        {
            return ContainsState(id) ? _states[id] : null;
        }
    
        public void Dispose()
        {
            _area = null;
        }
    }
}