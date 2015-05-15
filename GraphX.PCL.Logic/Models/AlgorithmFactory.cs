using System.Collections.Generic;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Common.Models;
using GraphX.PCL.Logic.Algorithms;
using GraphX.PCL.Logic.Algorithms.EdgeRouting;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using QuickGraph;

namespace GraphX.PCL.Logic.Models
{
    public sealed class AlgorithmFactory<TVertex, TEdge, TGraph> : IAlgorithmFactory<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        #region Layout factory
        public ILayoutAlgorithm<TVertex, TEdge, TGraph> CreateLayoutAlgorithm(LayoutAlgorithmTypeEnum newAlgorithmType, TGraph iGraph, IDictionary<TVertex, Point> positions = null, IDictionary<TVertex, Size> sizes = null, ILayoutParameters parameters = null)
        {
            if (iGraph == null) return null;
            if (parameters == null) parameters = CreateLayoutParameters(newAlgorithmType);
            IMutableBidirectionalGraph<TVertex, TEdge> graph = iGraph.CopyToBidirectionalGraph();
            /*var dic = new Dictionary<TVertex, Point>();
            if (Positions != null)
            {
                dic = Positions.Where(a => a.Key.SkipProcessing == ProcessingOptionEnum.Freeze).ToDictionary(a=> a.Key, a=> a.Value);
            }*/
            graph.RemoveEdgeIf(a => a.SkipProcessing == ProcessingOptionEnum.Exclude);
            graph.RemoveVertexIf(a => a.SkipProcessing == ProcessingOptionEnum.Exclude);

            switch (newAlgorithmType)
            {
                case LayoutAlgorithmTypeEnum.Tree:
                    return new SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions, sizes, parameters as SimpleTreeLayoutParameters);
                case LayoutAlgorithmTypeEnum.SimpleRandom:
                    return new RandomLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions);
                case LayoutAlgorithmTypeEnum.Circular:
                    return new CircularLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions, sizes, parameters as CircularLayoutParameters);
                case LayoutAlgorithmTypeEnum.FR:
                    return new FRLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions, parameters as FRLayoutParametersBase);
                case LayoutAlgorithmTypeEnum.BoundedFR:
                    return new FRLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions, parameters as BoundedFRLayoutParameters);
                case LayoutAlgorithmTypeEnum.KK:
                    return new KKLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions, parameters as KKLayoutParameters);
                case LayoutAlgorithmTypeEnum.ISOM:
                    return new ISOMLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions, parameters as ISOMLayoutParameters);
                case LayoutAlgorithmTypeEnum.LinLog:
                    return new LinLogLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, positions, parameters as LinLogLayoutParameters);
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                    return new EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, parameters as EfficientSugiyamaLayoutParameters, positions, sizes);
                case LayoutAlgorithmTypeEnum.Sugiyama:
                    return new SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, sizes, positions, parameters as SugiyamaLayoutParameters,
                                                                                   e => (e is TypedEdge<TVertex>
                                                                                        ? (e as TypedEdge<TVertex>).Type
                                                                                        : EdgeTypes.Hierarchical));
                case LayoutAlgorithmTypeEnum.CompoundFDP:
                    return new CompoundFDPLayoutAlgorithm<TVertex, TEdge, TGraph>((TGraph)graph, sizes, new Dictionary<TVertex, Thickness>(), new Dictionary<TVertex, CompoundVertexInnerLayoutType>(),
                        positions, parameters as CompoundFDPLayoutParameters);

                /*case LayoutAlgorithmTypeEnum.BalloonTree:
                    return new BalloonTreeLayoutAlgorithm<TVertex, TEdge, TGraph>(Graph, Positions, Sizes, parameters as BalloonTreeLayoutParameters, Graph.Vertices.FirstOrDefault());*/
                default:
                    return null;
            }
        }

        public ILayoutParameters CreateLayoutParameters(LayoutAlgorithmTypeEnum algorithmType)
        {
            switch (algorithmType)
            {
                case LayoutAlgorithmTypeEnum.Tree:
                    return new SimpleTreeLayoutParameters();
                case LayoutAlgorithmTypeEnum.Circular:
                    return new CircularLayoutParameters();
                case LayoutAlgorithmTypeEnum.FR:
                    return new FreeFRLayoutParameters();
                case LayoutAlgorithmTypeEnum.BoundedFR:
                    return new BoundedFRLayoutParameters();
                case LayoutAlgorithmTypeEnum.KK:
                    return new KKLayoutParameters();
                case LayoutAlgorithmTypeEnum.ISOM:
                    return new ISOMLayoutParameters();
                case LayoutAlgorithmTypeEnum.LinLog:
                    return new LinLogLayoutParameters();
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                    return new EfficientSugiyamaLayoutParameters();
                case LayoutAlgorithmTypeEnum.Sugiyama:
                    return new SugiyamaLayoutParameters();
                case LayoutAlgorithmTypeEnum.CompoundFDP:
                    return new CompoundFDPLayoutParameters();
               // case LayoutAlgorithmTypeEnum.BalloonTree:
               //     return new BalloonTreeLayoutParameters();
                default:
                    return null;
            }
        }

        public bool NeedSizes(LayoutAlgorithmTypeEnum algorithmType)
        {
            return (algorithmType == LayoutAlgorithmTypeEnum.Tree) /*|| (algorithmType == LayoutAlgorithmTypeEnum.BalloonTree)*/ || (algorithmType == LayoutAlgorithmTypeEnum.Circular) ||
                (algorithmType == LayoutAlgorithmTypeEnum.EfficientSugiyama) || (algorithmType == LayoutAlgorithmTypeEnum.Sugiyama)
                 || (algorithmType == LayoutAlgorithmTypeEnum.CompoundFDP);
        }

        public bool NeedEdgeRouting(LayoutAlgorithmTypeEnum algorithmType)
        {
            return (algorithmType != LayoutAlgorithmTypeEnum.Sugiyama) && (algorithmType != LayoutAlgorithmTypeEnum.EfficientSugiyama);
        }

        public bool NeedOverlapRemoval(LayoutAlgorithmTypeEnum algorithmType)
        {
            return (algorithmType != LayoutAlgorithmTypeEnum.Sugiyama
                && algorithmType != LayoutAlgorithmTypeEnum.EfficientSugiyama
                && algorithmType != LayoutAlgorithmTypeEnum.Circular
                && algorithmType != LayoutAlgorithmTypeEnum.Tree
                /*&& algorithmType != LayoutAlgorithmTypeEnum.BalloonTree*/);
        }
        #endregion

        #region OverlapRemoval factory

        public IOverlapRemovalAlgorithm<TVertex> CreateOverlapRemovalAlgorithm(OverlapRemovalAlgorithmTypeEnum newAlgorithmType, IDictionary<TVertex, Rect> rectangles, IOverlapRemovalParameters parameters = null)
        {
            //if (Rectangles == null) return null;
            if (parameters == null) parameters = CreateOverlapRemovalParameters(newAlgorithmType);

            switch (newAlgorithmType)
            {
                case OverlapRemovalAlgorithmTypeEnum.FSA:
                    return new FSAAlgorithm<TVertex>(rectangles, parameters is OverlapRemovalParameters ? parameters : new OverlapRemovalParameters());
                case OverlapRemovalAlgorithmTypeEnum.OneWayFSA:
                    
                    return new OneWayFSAAlgorithm<TVertex>(rectangles, parameters is OneWayFSAParameters ? parameters as OneWayFSAParameters : new OneWayFSAParameters());
                default:
                    return null;
            }
        }

        public IOverlapRemovalAlgorithm<T> CreateFSAA<T>(IDictionary<T, Rect> rectangles, float horGap, float vertGap) where T : class
        {
            return  new FSAAlgorithm<T>(rectangles, new OverlapRemovalParameters { HorizontalGap = horGap, VerticalGap = vertGap});
        }

        public IOverlapRemovalParameters CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum algorithmType)
        {
            switch (algorithmType)
            {
                case OverlapRemovalAlgorithmTypeEnum.FSA:
                    return new OverlapRemovalParameters();
                case OverlapRemovalAlgorithmTypeEnum.OneWayFSA:
                    return new OneWayFSAParameters();
                default:
                    return null;
            }
        }


        #endregion

        #region Edge Routing factory
        public IExternalEdgeRouting<TVertex, TEdge> CreateEdgeRoutingAlgorithm(EdgeRoutingAlgorithmTypeEnum newAlgorithmType, Rect graphArea, TGraph iGraph, IDictionary<TVertex, Point> positions, IDictionary<TVertex, Rect> rectangles, IEdgeRoutingParameters parameters = null)
        {
            //if (Rectangles == null) return null;
            if (parameters == null) parameters = CreateEdgeRoutingParameters(newAlgorithmType);
            IMutableBidirectionalGraph<TVertex, TEdge> graph = iGraph.CopyToBidirectionalGraph();
            graph.RemoveEdgeIf(a => a.SkipProcessing == ProcessingOptionEnum.Exclude);
            graph.RemoveVertexIf(a => a.SkipProcessing == ProcessingOptionEnum.Exclude);

            switch (newAlgorithmType)
            {
                case EdgeRoutingAlgorithmTypeEnum.SimpleER:
                    return new SimpleEdgeRouting<TVertex, TEdge,  TGraph>((TGraph)graph, positions, rectangles, parameters);
                case EdgeRoutingAlgorithmTypeEnum.Bundling:
                    return new BundleEdgeRouting<TVertex, TEdge, TGraph>(graphArea, (TGraph)graph, positions, rectangles, parameters);
                case EdgeRoutingAlgorithmTypeEnum.PathFinder:
                    return new PathFinderEdgeRouting<TVertex, TEdge, TGraph>((TGraph)graph, positions, rectangles, parameters);
                default:
                    return null;
            }
        }

        public IEdgeRoutingParameters CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum algorithmType)
        {
            switch (algorithmType)
            {
                case EdgeRoutingAlgorithmTypeEnum.SimpleER:
                    return new SimpleERParameters();
                case EdgeRoutingAlgorithmTypeEnum.Bundling:
                    return new BundleEdgeRoutingParameters();
                case EdgeRoutingAlgorithmTypeEnum.PathFinder:
                    return new PathFinderEdgeRoutingParameters();
                default:
                    return null;
            }
        }
        #endregion
    }
}
